namespace Pancake.UI
{
    public interface IPopupContainerCallbackReceiver
    {
        void BeforePush(Popup enter, Popup exit);

        void AfterPush(Popup enter, Popup exit);

        void BeforePop(Popup enter, Popup exit);

        void AfterPop(Popup enter, Popup exit);
    }
}