﻿using System.IO;
using Newtonsoft.Json.Linq;
using UnityEditor;
using UnityEditor.PackageManager;
using UnityEngine;

namespace Pancake.Editor
{
    public static class RegistryManager
    {
        private static readonly string Manifest = Path.Combine(Application.dataPath, "..", "Packages", "manifest.json");

        public static void Add(string name, string version)
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

        public static void Remove(string name)
        {
            var json = JObject.Parse(File.ReadAllText(Manifest));
            var dependencies = (JObject) json["dependencies"];
            dependencies?.Remove(name);
            Write(json);
        }

        public static bool IsInstalled(string name)
        {
            var json = JObject.Parse(File.ReadAllText(Manifest));
            var dependencies = (JObject) json["dependencies"];
            if (dependencies != null)
            {
                foreach (var dependency in dependencies)
                {
                    if (dependency.Key.Equals(name)) return true;
                }
            }

            return false;
        }

        public static void Resolve()
        {
            Client.Resolve();
        }

        private static void Write(JObject json)
        {
            File.WriteAllText(Manifest, json.ToString());
        }
    }
}