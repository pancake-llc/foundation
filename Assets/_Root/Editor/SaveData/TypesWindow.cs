using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;
using System.Reflection;
using System.Linq;
using System.IO;
using Pancake.SaveData;

// ReSharper disable FieldCanBeMadeReadOnly.Global

namespace Pancake.Editor.SaveData
{
    internal class TypesWindow : SubWindow
    {
        private TypeListItem[] _types;
        private const int RECENT_TYPE_COUNT = 5;
        private List<int> _recentTypes = new List<int>(RECENT_TYPE_COUNT);

        private Vector2 _typeListScrollPos = Vector2.zero;
        private Vector2 _typePaneScrollPos = Vector2.zero;
        private int _leftPaneWidth = 300;

        private string _searchFieldValue = "";

        private int _selectedType = -1;
        private Reflection.ReflectedMember[] _fields = new Reflection.ReflectedMember[0];
        private bool[] _fieldSelected = new bool[0];

        private GUIStyle _searchBarStyle;
        private GUIStyle _searchBarCancelButtonStyle;
        private GUIStyle _leftPaneStyle;
        private GUIStyle _typeButtonStyle;
        private GUIStyle _selectedTypeButtonStyle;
        private GUIStyle _selectAllNoneButtonStyle;

        private bool _unsavedChanges = false;

        public TypesWindow(EditorWindow window)
            : base("Types", window)
        {
        }

        public override void OnGUI()
        {
            if (_types == null) Init();

            EditorGUILayout.BeginHorizontal();

            EditorGUILayout.BeginVertical(_leftPaneStyle);
            SearchBar();
            TypeList();
            EditorGUILayout.EndVertical();
            EditorGUILayout.BeginVertical();
            TypePane();
            EditorGUILayout.EndVertical();

            EditorGUILayout.EndHorizontal();
        }

        private void SearchBar()
        {
            var style = EditorStyle.Get;

            GUILayout.Label("Enter a type name in the field below\n* Type names are case-sensitive *", style.subheading2);

            EditorGUILayout.BeginHorizontal();

            // Set control name so we can force a Focus reset for it.
            string currentSearchFieldValue = EditorGUILayout.TextField(_searchFieldValue, _searchBarStyle);

            if (_searchFieldValue != currentSearchFieldValue)
            {
                _searchFieldValue = currentSearchFieldValue;
                PerformSearch(currentSearchFieldValue);
            }

            GUI.SetNextControlName("Clear");

            if (GUILayout.Button("x", _searchBarCancelButtonStyle))
            {
                _searchFieldValue = "";
                GUI.FocusControl("Clear");
                PerformSearch("");
            }

            EditorGUILayout.EndHorizontal();
        }

        private void RecentTypeList()
        {
            if (!string.IsNullOrEmpty(_searchFieldValue) || _recentTypes.Count == 0) return;

            for (int i = _recentTypes.Count - 1; i > -1; i--)
                TypeButton(_recentTypes[i]);

            EditorGUILayout.TextArea("", GUI.skin.horizontalSlider);
        }

        private void TypeList()
        {
            if (!string.IsNullOrEmpty(_searchFieldValue))
                GUILayout.Label("Search Results", EditorStyles.boldLabel);

            _typeListScrollPos = EditorGUILayout.BeginScrollView(_typeListScrollPos);

            RecentTypeList();

            if (!string.IsNullOrEmpty(_searchFieldValue))
                for (int i = 0; i < _types.Length; i++)
                    TypeButton(i);

            EditorGUILayout.EndScrollView();
        }

