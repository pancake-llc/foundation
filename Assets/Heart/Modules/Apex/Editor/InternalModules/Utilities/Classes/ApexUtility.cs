using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor.Compilation;
using UnityEditor;
using System.Text.RegularExpressions;
using System.Reflection;

namespace Pancake.ApexEditor
{
    /// <summary>
    /// Miscellaneous helper stuff for Apex.
    /// </summary>
    public static class ApexUtility
    {
        private static bool IsEnabled;
        private static bool IsMasterMode;
        private static bool AllAssemblyScope;
        private static bool DefaultAssemblyScope;

        private static readonly string[] DefaultAssemblies;

        internal static readonly HashSet<AssemblyScope> AssemblyScopes;
        internal static readonly HashSet<ExceptType> ExceptTypes;

        static ApexUtility()
        {
            IsEnabled = true;
            IsMasterMode = true;
            AllAssemblyScope = true;
            DefaultAssemblyScope = true;

            DefaultAssemblies = new[] {"Apex", "ApexEditor"};

            AssemblyScopes = new HashSet<AssemblyScope>();
            ExceptTypes = new HashSet<ExceptType>();
        }

        /// <summary>
        /// Apex editor is enabled.
        /// </summary>
        public static bool Enabled
        {
            get { return IsEnabled; }
            internal set
            {
                if (IsMasterMode && IsEnabled && !value)
                {
                    RequestScriptCompilation(value);
                }

                IsEnabled = value;
            }
        }

        /// <summary>
        /// Make Apex editor as master editor, regardless of the user custom editors with default Unity types.
        /// </summary>
        public static bool Master
        {
            get { return IsMasterMode; }
            internal set
            {
                if (IsEnabled)
                {
                    RequestScriptCompilation(value);
                }

                IsMasterMode = value;
            }
        }

        /// <summary>
        /// Include all assemblies in AssemblyScope.
        /// </summary>
        public static bool IncludeAllAssemblies { get { return AllAssemblyScope; } internal set { AllAssemblyScope = value; } }

        /// <summary>
        /// Include Unity pre-defined default assemblies in AssemblyScope.
        /// </summary>
        public static bool IncludeDefaultAssemblies { get { return DefaultAssemblyScope; } internal set { DefaultAssemblyScope = value; } }

        /// <summary>
        /// Check if specified type is added as excepted type.
        /// </summary>
        /// <param name="type">Type to check.</param>
        /// <returns>True if type excepted, otherwise false.</returns>
        public static bool IsExceptType(Type type)
        {
            Func<ExceptType, bool> predicate = (exceptType) =>
            {
                Type _type = type;
                string name = exceptType.GetName();
                bool subClasses = exceptType.SubClasses();
                do
                {
                    if (_type.Name == name)
                    {
                        return true;
                    }

                    _type = _type.BaseType;
                } while (_type != null && subClasses);

                return false;
            };

            if (ExceptTypes.Count > 0)
            {
                return ExceptTypes.Any(predicate);
            }

            return false;
        }

        /// <summary>
        /// Check if specified assembly contains in one of assembly scope.
        /// </summary>
        /// <param name="assembly">System reflection assembly.</param>
        /// <returns>True if assembly contains in the assembly scope and version is match, otherwise false.</returns>
        public static bool InAssemblyScope(System.Reflection.Assembly assembly)
        {
            if (AllAssemblyScope)
            {
                return true;
            }

            AssemblyName assemblyName = assembly.GetName();

            const string DEFAULT_RUNTIME_ASSEMBLY_NAME = "Assembly-CSharp";
            const string DEFAULT_EDITOR_ASSEMBLY_NAME = "Assembly-CSharp-Editor";
            if (DefaultAssemblyScope && (assemblyName.Name == DEFAULT_RUNTIME_ASSEMBLY_NAME || assemblyName.Name == DEFAULT_EDITOR_ASSEMBLY_NAME))
            {
                return true;
            }

            for (int i = 0; i < DefaultAssemblies.Length; i++)
            {
                if (DefaultAssemblies[i] == assemblyName.Name)
                {
                    return true;
                }
            }

            const string PATTERN = @"^x\s*(==|>|<|>=|<=)\s*([\d.]+)\s*$";
            Regex regex = new Regex(PATTERN);

            foreach (AssemblyScope scope in AssemblyScopes)
            {
                if (scope.GetName() == assemblyName.Name)
                {
                    Match match = regex.Match(scope.GetCondition());
                    if (match.Success)
                    {
                        string opr = match.Groups[1].Value;
                        Version version = new Version(match.Groups[2].Value);
                        switch (opr)
                        {
                            case "==":
                                return assemblyName.Version == version;
                            case ">":
                                return assemblyName.Version > version;
                            case "<":
                                return assemblyName.Version < version;
                            case ">=":
                                return assemblyName.Version >= version;
                            case "<=":
                                return assemblyName.Version <= version;
                        }
                    }
                    else
                    {
                        return true;
                    }

                    break;
                }
            }

            return false;
        }

        private static void RequestScriptCompilation(bool value)
        {
            const string EDITOR_RECOMPILED_GUID = "ApexInternal.EditorRecompiled";
            if (value != SessionState.GetBool(EDITOR_RECOMPILED_GUID, value))
            {
                Selection.activeObject = null;
                CompilationPipeline.RequestScriptCompilation();
            }

            SessionState.SetBool(EDITOR_RECOMPILED_GUID, value);
        }
    }
}