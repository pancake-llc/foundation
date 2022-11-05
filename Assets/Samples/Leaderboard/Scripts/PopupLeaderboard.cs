using System;
using System.Collections.Generic;
using System.Globalization;
using Pancake.Threading.Tasks;
#if PANCAKE_FACEBOOK
using Pancake.Facebook;
#endif
using Pancake.Linq;
using Pancake.Tween;
using Pancake.UI;
using PlayFab;
using PlayFab.ClientModels;
#if ENABLE_PLAYFABADMIN_API
using PlayFab.ServerModels;
#endif
using TMPro;
using UnityEngine;
using GetLeaderboardResult = PlayFab.ClientModels.GetLeaderboardResult;
using GetPlayFabIDsFromFacebookIDsResult = PlayFab.ClientModels.GetPlayFabIDsFromFacebookIDsResult;
using PlayerLeaderboardEntry = PlayFab.ClientModels.PlayerLeaderboardEntry;

// ReSharper disable FieldCanBeMadeReadOnly.Local

namespace Pancake.GameService
{
    public class PopupLeaderboard : MonoBehaviour
    {
        public enum ELeaderboardTab
        {
            World = 0,
            Country = 1,
            Friend = 2
        }

        [Serializable]
        public class ElementColor
        {
            public Color colorBackground;
            public Color colorOverlay;
            public Color colorBoder;
            public Color colorHeader;
            public Color colorText;

            public ElementColor(Color colorBackground, Color colorOverlay, Color colorBoder, Color colorHeader, Color colorText)
            {
                this.colorBackground = colorBackground;
                this.colorOverlay = colorOverlay;
                this.colorBoder = colorBoder;
                this.colorHeader = colorHeader;
                this.colorText = colorText;
            }

            public ElementColor()
            {
                colorBackground = new Color(0.99f, 0.96f, 0.82f);
                colorOverlay = new Color(0.8f, 0.66f, 0.33f);
                colorBoder = new Color(0.99f, 0.96f, 0.82f);
                colorHeader = new Color(1f, 0.67f, 0.26f);
                colorText = new Color(0.68f, 0.3f, 0.01f);
            }
        }

        protected class Data
        {
            public int currentPage;
            public List<PlayerLeaderboardEntry> players;
            public bool firstTime;
            public int pageCount;
            public int myPosition;
            private readonly string _key;

            public Data(string key)
            {
                _key = key;
                firstTime = true;
                players = new List<PlayerLeaderboardEntry>();
                currentPage = 0;
                pageCount = 0;
                myPosition = -1;
            }

            public DateTime LastTimeRefreshLeaderboard
            {
                get
                {
                    DateTime.TryParse(PlayerPrefs.GetString($"{LAST_TIME_FETCH_RANK_KEY}_{_key}", DateTime.UtcNow.ToString(CultureInfo.InvariantCulture)),
                        out var result);
                    return result;
                }
                set => PlayerPrefs.SetString($"{LAST_TIME_FETCH_RANK_KEY}_{_key}", value.ToString(CultureInfo.InvariantCulture));
            }

            public bool IsCanRefresh(int limitTime) => (DateTime.UtcNow - LastTimeRefreshLeaderboard).TotalSeconds >= limitTime || firstTime;
        }

#if PANCAKE_FACEBOOK
        protected class FriendData
        {
            public sealed class Entry
            {
                public Sprite sprite;
                public string displayName;
                public string playfabId;
                public string facebookId;
                public int statValue;
            }

            public int currentPage;
            public List<Entry> players;
            public bool firstTime;
            public int pageCount;
            public int myPosition;
            private readonly string _key;

            public FriendData(string key)
            {
                _key = key;
                firstTime = true;
                players = new List<Entry>();
                currentPage = 0;
                pageCount = 1;
                myPosition = -1;
            }

            public DateTime LastTimeRefreshLeaderboard
            {
                get
                {
                    DateTime.TryParse(PlayerPrefs.GetString($"{LAST_TIME_FETCH_RANK_KEY}_{_key}", DateTime.UtcNow.ToString(CultureInfo.InvariantCulture)),
                        out var result);
                    return result;
                }
                set => PlayerPrefs.SetString($"{LAST_TIME_FETCH_RANK_KEY}_{_key}", value.ToString(CultureInfo.InvariantCulture));
            }

            public bool IsCanRefresh(int limitTime) => (DateTime.UtcNow - LastTimeRefreshLeaderboard).TotalSeconds >= limitTime || firstTime;
        }
#endif

        private const string LAST_TIME_FETCH_RANK_KEY = "last_time_fetch_rank";

        #region field

