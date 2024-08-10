using TMPro;
using UnityEngine.UI;

namespace Pancake.Game.UI
{
    using UnityEngine;

    public sealed class LeaderboardElementVisual : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI textRank;
        [SerializeField] private Image imageCountry;
        [SerializeField] private TextMeshProUGUI textUserName;
        [SerializeField] private TextMeshProUGUI textScore;
        [SerializeField] private Image imageForcegound;
        [SerializeField] private GameObject you;
        [SerializeField] private Image imageRank;


        public void Init(int rank, Sprite icon, string userName, int score, LeaderboardElementColor color, bool self)
        {
            textRank.text = $"{rank}";
            textUserName.text = userName;
            textScore.color = color.colorText;
            textUserName.color = color.colorText;
            imageForcegound.color = color.colorBackground;
            imageRank.color = color.colorRank;
            textScore.text = score.ToString();
            imageCountry.sprite = icon;
            imageCountry.gameObject.SetActive(true);
            you.SetActive(self);
        }
    }
}