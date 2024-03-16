using UnityEngine;

namespace Pancake.Physics
{
    public struct AsyncRaycastCallbackResult
    {
        public bool hit;
        public RaycastHit hitInfo;
        public object context;
    }
}