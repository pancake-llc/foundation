#if UNITY_EDITOR
namespace Sisus.ComponentNames.Editor
{
    internal enum FailModifyPrefabReason
    {
        None = 0,
#if !UNITY_2022_3_OR_NEWER
		PartOfPrefabInstance,
#endif
        MissingComponent,
        MultipleComponentsOfSameType,
        MultipleGameObjectsWithSameName,
        SaveAsPrefabAssetFailed
    }
}
#endif