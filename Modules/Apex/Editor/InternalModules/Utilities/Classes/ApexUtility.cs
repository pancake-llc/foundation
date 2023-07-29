using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor.Compilation;
using UnityEditor;

namespace Pancake.ApexEditor
{
    /// <summary>
    /// Miscellaneous helper stuff for Apex.
    /// </summary>
    public static class ApexUtility
    {
        private static bool _Enabled = true;
        private static bool _Master = true;
        internal static readonly HashSet<ExceptType> ExceptTypes = new HashSet<ExceptType>();

        /// <summary>
        /// editor is enabled.
        /// </summary>
        public static bool Enabled
        {
            get
            {
                return _Enabled;
            }
            internal set
            {
                if (_Master && _Enabled && !value)
                {
                    RequestScriptCompilation(value);
                }
                _Enabled = value;
            }
        }
        
        /// <summary>
        /// Make Apex editor as master editor, regardless of the user custom editors with default Unity types.
        /// </summary>
        public static bool Master
        {
            get
            {
                return _Master;
            }
            internal set
            {
                if (_Enabled)
                {
                    RequestScriptCompilation(value);
                }
                _Master = value;
            }
        }
        
        
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