        private void TypePane()
        {
            if (_selectedType < 0)
                return;

            var style = EditorStyle.Get;

            var typeListItem = _types[_selectedType];
            var type = typeListItem.type;

            _typePaneScrollPos = EditorGUILayout.BeginScrollView(_typePaneScrollPos, style.area);

            GUILayout.Label(typeListItem.name, style.subheading);
            GUILayout.Label(typeListItem.namespaceName);

            EditorGUILayout.BeginVertical(style.area);

            bool hasParameterlessConstructor = Reflection.HasParameterlessConstructor(type);
            bool isComponent = Reflection.IsAssignableFrom(typeof(Component), type);

            string path = GetOutputPath(_types[_selectedType].type);
            // An CustomType file already exists.
            if (File.Exists(path))
            {
                if (hasParameterlessConstructor || isComponent)
                {
                    EditorGUILayout.BeginHorizontal();
                    if (GUILayout.Button("Reset to Default"))
                    {
                        SelectNone(true, true);
                        AssetDatabase.MoveAssetToTrash("Assets" + path.Remove(0, Application.dataPath.Length));
                        SelectType(_selectedType);
                    }

                    if (GUILayout.Button("Edit CustomType Script"))
                        AssetDatabase.OpenAsset(AssetDatabase.LoadMainAssetAtPath("Assets" + path.Remove(0, Application.dataPath.Length)));
                    EditorGUILayout.EndHorizontal();
                }
                else
                {
                    EditorGUILayout.HelpBox(
                        "This type has no public parameterless constructors.\n\nTo support this type you will need to modify the CustomType script to use a specific constructor instead of the parameterless constructor.",
                        MessageType.Info);
                    if (GUILayout.Button("Click here to edit the CustomType script"))
                        AssetDatabase.OpenAsset(AssetDatabase.LoadMainAssetAtPath("Assets" + path.Remove(0, Application.dataPath.Length)));
                    if (GUILayout.Button("Reset to Default"))
                    {
                        SelectAll(true, true);
                        File.Delete(path);
                        AssetDatabase.Refresh();
                    }
                }
            }
            // No CustomType file and no fields.
            else if (_fields.Length == 0)
            {
                if (!hasParameterlessConstructor && !isComponent)
                    EditorGUILayout.HelpBox(
                        "This type has no public parameterless constructors.\n\nTo support this type you will need to create an CustomType script and modify it to use a specific constructor instead of the parameterless constructor.",
                        MessageType.Info);

                if (GUILayout.Button("Create CustomType Script"))
                    Generate();
            }
            // No CustomType file, but fields are selectable.
            else
            {
                if (!hasParameterlessConstructor && !isComponent)
                {
                    EditorGUILayout.HelpBox(
                        "This type has no public parameterless constructors.\n\nTo support this type you will need to select the fields you wish to serialize below, and then modify the generated CustomType script to use a specific constructor instead of the parameterless constructor.",
                        MessageType.Info);
                    if (GUILayout.Button("Select all fields and generate CustomType script"))
                    {
                        SelectAll(true, false);
                        Generate();
                    }
                }
                else
                {
                    if (GUILayout.Button("Create CustomType Script"))
                        Generate();
                }
            }

            EditorGUILayout.EndVertical();

            PropertyPane();

            EditorGUILayout.EndScrollView();
        }

        private void PropertyPane()
        {
            var style = EditorStyle.Get;

            EditorGUILayout.BeginVertical(style.area);

            GUILayout.Label("Fields", EditorStyles.boldLabel);

            DisplayFieldsOrProperties(true, false);
            EditorGUILayout.Space();

            GUILayout.Label("Properties", EditorStyles.boldLabel);

            DisplayFieldsOrProperties(false, true);
            EditorGUILayout.EndVertical();
        }

        private void DisplayFieldsOrProperties(bool showFields, bool showProperties)
        {
            // Get field and property counts.
            int fieldCount = 0;
            int propertyCount = 0;
            for (int i = 0; i < _fields.Length; i++)
            {
                if (_fields[i].isProperty && showProperties)
                    propertyCount++;
                else if ((!_fields[i].isProperty) && showFields)
                    fieldCount++;
            }

            // If there is nothing to display, show message.
            if (showFields && showProperties && fieldCount == 0 && propertyCount == 0)
                GUILayout.Label("This type has no serializable fields or properties.");
            else if (showFields && fieldCount == 0)
                GUILayout.Label("This type has no serializable fields.");
            else if (showProperties && propertyCount == 0)
                GUILayout.Label("This type has no serializable properties.");

            // Display Select All/Select None buttons only if there are fields to display.
            if (fieldCount > 0 || propertyCount > 0)
            {
                EditorGUILayout.BeginHorizontal();

                if (GUILayout.Button("Select All", _selectAllNoneButtonStyle))
                {
                    SelectAll(showFields, showProperties);
                    Generate();
                }

                if (GUILayout.Button("Select None", _selectAllNoneButtonStyle))
                {
                    SelectNone(showFields, showProperties);
                    Generate();
                }

                EditorGUILayout.EndHorizontal();
            }

            for (int i = 0; i < _fields.Length; i++)
            {
                var field = _fields[i];
                if ((field.isProperty && !showProperties) || ((!field.isProperty) && !showFields))
                    continue;

                EditorGUILayout.BeginHorizontal();

                var content = new GUIContent(field.Name);

                if (typeof(UnityEngine.Object).IsAssignableFrom(field.MemberType))
                    content.tooltip = field.MemberType.ToString() + "\nSaved by reference";
                else
                    content.tooltip = field.MemberType.ToString() + "\nSaved by value";


                bool selected = EditorGUILayout.ToggleLeft(content, _fieldSelected[i]);
                if (selected != _fieldSelected[i])
                {
                    _fieldSelected[i] = selected;
                    _unsavedChanges = true;
                }

                EditorGUILayout.EndHorizontal();
            }
        }