        [SerializeField] private CountryCode countryCode;
        [SerializeField] private UIButton btnNextPage;
        [SerializeField] private UIButton btnBackPage;
        [SerializeField] private UIButtonTMP btnWorld;
        [SerializeField] private UIButtonTMP btnCountry;
        [SerializeField] private UIButtonTMP btnFriend;
        [SerializeField] private TextMeshProUGUI txtName;
        [SerializeField] private TextMeshProUGUI txtRank;
        [SerializeField] private TextMeshProUGUI txtCurrentPage;
        [SerializeField] private GameObject content;
        [SerializeField] private LeaderboardElement[] rankSlots;

        [SerializeField] private ElementColor colorRank1 = new ElementColor(new Color(1f, 0.82f, 0f),
            new Color(0.44f, 0.33f, 0f),
            new Color(0.99f, 0.96f, 0.82f),
            new Color(1f, 0.55f, 0.01f),
            new Color(0.47f, 0.31f, 0f));

        [SerializeField] private ElementColor colorRank2 = new ElementColor(new Color(0.79f, 0.84f, 0.91f),
            new Color(0.29f, 0.4f, 0.6f),
            new Color(0.94f, 0.94f, 0.94f),
            new Color(0.45f, 0.54f, 0.56f),
            new Color(0.18f, 0.31f, 0.48f));

        [SerializeField] private ElementColor colorRank3 = new ElementColor(new Color(0.8f, 0.59f, 0.31f),
            new Color(0.34f, 0.23f, 0.09f),
            new Color(1f, 0.82f, 0.57f),
            new Color(0.3f, 0.22f, 0.12f),
            new Color(0.4f, 0.25f, 0.1f));

        [SerializeField] private ElementColor colorRankYou = new ElementColor(new Color(0.47f, 0.76f, 0.92f),
            new Color(0.08f, 0.53f, 0.71f),
            new Color(0.09f, 0.53f, 0.71f),
            new Color(0.22f, 0.58f, 0.85f),
            new Color(0.08f, 0.27f, 0.42f));

        [SerializeField] private ElementColor colorOutRank = new ElementColor();

        [SerializeField] private TextMeshProUGUI txtWarning;
        [SerializeField] private GameObject block;
        [SerializeField] private string nameTableLeaderboard;
        [SerializeField] private AnimationCurve displayRankCurve;
        [SerializeField] private Sprite spriteTabSelected;
        [SerializeField] private Sprite spriteTabNormal;
        [SerializeField] private Color colorTabTextSelected;
        [SerializeField] private Color colorTabTextNormal;

        #endregion

        protected Data worldData = new Data("world");
        protected Data countryData = new Data("country");
#if PANCAKE_FACEBOOK
        protected FriendData friendData = new FriendData("friend");
#endif
        protected Dictionary<string, InternalConfig> userInternalConfig = new Dictionary<string, InternalConfig>();
        protected Dictionary<string, Sprite> friendCacheAvatar = new Dictionary<string, Sprite>();
        protected ELeaderboardTab currentTab = ELeaderboardTab.World;
        protected bool sessionFirstTime;
        protected HandleIconFacebook handleIconFacebook;
        protected Canvas canvas;

        public HandleIconFacebook HandleIconFacebook
        {
            get
            {
                if (handleIconFacebook == null) handleIconFacebook = btnFriend.GetComponent<HandleIconFacebook>();
                return handleIconFacebook;
            }
        }

        public int CountInOnePage => rankSlots.Length;

        protected virtual ElementColor ColorDivision(int rank, string playerId)
        {
            if (playerId.Equals(LoginResultModel.playerId)) return colorRankYou;
            switch (rank)
            {
                case 1: return colorRank1;
                case 2: return colorRank2;
                case 3: return colorRank3;
                default: return colorOutRank;
            }
        }
#if ENABLE_PLAYFABSERVER_API
        protected virtual void OnEnable()
        {
            btnBackPage.onClick.AddListener(OnBackPageButtonClicked);
            btnNextPage.onClick.AddListener(OnNextPageButtonClicked);
            btnWorld.onClick.AddListener(OnWorldButtonClicked);
            btnCountry.onClick.AddListener(OnCountryButtonClicked);
#if PANCAKE_FACEBOOK
            btnFriend.onClick.AddListener(OnFriendButtonClicked);
#endif
            txtName.text = LoginResultModel.playerDisplayName;

            if (!sessionFirstTime)
            {
                sessionFirstTime = true;
                userInternalConfig.Clear();
                friendCacheAvatar.Clear();
            }

            currentTab = ELeaderboardTab.World;
            WorldButtonInvokeImpl();
        }
#endif
        protected virtual void Refresh(Data data)
        {
            txtCurrentPage.text = $"PAGE {data.currentPage + 1}";
            if (data.currentPage >= data.pageCount) // reach the end
            {
                btnNextPage.gameObject.SetActive(false);
                txtWarning.text = "Nothing to show\nYou have reached the end of the rankings";
                txtWarning.gameObject.SetActive(true);
                block.SetActive(false);
                btnBackPage.gameObject.SetActive(data.currentPage != 0);
                return;
            }

            block.SetActive(true);
            var pageData = new List<PlayerLeaderboardEntry>();
            for (int i = 0; i < CountInOnePage; i++)
            {
                int index = data.currentPage * CountInOnePage + i;
                if (data.players.Count <= index) break;

                pageData.Add(data.players[index]);
            }

            btnBackPage.gameObject.SetActive(data.currentPage != 0);
            btnNextPage.gameObject.SetActive(data.currentPage < data.pageCount && !(data.players.Count < 100 && data.currentPage == data.pageCount - 1));

            content.SetActive(false);
            foreach (var element in rankSlots)
            {
                element.gameObject.SetActive(false);
            }

            FetchInternalConfig(pageData, OnOnePageFetchInternalConfigCompleted);
        }

