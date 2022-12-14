#if PANCAKE_PLAYFAB
using System;
using System.Collections.Generic;
using Pancake.Threading.Tasks;
#if UNITY_IOS
using AppleAuth;
using AppleAuth.Interfaces;
using AppleAuth.Native;
#endif
using PlayFab;
using PlayFab.ClientModels;
using PlayFab.Json;
#if ENABLE_PLAYFABADMIN_API
using PlayFab.ServerModels;
#endif
using UnityEngine;
using GetLeaderboardRequest = PlayFab.ClientModels.GetLeaderboardRequest;
using GetLeaderboardResult = PlayFab.ClientModels.GetLeaderboardResult;
using GetPlayerCombinedInfoRequestParams = PlayFab.ClientModels.GetPlayerCombinedInfoRequestParams;
using GetPlayFabIDsFromFacebookIDsRequest = PlayFab.ClientModels.GetPlayFabIDsFromFacebookIDsRequest;
using GetPlayFabIDsFromFacebookIDsResult = PlayFab.ClientModels.GetPlayFabIDsFromFacebookIDsResult;
using GetUserDataRequest = PlayFab.ClientModels.GetUserDataRequest;
using GetUserDataResult = PlayFab.ClientModels.GetUserDataResult;
using LoginResult = PlayFab.ClientModels.LoginResult;
using PlayerProfileViewConstraints = PlayFab.ClientModels.PlayerProfileViewConstraints;
using StatisticUpdate = PlayFab.ClientModels.StatisticUpdate;
using UpdatePlayerStatisticsRequest = PlayFab.ClientModels.UpdatePlayerStatisticsRequest;
using UpdatePlayerStatisticsResult = PlayFab.ClientModels.UpdatePlayerStatisticsResult;
using UpdateUserDataRequest = PlayFab.ClientModels.UpdateUserDataRequest;
using UpdateUserDataResult = PlayFab.ClientModels.UpdateUserDataResult;
using UpdateUserTitleDisplayNameRequest = PlayFab.ClientModels.UpdateUserTitleDisplayNameRequest;
using UpdateUserTitleDisplayNameResult = PlayFab.ClientModels.UpdateUserTitleDisplayNameResult;
using UserDataPermission = PlayFab.ClientModels.UserDataPermission;

namespace Pancake.GameService
{
    /// <summary>
    /// best practice
    /// 1. Anonymous login when logging in for the first time
    /// 2. You should also authentication other than anonymous login so that you can recover your account
    /// 3. I don't like entering the user name and password from the first time
    /// </summary>
    public class AuthService
    {
        //Events to subscribe to for this service
        public delegate void DisplayAuthenticationEvent();

        public delegate void LoginSuccessEvent(LoginResult success);

        public delegate void PlayFabErrorEvent(PlayFabError error);

        public delegate void UpdateUserTitleDisplayNameSuccessEvent(UpdateUserTitleDisplayNameResult success);

        public delegate void UpdatePlayerStatisticsSuccessEvent(UpdatePlayerStatisticsResult success);


        public static event DisplayAuthenticationEvent OnDisplayAuthentication;
        public static event LoginSuccessEvent OnLoginSuccess;
        public static event PlayFabErrorEvent OnLoginError;
        public static event UpdateUserTitleDisplayNameSuccessEvent OnUpdateUserTitleDisplayNameSuccess;
        public static event PlayFabErrorEvent OnUpdateUserTitleDisplayNameError;
        public static event UpdatePlayerStatisticsSuccessEvent OnUpdatePlayerStatisticsSuccess;
        public static event PlayFabErrorEvent OnUpdatePlayerStatisticsError;

        public string email;
        public string userName;
        public string password;
        public string authTicket;
        public GetPlayerCombinedInfoRequestParams infoRequestParams;
        public bool forceLink;
        public bool isLoggedIn;
        public bool isRequestCompleted;
        public static string PlayFabId { get; private set; }
        public static string SessionTicket { get; private set; }
        private const string LOGIN_REMEMBER_KEY = "PLAYFAB_LOGIN_REMEMBER";
        private const string AUTH_TYPE_KEY = "PLAYFAB_AUTH_TYPE";
        private const string CUSTOM_ID_STORE_KEY = "PLAYFAB_CUSTOM_ID_AUTH";
        private const string COMPLETE_SETUP_NAME_STORE_KEY = "PLAYFAB_COMPLETE_SETUP_NAME";
        private static AuthService instance;
#if UNITY_IOS
        public static byte[] identityToken;
        private static IAppleAuthManager appleAuthManager;
        private const string APPLE_USER_ID = "APPLE_USER_ID";
#endif

