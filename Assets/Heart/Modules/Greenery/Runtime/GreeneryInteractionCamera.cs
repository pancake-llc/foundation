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

        private RenderTexture _renderTexture;
        private Camera _cam;
        private static readonly int GlobalGreeneryInteractionRT = Shader.PropertyToID("_GLOBAL_GreeneryInteractionRT");
        private static readonly int GlobalGreeneryOrthoCam = Shader.PropertyToID("_GLOBAL_GreeneryOrthoCam");

        private void OnEnable()
        {
            int rtSize = 256 << (int) renderTextureResolution;
            _renderTexture = new RenderTexture(rtSize, rtSize, 0, RenderTextureFormat.ARGB32);
            _cam = GetComponent<Camera>();
            _cam.targetTexture = _renderTexture;
            Shader.SetGlobalTexture(GlobalGreeneryInteractionRT, _renderTexture);
        }

        private void OnDisable() { _renderTexture.Release(); }

        void Update() { Shader.SetGlobalVector(GlobalGreeneryOrthoCam, new float4(transform.position, _cam.orthographicSize)); }
    }
}