        public void Init(Canvas canvas)
        {
            this.canvas = canvas;
        }

        private IEnumerator<float> OnOnePageFetchInternalConfigCompleted(List<PlayerLeaderboardEntry> entries, InternalConfig[] internalConfigs)
        {
            block.SetActive(false);
            content.SetActive(true);

            for (int i = 0; i < internalConfigs.Length; i++)
            {
                rankSlots[i]
                .Init(internalConfigs[i],
                    entries[i].Position + 1,
                    countryCode.Get(internalConfigs[i].countryCode).icon,
                    entries[i].DisplayName,
                    entries[i].StatValue,
                    ColorDivision(entries[i].Position + 1, entries[i].PlayFabId),
                    canvas,
                    entries[i].PlayFabId.Equals(LoginResultModel.playerId));
                rankSlots[i].gameObject.SetActive(true);
                var sequense = TweenManager.Sequence();
                sequense.Append(rankSlots[i].transform.TweenLocalScale(new Vector3(1.04f, 1.06f, 1), 0.15f).SetEase(Ease.OutQuad));
                sequense.Append(rankSlots[i].transform.TweenLocalScale(Vector3.one, 0.08f).SetEase(Ease.InQuad));
                sequense.Play();
                yield return Timing.WaitForSeconds(displayRankCurve.Evaluate(i / (float) internalConfigs.Length));
            }
        }

        private async void FetchInternalConfig(List<PlayerLeaderboardEntry> entries, Func<List<PlayerLeaderboardEntry>, InternalConfig[], IEnumerator<float>> onCompleted)
        {
            InternalConfig[] configs = new InternalConfig[entries.Count];
            for (int i = 0; i < entries.Count; i++)
            {
                if (!userInternalConfig.ContainsKey(entries[i].PlayFabId))
                {
                    configs[i] = await AuthService.GetUserData<InternalConfig>(entries[i].PlayFabId,
                        ServiceSettings.INTERNAL_CONFIG_KEY,
                        errorCallback: error => Debug.Log(error.ErrorMessage));
                    userInternalConfig.Add(entries[i].PlayFabId, configs[i]);
                }
                else
                {
                    configs[i] = userInternalConfig[entries[i].PlayFabId];
                }
            }

            Timing.RunCoroutine(onCompleted?.Invoke(entries, configs));
        }

#if PANCAKE_FACEBOOK
        protected virtual void OnFriendButtonClicked()
        {
            if (currentTab == ELeaderboardTab.Friend) return;
            currentTab = ELeaderboardTab.Friend;
            UpdateDisplayTab();
            content.SetActive(false);
            block.SetActive(true);
            txtRank.text = "";
            HideWarning();
            // validate status login facebook
            if (!FacebookManager.Instance.IsLoggedIn)
            {
                FacebookManager.Instance.Login(OnFacebookLoginCompleted, OnFacebookLoginFaild, OnFacebookLoginError);
            }
            else
            {
                FetchFriend();
            }
        }
#endif
#if ENABLE_PLAYFABSERVER_API
        protected virtual void OnCountryButtonClicked()
        {
            if (currentTab == ELeaderboardTab.Country) return;
            currentTab = ELeaderboardTab.Country;
            UpdateDisplayTab();
            HideWarning();
            content.SetActive(false);
            if (countryData.IsCanRefresh(ServiceSettings.delayFetchRank))
            {
                countryData.firstTime = false;
                countryData.players.Clear();
                countryData.LastTimeRefreshLeaderboard = DateTime.UtcNow;
                if (AuthService.Instance.isLoggedIn && AuthService.Instance.isRequestCompleted)
                {
                    block.SetActive(true);
                    AuthService.GetStatistic(LoginResultModel.playerId,
                        $"{nameTableLeaderboard}_{LoginResultModel.countryCode}",
                        OnGetLeaderboardAroundUserCountrySuccess,
                        OnGetLeaderboardAroundUserCountryError);
                }
                else
                {
                    LogError();
                }
            }
            else
            {
                txtRank.text = $"Country Rank: {countryData.myPosition + 1}";
                Refresh(countryData);
            }
        }
#endif
#if ENABLE_PLAYFABSERVER_API
        protected virtual void OnWorldButtonClicked()
        {
            if (currentTab == ELeaderboardTab.World) return;
            currentTab = ELeaderboardTab.World;
            UpdateDisplayTab();
            HideWarning();
            WorldButtonInvokeImpl();
        }
#endif
        private void LogError()
        {
            if (AuthService.Instance.isLoggedIn)
            {
                #region replace your code show popup notification

                var popupNotification = GameObject.Find("PopupNotification").GetComponent<PopupNotification>();
                popupNotification.Message("An error occurred,\nYou seem to have not completed entering your name and selecting your country");

                #endregion
            }
            else
            {
                #region replace your code show popup notification

                var popupNotification = GameObject.Find("PopupNotification").GetComponent<PopupNotification>();
                popupNotification.Message("Login failed for unknown reason!");

                #endregion
            }
        }

