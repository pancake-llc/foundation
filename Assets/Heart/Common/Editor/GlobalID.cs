using UnityEditor;
using UnityEngine;

namespace PancakeEditor.Common
{
    [System.Serializable]
    public struct GlobalID : System.IEquatable<GlobalID>
    {
        public Object GetObject() => GlobalObjectId.GlobalObjectIdentifierToObjectSlow(GlobalObjectId);
        public int GetObjectInstanceId() => GlobalObjectId.GlobalObjectIdentifierToInstanceIDSlow(GlobalObjectId);

        public string Guid => GlobalObjectId.assetGUID.ToString();
        public ulong FileId => GlobalObjectId.targetObjectId;

        public bool IsNull => GlobalObjectId.identifierType == 0;
        public bool IsAsset => GlobalObjectId.identifierType == 1;
        public bool IsSceneObject => GlobalObjectId.identifierType == 2;

        public GlobalObjectId GlobalObjectId =>
            globalObjectId.Equals(default) && globalObjectIdString != null && GlobalObjectId.TryParse(globalObjectIdString, out var r)
                ? globalObjectId = r
                : globalObjectId;

        public GlobalObjectId globalObjectId;

        public GlobalID(Object o) => globalObjectIdString = (globalObjectId = GlobalObjectId.GetGlobalObjectIdSlow(o)).ToString();
        public GlobalID(string s) => globalObjectIdString = GlobalObjectId.TryParse(s, out globalObjectId) ? globalObjectId.ToString() : s;

        public string globalObjectIdString;


        public bool Equals(GlobalID other) => globalObjectIdString.Equals(other.globalObjectIdString);

        public static bool operator ==(GlobalID a, GlobalID b) => a.Equals(b);
        public static bool operator !=(GlobalID a, GlobalID b) => !a.Equals(b);

        public override bool Equals(object other) => other is GlobalID otherglobalID && Equals(otherglobalID);
        public override int GetHashCode() => globalObjectIdString == null ? 0 : globalObjectIdString.GetHashCode();

        public override string ToString() => globalObjectIdString;
    }
}