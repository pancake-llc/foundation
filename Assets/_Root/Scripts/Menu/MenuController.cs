using System;
using System.Threading;
using Pancake.Scriptable;
using Pancake.Sound;
using Pancake.Threading.Tasks;
using Pancake.UI;
using UnityEngine;
using UnityEngine.UI;

namespace Pancake.SceneFlow
{
    [EditorIcon("script_controller")]
    public class MenuController : GameComponent
    {
        [SerializeField] private Transform canvasUI;
        [SerializeField] private ScriptableEventGetGameObject canvasMaster;
        [SerializeField] private BoolVariable remoteConfigFetchCompleted;
        [SerializeField] private StringVariable remoteConfigNewVersion;
        [SerializeField] private BoolVariable dontShowUpdateAgain;
        [Header("BUTTON")] [SerializeField] private Button buttonSetting;
        [SerializeField] private Button buttonTapToPlay;
        [SerializeField] private Button buttonShop;

        //[Header("POPUP")] [SerializeField] private PopupShowEvent popupShowEvent;
        // [SerializeField, PopupPickup] private string popupShop;
        // [SerializeField, PopupPickup] private string popupSetting;
        // [SerializeField, PopupPickup] private string popupUpdate;

        [Header("OTHER")] [SerializeField] private AudioComponent buttonAudio;
        [SerializeField] private ScriptableEventString changeSceneEvent;
        private CancellationTokenSource _tokenShowUpdate;

        private void Start()
        {
            buttonSetting.onClick.AddListener(ShowPopupSetting);
            buttonTapToPlay.onClick.AddListener(GoToGameplay);
            buttonShop.onClick.AddListener(ShowPopupShop);
            WaitShowUpdate();
        }

        private async void WaitShowUpdate()
        {
            if (remoteConfigFetchCompleted == null) return;
            _tokenShowUpdate = new CancellationTokenSource();
            try
            {
                await UniTask.WaitUntil(() => remoteConfigFetchCompleted, PlayerLoopTiming.Update, _tokenShowUpdate.Token);
                var version = new Version(remoteConfigNewVersion.Value);
                int result = version.CompareTo(new Version(Application.version));
                // is new version
                //if (result > 0 && !dontShowUpdateAgain) popupShowEvent.Raise(popupUpdate, canvasUI);
            }
            catch (OperationCanceledException)
            {
                // ignored
            }
        }

        private void GoToGameplay() { changeSceneEvent.Raise(Constant.GAMEPLAY_SCENE); }

        private void ShowPopupShop()
        {
            //popupShowEvent.Raise(popupShop, canvasMaster.Raise().transform);
        }
        
        private void ShowPopupSetting()
        {
            //buttonAudio.PlayAudio();
            //popupShowEvent.Raise(popupSetting, canvasUI);
        }

        protected override void OnDisabled()
        {
            if (_tokenShowUpdate != null)
            {
                _tokenShowUpdate.Cancel();
                _tokenShowUpdate.Dispose();
            }
        }
    }
}