        // Selects all fields, properties or both.
        private void SelectAll(bool selectFields, bool selectProperties)
        {
            for (int i = 0; i < _fieldSelected.Length; i++)
                if ((_fields[i].isProperty && selectProperties) || (!_fields[i].isProperty) && selectFields)
                    _fieldSelected[i] = true;
        }

        // Selects all fields, properties or both.
        private void SelectNone(bool selectFields, bool selectProperties)
        {
            for (int i = 0; i < _fieldSelected.Length; i++)
                if ((_fields[i].isProperty && selectProperties) || (!_fields[i].isProperty) && selectFields)
                    _fieldSelected[i] = false;
        }

        public override void OnLostFocus()
        {
            if (_unsavedChanges)
                Generate();
        }

        private void TypeButton(int i)
        {
            var type = _types[i];
            if (!_types[i].showInList)
                return;

            if (type.hasExplicitCustomType)
                EditorGUILayout.BeginHorizontal();


            var thisTypeButtonStyle = (i == _selectedType) ? _selectedTypeButtonStyle : _typeButtonStyle;

            if (GUILayout.Button(new GUIContent(type.name, type.namespaceName), thisTypeButtonStyle))
                SelectType(i);

            // Set the cursor.
            var buttonRect = GUILayoutUtility.GetLastRect();
            EditorGUIUtility.AddCursorRect(buttonRect, MouseCursor.Link);


            if (type.hasExplicitCustomType)
            {
                GUILayout.Box(new GUIContent(EditorResources.CircleCheckmark, "Type is explicitly supported"), EditorStyles.largeLabel);
                EditorGUILayout.EndHorizontal();
            }
        }

        private void PerformSearch(string query)
        {
            var lowerCaseQuery = query.ToLowerInvariant();
            var emptyQuery = string.IsNullOrEmpty(query);

            for (int i = 0; i < _types.Length; i++)
                _types[i].showInList = (emptyQuery || _types[i].lowercaseName.Contains(lowerCaseQuery));
        }

        public void SelectType(Type type)
        {
            Init();
            for (int i = 0; i < _types.Length; i++)
                if (_types[i].type == type)
                    SelectType(i);
        }

        private void SelectType(int typeIndex)
        {
            _selectedType = typeIndex;

            if (_selectedType == -1)
            {
                SaveType("TypesWindowSelectedType", -1);
                return;
            }

            SaveType("TypesWindowSelectedType", _selectedType);

            if (!_recentTypes.Contains(typeIndex))
            {
                // If our recent type queue is full, remove an item before adding another.
                if (_recentTypes.Count == RECENT_TYPE_COUNT)
                    _recentTypes.RemoveAt(0);
                _recentTypes.Add(typeIndex);
                for (int j = 0; j < _recentTypes.Count; j++)
                    SaveType("TypesWindowRecentType" + j, _recentTypes[j]);
            }

            var type = _types[_selectedType].type;

            _fields = Reflection.GetSerializableMembers(type, false);
            _fieldSelected = new bool[_fields.Length];

            var customType = TypeManager.GetCustomType(type);
            // If there's no CustomType for this, only select fields which are supported by reflection.
            if (customType == null)
            {
                var safeFields = Reflection.GetSerializableMembers(type, true);
                for (int i = 0; i < _fields.Length; i++)
                    _fieldSelected[i] = safeFields.Any(item => item.Name == _fields[i].Name);
                return;
            }

            // Get fields and whether they're selected.
            var selectedFields = new List<string>();
            var propertyAttributes = customType.GetType().GetCustomAttributes(typeof(Properties), false);
            if (propertyAttributes.Length > 0)
                selectedFields.AddRange(((Properties) propertyAttributes[0]).members);

            _fieldSelected = new bool[_fields.Length];

            for (int i = 0; i < _fields.Length; i++)
                _fieldSelected[i] = selectedFields.Contains(_fields[i].Name);
        }

