#if UNITY_EDITOR
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using Pancake.Linq;
using UnityEditor.PackageManager;
using UnityEngine;

namespace PancakeEditor.Common
{
    public static class RegistryManager
    {
        private static readonly string Manifest = Path.Combine(Application.dataPath, "..", "Packages", "manifest.json");

        public static void AddUnityPackage(string packageName) { Client.Add(packageName); }

        public static void AddUnityPackages(string[] packageNames) { Client.AddAndRemove(packageNames); }

        public static void AddPackage(string name, string version)
        {
            var json = JObject.Parse(File.ReadAllText(Manifest));
            var dependencies = (JObject) json["dependencies"];

            if (dependencies != null)
            {
                foreach (var dependency in dependencies)
                {
                    if (dependency.Key.Equals(name)) return;
                }

                dependencies.Add(name, version);
            }

            Write(json);
        }

        public static void RemovePackage(string name)
        {
            var json = JObject.Parse(File.ReadAllText(Manifest));
            var dependencies = (JObject) json["dependencies"];
            dependencies?.Remove(name);
            Write(json);
        }

        public static void RemoveAllPackagesStartWith(string prefix)
        {
            var json = JObject.Parse(File.ReadAllText(Manifest));
            var dependencies = (JObject) json["dependencies"];

            if (dependencies != null)
            {
                var packagesToRemove = new List<string>();

                foreach (var dependency in dependencies)
                {
                    if (dependency.Key.StartsWith(prefix)) packagesToRemove.Add(dependency.Key);
                }

                foreach (string package in packagesToRemove)
                {
                    dependencies.Remove(package);
                }
            }

            Write(json);
        }

        public static (bool, string) IsInstalled(string name)
        {
            var json = JObject.Parse(File.ReadAllText(Manifest));
            var dependencies = (JObject) json["dependencies"];
            if (dependencies != null)
            {
                foreach (var dependency in dependencies)
                {
                    if (dependency.Key.Equals(name)) return (true, dependency.Value.ToString());
                }
            }

            return (false, "");
        }

        public static void Resolve() { Client.Resolve(); }

        public static void AddScopedRegistry(string registryName, string url, List<string> scopes)
        {
            var json = JObject.Parse(File.ReadAllText(Manifest));
            var scopedRegistries = (JArray) json["scopedRegistries"] ?? new JArray();

            foreach (var registry in scopedRegistries)
            {
                if (registry["name"] != null && registry["name"].ToString().Equals(registryName))
                {
                    Debug.Log($"Registry '{registryName}' already exists.");
                    return;
                }
            }

            var newRegistry = new JObject {["name"] = registryName, ["url"] = url, ["scopes"] = new JArray(scopes)};

            scopedRegistries.Add(newRegistry);
            json["scopedRegistries"] = scopedRegistries;

            Write(json);
        }

        public static void RemoveScopedRegistry(string registryName)
        {
            var json = JObject.Parse(File.ReadAllText(Manifest));
            var scopedRegistries = (JArray) json["scopedRegistries"];

            if (scopedRegistries != null)
            {
                for (int i = 0; i < scopedRegistries.Count; i++)
                {
                    if (scopedRegistries[i]["name"] != null && scopedRegistries[i]["name"].ToString().Equals(registryName))
                    {
                        scopedRegistries.RemoveAt(i);
                        break;
                    }
                }
            }

            Write(json);
        }

        public static (bool, string) IsScopedRegistryInstalled(string registryName)
        {
            var json = JObject.Parse(File.ReadAllText(Manifest));
            var scopedRegistries = (JArray) json["scopedRegistries"];

            if (scopedRegistries != null)
            {
                foreach (var registry in scopedRegistries)
                {
                    if (registry["name"] != null && registry["name"].ToString().Equals(registryName)) return (true, registry["url"]?.ToString());
                }
            }

            return (false, "");
        }

        private static void Write(JObject json) { File.WriteAllText(Manifest, json.ToString()); }

        private static async Task<string> GetLatestVersionForPacakge(string package)
        {
            var request = Client.Search(package);
            while (!request.IsCompleted)
            {
                await Task.Delay(100);
            }

            if (request.Status != StatusCode.Success) return "";

            return request.Result.First().versions.latestCompatible;
        }

        public static async Task InstallLastVersionForPacakge(string package)
        {
            string version = await GetLatestVersionForPacakge(package);
            if (string.IsNullOrEmpty(version)) return;

            AddPackage(package, version);
            Resolve();
        }

        public static string GetVersionByPackageJson(string namePackage)
        {
            var upmPath = $"Packages/{namePackage}/package.json";
            string path = Path.GetFullPath(upmPath);
            if (File.Exists(path))
            {
                string json = File.ReadAllText(Path.GetFullPath(upmPath));
                var jsonObject = JObject.Parse(json);
                var version = jsonObject["version"]?.ToString();
                return version;
            }

            return "1.0.0";
        }
    }
}
#endif