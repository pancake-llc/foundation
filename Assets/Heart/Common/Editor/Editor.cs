using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace PancakeEditor.Common
{
    public static class Editor
    {
        public const string DEFAULT_RESOURCE_PATH = "Assets/_Root/Resources";
        public const string DEFAULT_EDITOR_RESOURCE_PATH = "Assets/_Root/Editor/Resources";

        public static Rect GetInnerGuiPosition(SceneView sceneView)
        {
            var position = sceneView.position;
            position.x = position.y = 0;
            position.height -= EditorStyles.toolbar.fixedHeight;
            return position;
        }

        public static bool Get2DMouseScenePosition(out Vector2 result)
        {
            result = Vector2.zero;

            var cam = Camera.current;
            var mp = Event.current.mousePosition;
            mp.y = cam.pixelHeight - mp.y;
            var ray = cam.ScreenPointToRay(mp);
            if (ray.direction != Vector3.forward) return false;

            result = ray.origin;
            return true;
        }

        /// <summary>
        /// Render an object on sceneView using sprite renderers
        /// </summary>
        public static void FakeRenderSprite(GameObject obs, Vector3 position, Vector3 scale, Quaternion rotation)
        {
            var rends = obs.GetComponentsInChildren<SpriteRenderer>();
            foreach (var rend in rends)
            {
                var bounds = rend.bounds;
                var pos = rend.transform.position - obs.transform.position + position;
                DrawSprite(rend.sprite, pos, Vector3.Scale(bounds.size, scale));
            }
        }

        private static void DrawSprite(Sprite sprite, Vector3 worldSpace, Vector3 size)
        {
            if (sprite == null) return;
            var spriteTextureRect = LocalTextureRect(sprite);

            Handles.BeginGUI();

            var v0 = HandleUtility.WorldToGUIPoint(worldSpace - size / 2f);
            var v1 = HandleUtility.WorldToGUIPoint(worldSpace + size / 2f);
            var vMin = new Vector2(Mathf.Min(v0.x, v1.x), Mathf.Min(v0.y, v1.y));
            var vMax = new Vector2(Mathf.Max(v0.x, v1.x), Mathf.Max(v0.y, v1.y));
            var r = new Rect(vMin, vMax - vMin);
            GUI.DrawTextureWithTexCoords(r, sprite.texture, spriteTextureRect);

            Handles.EndGUI();
        }

        /// <summary>
        /// Calculate normalized texturerect of a sprite (0->1)
        /// </summary>
        private static Rect LocalTextureRect(Sprite sprite)
        {
            var texturePosition = sprite.textureRect.position;
            var textureSize = sprite.textureRect.size;
            texturePosition.x /= sprite.texture.width;
            texturePosition.y /= sprite.texture.height;
            textureSize.x /= sprite.texture.width;
            textureSize.y /= sprite.texture.height;
            return new Rect(texturePosition, textureSize);
        }

        public static void SkipEvent()
        {
            int id = GUIUtility.GetControlID(FocusType.Passive);
            GUIUtility.hotControl = id;
            HandleUtility.AddDefaultControl(id);
            Event.current.Use();
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="root"></param>
        /// <param name="bounds"></param>
        /// <param name="space"></param>
        /// <param name="renderers"></param>
        /// <param name="colliders"></param>
        /// <param name="meshes"></param>
        /// <param name="graphics"></param>
        /// <param name="particles"></param>
        /// <returns></returns>
        public static bool CalculateBounds(
            this GameObject root,
            out Bounds bounds,
            Space space,
            bool renderers = true,
            bool colliders = true,
            bool meshes = false,
            bool graphics = true,
            bool particles = false)
        {
            bounds = new Bounds();

            var first = true;

            if (space == Space.Self)
            {
                if (renderers)
                {
                    var results = new List<Renderer>();
                    root.GetComponentsInChildren(results);

                    foreach (var renderer in results)
                    {
                        if (!renderer.enabled)
                        {
                            continue;
                        }

                        if (!particles && renderer is ParticleSystemRenderer)
                        {
                            continue;
                        }

                        var rendererBounds = renderer.bounds;

                        rendererBounds.SetMinMax(root.transform.InverseTransformPoint(rendererBounds.min), root.transform.InverseTransformPoint(rendererBounds.max));

                        if (first)
                        {
                            bounds = rendererBounds;
                            first = false;
                        }
                        else
                        {
                            bounds.Encapsulate(rendererBounds);
                        }
                    }

                    results = null;
                }

                if (meshes)
                {
                    var meshFilters = new List<MeshFilter>();
                    root.GetComponentsInChildren(meshFilters);

                    foreach (var meshFilter in meshFilters)
                    {
                        var mesh = Application.isPlaying ? meshFilter.mesh : meshFilter.sharedMesh;

                        if (mesh == null)
                        {
                            continue;
                        }

                        var meshBounds = mesh.bounds;

                        meshBounds.SetMinMax(root.transform.InverseTransformPoint(meshFilter.transform.TransformPoint(meshBounds.min)),
                            root.transform.InverseTransformPoint(meshFilter.transform.TransformPoint(meshBounds.max)));

                        if (first)
                        {
                            bounds = meshBounds;
                            first = false;
                        }
                        else
                        {
                            bounds.Encapsulate(meshBounds);
                        }
                    }

                    meshFilters = null;
                }

                if (graphics)
                {
                    var results = new List<Graphic>();
                    root.GetComponentsInChildren(results);

                    foreach (var graphic in results)
                    {
                        if (!graphic.enabled)
                        {
                            continue;
                        }

                        var graphicCorners = new Vector3[4] {Vector3.zero, Vector3.zero, Vector3.zero, Vector3.zero};
                        graphic.rectTransform.GetLocalCorners(graphicCorners);
                        var graphicsBounds = BoundsFromCorners(graphicCorners);
                        graphicCorners = null;

                        if (first)
                        {
                            bounds = graphicsBounds;
                            first = false;
                        }
                        else
                        {
                            bounds.Encapsulate(graphicsBounds);
                        }
                    }

                    results = null;
                }

                if (colliders && first)
                {
                    var results = new List<Collider>();
                    root.GetComponentsInChildren(results);

                    foreach (var collider in results)
                    {
                        if (!collider.enabled)
                        {
                            continue;
                        }

                        var colliderBounds = collider.bounds;

                        colliderBounds.SetMinMax(root.transform.InverseTransformPoint(colliderBounds.min), root.transform.InverseTransformPoint(colliderBounds.max));

                        if (first)
                        {
                            bounds = colliderBounds;
                            first = false;
                        }
                        else
                        {
                            bounds.Encapsulate(colliderBounds);
                        }
                    }

                    results = null;
                }

                return !first;
            }
            else // if (space == Space.World)
            {
                if (renderers)
                {
                    var results = new List<Renderer>();
                    root.GetComponentsInChildren(results);

                    foreach (var renderer in results)
                    {
                        if (!renderer.enabled)
                        {
                            continue;
                        }

                        if (!particles && renderer is ParticleSystemRenderer)
                        {
                            continue;
                        }

                        if (first)
                        {
                            bounds = renderer.bounds;
                            first = false;
                        }
                        else
                        {
                            bounds.Encapsulate(renderer.bounds);
                        }
                    }

                    results = null;
                }

                if (meshes)
                {
                    var filters = new List<MeshFilter>();
                    root.GetComponentsInChildren(filters);

                    foreach (var meshFilter in filters)
                    {
                        var mesh = (Application.isPlaying ? meshFilter.mesh : meshFilter.sharedMesh);

                        if (mesh == null)
                        {
                            continue;
                        }

                        var meshBounds = mesh.bounds;

                        meshBounds.SetMinMax(root.transform.TransformPoint(meshBounds.min), root.transform.TransformPoint(meshBounds.max));

                        if (first)
                        {
                            bounds = meshBounds;
                            first = false;
                        }
                        else
                        {
                            bounds.Encapsulate(meshBounds);
                        }
                    }

                    filters = null;
                }

                if (graphics)
                {
                    var results = new List<Graphic>();
                    root.GetComponentsInChildren(results);

                    foreach (var graphic in results)
                    {
                        if (!graphic.enabled)
                        {
                            continue;
                        }

                        var graphicCorners = new Vector3[4] {Vector3.zero, Vector3.zero, Vector3.zero, Vector3.zero};
                        graphic.rectTransform.GetWorldCorners(graphicCorners);
                        var graphicsBounds = BoundsFromCorners(graphicCorners);
                        graphicCorners = null;

                        if (first)
                        {
                            bounds = graphicsBounds;
                            first = false;
                        }
                        else
                        {
                            bounds.Encapsulate(graphicsBounds);
                        }
                    }

                    results = null;
                }

                if (colliders && first)
                {
                    var results = new List<Collider>();
                    root.GetComponentsInChildren(results);

                    foreach (var collider in results)
                    {
                        if (!collider.enabled)
                        {
                            continue;
                        }

                        if (first)
                        {
                            bounds = collider.bounds;
                            first = false;
                        }
                        else
                        {
                            bounds.Encapsulate(collider.bounds);
                        }
                    }

                    results = null;
                }
            }

            return !first;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="corners"></param>
        /// <returns></returns>
        private static Bounds BoundsFromCorners(Vector3[] corners)
        {
            var minX = float.MaxValue;
            var minY = float.MaxValue;
            var minZ = float.MaxValue;

            var maxX = float.MinValue;
            var maxY = float.MinValue;
            var maxZ = float.MinValue;

            foreach (var corner in corners)
            {
                if (corner.x < minX)
                {
                    minX = corner.x;
                }

                if (corner.y < minY)
                {
                    minY = corner.y;
                }

                if (corner.z < minZ)
                {
                    minZ = corner.z;
                }

                if (corner.x > minX)
                {
                    maxX = corner.x;
                }

                if (corner.y > minY)
                {
                    maxY = corner.y;
                }

                if (corner.z > minZ)
                {
                    maxZ = corner.z;
                }
            }

            return new Bounds() {min = new Vector3(minX, minY, minZ), max = new Vector3(maxX, maxY, maxZ)};
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        public static string ToCamelCase(this string source) { return new CamelCaseNamingStrategy().GetPropertyName(source, false); }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        public static string ToSnakeCase(this string source) { return new SnakeCaseNamingStrategy().GetPropertyName(source, false); }

        /// <summary>
        /// check if given type is array or list
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static bool IsCollectionType(this Type type) { return type.IsArray || type.IsGenericType && type.GetGenericTypeDefinition() == typeof(List<>); }

        public static Type GetCorrectElementType(this Type type)
        {
            if (type.IsArray)
            {
                return type.GetElementType();
            }

            if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(List<>))
            {
                return type.GetGenericArguments()[0];
            }

            return null;
        }

        /// <summary>
        /// Convert image from string base 64
        /// </summary>
        /// <param name="base64"></param>
        /// <returns></returns>
        public static Texture2D ConvertToTexture(string base64)
        {
            var tex = new Texture2D(1,
                1,
                TextureFormat.RGBA32,
                false,
                false) {hideFlags = HideFlags.HideAndDontSave};
            tex.LoadImage(Convert.FromBase64String(base64));
            return tex;
        }

        /// <summary>
        /// thanks, @JoshuaMcKenzie and @Edvard-D
        /// remove all empty object reference elements
        /// </summary>
        /// <param name="list"></param>
        /// <returns></returns>
        public static int RemoveEmptyArrayElements(this SerializedProperty list)
        {
            var elementsRemoved = 0;
            if (list == null) return elementsRemoved;

            for (int i = list.arraySize - 1; i >= 0; i--)
            {
                var element = list.GetArrayElementAtIndex(i);
                if (element.propertyType != SerializedPropertyType.ObjectReference) continue;
                if (list.GetArrayElementAtIndex(i).objectReferenceValue == null)
                {
                    list.RemoveElement(i);
                    elementsRemoved++;
                }
            }

            return elementsRemoved;
        }

        /// <summary>
        /// thanks @JoshuaMcKenzie
        /// </summary>
        /// <param name="list"></param>
        /// <param name="index"></param>
        public static void RemoveElement(this SerializedProperty list, int index)
        {
            if (list == null) throw new ArgumentNullException(nameof(list));

            if (!list.isArray) throw new ArgumentException("Property is not an array");

            if (index < 0 || index >= list.arraySize) throw new ArgumentOutOfRangeException(nameof(list));

            list.GetArrayElementAtIndex(index).SetPropertyValue(null);
            list.DeleteArrayElementAtIndex(index);

            list.serializedObject.ApplyModifiedProperties();
        }

        /// <summary>
        /// thanks @JoshuaMcKenzie
        /// </summary>
        /// <param name="property"></param>
        /// <param name="value"></param>
        public static void SetPropertyValue(this SerializedProperty property, object value)
        {
            switch (property.propertyType)
            {
                case SerializedPropertyType.AnimationCurve:
                    property.animationCurveValue = value as AnimationCurve;
                    break;

                case SerializedPropertyType.ArraySize:
                    property.intValue = Convert.ToInt32(value);
                    break;

                case SerializedPropertyType.Boolean:
                    property.boolValue = Convert.ToBoolean(value);
                    break;

                case SerializedPropertyType.Bounds:
                    property.boundsValue = (Bounds?) value ?? new Bounds();
                    break;

                case SerializedPropertyType.Character:
                    property.intValue = Convert.ToInt32(value);
                    break;

                case SerializedPropertyType.Color:
                    property.colorValue = (Color?) value ?? new Color();
                    break;

                case SerializedPropertyType.Float:
                    property.floatValue = Convert.ToSingle(value);
                    break;

                case SerializedPropertyType.Integer:
                    property.intValue = Convert.ToInt32(value);
                    break;

                case SerializedPropertyType.LayerMask:
                    property.intValue = (value as LayerMask?)?.value ?? Convert.ToInt32(value);
                    break;

                case SerializedPropertyType.ObjectReference:
                    property.objectReferenceValue = value as Object;
                    break;

                case SerializedPropertyType.Quaternion:
                    property.quaternionValue = (Quaternion?) value ?? Quaternion.identity;
                    break;

                case SerializedPropertyType.Rect:
                    property.rectValue = (Rect?) value ?? new Rect();
                    break;

                case SerializedPropertyType.String:
                    property.stringValue = value as string;
                    break;

                case SerializedPropertyType.Vector2:
                    property.vector2Value = (Vector2?) value ?? Vector2.zero;
                    break;

                case SerializedPropertyType.Vector3:
                    property.vector3Value = (Vector3?) value ?? Vector3.zero;
                    break;

                case SerializedPropertyType.Vector4:
                    property.vector4Value = (Vector4?) value ?? Vector4.zero;
                    break;
            }
        }

        public static string GetSizeInMemory(this long byteSize)
        {
            string[] sizes = {"B", "KB", "MB", "GB", "TB"};
            var len = Convert.ToDouble(byteSize);
            var order = 0;
            while (len >= 1024D && order < sizes.Length - 1)
            {
                order++;
                len /= 1024;
            }

            return string.Format(System.Globalization.CultureInfo.CurrentCulture, "{0:0.##} {1}", len, sizes[order]);
        }

        public static void MoveToCenter(this EditorWindow window)
        {
            var position = window.position;
            var mainWindowPosition = ScreenUtility.GetCenter();
            if (mainWindowPosition != Rect.zero)
            {
                float width = (mainWindowPosition.width - position.width) * 0.5f;
                float height = (mainWindowPosition.height - position.height) * 0.5f;
                position.x = mainWindowPosition.x + width;
                position.y = mainWindowPosition.y + height;
                window.position = position;
            }
            else
            {
                window.position = new Rect(new Vector2(Screen.width, Screen.height), new Vector2(position.width, position.height));
            }
        }

        public static bool GetEditorBool(string key, bool defaultValue) { return EditorPrefs.GetBool($"{Application.identifier}_{key}", defaultValue); }

        public static void SetEditorBool(string key, bool value) { EditorPrefs.SetBool($"{Application.identifier}_{key}", value); }

        /// <summary>
        /// Deletes an object after showing a confirmation dialog.
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static bool DeleteObjectWithConfirmation(Object obj)
        {
            bool confirmDelete = EditorUtility.DisplayDialog($"Delete {obj.name}?", $"Are you sure you want to delete '{obj.name}'?", "Yes", "No");
            if (confirmDelete)
            {
                string path = AssetDatabase.GetAssetPath(obj);
                AssetDatabase.DeleteAsset(path);
                return true;
            }

            return false;
        }

        public static void RecordUndo(this Object o, string operationName = "") => Undo.RecordObject(o, operationName);

        public static void Dirty(this Object o) => EditorUtility.SetDirty(o);

        #region Rect

        private static WrappedEvent Wrap(this Event e) => new(e);

        public static WrappedEvent CurrentEvent =>
            (Event.current ?? typeof(Event).GetField("s_Current", TypeExtensions.MAX_BINDING_FLAGS)?.GetValue(null) as Event).Wrap();

        public static Rect SetPosition(this Rect rect, Vector2 v) => rect.SetPosition(v.x, v.y);

        public static Rect SetPosition(this Rect rect, float x, float y)
        {
            rect.x = x;
            rect.y = y;
            return rect;
        }

        public static Rect SetXMax(this Rect rect, float value)
        {
            rect.xMax = value;
            return rect;
        }

        public static Rect AddX(this Rect rect, float value)
        {
            rect.x += value;
            return rect;
        }

        public static Rect AddY(this Rect rect, float value)
        {
            rect.y += value;
            return rect;
        }
        
        public static Rect AddXMax(this Rect rect, float value)
        {
            rect.xMax += value;
            return rect;
        }

        public static Rect AddYMax(this Rect rect, float value)
        {
            rect.yMax += value;
            return rect;
        }

        public static Rect SetWidth(this Rect rect, float value)
        {
            rect.width = value;
            return rect;
        }

        public static Rect SetWidthFromMid(this Rect rect, float px)
        {
            rect.x += rect.width / 2;
            rect.width = px;
            rect.x -= rect.width / 2;
            return rect;
        }

        public static Rect SetWidthFromRight(this Rect rect, float px)
        {
            rect.x += rect.width;
            rect.width = px;
            rect.x -= rect.width;
            return rect;
        }

        public static Rect SetHeight(this Rect rect, float f)
        {
            rect.height = f;
            return rect;
        }

        public static Rect SetHeightFromBottom(this Rect rect, float px)
        {
            rect.y += rect.height;
            rect.height = px;
            rect.y -= rect.height;
            return rect;
        }

        public static Rect AddWidth(this Rect rect, float f) => rect.SetWidth(rect.width + f);

        public static Rect AddWidthFromMid(this Rect rect, float f) => rect.SetWidthFromMid(rect.width + f);

        public static Rect AddWidthFromRight(this Rect rect, float f) => rect.SetWidthFromRight(rect.width + f);

        public static Rect AddHeight(this Rect rect, float f) => rect.SetHeight(rect.height + f);

        public static Rect AddHeightFromBottom(this Rect rect, float f) => rect.SetHeightFromBottom(rect.height + f);

        public static bool IsHovered(this Rect r) => !CurrentEvent.IsNull && r.Contains(CurrentEvent.MousePosition);

        public static void MarkInteractive(this Rect rect)
        {
            if (!CurrentEvent.IsRepaint) return;

            var unclippedRect = (Rect) TypeExtensions.GUIClipUnclipToWindow.Invoke(null, new object[] {rect});

            object curGuiView = TypeExtensions.CurrentGuiView.GetValue(null);

            TypeExtensions.GUIViewMarkHotRegion.Invoke(curGuiView, new object[] {unclippedRect});
        }

        #endregion
    }
}