        public static AuthService Instance
        {
            get
            {
                if (instance == null) instance = new AuthService();
                return instance;
            }
        }

        public AuthService()
        {
            instance = this;

#if UNITY_IOS
            if (AppleAuthManager.IsCurrentPlatformSupported)
            {
                var deserializer = new PayloadDeserializer();
                appleAuthManager = new AppleAuthManager(deserializer);
            }
#endif
        }

        public bool RememberMe { get => PlayerPrefs.GetInt(LOGIN_REMEMBER_KEY, 0) != 0; set => PlayerPrefs.SetInt(LOGIN_REMEMBER_KEY, value ? 1 : 0); }

        public EAuthType AuthType { get => (EAuthType) PlayerPrefs.GetInt(AUTH_TYPE_KEY, 0); set => PlayerPrefs.SetInt(AUTH_TYPE_KEY, (int) value); }

        public string CustomId
        {
            get
            {
                string id = PlayerPrefs.GetString(CUSTOM_ID_STORE_KEY, "");
                if (string.IsNullOrEmpty(id))
                {
                    id = Ulid.NewUlid().ToString();
                    PlayerPrefs.SetString(CUSTOM_ID_STORE_KEY, id);
                }

                return id;
            }
            set => PlayerPrefs.SetString(CUSTOM_ID_STORE_KEY, value);
        }

        public bool IsCompleteSetupName
        {
            get
            {
                int state = PlayerPrefs.GetInt(COMPLETE_SETUP_NAME_STORE_KEY, 0);
                return state != 0;
            }
            set => PlayerPrefs.SetInt(COMPLETE_SETUP_NAME_STORE_KEY, value ? 1 : 0);
        }

        public void ClearData()
        {
            PlayerPrefs.DeleteKey(LOGIN_REMEMBER_KEY);
            PlayerPrefs.DeleteKey(CUSTOM_ID_STORE_KEY);
        }

        public void Authenticate(EAuthType authType)
        {
            AuthType = authType;
            Authenticate();
        }

        public void Authenticate()
        {
            switch (AuthType)
            {
                case EAuthType.None:
                    OnDisplayAuthentication?.Invoke();
                    break;
                case EAuthType.Silent:
                    SilentlyAuthenticate();
                    break;
                case EAuthType.UsernameAndPassword:

                    break;
                case EAuthType.EmailAndPassword:
                    AuthenticateEmailPassword();
                    break;
                case EAuthType.RegisterPlayFabAccount:
                    AddAccountAndPassword();
                    break;
                case EAuthType.Facebook:
                    AuthenticateFacebook();
                    break;
                case EAuthType.Google:
                    AuthenticateGooglePlayGames();
                    break;
                case EAuthType.Apple:
#if UNITY_IOS
                    AttemptQuickLoginApple();
#endif
                    break;
            }
        }

