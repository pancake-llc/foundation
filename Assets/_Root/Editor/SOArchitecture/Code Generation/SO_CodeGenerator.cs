using System.IO;
using System.Linq;
using Pancake.SOA;
using UnityEngine;
// ReSharper disable InconsistentNaming

namespace Pancake.Editor.SOA
{
    public static class SO_CodeGenerator
    {
        static SO_CodeGenerator()
        {
            CreateTargetDirectories();
        }
        private static void CreateTargetDirectories()
        {
            _targetDirectories = new string[TYPE_COUNT]
            {
                Application.dataPath + "/_Root/Scripts/CODE_GENERATION/Events/Listeners",
                Application.dataPath + "/_Root/Scripts/CODE_GENERATION/Events/Game Events",
                Application.dataPath + "/_Root/Scripts/CODE_GENERATION/References",
                Application.dataPath + "/_Root/Scripts/CODE_GENERATION/Collections",
                Application.dataPath + "/_Root/Scripts/CODE_GENERATION/Events/Responses",
                Application.dataPath + "/_Root/Scripts/CODE_GENERATION/Variables",
            };
        }

        public const int TYPE_COUNT = 6;

        public struct Data
        {
            public bool[] types;
            public string typeName;
            public string menuName;
            public string @namespace;
            public int order;
        }

        private static string[] _templateNames = new string[TYPE_COUNT]
        {
            "GameEventListenerTemplate",
            "GameEventTemplate",
            "ReferenceTemplate",
            "CollectionTemplate",
            "UnityEventTemplate",
            "VariableTemplate",
        };

        private static readonly string[] _targetFileNames = new string[TYPE_COUNT]
        {
            "{0}GameEventListener.cs",
            "{0}GameEvent.cs",
            "{0}Reference.cs",
            "{0}Collection.cs",
            "{0}UnityEvent.cs",
            "{0}Variable.cs",
        };

        private static string[] _targetDirectories = null;
        private static string[,] _replacementStrings = null;

        private static string TypeName { get { return _replacementStrings[1, 1]; } }

        public static void Generate(Data data, bool codeGenerationAllowOverwrite)
        {
            _replacementStrings = new string[5, 2]
            {
            { "$TYPE$", data.typeName },
            { "$TYPE_NAME$", CapitalizeFirstLetter(data.typeName) },
            { "$MENU_NAME$", data.menuName },
            { "$ORDER$", data.order.ToString() },
            { "$NAMESPACE$", data.@namespace}
            };

            for (int i = 0; i < TYPE_COUNT; i++)
            {
                if (data.types[i])
                {
                    GenerateScript(i, codeGenerationAllowOverwrite);
                }
            }
        }
        private static void GenerateScript(int index, bool codeGenerationAllowOverwrite)
        {
            string targetFilePath = GetTargetFilePath(index);
            string contents = GetScriptContents(index);

            if (File.Exists(targetFilePath) && !codeGenerationAllowOverwrite)
            {
                Debug.Log("Cannot create file at " + targetFilePath + " because a file already exists, and overwrites are disabled");
                return;
            }

            Debug.Log("Creating " + targetFilePath);

            Directory.CreateDirectory(Path.GetDirectoryName(targetFilePath));
            File.WriteAllText(targetFilePath, contents);
        }
        private static string GetScriptContents(int index)
        {
            string templateContent = "";
            switch (index)
            {
                case 0:
                    templateContent = EditorResources.GameEventListenerTemplate.text;
                    break;
                case 1:
                    templateContent = EditorResources.GameEventTemplate.text;
                    break;
                case 2:
                    templateContent = EditorResources.ReferenceTemplate.text;
                    break;
                case 3:
                    templateContent = EditorResources.CollectionTemlate.text;
                    break;
                case 4:
                    templateContent = EditorResources.UnityEventTemplate.text;
                    break;    
                case 5:
                    templateContent = EditorResources.VariableTemplate.text;
                    break;
            }

            string output = templateContent;

            for (int i = 0; i < _replacementStrings.GetLength(0); i++)
            {
                output = output.Replace(_replacementStrings[i, 0], _replacementStrings[i, 1]);
            }

            return output;
        }
        private static string GetTargetFilePath(int index)
        {
            return _targetDirectories[index] + "/" + string.Format(_targetFileNames[index], TypeName);
        }
        private static string CapitalizeFirstLetter(string input)
        {
            return input.First().ToString().ToUpper() + input.Substring(1);
        }
    }
}