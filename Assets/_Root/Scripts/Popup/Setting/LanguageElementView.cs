using System;
using Pancake.Localization;
using Pancake.UI;
using PrimeTween;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Pancake.SceneFlow
{
    public class LanguageElementView : EnhancedScrollerCellView
    {
        [SerializeField] private TextMeshProUGUI textName;
        [SerializeField] private LocaleTextComponent localeTextName;
        [SerializeField] private GameObject objectHightLight;
        [SerializeField] private Button buttonSelect;

        private Func<string, bool> _funcSelected;
        private Action<LanguageElementView> _onClicked;
        private Action _onClickedAdvance;
        private Tween _tween;
        public Language Lang { get; private set; }
        
        public void Init(
            Language language,
            Action<LanguageElementView> onClicked,
            Func<string, bool> funcSelected,
            Func<int, LocaleText> getLocaleText,
            Action onClickedAdvance)
        {
            _funcSelected = funcSelected;
            _onClicked = onClicked;
            _onClickedAdvance = onClickedAdvance;
            Lang = language;
            textName.text = language.Name;
            localeTextName.Variable = getLocaleText?.Invoke(LocaleSettings.AvailableLanguages.FindIndex(x => x.Code == language.Code));
            localeTextName.ForceUpdate();
            buttonSelect.onClick.RemoveListener(OnButtonSelectPressed);
            buttonSelect.onClick.AddListener(OnButtonSelectPressed);
            objectHightLight.SetActive(_funcSelected.Invoke(Lang.Code));
        }

        private void OnButtonSelectPressed() { _onClicked?.Invoke(this); }

        public override void RefreshCellView()
        {
            _tween.Stop();
            objectHightLight.SetActive(_funcSelected.Invoke(Lang.Code));
            if (objectHightLight.activeInHierarchy)
            {
                objectHightLight.transform.SetScaleX(0);
                _tween = Tween.ScaleX(objectHightLight.transform, 1f, 0.25f).OnComplete(() => _onClickedAdvance?.Invoke());
            }
        }
    }
}