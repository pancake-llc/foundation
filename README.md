# What

- Heart of the tree

# Environment

- Unity 2021.3.8f1
- scriptingBackend : IL2CPP
- apiCompatibilityLevel : .NetFramework

# How To Install

Add the lines below to `Packages/manifest.json`

- for dev version

```csharp
"com.pancake.heart": "https://github.com/pancake-llc/heart.git?path=Assets/_Root",
```

# Usages

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

<p style="text-align: center;">
  <img src="https://user-images.githubusercontent.com/44673303/190450836-492326a7-d0cf-47a7-965f-9c0d41afe1ce.png" width="600"  alt=""/>
</p>

<p style="text-align: center;">
  <img src="https://user-images.githubusercontent.com/44673303/190456451-86c0b01f-845a-4222-bcaa-543faa31f20c.png" width="600"  alt=""/>
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
  <img src="https://user-images.githubusercontent.com/44673303/190464081-dad74533-55fb-4919-a375-3abecfaf8a9b.png" width="600"  alt=""/>
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
  <img src="https://user-images.githubusercontent.com/44673303/190466539-f79fd032-2a6f-46ec-8252-d1b8fa2a3ea4.png" width="600"  alt=""/>
</p>

## NOTIFICATION

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

- Version 2.+ require minimum android api support is 5.+

## LOADING SCENE

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

## TIMER

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
   the scene changes. Note that as a result of this, creating a `Timer` when the scene is being closed, e.g. in an object's `OnDestroy` method, will result in a Unity error when the
   `TimerController` is spawned (test this on unity 2021+ but no error is thrown)

<p style="text-align: center;">
  <img src="https://i.imgur.com/ESFmFDO.png" width="600"  alt=""/>
</p>


## ARCHIVE

```csharp
public class ArchiveDemo : MonoBehaviour
{
    [SerializeField] private PlayerData playerData;
    
    public void Load()
    {
        // There's also async version
        // Usually LoadFile will be called from a different place (e.g load menu, bootstrap) but for simplicity of example I called it here
        Archive.LoadFile(fileName: "Save");

        playerData = Archive.Load<PlayerData>(key: "PlayerData");
    }

    public void Save()
    {
        Archive.Save(key: "PlayerData", playerData);

        // There's also async version
        // Usually SaveFile will be called from a different place (e.g save menu) but for simplicity of example I called it here
        Archive.SaveFile(fileName: "Save");
    }
}


[MessagePackObject, Serializable]
public class PlayerData
{
    [Key(0), SerializeField] private string playerName;
    [Key(1), SerializeField] private int level;
    [Key(2), SerializeField, Array] private List<Item> inventories;

    [Key(3), SerializeField, MessagePackFormatter(typeof(AssetFormatter<Sprite>))]
    private Sprite avatar;
}

[CreateAssetMenu, MessagePackFormatter(typeof(AssetFormatter<Item>))]
public class Item : ScriptableObject
{
    [SerializeField] private string itemName;
    [SerializeField] private int cost;
}
```