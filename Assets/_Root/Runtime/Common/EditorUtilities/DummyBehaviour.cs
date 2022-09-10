#if UNITY_EDITOR
namespace Pancake
{
    using System;
    using System.Collections.Generic;
    using UnityEngine;
    using System.Linq;

    public class DummyBehaviour : MonoBehaviour
    {
        [SerializeField] private string label = "";

        public static DummyBehaviour Add(GameObject go, string label = null)
        {
            DummyBehaviour db = go.AddComponent<DummyBehaviour>();
            db.label = label ?? "";
            return db;
        }

        public static DummyBehaviour Get(GameObject go, string newLabel = null) { return go.GetComponent<DummyBehaviour>() ?? Add(go, newLabel); }

        public static DummyBehaviour Create(string name, string label = null)
        {
            GameObject go = new GameObject(name);
            return Add(go, label);
        }

        public static DummyBehaviour Search(string label)
        {
            var dummies = C.GetSceneObjectsOfType<DummyBehaviour>();
            return dummies.FirstOrDefault(d => d.IsLabel(label));
        }

        public static IEnumerable<DummyBehaviour> SearchAll(string label)
        {
            var dummies = C.GetSceneObjectsOfType<DummyBehaviour>();
            return dummies.Where(d => d.IsLabel(label));
        }


        public bool IsLabel(string s) { return string.Equals(label, s, StringComparison.Ordinal); }
    }
}
#endif