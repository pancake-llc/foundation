namespace Pancake.Greenery
{
    using Unity.Mathematics;
    using UnityEngine;

    [ExecuteAlways]
    public class GreeneryWind : MonoBehaviour
    {
        public Texture2D windTexture;
        public float2 textureScale = new(1, 1);
        public Vector4 frequencyFactors = new(1, 1, 1, 1);
        public float strength;
        public float speed;
        private static readonly int GlobalGreeneryWindTexture = Shader.PropertyToID("_GLOBAL_GreeneryWindTexture");
        private static readonly int GlobalGreeneryWindTextureScale = Shader.PropertyToID("_GLOBAL_GreeneryWindTextureScale");
        private static readonly int GlobalGreeneryWindDirection = Shader.PropertyToID("_GLOBAL_GreeneryWindDirection");
        private static readonly int GlobalGreeneryWindStrength = Shader.PropertyToID("_GLOBAL_GreeneryWindStrength");
        private static readonly int GlobalGreeneryWindSpeed = Shader.PropertyToID("_GLOBAL_GreeneryWindSpeed");
        private static readonly int GlobalGreeneryFrequencyFactors = Shader.PropertyToID("_GLOBAL_GreeneryFrequencyFactors");

        private void OnEnable() { UpdateWind(); }

        private void OnValidate() { UpdateWind(); }

        private void Update()
        {
            if (transform.hasChanged)
            {
                OnValidate();
            }
        }

        public void UpdateWind()
        {
            Shader.SetGlobalTexture(GlobalGreeneryWindTexture, windTexture);
            Shader.SetGlobalVector(GlobalGreeneryWindTextureScale, new float4(textureScale, 0, 0));
            Shader.SetGlobalVector(GlobalGreeneryWindDirection, math.normalize(new float4(-((float3) transform.forward).xz, 0, 0)));
            Shader.SetGlobalFloat(GlobalGreeneryWindStrength, strength);
            Shader.SetGlobalFloat(GlobalGreeneryWindSpeed, speed);
            Shader.SetGlobalVector(GlobalGreeneryFrequencyFactors, frequencyFactors);
        }

        private void OnDrawGizmos()
        {
            Gizmos.color = Color.green;
            Gizmos.DrawLine(transform.TransformPoint(new Vector3(-1, 0, -2)), transform.TransformPoint(new Vector3(-1, 0, 1)));
            Gizmos.DrawLine(transform.TransformPoint(new Vector3(-1, 0, 1)), transform.TransformPoint(new Vector3(-2, 0, 1)));
            Gizmos.DrawLine(transform.TransformPoint(new Vector3(-2, 0, 1)), transform.TransformPoint(new Vector3(0, 0, 3)));
            Gizmos.DrawLine(transform.TransformPoint(new Vector3(0, 0, 3)), transform.TransformPoint(new Vector3(2, 0, 1)));
            Gizmos.DrawLine(transform.TransformPoint(new Vector3(2, 0, 1)), transform.TransformPoint(new Vector3(1, 0, 1)));
            Gizmos.DrawLine(transform.TransformPoint(new Vector3(1, 0, 1)), transform.TransformPoint(new Vector3(1, 0, -2)));
            Gizmos.DrawLine(transform.TransformPoint(new Vector3(1, 0, -2)), transform.TransformPoint(new Vector3(-1, 0, -2)));

            Gizmos.DrawLine(transform.TransformPoint(new Vector3(0, -1, -2)), transform.TransformPoint(new Vector3(0, -1, 1)));
            Gizmos.DrawLine(transform.TransformPoint(new Vector3(0, -1, 1)), transform.TransformPoint(new Vector3(0, -2, 1)));
            Gizmos.DrawLine(transform.TransformPoint(new Vector3(0, -2, 1)), transform.TransformPoint(new Vector3(0, 0, 3)));
            Gizmos.DrawLine(transform.TransformPoint(new Vector3(0, 0, 3)), transform.TransformPoint(new Vector3(0, 2, 1)));
            Gizmos.DrawLine(transform.TransformPoint(new Vector3(0, 2, 1)), transform.TransformPoint(new Vector3(0, 1, 1)));
            Gizmos.DrawLine(transform.TransformPoint(new Vector3(0, 1, 1)), transform.TransformPoint(new Vector3(0, 1, -2)));
            Gizmos.DrawLine(transform.TransformPoint(new Vector3(0, 1, -2)), transform.TransformPoint(new Vector3(0, -1, -2)));
        }
    }
}