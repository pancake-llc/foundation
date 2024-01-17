using System;
using System.Collections.Generic;
using UnityPlayerLoop = UnityEngine.PlayerLoop;

namespace Pancake.PlayerLoop
{
    internal static class UpdateTypeExtension
    {
        private static readonly Dictionary<UpdateType, Type> Updates = new()
        {
            {UpdateType.EarlyUpdate, typeof(UnityPlayerLoop.EarlyUpdate)},
            {UpdateType.FixedUpdate, typeof(UnityPlayerLoop.FixedUpdate)},
            {UpdateType.PreUpdate, typeof(UnityPlayerLoop.PreUpdate)},
            {UpdateType.Update, typeof(UnityPlayerLoop.Update)},
            {UpdateType.PreLateUpdate, typeof(UnityPlayerLoop.PreLateUpdate)},
            {UpdateType.PostLateUpdate, typeof(UnityPlayerLoop.PostLateUpdate)}
        };

        public static Type ToType(this UpdateType updateType) => Updates[updateType];
        public static int ToIndex(this UpdateType updateType) => (int) updateType;
        public static UpdateType FromIndex(int index) => (UpdateType) index;
    }
}