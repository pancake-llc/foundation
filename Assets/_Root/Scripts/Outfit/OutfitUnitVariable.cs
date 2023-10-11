using Pancake.Scriptable;
using UnityEngine;

namespace Pancake.SceneFlow
{
    [CreateAssetMenu(fileName = "outfit_variable.asset", menuName = "Pancake/Game/Outfit/Outfit Element")]
    [EditorIcon("scriptable_variable")]
    public class OutfitUnitVariable : ScriptableVariable<OutfitElement>
    {
        public override void Save()
        {
            Data.Save(Guid, Value.isUnlocked);
            base.Save();
        }

        public override void Load()
        {
            Value.isUnlocked = Data.Load(Guid, DefaultValue.isUnlocked);
            base.Load();
        }
    }
}