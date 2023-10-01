namespace Pancake.UI.Popup
{
    public interface ISheetContainerCallbackReceiver
    {
        void BeforeShow(Sheet enterSheet, Sheet exitSheet);

        void AfterShow(Sheet enterSheet, Sheet exitSheet);

        void BeforeHide(Sheet exitSheet);

        void AfterHide(Sheet exitSheet);
    }
}