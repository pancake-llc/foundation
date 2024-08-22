namespace Pancake.PlayerLoop
{
    public interface IInitialize
    {
        void OnInitialize();
    }

    public interface IPostInitialize
    {
        void OnPostInitialize();
    }

    public interface IStart
    {
        void OnStartup();
    }

    public interface IPostStart
    {
        void OnPostStartup();
    }

    public interface IFixedUpdate
    {
        void OnFixedUpdate();
    }

    public interface IPostFixedUpdate
    {
        void OnPostFixedUpdate();
    }

    public interface IUpdate
    {
        void OnUpdate();
    }

    public interface IPostUpdate
    {
        void OnPostUpdate();
    }

    public interface ILateUpdate
    {
        void OnLateUpdate();
    }

    public interface IPostLateUpdate
    {
        void OnPostLateUpdate();
    }
}