# What

- The heart of the world tree

# Environment

- unity 2021.3.8f1
- scriptingBackend : IL2CPP
- apiCompatibilityLevel : .NetFramework

# How To Install

Add the lines below to `Packages/manifest.json`

- for dev version

```csharp
"com.pancake.heart": "https://github.com/pancake-llc/heart.git",
```

- for version `1.2.1`

```csharp
"com.pancake.heart": "https://github.com/pancake-llc/heart.git#1.2.1",
```

# Usages

Summary

- [Anti Singleton](#anti-singleton)
- [Level Editor](#level-editor)
- [Notification](#notification)
- [Loading Scene](#loading-scene)
- [Timer](#timer)
- [SimpleJson](#simplejson)
- [Linq](#linq)
- [Facebook](#facebook)
- [Playfab](#playfab)
- [IAP](#iap)
- [Ads](#advertisement)
- [Tween](#tween)
- [Feedback](#feedback)

## ANTI SINGLETON

```csharp
/// <summary>
/// I don't want to use singleton as a pattern outside internal
/// so no base class singleton was created
/// </summary>

/// <summary>
/// Singleton is programming pattern uses a single globally-accessible
/// instance of a class, avaiable at all time.
///
/// This is useful to make global manager that hold variables
/// and functions that are globally accessible
///
/// achieve a persistent state across multiple scenes and are fast to implement
/// with a smaller project, this approach can be usefull
///
/// When we're referencing the instance of a singleton class from another script
/// we're creating a dependency between these two classes
/// </summary> 
```

## LEVEL EDITOR

<details>
<summary>Provide workspace space to drop and drag prefab to easy make level</summary>

<p style="text-align: center;">
  <img src="https://user-images.githubusercontent.com/44673303/198231314-85dedbcd-d962-4e94-9eac-50a7e0d56d89.png" width="600"  alt=""/>
</p>

<p style="text-align: center;">
  <img src="https://user-images.githubusercontent.com/44673303/198229730-1b1fd11d-0b91-422b-a176-c27a72491fb7.png" width="600"  alt=""/>
</p>

### _DROP AREA_

1. White List : Contains a list of links to list all the prefabs you can choose from in the PickUp Area
2. Black List : Contains a list of links to list all prefabs that won't show up in the PickUp Area
3. Using `Right Click` in `White List Area` or `Black List Area` to clear all `White List` or `Black List`

### _SETTING_

1. Where Spawn :
    1. Default:
        1. New instantiate object will spawn in root prefab when you in prefab mode
        2. New instantiate object will spawn in world space when you in scene mode

    2. Index: The newly created object is the child of the object with the specified index of root
        1. `This mode only works inside PrefabMode`
    3. Custom: You can choose to use the object as the root to spawn a new object here

### _PICKUP AREA_

<p style="text-align: center;">
  <img src="https://user-images.githubusercontent.com/44673303/198229738-f9b1737c-dcd0-4629-bb0e-696c9d1e713f.png" width="600"  alt=""/>
</p>

Where you choose the object to spawn

+ Using `Shift + Click` to instantiate object
+ Using `Right Click` in item to ping object prefab
+ Using `Right Click` in header Pickup Area to refresh draw collection item pickup

Right click to header of tab to refresh pickup object in tab area

<p style="text-align: center;">
  <img src="https://user-images.githubusercontent.com/44673303/163969707-bc0beca6-2952-414f-8732-e1e4bcbaa630.png" width="600"  alt=""/>
</p>

Right click to specifically pickup object to show menu

+ Ignore: Mark this pickup object on the `black list`
+ Ping: Live property locator see where it is

<p style="text-align: center;">
  <img src="https://user-images.githubusercontent.com/44673303/198229743-c8f0177c-7d97-466e-ab1f-861daa936a79.png" width="600"  alt=""/>
</p>

</details>

## NOTIFICATION

<details>
<summary>Simple api to push notification in game</summary>

- Add component `NotificationConsole` into object has dont destroy to reschedule each time go to background and cancel when back to forceground

- Type `Repeat` will fire notification after each number `Minute`

<p style="text-align: center;">
  <img src="https://cdn.jsdelivr.net/npm/yenmoc-assets@1.0.17/img/unity_notification.jpg" width="600"  alt=""/>
</p>

- Event `OnUpdateDeliveryTime` to use when you want to send notification one time with diffirent custom `Minute` each time such as fire notification in game idle when building
  house completed. In case you need write your custom method
  to assign to event by call to API

```csharp
public void UpdateDeliveryTimeBy(string id, int customTimeSchedule = -1){}
public void UpdateDeliveryTimeBy(int index, int customTimeSchedule = -1){}
public void UpdateDeliveryTimeByIncremental(string id, int indexData, int customTimeSchedule = -1){}
public void UpdateDeliveryTimeByIncremental(int index, int indexData, int customTimeSchedule = -1){}
```

*Note :

- Version 2+ require minimum android api support is 22

</details>

## LOADING SCENE

<details>
<summary>Provide best loading scene synchronization</summary>

<p style="text-align: center;">
  <img src="https://cdn.jsdelivr.net/npm/yenmoc-assets@1.0.19/img/loading-component2.png" width="600"  alt=""/>
</p>

- Add component `Loading` into GameObject to start handle loading scene. You can you method `LoadScene` inside `Loading` to switch scene
- There are two overide for method `LoadingScene`

```csharp
public void LoadScene(string sceneName, Func<bool> funcWaiting = null, Action prepareActiveScene = null){}
public void LoadScene(string sceneName, string subScene, Func<bool> funcWaiting = null, Action prepareActiveScene = null){}
```

- You can customize the Loading Scene prefab by creating a prefab variant via the menu item below. Then, you can use it by selecting via `Selected Template` in `Loading`

<p style="text-align: center;">
  <img src="https://cdn.jsdelivr.net/npm/yenmoc-assets@1.0.19/img/create-loading-prefab2.png" width="600"  alt=""/>
</p>

</details>

## TIMER

<details>
<summary>Powerful and convenient library for running actions after a delay in Unity3D.</summary>

`Timer` provides the following method for creating timers:

```c#
using Pancake;

/// <summary>
/// Register a new timer that should fire an event after a certain amount of time
/// has elapsed.
/// </summary>
/// <param name="duration">The time to wait before the timer should fire, in seconds.</param>
/// <param name="onComplete">An action to fire when the timer completes.</param>
public static Timer Register(float duration, Action onComplete);
```

The method is called like this:

```c#
// Log "Hello World" after five seconds.

Timer.Register(5f, () => Debug.Log("Hello World"));
```

### Motivation

Out of the box, without this library, there are two main ways of handling timers in Unity:

1. Use a coroutine with the WaitForSeconds method.
2. Store the time that your timer started in a private variable (e.g. `startTime = Time.time`), then check in an Update call if `Time.time - startTime >= timerDuration`.

The first method is verbose, forcing you to refactor your code to use IEnumerator functions. Furthermore, it necessitates having access to a MonoBehaviour instance to start the
coroutine, meaning that solution will not work in non-MonoBehaviour classes. Finally, there is no way to prevent WaitForSeconds from being affected by changes to
the [time scale](http://docs.unity3d.com/ScriptReference/Time-timeScale.html).

The second method is error-prone, and hides away the actual game logic that you are trying to express.

`Timer` alleviates both of these concerns, making it easy to add an easy-to-read, expressive timer to any class in your Unity project.

### Features

**Make a timer repeat by setting `isLooped` to true.**

```c#
// Call the player's jump method every two seconds.

Timer.Register(2f, player.Jump, isLooped: true);
```

**Cancel a timer after calling it.**

```c#
Timer timer;

void Start() {
   timer = Timer.Register(2f, () => Debug.Log("You won't see this text if you press X."));
}

void Update() {
   if (Input.GetKeyDown(KeyCode.X)) {
      Timer.Cancel(timer);
   }
}
```

**Measure time by [realtimeSinceStartup](http://docs.unity3d.com/ScriptReference/Time-realtimeSinceStartup.html) instead of scaled game time by setting `useRealTime` to true.**

```c#
// Let's say you pause your game by setting the timescale to 0.
Time.timeScale = 0f;

// ...Then set useRealTime so this timer will still fire even though the game time isn't progressing.
Timer.Register(1f, this.HandlePausedGameState, useRealTime: true);
```

**Attach the timer to a MonoBehaviour so that the timer is destroyed when the MonoBehaviour is.**

Very often, a timer called from a MonoBehaviour will manipulate that behaviour's state. Thus, it is common practice to cancel the timer in the OnDestroy method of the
MonoBehaviour. We've added a convenient extension method that attaches a Timer to a MonoBehaviour such that it will automatically cancel the timer when the MonoBehaviour is
detected as null.

```c#
public class CoolMonoBehaviour : MonoBehaviour {

   void Start() {
      // Use the AttachTimer extension method to create a timer that is destroyed when this
      // object is destroyed.
      this.AttachTimer(5f, () => {
      
         // If this code runs after the object is destroyed, a null reference will be thrown,
         // which could corrupt game state.
         this.gameObject.transform.position = Vector3.zero;
      });
   }
   
   void Update() {
      // This code could destroy the object at any time!
      if (Input.GetKeyDown(KeyCode.X)) {
         GameObject.Destroy(this.gameObject);
      }
   }
}
```

**Update a value gradually over time using the `onUpdate` callback.**

```c#
// Change a color from white to red over the course of five seconds.
Color color = Color.white;
float transitionDuration = 5f;

Timer.Register(transitionDuration,
   onUpdate: secondsElapsed => color.r = 255 * (secondsElapsed / transitionDuration),
   onComplete: () => Debug.Log("Color is now red"));
```

**A number of other useful features are included!**

- timer.Pause()
- timer.Resume()
- timer.GetTimeRemaining()
- timer.GetRatioComplete()
- timer.isDone

A test scene + script demoing all the features is included with the package in the `Timer/Example` folder.

### Usage Notes / Caveats

1. All timers are destroyed when changing scenes. This behaviour is typically desired, and it happens because timers are updated by a `TimerController` that is also destroyed when
   the scene changes. Note that as a result of this, creating a `Timer` when the scene is being closed, e.g. in an object's `OnDestroy` method, will result in a Unity error when
   the
   `TimerController` is spawned (test this on unity 2021+ but no error is thrown)

<p style="text-align: center;">
  <img src="https://i.imgur.com/ESFmFDO.png" width="600"  alt=""/>
</p>

</details>

## SimpleJSON

<details>
<summary>A simple JSON parser in C# </summary>

- Serialize

```c#
public void Serialize(T data, Stream writer)
{
	var jsonNode = JSON.Parse(JsonUtility.ToJson(data));
	jsonNode.SaveToBinaryStream(writer);
}
```

- Deserialize

```c#
public T Deserialize(Stream reader)
{
	var jsonData = JSONNode.LoadFromBinaryStream(reader);
	string json = jsonData.ToString()
    // ... 
}
```

```c#
var jsonNode = JSON.Parse(jsonString);
if (jsonNode == null) return jsonString;

object jsonObject = jsonNode;
var version = jsonNode["version"].AsInt;
```

</details>

## Linq

<details>
<summary>High performance Linq for Unity</summary>

Improved performance when using Linq with Mobile (il2cpp).
To use it instead of using **System.Linq**, change it to **Pancake.Linq**

It will be a little different from System Linq that **Select** is replaced with **Map**, and **Where** is changed to **Filter**

#### Result test in Android v8 snapdradon 855

|                | System.Linq (ms) | Pancake.Linq (ms) |
|----------------|------------------|-------------------|
| Aggregate      | 2890             | 140               |
| Any            | 2986             | 159               |
| All            | 3167             | 159               |
| Averange       | 3042             | 106               |
| Contains       | 10048            | 133               |
| Count          | 3227             | 186               |
| Distinct       | 4578             | 8449              |
| First          | 311              | 18                |
| Last           | 3118             | 0                 |
| Max            | 1283             | 377               |
| Min            | 1233             | 398               |
| OrderBy        | 16221            | 15136             |
| Range          | 314              | 61                |
| Repeat         | 593              | 198               |
| Reverse        | 3607             | 456               |
| Select (Map)   | 3925             | 629               |
| Single         | 3264             | 185               |
| Skip           | 10384            | 432               |
| Sum            | 3055             | 79                |
| Take           | 1365             | 67                |
| Where (Filter) | 2267             | 1054              |
| Where2         | 233              | 615               |
| Where3         | 3842             | 622               |
| WhereSelect    | 4372             | 810               |
| WhereSpan      | 655              | 674               |
| Zip            | 17219            | 580               |

#### 1000 loop in array 10k element (anroid 11 redmi note 10 pro)

|                | System.Linq (ms) | Pancake.Linq (ms) |
|----------------|------------------|-------------------|
| Aggregate      | 344              | 30                |
| Any            | 0                | 0                 |
| All            | 0                | 0                 |
| Averange       | 329              | 17                |
| Contains       | 0                | 1                 |
| Count          | 362              | 28                |
| First          | 0                | 0                 |
| Last           | 350              | 0                 |
| Max            | 228              | 70                |
| Min            | 412              | 8                 |
| Range          | 40               | 6                 |
| Repeat         | 68               | 21                |
| Reverse        | 595              | 64                |
| Select (Map)   | 508              | 77                |
| Skip           | 515              | 32                |
| Sum            | 387              | 79                |
| Take           | 437              | 32                |
| Where (Filter) | 276              | 134               |
| WhereSelect    | 263              | 144               |
| Zip            | 1397             | 88                |

</details>

## FACEBOOK

<details>
<summary>Simple Facebook manager</summary>

Require install [facebook](https://github.com/pancake-llc/facebook)

### Friend Facebook

- Facebook application need create with type is `gaming`
- If permission `gaming_user_picture` not include will return avartar, if include it will return profile picture

```cs
    public Image prefab;
    public Transform root;
    private async void Start()
    {
        if (FacebookManager.Instance.IsLoggedIn)
        {
            FacebookManager.Instance.GetMeProfile(FacebookManager.Instance.OnGetProfilePhotoCompleted);

            await UniTask.WaitUntil(() => !FacebookManager.Instance.IsRequestingProfile);
            var o = Instantiate(prefab, root);
            o.sprite = FacebookManager.CreateSprite(FacebookManager.Instance.ProfilePicture, Vector2.one * 0.5f);
        }
    }
    

    public void Login() { FacebookManager.Instance.Login(OnLoginCompleted, OnLoginFaild, OnLoginError); }

    private void OnLoginError() { }

    private void OnLoginFaild() { }

    private async void OnLoginCompleted()
    {
        await UniTask.WaitUntil(() => !FacebookManager.Instance.IsRequestingProfile);
        var o = Instantiate(prefab, root);
        o.sprite = FacebookManager.CreateSprite(FacebookManager.Instance.ProfilePicture, Vector2.one * 0.5f);
        
        FacebookManager.Instance.GetMeFriend();

        await UniTask.WaitUntil(() => !FacebookManager.Instance.IsRequestingFriend);
        var p = FacebookManager.Instance.LoadProfileAllFriend();
        await p;
        for (int i = 0; i < FacebookManager.Instance.FriendDatas.Count; i++)
        {
            var result = Instantiate(prefab, root);
            Debug.Log("friend : "  + FacebookManager.Instance.FriendDatas[i].name);
            result.sprite = FacebookManager.CreateSprite(FacebookManager.Instance.FriendDatas[i].avatar, Vector2.one * 0.5f);
        }
    }
```

</details>

## PLAYFAB

<details>
<summary>Playfab simple Auth</summary>

### LEADERBOARD

- install package [playfab](https://github.com/pancake-llc/playfab)
- install package [ios login](https://github.com/lupidan/apple-signin-unity) (optional if you build for ios platform)
- config setting via menu item `Tool/Pancake/Playfab`
  ![image](https://user-images.githubusercontent.com/44673303/193963879-16e7337d-3ebe-42b2-a700-feff49f1f1b0.png)
- in tab [API Features] enable `Allow client to post player statistics`
- in tab [Client Profile Options]
    - in `Allow client access to profile properties` enable `Display Name`, `Locations`, `Statistics`
    - in `Allow client access to sensitive profile properties` enable `Linked accounts`
      <img width="947" alt="client profile options in playfab title setting" src="https://user-images.githubusercontent.com/44673303/200122264-c5536d05-98c6-411b-b204-1342d65d196b.png">

- install sample leaderboard via PackageManager, sample need install pacakge [ui effect](https://github.com/mob-sakai/UIEffect.git) to run correctly
- in sample leaderboard has already file config, select `GameServiceSettings` in folder resources to active setting
- use `Update Aggregation` menu in context menu of PopupLeaderboard to create table leaderboard for 240 countries just do this once
- replace the code in `#if region replace your code` with your own code to manage popups the way you want
- for update score to leaderboard when first time you enter name complete. You can via using `valueExpression` in `ButtonLeaderBoard`

```c#
GetComponent<ButtonLeaderboard>().valueExpression += () => UnityEngine.Random.Range(1, 100);
```

</details>

## IAP

<details>
<summary>Easy IAP in Unity</summary>

### _SETTING_

1. Auto Init : Always true, when game starting IAPManager auto initialize

2. Skus : List of product id
    - Id : Default id use when override mark false
    - Android Id : Product id use for android platfom when override mark true
    - iOS Id : Product id use for ios platfom when override mark true
    - Product Type:
        - Consumable : (pay everytime)
          A consumable In-App Purchase must be purchased every time the user downloads it. One-time services, such as fish food in a fishing app, are usually implemented as
          consumables.
        - Non-Consumable : (one time payment)
          Non-consumable In-App Purchases only need to be purchased once by users. Services that do not expire or decrease with use are usually implemented as non-consumables, such
          as new race tracks for a game app.
        - Subscriptions : (will deduct money from your credit card on a cycle complete)
          Subscriptions allow the user to purchase updating and dynamic content for a set duration of time. Subscriptions renew automatically unless the user opts out, such as
          magazine subscriptions.

3. Button Generate Script:
    - Generate script contains all method purchase for all skus definition in IapSettings.asset
    - Then you can call method inside ProductImpl class to make specific item purchase

```c#
Product.PurchaseRemoveads(); // ex call purchase remove ads item
```

4. You need to attach your custom event callback (purchase success and purchase faild) manual by following way, IAPManager is initialized automatically so don't worry about null
   error

```c#

        public static void Init()
        {
            IAPManager.OnPurchaseSucceedEvent += YourHandlePurchaseSucceedEvent;
            IAPManager.OnPurchaseFailedEvent += YourHandlePurchaseFailedEvent;
        }

        private static void YourHandlePurchaseFailedEvent(string productId)
        {
            switch (productId)
            {
                case "com.larnten.removeads":
                    break;
                // TO_DO
            }
        }

        private static void YourHandlePurchaseSucceedEvent(string productId)
        {
            // TO_DO
        }

```

- or you can use chain method to handle callback purchase success

```c#
        private static void OnButtonRemoveAdsClicked()
        {
            Product.PurchaseRemoveads()
                .OnCompleted(() =>
                {
                    Debug.Log("Remove Ad Completed!");
                    // TO_DO
                });
        }
```

</details>

## Advertisement

<details>
<summary>Easy Advertising in Unity</summary>

![1](https://user-images.githubusercontent.com/44673303/161428593-fce3bccd-e05c-435f-b482-7f3a3a68b2ef.png)

### _BASIC_

1. Auto Init :
    1. `true` if you want Adverstising to automatically initialize setting at `Start()`
    2. `false` you need to call `Advertising.Initialize()` manually where you want
    3. `Advertising.Initialize()` is required to use other Adverstising APIs

2. [GDPR](https://developers.google.com/admob/unity/eu-consent) : General Data Protection Regulation
    - Under the Google EU User Consent Policy, you must make certain disclosures to your users in the European Economic Area (EEA) and obtain their consent to use cookies or other
      local storage, where legally required, and to use personal data (such as AdID) to serve ads. This policy reflects the requirements of the EU ePrivacy Directive and the
      General Data Protection Regulation (GDPR)

    1. `true` the consent popup will be displayed automatically as soon as `GoogleMobileAds Initialize` is successful if you use Admob for show ad, or `MaxSdk.InitializeSdk()`
       initalize completed when you use `max` for show ad
    2. `false` nothing happened
    3. you can call manual consent form by
    ```c#
        if (!GDPRHelper.CheckStatusGDPR())
        {
            Advertising.ShowConsentFrom();
        }
    ```

- Note:
    - You can also call manually by calling through `Advertising.ShowConsentForm()`
    - On android it will show consent form popup,
    - On ios it will show ATT popup

3. Multi Dex:
    - enable multi dex to fix build gradle error

4. Current Network:
    - the ad network currently used to display ads

6. Privacy & Policy : displayed to edit when GDPR enabled
    - the link to the website containing your privacy policy information

### _AUTO AD-LOADING_

1. Auto Ad-Loading Mode
    1. All : auto load `interstitial ad`, `rewarded ad`, `rewarded interstitial ad`, `app open ad`
2. Ad Checking Interval
    1. ad availability check time. ex: `Ad Checking Interval = 8` checking load ad after each 8 second
3. Ad Loading Interval
    1. time between 2 ad loads. ex: `Ad Loading Interval = 15` the next call to load the ad must be 15 seconds after the previous one

### _ADMOB_

![2](https://user-images.githubusercontent.com/44673303/157592895-32e01024-3de7-41f4-8823-9a9b996371f2.png)

1. BannerAd:
    1. when size banner is SmartBanner you can choose option use Apdaptive Banner

### _MAX_

![3](https://user-images.githubusercontent.com/44673303/157606179-7ea14705-175f-4297-bc96-d4516bee50cf.png)

1. Age Restrictd User

    - To ensure COPPA, GDPR, and Google Play policy compliance, you should indicate when a user is a child. If you know that the user is in an age-restricted category (i.e., under
      the age of 16), set the age-restricted user flag to true

    - If you know that the user is not in an age-restricted category (i.e., age 16 or older), set the age-restricted user flag to false

### _Adverstising_

```c#
Advertising.ShowBannerAd()
Advertising.HideBannerAd()
Advertising.DestroyBannerAd()
Advertising.GetAdaptiveBannerHeight()


Advertising.LoadInsterstitialAd()
Advertising.IsInterstitialAdReady()
Advertising.ShowInterstitialAd()


Advertising.LoadRewardedAd()
Advertising.IsRewardedAdReady()
Advertising.ShowRewardedAd()


Advertising.LoadRewardedInterstitialAd()
Advertising.IsRewardedInterstitialAdReady()
Advertising.ShowRewardedInterstitialAd()


Advertising.LoadAppOpenAd()
Advertising.IsAppOpenAdReady()
Advertising.ShowAppOpenAd()


Advertising.ShowConsentFrom()

```

- you can attach your custom event callback by

```c#
Action<EInterstitialAdNetwork> InterstitialAdCompletedEvent; // call when user completed watch interstitialAd


Action<ERewardedAdNetwork> RewardedAdCompletedEvent; // call when user completed receive reward form rewardedAd
Action<ERewardedAdNetwork> RewardedAdSkippedEvent; // call when user skip watching rewardedAd


Action<ERewardedInterstitialAdNetwork> RewardedInterstitialAdCompletedEvent; // call when user completed receive reward form rewardedInterstitialAd
Action<ERewardedInterstitialAdNetwork> RewardedInterstitialAdSkippedEvent; // call when user skip watching rewardedInterstitialAd


Action<EAppOpenAdNetwork> AppOpenAdCompletedEvent; // call when user completed watch appOpenAd
```

### Update current use network

- by default admob will be used to show ad, you can use the following syntax

```c#
Advertising.SetCurrentNetwork("name network");

ex: Advertising.SetCurrentNetwork("applovin");
or: Advertising.SetCurrentNetwork(EAdNetwork.AppLovin);
```

1. "admob"
2. "applovin"
3. "ironsource"

#### Notes

1. [Setting scripting symbols for Editor script compilation](https://docs.unity3d.com/Manual/CustomScriptingSymbols.html)

```text
If you need to define scripting symbols via scripts in the Editor so that your Editor scripts are affected by the change, you must use PlayerSettings.SetScriptingDefineSymbolsForGroup. However, there are some important details to note about how this operates.

Important: this method does not take immediate effect. Calling this method from script does not immediately apply and recompile your scripts. For your directives to take effect based on a change in scripting symbols, you must allow control to be returned to the Editor, where it then asynchronously reloads the scripts and recompiles them based on your new symbols and the directives which act on them.

So, for example, if you use this method in an Editor script, then immediately call BuildPipeline.BuildPlayer on the following line in the same script, at that point Unity is still running your Editor scripts with the old set of scripting symbols, because they have not yet been recompiled with the new symbols. This means if you have Editor scripts which run as part of your BuildPlayer execution, they run with the old scripting symbols and your player might not build as you expected.
```

2. IronSource SDK
    - In case you have successfully imported ironSOurce but Unity Editor still says plugin not found `IronSource plugin not found. Please import it to show ads from IronSource`
      ![image](https://user-images.githubusercontent.com/44673303/161428343-19750d61-b75e-4f37-a532-2a01a3e379e7.png)

      Open ProjectSetting and navigate to Scripting Definition Symbol then remove the line PANCAKE_IRONSOURCE_ENABLE -> wait editor complie and add symbol again
      ![Screenshot_1](https://user-images.githubusercontent.com/44673303/161428348-2e330f02-ca78-4b6d-8f4c-25d539c771b4.png)

3. AppLovin SDK (fixed in version 8.4.1.1)
    - Mediation adapter Chartboost 8.4.1 is crashing and not building on Unity after they updated to Java 11
      ![image (1)](https://user-images.githubusercontent.com/44673303/161477158-1deae20f-ce7c-436a-8e8c-d4c5fe196ed7.png)
    - so you need use old version of Chartboot (8.2.1.0)
```xml
<?xml version="1.0" encoding="utf-8"?>
    <dependencies>
        <androidPackages>
            <androidPackage spec="com.applovin.mediation:chartboost-adapter:8.2.1.0" />
            <androidPackage spec="com.google.android.gms:play-services-base:16.1.0" />
        </androidPackages>
        <iosPods>
            <iosPod name="AppLovinMediationChartboostAdapter" version="8.4.2.0" />
        </iosPods>
    </dependencies>
```

</details>


## TWEEN

<details>
<summary>Simple Tween engine for Unity</summary>

### Delay

- `CallbackTween`, `ResetableCallbackTween`, `WaitTween` can not use `.Delay()`

```csharp
        var sequense = TweenManager.Sequence();
        sequense.Join(transform.TweenPosition(Vector3.one, 1f).OnComplete(() => Debug.Log("DONE POSITION")));
        sequense.Join(transform.TweenLocalScale(Vector3.one, 1f).OnComplete(() => Debug.Log("DONE SCALE")));
        sequense.Append(new WaitTween(5));
        sequense.Append(transform.TweenPosition(Vector3.zero, 1f).OnComplete(() => Debug.Log("DONE POSITION BACK")));
        sequense.Delay(5);
        
        sequense.Play();
```

```csharp
        transform.TweenPosition(Vector3.one, 1f).OnComplete(() => Debug.Log("DONE POSITION")).Delay(5f).Play();
```

### Loop

```csharp
var sequence = TweenManager.Sequence();
        sequence.Append(GetComponent<Image>().TweenColor(Color.red, 1f));
        sequence.SetLoops(10, ResetMode.InitialValues).OnLoop(() => Debug.Log("LOOP SEQUENSE"))
            .OnComplete(()=> Debug.Log("ON COMPLETED!!!"));
        sequence.Play();
```

- for infinite loop pass `-1` as parameter

```csharp
 GetComponent<Image>().TweenColor(Color.red, 1f).SetEase(interpolator).SetLoops(-1, ResetMode.InitialValues).OnLoop(() => Debug.Log("LOOP")).Play();
```

</details>

## FEEDBACK

<details>
<summary>Simple api collect feedback of user and push to trello</summary>

Goto feedback setting ProjectSetting -> Pancake -> Feedback

![Screenshot_4](https://user-images.githubusercontent.com/44673303/202736092-c9f6b1f5-965a-468b-b16d-2da435b864ce.jpg)

1. Get token by click to button `Get Trello API Token` and accept permissions for application and copy token
2. Paste token already copy to `Token`
3. Click button `Authenticate With Token`

![Screenshot_5](https://user-images.githubusercontent.com/44673303/202736080-73b25be4-02c9-4f6a-a53c-db56e8266046.jpg)

4. Create new board and select board using for user push feedback.
5. See demo in package sample in package manager

- Image demo

![Screenshot_3](https://user-images.githubusercontent.com/44673303/202736089-ba77a2e0-e234-4d8f-815e-d09187a4867a.jpg)

</details>

## Ulid

<details>
<summary>Fast .NET C# Implementation of ULID for .NET Core and Unity</summary>

[Fast .NET C# Implementation of ULID for Unity](https://github.com/Cysharp/Ulid) it requirement enable unsafe in its asmdef

Similar api to Guid.

- .Ulid.NewUlid()
- .Ulid.Parse()
- .Ulid.TryParse()
- .new Ulid()
- .ToString()
- .ToByteArray()
- .TryWriteBytes()
- .TryWriteStringify()
- .ToBase64()
- .Time
- .Random

```csharp
var stringId = Ulid.NewUlid().ToString();
```

provide attribute `UlidAttribute` to auto create unique id

</details>

## UGUI
### UIButton
### UIPopup
### Loading
### Enhanced Scroll

## Observable Collection

<details>
<summary>High performance observable collections and synchronized views for Unity</summary>

You can find source in [here](https://github.com/Cysharp/ObservableCollections)

```csharp
// Basic sample, use like ObservableCollection<T>.
// CollectionChanged observes all collection modification
var list = new ObservableList<int>();
list.CollectionChanged += List_CollectionChanged;

list.Add(10);
list.Add(20);
list.AddRange(new[] { 10, 20, 30 });

static void List_CollectionChanged(in NotifyCollectionChangedEventArgs<int> e)
{
    switch (e.Action)
    {
        case NotifyCollectionChangedAction.Add:
            if (e.IsSingleItem)
            {
               Debug.Log(e.NewItem);
            }
            else
            {
                foreach (var item in e.NewItems)
                {
                    Debug.Log(item);
                }
            }
            break;
        // Remove, Replace, Move, Reset
        default:
            break;
    }
}
```

</details>

## Atom

## Common

## Inspector

## TempCollection

<details>
<summary>
This is intended for a short lived collection that needs to be memory efficient and fast
</summary>

Call the static 'GetCollection' method to get a cached collection for use.
When you're done with the collection you call Release to make it available for reuse again later.
Do NOT use it again after calling Release.

Due to the design of this, it should only ever be used in a single threaded manner. Primarily intended
for the main Unity thread.

If you're in a separate thread, it's best to cache your own list local to there, and don't even bother with this.

### Sample Usage

```csharp
var simpleTempList = TempCollection.GetList<int>();

// TO_DO something
simpleTempList.Add(10);
simpleTempList.Add(0);
simpleTempList.Remove(5);
...

// Call Dispose when complete use simpleTempList
sinmpleTempList.Dispose();

```

You can you directly temp type like **TempArray**, **TempList**, **TempDictionary**, **TempHash**

```csharp
RaycastHit[] raycastHits = TempArray.TryGetTemp<RaycastHit>(4);

var ray3D = HandleUtility.GUIPointToWorldRay(guiPosition);
var raycastHitCount = Physics.RaycastNonAlloc(ray3D, raycastHits, Mathf.Infinity, layerMask);

for (var i = 0; i < raycastHitCount; i++)
{
    var raycastHit = raycastHits[i];
    // TO_DO
}

...

// Call Dispose when completed use
TempArray.Release(raycastHits);

```

</details>


## Level Base