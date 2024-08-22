using Pancake.Sound;

namespace Pancake.Game
{
    using UnityEngine;

    [EditorIcon("icon_entry")]
    public class Gameplay : MonoBehaviour
    {
        public async void GotoMenu()
        {
            AudioStatic.StopAll();
            await SceneLoader.LoadScene(Constant.Scene.MENU);
        }
    }
}