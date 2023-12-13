namespace Pancake.Greenery
{
    using Unity.Mathematics;
    using UnityEngine;

    public class GreeneryInteractionCamera : MonoBehaviour
    {
        public enum Resolution
        {
            _256 = 0,
            _512 = 1,
            _1024 = 2,
            _2048 = 3
        }

        public Resolution renderTextureResolution;

        private RenderTexture renderTexture;
        private Camera cam;

        private void OnEnable()
        {
            int rtSize = 256 << (int) renderTextureResolution;
            renderTexture = new RenderTexture(rtSize, rtSize, 0, RenderTextureFormat.ARGB32);
            cam = GetComponent<Camera>();
            cam.targetTexture = renderTexture;
            Shader.SetGlobalTexture("_GLOBAL_GreeneryInteractionRT", renderTexture);
        }

        private void OnDisable() { renderTexture.Release(); }

        void Update() { Shader.SetGlobalVector("_GLOBAL_GreeneryOrthoCam", new float4(transform.position, cam.orthographicSize)); }
    }
}