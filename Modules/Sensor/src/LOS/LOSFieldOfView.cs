using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Pancake.Sensor
{
    public class LOSFieldOfView
    {
        public float HorizAngle { get; private set; }
        public float VertAngle { get; private set; }
        public Vector3 Position { get; private set; }
        public Quaternion Rotation { get; private set; }

        CuttingPlane rightPlane, leftPlane, topPlane, bottomPlane;

        public void Set(float horizAngle, float vertAngle, Vector3 position, Quaternion rotation)
        {
            if (horizAngle == HorizAngle && vertAngle == VertAngle && position == Position && rotation == Rotation)
            {
                return;
            }

            HorizAngle = Mathf.Clamp(horizAngle, 0f, 180f);
            VertAngle = Mathf.Clamp(vertAngle, 0f, 180f);
            Position = position;
            Rotation = rotation;

            UpdatePlanes();
        }

        public void Clip(List<Triangle> triangles)
        {
            rightPlane.Cut(triangles);
            leftPlane.Cut(triangles);
            topPlane.Cut(triangles);
            bottomPlane.Cut(triangles);
        }

        void UpdatePlanes()
        {
            var horizRightRot = Quaternion.AngleAxis(HorizAngle / 2f, Vector3.up);
            var horizLeftRot = Quaternion.Inverse(horizRightRot);
            rightPlane = new CuttingPlane {Point = Position, Normal = Rotation * horizRightRot * Vector3.left};
            leftPlane = new CuttingPlane {Point = Position, Normal = Rotation * horizLeftRot * Vector3.right};

            var vertUpRot = Quaternion.AngleAxis(VertAngle / 2f, Vector3.right);
            var vertDownRot = Quaternion.Inverse(vertUpRot);
            topPlane = new CuttingPlane {Point = Position, Normal = Rotation * vertUpRot * Vector3.up};
            bottomPlane = new CuttingPlane {Point = Position, Normal = Rotation * vertDownRot * Vector3.down};
        }
    }

    public class LOSFieldOfView2D
    {
        public float Angle { get; private set; }
        public Vector3 Position { get; private set; }
        public Quaternion Rotation { get; private set; }

        CuttingPlane rightPlane, leftPlane;

        public void Set(float angle, Vector3 position, Quaternion rotation)
        {
            if (angle == Angle && position == Position && rotation == Rotation)
            {
                return;
            }

            Angle = Mathf.Clamp(angle, 0f, 180f);
            Position = position;
            Rotation = rotation;

            UpdatePlanes();
        }

        public void Clip(List<Edge2D> edges)
        {
            rightPlane.Cut(edges);
            leftPlane.Cut(edges);
        }

        void UpdatePlanes()
        {
            var rightRot = Quaternion.AngleAxis(Angle / 2f, Vector3.up);
            var leftRot = Quaternion.Inverse(rightRot);
            rightPlane = new CuttingPlane {Point = Position, Normal = Rotation * rightRot * Vector2.left};
            leftPlane = new CuttingPlane {Point = Position, Normal = Rotation * leftRot * Vector2.right};
        }
    }
}