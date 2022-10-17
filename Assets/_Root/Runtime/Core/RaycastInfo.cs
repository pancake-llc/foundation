using UnityEngine;

namespace Pancake
{
    [System.Serializable]
    public struct RaycastInfo
    {
        [SerializeField] private Vector3 origin;
        [SerializeField] private Vector3 dir;
        [SerializeField] private float dist;

        #region CONSTRUCTOR

        public RaycastInfo(Ray r, float dist)
        {
            origin = r.origin;
            dir = r.direction;
            this.dist = dist;
        }

        public RaycastInfo(Vector3 origin, Vector3 dir, float dist)
        {
            this.origin = origin;
            this.dir = dir.normalized;
            this.dist = dist;
        }

        #endregion

        public Ray Ray
        {
            get { return new Ray(origin, dir); }
            set
            {
                origin = value.origin;
                dir = value.direction.normalized;
            }
        }

        public Vector3 Origin { get => origin; set => origin = value; }

        public Vector3 Direction { get => dir; set => dir = value.normalized; }

        public float Distance { get => dist; set => dist = value; }
    }
}