// using UnityEngine;
//
// namespace Pancake.Core
// {
//     public class MoveToInterpret : MonoBehaviour
//     {
//         [SerializeField] private Transform moveHandle;
//         [SerializeField] private Vector3 moveMask = Vector3.one;
//         private Vector3 _fromPosition = Vector3.zero;
//         private Transform _toHandle;
//         private Vector3 _toPosition = Vector3.zero;
//         private bool _isLocal;
//         [SerializeField] private BaseInterpret moveInterper;
//         [SerializeField] private float depthOffset;
//         [SerializeField] private Transform rotateHandle;
//         private Quaternion _fromRotation = Quaternion.identity;
//         private Quaternion _toRotation = Quaternion.identity;
//         [SerializeField] private Transform scaleHandle;
//         private Vector3 _fromScale = Vector3.one;
//         private Vector3 _toScale = Vector3.one;
//
//         public Transform MoveHandle => moveHandle;
//
//         public float Depth { set => depthOffset = value; }
//
//         private void Awake() { moveInterper.OnUpdated += OnInterpUpdated; }
//
//         public void MoveTo(Transform handle, bool instant, bool isLocal)
//         {
//             _toHandle = handle;
//             _isLocal = isLocal;
//             SetNewTarget(instant);
//         }
//
//         public void MoveTo(Vector3 position, bool instant, bool isLocal)
//         {
//             MoveTo(position,
//                 Quaternion.identity,
//                 Vector3.one,
//                 instant,
//                 isLocal);
//         }
//
//         public void MoveTo(Vector3 position, Quaternion rotation, Vector3 scale, bool instant, bool isLocal)
//         {
//             _toHandle = null;
//             _isLocal = isLocal;
//             _toPosition = position;
//             _toRotation = rotation;
//             _toScale = scale;
//             SetNewTarget(instant);
//         }
//
//         private void SetNewTarget(bool instant)
//         {
//             CacheFrom();
//             if (!instant) moveInterper.SetGoal(0f, true);
//             moveInterper.SetGoal(1f, instant);
//         }
//
//         private void CacheFrom()
//         {
//             if (moveHandle != null)
//             {
//                 _fromPosition = moveHandle.position;
//                 if (_isLocal) _fromPosition = moveHandle.parent.InverseTransformPoint(_fromPosition);
//             }
//
//             if (rotateHandle != null)
//             {
//                 _fromRotation = (_isLocal ? rotateHandle.localRotation : rotateHandle.rotation);
//             }
//
//             if (scaleHandle != null)
//             {
//                 _fromScale = scaleHandle.localScale;
//             }
//         }
//
//         private void OnInterpUpdated(float interpValue)
//         {
//             Vector3 vector = _toPosition;
//             Quaternion b = _toRotation;
//             Vector3 b2 = _toScale;
//             if (_toHandle != null)
//             {
//                 vector = _toHandle.position;
//                 b = (_isLocal ? _toHandle.localRotation : _toHandle.rotation);
//                 b2 = _toHandle.localScale;
//                 if (_isLocal) vector = moveHandle.parent.InverseTransformPoint(vector);
//             }
//
//             if (moveHandle != null)
//             {
//                 Vector3 vector2 = Vector3.LerpUnclamped(_fromPosition, vector, interpValue);
//                 vector2 = Vector3.Scale(vector2, moveMask);
//                 vector2.z += depthOffset;
//                 if (_isLocal) moveHandle.localPosition = vector2;
//                 else moveHandle.position = vector2;
//             }
//
//             if (rotateHandle != null)
//             {
//                 Quaternion quaternion = Quaternion.LerpUnclamped(_fromRotation, b, interpValue);
//                 if (_isLocal) rotateHandle.localRotation = quaternion;
//                 else rotateHandle.rotation = quaternion;
//             }
//
//             if (scaleHandle != null)
//             {
//                 scaleHandle.localScale = Vector3.LerpUnclamped(_fromScale, b2, interpValue);
//             }
//         }
//     }
// }