        private void SilentlyAuthenticate(Action<LoginResult> onSuccess = null, Action<PlayFabError> onError = null)
        {
            if (ServiceSettings.UseCustomIdAsDefault)
            {
                LoginWithCustomId(onSuccess);
            }
            else
            {
#if UNITY_ANDROID && !UNITY_EDITOR
            PlayFabClientAPI.LoginWithAndroidDeviceID(new LoginWithAndroidDeviceIDRequest()
                {
                    TitleId = PlayFabSettings.TitleId,
                    AndroidDevice = SystemInfo.deviceModel,
                    OS = SystemInfo.operatingSystem,
                    AndroidDeviceId = CustomId,
                    CreateAccount = true,
                    InfoRequestParameters = infoRequestParams
                },
                result =>
                {
                    SetResultInfo(result);

                    if (onSuccess == null && OnLoginSuccess != null)
                    {
                        OnLoginSuccess.Invoke(result);
                    }
                    else
                    {
                        onSuccess?.Invoke(result);
                    }
                },
                error =>
                {
                    isLoggedIn = false;
                    isRequestCompleted = true;
                    if (onSuccess == null && OnLoginError != null)
                    {
                        OnLoginError.Invoke(error);
                    }
                    else
                    {
                        //make sure the loop completes, callback with null
                        onSuccess?.Invoke(null);
                        Debug.LogError(error.GenerateErrorReport());
                    }
                });
#elif (UNITY_IPHONE || UNITY_IOS) && !UNITY_EDITOR
            PlayFabClientAPI.LoginWithIOSDeviceID(new LoginWithIOSDeviceIDRequest()
                {
                    TitleId = PlayFabSettings.TitleId,
                    DeviceModel = SystemInfo.deviceModel,
                    OS = SystemInfo.operatingSystem,
                    DeviceId = CustomId,
                    CreateAccount = true,
                    InfoRequestParameters = infoRequestParams
                },
                result =>
                {
                    SetResultInfo(result);

                    if (onSuccess == null && OnLoginSuccess != null)
                    {
                        OnLoginSuccess.Invoke(result);
                    }
                    else
                    {
                        onSuccess?.Invoke(result);
                    }
                },
                error =>
                {
                    isLoggedIn = false;
                    isRequestCompleted = true;
                    if (onSuccess == null && OnLoginError != null)
                    {
                        OnLoginError.Invoke(error);
                    }
                    else
                    {
                        //make sure the loop completes, callback with null
                        onSuccess?.Invoke(null);
                        Debug.LogError(error.GenerateErrorReport());
                    }
                });
#else
                LoginWithCustomId(onSuccess);
#endif
            }
        }

        private void LoginWithCustomId(Action<LoginResult> onSuccess)
        {
            PlayFabClientAPI.LoginWithCustomID(
                new LoginWithCustomIDRequest {TitleId = PlayFabSettings.TitleId, CustomId = CustomId, CreateAccount = true, InfoRequestParameters = infoRequestParams},
                result =>
                {
                    SetResultInfo(result);

                    if (onSuccess == null && OnLoginSuccess != null)
                    {
                        OnLoginSuccess.Invoke(result);
                    }
                    else
                    {
                        onSuccess?.Invoke(result);
                    }
                },
                error =>
                {
                    SetErrorInfo();
                    if (onSuccess == null && OnLoginError != null)
                    {
                        OnLoginError.Invoke(error);
                    }
                    else
                    {
                        //make sure the loop completes, callback with null
                        onSuccess?.Invoke(null);
                        Debug.LogError(error.GenerateErrorReport());
                    }
                });
        }

        /// <summary>
        /// Authenticate a user in PlayFab using an Email & Password
        /// </summary>
        private void AuthenticateEmailPassword()
        {
            //Check if the users has opted to be remembered.
            if (RememberMe && string.IsNullOrEmpty(CustomId))
            {
                PlayFabClientAPI.LoginWithCustomID(new LoginWithCustomIDRequest
                    {
                        TitleId = PlayFabSettings.TitleId, CustomId = CustomId, CreateAccount = true, InfoRequestParameters = infoRequestParams
                    },
                    result =>
                    {
                        SetResultInfo(result);
                        OnLoginSuccess?.Invoke(result);
                    },
                    error =>
                    {
                        SetErrorInfo();
                        OnLoginError?.Invoke(error);
                    });

                return;
            }

            // If username & password is empty, then do not continue, and Call back to Authentication UI Display 
            if (!RememberMe && string.IsNullOrEmpty(email) && string.IsNullOrEmpty(password))
            {
                OnDisplayAuthentication?.Invoke();
                return;
            }

            //We have not opted for remember me in a previous session, so now we have to login the user with email & password.
            PlayFabClientAPI.LoginWithEmailAddress(new LoginWithEmailAddressRequest
                {
                    TitleId = PlayFabSettings.TitleId, Email = email, Password = password, InfoRequestParameters = infoRequestParams
                },
                result =>
                {
                    SetResultInfo(result);

                    //Note: At this point, they already have an account with PlayFab using a Username (email) & Password
                    //If RememberMe is checked, then generate a new Guid for Login with CustomId.
                    if (RememberMe)
                    {
                        PlayerPrefs.DeleteKey(CUSTOM_ID_STORE_KEY);
                        AuthType = EAuthType.EmailAndPassword;
                        //Fire and forget, but link a custom ID to this PlayFab Account.
                        PlayFabClientAPI.LinkCustomID(new LinkCustomIDRequest {CustomId = CustomId, ForceLink = forceLink}, null, null);
                    }

                    OnLoginSuccess?.Invoke(result);
                },
                error =>
                {
                    SetErrorInfo();
                    OnLoginError?.Invoke(error);
                });
        }