        private void SaveType(string key, int typeIndex)
        {
            if (typeIndex == -1)
                return;
            SaveType(key, _types[typeIndex].type);
        }

        private void SaveType(string key, Type type) { EditorPrefs.SetString(key, type.AssemblyQualifiedName); }

        private int LoadTypeIndex(string key)
        {
            string selectedTypeName = EditorPrefs.GetString(key, "");
            if (selectedTypeName != "")
            {
                var type = Reflection.GetType(selectedTypeName);
                if (type != null)
                {
                    int typeIndex = GetTypeIndex(type);
                    if (typeIndex != -1)
                        return typeIndex;
                }
            }

            return -1;
        }

        private int GetTypeIndex(Type type)
        {
            for (int i = 0; i < _types.Length; i++)
                if (_types[i].type == type)
                    return i;
            return -1;
        }

        private void Init()
        {
            // Init Type List
            var tempTypes = new List<TypeListItem>();
            
            var assemblies = AppDomain.CurrentDomain.GetAssemblies()
                    .Where(assembly => !assembly.FullName.Contains("Editor") && !assembly.FullName.Contains("UIElements") &&
                                       !assembly.FullName.Contains("AndroidPlayerBuildProgram") && !assembly.FullName.Contains("NiceIO") &&
                                       !assembly.FullName.Contains("nunit.framework") &&  
                                       !assembly.FullName.Contains("PlayerBuildProgramLibrary") &&
                                       !assembly.FullName.Contains("Mono.Security") &&
                                       !assembly.FullName.Contains("System") &&
                                       !assembly.FullName.Contains("ExCSS") &&
                                       !assembly.FullName.Contains("mscorlib") &&
                                       !assembly.FullName.Contains("netstandard") &&
                                       !assembly.FullName.Contains("savedata.editor") &&
                                       !assembly.FullName.Contains("Bee.BeeDriver") &&
                                       !assembly.FullName.Contains("UnityEngine") &&
                                       !assembly.FullName.Contains("Unity.Android") &&
                                       !assembly.FullName.Contains("Unity.Cecil") &&
                                       assembly.FullName != "Archive" && !assembly.FullName.Contains("Archive"))
                .OrderBy(assembly => assembly.GetName().Name)
                .ToArray();

            foreach (var assembly in assemblies)
            {
                var assemblyTypes = assembly.GetTypes();

                for (int i = 0; i < assemblyTypes.Length; i++)
                {
                    var type = assemblyTypes[i];
                    if (type.IsGenericType || type.IsEnum || type.IsNotPublic || type.IsAbstract || type.IsInterface || type.IsSubclassOf(typeof(UnityEngine.Component)))
                        continue;

                    var typeName = type.Name;
                    if (typeName[0] == '$' || typeName[0] == '_' || typeName[0] == '<')
                        continue;

                    var typeNamespace = type.Namespace;
                    var namespaceName = typeNamespace == null ? "" : typeNamespace.ToString();

                    tempTypes.Add(new TypeListItem(type.Name,
                        namespaceName,
                        type,
                        true,
                        HasExplicitCustomType(type)));
                }
            }

            _types = tempTypes.OrderBy(type => type.name).ToArray();

            // Load types and recent types.
            if (Event.current.type == EventType.Layout)
            {
                _recentTypes = new List<int>();
                for (int i = 0; i < RECENT_TYPE_COUNT; i++)
                {
                    int typeIndex = LoadTypeIndex("TypesWindowRecentType" + i);
                    if (typeIndex != -1)
                        _recentTypes.Add(typeIndex);
                }

                SelectType(LoadTypeIndex("TypesWindowSelectedType"));
            }

            PerformSearch(_searchFieldValue);

            // Init Styles.
            _searchBarCancelButtonStyle = new GUIStyle(EditorStyles.miniButton);
            var cancelButtonSize = EditorStyles.miniTextField.CalcHeight(new GUIContent(""), 20);
            _searchBarCancelButtonStyle.fixedWidth = cancelButtonSize;
            _searchBarCancelButtonStyle.fixedHeight = cancelButtonSize;
            _searchBarCancelButtonStyle.fontSize = 8;
            _searchBarCancelButtonStyle.padding = new RectOffset();
            _searchBarStyle = new GUIStyle(EditorStyles.toolbarTextField);
            _searchBarStyle.stretchWidth = true;

            _typeButtonStyle = new GUIStyle(EditorStyles.largeLabel);
            _typeButtonStyle.alignment = TextAnchor.MiddleLeft;
            _typeButtonStyle.stretchWidth = false;
            _selectedTypeButtonStyle = new GUIStyle(_typeButtonStyle);
            _selectedTypeButtonStyle.fontStyle = FontStyle.Bold;

            _leftPaneStyle = new GUIStyle();
            _leftPaneStyle.fixedWidth = _leftPaneWidth;
            _leftPaneStyle.clipping = TextClipping.Clip;
            _leftPaneStyle.padding = new RectOffset(10, 10, 10, 10);

            _selectAllNoneButtonStyle = new GUIStyle(EditorStyles.miniButton);
            _selectAllNoneButtonStyle.stretchWidth = false;
            _selectAllNoneButtonStyle.margin = new RectOffset(0, 0, 0, 10);
        }

