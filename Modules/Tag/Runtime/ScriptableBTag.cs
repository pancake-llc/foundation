using System;
using UnityEngine;

namespace Pancake.BTag
{
    public class ScriptableBTag : ScriptableObject
    {
        // A ReadOnly unique hash created once for the asset
        [SerializeField, HideInInspector] private BHash128 hash = GenerateDefaultGuid();
        public BHash128 Hash => hash;

        public bool IsDefault => !hash.IsValid;
        public string ShortName => this == null ? "null" : name.Contains("/") ? name.Substring(name.LastIndexOf("/", StringComparison.Ordinal) + 1) : name;


        // The following property has been added for detecting Duplication of an asset
        // through Unity Editor. When this happens, the hash will no longer be unique.
        public void EditorGenerateNewHash() => hash = GenerateDefaultGuid();

        // Updating a runtime asset's hash is sometimes required - in general this method should not be required
        public void ManuallySetHash(BHash128 hash) => this.hash = hash;

        // Can implicitly convert to a Hash
        public static implicit operator BHash128(ScriptableBTag so) { return so == null ? default : so.Hash; }

        static unsafe BHash128 GenerateDefaultGuid()
        {
            var guid = System.Guid.NewGuid();
            var hash = *(BHash128*) &guid;
            return hash;
        }
    }
}