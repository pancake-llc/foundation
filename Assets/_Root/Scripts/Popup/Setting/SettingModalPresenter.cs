using VContainer;
using VContainer.Unity;

namespace Pancake.SceneFlow
{
    public class SettingModalPresenter : VObject<SettingModalContext>, IStartable
    {
        [Inject] private readonly SettingModalContext.UIModel _model;
        [Inject] private readonly SettingModalContext.UIView _view;


        public void Start()
        {
            InitializeModel();
            InitializeView();
        }

        private void InitializeModel()
        {
            
        }

        private void InitializeView()
        {
            
        }
    }
}