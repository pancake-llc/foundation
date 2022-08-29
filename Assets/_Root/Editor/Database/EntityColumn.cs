using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace Pancake.Database
{
    public class EntityColumn : DashboardColumn
    {
        public Action<Entity> onPick;
        public ListView listElement;
        private List<Entity> _filterdList;
        private bool _isFiltering;

        public List<Entity> currentSelections;
        public EntityColumn() { Rebuild(); }

        public override void Rebuild()
        {
            Clear();

            this.style.flexGrow = 1;
            this.name = "Asset List Wrapper";
            this.viewDataKey = "asset_list_wrapper";

            listElement = new ListView(Dashboard.CurrentSelectedGroup == null ? new List<Entity>() : Dashboard.CurrentSelectedGroup.Content,
                32,
                ListMakeItem,
                ListBindItem)
            {
                name = "Asset List View",
                viewDataKey = "asset_list",
                style = {flexGrow = 1, height = new StyleLength(new Length(100, LengthUnit.Percent))},
                selectionType = SelectionType.Multiple
            };

#if UNITY_2020_3_OR_NEWER
            listElement.onSelectionChange += SelectAssetsInternal;
            listElement.onItemsChosen += ChooseAssetInternal;
#else
            ListElement.onSelectionChanged += SelectAssetsInternal;
            ListElement.onItemChosen += ChooseAssetInternal;
#endif
            // plug into events for updates.
            Dashboard.onSearchEntity = CallbackListBySearch;
            Dashboard.onCurrentGroupChanged = CallbackListByType;

            Add(listElement);
            ListAssetsByGroup();

            if (!string.IsNullOrEmpty(Dashboard.entitySearch.value))
            {
                ListAssetsBySearch();
            }

            GetSelectionPersistence();
            if (_isFiltering) CallbackListBySearch();

            Pick(Dashboard.CurrentSelectedEntity);
        }

        private void CallbackListBySearch() { ListAssetsBySearch(true); }

        private void CallbackListByType() { ListAssetsByGroup(true); }

        public void ListAssetsByGroup(bool scrollToTop = false)
        {
            if (Dashboard.CurrentSelectedGroup == null) return;

            _isFiltering = false;

            if (Dashboard.CurrentSelectedGroup.Content != null && Dashboard.CurrentSelectedGroup.Content.Count > 1)
            {
                Dashboard.CurrentSelectedGroup.CleanUp();
                Dashboard.CurrentSelectedGroup.Content.Sort((x, y) => string.CompareOrdinal(x.Title, y.Title));
            }

            listElement.itemsSource = Dashboard.CurrentSelectedGroup.Content;
            listElement.Rebuild();

            if (scrollToTop) listElement.ScrollToItem(0);
        }

        public void ListAssetsBySearch(bool scrollToTop = false)
        {
            if (Dashboard.CurrentSelectedGroup.Content.Count == 0) ListAssetsByGroup();

            _isFiltering = true;
            _filterdList = Dashboard.CurrentSelectedGroup.Content.FindAll(SearchMatchesItem);
            _filterdList.Sort((x, y) => string.CompareOrdinal(x.Title, y.Title));

            listElement.itemsSource = _filterdList;
            listElement.Rebuild();
            if (scrollToTop) listElement.ScrollToItem(0);
        }

        /// <summary>
        /// ONLY for use when you want something external to change the list selection.
        /// This will change the list index and subsequently trigger the internal method
        /// to fire the global changed event so everything else catches up.
        /// </summary>
        public void Pick(Entity asset)
        {
            // fail out
            if (asset == null) return;
            //if (!Dashboard.CurrentSelectedGroup.Data.Contains(asset)) Rebuild(); // todo fails and causes stack alloc overload?

            // set index
            int index = listElement.itemsSource.IndexOf(asset);
            listElement.selectedIndex = index;
            listElement.ScrollToItem(index);
            onPick?.Invoke(asset);
        }

        private static bool SearchMatchesItem(Entity entity)
        {
            bool result = entity.Title.ToLower().Contains(Dashboard.entitySearch.value.ToLower());
            return result;
        }

        /// <summary>
        /// ONLY for use when the list has chosen something.
        /// </summary>
        /// <param name="obj"></param>
        private void ChooseAssetInternal(object obj)
        {
            // fail
            Entity entity;
            if (listElement.selectedIndex < 0) return;
            if (obj == null) return;
            if (obj is IList)
            {
                entity = ((List<object>) obj)[0] as Entity;
                if (entity == Dashboard.CurrentSelectedEntity) return;
            }
            else
            {
                entity = (Entity) obj;
                if (entity == Dashboard.CurrentSelectedEntity) return;
            }

            // set index in prefs
            int index = listElement.itemsSource.IndexOf(entity);
            EditorSettings.Set(EditorSettings.ESettingKey.CurrentEntityGuid, listElement.selectedIndex.ToString());

            // broadcast change
            Dashboard.SetCurrentInspectorAsset(entity);
        }
#if UNITY_2020_3_OR_NEWER
        private void SelectAssetsInternal(IEnumerable<object> input)
        {
            var objs = (List<object>) input;
#else
        private void SelectAssetsInternal(List<object> objs)
        {
#endif
            currentSelections = objs.ConvertAll(asset => (Entity) asset);
            var sb = new StringBuilder();
            foreach (var assetFile in currentSelections)
            {
                sb.Append(AssetDatabase.GetAssetPath(assetFile) + "|");
            }

            EditorSettings.Set(EditorSettings.ESettingKey.SelectedEntityGuids, sb.ToString());
            if (objs.Count > 0) ChooseAssetInternal(objs[0]);
        }

        private void GetSelectionPersistence()
        {
            string selected = EditorSettings.Get(EditorSettings.ESettingKey.SelectedEntityGuids);
            if (string.IsNullOrEmpty(selected)) return;

            currentSelections = new List<Entity>();
            string[] split = selected.Split('|');
            foreach (string path in split)
            {
                if (path == string.Empty || path.Contains('|')) continue;
                currentSelections.Add(AssetDatabase.LoadAssetAtPath<Entity>(path));
            }

            Dashboard.CurrentSelectedEntity = currentSelections[0];
        }

        private void ListBindItem(VisualElement element, int listIndex)
        {
            // find the serialized property
            var ed = UnityEditor.Editor.CreateEditor(_isFiltering ? _filterdList[listIndex] : Dashboard.CurrentSelectedGroup.Content[listIndex]);
            if (ed == null) return;
            var so = ed.serializedObject;

            var propTitle = so.FindProperty("title");
            var sprite = ((Entity) so.targetObject).GetIcon;

            //element.RegisterCallback<PointerDownEvent>(evt => InDrag = (Entity)ListElement.itemsSource[listIndex]);

            // images are not bindable
            ((Image) element.ElementAt(0)).image = sprite != null ? AssetPreview.GetAssetPreview(sprite) : EditorGUIUtility.IconContent("d_ToolHandleLocal@2x").image;

            // bind the label to the serialized target target property title
            ((Label) element.ElementAt(1)).BindProperty(propTitle);
        }

        private static VisualElement ListMakeItem()
        {
            var selectableItem = new VisualElement
            {
                style =
                {
                    flexDirection = FlexDirection.Row,
                    flexGrow = 1f,
                    flexBasis = 1,
                    flexShrink = 1,
                    flexWrap = new StyleEnum<Wrap>(Wrap.NoWrap)
                }
            };

            //selectableItem.Add(new Label {name = "Prefix", text = "error", style = {unityFontStyleAndWeight = FontStyle.Bold}});
            var icon = new Image
            {
                style =
                {
                    flexGrow = 0,
                    flexShrink = 0,
                    paddingLeft = 5,
                    height = 32,
                    width = 32
                },
                scaleMode = ScaleMode.ScaleAndCrop
            };

            var label = new Label {name = "Asset Title", text = "loading...", style = {paddingLeft = 5}};

            selectableItem.Add(icon);
            selectableItem.Add(label);
            selectableItem.AddManipulator(new DashboardDragManipulator(selectableItem, EditorWindow.focusedWindow.rootVisualElement));

            return selectableItem;
        }

        /// <summary>
        /// Creates a new asset of the provided type, then focuses the dashboard on it.
        /// </summary>
        /// <param name="t">Type to create. Must derive from Entity.</param>
        /// <returns>The newly created asset object</returns>
        public Entity Create(Type t)
        {
            if (t == null)
            {
                Debug.LogError("Type for new asset cannot be null.");
                return null;
            }

            if (t.IsAbstract)
            {
                Debug.LogError("Cannot create instances of abstract classes.");
                return null;
            }

            const string prefix = "Data-";
            const string suffix = ".asset";
            var filename = $"{prefix}{t.Name}-{Ulid.NewUlid()}{suffix}";
            string assetPathAndName = AssetDatabase.GenerateUniqueAssetPath($"{EditorUtility.StoragePath()}{filename}");

            var asset = ScriptableObject.CreateInstance(t);
            AssetDatabase.CreateAsset(asset, assetPathAndName);
            var real = (Entity) AssetDatabase.LoadAssetAtPath<ScriptableObject>(assetPathAndName);
            Data.Database.Add(real, true);
            var group = Data.Database.GetStaticGroup(t);
            group.Add(real);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Dashboard.entitySearch.SetValueWithoutNotify(string.Empty);

            Dashboard.SetCurrentGroup(group);
            Pick(real);

            return real;
        }

        public void CloneSelection()
        {
            if (Dashboard.CurrentSelectedGroup == null) return;
            if (Dashboard.CurrentSelectedEntity == null) return;

            const string prefix = "Data-";
            const string suffix = ".asset";
            var filename = $"{prefix}{Dashboard.CurrentSelectedEntity.GetType().Name}-{Ulid.NewUlid()}{suffix}";
            string assetPathAndName = AssetDatabase.GenerateUniqueAssetPath($"{EditorUtility.StoragePath()}{filename}");

            AssetDatabase.CopyAsset(AssetDatabase.GetAssetPath(Dashboard.CurrentSelectedEntity), assetPathAndName);
            var newEntity = AssetDatabase.LoadAssetAtPath<Entity>(assetPathAndName);
            newEntity.Title += " (CLONED)";
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            var real = (Entity) AssetDatabase.LoadAssetAtPath<ScriptableObject>(assetPathAndName);
            real.ID = Ulid.NewUlid().ToString();
            Dashboard.entitySearch.SetValueWithoutNotify(string.Empty);

            Rebuild();

            int i = Mathf.Clamp(listElement.itemsSource.IndexOf(real), 0, listElement.itemsSource.Count);
            listElement.ScrollToItem(i);
            listElement.selectedIndex = i;
        }

        public void DeleteSelection()
        {
            var sb = new StringBuilder();
            if (currentSelections.Count == 0) return;
            if (Dashboard.CurrentSelectedEntity == null) return;

            foreach (var asset in currentSelections)
            {
                if (asset == null) continue;
                sb.Append(asset.Title + "\n");
            }

            bool confirm = UnityEditor.EditorUtility.DisplayDialog("Deletion warning!", $"Delete assets from the disk?\n\n{sb}", "Yes", "Cancel");
            if (!confirm) return;

            foreach (var asset in currentSelections)
            {
                Data.Database.Remove(asset.ID);
                AssetDatabase.DeleteAsset(AssetDatabase.GetAssetPath(asset));
            }

            Dashboard.CurrentSelectedEntity = null;

            Rebuild();
        }
    }
}