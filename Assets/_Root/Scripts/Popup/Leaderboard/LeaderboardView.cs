using System;
using System.Collections.Generic;
using Pancake.SceneFlow;
using Pancake.Scriptable;
using Pancake.Threading.Tasks;
using TMPro;
using Unity.Services.Authentication;
using Unity.Services.Leaderboards;
using Unity.Services.Leaderboards.Models;
using UnityEngine;
using UnityEngine.UI;

namespace Pancake.UI
{
    public sealed class LeaderboardView : View
    {
        protected class LeaderboardData
        {
            public int currentPage;
            public List<LeaderboardEntry> entries;
            public bool firstTime;
            public int pageCount;
            public int myPosition;
            private readonly string _key;

            public LeaderboardData(string key)
            {
                _key = key;
                firstTime = true;
                entries = new List<LeaderboardEntry>();
                currentPage = 0;
                pageCount = 0;
                myPosition = -1;
            }
        }

        [SerializeField] private CountryCollection countryCollection;
        [SerializeField] private string tableId;
        [SerializeField] private IntVariable currentLevel;
        [SerializeField] private Button buttonClose;
        [SerializeField] private Button buttonNextPage;
        [SerializeField] private Button buttonPreviousPage;
        [SerializeField] private Button buttonWorld;
        [SerializeField] private Button buttonCountry;
        [SerializeField] private Button buttonAllTimeRank;
        [SerializeField] private Button buttonWeeklyRank;
        [SerializeField] private TextMeshProUGUI textName;
        [SerializeField] private TextMeshProUGUI textRank;
        [SerializeField] private TextMeshProUGUI textCurrentPage;
        [SerializeField] private GameObject content;
        [SerializeField] private GameObject block;
        [SerializeField] private LeaderboardElementView[] slots;
        [SerializeField] private GameObject waitLoginObject;
        [SerializeField, PopupPickup] private string popupRename;

        [SerializeField] private LeaderboardElementColor colorRank1 = new LeaderboardElementColor(new Color(1f, 0.82f, 0f),
            new Color(0.44f, 0.33f, 0f),
            new Color(0.99f, 0.96f, 0.82f),
            new Color(1f, 0.55f, 0.01f),
            new Color(0.47f, 0.31f, 0f));

        [SerializeField] private LeaderboardElementColor colorRank2 = new LeaderboardElementColor(new Color(0.79f, 0.84f, 0.91f),
            new Color(0.29f, 0.4f, 0.6f),
            new Color(0.94f, 0.94f, 0.94f),
            new Color(0.45f, 0.54f, 0.56f),
            new Color(0.18f, 0.31f, 0.48f));

        [SerializeField] private LeaderboardElementColor colorRank3 = new LeaderboardElementColor(new Color(0.8f, 0.59f, 0.31f),
            new Color(0.34f, 0.23f, 0.09f),
            new Color(1f, 0.82f, 0.57f),
            new Color(0.3f, 0.22f, 0.12f),
            new Color(0.4f, 0.25f, 0.1f));

        [SerializeField] private LeaderboardElementColor colorRankYou = new LeaderboardElementColor(new Color(0.47f, 0.76f, 0.92f),
            new Color(0.08f, 0.53f, 0.71f),
            new Color(0.09f, 0.53f, 0.71f),
            new Color(0.22f, 0.58f, 0.85f),
            new Color(0.08f, 0.27f, 0.42f));

        [SerializeField] private LeaderboardElementColor colorOutRank = new LeaderboardElementColor();


        private LeaderboardData _worldData = new LeaderboardData("world-alltime");
        private int _countInOnePage;

        protected override UniTask Initialize()
        {
            _countInOnePage = slots.Length;
            buttonClose.onClick.AddListener(OnButtonClosePressed);
            buttonNextPage.onClick.AddListener(OnButtonNextPagePressed);
            buttonPreviousPage.onClick.AddListener(OnButtonPreviousPagePressed);
            buttonWorld.onClick.AddListener(OnButtonWorldPressed);
            buttonCountry.onClick.AddListener(OnButtonCountryPressed);

            InternalInit();
            return UniTask.CompletedTask;
        }

        private void OnButtonCountryPressed() { }

        private void OnButtonWorldPressed() { }


        private void OnButtonPreviousPagePressed() { }

        private void OnButtonNextPagePressed() { }

        private void OnButtonClosePressed()
        {
            PlaySoundClose();
            PopupHelper.Close(transform);
        }

        private async void InternalInit()
        {
            if (!AuthenticationService.Instance.IsSignedIn)
            {
                waitLoginObject.SetActive(true);
                await AuthenticationService.Instance.SignInAnonymouslyAsync();
                await LeaderboardsService.Instance.AddPlayerScoreAsync(tableId, currentLevel.Value);
                waitLoginObject.SetActive(false);

                if (string.IsNullOrEmpty(AuthenticationService.Instance.PlayerName)) ShowPopupRename(OnPopupRenameClosed);
                else
                {
                    textName.text = AuthenticationService.Instance.PlayerName;
                }
            }
            else
            {
                // todo
            }
        }

        private void OnPopupRenameClosed() { textName.text = AuthenticationService.Instance.PlayerName; }

        private void ShowPopupRename(Action onPopupRenameClosed)
        {
            PopupContainer.Find(Constant.MAIN_POPUP_CONTAINER).Push<RenamePopup>(popupRename, true, onLoad: t => { t.popup.view.SetCallbackClose(onPopupRenameClosed); });
        }

        private LeaderboardElementColor ColorDivision(int rank, string playerId)
        {
            if (playerId.Equals(AuthenticationService.Instance.PlayerId)) return colorRankYou;

            return rank switch
            {
                1 => colorRank1,
                2 => colorRank2,
                3 => colorRank3,
                _ => colorOutRank
            };
        }

        private void HideAllSlot()
        {
            foreach (var slot in slots)
            {
                slot.gameObject.SetActive(false);
            }
        }

        private void Refresh(LeaderboardData data)
        {
            HideAllSlot();
            textCurrentPage.text = $"PAGE {data.currentPage + 1}";
            if (data.currentPage >= data.pageCount)
            {
                buttonNextPage.gameObject.SetActive(false);
                buttonPreviousPage.gameObject.SetActive(data.currentPage != 0);
                block.SetActive(false);
                return;
            }

            block.SetActive(true);
            var pageData = new List<LeaderboardEntry>();
            for (int i = 0; i < _countInOnePage; i++)
            {
                int index = data.currentPage * _countInOnePage + i;
                if (data.entries.Count <= index) break;
                pageData.Add(data.entries[index]);
            }

            buttonPreviousPage.gameObject.SetActive(data.currentPage != 0);
            buttonNextPage.gameObject.SetActive(data.currentPage < data.pageCount && !(data.entries.Count < 100 && data.currentPage == data.pageCount - 1));
            content.SetActive(false);
        }
    }
}