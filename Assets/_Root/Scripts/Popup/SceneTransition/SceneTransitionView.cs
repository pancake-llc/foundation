using Pancake.Apex;
using Pancake.Common;
using Pancake.SceneFlow;
using Pancake.Spine;
using Pancake.Threading.Tasks;
using Spine.Unity;
using UnityEngine;

namespace Pancake.UI
{
    public sealed class SceneTransitionView : View
    {
        [SerializeField] private SkeletonGraphic transitionGraphic;

        [SerializeField, SpineAnimation(dataField = nameof(transitionGraphic))]
        private string nameAnimOpen;

        [SerializeField, SpineAnimation(dataField = nameof(transitionGraphic))]
        private string nameAnimClose;

        [SerializeField] private bool overrideDuration;
        [SerializeField, ShowIf(nameof(overrideDuration))] private float duration;

        protected override UniTask Initialize() { return UniTask.CompletedTask; }

        public void Setup(bool isOpen = false)
        {
            string anim = isOpen ? nameAnimOpen : nameAnimClose;
            float d = overrideDuration ? duration : transitionGraphic.Duration(anim);

            App.Delay(transitionGraphic, d, () => { PopupHelper.Close(transform, false); });
            transitionGraphic.PlayOnly(anim);
        }
    }
}