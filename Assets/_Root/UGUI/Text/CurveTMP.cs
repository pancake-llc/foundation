using TMPro;
using UnityEngine;

namespace Pancake.UI
{
    [RequireComponent(typeof(TMP_Text))]
    [ExecuteAlways]
    public class CurveTMP : MonoBehaviour
    {
        private TMP_Text _text;

        public AnimationCurve vertexCurve = new AnimationCurve(new Keyframe(0, 0),
            new Keyframe(0.25f, 2.0f),
            new Keyframe(0.5f, 0),
            new Keyframe(0.75f, 2.0f),
            new Keyframe(1, 0f));
        
        public float curveScale = 1f;
        
        private float _oldCurveScale = float.MaxValue;
        private AnimationCurve _oldVertexCurve;

        private bool _forceUpdate;

        private void Awake() { _text = GetComponent<TMP_Text>(); }

        private void OnEnable()
        {
            _forceUpdate = true;
#if UNITY_EDITOR
            UnityEditor.EditorApplication.update += OnEditorUpdate;
#endif
        }

#if UNITY_EDITOR
        private void OnEditorUpdate()
        {
            _forceUpdate = true;
        }
#endif
        private void OnDisable()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.update -= OnEditorUpdate;
#endif
        }

        private void Update()
        {
            if (!_forceUpdate && !_text.havePropertiesChanged && !ParametersHaveChanged()) return;
            _forceUpdate = false;
            vertexCurve.preWrapMode = WrapMode.Clamp;
            vertexCurve.postWrapMode = WrapMode.Clamp;
            Vector3[] vertices;
            Matrix4x4 matrix;
            _text.ForceMeshUpdate();

            TMP_TextInfo textInfo = _text.textInfo;
            int characterCount = textInfo.characterCount;
            if (characterCount == 0) return;

            float boundsMinX = _text.bounds.min.x;
            float boundsMaxX = _text.bounds.max.x;

            for (int i = 0; i < characterCount; i++)
            {
                if (!textInfo.characterInfo[i].isVisible)
                    continue;

                int vertexIndex = textInfo.characterInfo[i].vertexIndex;

                // Get the index of the mesh used by this character.
                int materialIndex = textInfo.characterInfo[i].materialReferenceIndex;

                vertices = textInfo.meshInfo[materialIndex].vertices;

                // Compute the baseline mid point for each character
                Vector3 offsetToMidBaseline = new Vector2((vertices[vertexIndex + 0].x + vertices[vertexIndex + 2].x) / 2, textInfo.characterInfo[i].baseLine);
                //float offsetY = VertexCurve.Evaluate((float)i / characterCount + loopCount / 50f); // Random.Range(-0.25f, 0.25f);

                // Apply offset to adjust our pivot point.
                vertices[vertexIndex + 0] += -offsetToMidBaseline;
                vertices[vertexIndex + 1] += -offsetToMidBaseline;
                vertices[vertexIndex + 2] += -offsetToMidBaseline;
                vertices[vertexIndex + 3] += -offsetToMidBaseline;

                // Compute the angle of rotation for each character based on the animation curve
                float x0 = (offsetToMidBaseline.x - boundsMinX) / (boundsMaxX - boundsMinX); // Character's position relative to the bounds of the mesh.
                float x1 = x0 + 0.0001f;
                float y0 = vertexCurve.Evaluate(x0) * curveScale;
                float y1 = vertexCurve.Evaluate(x1) * curveScale;

                Vector3 horizontal = new Vector3(1, 0, 0);
                //Vector3 normal = new Vector3(-(y1 - y0), (x1 * (boundsMaxX - boundsMinX) + boundsMinX) - offsetToMidBaseline.x, 0);
                Vector3 tangent = new Vector3(x1 * (boundsMaxX - boundsMinX) + boundsMinX, y1) - new Vector3(offsetToMidBaseline.x, y0);

                float dot = Mathf.Acos(Vector3.Dot(horizontal, tangent.normalized)) * 57.2957795f;
                Vector3 cross = Vector3.Cross(horizontal, tangent);
                float angle = cross.z > 0 ? dot : 360 - dot;

                matrix = Matrix4x4.TRS(new Vector3(0, y0, 0), Quaternion.Euler(0, 0, angle), Vector3.one);

                vertices[vertexIndex + 0] = matrix.MultiplyPoint3x4(vertices[vertexIndex + 0]);
                vertices[vertexIndex + 1] = matrix.MultiplyPoint3x4(vertices[vertexIndex + 1]);
                vertices[vertexIndex + 2] = matrix.MultiplyPoint3x4(vertices[vertexIndex + 2]);
                vertices[vertexIndex + 3] = matrix.MultiplyPoint3x4(vertices[vertexIndex + 3]);

                vertices[vertexIndex + 0] += offsetToMidBaseline;
                vertices[vertexIndex + 1] += offsetToMidBaseline;
                vertices[vertexIndex + 2] += offsetToMidBaseline;
                vertices[vertexIndex + 3] += offsetToMidBaseline;
            }

            //Upload the mesh with the revised information
            _text.UpdateVertexData();
        }

        private AnimationCurve CopyAnimationCurve(AnimationCurve curve)
        {
            var newCurve = new AnimationCurve {keys = curve.keys};
            return newCurve;
        }

        protected virtual bool ParametersHaveChanged()
        {
            // ReSharper disable CompareOfFloatsByEqualityOperator
            bool result =   curveScale != _oldCurveScale ||
                          vertexCurve.keys[1].value != _oldVertexCurve.keys[1].value;
            
            _oldCurveScale = curveScale;
            _oldVertexCurve = CopyAnimationCurve(vertexCurve);
            return result;
        }
    }
}