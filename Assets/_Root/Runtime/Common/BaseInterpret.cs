// using System.Collections;
// using UnityEngine;
//
// namespace Pancake.Core
// {
//     public abstract class BaseInterpret : MonoBehaviour
//     {
//         [SerializeField] protected string interpretName = string.Empty;
//         public float startGoal;
//         public float minimumDelta = 0.01f;
//         public C.ETimeScale timeType;
//         protected float goal;
//         protected float value;
//         protected Coroutine routine;
//         protected float lastInterpretValue;
//         private bool _interpretSet;
//         private GameObject _cacheGameObject;
//         private bool _isCached;
//         [SerializeField] private Vector2 visibilityValue = new Vector2(1f, 0f);
//
//         public delegate void InterpretChangedHandler(float value);
//
//         public event InterpretChangedHandler OnUpdated = delegate(float f) { };
//
//         protected float Goal => goal;
//
//         protected float Value => value;
//         public bool IsSleeping => routine == null;
//
//         private void Start()
//         {
//             if (!_interpretSet) SetGoal(startGoal, true);
//         }
//
//         private void Cache()
//         {
//             if (!_isCached)
//             {
//                 _isCached = true;
//                 _cacheGameObject = gameObject;
//             }
//         }
//
//         public void SetVisible(bool isVisible, bool instant)
//         {
//             float x = visibilityValue.x;
//             if (isVisible)
//             {
//                 x = visibilityValue.y;
//             }
//
//             SetGoal(x, instant);
//         }
//
//         public virtual void SetGoal(float goal, bool instant = false)
//         {
//             _interpretSet = true;
//             this.goal = goal;
//             Cache();
//             if (!_cacheGameObject.activeInHierarchy) instant = true;
//             if (instant)
//             {
//                 StopInterpret();
//                 return;
//             }
//
//             UpdateGoal();
//         }
//
//         protected virtual void StopInterpret()
//         {
//             if (routine != null)
//             {
//                 StopCoroutine(routine);
//                 routine = null;
//             }
//             value = goal;
//             UpdatePosition(true);
//         }
//
//         protected abstract void UpdateGoal();
//
//         protected abstract IEnumerator ExecuteInterpret();
//
//         public virtual void Nudge(float strength) { }
//
//         public void OffsetGoal(float delta, bool instant = false) { SetGoal(goal + delta, instant); }
//
//         protected bool UpdatePosition(bool force = false)
//         {
//             bool result = false;
//             bool flag = force;
//             if (!flag)
//             {
//                 flag = (M.Abs(value - lastInterpretValue) > minimumDelta);
//             }
//
//             if (flag)
//             {
//                 result = true;
//                 lastInterpretValue = value;
//                 OnUpdated(value);
//             }
//
//             return result;
//         }
//
//         private void OnDisable() { StopInterpret(); }
//     }
// }