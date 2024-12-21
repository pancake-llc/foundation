namespace Sisus.Init
{
    public enum ServiceInitFailReason
    {
        None = 0,
        MissingSceneObject,
#if UNITY_ADDRESSABLES_1_17_4_OR_NEWER
        MissingAddressable,
#endif
        MissingResource,
        MissingComponent,
        AssetNotConvertible,
        CreatingServiceProviderFailed,
        ServiceProviderThrewException,
        ServiceProviderResultNull,
        InitializerThrewException,
        InitializerReturnedNull,
        WrapperReturnedNull,
        InvalidDefiningType,
        UnresolveableConcreteType,
        CircularDependencies,
        ExceptionWasThrown,
        ScriptableObjectWithFindFromScene,
        MissingDependency
    }
}