using Pancake.Sound;

namespace Pancake.Game
{
    using UnityEngine;

    [EditorIcon("icon_entry")]
    public class Gameplay : MonoBehaviour
    {
        [SerializeField, AudioPickup] private AudioId bgm;

        private void Start() { bgm.Play(); }

        public async void GotoMenu() { await SceneLoader.LoadScene(Constant.Scene.MENU); }

        private void OnDestroy() { AudioStatic.StopAllAndClean(); }
    }
}