        /// <summary>
        /// Register a user with an Email & Password
        /// Note: We are not using the RegisterPlayFab API
        /// </summary>
        private void AddAccountAndPassword()
        {
            //Any time we attempt to register a player, first silently authenticate the player.
            //This will retain the players True Origination (Android, iOS, Desktop)
            SilentlyAuthenticate((result) =>
            {
                if (result == null)
                {
                    //something went wrong with Silent Authentication, Check the debug console.
                    OnLoginError?.Invoke(new PlayFabError() {Error = PlayFabErrorCode.UnknownError, ErrorMessage = "Silent Authentication by device failed"});
                }

                //Note: If silent auth is success, which is should always be and the following 
                //below code fails because of some error returned by the server ( like invalid email or bad password )
                //this is okay, because the next attempt will still use the same silent account that was already created.

                //Now add our username & password.
                PlayFabClientAPI.AddUsernamePassword(new AddUsernamePasswordRequest()
                    {
                        Username = !string.IsNullOrEmpty(userName) ? userName : CustomId, //Because it is required & Unique and not supplied by User.
                        Email = email,
                        Password = password,
                    },
                    _ =>
                    {
                        if (OnLoginSuccess != null)
                        {
                            //Store identity and session
                            SetResultInfo(result);

                            //If they opted to be remembered on next login.
                            if (RememberMe)
                            {
                                //Generate a new Guid 
                                PlayerPrefs.DeleteKey(CUSTOM_ID_STORE_KEY);
                                //Fire and forget, but link the custom ID to this PlayFab Account.
                                PlayFabClientAPI.LinkCustomID(new LinkCustomIDRequest() {CustomId = CustomId, ForceLink = forceLink}, null, null);
                            }

                            //Override the auth type to ensure next login is using this auth type.
                            AuthType = EAuthType.EmailAndPassword;

                            //Report login result back to subscriber.
                            OnLoginSuccess.Invoke(result);
                        }
                    },
                    error =>
                    {
                        SetErrorInfo();
                        //Report error result back to subscriber
                        OnLoginError?.Invoke(error);
                    });
            });
        }

        private void AuthenticateFacebook()
        {
#if FACEBOOK
        if (FB.IsInitialized && FB.IsLoggedIn && !string.IsNullOrEmpty(authTicket))
        {
            PlayFabClientAPI.LoginWithFacebook(new LoginWithFacebookRequest()
            {
                TitleId = PlayFabSettings.TitleId,
                AccessToken = authTicket,
                CreateAccount = true,
                InfoRequestParameters = infoRequestParams
            }, result =>
            {
                //Store Identity and session
                SetResultInfo(result);

                //check if we want to get this callback directly or send to event subscribers.
                //report login result back to the subscriber
                OnLoginSuccess?.Invoke(result);
            }, error =>
            {
                SetErrorInfo();
                //report errro back to the subscriber
                OnLoginError?.Invoke(error);
            });
        }
        else
        {
            OnDisplayAuthentication?.Invoke();
        }
#endif
        }

        private void AuthenticateGooglePlayGames()
        {
#if GOOGLEGAMES
        PlayFabClientAPI.LoginWithGoogleAccount(new LoginWithGoogleAccountRequest()
        {
            TitleId = PlayFabSettings.TitleId,
            ServerAuthCode = authTicket,
            InfoRequestParameters = infoRequestParams,
            CreateAccount = true
        }, (result) =>
        {
            //Store Identity and session
            SetResultInfo(result);

            //check if we want to get this callback directly or send to event subscribers.
            //report login result back to the subscriber
            OnLoginSuccess?.Invoke(result);
        }, (error) =>
        {
            SetErrorInfo();
            //report errro back to the subscriber
            OnLoginError?.Invoke(error);
        });
#endif
        }

