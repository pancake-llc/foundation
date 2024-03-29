﻿using UnityEngine;

namespace Pancake.Scriptable
{
    [CreateAssetMenu(fileName = "event_color.asset", menuName = "Pancake/Scriptable/Events/color")]
    [EditorIcon("scriptable_event")]
    public class ScriptableEventColor : ScriptableEvent<Color>
    {
    }
}