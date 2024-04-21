using System;
using LitMotion;
using LitMotion.Extensions;
using Pancake.Common;
using Pancake.Localization;
using Pancake.UI;
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

        private MotionHandle _handle;

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
                if (_handle.IsActive()) _handle.Cancel();
                objectHightLight.SetActive(model.IsSelected(model.Lang));
                if (objectHightLight.activeInHierarchy)
                {
                    _handle = LMotion.Create(0f, 1f, 0.25f)
                        .WithOnComplete(action.Invoke)
                        .BindToLocalScaleX(objectHightLight.transform);
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