        private void Generate()
        {
            var type = _types[_selectedType].type;
            if (type == null)
            {
                EditorUtility.DisplayDialog("Type not selected", "Type not selected. Please ensure you select a type", "Ok");
                return;
            }

            _unsavedChanges = false;

            // Get the serializable fields of this class.
            //var fields = Reflection.GetSerializableCustomFields(type);

            // The string that we suffix to the class name. i.e. UnityEngine_UnityEngine_Transform.
            string typeSuffix = type.Name;
            // The string for the full C#-safe type name. This name must be suitable for going inside typeof().
            string fullType = GetFullTypeName(type);
            // The list of WriteProperty calls to write the properties of this type.
            string writes = GenerateWrites();
            // The list of case statements and Read calls to read the properties of this type.
            string reads = GenerateReads();
            // A comma-seperated string of fields we've supported in this type.
            string propertyNames = "";

            bool first = true;
            for (int i = 0; i < _fields.Length; i++)
            {
                if (!_fieldSelected[i])
                    continue;

                if (first)
                    first = false;
                else
                    propertyNames += ", ";
                propertyNames += "\"" + _fields[i].Name + "\"";
            }

            // Insert the relevant strings into the template.
            string template;
            if (Reflection.IsValueType(type)) template = EditorResources.ValueTypeTemplate.text;
            else template = EditorResources.ClassTypeTemplate.text;
            template = template.Replace("[typeSuffix]", typeSuffix);
            template = template.Replace("[fullType]", fullType);
            template = template.Replace("[writes]", writes);
            template = template.Replace("[reads]", reads);
            template = template.Replace("[propertyNames]", propertyNames);

            // Create the output file.

            string outputFilePath = GetOutputPath(type);
            var fileInfo = new FileInfo(outputFilePath);
            fileInfo.Directory.Create();
            File.WriteAllText(outputFilePath, template);
            AssetDatabase.Refresh();
        }

        private string GenerateWrites()
        {
            var type = _types[_selectedType].type;
            bool isComponent = typeof(Component).IsAssignableFrom(type);
            string writes = "";

            for (int i = 0; i < _fields.Length; i++)
            {
                var field = _fields[i];
                var selected = _fieldSelected[i];
                var customType = TypeManager.GetCustomType(field.MemberType);

                if (!selected || isComponent && (field.Name == Reflection.COMPONENT_TAG_FIELD_NAME || field.Name == Reflection.COMPONENT_NAME_FIELD_NAME))
                    continue;

                string writeByRef = Reflection.IsAssignableFrom(typeof(UnityEngine.Object), field.MemberType) ? "ByRef" : "";
                string typeParam = HasExplicitCustomType(customType) && writeByRef == ""
                    ? ", " + customType.GetType().Name + ".Instance"
                    : (writeByRef == "" ? ", TypeManager.GetOrCreateCustomType(typeof(" + GetFullTypeName(field.MemberType) + "))" : "");
                // If this is static, access the field through the class name rather than through an Instance.
                string instance = (field.IsStatic) ? GetFullTypeName(type) : "Instance";

                if (!field.IsPublic)
                {
                    string memberType = field.isProperty ? "Property" : "Field";
                    writes += String.Format("\r\n\t\t\twriter.WritePrivate{2}{1}(\"{0}\", Instance);", field.Name, writeByRef, memberType);
                }
                else
                    writes += String.Format("\r\n\t\t\twriter.WriteProperty{1}(\"{0}\", {3}.{0}{2});",
                        field.Name,
                        writeByRef,
                        typeParam,
                        instance);
            }

            return writes;
        }

