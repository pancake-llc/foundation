using System;
using UnityEngine;

namespace Pancake
{
    [Serializable]
    public class StringPair
    {
        [SerializeField] private string key;
        public string value;

        public string Key => key;
    }
}