        private void HideWarning() { txtWarning.gameObject.SetActive(false); }

        protected virtual void UpdateDisplayTab()
        {
            switch (currentTab)
            {
                case ELeaderboardTab.World:
                    btnWorld.image.sprite = spriteTabSelected;
                    btnWorld.Label.color = colorTabTextSelected;

                    btnCountry.image.sprite = spriteTabNormal;
                    btnCountry.Label.color = colorTabTextNormal;

                    btnFriend.image.sprite = spriteTabNormal;
                    btnFriend.Label.color = colorTabTextNormal;
                    HandleIconFacebook.DeSelect();
                    break;
                case ELeaderboardTab.Country:
                    btnWorld.image.sprite = spriteTabNormal;
                    btnWorld.Label.color = colorTabTextNormal;

                    btnCountry.image.sprite = spriteTabSelected;
                    btnCountry.Label.color = colorTabTextSelected;

                    btnFriend.image.sprite = spriteTabNormal;
                    btnFriend.Label.color = colorTabTextNormal;
                    HandleIconFacebook.DeSelect();
                    break;
                case ELeaderboardTab.Friend:
                    btnWorld.image.sprite = spriteTabNormal;
                    btnWorld.Label.color = colorTabTextNormal;

                    btnCountry.image.sprite = spriteTabNormal;
                    btnCountry.Label.color = colorTabTextNormal;

                    btnFriend.image.sprite = spriteTabSelected;
                    btnFriend.Label.color = colorTabTextSelected;
                    HandleIconFacebook.Select();
                    break;
            }
        }

        /// <summary>
        /// next page
        /// </summary>
        protected virtual void OnNextPageButtonClicked()
        {
            btnNextPage.interactable = false;
            txtWarning.gameObject.SetActive(false);
            switch (currentTab)
            {
                case ELeaderboardTab.World:
                    worldData.currentPage++;
                    if (worldData.currentPage == worldData.pageCount)
                    {
                        if (worldData.currentPage * CountInOnePage >= worldData.players.Count && worldData.players.Count > 0)
                        {
                            block.SetActive(true);
                            content.SetActive(false);
                            AuthService.RequestLeaderboard(nameTableLeaderboard,
                                NextPageRequestWorldLeaderboardSuccess,
                                NextPageRequestWorldLeaderboardError,
                                worldData.currentPage * CountInOnePage);
                        }
                    }
                    else
                    {
                        btnNextPage.interactable = true;
                        Refresh(worldData);
                    }

                    break;
                case ELeaderboardTab.Country:
                    countryData.currentPage++;
                    if (countryData.currentPage == countryData.pageCount)
                    {
                        if (countryData.currentPage * CountInOnePage >= countryData.players.Count && countryData.players.Count > 0)
                        {
                            block.SetActive(true);
                            content.SetActive(false);
                            AuthService.RequestLeaderboard($"{nameTableLeaderboard}_{LoginResultModel.countryCode}",
                                NextPageRequestCountryLeaderboardSuccess,
                                NextPageRequestCountryLeaderboardError,
                                countryData.currentPage * CountInOnePage);
                        }
                    }
                    else
                    {
                        btnNextPage.interactable = true;
                        Refresh(countryData);
                    }

                    break;
                case ELeaderboardTab.Friend:
#if PANCAKE_FACEBOOK
                    if (friendData.currentPage < friendData.pageCount)
                    {
                        friendData.currentPage++;
                        btnNextPage.interactable = true;
                        Refresh(friendData);
                    }
#endif

                    break;
            }
        }

