using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using Pancake.Tag;
using Pancake.ExLibEditor;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Pancake.TagEditor
{
    public abstract class TagPropertyDrawerBase : PropertyDrawer
    {
        [NonSerialized]
        // Used for a visual highlight to help editor navigation of assets
        public static int sentFromInstanceId = 0;
    }

    //[CanEditMultipleObjects]
    public abstract class TagPropertyDrawerBase<TGroup, T> : TagPropertyDrawerBase where TGroup : TagGroupBase where T : ScriptableTag
    {
        protected static bool initialized = false;
        protected static TagDropDownMenuWindow menuPopup;
        protected static List<(TagGroupBase group, T asset)> foundAssets = new List<(TagGroupBase, T)>();
        protected static List<(string label, int index, bool editable)> menuEntries = new List<(string, int, bool editable)>();

        // This is quite unfortunate.
        // It seems creating an asset can cause the underlying serializedProperty to update and consequently cause the property drawer to be rebuilt.
        // Therefore something like a local bool will be lost during an operation such as add or duplicate.
        // Consequently we need to store and lookup whether to edit and what the edit string is through a combination of the serializedObject and property path.
        // 'serializedProperty' and 'this' cannot be used as keys as they will be unique upon rebuild. Thus: 
        // key = serializedProperty.serializedObject, serializedProperty.propertyPath
        internal static Dictionary<(SerializedObject, string), bool> propertyDrawerShouldEdit = new Dictionary<(SerializedObject, string), bool>();
        internal static Dictionary<(SerializedObject, string), bool> propertyDrawerIsEditing = new Dictionary<(SerializedObject, string), bool>();
        internal static Dictionary<(SerializedObject, string), string> propertyDrawerEditString = new Dictionary<(SerializedObject, string), string>();

        internal static void SetShouldEdit(SerializedProperty serializedProperty, bool value)
        {
            if (serializedProperty != null) SetShouldEdit(serializedProperty.serializedObject, serializedProperty.propertyPath, value);
        }

        internal static void SetShouldEdit(SerializedObject serializedObject, string propPath, bool value)
        {
            if (serializedObject != null) propertyDrawerShouldEdit[(serializedObject, propPath)] = value;
        }

        internal static bool GetShouldEdit(SerializedProperty serializedProperty)
        {
            return serializedProperty.serializedObject == null ? default : GetShouldEdit(serializedProperty.serializedObject, serializedProperty.propertyPath);
        }

        internal static bool GetShouldEdit(SerializedObject serializedObject, string propPath)
        {
            return propertyDrawerShouldEdit.TryGetValue((serializedObject, propPath), out var result) ? result : default;
        }

        internal static void SetIsEdit(SerializedProperty serializedProperty, bool value)
        {
            if (serializedProperty != null) SetIsEdit(serializedProperty.serializedObject, serializedProperty.propertyPath, value);
        }

        internal static void SetIsEdit(SerializedObject serializedObject, string propPath, bool value)
        {
            if (serializedObject != null) propertyDrawerIsEditing[(serializedObject, propPath)] = value;
        }

        internal static bool GetIsEdit(SerializedProperty serializedProperty)
        {
            return serializedProperty.serializedObject == null ? default : GetIsEdit(serializedProperty.serializedObject, serializedProperty.propertyPath);
        }

        internal static bool GetIsEdit(SerializedObject serializedObject, string propPath)
        {
            return propertyDrawerIsEditing.TryGetValue((serializedObject, propPath), out var result) ? result : default;
        }

        internal static void SetEditString(SerializedProperty serializedProperty, string value)
        {
            if (serializedProperty != null) SetEditString(serializedProperty.serializedObject, serializedProperty.propertyPath, value);
        }

        internal static void SetEditString(SerializedObject serializedObject, string propPath, string value)
        {
            if (serializedObject != null) propertyDrawerEditString[(serializedObject, propPath)] = value;
        }

        internal static string GetEditString(SerializedProperty serializedProperty)
        {
            return serializedProperty.serializedObject == null ? default : GetEditString(serializedProperty.serializedObject, serializedProperty.propertyPath);
        }

        internal static string GetEditString(SerializedObject serializedObject, string propPath)
        {
            return propertyDrawerEditString.TryGetValue((serializedObject, propPath), out var result) ? result : default;
        }

        private bool ShouldEdit { set => SetShouldEdit(serializedProperty, value); get => GetShouldEdit(serializedProperty); }
        private bool IsEditing { set => SetIsEdit(serializedProperty, value); get => GetIsEdit(serializedProperty); }
        private string EditString { set => SetEditString(serializedProperty, value); get => GetEditString(serializedProperty); }

        protected SerializedProperty serializedProperty;
        protected string label = "";
        protected string helpText = "";
        protected bool showHelpIcon = false;
        protected string defaultLabel = "-";

        private bool _editingCategory = false;
        private bool _renamingMainAsset = false;
        private List<int> _assetIndiciesToEdit = new List<int>();

        ~TagPropertyDrawerBase()
        {
            var key = (serializedProperty.serializedObject, serializedProperty.propertyPath);
            if (propertyDrawerShouldEdit.ContainsKey(key)) propertyDrawerShouldEdit.Remove(key);
            if (propertyDrawerIsEditing.ContainsKey(key)) propertyDrawerIsEditing.Remove(key);
            if (propertyDrawerEditString.ContainsKey(key)) propertyDrawerEditString.Remove(key);
            sentFromInstanceId = 0;
        }

        protected const float OPEN_BUTTON_WIDTH = 20f;

        protected float maxLabelWidth = 80f;
        protected float labelWidth = 80f;
        protected Rect dropDownRect;
        private float _lastLabelClickTime = 0;
        private static Type propertyType = typeof(ScriptableTag);
        private string _origSoGuid = string.Empty;
        private GUIStyle _lblStyle;
        private Rect _editRect;
        private GUIContent _pickerBtnContent = new GUIContent(EditorGUIUtility.FindTexture("Record Off@2x"), "Click to Open/Select this asset.");
        private GUIStyle _pickerBtnStyle;

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent lblIn)
        {
            _lblStyle = new GUIStyle(GUI.skin.label);
            _pickerBtnStyle = new GUIStyle(GUI.skin.button);
            GUIContent lbl = new GUIContent(lblIn);
            serializedProperty = property;
            //if (showHelpIcon)
            //{
            //    position.width -= 16;
            //}

            string origLabel = lbl.text;
            T currentSo = serializedProperty.objectReferenceValue as T;
            if (currentSo != null) AssetDatabase.TryGetGUIDAndLocalFileIdentifier(currentSo, out _origSoGuid, out long newId);
            Init(property, defaultLabel);

            if (!Application.isPlaying && (currentSo == null || (!AssetDatabase.IsSubAsset(currentSo) && !AssetDatabase.IsMainAsset(currentSo))))
            {
                if (defaultSo == null) FindAllAssets(property, defaultLabel);
                serializedProperty.objectReferenceValue = defaultSo;
                serializedProperty.serializedObject.ApplyModifiedProperties();
            }

            _editRect = new Rect(position);
            if (Event.current.type == EventType.Repaint)
            {
                maxLabelWidth = (position.width - 140f);

                float widthForLabel = GUI.skin.label.CalcSize(lbl).x + 10f;
                int labelLength = origLabel.Length;
                while (widthForLabel > maxLabelWidth && labelLength > 3)
                {
                    lbl.text = origLabel.Substring(0, labelLength--) + "..";
                    widthForLabel = GUI.skin.label.CalcSize(lbl).x + 10f;
                }

                labelWidth = Mathf.Min(widthForLabel, maxLabelWidth);
            }

            if (string.IsNullOrEmpty(origLabel)) labelWidth = -6f;
            float buttonLabelWidth = (labelWidth + OPEN_BUTTON_WIDTH);
            _editRect.x += buttonLabelWidth;
            _editRect.y += 1;
            _editRect.width -= buttonLabelWidth;

            float totalWidth = position.width;
            position.width = OPEN_BUTTON_WIDTH;

            _lblStyle.richText = true;

            _pickerBtnStyle.padding = new RectOffset(0, 0, 3, 3);
            Color defaultGUIColor = GUI.color;

            if (currentSo != null && sentFromInstanceId == currentSo.GetInstanceID()) GUI.color = Color.yellow;
            if (GUI.Button(position, _pickerBtnContent, _pickerBtnStyle))
            {
                if (property.objectReferenceValue != null)
                {
                    AssetDatabase.OpenAsset(property.objectReferenceValue);
                }
            }

            GUI.color = defaultGUIColor;
            position.x += position.width + 6f;
            position.width = labelWidth;
            lbl.tooltip = origLabel + "\nClick to ping, double-click to select this asset.";
            if (GUI.Button(position, lbl, GUI.skin.label))
            {
                if (property.objectReferenceValue != null)
                {
                    if (Time.time - _lastLabelClickTime < 0.5f)
                    {
                        TagEditorBase.SentFromInstanceId = serializedProperty.serializedObject.targetObject.GetInstanceID();
                        Selection.activeObject = property.objectReferenceValue;
                    }
                    else
                    {
                        EditorGUIUtility.PingObject(property.objectReferenceValue);
                    }
                }

                _lastLabelClickTime = Time.time;
            }

            position.x += position.width;
            //position.y += 1f;
            //position.width = totalWidth - labelWidth - OpenButtonWidth - HelpButtonWidth - 10f;
            position.width = totalWidth - labelWidth - OPEN_BUTTON_WIDTH - 6f;

            string tfControlName = "edit_tf";
            if (ShouldEdit)
            {
                GUI.SetNextControlName(tfControlName);
                EditString = EditorGUI.TextField(_editRect, EditString);
                if (!IsEditing)
                {
                    switch (Event.current.type)
                    {
                        case EventType.Layout:
                            EditorGUI.FocusTextInControl(tfControlName);
                            break;
                        case EventType.Repaint:
                            TextEditor tEditor = GetCurrTextEditor();
                            if (tEditor != null)
                            {
                                IsEditing = true;
                                float txtW = EditorStyles.textField.CalcSize(new GUIContent(EditString)).x;
                                tEditor.selectIndex = EditString.LastIndexOf("/") + 1;
                                tEditor.cursorIndex = EditString.Length;
                                tEditor.scrollOffset = new Vector2(txtW - tEditor.position.width + 1, 0);
                            }

                            break;
                    }
                }

                if ((Event.current.keyCode == KeyCode.Return || Event.current.keyCode == KeyCode.KeypadEnter || GUI.GetNameOfFocusedControl() != tfControlName))
                {
                    ShouldEdit = false;
                    IsEditing = false;
                    TryApplyEdit();
                }
                else if (Event.current.keyCode == KeyCode.Escape)
                {
                    ShouldEdit = false;
                    IsEditing = false;
                }
            }
            else
            {
                GUIContent content = new GUIContent(currentSo == null || currentSo.IsDefault ? defaultLabel : currentSo.name);
                if (TagSetting.Instance.showGroupNames && currentSo != null && !currentSo.IsDefault && !AssetDatabase.IsMainAsset(currentSo))
                {
                    var soPath = AssetDatabase.GetAssetPath(currentSo);
                    if (!string.IsNullOrEmpty(soPath))
                    {
                        var mainAss = AssetDatabase.LoadMainAssetAtPath(soPath);
                        if (mainAss != null) content = new GUIContent(mainAss.name + "/" + content.text);
                    }
                }

                content.tooltip = content.text + "\nClick to choose a different " + label;
                float widthForDropDown = EditorStyles.textField.CalcSize(content).x + 10f;
                int lblLength = content.text.Length;
                if (widthForDropDown > (position.width - 15f))
                {
                    int idx = content.text.Length - Mathf.Clamp(Mathf.RoundToInt((position.width - 15f) / widthForDropDown * lblLength) - 1, 8, lblLength - 1);
                    if (idx > 5 && lblLength > 5) content.text = content.text.Substring(0, 5) + ".." + content.text.Substring(idx + 5);
                }

                bool pressed = EditorGUI.DropdownButton(position, content, FocusType.Keyboard);
                if (pressed)
                {
                    FindAllAssets(serializedProperty, defaultLabel);
                    dropDownRect = GUIUtility.GUIToScreenRect(position);
                    ShowPopup(dropDownRect);
                }
            }

            if (showHelpIcon)
            {
                GUIContent helpTxt = new GUIContent("?", helpText);
                position.x += position.width + 4f;
                position.width = 10f;
                GUI.Button(position, helpTxt, GUI.skin.label);
            }
        }

        // Just WOW
        // Not only is *this* the way to change selection but we have to use reflection for EditorGUI.TextField
        // Although we could use GUI.TextField, text doesn't scroll as you type or allow copy/paste with one of those so here we are
        // Oh and it's not available on the first frame but we only want to override the selction on the frame we started editing
        // so there's that too. 
        private TextEditor GetCurrTextEditor()
        {
            return typeof(EditorGUI).GetField("activeEditor", BindingFlags.Static | BindingFlags.NonPublic).GetValue(null) as TextEditor;
        }

        private const float MIN_POPUP_WIDTH = 270f;

        public void ShowPopup(Rect popupButtonRect)
        {
            if (menuEntries == null || menuEntries.Count < 1) FindAllAssets(serializedProperty, defaultLabel);
            if (menuPopup == null)
            {
                menuPopup = ScriptableObject.CreateInstance<TagDropDownMenuWindow>();
            }
            else
            {
                menuPopup.Close();
                menuPopup = ScriptableObject.CreateInstance<TagDropDownMenuWindow>();
            }

            Rect popupWindowSize = new Rect(popupButtonRect);
            if (popupWindowSize.width < MIN_POPUP_WIDTH)
            {
                popupButtonRect.x += popupWindowSize.width - MIN_POPUP_WIDTH;
                popupWindowSize.width = MIN_POPUP_WIDTH;
            }

            T currentSo = serializedProperty.objectReferenceValue as T;
            if (currentSo != null)
            {
                TagGroupBase grp = AssetDatabase.IsMainAsset(currentSo)
                    ? null
                    : AssetDatabase.LoadMainAssetAtPath(AssetDatabase.GetAssetPath(currentSo)) as TagGroupBase;
                menuPopup.SelectedEntry = foundAssets.IndexOf((grp, currentSo));
            }
            else
            {
                menuPopup.SelectedEntry = -1;
            }

            // Arrays call the same property drawer multiple times so we need the 
            // popup window to return the property it was actually called for
            menuPopup.RelatedProperty = serializedProperty;
            menuPopup.OnAddCategory = HandleAddCategory;
            menuPopup.OnEditCategory = HandleEditCategory;
            menuPopup.OnDeleteCategory = HandleDeleteCategory;
            menuPopup.OnSelect = HandleSelect;
            menuPopup.OnEdit = HandleEdit;
            menuPopup.OnDuplicate = HandleDuplicate;
            menuPopup.OnDelete = HandleDelete;
            menuPopup.Build(menuEntries, popupWindowSize.width);
            menuPopup.ShowAsDropDown(popupButtonRect, new Vector2(popupWindowSize.width, 200f));
            menuPopup.Focus();
        }

        #region Selection

        private void HandleSelect(SerializedProperty prop, int menuIdx)
        {
            serializedProperty = prop;
            if (serializedProperty.serializedObject.targetObject == null) Debug.LogWarning("No targetobj for " + serializedProperty.serializedObject);
            Undo.RecordObject(serializedProperty.serializedObject.targetObject, "Change " + label);
            if (menuIdx < 0 || menuIdx >= foundAssets.Count)
            {
                SetToEmpty(ref serializedProperty);
                return;
            }

            if (foundAssets[menuIdx].asset != null) SetValue(foundAssets[menuIdx].asset);
        }

        protected virtual void SetValue(T asset)
        {
            if (serializedProperty.objectReferenceValue != null && serializedProperty.serializedObject.targetObject == asset)
            {
                Debug.LogWarning("Cyclical dependency detected. Reference to " + serializedProperty.serializedObject.targetObject + " can not be set to itself.");
                return;
            }

            serializedProperty.objectReferenceValue = asset;

            // Allows updating of registered tags when changing via the inspector at runtime
            if (asset is Tag.Tag && serializedProperty.serializedObject.targetObject != null)
            {
                var mtComponent = (serializedProperty.serializedObject.targetObject as MultiTagGameObject);
                if (mtComponent != null)
                {
                    var tags = serializedProperty.serializedObject.FindProperty("tags");
                    if (tags != null && tags.isArray)
                    {
                        Tag.Tag[] newTags = new Tag.Tag[tags.arraySize];
                        for (int i = 0; i < newTags.Length; ++i)
                        {
                            newTags[i] = tags.GetArrayElementAtIndex(i).objectReferenceValue as Tag.Tag;
                        }

                        mtComponent.SetTags(newTags);
                    }
                }
                else
                {
                    var tComponent = (serializedProperty.serializedObject.targetObject as TagGameObject);
                    if (tComponent != null)
                    {
                        tComponent.SetTag(asset as Tag.Tag);
                    }
                }
            }

            serializedProperty.serializedObject.ApplyModifiedProperties();

            // Hate that this is necessary
            // It appears if e.g. a subasset becomes a main asset with a new id, the object under serializedProperty.objectReferenceValue
            // is swapped out automatically under us without marking the serialized object as dirty.
            // As we don't want to dirty the component if the asset didn't actually change we get around it by comparing
            // the guid of the selected asset with the guid it had in OnGUI. 
            // For clarity, checking serizliedProperty.objectReferenceValue before setting it to 'asset' for any kind of evaluation
            // (guid, file id, instance id) will all appear as though they are identical.
            AssetDatabase.TryGetGUIDAndLocalFileIdentifier(asset, out var newGuid, out long newId);
            if (newGuid != _origSoGuid) EditorUtility.SetDirty(serializedProperty.serializedObject.targetObject);
            _origSoGuid = newGuid;
            if (menuPopup != null) menuPopup.Close();
        }

        #endregion

        #region Adding

        private void HandleAddCategory(SerializedProperty prop, int menuIdx, string categoryLabel)
        {
            serializedProperty = prop;
            if (menuIdx < 0 || menuIdx >= foundAssets.Count || foundAssets[menuIdx].group == null)
            {
                if (typeof(ScriptableTag).IsAssignableFrom(typeof(T)))
                {
                    var foundType = fieldInfo.FieldType;
                    if (foundType.IsArray) foundType = foundType.GetElementType();
                    var group = TagEditorUtils.CreateGroupForType(foundType, "Assets/", "New Group").group;
                    if (group == null)
                    {
                        // Can't create a Group for this type of SO - should have created a generic TagGroup but we'll try and create new individual asset
                        var newAsset = TagEditorUtils.CreateSOAsMainAssetForType(foundType, "Assets/", "New").asset as T;
                        if (newAsset == null)
                        {
                            Debug.LogWarning("Unable to create asset of type " + foundType);
                            return;
                        }

                        SetValue(newAsset);
                        SetEdit(null, newAsset);
                    }
                    else
                    {
                        var newAsset = CreateAssetInGroup(group, "New", true);
                        SetValue(newAsset);
                        SetEdit(group, newAsset);
                    }
                }
            }
            else
            {
                TagEditorUtils.ShowAddToGroupPopup(foundAssets[menuIdx].group,
                    string.IsNullOrEmpty(categoryLabel) ? string.Empty : categoryLabel + "/New",
                    HandleCreatedNewSo);
            }
        }

        internal void HandleCreatedNewSo(ScriptableTag so)
        {
            SetValue(so as T);
            FindAllAssets(serializedProperty, defaultLabel);
            SetEdit(AssetDatabase.LoadAssetAtPath<TagGroupBase>(AssetDatabase.GetAssetPath(so)), so as T);
        }

        //Group path is set when 'New' is clicked within a groups popup menu
        internal void AddNew(string groupPath = "", string existingName = "")
        {
            if (menuPopup != null) menuPopup.Close();

            T currentSo = serializedProperty.objectReferenceValue as T;
            TagGroupBase soGroup = (currentSo != null && !AssetDatabase.IsMainAsset(currentSo))
                ? AssetDatabase.LoadMainAssetAtPath(AssetDatabase.GetAssetPath(currentSo)) as TagGroupBase
                : null;

            T newSo = CreateAssetInGroup(soGroup, existingName);

            if (newSo != null)
            {
                // Select the newly created asset
                SetValue(newSo);

                // Immediately make it available to be renamed
                SetEdit(soGroup, newSo);
            }
        }

        public static (TagGroupBase group, string path) CreateGroup(string groupPath, string groupName) =>
            TagEditorUtils.CreateGroupForType(propertyType, groupPath, groupName);

        public static T CreateAssetInGroupAtPath(string groupPath = "", string soName = "", bool createIfAlreadyExists = false)
        {
            if (string.IsNullOrEmpty(groupPath)) groupPath = "Assets/New";
            bool addAssetExt = !Path.HasExtension(groupPath);
            TagGroupBase soGroup = AssetDatabase.LoadMainAssetAtPath(groupPath + (addAssetExt ? ".asset" : string.Empty)) as TagGroupBase;

            if (soGroup == null) soGroup = CreateGroup(groupPath, string.Empty).Item1;
            return (soGroup != null && !string.IsNullOrEmpty(soName) ? CreateAssetInGroup(soGroup, soName, createIfAlreadyExists) : null);
        }

        public static T CreateAssetInGroup(TagGroupBase soGroup, string soName = "", bool createIfAlreadyExists = true)
        {
            if (soGroup == null) return CreateAssetInGroupAtPath(string.Empty, soName, createIfAlreadyExists);
            if (!createIfAlreadyExists)
            {
                var existing = TagEditorUtils<TagGroupBase, T>.GetSOsForGroup(soGroup).FirstOrDefault(x => x.name.ToLower() == soName.ToLower());
                if (existing != null) return existing;
            }

            string newAssetName = TagEditorUtils.GetNextAvailableName(AssetDatabase.GetAssetPath(soGroup), soName);

            if (propertyType == typeof(ScriptableTag)) propertyType = typeof(T);

            // Create the new asset & reimport it to force update
            T newSo = ScriptableObject.CreateInstance(propertyType) as T;
            if (newSo == null)
            {
                Debug.LogWarning("Unable to create an asset of type " + typeof(T) + ", " + propertyType);
                return default;
            }

            newSo.name = newAssetName;
            AddAssetToGroup(newSo, soGroup);
            return newSo;
        }

        public static T AddAssetToGroup(T newSo, TagGroupBase soGroup)
        {
            AssetDatabase.AddObjectToAsset(newSo, soGroup);
            TagEditorUtils.ForceRefresh(soGroup);
            return newSo;
        }

        #endregion

        #region Duplication

        private void HandleDuplicate(SerializedProperty prop, int menuIdx)
        {
            serializedProperty = prop;
            if (menuIdx < 0 || menuIdx >= foundAssets.Count) return;

            if (menuPopup != null) menuPopup.Close();

            T currentSo = foundAssets[menuIdx].asset;
            TagGroupBase soGroup = (currentSo != null && !AssetDatabase.IsMainAsset(currentSo))
                ? AssetDatabase.LoadMainAssetAtPath(AssetDatabase.GetAssetPath(currentSo)) as TagGroupBase
                : null;

            T newSo = Object.Instantiate(currentSo);
            if (newSo != null)
            {
                string newAssetName = TagEditorUtils.GetNextAvailableName(AssetDatabase.GetAssetPath(soGroup), currentSo.name);
                newSo.EditorGenerateNewHash();
                newSo.name = newAssetName;
                AddAssetToGroup(newSo, soGroup);

                // Select the newly created asset
                SetValue(newSo);

                // Immediately make it available to be renamed
                SetEdit(soGroup, newSo);
            }
        }

        #endregion

        #region Editing

        private void HandleEdit(SerializedProperty prop, int menuIdx)
        {
            serializedProperty = prop;
            if (menuIdx < 0 || menuIdx >= foundAssets.Count) return;
            SetValue(foundAssets[menuIdx].asset);
            SetEdit(foundAssets[menuIdx].group, foundAssets[menuIdx].asset);
            FindAllAssets(serializedProperty, defaultLabel);
        }

        private TagGroupBase _workingGroup;

        private void HandleEditCategory(SerializedProperty prop, int menuIdx, int depth, string categoryLabel)
        {
            serializedProperty = prop;
            if (menuPopup != null) menuPopup.Close();

            _editingCategory = true;

            _assetIndiciesToEdit.Clear();
            FindmatchingAssets(menuIdx, depth, ref _assetIndiciesToEdit);
            _renamingMainAsset = false;
            if (_assetIndiciesToEdit.Count > 0)
            {
                TagGroupBase group = foundAssets[menuIdx].group;
                _workingGroup = group;
                string assetName = foundAssets[menuIdx].asset.name;
                string[] splitLabels = assetName.Split('/');
                for (int i = 1; i < splitLabels.Length; ++i) splitLabels[i] = splitLabels[i - 1] + "/" + splitLabels[i];
                string subDirName = string.Empty;
                if (depth > 0 && splitLabels.Length > 1 && depth <= splitLabels.Length) subDirName = splitLabels[depth - 1];

                if (subDirName == string.Empty)
                {
                    EditString = group != null ? group.name : string.Empty;
                    _renamingMainAsset = true;
                }
                else
                {
                    EditString = (group != null ? group.name + "/" : string.Empty) + subDirName;
                }

                //editString = partToEdit;
                ShouldEdit = true;
            }
        }

        private void SetEdit(TagGroupBase soGroup, T currentSo)
        {
            if (currentSo != null)
            {
                ShouldEdit = true;
                EditString = (soGroup != null ? soGroup.name + "/" : string.Empty) + currentSo.name;
            }
        }

        private void TryApplyEdit()
        {
            Undo.SetCurrentGroupName("Renaming " + label + "s");
            T currentSo = serializedProperty.objectReferenceValue as T;
            if (_editingCategory)
            {
                string newName = EditString;
                if (_renamingMainAsset)
                {
                    RenameGroup(_workingGroup, newName);
                }
                else
                {
                    for (int i = 0; i < _assetIndiciesToEdit.Count; ++i)
                    {
                        T asset = foundAssets[_assetIndiciesToEdit[i]].asset;
                        int lastDirIdx = Mathf.Max(0, asset.name.LastIndexOf("/"));

                        currentSo = RenameSoAsset(_workingGroup, asset, newName + asset.name.Substring(lastDirIdx));
                    }
                }

                _editingCategory = false;
            }
            else
            {
                if (currentSo == null) return;
                string newAssetName = EditString;
                _workingGroup = AssetDatabase.LoadAssetAtPath<TagGroupBase>(AssetDatabase.GetAssetPath(currentSo));
                currentSo = RenameSoAsset(_workingGroup, currentSo, newAssetName);
            }

            IsEditing = false;
            TagEditorUtils.ForceRefresh(_workingGroup);
            FindAllAssets(serializedProperty, defaultLabel);
            SetValue(currentSo);
        }

        #endregion

        #region Deleting

        private void HandleDelete(SerializedProperty prop, int menuIdx)
        {
            serializedProperty = prop;
            if (menuIdx < 0 || menuIdx >= foundAssets.Count) return;
            var delRslt = TryDeleteSo(foundAssets[menuIdx].asset, serializedProperty);
            FindAllAssets(serializedProperty, defaultLabel);
            if (delRslt) ShowPopup(dropDownRect);
        }

        private void HandleDeleteCategory(SerializedProperty prop, int menuIdx, int depth, string categoryLabel)
        {
            int numToDelete = 0;
            int numDeleted = 0;
            serializedProperty = prop;
            List<int> assetIndiciesToDelete = new List<int>();
            FindmatchingAssets(menuIdx, depth, ref assetIndiciesToDelete);

            for (int i = 0; i < assetIndiciesToDelete.Count; ++i)
            {
                if (assetIndiciesToDelete[i] < 0 && depth == 0 && foundAssets[menuIdx].group != null)
                {
                    for (int a = 0; a < foundAssets.Count; ++a)
                    {
                        if (foundAssets[a].group == foundAssets[menuIdx].group)
                        {
                            numToDelete++;
                            if (TryDeleteSo(foundAssets[a].asset, serializedProperty)) numDeleted++;
                        }
                    }
                }
                else
                {
                    numToDelete++;
                    if (TryDeleteSo(foundAssets[assetIndiciesToDelete[i]].asset, serializedProperty)) numDeleted++;
                }
            }

            if (numDeleted == numToDelete)
            {
                ShowNotification("Deleted " + categoryLabel);
            }
            else if (numDeleted > 0)
            {
                ShowNotification("Deleted " + numDeleted + "/" + numToDelete + " items in " + categoryLabel);
            }

            if (numDeleted == numToDelete && depth == 0)
            {
                if (foundAssets[menuIdx].group != null) DeleteAsset(foundAssets[menuIdx].group, serializedProperty);
            }

            if (menuPopup != null) menuPopup.Close();
            FindAllAssets(serializedProperty, defaultLabel);
            ShowPopup(dropDownRect);
        }

        public static bool TryDeleteSo(T asset, SerializedProperty serializedProperty = null)
        {
            bool success = true;
            if (asset != null)
            {
                success = CanDelete(asset, serializedProperty);
                if (success) DeleteAsset(asset, serializedProperty);
            }

            return success;
        }

        private static bool CanDelete(T asset, SerializedProperty serializedProperty = null)
        {
            if (asset == null) return false;
            List<SceneObjectIDBundle> assetReferences = new List<SceneObjectIDBundle>();
            ScriptableTagRegistry.References(asset, ref assetReferences, SearchRegistryOption.FullRefresh);

            if (assetReferences.Count > 1 || (serializedProperty != null && assetReferences.Count == 1 && serializedProperty.serializedObject.targetObject != null &&
                                              assetReferences[0].id != serializedProperty.serializedObject.targetObject.GetInstanceID()))
            {
                var showReferences = EditorUtility.DisplayDialog("Warning",
                    asset.name + " has " + assetReferences.Count + " references in this project. Would you like to see them?",
                    "No",
                    "Yes");
                if (!showReferences)
                {
                    if (menuPopup != null) menuPopup.Close();
                    Selection.activeObject = asset;
                    return false;
                }

                string msg = "This can't be undone. Any remaining references to " + asset.name +
                             " in your project will become invalid.\nAre you really sure you wish to delete it?";
                bool shouldDelete = !EditorUtility.DisplayDialog(asset.name + " uses", msg, "No", "Yes");

                if (!shouldDelete)
                {
                    TagGroupBase group = AssetDatabase.LoadMainAssetAtPath(AssetDatabase.GetAssetPath(asset)) as TagGroupBase;
                    msg = "<b>" + (group != null && group != asset ? group.name + "/" : string.Empty) + asset.name + "</b> is referenced in the following locations:\n";
                    msg += "<color=yellow>Warning:</color>\n\n";
                    for (int a = 0; a < Math.Min(100, assetReferences.Count); ++a)
                    {
                        msg += "<b>" + assetReferences[a].scenePath + ": " + assetReferences[a].objectName + "</b>\n";
                    }

                    msg += "\n";
                    Debug.LogWarning(msg);
                }

                return shouldDelete;
            }

            return true;
        }

        private static void DeleteAsset(ScriptableObject asset, SerializedProperty serializedProperty = null)
        {
            string assetPath = AssetDatabase.GetAssetPath(asset);
            if (AssetDatabase.IsMainAsset(asset))
            {
                AssetDatabase.DeleteAsset(AssetDatabase.GetAssetPath(asset));
            }
            else
            {
                AssetDatabase.RemoveObjectFromAsset(asset);
                TagEditorUtils.ForceRefresh(AssetDatabase.LoadAssetAtPath<TagGroupBase>(assetPath));
            }

            if (serializedProperty != null)
            {
                bool deletedAssetWasSelected = asset == serializedProperty.objectReferenceValue;
                if (deletedAssetWasSelected) SetToEmpty(ref serializedProperty);
            }
        }

        #endregion

        #region Utils

        private static void SetToEmpty(ref SerializedProperty prop)
        {
            prop.objectReferenceValue = defaultSo;
            prop.serializedObject.ApplyModifiedProperties();
            if (menuPopup != null) menuPopup.Close();
        }

        private void ShowNotification(string msg)
        {
            foreach (SceneView scene in SceneView.sceneViews)
            {
                scene.ShowNotification(new GUIContent(msg));
            }
        }

        private void FindmatchingAssets(int menuIdx, int depth, ref List<int> results)
        {
            TagGroupBase group = foundAssets[menuIdx].group;

            if (group != null && depth == 0)
            {
                // If we're affecting an entire group - i.e. a main asset,
                // simply return an entry out of range and the main asset will be edited
                results.Add(-1);
                return;
            }

            string assetName = foundAssets[menuIdx].asset.name;
            string[] splitLabels = assetName.Split('/');
            for (int i = 1; i < splitLabels.Length; ++i) splitLabels[i] = splitLabels[i - 1] + "/" + splitLabels[i];
            string subDirName = string.Empty;
            if (depth > 0 && splitLabels.Length > 1 && depth <= splitLabels.Length) subDirName = splitLabels[depth - 1];
            for (int i = 0; i < foundAssets.Count; ++i)
            {
                if (foundAssets[i].asset == null) continue;
                string curAssetName = foundAssets[i].asset.name;
                bool matches = group != null && foundAssets[i].group == group && !assetName.Contains('/');
                if (curAssetName.StartsWith(subDirName)) matches = foundAssets[i].group == group;
                if (matches) results.Add(i);
            }
        }

        internal static void RenameGroup(TagGroupBase group, string newName)
        {
            if (string.IsNullOrEmpty(newName)) return;

            Undo.RegisterCompleteObjectUndo(group, "Rename " + group.name + " to " + newName);

            newName = ReplaceInvalidChars(newName);
            string newMainAsset = newName.Split('/')[0];
            string remainingName = newName.Length > newMainAsset.Length ? newName.Substring(newMainAsset.Length + 1) : string.Empty;
            TagGroupBase existingGroup = null;
            for (int a = 0; a < foundAssets.Count; ++a)
            {
                if (foundAssets[a].group != null && foundAssets[a].group != group && foundAssets[a].group.name.ToLower() == newMainAsset.ToLower())
                {
                    existingGroup = foundAssets[a].group;
                    break;
                }
            }

            bool moveSubAssets = false;

            // If moving all assets in a group to a different existing group
            if (existingGroup != null)
            {
                // This will potentially change guids for serialized assets 
                if (!CheckOkIfInvalidateReference(group, "Asset")) return;
                moveSubAssets = true;
            }
            else
            {
                // If renaming the main name of the group, do that here
                AssetDatabase.SetMainObject(group, AssetDatabase.GetAssetPath(group));
                AssetDatabase.RenameAsset(AssetDatabase.GetAssetPath(group), newMainAsset);
            }

            // It's possible that an entire group could be renamed in the following way:
            // GroupA -> Group/A
            // In such a case, as well as renaming the main asset we also want to change the names
            // of all the subassets of GroupA
            if (remainingName.Length > 0 || moveSubAssets)
            {
                for (int a = 0; a < foundAssets.Count; ++a)
                {
                    if (foundAssets[a].group == group && foundAssets[a].asset != null)
                    {
                        foundAssets[a].asset.name = remainingName + "/" + foundAssets[a].asset.name;

                        // Subassets are moving to a different main asset
                        // Doing so will change GUIDs for any serialized asset references
                        if (moveSubAssets)
                        {
                            var existingHash = foundAssets[a].asset.Hash;
                            AssetDatabase.RemoveObjectFromAsset(foundAssets[a].asset);
                            AssetDatabase.SetMainObject(existingGroup, AssetDatabase.GetAssetPath(existingGroup));
                            AssetDatabase.AddObjectToAsset(foundAssets[a].asset, AssetDatabase.GetAssetPath(existingGroup));
                            var prop = typeof(ScriptableTag).GetField("hash", BindingFlags.NonPublic | BindingFlags.Instance);
                            prop.SetValue(foundAssets[a].asset, existingHash);
                        }
                    }
                }
            }

            TagEditorUtils.ForceRefresh(group);
        }

        internal static T RenameSoAsset(TagGroupBase existingGroup, T soAsset, string newName)
        {
            if (string.IsNullOrEmpty(newName)) return soAsset;

            Undo.RecordObject(soAsset, "Rename " + soAsset.name);

            newName = ReplaceInvalidChars(newName);
            bool nameHasDirectory = newName.Contains("/");
            // If soAsset has no related group, assumed to also be main asset
            // If the new name contains / this means the main asset wants to be relocated to be a subasset
            if (existingGroup == null && !nameHasDirectory)
            {
                // This sub asset apparently has no group - it is then assumped it must in fact be a main asset
                if (!AssetDatabase.IsMainAsset(soAsset))
                {
                    Debug.LogAssertion("This was unexpected " + soAsset + " is not a Main Asset");
                    return soAsset;
                }

                // If it is a main asset and can be simply renamed
                AssetDatabase.RenameAsset(AssetDatabase.GetAssetPath(soAsset), newName);
            }
            else
            {
                var nameSlices = newName.Split('/');
                string newMainAsset = nameSlices[0];
                string remainingName = newName.Length > newMainAsset.Length ? newName.Substring(newMainAsset.Length + 1) : newName;

                if (existingGroup != null && newMainAsset == existingGroup.name)
                {
                    // If the subasset is still a member of it's original group then simply rename
                    if (newName.Length > existingGroup.name.Length) soAsset.name = newName.Substring(existingGroup.name.Length + 1);
                    TagEditorUtils.ForceRefresh(existingGroup);
                }
                else
                {
                    // Asset was subasset (as otherwise would have had group and caught in previous if)
                    // so if no "/" assume sub asset wants to be removed from group
                    // and exist as its own main asset
                    if (!nameHasDirectory)
                    {
                        return TagEditorUtils.ChangeSubAssetToMainAsset(soAsset, existingGroup, newName);
                    }

                    TagGroupBase destGroup = null;
                    for (int a = 0; a < foundAssets.Count; ++a)
                    {
                        if (foundAssets[a].group != null && foundAssets[a].group.name.ToLower() == newMainAsset.ToLower())
                        {
                            destGroup = foundAssets[a].group;
                            break;
                        }
                    }

                    string destGroupPath;
                    if (destGroup == null)
                    {
                        // If group doesn't exist, create it
                        if (existingGroup == null)
                        {
                            destGroupPath = AssetDatabase.GetAssetPath(soAsset).Replace(soAsset.name, newMainAsset);
                        }
                        else
                        {
                            destGroupPath = AssetDatabase.GetAssetPath(existingGroup).Replace(existingGroup.name, newMainAsset);
                        }

                        var newGrpResult = CreateGroup(destGroupPath, newMainAsset);
                        destGroup = newGrpResult.group;
                        destGroupPath = newGrpResult.path;
                    }
                    else
                    {
                        destGroupPath = AssetDatabase.GetAssetPath(destGroup);
                    }

                    // Merging sub asset into existing group
                    soAsset.name = remainingName;

                    bool alreadyExists = false;
                    if (destGroup == null)
                    {
                        Debug.LogWarning("Failed to find or create group " + destGroupPath + "," + newMainAsset);
                        return soAsset;
                    }

                    // If a subasset within an existing group has the same name, check whether user
                    // wants to merge them or keep two with identical names
                    var existingSubAssets = AssetDatabase.LoadAllAssetRepresentationsAtPath(destGroupPath);
                    for (int i = 0; i < existingSubAssets.Length; ++i)
                    {
                        if (existingSubAssets[i] is T && existingSubAssets[i].name.ToLower() == soAsset.name.ToLower())
                        {
                            alreadyExists = true;
                            break;
                        }
                    }

                    if (alreadyExists)
                    {
                        bool shouldHaveSameName = EditorUtility.DisplayDialog("Renaming " + soAsset.name,
                            "An asset in " + destGroup + " already has the same name: " + soAsset.name + ". Are you sure you want two with identical names?",
                            "Yes",
                            "No");
                        if (shouldHaveSameName) alreadyExists = false;
                    }

                    if (!alreadyExists)
                    {
                        var clone = Object.Instantiate(soAsset);
                        AssetDatabase.AddObjectToAsset(clone, destGroupPath);
                        clone.name = soAsset.name;

                        // Otherwise this asset is moving to a different asset
                        // This will potentially change guids for serialized assets 
                        if (!CheckOkIfInvalidateReference(soAsset, "asset's")) return soAsset;
                        ScriptableTagRegistry.ReplaceSOUsage(soAsset, SearchRegistryOption.FullRefresh, clone);

                        // If there is only one sub asset, delete the existing group too
                        if (existingGroup != null)
                        {
                            if (AssetDatabase.LoadAllAssetRepresentationsAtPath(AssetDatabase.GetAssetPath(existingGroup)).Length <= 1)
                            {
                                AssetDatabase.DeleteAsset(AssetDatabase.GetAssetPath(existingGroup));
                            }
                            else
                            {
                                AssetDatabase.RemoveObjectFromAsset(soAsset);
                                TagEditorUtils.ForceRefresh(existingGroup);
                            }
                        }
                        else
                        {
                            AssetDatabase.DeleteAsset(AssetDatabase.GetAssetPath(soAsset));
                        }

                        TagEditorUtils.ForceRefresh(destGroup);
                        return clone;
                    }
                }
            }

            return soAsset;
        }

        private static char[] invalidChars = Path.GetInvalidFileNameChars().Where(x => !x.Equals('/')).ToArray();

        public static string ReplaceInvalidChars(string filename)
        {
            while (filename.Contains("//")) filename = filename.Replace("//", "/");
            if (filename.StartsWith("/")) filename = filename.Substring(1);
            return string.Join("_", filename.Split(invalidChars));
        }

        // Tag serialized references to a base ScriptableObject
        // As names and categories are managed by manipulating Assets & Subassets it's important to note that:
        // If a subasset moves to another asset (i.e. it gets re-categorised) the GUID associated with the SO 
        // will change. This can lead to components referencing either un-intended or non-existent assets.
        // Tag automatically replaces all references in Prefabs, Scenes, Subscenes & Assets so we should no longer need to warn.
        private static bool CheckOkIfInvalidateReference(UnityEngine.Object obj, string label) { return true; }

        #endregion

        #region Static Find Assets

        private static void Init(SerializedProperty property, string defaultLabel)
        {
            if (!initialized)
            {
                initialized = true;
                FindAllAssets(property, defaultLabel);
                Undo.undoRedoPerformed -= () => FindAllAssets(property, defaultLabel);
                Undo.undoRedoPerformed += () => FindAllAssets(property, defaultLabel);
            }
        }

        internal static void SetPropertyTypeFrom(SerializedProperty property)
        {
            try
            {
                SetPropertyTypeFrom(property.type);
            }
            catch
            {
            }
        }

        internal static void SetPropertyTypeFrom(string type)
        {
            var match = Regex.Match(type, @"PPtr<\$(.*?)>");
            if (!match.Success)
            {
                Debug.LogWarning("No results for " + type);
                return;
            }

            type = match.Groups[1].Value;
            propertyType = TypeCache.GetTypesDerivedFrom<ScriptableTag>().FirstOrDefault(x => x.Name == type);
        }

        internal static T FindDefault()
        {
            if (defaultSo != null) return defaultSo;
            FindAllAssets(null, string.Empty);
            return defaultSo;
        }

        private static T defaultSo = null;

        internal static void FindAllAssets(SerializedProperty property, string defaultLabel)
        {
            menuEntries.Clear();
            foundAssets.Clear();

            SetPropertyTypeFrom(property);

            string[] groupGuids = AssetDatabase.FindAssets("t:" + typeof(TagGroupBase));
            for (int groupIndex = 0; groupIndex < groupGuids.Length; ++groupIndex)
            {
                string groupPath = AssetDatabase.GUIDToAssetPath(groupGuids[groupIndex]);
                TagGroupBase group = AssetDatabase.LoadMainAssetAtPath(groupPath) as TagGroupBase;
                string groupName = Path.GetFileNameWithoutExtension(groupPath) + "/";
                UnityEngine.Object[] allAssets = AssetDatabase.LoadAllAssetRepresentationsAtPath(groupPath);
                List<string> uniqueNames = new List<string>();

                for (int i = 0; i < allAssets.Length; ++i)
                {
                    if (allAssets[i] == null) continue;
                    string name = groupName + (allAssets[i].name.Length >= 1 ? allAssets[i].name : " -- ");
                    int attempts = 0;
                    while (uniqueNames.Contains(name) && attempts < 100)
                    {
                        attempts++;
                        name = groupName + allAssets[i].name + " (" + attempts + ")";
                    }

                    uniqueNames.Add(name);

                    if (propertyType.IsAssignableFrom(allAssets[i].GetType()))
                    {
                        T asset = allAssets[i] as T;
                        if (asset != null)
                        {
                            if (!asset.IsDefault)
                            {
                                foundAssets.Add((group, asset));
                                menuEntries.Add((name, foundAssets.Count, true));
                            }
                            else if (defaultSo == null)
                            {
                                defaultSo = asset;
                            }
                        }
                    }
                    else if (defaultSo == null && allAssets[i] is T)
                    {
                        defaultSo = allAssets[i] as T;
                    }
                }

                if (allAssets.Length == 0)
                {
                    foundAssets.Add((group, null));
                    menuEntries.Add((groupName, foundAssets.Count, true));
                }
            }

            string[] individualSoGuids = AssetDatabase.FindAssets("t:" + typeof(T));
            for (int i = 0; i < individualSoGuids.Length; ++i)
            {
                T individualAsset = AssetDatabase.LoadMainAssetAtPath(AssetDatabase.GUIDToAssetPath(individualSoGuids[i])) as T;
                if (individualAsset != null && propertyType.IsAssignableFrom(individualAsset.GetType()))
                {
                    if (individualAsset.IsDefault)
                    {
                        if (defaultSo == null) defaultSo = individualAsset;
                    }
                    else
                    {
                        foundAssets.Add((null, individualAsset));
                        menuEntries.Add((individualAsset.name, foundAssets.Count, true));
                    }
                }
            }

            if (defaultSo == null)
            {
                CreateDefaultSo(defaultLabel);
            }

            menuEntries.Sort((x, y) => x.label.CompareTo(y.label));

            // Add -None- entry
            foundAssets.Insert(0, (null, null));
            menuEntries.Insert(0, (defaultLabel, -1, false));

            // Add Create New Group entry
            foundAssets.Add((AssetDatabase.LoadAssetAtPath<TagGroupBase>(AssetDatabase.GetAssetPath(defaultSo)), defaultSo));
            menuEntries.Add(("New Group", -1, false));
        }

        private static void CreateDefaultSo(string defaultLabel)
        {
            var defaultGroup = ProjectDatabase.FindAssetWithPath<TagGroupBase>("Defaults.asset", "Modules/Tag");
            if (defaultGroup == null)
            {
                var gp = TagEditorUtils.CreateGroupForType(typeof(ScriptableTag), "Assets/heart/Modules/Tag/Defaults.asset", "Defaults");
                defaultGroup = gp.group;
            }

            defaultSo = CreateAssetInGroup(defaultGroup, "-None-", true);

            if (defaultSo == null)
            {
                Debug.LogWarning("Failed to automatically create default SO for " + propertyType);
            }
            else
            {
                defaultSo.name = defaultLabel;
                defaultSo.ManuallySetHash(default);
                TagEditorUtils.ForceRefresh(defaultGroup);
            }
        }

        #endregion
    }
}