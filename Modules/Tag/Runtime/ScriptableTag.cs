using System;
using UnityEngine;

namespace Pancake.Tag
{
    public class ScriptableTag : ScriptableObject
    {
        // A ReadOnly unique hash created once for the asset
        [SerializeField, HideInInspector] private Hash128 hash = GenerateDefaultGuid();
        public Hash128 Hash => hash;

        public bool IsDefault => !hash.IsValid;
        public string ShortName => this == null ? "null" : name.Contains("/") ? name.Substring(name.LastIndexOf("/", StringComparison.Ordinal) + 1) : name;


        // The following property has been added for detecting Duplication of an asset
        // through Unity Editor. When this happens, the hash will no longer be unique.
        public void EditorGenerateNewHash() => hash = GenerateDefaultGuid();

        // Updating a runtime asset's hash is sometimes required - in general this method should not be required
        public void ManuallySetHash(Hash128 hash) => this.hash = hash;

        // Can implicitly convert to a Hash
        public static implicit operator Hash128(ScriptableTag so) { return so == null ? default : so.Hash; }

        private static unsafe Hash128 GenerateDefaultGuid()
        {
            var guid = System.Guid.NewGuid();
            var hash = *(Hash128*) &guid;
            return hash;
        }
    }
}