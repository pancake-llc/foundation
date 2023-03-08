using System;
using System.Collections.Generic;

namespace Pancake.Monetization
{
    [Serializable]
    public class Network
    {
        public string name;
        public string displayName;
        public string[] versions;
        public string path;
        public string dependenciesFilePath;
        public string[] pluginFilePath;
        public NetworkVersion lastVersion;
        [NonSerialized] public NetworkVersion currentVersion;
        [NonSerialized] public EVersionComparisonResult CurrentToLatestVersionComparisonResult = EVersionComparisonResult.Lesser;
        [NonSerialized] public bool requireUpdate;
    }

    [Serializable]
    public class NetworkVersion
    {
        public string android;
        public string ios;
        public string unity;

        public NetworkVersion()
        {
            android = "";
            ios = "";
            unity = "";
        }

        public override bool Equals(object value)
        {
            return value is NetworkVersion versions && unity.Equals(versions.unity) && (android == null || android.Equals(versions.android)) &&
                   (ios == null || ios.Equals(versions.ios));
        }

        public bool HasEqualSdkVersions(NetworkVersion versions)
        {
            return versions != null && AdapterSdkVersion(android).Equals(AdapterSdkVersion(versions.android)) &&
                   AdapterSdkVersion(ios).Equals(AdapterSdkVersion(versions.ios));
        }

        public override int GetHashCode() { return new {unity, android, ios}.GetHashCode(); }

        private static string AdapterSdkVersion(string adapterVersion)
        {
            var index = adapterVersion.LastIndexOf(".");
            return index > 0 ? adapterVersion.Substring(0, index) : adapterVersion;
        }
    }

    [Serializable]
    public class MaxPluginData
    {
        public MaxNetwork AppLovinMax;
        public List<MaxNetwork> MediatedNetworks;
    }

    [Serializable]
    public class MaxNetwork
    {
        public string Name;
        public string DisplayName;
        public string DownloadUrl;
        public string DependenciesFilePath;
        public string[] PluginFilePaths;
        public MaxVersions LatestVersions;
        [NonSerialized] public MaxVersions CurrentVersions;
        [NonSerialized] public EVersionComparisonResult CurrentToLatestVersionComparisonResult = EVersionComparisonResult.Lesser;
        [NonSerialized] public bool RequiresUpdate;
    }

    [Serializable]
    public class MaxVersions
    {
        public string Unity;
        public string Android;
        public string Ios;

        public override bool Equals(object value)
        {
            var versions = value as MaxVersions;

            return versions != null && Unity.Equals(versions.Unity) && (Android == null || Android.Equals(versions.Android)) && (Ios == null || Ios.Equals(versions.Ios));
        }

        public bool HasEqualSdkVersions(MaxVersions versions)
        {
            return versions != null && AdapterSdkVersion(Android).Equals(AdapterSdkVersion(versions.Android)) &&
                   AdapterSdkVersion(Ios).Equals(AdapterSdkVersion(versions.Ios));
        }

        public override int GetHashCode() { return new {Unity, Android, Ios}.GetHashCode(); }

        private static string AdapterSdkVersion(string adapterVersion)
        {
            var index = adapterVersion.LastIndexOf(".");
            return index > 0 ? adapterVersion.Substring(0, index) : adapterVersion;
        }
    }
}