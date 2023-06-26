using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Pancake.Sensor
{
    /*
     *  Put this component on an object to define explicitely what colliders it 'owns'. A line of sight
     *  test performed on the object will not be blocked by any colliders that belong to it.
     *  
     *  This component will be useful for example if you're doing LOS tests on a character set up like
     *  a ragdoll. If the character has Rigidbodies and Colliders on each of its limbs then those limbs
     *  may block the LOS rays targeting the character. Each limb with a Rigidbody will 'own' any Colliders
     *  on child objects, but the root of the character doesn't 'own' the limbs.
     *  
     *  To fix this you can put this component on the character and assign each limb into the list. You can
     *  put either the GameObjects with the Rigidbodies or the GameObjects with the Colliders into the list,
     *  either will work.
     */
    [AddComponentMenu("Sensors/LOS Collider Owner")]
    public class LOSColliderOwner : MonoBehaviour
    {
        public List<GameObject> Colliders;

        public bool IsColliderOwner(Collider c)
        {
            if (Colliders == null)
            {
                return false;
            }

            if (c == null)
            {
                return false;
            }

            if (Colliders.Contains(c.gameObject))
            {
                return true;
            }

            var rb = c.attachedRigidbody;
            if (rb != null && Colliders.Contains(rb.gameObject))
            {
                return true;
            }

            return false;
        }

        public bool IsColliderOwner(Collider2D c)
        {
            if (Colliders == null)
            {
                return false;
            }

            if (c == null)
            {
                return false;
            }

            if (Colliders.Contains(c.gameObject))
            {
                return true;
            }

            var rb = c.attachedRigidbody;
            if (rb != null && Colliders.Contains(rb.gameObject))
            {
                return true;
            }

            return false;
        }
    }
}