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

