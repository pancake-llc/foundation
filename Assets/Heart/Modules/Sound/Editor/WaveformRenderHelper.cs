using UnityEditor;
using UnityEngine;
using System;
using PancakeEditor.Sound.Reflection;

namespace PancakeEditor.Sound
{
    public class WaveformRenderHelper
    {
        private delegate void DoRenderPreview(bool setMaterial, AudioClip clip, AudioImporter audioImporter, Rect wantedRect, float scaleFactor);

        private Editor _editor;
        private Type _clipInspectorClass;
        private DoRenderPreview _doRenderPreview;

        public void RenderClipWaveform(Rect rect, AudioClip clip)
        {
            _clipInspectorClass ??= AudioClassReflectionHelper.GetUnityEditorClass("AudioClipInspector");
            string assetPath = AssetDatabase.GetAssetPath(clip);
            var importer = AssetImporter.GetAtPath(assetPath) as AudioImporter;
            Editor.CreateCachedEditor(clip, _clipInspectorClass, ref _editor);

            if (_doRenderPreview == null)
            {
                var method = _clipInspectorClass.GetMethod("DoRenderPreview", ReflectionExtension.PRIVATE_FLAG);
                _doRenderPreview = (DoRenderPreview) method.CreateDelegate(typeof(DoRenderPreview), _editor);
            }

            _doRenderPreview?.Invoke(true,
                clip,
                importer,
                rect,
                1f);
        }
    }
}