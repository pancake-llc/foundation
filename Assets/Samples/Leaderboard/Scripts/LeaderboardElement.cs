using Coffee.UIEffects;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Pancake.GameService
{
    public class LeaderboardElement : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI txtRank;
        [SerializeField] private Image imgCountry;
        [SerializeField] private TextMeshProUGUI txtUserName;
        [SerializeField] private TextMeshProUGUI txtScore;
        [SerializeField] private Image imgForcegound;
        [SerializeField] private Image imgOverlay1;
        [SerializeField] private Image imgOverlay2;
        [SerializeField] private GameObject you;
        [SerializeField] private Image imgHeader;
        [SerializeField] private Image imgCircleRank;
        [SerializeField] private GameObject decor;

        protected InternalConfig userInternalConfig;

        public virtual void Init(InternalConfig userInternalConfig, int rank, Sprite icon, string userName, int score, PopupLeaderboard.ElementColor color, Canvas root, bool self)
        {
            this.userInternalConfig = userInternalConfig;
            txtRank.text = $"{rank}";
            txtUserName.text = userName;
            txtScore.color = color.colorText;
            txtUserName.color = color.colorText;
            imgForcegound.color = color.colorBackground;
            imgOverlay1.color = color.colorOverlay;
            imgOverlay2.color = color.colorBoder;
            imgHeader.color = color.colorHeader;
            txtScore.text = score.ToString();
            imgCountry.sprite = icon;
            imgCountry.gameObject.SetActive(true);
            you.SetActive(self);
            if (rank != 1 && rank != 2 && rank != 3)
            {
                imgCircleRank.gameObject.SetActive(true);
                txtRank.gameObject.SetActive(true);
                decor.SetActive(false);
            }
            else
            {
                imgCircleRank.gameObject.SetActive(false);
                txtRank.gameObject.SetActive(false);
                decor.SetActive(true);
                var eff = decor.GetComponentInChildren<ParticleSystemRenderer>();
                if (eff != null)
                {
                    eff.sortingLayerName = root.sortingLayerName;
                    eff.sortingOrder = root.sortingOrder + 1;
                }
            }

            TryGetComponent<UIShiny>(out var uiShiny);
            if (uiShiny != null) uiShiny.enabled = (rank == 1 || rank == 2 || rank == 3);
        }
    }
}