        private void SetResultInfo(LoginResult result)
        {
            PlayFabId = result.PlayFabId;
            SessionTicket = result.SessionTicket;
            isLoggedIn = true;
            isRequestCompleted = true;
        }

        private void SetErrorInfo()
        {
            isLoggedIn = false;
            isRequestCompleted = true;
        }

        public void UnlinkSilentAuth()
        {
            SilentlyAuthenticate(result =>
            {
                if (ServiceSettings.UseCustomIdAsDefault)
                {
                    PlayFabClientAPI.UnlinkCustomID(new UnlinkCustomIDRequest {CustomId = CustomId}, null, null);
                }
                else
                {
#if UNITY_ANDROID && !UNITY_EDITOR
                //Fire and forget, unlink this android device.
                PlayFabClientAPI.UnlinkAndroidDeviceID(new UnlinkAndroidDeviceIDRequest() {AndroidDeviceId = CustomId}, null, null);

#elif (UNITY_IPHONE || UNITY_IOS) && !UNITY_EDITOR
                PlayFabClientAPI.UnlinkIOSDeviceID(new UnlinkIOSDeviceIDRequest() {DeviceId = CustomId}, null, null);
#else
                    PlayFabClientAPI.UnlinkCustomID(new UnlinkCustomIDRequest {CustomId = CustomId}, null, null);
#endif
                }
            });
        }

        public void Reset()
        {
            isLoggedIn = false;
            isRequestCompleted = false;
        }

        /// <summary>
        /// require enable put static score in setting dashboard
        /// auto push score to country table
        /// </summary>
        /// <param name="value"></param>
        /// <param name="nameTable"></param>
        public static void UpdatePlayerStatistics(string nameTable, int value)
        {
            PlayFabClientAPI.UpdatePlayerStatistics(
                new UpdatePlayerStatisticsRequest
                {
                    Statistics = new List<StatisticUpdate>
                    {
                        new() {StatisticName = nameTable, Value = value}, new() {StatisticName = $"{nameTable}_{LoginResultModel.countryCode}", Value = value}
                    },
                },
                result => { OnUpdatePlayerStatisticsSuccess?.Invoke(result); },
                error => { OnUpdatePlayerStatisticsError?.Invoke(error); });
        }

#if UNITY_IOS
        /// <summary>
        /// use for apple login
        /// </summary>
        private void AttemptQuickLoginApple()
        {
            var quickLoginArgs = new AppleAuthQuickLoginArgs();

            appleAuthManager.QuickLogin(quickLoginArgs, OnAppleLoginSuccess, OnAppleLoginFail);
        }

        private void OnAppleLoginSuccess(ICredential credential)
        {
            if (credential is IAppleIDCredential appleIdCredential)
            {
                PlayerPrefs.SetString(APPLE_USER_ID, appleIdCredential.User);
                identityToken = appleIdCredential.IdentityToken;

                PlayFabClientAPI.LoginWithApple(new LoginWithAppleRequest()
                    {
                        TitleId = PlayFabSettings.TitleId,
                        IdentityToken = System.Text.Encoding.UTF8.GetString(identityToken),
                        CreateAccount = true,
                        InfoRequestParameters = infoRequestParams
                    },
                    result =>
                    {
                        //Store Identity and session
                        SetResultInfo(result);

                        //check if we want to get this callback directly or send to event subscribers.
                        //report login result back to the subscriber
                        OnLoginSuccess?.Invoke(result);
                    },
                    error =>
                    {
                        //report errro back to the subscriber
                        OnLoginError?.Invoke(error);
                    });
            }
        }

        private static void OnAppleLoginFail(IAppleError error) { Debug.LogWarning("[Login Apple]: failed by " + error); }
#endif

        public static void UpdateUserTitleDisplayName(string name)
        {
            PlayFabClientAPI.UpdateUserTitleDisplayName(new UpdateUserTitleDisplayNameRequest {DisplayName = name},
                result => { OnUpdateUserTitleDisplayNameSuccess?.Invoke(result); },
                error => { OnUpdateUserTitleDisplayNameError?.Invoke(error); });
        }

