using UnityEngine;
using UnityEngine.Experimental.Rendering;

namespace Pancake.GreeneryEditor
{
    public static class GreenerySurfaceColorSampling
    {
        public static Color GetSurfaceColor(RaycastHit hit, ref Texture2D surfaceColorTexture)
        {
            var surfaceColor = Color.clear;
            if (hit.transform.TryGetComponent(out Terrain terrain))
            {
                if (surfaceColorTexture == null && terrain.terrainData.alphamapLayers > 0)
                {
                    surfaceColorTexture = GetTerrainSurfaceColorTexture(terrain);
                }

                surfaceColor = GetTerrainSurfaceColor(hit, surfaceColorTexture);
                surfaceColor.a = 1.0f;
            }
            else
            {
                if (hit.transform.TryGetComponent(out Renderer renderer))
                {
                    surfaceColor = GetMeshColor(renderer, hit.textureCoord);
                }
                else
                {
                    if (Camera.current.actualRenderingPath == RenderingPath.DeferredShading)
                    {
                        if (surfaceColorTexture == null)
                        {
                            surfaceColorTexture = GetAlbedoGBuffer();
                        }

                        surfaceColor = GetAlbedoGBufferColor(hit, surfaceColorTexture);
                        surfaceColor.a = 1.0f;
                    }
                }
            }

            return surfaceColor;
        }

        public static Color GetMeshColor(Renderer renderer, Vector2 uv)
        {
            Color surfaceColor;
            if (renderer.sharedMaterial.mainTexture != null)
            {
                var surfaceColorTexture = renderer.sharedMaterial.mainTexture as Texture2D;
                surfaceColor = SampleTexture(surfaceColorTexture, uv);
                surfaceColor.a = 1.0f;
            }
            else
            {
                surfaceColor = renderer.sharedMaterial.color.linear;
            }

            return surfaceColor;
        }

        private static Color SampleTexture(Texture2D texture, Vector2 uv)
        {
            return QualitySettings.activeColorSpace == ColorSpace.Gamma ? texture.GetPixelBilinear(uv.x, uv.y) : texture.GetPixelBilinear(uv.x, uv.y).linear;
        }

        private static Texture2D GetTerrainSurfaceColorTexture(Terrain terrain)
        {
            var rt = new RenderTexture(terrain.terrainData.alphamapTextures[0].width,
                terrain.terrainData.alphamapTextures[0].height,
                0,
                GraphicsFormat.R8G8B8A8_SRGB);
            RenderTexture.active = rt;

            var mat = new Material(Shader.Find("Hidden/TerrainColorShader"));
            mat.SetTexture("_SplatMap", terrain.terrainData.alphamapTextures[0]);
            for (var i = 0; i < terrain.terrainData.terrainLayers.Length; i++)
            {
                mat.SetTexture("_Diffuse" + i, terrain.terrainData.terrainLayers[i].diffuseTexture);
                mat.SetVector("_ST" + i,
                    new Vector4(terrain.terrainData.size.x / terrain.terrainData.terrainLayers[i].tileSize.x,
                        terrain.terrainData.size.y / terrain.terrainData.terrainLayers[i].tileSize.y,
                        terrain.terrainData.terrainLayers[i].tileOffset.x,
                        terrain.terrainData.terrainLayers[i].tileOffset.y));
            }

            Graphics.Blit(null, rt, mat);

            var terrainColTex = new Texture2D(rt.width, rt.height, TextureFormat.RGB24, false);
            terrainColTex.ReadPixels(new Rect(0, 0, rt.width, rt.height), 0, 0);
            terrainColTex.Apply();

            RenderTexture.active = null;
            rt.Release();

            return terrainColTex;
        }

        private static Color GetTerrainSurfaceColor(RaycastHit hit, Texture2D terrainColTex)
        {
            var surfaceColor = Color.black;
            if (terrainColTex != null) surfaceColor = SampleTexture(terrainColTex, hit.textureCoord);
            return surfaceColor;
        }

        private static Texture2D GetAlbedoGBuffer()
        {
            var rt = new RenderTexture(Screen.width, Screen.height, 0, GraphicsFormat.R8G8B8A8_SRGB);
            RenderTexture.active = rt;
            var mat = new Material(Shader.Find("Hidden/AlbedoGBufferColor"));
            Graphics.Blit(null, rt, mat);

            var albedoGBuffer = new Texture2D(rt.width, rt.height, TextureFormat.RGBA32, false);
            albedoGBuffer.wrapMode = TextureWrapMode.Clamp;
            albedoGBuffer.ReadPixels(new Rect(0, 0, rt.width, rt.height), 0, 0);
            albedoGBuffer.Apply();

            RenderTexture.active = null;
            rt.Release();

            return albedoGBuffer;
        }

        private static Color GetAlbedoGBufferColor(RaycastHit hit, Texture2D albedoGBuffer)
        {
            Vector2 uv = Camera.current.WorldToViewportPoint(hit.point);
            var gbufferColor = SampleTexture(albedoGBuffer, uv);
            return gbufferColor;
        }
    }
}