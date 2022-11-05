using System.Threading;
using System.Threading.Tasks;
using UnityEngine.UI;

namespace Pancake.UI
{
    public static class PopupHelper
    {
        public const string POPUP_LABEL = "ui_popup";
        
        public static async Task<Button> SelectButton(CancellationToken token, params Button[] buttons)
        {
            var selected = false;
            Button buttonSelected = null;

            foreach (var button in buttons)
            {
                button.onClick.AddListener(() =>
                {
                    selected = true;
                    buttonSelected = button;
                });
            }

            return await Task.Run(async () =>
                {
                    while (true)
                    {
                        if (token.IsCancellationRequested)
                        {
                            break;
                        }

                        if (selected) return buttonSelected;
                        await Task.Yield();
                    }

                    return null;
                },
                token);
        }

        /// <summary>
        /// waiting back button pressed while popup active
        /// </summary>
        /// <param name="popup"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        public static async Task<bool> WaitForPressBackButton(IPopup popup, CancellationToken token)
        {
            return await Task.Run(async () =>
                {
                    while (popup.Active)
                    {
                        if (token.IsCancellationRequested) token.ThrowIfCancellationRequested();
                        if (popup.BackButtonPressed) return true;
                        await Task.Yield();
                    }

                    return false;
                },
                token);
        }
    }
    
}