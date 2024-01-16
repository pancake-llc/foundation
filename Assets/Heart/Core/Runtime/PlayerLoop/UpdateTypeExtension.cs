using System;
using System.Collections.Generic;
using UnityPlayerLoop = UnityEngine.PlayerLoop;

namespace Pancake.PlayerLoop
{
    internal static class UpdateTypeExtension
    {
        private static readonly Dictionary<UpdateType, Type> _updates = new()
        {
            {UpdateType.EarlyUpdate, typeof(UnityPlayerLoop.EarlyUpdate)},
            {UpdateType.FixedUpdate, typeof(UnityPlayerLoop.FixedUpdate)},
            {UpdateType.PreUpdate, typeof(UnityPlayerLoop.PreUpdate)},
            {UpdateType.Update, typeof(UnityPlayerLoop.Update)},
            {UpdateType.PreLateUpdate, typeof(UnityPlayerLoop.PreLateUpdate)},
            {UpdateType.PostLateUpdate, typeof(UnityPlayerLoop.PostLateUpdate)}
        };

        public static Type ToType(this UpdateType plt) => _updates[plt];
        public static int ToIndex(this UpdateType plt) => (int) plt;
        public static UpdateType FromIndex(int index) => (UpdateType) index;
    }
}