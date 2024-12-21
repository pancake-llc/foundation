#if UNITY_ADDRESSABLES_1_17_4_OR_NEWER
#endif

namespace Sisus.Init.Internal
{
    public enum InitFailReason
    {
        Success = 0,
        MissingSceneObject,
#if UNITY_ADDRESSABLES_1_17_4_OR_NEWER
        MissingAddressable,
#endif
        MissingResource,
        MissingComponent,
        AssetNotConvertible,
        CreatingServiceInitializerFailed,
        ServiceInitializerThrewException,
        ServiceInitializerReturnedNull,
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

    // internal class ServiceTagContext : InitContext
    // {
    //
    // }
    //
    // public class InitContext
    // {
    // 	public GlobalServiceInfo Info;
    // 	public virtual GameObject Container { get; }
    // }
}