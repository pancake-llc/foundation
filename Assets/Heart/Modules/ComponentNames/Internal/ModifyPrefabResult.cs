#if UNITY_EDITOR
using UnityEngine;

namespace Sisus.ComponentNames.Editor
{
    internal readonly struct ModifyPrefabResult
    {
        public static readonly ModifyPrefabResult Success = new(FailModifyPrefabReason.None, null, null);

        public FailModifyPrefabReason FailReason { get; }
        public string PrefabPath { get; }
        public string Context { get; }
        public bool IsSuccess => FailReason is FailModifyPrefabReason.None;

        public ModifyPrefabResult(FailModifyPrefabReason failReason, string prefabPath, string context)
        {
            FailReason = failReason;
            PrefabPath = prefabPath;
            Context = context;
        }
    }
}
#endif