using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Pancake.Sensor
{
    [System.Serializable]
    public class SteerSeek
    {
        [SerializeField] Transform destinationTransform;

        public Transform DestinationTransform
        {
            get => destinationTransform;
            set
            {
                destinationTransform = value;
                seekDestinationPosition = false;
            }
        }

        public Vector3 Destination
        {
            get
            {
                if (DestinationTransform != null)
                {
                    return DestinationTransform.position;
                }
                else
                {
                    return destination;
                }
            }
            set
            {
                DestinationTransform = null;
                destination = value;
                seekDestinationPosition = true;
            }
        }

        public float TargetDistance;
        public float AcceptedDistanceRange = 1f;

        Vector3 destination;
        bool seekDestinationPosition;

        public void ClearDestination()
        {
            destination = Vector3.zero;
            destinationTransform = null;
            seekDestinationPosition = false;
        }

        public void SetInterest(GameObject go, DirectionalGrid grid)
        {
            grid.Fill(0);

            if (GetIsDestinationReached(go))
            {
                return;
            }

            var delta = Destination - go.transform.position;
            var toTargetDist = delta - (delta.normalized * TargetDistance);
            grid.GradientFill(toTargetDist, -1);
        }

        public bool GetIsDestinationReached(GameObject go)
        {
            if (destinationTransform == null && !seekDestinationPosition)
            {
                return true;
            }

            var delta = (Destination - go.transform.position);
            return Mathf.Abs(delta.magnitude - TargetDistance) <= AcceptedDistanceRange;
        }
    }
}