        /// <summary>
        /// previous page
        /// </summary>
        protected virtual void OnBackPageButtonClicked()
        {
            btnBackPage.interactable = false;
            txtWarning.gameObject.SetActive(false);
            switch (currentTab)
            {
                case ELeaderboardTab.World:
                    if (worldData.currentPage > 0)
                    {
                        worldData.currentPage--;
                        btnBackPage.interactable = true;
                        Refresh(worldData);
                    }

                    break;
                case ELeaderboardTab.Country:
                    if (countryData.currentPage > 0)
                    {
                        countryData.currentPage--;
                        btnBackPage.interactable = true;
                        Refresh(countryData);
                    }

                    break;
                case ELeaderboardTab.Friend:
#if PANCAKE_FACEBOOK
                    if (friendData.currentPage > 0)
                    {
                        friendData.currentPage--;
                        btnBackPage.interactable = true;
                        Refresh(friendData);
                    }

#endif
                    break;
            }
        }

        #region world

        private void NextPageRequestWorldLeaderboardSuccess(GetLeaderboardResult result)
        {
            btnNextPage.interactable = true;
            if (result == null && worldData.players.Count == 0) return;

            txtWarning.gameObject.SetActive(false);
            if (result != null) worldData.players.AddRange(result.Leaderboard);
            worldData.pageCount = M.CeilToInt(worldData.players.Count / (float) CountInOnePage);
            Refresh(worldData);
        }

        private void NextPageRequestWorldLeaderboardError(PlayFabError error) { btnNextPage.interactable = true; }

        private void OnGetLeaderboardAroundUserWorldError(PlayFabError error)
        {
            #region replace your code show popup notification

            var popupNotification = GameObject.Find("PopupNotification").GetComponent<PopupNotification>();
            popupNotification.Message($"Retrieve your position in the failed world ranking!\nError code: {error.Error}");

            #endregion
        }
#if ENABLE_PLAYFABSERVER_API
        private void OnGetLeaderboardAroundUserWorldSuccess(GetLeaderboardAroundUserResult success)
        {
            worldData.myPosition = success.Leaderboard[0].Position;
            txtRank.text = $"World Rank: {worldData.myPosition + 1}";
            AuthService.RequestLeaderboard(nameTableLeaderboard, RequestWorldLeaderboardSuccess, RequestWorldLeaderboardError);
        }
#endif
        private void RequestWorldLeaderboardError(PlayFabError error)
        {
            #region replace your code show popup notification

            var popupNotification = GameObject.Find("PopupNotification").GetComponent<PopupNotification>();
            popupNotification.Message($"Retrieve world ranking information failed!\nError code: {error.Error}");

            #endregion
        }

        private void RequestWorldLeaderboardSuccess(GetLeaderboardResult result)
        {
            if (result == null && worldData.players.Count == 0) return;

            txtWarning.gameObject.SetActive(false);
            if (result != null) worldData.players = result.Leaderboard;
            worldData.pageCount = M.CeilToInt(worldData.players.Count / (float) CountInOnePage);
            Refresh(worldData);
        }
#if ENABLE_PLAYFABSERVER_API
        private void WorldButtonInvokeImpl()
        {
            content.SetActive(false);

            if (worldData.IsCanRefresh(ServiceSettings.delayFetchRank))
            {
                worldData.firstTime = false;
                worldData.players.Clear();
                worldData.LastTimeRefreshLeaderboard = DateTime.UtcNow;
                if (AuthService.Instance.isLoggedIn && AuthService.Instance.isRequestCompleted)
                {
                    // wait if need
                    block.SetActive(true);
                    AuthService.GetStatistic(LoginResultModel.playerId,
                        nameTableLeaderboard,
                        OnGetLeaderboardAroundUserWorldSuccess,
                        OnGetLeaderboardAroundUserWorldError);
                }
                else
                {
                    LogError();
                }
            }
            else
            {
                // display with old data
                txtRank.text = $"World Rank: {worldData.myPosition + 1}";
                Refresh(worldData);
            }
        }
#endif

        #endregion

        #region country

        private void NextPageRequestCountryLeaderboardSuccess(GetLeaderboardResult result)
        {
            btnNextPage.interactable = true;
            if (result == null && countryData.players.Count == 0) return;

            txtWarning.gameObject.SetActive(false);
            if (result != null) countryData.players.AddRange(result.Leaderboard);
            countryData.pageCount = M.CeilToInt(countryData.players.Count / (float) CountInOnePage);
            Refresh(countryData);
        }

        private void NextPageRequestCountryLeaderboardError(PlayFabError error) { btnNextPage.interactable = true; }

