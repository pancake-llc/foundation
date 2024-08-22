using System;

namespace Pancake.UI
{
    internal static class PopupBackdropHandlerFactory
    {
        public static IPopupBackdropHandler Create(EPopupBackdropStrategy strategy, PopupBackdrop prefab)
        {
            return strategy switch
            {
                EPopupBackdropStrategy.GeneratePerPopup => new GeneratePerPopupPopupBackdropHandler(prefab),
                EPopupBackdropStrategy.OnlyFirstBackdrop => new OnlyFirstBackdropPopupBackdropHandler(prefab),
                EPopupBackdropStrategy.ChangeOrderBeforeAnimation => new ChangeOrderPopupBackdropHandler(prefab,
                    ChangeOrderPopupBackdropHandler.ChangeTiming.BeforeAnimation),
                EPopupBackdropStrategy.ChangeOrderAfterAnimation => new ChangeOrderPopupBackdropHandler(prefab,
                    ChangeOrderPopupBackdropHandler.ChangeTiming.AfterAnimation),
                _ => throw new ArgumentOutOfRangeException(nameof(strategy), strategy, null)
            };
        }
    }
}