        private string GenerateReads()
        {
            var type = _types[_selectedType].type;
            bool isComponent = typeof(Component).IsAssignableFrom(type);
            string reads = "";

            for (int i = 0; i < _fields.Length; i++)
            {
                var field = _fields[i];
                var selected = _fieldSelected[i];

                if (!selected || isComponent && (field.Name == "tag" || field.Name == "name"))
                    continue;

                string fieldTypeName = GetFullTypeName(field.MemberType);
                string typeParam = HasExplicitCustomType(field.MemberType) ? TypeManager.GetCustomType(field.MemberType).GetType().Name + ".Instance" : "";
                // If this is static, access the field through the class name rather than through an Instance.
                string instance = (field.IsStatic) ? GetFullTypeName(type) : "Instance";

                // If we're writing a private field or property, we need to write it using a different method.
                if (!field.IsPublic)
                {
                    typeParam = ", " + typeParam;
                    if (field.isProperty)
                        reads += String.Format(
                            "\r\n\t\t\t\t\tcase \"{0}\":\r\n\t\t\t\t\treader.SetPrivateProperty(\"{0}\", reader.Read<{1}>(), Instance);\r\n\t\t\t\t\tbreak;",
                            field.Name,
                            fieldTypeName);
                    else
                        reads += String.Format(
                            "\r\n\t\t\t\t\tcase \"{0}\":\r\n\t\t\t\t\treader.SetPrivateField(\"{0}\", reader.Read<{1}>(), Instance);\r\n\t\t\t\t\tbreak;",
                            field.Name,
                            fieldTypeName);
                }
                else
                    reads += String.Format("\r\n\t\t\t\t\tcase \"{0}\":\r\n\t\t\t\t\t\t{3}.{0} = reader.Read<{1}>({2});\r\n\t\t\t\t\t\tbreak;",
                        field.Name,
                        fieldTypeName,
                        typeParam,
                        instance);
            }

            return reads;
        }

        private string GetOutputPath(Type type) { return Application.dataPath + "/_Root/Scripts/CustomType/UserType_" + type.Name + ".cs"; }

        /* Gets the full Type name, replacing any syntax (such as '+') with a dot to make it a valid type name */
        private static string GetFullTypeName(Type type)
        {
            string typeName = type.ToString();

            typeName = typeName.Replace('+', '.');

            // If it's a generic type, replace syntax with angled brackets.
            int genericArgumentCount = type.GetGenericArguments().Length;
            if (genericArgumentCount > 0)
            {
                return string.Format("{0}<{1}>", type.ToString().Split('`')[0], string.Join(", ", type.GetGenericArguments().Select(x => GetFullTypeName(x)).ToArray()));
            }

            return typeName;
        }

        /* Whether this type has an explicit CustomType. For example, CustomArrayType would return false, but CustomVector3ArrayType would return true */
        private static bool HasExplicitCustomType(Type type)
        {
            var customType = TypeManager.GetCustomType(type);
            if (customType == null)
                return false;
            // If this CustomType has a static Instance property, return true.
            if (customType.GetType().GetField("Instance", BindingFlags.Public | BindingFlags.Static) != null)
                return true;
            return false;
        }

        private static bool HasExplicitCustomType(CustomType customType)
        {
            if (customType == null)
                return false;
            // If this CustomType has a static Instance property, return true.
            if (customType.GetType().GetField("Instance", BindingFlags.Public | BindingFlags.Static) != null)
                return true;
            return false;
        }

        public class TypeListItem
        {
            public string name;
            public string lowercaseName;
            public string namespaceName;
            public Type type;
            public bool showInList;
            public bool hasExplicitCustomType;

            public TypeListItem(string name, string namespaceName, Type type, bool showInList, bool hasExplicitCustomType)
            {
                this.name = name;
                this.lowercaseName = name.ToLowerInvariant();
                this.namespaceName = namespaceName;
                this.type = type;
                this.showInList = showInList;
                this.hasExplicitCustomType = hasExplicitCustomType;
            }
        }
    }
}