// using System;
// using UnityEngine;
//
// namespace Pancake.SceneFlow
// {
//     [Serializable]
//     [CreateAssetMenu(fileName = "event_preview_locked_outfit.asset", menuName = "Pancake/Misc/Events/Preview Locked Outfit")]
//     [EditorIcon("so_blue_event")]
//     public class ScriptableEventPreviewLockedOutfit : ScriptableEventBase
//     {
//         private Action<OutfitType, string> _onRaised;
//
//         /// <summary> Event raised when the event is raised. </summary>
//         public event Action<OutfitType, string> OnRaised { add => _onRaised += value; remove => _onRaised -= value; }
//
//         /// <summary> Raise the event </summary>
//         public void Raise(OutfitType type, string skinId)
//         {
//             if (!Application.isPlaying) return;
//
//             _onRaised?.Invoke(type, skinId);
//         }
//     }
// }