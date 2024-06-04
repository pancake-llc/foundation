using Pancake.Sound;
using R3;
using VContainer;
using VContainer.Unity;

namespace Pancake.SceneFlow
{
    public class SettingModalPresenter : VObject<SettingModalContext>, IStartable
    {
        [Inject] private readonly SettingModalContext.UIModel _model;
        [Inject] private readonly SettingModalContext.UIView _view;
        [Inject] private readonly AudioManager _audioManager;

        public void Start()
        {
            InitializeModel();
            InitializeView();
        }

        private void InitializeModel() { }

        private void InitializeView()
        {
            _view.ButtonMusic.OnClickAsObservable().Subscribe(_ =>
            {
                // todo
            });
        }
    }
}