        private void OnGetLeaderboardAroundUserCountryError(PlayFabError error)
        {
            #region replace your code show popup notification

            var popupNotification = GameObject.Find("PopupNotification").GetComponent<PopupNotification>();
            popupNotification.Message($"Retrieve your position in the failed country ranking!\nError code: {error.Error}");

            #endregion
        }
#if ENABLE_PLAYFABSERVER_API
        private void OnGetLeaderboardAroundUserCountrySuccess(GetLeaderboardAroundUserResult success)
        {
            countryData.myPosition = success.Leaderboard[0].Position;
            txtRank.text = $"Country Rank: {countryData.myPosition + 1}";
            AuthService.RequestLeaderboard($"{nameTableLeaderboard}_{LoginResultModel.countryCode}", RequestCountryLeaderboardSuccess, RequestCountryLeaderboardError);
        }

        private void RequestCountryLeaderboardError(PlayFabError error)
        {
            #region replace your code show popup notification

            var popupNotification = GameObject.Find("PopupNotification").GetComponent<PopupNotification>();
            popupNotification.Message($"Retrieve country ranking information failed!\nError code: {error.Error}");

            #endregion
        }
#endif

        private void RequestCountryLeaderboardSuccess(GetLeaderboardResult result)
        {
            if (result == null && countryData.players.Count == 0) return;

            txtWarning.gameObject.SetActive(false);
            if (result != null) countryData.players = result.Leaderboard;
            countryData.pageCount = M.CeilToInt(countryData.players.Count / (float) CountInOnePage);
            Refresh(countryData);
        }

        #endregion

#if PANCAKE_FACEBOOK
        #region friend

        private void FetchFriend()
        {
            if (!FacebookManager.Instance.IsCompletedGetFriendData)
            {
                friendData.players.Clear();
                FacebookManager.Instance.GetMeFriend(FetchFriendDataFb);
            }
            else
            {
                FetchFriendDataFb();
            }
        }

        private void OnFacebookLoginError()
        {
            block.SetActive(false);
            Popup.Show<PopupNotification>(_ =>
            {
                _.Message("Error to login Facebook!\nPlease try again!");
                _.Ok(OnWorldButtonClicked);
            });
        }

        private void OnFacebookLoginFaild()
        {
            block.SetActive(false);
            Popup.Show<PopupNotification>(_ =>
            {
                _.Message("Faild to login Facebook!\nPlease try again!");
                _.Ok(OnWorldButtonClicked);
            });
        }

        private void OnFacebookLoginCompleted()
        {
            // link account with facebook.
            if (!LoginResultModel.facebookAuth)
            {
                AuthService.LinkFacebook(FacebookManager.Instance.Token, OnLinkFacebookCompleted, OnLinkFacebookError);
            }
            else
            {
                FetchFriend();
            }
        }

        private void OnLinkFacebookCompleted(LinkFacebookAccountResult obj)
        {
            FetchFriend();

            block.SetActive(false);
            LoginResultModel.facebookAuth = true;
        }

        private void OnLinkFacebookError(PlayFabError error)
        {
            Popup.Show<PopupNotification>(_ =>
            {
                _.Message(error.ErrorMessage);
                _.Ok(OnWorldButtonClicked);
            });
        }

        private async void OnGetPlayfabIDsFromFacebookIDsCompleted(GetPlayFabIDsFromFacebookIDsResult result)
        {
            foreach (var idPair in result.Data)
            {
                bool status = false;

                AuthService.GetStatistic(idPair.PlayFabId,
                    nameTableLeaderboard,
                    userResult =>
                    {
                        Debug.Log("Display Name: " + userResult.Leaderboard[0].DisplayName + "    Score: " + userResult.Leaderboard[0].StatValue + "   Facebook Id:" +
                                  idPair.FacebookId);
                        friendData.players.Add(new FriendData.Entry()
                        {
                            playfabId = userResult.Leaderboard[0].PlayFabId,
                            facebookId = idPair.FacebookId,
                            sprite = null,
                            displayName = userResult.Leaderboard[0].DisplayName,
                            statValue = userResult.Leaderboard[0].StatValue
                        });
                        status = true;
                    },
                    error => { status = true; });
                await UniTask.WaitUntil(() => status);
            }

            AddMyDataInFriendScope(() => Refresh(friendData));
        }

