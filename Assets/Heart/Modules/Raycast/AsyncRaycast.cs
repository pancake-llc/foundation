namespace Pancake.Physics
{
    public struct AsyncRaycast
    {
        public RaycastTicket ticket;
        public object context;
        public System.Action<AsyncRaycastCallbackResult> onComplete;
    }
}