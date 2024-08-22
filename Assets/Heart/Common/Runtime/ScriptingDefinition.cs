#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Text;
using UnityEditor;
using UnityEditor.Build;

namespace Pancake.Common
{
    public static class ScriptingDefinition
    {
        private static BuildTargetGroup[] workingBuildTargetGroups;

        /// <summary>
        /// Gets all supported build target groups, excluding the <see cref="BuildTargetGroup.Unknown"/>
        /// and the obsolete ones.
        /// </summary>
        /// <returns>The working build target groups.</returns>
        private static BuildTargetGroup[] GetWorkingBuildTargetGroups()
        {
            if (workingBuildTargetGroups != null)
                return workingBuildTargetGroups;

            var groups = new List<BuildTargetGroup>();
            var btgType = typeof(BuildTargetGroup);

            foreach (string name in Enum.GetNames(btgType))
            {
                // First check obsolete.
                var memberInfo = btgType.GetMember(name)[0];
                if (Attribute.IsDefined(memberInfo, typeof(ObsoleteAttribute))) continue;

                // Name -> enum value and exclude the 'Unknown'.
                var g = (BuildTargetGroup) Enum.Parse(btgType, name);
                if (g != BuildTargetGroup.Unknown) groups.Add(g);
            }

            workingBuildTargetGroups = groups.ToArray();
            return workingBuildTargetGroups;
        }

        private static bool IsSymbolDefined(string symbol, BuildTargetGroup platform)
        {
            string currentSymbol = PlayerSettings.GetScriptingDefineSymbols(NamedBuildTarget.FromBuildTargetGroup(platform));
            var symbols = new List<string>(currentSymbol.Split(';'));

            return symbols.Contains(symbol);
        }

        public static bool IsSymbolDefined(string symbol)
        {
            var platform = BuildPipeline.GetBuildTargetGroup(EditorUserBuildSettings.activeBuildTarget);
            return IsSymbolDefined(symbol, platform);
        }

        /// <summary>
        /// Adds the scripting define symbol to all platforms where it doesn't exist.
        /// </summary>
        /// <param name="symbol">Symbol.</param>
        public static void AddDefineSymbolOnAllPlatforms(string symbol)
        {
            foreach (var target in GetWorkingBuildTargetGroups())
            {
                AddDefineSymbol(symbol, target);
            }
        }

        /// <summary>
        /// Adds the scripting define symbols in the given array to all platforms where they don't exist. 
        /// </summary>
        /// <param name="symbols">Symbols.</param>
        public static void AddDefineSymbolsOnAllPlatforms(string[] symbols)
        {
            foreach (var target in GetWorkingBuildTargetGroups())
            {
                AddDefineSymbols(symbols, target);
            }
        }

        /// <summary>
        /// Adds the scripting define symbols in given array to the target platforms if they don't exist.
        /// </summary>
        /// <param name="symbols">Symbols.</param>
        /// <param name="platform">Platform.</param>
        public static void AddDefineSymbols(string[] symbols, BuildTargetGroup platform)
        {
            string currentSymbol = PlayerSettings.GetScriptingDefineSymbols(NamedBuildTarget.FromBuildTargetGroup(platform));
            var currentSymbols = new List<string>(currentSymbol.Split(';'));
            var added = 0;

            foreach (var symbol in symbols)
            {
                if (!currentSymbols.Contains(symbol))
                {
                    currentSymbols.Add(symbol);
                    added++;
                }
            }

            if (added > 0)
            {
                var sb = new StringBuilder();

                for (var i = 0; i < currentSymbols.Count; i++)
                {
                    sb.Append(currentSymbols[i]);
                    if (i < currentSymbols.Count - 1)
                        sb.Append(";");
                }

                PlayerSettings.SetScriptingDefineSymbols(NamedBuildTarget.FromBuildTargetGroup(platform), sb.ToString());
            }
        }

        /// <summary>
        /// Adds the scripting define symbols on the platform if it doesn't exist.
        /// </summary>
        /// <param name="symbol">Symbol.</param>
        /// <param name="platform"></param>
        public static void AddDefineSymbol(string symbol, BuildTargetGroup platform)
        {
            string currentSymbol = PlayerSettings.GetScriptingDefineSymbols(NamedBuildTarget.FromBuildTargetGroup(platform));
            var symbols = new List<string>(currentSymbol.Split(';'));

            if (!symbols.Contains(symbol))
            {
                symbols.Add(symbol);

                var sb = new StringBuilder();

                for (var i = 0; i < symbols.Count; i++)
                {
                    sb.Append(symbols[i]);
                    if (i < symbols.Count - 1)
                        sb.Append(";");
                }

                PlayerSettings.SetScriptingDefineSymbols(NamedBuildTarget.FromBuildTargetGroup(platform), sb.ToString());
            }
        }

        /// <summary>
        /// Removes the scripting define symbols in the given array on all platforms where they exist.
        /// </summary>
        /// <param name="symbols">Symbols.</param>
        public static void RemoveDefineSymbolsOnAllPlatforms(string[] symbols)
        {
            foreach (var target in GetWorkingBuildTargetGroups())
            {
                RemoveDefineSymbols(symbols, target);
            }
        }

        /// <summary>
        /// Removes the scripting define symbol on all platforms where it exists.
        /// </summary>
        /// <param name="symbol">Symbol.</param>
        public static void RemoveDefineSymbolOnAllPlatforms(string symbol)
        {
            foreach (var target in GetWorkingBuildTargetGroups())
            {
                RemoveDefineSymbol(symbol, target);
            }
        }

        /// <summary>
        /// Removes the scripting define symbols in the given array on the target platform if they exists.
        /// </summary>
        /// <param name="symbols">Symbols.</param>
        /// <param name="platform">Platform.</param>
        public static void RemoveDefineSymbols(string[] symbols, BuildTargetGroup platform)
        {
            string currentSymbol = PlayerSettings.GetScriptingDefineSymbols(NamedBuildTarget.FromBuildTargetGroup(platform));
            var currentSymbols = new List<string>(currentSymbol.Split(';'));
            var removed = 0;

            foreach (var symbol in symbols)
            {
                if (currentSymbols.Contains(symbol))
                {
                    currentSymbols.Remove(symbol);
                    removed++;
                }
            }

            if (removed > 0)
            {
                var sb = new StringBuilder();

                for (var i = 0; i < currentSymbols.Count; i++)
                {
                    sb.Append(currentSymbols[i]);
                    if (i < currentSymbols.Count - 1)
                        sb.Append(";");
                }

                PlayerSettings.SetScriptingDefineSymbols(NamedBuildTarget.FromBuildTargetGroup(platform), sb.ToString());
            }
        }

        /// <summary>
        /// Removes the scripting define symbol on the platform if it exists.
        /// </summary>
        /// <param name="symbol">Symbol.</param>
        /// <param name="platform">Platform.</param>
        public static void RemoveDefineSymbol(string symbol, BuildTargetGroup platform)
        {
            string currentSymbol = PlayerSettings.GetScriptingDefineSymbols(NamedBuildTarget.FromBuildTargetGroup(platform));
            var symbols = new List<string>(currentSymbol.Split(';'));

            if (symbols.Contains(symbol))
            {
                symbols.Remove(symbol);

                var sb = new StringBuilder();

                for (var i = 0; i < symbols.Count; i++)
                {
                    sb.Append(symbols[i]);
                    if (i < symbols.Count - 1)
                        sb.Append(";");
                }

                PlayerSettings.SetScriptingDefineSymbols(NamedBuildTarget.FromBuildTargetGroup(platform), sb.ToString());
            }
        }
    }
}
#endif