        public static void RequestLeaderboard(
            string nameTable,
            Action<GetLeaderboardResult> callbackResult,
            Action<PlayFabError> callbackError,
            int startPosition = 0,
            int maxResultQuery = 100)
        {
            PlayFabClientAPI.GetLeaderboard(new GetLeaderboardRequest
                {
                    StatisticName = nameTable,
                    StartPosition = startPosition,
                    MaxResultsCount = maxResultQuery,
                    ProfileConstraints = new PlayerProfileViewConstraints() {ShowDisplayName = true, ShowLocations = true,}
                },
                callbackResult,
                callbackError);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <param name="permission"></param>
        /// <param name="resultCallback"></param>
        /// <param name="errorCallback"></param>
        public static void UpdateUserData<T>(
            string key,
            T value,
            UserDataPermission permission,
            Action<UpdateUserDataResult> resultCallback,
            Action<PlayFabError> errorCallback)
        {
            var changes = new Dictionary<string, string>() {{key, PlayFabSimpleJson.SerializeObject(value)}};

            PlayFabClientAPI.UpdateUserData(new UpdateUserDataRequest {Data = changes, Permission = permission}, resultCallback, errorCallback);
        }

        public static void GetUserData(Action<GetUserDataResult> resultCallback, Action<PlayFabError> errorCallback)
        {
            PlayFabClientAPI.GetUserData(new GetUserDataRequest() { }, resultCallback, errorCallback);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="key"></param>
        /// <param name="resultCallback"></param>
        /// <param name="errorCallback"></param>
        /// <typeparam name="T"></typeparam>
        public static async UniTask<T> GetUserData<T>(string key, Action<T> resultCallback = null, Action<PlayFabError> errorCallback = null) where T : class
        {
            T t = default;
            bool flag = false;
            PlayFabClientAPI.GetUserData(new GetUserDataRequest() { },
                result =>
                {
                    flag = true;
                    t = PlayFabSimpleJson.DeserializeObject<T>(result.Data[key].Value);
                    resultCallback?.Invoke(t);
                },
                errorCallback);
            await UniTask.WaitUntil(() => flag);
            return t;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="key"></param>
        /// <param name="resultCallback"></param>
        /// <param name="errorCallback"></param>
        /// <typeparam name="T"></typeparam>
        public static async UniTask<T> GetUserData<T>(string userId, string key, Action<T> resultCallback = null, Action<PlayFabError> errorCallback = null)
            where T : class
        {
            T t = default;
            bool flag = false;
            PlayFabClientAPI.GetUserData(new GetUserDataRequest() {PlayFabId = userId, Keys = new List<string> {key}},
                result =>
                {
                    flag = true;
                    if (result.Data.ContainsKey(key))
                    {
                        t = PlayFabSimpleJson.DeserializeObject<T>(result.Data[key].Value);
                        resultCallback?.Invoke(t);
                    }
                    else
                    {
                        Debug.Log($"[PlayFab] Can not found {key} in user data");
                    }
                },
                errorCallback);
            await UniTask.WaitUntil(() => flag);
            return t;
        }
#if ENABLE_PLAYFABADMIN_API
        public static void GetStatistic(
            string playerId,
            string nameTable,
            Action<GetLeaderboardAroundUserResult> onGetLeaderboardAroundUserSuccess,
            Action<PlayFabError> onGetLeaderboardAroundUserError)
        {
            PlayFabServerAPI.GetLeaderboardAroundUser(new GetLeaderboardAroundUserRequest() {PlayFabId = playerId, MaxResultsCount = 1, StatisticName = nameTable},
                onGetLeaderboardAroundUserSuccess,
                onGetLeaderboardAroundUserError);
        }
#endif

        /// <summary>
        /// use to link current account to facebook
        /// </summary>
        public static void LinkFacebook(string token, Action<LinkFacebookAccountResult> onLinkCompleted, Action<PlayFabError> onLinkError)
        {
            PlayFabClientAPI.LinkFacebookAccount(new LinkFacebookAccountRequest() {AccessToken = token}, onLinkCompleted, onLinkError);
        }

        public static void GetPlayFabIDsFromFacebook(List<string> facebookIDs, Action<GetPlayFabIDsFromFacebookIDsResult> onCompleted, Action<PlayFabError> onError)
        {
            PlayFabClientAPI.GetPlayFabIDsFromFacebookIDs(new GetPlayFabIDsFromFacebookIDsRequest() {FacebookIDs = facebookIDs}, onCompleted, onError);
        }
    }
}
#endif