using System;
using System.Runtime.CompilerServices;
using Pancake.Scriptable;
using UnityEngine;

namespace Pancake.SceneFlow
{
    [CreateAssetMenu(fileName = "outfit_variable.asset", menuName = "Pancake/Game/Outfit/Outfit Element")]
    [EditorIcon("scriptable_variable")]
    public class OutfitUnitVariable : ScriptableVariable<OutfitElement>
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Unlock()
        {
            Value.isUnlocked = true;
            Save();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool IsUnlocked() => Value.isUnlocked;

        public override void Save()
        {
            Data.Save(Guid, Value.isUnlocked);
            base.Save();
        }

        public override void Load()
        {
            base.Load();
            Value.isUnlocked = Data.Load(Guid, InitialValue.isUnlocked);
        }
        
#if UNITY_EDITOR
        protected override void OnValidate()
        {
            try
            {
                if (value == null || value.isUnlocked == PreviousValue.isUnlocked) return;
                ValueChanged();
            }
            catch (Exception)
            {
                // ignored
            }
        }
#endif
    }
}