        private async void AddMyDataInFriendScope(Action onCompleted)
        {
            bool validate = false;
            foreach (var player in friendData.players)
            {
                if (player.facebookId.Equals(FacebookManager.Instance.UserId)) validate = true;
            }

            if (validate)
            {
                onCompleted?.Invoke();
                return;
            }

            await UniTask.WaitUntil(() => !FacebookManager.Instance.IsRequestingProfile);
            var localFlag = false;
            AuthService.GetStatistic(LoginResultModel.playerId,
                nameTableLeaderboard,
                userResult =>
                {
                    friendData.players.Add(new FriendData.Entry()
                    {
                        playfabId = LoginResultModel.playerId,
                        facebookId = FacebookManager.Instance.UserId,
                        sprite = FacebookManager.CreateSprite(FacebookManager.Instance.ProfilePicture, Vector2.one * 0.5f),
                        displayName = LoginResultModel.playerDisplayName,
                        statValue = userResult.Leaderboard[0].StatValue
                    });
                    localFlag = true;
                },
                error =>
                {
                    Debug.LogError(error.Error);
                    localFlag = true;
                });

            await UniTask.WaitUntil(() => localFlag);
            onCompleted?.Invoke();
        }

        private void OnGetPlayFabIDsFromFacebookIDsError(PlayFabError error) { }

        private async void FetchFriendDataFb()
        {
            if (FacebookManager.Instance.IsRequestingFriend || FacebookManager.Instance.FriendDatas == null)
            {
                await UniTask.WaitUntil(() => !FacebookManager.Instance.IsRequestingFriend && FacebookManager.Instance.FriendDatas != null);
            }

            var friendIds = FacebookManager.Instance.FriendDatas.Map(_ => _.id).Filter(_ => !friendCacheAvatar.ContainsKey(_));

            if (friendIds.Count == 0)
            {
                AddMyDataInFriendScope(() => Refresh(friendData));
            }
            else
            {
                AuthService.GetPlayFabIDsFromFacebook(friendIds, OnGetPlayfabIDsFromFacebookIDsCompleted, OnGetPlayFabIDsFromFacebookIDsError);
            }
        }

        private async void FetchFriendConfig(List<FriendData.Entry> entries, Func<List<FriendData.Entry>, InternalConfig[], IEnumerator<float>> onCompleted)
        {
            // TO_DO
            for (int i = 0; i < entries.Count; i++)
            {
                if (entries[i].sprite == null)
                {
                    var fbId = entries[i].facebookId;
                    if (!friendCacheAvatar.ContainsKey(fbId))
                    {
                        var status = await FacebookManager.Instance.LoadTextureInternal(FacebookManager.Instance.FriendDatas.First(_ => _.id.Equals(fbId)).pictureUrl);
                        if (status)
                        {
                            entries[i].sprite =
                                FacebookManager.CreateSprite(FacebookManager.Instance.FriendDatas.First(_ => _.id.Equals(fbId)).avatar, Vector2.one * 0.5f);
                            friendCacheAvatar.Add(fbId, entries[i].sprite);
                        }
                    }
                    else
                    {
                        entries[i].sprite = friendCacheAvatar[entries[i].facebookId];
                    }
                }
            }

            Timing.RunCoroutine(onCompleted?.Invoke(entries, null));
        }

        protected virtual void Refresh(FriendData data)
        {
            data.pageCount = M.CeilToInt(data.players.Count / (float) CountInOnePage);
            txtCurrentPage.text = $"PAGE {data.currentPage + 1}";
            if (data.currentPage >= data.pageCount) // reach the end
            {
                btnNextPage.gameObject.SetActive(false);
                txtWarning.text = "Nothing to show\nYou have reached the end of the rankings";
                txtWarning.gameObject.SetActive(true);
                block.SetActive(false);
                btnBackPage.gameObject.SetActive(data.currentPage != 0);
                return;
            }

            block.SetActive(true);
            data.players.OrderByDescending(_ => _.statValue);
            for (int i = 0; i < data.players.Count; i++)
            {
                if (data.players[i].playfabId.Equals(LoginResultModel.playerId))
                {
                    data.myPosition = i;
                    break;
                }
            }

            txtRank.text = $"Friend Rank: {data.myPosition + 1}";
            var pageData = new List<FriendData.Entry>();
            for (int i = 0; i < CountInOnePage; i++)
            {
                int index = data.currentPage * CountInOnePage + i;
                if (data.players.Count <= index) break;

                pageData.Add(data.players[index]);
            }

            btnBackPage.gameObject.SetActive(data.currentPage != 0);
            btnNextPage.gameObject.SetActive(data.currentPage < data.pageCount && !(data.players.Count < 100 && data.currentPage == data.pageCount - 1));

            content.SetActive(false);
            foreach (var element in rankSlots)
            {
                element.gameObject.SetActive(false);
            }

            FetchFriendConfig(pageData, OnOnePageFetchFriendConfigCompleted);
        }

