#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;

// ReSharper disable InconsistentNaming
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

        public override int GetHashCode() { return new { unity, android, ios }.GetHashCode(); }

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

        public override int GetHashCode() { return new { Unity, Android, Ios }.GetHashCode(); }

        private static string AdapterSdkVersion(string adapterVersion)
        {
            var index = adapterVersion.LastIndexOf(".");
            return index > 0 ? adapterVersion.Substring(0, index) : adapterVersion;
        }
    }

    public class AdapterMediationIronSource
    {
        public const string AdapterInstallPath = "Assets/IronSource/Editor";
        public EAdapterStatus currentStatus;
        public string adapterName;
        public string currentUnityVersion;
        public string latestUnityVersion;
        public string downloadUrl;
        public string displayAdapterName;
        public bool isNewAdapter;
        public string fileName;
        public Dictionary<string, string> sdkVersionDic;

        public AdapterMediationIronSource()
        {
            isNewAdapter = false;
            fileName = string.Empty;
            downloadUrl = string.Empty;
            currentUnityVersion = string.Empty;
            sdkVersionDic = new Dictionary<string, string>();
        }

        public bool GetFromJson(string name, Dictionary<string, object> dic)
        {
            adapterName = name;

            dic.TryGetValue("keyname", out object obj);
            if (obj != null) displayAdapterName = obj as string;
            else displayAdapterName = adapterName;

            dic.TryGetValue("isNewProvider", out obj);
            if (obj != null) isNewAdapter = bool.Parse(obj as string ?? "false");

            //Get Unity versions
            if (dic.TryGetValue("Unity", out obj))
            {
                if (obj is Dictionary<string, object> remoteVersions)
                {
                    if (remoteVersions.TryGetValue("DownloadUrl", out obj)) downloadUrl = obj as string;
                    if (remoteVersions.TryGetValue("FileName", out obj)) fileName = obj as string;
                    if (remoteVersions.TryGetValue("UnityAdapterVersion", out obj)) latestUnityVersion = obj as string;
                }
            }

            ////Get Android version
            if (dic.TryGetValue("Android", out obj))
            {
                if (obj is Dictionary<string, object> androidVersion)
                {
                    androidVersion.TryGetValue("version", out obj);
                    androidVersion = obj as Dictionary<string, object>;
                    if (androidVersion != null)
                    {
                        if (androidVersion.TryGetValue("sdk", out obj)) sdkVersionDic.Add("Android", obj as string);
                    }
                }
            }

            //Get iOS version
            dic.TryGetValue("iOS", out obj);
            if (obj is Dictionary<string, object> iosVersion)
            {
                iosVersion.TryGetValue("version", out obj);
                iosVersion = obj as Dictionary<string, object>;
                if (iosVersion != null)
                {
                    if (iosVersion.TryGetValue("sdk", out obj)) sdkVersionDic.Add("iOS", obj as string);
                }
            }

            RefreshCurrentUnityVersion();

            return true;
        }

        public void RefreshCurrentUnityVersion()
        {
            currentUnityVersion = GetVersionFromXML(fileName);
            if (string.IsNullOrEmpty(currentUnityVersion)) currentStatus = EAdapterStatus.NotInstall;
            else currentStatus = IsNewerVersion(currentUnityVersion, latestUnityVersion) ? EAdapterStatus.Upgrade : EAdapterStatus.Installed;
        }

        private static bool IsNewerVersion(string current, string latest)
        {
            var isNewer = false;
            try
            {
                int[] currentVersion = Array.ConvertAll(current.Split('.'), int.Parse);
                int[] remoteVersion = Array.ConvertAll(latest.Split('.'), int.Parse);
                var remoteBuild = 0;
                var curBuild = 0;
                if (currentVersion.Length > 3) curBuild = currentVersion[3];

                if (remoteVersion.Length > 3) remoteBuild = remoteVersion[3];

                var cur = new Version(currentVersion[0], currentVersion[1], currentVersion[2], curBuild);
                var remote = new Version(remoteVersion[0], remoteVersion[1], remoteVersion[2], remoteBuild);
                isNewer = cur < remote;
            }
            catch (Exception)
            {
                // ignored
            }

            return isNewer;
        }

        private static string GetVersionFromXML(string fileName)
        {
            var xmlDoc = new XmlDocument();
            const string version = "";
            try
            {
                xmlDoc.LoadXml(File.ReadAllText(Path.Combine(AdapterInstallPath, fileName)));
            }
            catch (Exception)
            {
                return version;
            }

            var unityVersion = xmlDoc.SelectSingleNode("dependencies/unityversion");
            if (unityVersion != null)
            {
                return unityVersion.InnerText;
            }

            File.Delete(Path.Combine(Path.Combine(AdapterInstallPath, fileName)));
            return version;
        }
    }
}
#endif