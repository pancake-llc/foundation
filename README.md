# What
- Heart of the tree

# Environment
- Unity 2021.3.8f1
- scriptingBackend : IL2CPP
- apiCompatibilityLevel : .NetFramework

# How To Install
Add the lines below to `Packages/manifest.json`

- for version 1.0.0
```csharp
"com.pancake.heart": "https://github.com/pancake-llc/heart.git?path=Assets/_Root#1.0.0",

"com.system-community.ben-demystifier": "https://github.com/system-community/BenDemystifier.git?path=Assets/_Root#0.4.1",
"com.system-community.harmony": "https://github.com/system-community/harmony.git?path=Assets/_Root#2.2.2",
"com.system-community.stringtools": "https://github.com/system-community/StringTools.git?path=Assets/_Root#1.0.0",
"com.system-community.reflection-metadata": "https://github.com/system-community/SystemReflectionMetadata.git?path=Assets/_Root#5.0.0",
"com.system-community.systemcollectionsimmutable": "https://github.com/system-community/SystemCollectionsImmutable.git?path=Assets/_Root#5.0.0",
"com.system-community.systemruntimecompilerservicesunsafe": "https://github.com/system-community/SystemRuntimeCompilerServicesUnsafe.git?path=Assets/_Root#5.0.0",
```

- for dev version
```csharp
"com.pancake.heart": "https://github.com/pancake-llc/heart.git?path=Assets/_Root",

"com.system-community.ben-demystifier": "https://github.com/system-community/BenDemystifier.git?path=Assets/_Root#0.4.1",
"com.system-community.harmony": "https://github.com/system-community/harmony.git?path=Assets/_Root#2.2.2",
"com.system-community.stringtools": "https://github.com/system-community/StringTools.git?path=Assets/_Root#1.0.0",
"com.system-community.reflection-metadata": "https://github.com/system-community/SystemReflectionMetadata.git?path=Assets/_Root#5.0.0",
"com.system-community.systemcollectionsimmutable": "https://github.com/system-community/SystemCollectionsImmutable.git?path=Assets/_Root#5.0.0",
"com.system-community.systemruntimecompilerservicesunsafe": "https://github.com/system-community/SystemRuntimeCompilerServicesUnsafe.git?path=Assets/_Root#5.0.0",
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

![image](https://user-images.githubusercontent.com/44673303/163957286-6714b6bc-68f5-46b6-9c9e-c3c7a2e1255b.png) 
![image](https://user-images.githubusercontent.com/44673303/163957353-c9b508ef-3425-4625-96a3-1ba3e09f319c.png)


#### _DROP AREA_

1. White List : Contains a list of links to list all the prefabs you can choose from in the PickUp Area
2. Black List : Contains a list of links to list all prefabs that won't show up in the PickUp Area
3. Using `Right Click` to clear all `White List` or `Black List`


### _SETTING_

1. Where Spawn :
   1. Default : 
      1. New instantiate object will spawn in root prefab when you in prefab mode
      2. New instantiate object will spawn in world space when you in scene mode
   
   2. Custom: You can choose to use the object as the root to spawn a new object here


### _PICKUP AREA_

![image](https://user-images.githubusercontent.com/44673303/163959317-78c6f079-69ee-4bb2-b476-21d8e9f7ce3e.png)

Where you choose the object to spawn

+ Using `Shift + Click` to instantiate object
+ Using `Right Click` in item to ping object prefab
+ Using `Right Click` in header Pickup Area to refresh draw collection item pickup
  ![image](https://user-images.githubusercontent.com/44673303/163969707-bc0beca6-2952-414f-8732-e1e4bcbaa630.png)