        private IEnumerator<float> OnOnePageFetchFriendConfigCompleted(List<FriendData.Entry> entries, InternalConfig[] internalConfigs)
        {
            block.SetActive(false);
            content.SetActive(true);

            for (int i = 0; i < entries.Count; i++)
            {
                InternalConfig interConfig = null;
                float c = 0;
                if (internalConfigs != null)
                {
                    c = internalConfigs.Length > 0 ? internalConfigs.Length : 1;
                    if (internalConfigs.Length - 1 > i) interConfig = internalConfigs[i];
                }
                else
                {
                    c = 1;
                }

                rankSlots[i]
                .Init(interConfig,
                    i + 1,
                    entries[i].sprite,
                    entries[i].displayName,
                    entries[i].statValue,
                    ColorDivision(i + 1, entries[i].playfabId),
                    canvas,
                    entries[i].playfabId.Equals(LoginResultModel.playerId));
                rankSlots[i].gameObject.SetActive(true);
                var sequense = TweenManager.Sequence();
                sequense.Append(rankSlots[i].transform.TweenLocalScale(new Vector3(1.04f, 1.06f, 1), 0.15f).SetEase(Ease.OutQuad));
                sequense.Append(rankSlots[i].transform.TweenLocalScale(Vector3.one, 0.08f).SetEase(Ease.InQuad));
                sequense.Play();
                yield return Timing.WaitForSeconds(displayRankCurve.Evaluate(i / c));
            }
        }

        #endregion
#endif
        
#if ENABLE_PLAYFABSERVER_API
        protected virtual void OnDisable()
        {
            btnBackPage.onClick.RemoveListener(OnBackPageButtonClicked);
            btnNextPage.onClick.RemoveListener(OnNextPageButtonClicked);
            btnWorld.onClick.RemoveListener(OnWorldButtonClicked);
            btnCountry.onClick.RemoveListener(OnCountryButtonClicked);
#if PANCAKE_FACEBOOK
            btnFriend.onClick.RemoveListener(OnFriendButtonClicked);
#endif
        }
#endif

#if UNITY_EDITOR && ENABLE_PLAYFABSERVER_API
        private int _internalIndex = 0;
        [ContextMenu("Update Aggregation")]
        public void CreateOrUpdateAggregationLeaderboard()
        {
            if (UnityEditor.EditorUtility.DisplayDialog("Update Aggregation All Leaderboard",
                    "Are you sure you wish to update aggregation for all leaderboard to Maximum?\nThis action cannot be reversed.",
                    "Update",
                    "Cancel"))
            {
                _internalIndex = 0;
                Call();
            }

            void Call()
            {
                var c = countryCode.countryCodeDatas[_internalIndex];
                UnityEditor.EditorUtility.DisplayProgressBar("Update Aggregation Highest Value",
                    $"Updating {c.code.ToString()}...",
                    _internalIndex / (float) countryCode.countryCodeDatas.Length);
                PlayFabAdminAPI.CreatePlayerStatisticDefinition(new PlayFab.AdminModels.CreatePlayerStatisticDefinitionRequest()
                    {
                        StatisticName = $"{nameTableLeaderboard}_{c.code.ToString()}",
                        AggregationMethod = PlayFab.AdminModels.StatisticAggregationMethod.Max,
                        VersionChangeInterval = PlayFab.AdminModels.StatisticResetIntervalOption.Never
                    },
                    _ =>
                    {
                        if (_internalIndex < countryCode.countryCodeDatas.Length - 1)
                        {
                            _internalIndex++;
                            Call();
                        }
                        else
                        {
                            Debug.Log("Update Aggregation Completed!");
                            UnityEditor.EditorUtility.ClearProgressBar();
                        }
                    },
                    error =>
                    {
                        if (error.Error == PlayFabErrorCode.StatisticNameConflict)
                        {
                            PlayFabAdminAPI.UpdatePlayerStatisticDefinition(new PlayFab.AdminModels.UpdatePlayerStatisticDefinitionRequest()
                                {
                                    StatisticName = $"{nameTableLeaderboard}_{c.code.ToString()}",
                                    AggregationMethod = PlayFab.AdminModels.StatisticAggregationMethod.Max,
                                    VersionChangeInterval = PlayFab.AdminModels.StatisticResetIntervalOption.Never
                                },
                                _ =>
                                {
                                    if (_internalIndex < countryCode.countryCodeDatas.Length - 1)
                                    {
                                        _internalIndex++;
                                        Call();
                                    }
                                    else
                                    {
                                        Debug.Log("Update Aggregation Completed!");
                                        UnityEditor.EditorUtility.ClearProgressBar();
                                    }
                                },
                                fabError => { Debug.LogError(fabError.Error); });
                        }
                        else
                        {
                            Debug.LogError(error.Error);
                        }
                    });
            }
        }
#endif
    }
}