using System;
using System.Collections.Generic;
using Pancake.Linq;

namespace Pancake.Editor
{
    [Serializable]
    public class UniformFoldoutState
    {
        public List<FoldoutState> uniformFoldouts;

        public UniformFoldoutState() { uniformFoldouts = new List<FoldoutState>(); }

        public bool ContainsKey(string key) { return uniformFoldouts.Any(foldoutState => foldoutState.key.Equals(key)); }

        public void Add(string key, bool value) { uniformFoldouts.Add(new FoldoutState {key = key, state = value}); }

        public bool this[string key]
        {
            get
            {
                foreach (var foldoutState in uniformFoldouts)
                {
                    if (foldoutState.key.Equals(key))
                    {
                        return foldoutState.state;
                    }
                }

                return false;
            }
            set
            {
                foreach (var foldoutState in uniformFoldouts)
                {
                    if (foldoutState.key.Equals(key))
                    {
                        foldoutState.state = value;
                        break;
                    }
                }
            }
        }
    }

    [Serializable]
    public class FoldoutState
    {
        public string key;
        public bool state;
    }
}