using System;
using UnityEngine;

namespace Pancake.AttributeDrawer
{
    public abstract class PropertyOverrideContext
    {
        private static PropertyOverrideContext Override { get; set; }
        public static PropertyOverrideContext Current { get; private set; }

        public abstract bool TryGetDisplayName(Property property, out GUIContent displayName);

        public static EnterPropertyScope BeginProperty() { return new EnterPropertyScope().Init(); }

        public static OverrideScope BeginOverride(PropertyOverrideContext overrideContext) { return new OverrideScope(overrideContext); }

        public struct EnterPropertyScope : IDisposable
        {
            private PropertyOverrideContext _previousContext;

            public EnterPropertyScope Init()
            {
                _previousContext = Current;
                Current = Override;
                return this;
            }

            public void Dispose()
            {
                Override = Current;
                Current = _previousContext;
            }
        }

        public readonly struct OverrideScope : IDisposable
        {
            public OverrideScope(PropertyOverrideContext context)
            {
                if (Override != null)
                {
                    Debug.LogError($"TriPropertyContext already overriden with {Override.GetType()}");
                }

                Override = context;
            }

            public void Dispose() { Override = null; }
        }
    }
}