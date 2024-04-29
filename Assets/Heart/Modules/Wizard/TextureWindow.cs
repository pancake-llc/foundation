using System.IO;
using System.Reflection;
using PancakeEditor.Common;
using UnityEditor;
using UnityEditor.U2D;
using UnityEngine;
using UnityEngine.U2D;

namespace PancakeEditor
{
    internal static class TextureWindow
    {
        public static void OnInspectorGUI(ref SpriteAtlas spriteAtlas, EditorWindow window)
        {
            EditorGUILayout.HelpBox("Please use uncompressed format for the source textues of Sprites if they are part of Sprite Atlas beacause:" +
                                    "\n     1) Only SpriteAtlas textures are ever shipped if sprites are part of SpriteAtlas." +
                                    "\n     2) When SpriteAtlas textures are packed, the original pixel of source textures need to be fetched. If they are compressed, this usually involves decompression and might result in lossy pixels depending on the compressed format." +
                                    "\n     3) If the sprites are part of SpriteSheets this may also result in artifacts depending on the compressed format." +
                                    "\n     4) Also speeds up SpriteAtlas packing as it does not involve decompression (some formats can be slow to decompress).",
                MessageType.Info);
            spriteAtlas = EditorGUILayout.ObjectField("Target", spriteAtlas, typeof(SpriteAtlas), false) as SpriteAtlas;
            if (GUILayout.Button("Uncompressed Source Texture", GUILayout.MaxHeight(Wizard.BUTTON_HEIGHT)))
            {
                if (spriteAtlas == null)
                {
                    window.ShowNotification(new GUIContent("Target can not be null!"));
                    return;
                }

                foreach (var packable in spriteAtlas.GetPackables())
                {
                    string path = AssetDatabase.GetAssetPath(packable);
                    if (File.Exists(path))
                    {
                        UnCompressed(path);
                    }
                    else if (Directory.Exists(path))
                    {
                        var textures = ProjectDatabase.FindAll<Texture2D>(path);
                        foreach (var tex in textures)
                        {
                            UnCompressed(AssetDatabase.GetAssetPath(tex));
                        }
                    }
                }

                AssetDatabase.Refresh();

                void UnCompressed(string path)
                {
                    var importer = (TextureImporter) AssetImporter.GetAtPath(path);
                    importer.textureCompression = TextureImporterCompression.Uncompressed;
                    importer.ClearPlatformTextureSettings("Standalone");
                    importer.ClearPlatformTextureSettings("Android");
                    importer.ClearPlatformTextureSettings("iPhone");
                    AssetDatabase.ImportAsset(path);
                }

                // clear log
                var assembly = Assembly.GetAssembly(typeof(SceneView));
                var type = assembly.GetType("UnityEditor.LogEntries");
                var method = type.GetMethod("Clear");
                method?.Invoke(new object(), null);

                Debug.Log($"UnCompress all source file of {spriteAtlas.name} completed!");
            }
        }
    }
}