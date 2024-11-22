using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using LitMotion;
using LitMotion.Extensions;
using Pancake.Common;
using Pancake.Localization;
using Pancake.SignIn;
using Pancake.UI;
using TMPro;
using Unity.Services.Authentication;
using Unity.Services.Leaderboards;
using Unity.Services.Leaderboards.Models;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

namespace Pancake.Game.UI
{
    public sealed class LeaderboardView : View
    {
        private class LeaderboardData
        {
            public int currentPage;
            public List<LeaderboardEntry> entries;
            public bool firstTime;
            public int pageCount;
            public int myRank;
            public int offset;
            public int limit;
            private readonly string _key;

            public LeaderboardData(string key)
            {
                _key = key;
                firstTime = true;
                entries = new List<LeaderboardEntry>();
                currentPage = 0;
                pageCount = 1;
                offset = 0;
                limit = 100;
                myRank = -1;
            }
        }

        public enum ELeaderboardTab
        {
            AllTime,
            Weekly,
        }

        [SerializeField] private string allTimeTableId = "ALL_TIME_RANK";
        [SerializeField] private string weeklyTableId = "WEEKLY_RANK";
        [SerializeField] private CountryIconAsset countryIconAsset;
        [SerializeField] private Button buttonClose;
        [SerializeField] private Button buttonNextPage;
        [SerializeField] private Button buttonPreviousPage;
        [SerializeField] private Button buttonAllTimeRank;
        [SerializeField] private Button buttonWeeklyRank;
        [SerializeField] private Color colorCurrentTab;
        [SerializeField] private Color colorNormalTab;
        [SerializeField] private TextMeshProUGUI textName;
        [SerializeField] private LocaleTextComponent localeTextRank;
        [SerializeField] private LocaleTextComponent localeTextCurrentPage;
        [SerializeField] private GameObject contentSlot;
        [SerializeField] private GameObject rootLeaderboard;
        [SerializeField] private GameObject block;
        [SerializeField] private LeaderboardElementVisual[] slots;
        [SerializeField] private AnimationCurve displayRankCurve;
        [SerializeField, PopupPickup] private string popupRename;

        [SerializeField] private LeaderboardElementColor colorRank1 = new(new Color(1f, 0.82f, 0f), new Color(1f, 0.55f, 0.01f), new Color(0.47f, 0.31f, 0f));

        [SerializeField] private LeaderboardElementColor colorRank2 = new(new Color(0.79f, 0.84f, 0.91f), new Color(0.45f, 0.54f, 0.56f), new Color(0.18f, 0.31f, 0.48f));

        [SerializeField] private LeaderboardElementColor colorRank3 = new(new Color(0.8f, 0.59f, 0.31f), new Color(0.3f, 0.22f, 0.12f), new Color(0.4f, 0.25f, 0.1f));

        [SerializeField] private LeaderboardElementColor colorRankYou = new(new Color(0.47f, 0.76f, 0.92f),
            new Color(0.22f, 0.58f, 0.85f),
            new Color(0.08f, 0.27f, 0.42f));

        [SerializeField] private LeaderboardElementColor colorOutRank = new();

        [SerializeField, PopupPickup] private string popupNotification;
        [SerializeField] private LocaleText localeLoginGpgsFail;
        [SerializeField] private LocaleText localeLoginAppleFail;

        private LeaderboardData _allTimeData = new("alltime_data");
        private LeaderboardData _weeklyData = new("weekly_data");
        private Dictionary<string, Dictionary<string, object>> _userLeaderboardData = new();
        private int _countInOnePage;
        private MotionHandle[] _handles;
        private ELeaderboardTab _currentTab = ELeaderboardTab.AllTime;
        private bool _firstTimeEnterWeekly = true;
        private bool _firstTimeEnterWorld = true;

        protected override UniTask Initialize()
        {
            _countInOnePage = slots.Length;
            _handles = new MotionHandle[slots.Length];
            buttonClose.onClick.AddListener(OnButtonClosePressed);
            buttonNextPage.onClick.AddListener(OnButtonNextPagePressed);
            buttonPreviousPage.onClick.AddListener(OnButtonPreviousPagePressed);
            buttonAllTimeRank.onClick.AddListener(OnButtonAllTimeRankPressed);
            buttonWeeklyRank.onClick.AddListener(OnButtonWeeklyRankPressed);

            InternalInit();

            return UniTask.CompletedTask;
        }

