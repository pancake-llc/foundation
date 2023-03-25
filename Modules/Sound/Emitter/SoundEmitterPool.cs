using UnityEngine;

namespace Pancake.Sound
{
    [CreateAssetMenu(fileName = "SoundEmitterPool", menuName = "Pancake/Sound/Emitter Pool")]
    public class SoundEmitterPool : ComponentPool<SoundEmitter>
    {
        [SerializeField] private SoundEmitterFactory factory;

        public override IFactory<SoundEmitter> Factory { get => factory; set => factory = value as SoundEmitterFactory; }
    }
}