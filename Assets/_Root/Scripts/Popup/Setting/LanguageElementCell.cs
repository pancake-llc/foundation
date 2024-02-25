using System;
using Pancake.Localization;
using Pancake.UI;
using PrimeTween;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Pancake.SceneFlow
{
    public class LanguageElementCell : Cell<LanguageElementCellModel>
    {
        [SerializeField] private TextMeshProUGUI textName;
        [SerializeField] private LocaleTextComponent localeTextName;
        [SerializeField] private GameObject objectHightLight;
        [SerializeField] private Button buttonSelect;

        private Tween _tween;

        protected override void SetModel(LanguageElementCellModel model)
        {
            textName.text = model.Lang.Name;
            localeTextName.Variable = model.LocaleText;
            localeTextName.ForceUpdate();

            buttonSelect.onClick.RemoveAllListeners();
            buttonSelect.onClick.AddListener(() =>
            {
                model.OnSelectAction?.Invoke(model.Lang);
                ActiveTween(model.OnHideLanuageaAction);
            });
            ActiveTween(() => { });
            return;

            void ActiveTween(Action action)
            {
                _tween.Stop();
                objectHightLight.SetActive(model.IsSelected(model.Lang));
                if (objectHightLight.activeInHierarchy)
                {
                    objectHightLight.transform.SetScaleX(0);
                    _tween = Tween.ScaleX(objectHightLight.transform, 1f, 0.25f).OnComplete(action.Invoke);
                }
            }
        }
    }

    public sealed class LanguageElementCellModel : CellModel
    {
        public Language Lang { get; set; }
        public LocaleText LocaleText { get; set; }
        public Func<Language, bool> IsSelected { get; set; }
        public Action<Language> OnSelectAction { get; set; }
        public Action OnHideLanuageaAction { get; set; }
    }
}