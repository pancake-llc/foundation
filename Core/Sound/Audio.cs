using Pancake.Attribute;

namespace Pancake
{
    using UnityEngine;

    [Searchable]
    [EditorIcon("scriptable_audio")]
    [CreateAssetMenu(fileName = "Audio", menuName = "Pancake/Misc/Audio")]
    public class Audio : ScriptableObject
    {
        public bool loop;
        [SerializeField] private AudioGroup[] groups;

        public AudioClip[] GetClips()
        {
            int count = groups.Length;
            var result = new AudioClip[count];
            for (var i = 0; i < count; i++)
            {
                result[i] = groups[i].GetNextClip();
            }

            return result;
        }
    }
}