        private void OnButtonAllTimeRankPressed()
        {
            _currentTab = ELeaderboardTab.AllTime;
            buttonWeeklyRank.image.color = colorNormalTab;
            buttonAllTimeRank.image.color = colorCurrentTab;
            InternalInit();
        }

        private void OnButtonWeeklyRankPressed()
        {
            _currentTab = ELeaderboardTab.Weekly;
            buttonWeeklyRank.image.color = colorCurrentTab;
            buttonAllTimeRank.image.color = colorNormalTab;
            InternalInitWeekly();
        }

        private void OnButtonPreviousPagePressed()
        {
            buttonPreviousPage.interactable = false;
            switch (_currentTab)
            {
                case ELeaderboardTab.AllTime:
                    AllTimePreviousPage();
                    break;
                case ELeaderboardTab.Weekly:
                    WeeklyPreviousPage();
                    break;
            }
        }

        private void WeeklyPreviousPage()
        {
            if (_weeklyData.currentPage > 0)
            {
                _weeklyData.currentPage--;
                buttonPreviousPage.interactable = true;
                Refresh(_weeklyData);
            }
        }

        private void AllTimePreviousPage()
        {
            if (_allTimeData.currentPage > 0)
            {
                _allTimeData.currentPage--;
                buttonPreviousPage.interactable = true;
                Refresh(_allTimeData);
            }
        }

        private void OnButtonNextPagePressed()
        {
            buttonNextPage.interactable = false;
            switch (_currentTab)
            {
                case ELeaderboardTab.AllTime:
                    AllTimeNextPage();
                    break;
                case ELeaderboardTab.Weekly:
                    WeeklyNextPage();
                    break;
            }
        }

#pragma warning disable CS1998
        private async void WeeklyNextPage()
#pragma warning restore CS1998
        {
            _weeklyData.currentPage++;
            if (_weeklyData.currentPage == _weeklyData.pageCount - 1)
            {
                if (_weeklyData.entries.Count > 0)
                {
                    block.SetActive(true);
                    contentSlot.SetActive(false);
                    await LoadNextDataWeeklyScores(); // request more entry
                    block.SetActive(false);
                    Refresh(_weeklyData);
                }
            }
            else
            {
                buttonNextPage.interactable = true;
                Refresh(_weeklyData);
            }
        }

#pragma warning disable CS1998
        private async void AllTimeNextPage()
#pragma warning restore CS1998
        {
            _allTimeData.currentPage++;
            if (_allTimeData.currentPage == _allTimeData.pageCount - 1)
            {
                if (_allTimeData.entries.Count > 0)
                {
                    block.SetActive(true);
                    contentSlot.SetActive(false);
                    await LoadNextDataAllTimeScores(); // request more entry
                    block.SetActive(false);
                    Refresh(_allTimeData);
                }
            }
            else
            {
                buttonNextPage.interactable = true;
                Refresh(_allTimeData);
            }
        }

