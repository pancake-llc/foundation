using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Pancake.Sensor
{
    /*
     * If a GameObject is detected and it has this component then the detected object will become
     * the 'ProxyTarget' this component points to.
     * This could be useful for example if you have characters with Rigidbodies/Colliders on
     * each of its limbs. You could put this component on each limb and point it to the root
     * GameObject of the character. Then if a sensor detects one or more of the limbs, it will
     * show the root GameObject in the list of detections. Otherwise the limbs would each be
     * detected separately.
     */
    [AddComponentMenu("Sensors/Signal Proxy")]
    public class SignalProxy : MonoBehaviour
    {
        public GameObject ProxyTarget;

        public static GameObject GetProxyTarget(GameObject from)
        {
            GameObject target = from;
            SignalProxy proxy;
            while (target.TryGetComponent(out proxy))
            {
                if (proxy.ProxyTarget == null || proxy.ProxyTarget == target)
                {
                    break;
                }

                target = proxy.ProxyTarget;
            }

            return target;
        }
    }
}