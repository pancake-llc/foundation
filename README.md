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

- Event `OnUpdateDeliveryTime` to use when you want to send notification one time with diffirent custom `Minute` each time such as fire notification in game idle when building house completed. In case you need write your custom method
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
