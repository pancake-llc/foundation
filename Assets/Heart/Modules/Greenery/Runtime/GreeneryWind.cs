namespace Pancake.Greenery
{
    using Unity.Mathematics;
    using UnityEngine;

    [ExecuteAlways]
    public class GreeneryWind : MonoBehaviour
    {
        public Texture2D windTexture;
        public float2 textureScale = new float2(1, 1);
        public Vector4 frequencyFactors = new Vector4(1, 1, 1, 1);
        public float strength;
        public float speed;

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
            Shader.SetGlobalTexture("_GLOBAL_GreeneryWindTexture", windTexture);
            Shader.SetGlobalVector("_GLOBAL_GreeneryWindTextureScale", new float4(textureScale, 0, 0));
            Shader.SetGlobalVector("_GLOBAL_GreeneryWindDirection", math.normalize(new float4(-((float3) transform.forward).xz, 0, 0)));
            Shader.SetGlobalFloat("_GLOBAL_GreeneryWindStrength", strength);
            Shader.SetGlobalFloat("_GLOBAL_GreeneryWindSpeed", speed);
            Shader.SetGlobalVector("_GLOBAL_GreeneryFrequencyFactors", frequencyFactors);
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