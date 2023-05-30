using System.Threading;
using System.Threading.Tasks;
using UnityEngine.UI;

namespace Pancake.UI
{
    internal static class PopupHelper
    {
        internal static async Task<Button> SelectButton(CancellationToken token, params Button[] buttons)
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
                        if (token.IsCancellationRequested) break;
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
        internal static async Task<bool> WaitForPressBackButton(CancellationToken token, UIPopup popup)
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