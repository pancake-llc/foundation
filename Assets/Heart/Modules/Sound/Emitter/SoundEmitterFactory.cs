using UnityEngine;

namespace Pancake.Sound
{
    [Searchable]
    [EditorIcon("scriptable_factory")]
    [CreateAssetMenu(fileName = "SoundEmitterFactory", menuName = "Pancake/Sound/Emitter Factory")]
    public class SoundEmitterFactory : ScriptableFactory<SoundEmitter>
    {
        public SoundEmitter prefab;
        public override SoundEmitter Create() { return Instantiate(prefab); }
    }
}