        private void OnButtonClosePressed()
        {
            PlaySoundClose();
            PopupHelper.Close(transform, false);
        }

#pragma warning disable CS1998
        private async void InternalInit()
#pragma warning restore CS1998
        {
            block.SetActive(true);
            rootLeaderboard.SetActive(false);
#if UNITY_EDITOR
            if (!AuthenticationService.Instance.IsSignedIn) await AuthenticationService.Instance.SignInAnonymouslyAsync();
#elif UNITY_ANDROID && PANCAKE_GPGS
            if (!AuthenticationGooglePlayGames.IsSignIn())
            {
                SignInEvent.status = false;
                SignInEvent.Login();
                await UniTask.WaitUntil(() => SignInEvent.status);
                if (string.IsNullOrEmpty(SignInEvent.ServerCode))
                {
                    await MainUIContainer.In.GetMain<PopupContainer>()
                        .PushAsync<NotificationPopup>(popupNotification, true, onLoad: tuple => tuple.popup.view.Setup(localeLoginGpgsFail, OnButtonClosePressed));
                    return;
                }
            }
            else
            {
                SignInEvent.status = false;
                SignInEvent.GetNewServerCode();
                await UniTask.WaitUntil(() => SignInEvent.status);
            }
#elif UNITY_IOS
            SignInEvent.status = false;
            SignInEvent.Login();
            await UniTask.WaitUntil(() => SignInEvent.status);

            if (string.IsNullOrEmpty(SignInEvent.ServerCode))
            {
                await MainUIContainer.In.GetMain<PopupContainer>()
                    .PushAsync<NotificationPopup>(popupNotification, true, onLoad: tuple => tuple.popup.view.Setup(localeLoginAppleFail, OnButtonClosePressed));
                return;
            }
#endif

            if (!AuthenticationService.Instance.IsSignedIn)
            {
#if UNITY_ANDROID && !UNITY_EDITOR
                if (AuthenticationService.Instance.SessionTokenExists)
                {
                    // signin cached
                    await AuthenticationService.Instance.SignInAnonymouslyAsync();
                }
                else
                {
                    await AuthenticationService.Instance.SignInWithGooglePlayGamesAsync(SignInEvent.ServerCode);
                }

#elif UNITY_IOS && !UNITY_EDITOR
                if (AuthenticationService.Instance.SessionTokenExists)
                {
                    // signin cached
                    await AuthenticationService.Instance.SignInAnonymouslyAsync();
                }
                else
                {
                    await AuthenticationService.Instance.SignInWithAppleAsync(SignInEvent.ServerCode);
                }
#endif
            }

            await Excute();

            return;

            async Task Excute()
            {
                rootLeaderboard.SetActive(false);

                LeaderboardEntry resultAdded;
                if (_firstTimeEnterWorld)
                {
                    int value = Random.Range(0, 100); // todo replace with your value
                    resultAdded = await LeaderboardsService.Instance.AddPlayerScoreAsync(allTimeTableId, value);
                }
                else
                {
                    resultAdded = await LeaderboardsService.Instance.GetPlayerScoreAsync(allTimeTableId);
                }

                _allTimeData.myRank = resultAdded.Rank;
                if (string.IsNullOrEmpty(AuthenticationService.Instance.PlayerName)) ShowPopupRename(OnPopupRenameClosed);
                else
                {
                    if (_firstTimeEnterWorld)
                    {
                        _firstTimeEnterWorld = false;
                        await LoadNextDataAllTimeScores();
                    }

                    block.SetActive(false);
                    Refresh(_allTimeData);
                }
            }
        }

        private async void InternalInitWeekly()
        {
            rootLeaderboard.SetActive(false);
            LeaderboardEntry resultAdded;
            if (_firstTimeEnterWeekly)
            {
                int value = Random.Range(0, 100); // todo replace with your value
                resultAdded = await LeaderboardsService.Instance.AddPlayerScoreAsync(weeklyTableId, value);
            }
            else
            {
                resultAdded = await LeaderboardsService.Instance.GetPlayerScoreAsync(weeklyTableId);
            }

            _weeklyData.myRank = resultAdded.Rank;

            if (_firstTimeEnterWeekly)
            {
                _firstTimeEnterWeekly = false;
                await LoadNextDataWeeklyScores();
            }

            block.SetActive(false);
            Refresh(_weeklyData);
        }

#pragma warning disable CS1998
        private async UniTask<bool> LoadNextDataAllTimeScores()
#pragma warning restore CS1998
        {
            _allTimeData.offset = (_allTimeData.entries.Count - 1).Max(0);
            var scores = await LeaderboardsService.Instance.GetScoresAsync(allTimeTableId,
                new GetScoresOptions {Limit = _allTimeData.limit, Offset = _allTimeData.offset});
            _allTimeData.entries.AddRange(scores.Results);
            _allTimeData.pageCount = (_allTimeData.entries.Count / (float) _countInOnePage).CeilToInt();
            return true;
        }

#pragma warning disable CS1998
        private async UniTask<bool> LoadNextDataWeeklyScores()
#pragma warning restore CS1998
        {
            _weeklyData.offset = (_weeklyData.entries.Count - 1).Max(0);
            var scores = await LeaderboardsService.Instance.GetScoresAsync(weeklyTableId, new GetScoresOptions {Limit = _weeklyData.limit, Offset = _weeklyData.offset});
            _weeklyData.entries.AddRange(scores.Results);
            _weeklyData.pageCount = (_weeklyData.entries.Count / (float) _countInOnePage).CeilToInt();
            return true;
        }

#pragma warning disable CS1998
        private async void OnPopupRenameClosed(bool isCancel)
#pragma warning restore CS1998
        {
            if (string.IsNullOrEmpty(AuthenticationService.Instance.PlayerName) || isCancel)
            {
                await PopupHelper.Close(transform, false);
                return;
            }

            await LoadNextDataAllTimeScores();
            block.SetActive(false);

            Refresh(_allTimeData);
        }

        private async void ShowPopupRename(Action<bool> onPopupRenameClosed)
        {
            var popupContainer = MainUIContainer.In.GetMain<PopupContainer>();
            while (popupContainer.IsInTransition)
            {
                await UniTask.Yield();
            }

            await popupContainer.PushAsync<RenamePopup>(popupRename, true, onLoad: t => { t.popup.view.SetCallbackClose(onPopupRenameClosed); });
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
            buttonNextPage.interactable = true;
            string[] playerNameSplits = AuthenticationService.Instance.PlayerName.Split('#');
            textName.text = playerNameSplits[0];
            localeTextRank.UpdateArgs($"{data.myRank + 1}");
            rootLeaderboard.SetActive(true);
            HideAllSlot();
            localeTextCurrentPage.UpdateArgs($"{data.currentPage + 1}");
            if (data.currentPage >= data.pageCount - 1) // reach the end
            {
                buttonNextPage.gameObject.SetActive(false);
                buttonPreviousPage.gameObject.SetActive(data.currentPage != 0);
            }

            block.SetActive(true);

            foreach (var handle in _handles)
            {
                if (handle.IsActive()) handle.Cancel();
            }

            var pageData = new List<LeaderboardEntry>();
            for (int i = 0; i < _countInOnePage; i++)
            {
                int index = data.currentPage * _countInOnePage + i;
                if (data.entries.Count <= index) break;

                pageData.Add(data.entries[index]);
            }

            buttonPreviousPage.gameObject.SetActive(data.currentPage != 0);
            buttonNextPage.gameObject.SetActive(data.currentPage < data.pageCount - 1);
            contentSlot.SetActive(true);
            block.SetActive(false);

            PageSetup(pageData);
        }

        private async void PageSetup(List<LeaderboardEntry> pageData)
        {
            for (int i = 0; i < pageData.Count; i++)
            {
                slots[i]
                    .Init(pageData[i].Rank + 1,
                        countryIconAsset.Get(pageData[i].PlayerName.Split('#')[1]).icon,
                        pageData[i].PlayerName.Split('#')[0],
                        (int) pageData[i].Score,
                        ColorDivision(pageData[i].Rank + 1, pageData[i].PlayerId),
                        pageData[i].PlayerId.Equals(AuthenticationService.Instance.PlayerId));
                slots[i].gameObject.SetActive(true);

                if (_handles[i].IsActive()) _handles[i].Cancel();
                // todo play anim
                int i1 = i;
                _handles[i] = LMotion.Create(new Vector3(0.92f, 0.92f, 0.92f), new Vector3(1.04f, 1.06f, 1), 0.2f)
                    .WithEase(Ease.OutQuad)
                    .WithOnComplete(() =>
                    {
                        LMotion.Create(new Vector3(1.04f, 1.06f, 1), Vector3.one, 0.15f).WithEase(Ease.InQuad).BindToLocalScale(slots[i1].transform);
                    })
                    .BindToLocalScale(slots[i].transform);

                await Awaitable.WaitForSecondsAsync(displayRankCurve.Evaluate(i / (float) pageData.Count));
            }
        }
    }
}