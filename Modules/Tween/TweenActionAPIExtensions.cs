using UnityEngine;
using UnityEngine.UI;

namespace Pancake.Tween
{
    /// <summary>
    /// Extends the TweenAction API to other objects.
    /// </summary>
    public static class TweenActionAPIExtensions
    {
        #region Action Move X Y Z

        /// <summary>
        /// Creates a TweenAction that moves the transform position x to [x].
        /// </summary>
        public static TweenAction ActionMoveX(this Transform transform, float x, float duration)
        {
            return TweenAction.CreateFloat(() => transform.position.x, (value) => transform.SetPositionX(value), x, duration);
        }


        /// <summary>
        /// Creates a TweenAction that moves the transform position y to [y].
        /// </summary>
        public static TweenAction ActionMoveY(this Transform transform, float y, float duration)
        {
            return TweenAction.CreateFloat(() => transform.position.y, (value) => transform.SetPositionY(value), y, duration);
        }


        /// <summary>
        /// Creates a TweenAction that moves the transform position z to [z].
        /// </summary>
        public static TweenAction ActionMoveZ(this Transform transform, float z, float duration)
        {
            return TweenAction.CreateFloat(() => transform.position.z, (value) => transform.SetPositionZ(value), z, duration);
        }

        #endregion


        #region Action Move XY

        /// <summary>
        /// Creates a TweenAction that moves the transform position xy to [v2].
        /// </summary>
        public static TweenAction ActionMoveXY(this Transform transform, in Vector2 v2, float duration)
        {
            return TweenAction.CreateVector2((out Vector2 vector2) => vector2 = transform.position,
                (in Vector2 vector2) => transform.SetPositionXY(vector2),
                v2,
                duration);
        }


        /// <summary>
        /// Creates a TweenAction that moves the transform position xy to [xy].
        /// </summary>
        public static TweenAction ActionMoveXY(this Transform transform, float x, float y, float duration)
        {
            return ActionMoveXY(transform, new Vector2(x, y), duration);
        }


        /// <summary>
        /// Creates a TweenAction that moves the transform position xy to [target].
        /// </summary>
        public static TweenAction ActionMoveXY(this Transform transform, Transform target, float duration) { return ActionMoveXY(transform, target.position, duration); }

        #endregion


        #region Action Move XZ

        /// <summary>
        /// Creates a TweenAction that moves the transform position xz to [v2].
        /// </summary>
        public static TweenAction ActionMoveXZ(this Transform transform, in Vector2 v2, float duration)
        {
            return TweenAction.CreateVector2((out Vector2 vector2) => transform.GetPositionXZ(out vector2),
                (in Vector2 vector2) => transform.SetPositionXZ(in vector2),
                v2,
                duration);
        }


        /// <summary>
        /// Creates a TweenAction that moves the transform position xz to [xz].
        /// </summary>
        public static TweenAction ActionMoveXZ(this Transform transform, float x, float z, float duration)
        {
            return ActionMoveXZ(transform, new Vector2(x, z), duration);
        }


        /// <summary>
        /// Creates a TweenAction that moves the transform position xz to [target].
        /// </summary>
        public static TweenAction ActionMoveXZ(this Transform transform, Transform target, float duration)
        {
            return ActionMoveXZ(transform, target.GetPositionXZ(), duration);
        }

        #endregion


        #region Action Move YZ

        /// <summary>
        /// Creates a TweenAction that moves the transform position yz to [v2].
        /// </summary>
        public static TweenAction ActionMoveYZ(this Transform transform, in Vector2 v2, float duration)
        {
            return TweenAction.CreateVector2((out Vector2 vector2) => transform.GetPositionYZ(out vector2),
                (in Vector2 vector2) => transform.SetPositionYZ(in vector2),
                v2,
                duration);
        }


        /// <summary>
        /// Creates a TweenAction that moves the transform position yz to [yz].
        /// </summary>
        public static TweenAction ActionMoveYZ(this Transform transform, float y, float z, float duration)
        {
            return ActionMoveYZ(transform, new Vector2(y, z), duration);
        }


        /// <summary>
        /// Creates a TweenAction that moves the transform position yz to [target].
        /// </summary>
        public static TweenAction ActionMoveYZ(this Transform transform, Transform target, float duration)
        {
            return ActionMoveYZ(transform, target.GetPositionYZ(), duration);
        }

        #endregion


        #region Action Move

        /// <summary>
        /// Creates a TweenAction that moves the transform position to [v3].
        /// </summary>
        public static TweenAction ActionMove(this Transform transform, in Vector3 v3, float duration)
        {
            return TweenAction.CreateVector3((out Vector3 vector3) => vector3 = transform.position, (in Vector3 vector3) => transform.position = vector3, v3, duration);
        }


        /// <summary>
        /// Creates a TweenAction that moves the transform position to [xyz].
        /// </summary>
        public static TweenAction ActionMove(this Transform transform, float x, float y, float z, float duration)
        {
            return ActionMove(transform, new Vector3(x, y, z), duration);
        }


        /// <summary>
        /// Creates a TweenAction that moves the transform position to [target].
        /// </summary>
        public static TweenAction ActionMove(this Transform transform, Transform target, float duration) { return ActionMove(transform, target.position, duration); }

        #endregion


        #region Action Local Move X Y Z

        /// <summary>
        /// Creates a TweenAction that moves the transform localPosition x to [x].
        /// </summary>
        public static TweenAction ActionLocalMoveX(this Transform transform, float x, float duration)
        {
            return TweenAction.CreateFloat(() => transform.localPosition.x, (value) => transform.SetLocalPositionX(value), x, duration);
        }


        /// <summary>
        /// Creates a TweenAction that moves the transform localPosition y to [y].
        /// </summary>
        public static TweenAction ActionLocalMoveY(this Transform transform, float y, float duration)
        {
            return TweenAction.CreateFloat(() => transform.localPosition.y, (value) => transform.SetLocalPositionY(value), y, duration);
        }


        /// <summary>
        /// Creates a TweenAction that moves the transform localPosition z to [z].
        /// </summary>
        public static TweenAction ActionLocalMoveZ(this Transform transform, float z, float duration)
        {
            return TweenAction.CreateFloat(() => transform.localPosition.z, (value) => transform.SetLocalPositionZ(value), z, duration);
        }

        #endregion


        #region Action Local Move XY

        /// <summary>
        /// Creates a TweenAction that moves the transform localPosition xy to [v2].
        /// </summary>
        public static TweenAction ActionLocalMoveXY(this Transform transform, in Vector2 v2, float duration)
        {
            return TweenAction.CreateVector2((out Vector2 vector2) => vector2 = transform.localPosition,
                (in Vector2 vector2) => transform.SetLocalPositionXY(vector2),
                v2,
                duration);
        }


        /// <summary>
        /// Creates a TweenAction that moves the transform localPosition xy to [xy].
        /// </summary>
        public static TweenAction ActionLocalMoveXY(this Transform transform, float x, float y, float duration)
        {
            return ActionLocalMoveXY(transform, new Vector2(x, y), duration);
        }


        /// <summary>
        /// Creates a TweenAction that moves the transform localPosition xy to [target].
        /// </summary>
        public static TweenAction ActionLocalMoveXY(this Transform transform, Transform target, float duration)
        {
            return ActionLocalMoveXY(transform, target.localPosition, duration);
        }

        #endregion


        #region Action Local Move XZ

        /// <summary>
        /// Creates a TweenAction that moves the transform localPosition xz to [v2].
        /// </summary>
        public static TweenAction ActionLocalMoveXZ(this Transform transform, in Vector2 v2, float duration)
        {
            return TweenAction.CreateVector2((out Vector2 vector2) => transform.GetLocalPositionXZ(out vector2),
                (in Vector2 vector2) => transform.SetLocalPositionXZ(in vector2),
                v2,
                duration);
        }


        /// <summary>
        /// Creates a TweenAction that moves the transform localPosition xz to [xz].
        /// </summary>
        public static TweenAction ActionLocalMoveXZ(this Transform transform, float x, float z, float duration)
        {
            return ActionLocalMoveXZ(transform, new Vector2(x, z), duration);
        }


        /// <summary>
        /// Creates a TweenAction that moves the transform localPosition xz to [target].
        /// </summary>
        public static TweenAction ActionLocalMoveXZ(this Transform transform, Transform target, float duration)
        {
            return ActionLocalMoveXZ(transform, target.GetLocalPositionXZ(), duration);
        }

        #endregion


        #region Action Local Move YZ

        /// <summary>
        /// Creates a TweenAction that moves the transform localPosition yz to [v2].
        /// </summary>
        public static TweenAction ActionLocalMoveYZ(this Transform transform, in Vector2 v2, float duration)
        {
            return TweenAction.CreateVector2((out Vector2 vector2) => transform.GetLocalPositionYZ(out vector2),
                (in Vector2 vector2) => transform.SetLocalPositionYZ(in vector2),
                v2,
                duration);
        }


        /// <summary>
        /// Creates a TweenAction that moves the transform localPosition yz to [yz].
        /// </summary>
        public static TweenAction ActionLocalMoveYZ(this Transform transform, float y, float z, float duration)
        {
            return ActionLocalMoveYZ(transform, new Vector2(y, z), duration);
        }


        /// <summary>
        /// Creates a TweenAction that moves the transform localPosition yz to [target].
        /// </summary>
        public static TweenAction ActionLocalMoveYZ(this Transform transform, Transform target, float duration)
        {
            return ActionLocalMoveYZ(transform, target.GetLocalPositionYZ(), duration);
        }

        #endregion


        #region Action Local Move

        /// <summary>
        /// Creates a TweenAction that moves the transform localPosition to [v3].
        /// </summary>
        public static TweenAction ActionLocalMove(this Transform transform, in Vector3 v3, float duration)
        {
            return TweenAction.CreateVector3((out Vector3 vector3) => vector3 = transform.localPosition,
                (in Vector3 vector3) => transform.localPosition = vector3,
                v3,
                duration);
        }


        /// <summary>
        /// Creates a TweenAction that moves the transform localPosition to [xyz].
        /// </summary>
        public static TweenAction ActionLocalMove(this Transform transform, float x, float y, float z, float duration)
        {
            return ActionLocalMove(transform, new Vector3(x, y, z), duration);
        }


        /// <summary>
        /// Creates a TweenAction that moves the transform localPosition to [target].
        /// </summary>
        public static TweenAction ActionLocalMove(this Transform transform, Transform target, float duration)
        {
            return ActionLocalMove(transform, target.localPosition, duration);
        }

        #endregion


        #region Action Scale X Y Z

        /// <summary>
        /// Creates a TweenAction that scales the transform localScale x to [x].
        /// </summary>
        public static TweenAction ActionScaleX(this Transform transform, float x, float duration)
        {
            return TweenAction.CreateFloat(() => transform.localScale.x, (value) => transform.SetScaleX(value), x, duration);
        }


        /// <summary>
        /// Creates a TweenAction that scales the transform localScale y to [y].
        /// </summary>
        public static TweenAction ActionScaleY(this Transform transform, float y, float duration)
        {
            return TweenAction.CreateFloat(() => transform.localScale.y, (value) => transform.SetScaleY(value), y, duration);
        }


        /// <summary>
        /// Creates a TweenAction that scales the transform localScale z to [z].
        /// </summary>
        public static TweenAction ActionScaleZ(this Transform transform, float z, float duration)
        {
            return TweenAction.CreateFloat(() => transform.localScale.z, (value) => transform.SetScaleZ(value), z, duration);
        }

        #endregion


        #region Action Scale XY

        /// <summary>
        /// Creates a TweenAction that scales the transform localScale xy to [v2].
        /// </summary>
        public static TweenAction ActionScaleXY(this Transform transform, in Vector2 v2, float duration)
        {
            return TweenAction.CreateVector2((out Vector2 vector2) => vector2 = transform.localScale,
                (in Vector2 vector2) => transform.SetScaleXY(vector2),
                v2,
                duration);
        }


        /// <summary>
        /// Creates a TweenAction that scales the transform localScale xy to [xy].
        /// </summary>
        public static TweenAction ActionScaleXY(this Transform transform, float x, float y, float duration)
        {
            return ActionScaleXY(transform, new Vector2(x, y), duration);
        }


        /// <summary>
        /// Creates a TweenAction that scales the transform localScale xy to [value].
        /// </summary>
        public static TweenAction ActionScaleXY(this Transform transform, float value, float duration)
        {
            return ActionScaleXY(transform, new Vector2(value, value), duration);
        }


        /// <summary>
        /// Creates a TweenAction that scales the transform localScale xy to [target].
        /// </summary>
        public static TweenAction ActionScaleXY(this Transform transform, Transform target, float duration)
        {
            return ActionScaleXY(transform, target.localScale, duration);
        }

        #endregion


        #region Action Scale XZ

        /// <summary>
        /// Create a TweenAction that scales the transform localScale xz to [v2].
        /// </summary>
        public static TweenAction ActionScaleXZ(this Transform transform, in Vector2 v2, float duration)
        {
            return TweenAction.CreateVector2((out Vector2 vector2) => transform.GetScaleXZ(out vector2),
                (in Vector2 vector2) => transform.SetScaleXZ(in vector2),
                v2,
                duration);
        }


        /// <summary>
        /// Creates a TweenAction that scales the transform localScale xz to [xz].
        /// </summary>
        public static TweenAction ActionScaleXZ(this Transform transform, float x, float z, float duration)
        {
            return ActionScaleXZ(transform, new Vector2(x, z), duration);
        }


        /// <summary>
        /// Creates a TweenAction that scales the transform localScale xz to [value].
        /// </summary>
        public static TweenAction ActionScaleXZ(this Transform transform, float value, float duration)
        {
            return ActionScaleXZ(transform, new Vector2(value, value), duration);
        }


        /// <summary>
        /// Creates a TweenAction that scales the transform localScale xz to [target].
        /// </summary>
        public static TweenAction ActionScaleXZ(this Transform transform, Transform target, float duration)
        {
            return ActionScaleXZ(transform, target.GetScaleXZ(), duration);
        }

        #endregion


        #region Action Scale YZ

        /// <summary>
        /// Creates a TweenAction that scales the transform localScale z to [v2].
        /// </summary>
        public static TweenAction ActionScaleYZ(this Transform transform, in Vector2 v2, float duration)
        {
            return TweenAction.CreateVector2((out Vector2 vector2) => transform.GetScaleYZ(out vector2),
                (in Vector2 vector2) => transform.SetScaleYZ(in vector2),
                v2,
                duration);
        }


        /// <summary>
        /// Creates a TweenAction that scales the transform localScale yz to [yz].
        /// </summary>
        public static TweenAction ActionScaleYZ(this Transform transform, float y, float z, float duration)
        {
            return ActionScaleXZ(transform, new Vector2(y, z), duration);
        }


        /// <summary>
        /// Creates a TweenAction that scales the transform localScale yz to [value].
        /// </summary>
        public static TweenAction ActionScaleYZ(this Transform transform, float value, float duration)
        {
            return ActionScaleYZ(transform, new Vector2(value, value), duration);
        }


        /// <summary>
        /// Creates a TweenAction that scales the transform localScale yz to [target].
        /// </summary>
        public static TweenAction ActionScaleYZ(this Transform transform, Transform target, float duration)
        {
            return ActionScaleYZ(transform, target.GetScaleYZ(), duration);
        }

        #endregion


        #region Action Scale

        /// <summary>
        /// Creates a TweenAction that scales the transform localScale to [v3].
        /// </summary>
        public static TweenAction ActionScale(this Transform transform, in Vector3 v3, float duration)
        {
            return TweenAction.CreateVector3((out Vector3 vector3) => vector3 = transform.localScale,
                (in Vector3 vector3) => transform.localScale = vector3,
                v3,
                duration);
        }


        /// <summary>
        /// Creates a TweenAction that scales the transform localScale to [xyz].
        /// </summary>
        public static TweenAction ActionScale(this Transform transform, float x, float y, float z, float duration)
        {
            return ActionScale(transform, new Vector3(x, y, z), duration);
        }


        /// <summary>
        /// Creates a TweenAction that scales the transform localScale to [value].
        /// </summary>
        public static TweenAction ActionScale(this Transform transform, float value, float duration)
        {
            return ActionScale(transform, new Vector3(value, value, value), duration);
        }


        /// <summary>
        /// Creates a TweenAction that scales the transform localScale to [target].
        /// </summary>
        public static TweenAction ActionScale(this Transform transform, Transform target, float duration) { return ActionScale(transform, target.localScale, duration); }

        #endregion


        #region Action Rotate X Y Z

        /// <summary>
        /// Creates a TweenAction that rotates the transform eulerAngles x to [x].
        /// </summary>
        public static TweenAction ActionRotateX(this Transform transform, float x, float duration)
        {
            return TweenAction.CreateFloat(() => transform.eulerAngles.x, (value) => transform.SetRotationX(value), x, duration);
        }


        /// <summary>
        /// Creates a TweenAction that rotates the transform eulerAngles y to [y].
        /// </summary>
        public static TweenAction ActionRotateY(this Transform transform, float y, float duration)
        {
            return TweenAction.CreateFloat(() => transform.eulerAngles.y, (value) => transform.SetRotationY(value), y, duration);
        }


        /// <summary>
        /// Creates a TweenAction that rotates the transform eulerAngles z to [z].
        /// </summary>
        public static TweenAction ActionRotateZ(this Transform transform, float z, float duration)
        {
            return TweenAction.CreateFloat(() => transform.eulerAngles.z, (value) => transform.SetRotationZ(value), z, duration);
        }

        #endregion


        #region Action Rotate XY

        /// <summary>
        /// Creates a TweenAction that rotates the transform eulerAngles xy to [v2].
        /// </summary>
        public static TweenAction ActionRotateXY(this Transform transform, in Vector2 v2, float duration)
        {
            return TweenAction.CreateVector2((out Vector2 vector2) => vector2 = transform.eulerAngles,
                (in Vector2 vector2) => transform.SetRotationXY(vector2),
                v2,
                duration);
        }


        /// <summary>
        /// Creates a TweenAction that rotates the transform eulerAngles xy to [xy].
        /// </summary>
        public static TweenAction ActionRotateXY(this Transform transform, float x, float y, float duration)
        {
            return ActionRotateXY(transform, new Vector2(x, y), duration);
        }


        /// <summary>
        /// Creates a TweenAction that rotates the transform eulerAngles xy to [target].
        /// </summary>
        public static TweenAction ActionRotateXY(this Transform transform, Transform target, float duration)
        {
            return ActionRotateXY(transform, target.eulerAngles, duration);
        }

        #endregion


        #region Action Rotate XZ

        /// <summary>
        /// Creates a TweenAction that rotates the transform eulerAngles xz to [v2].
        /// </summary>
        public static TweenAction ActionRotateXZ(this Transform transform, in Vector2 v2, float duration)
        {
            return TweenAction.CreateVector2((out Vector2 vector2) => transform.GetRotationXZ(out vector2),
                (in Vector2 vector2) => transform.SetRotationXZ(in vector2),
                v2,
                duration);
        }


        /// <summary>
        /// Creates a TweenAction that rotates the transform eulerAngles xz to [xz].
        /// </summary>
        public static TweenAction ActionRotateXZ(this Transform transform, float x, float z, float duration)
        {
            return ActionRotateXZ(transform, new Vector2(x, z), duration);
        }


        /// <summary>
        /// Creates a TweenAction that rotates the transform eulerAngles xz to [target].
        /// </summary>
        public static TweenAction ActionRotateXZ(this Transform transform, Transform target, float duration)
        {
            return ActionRotateXZ(transform, target.GetRotationXZ(), duration);
        }

        #endregion


        #region Action Rotate YZ

        /// <summary>
        /// Creates a TweenAction that rotates the transform eulerAngles yz to [v2].
        /// </summary>
        public static TweenAction ActionRotateYZ(this Transform transform, in Vector2 v2, float duration)
        {
            return TweenAction.CreateVector2((out Vector2 vector2) => transform.GetRotationYZ(out vector2),
                (in Vector2 vector2) => transform.SetRotationYZ(in vector2),
                v2,
                duration);
        }


        /// <summary>
        /// Creates a TweenAction that rotates the transform eulerAngles yz to [xz].
        /// </summary>
        public static TweenAction ActionRotateYZ(this Transform transform, float y, float z, float duration)
        {
            return ActionRotateYZ(transform, new Vector2(y, z), duration);
        }


        /// <summary>
        /// Creates a TweenAction that rotates the transform eulerAngles yz to [target].
        /// </summary>
        public static TweenAction ActionRotateYZ(this Transform transform, Transform target, float duration)
        {
            return ActionRotateYZ(transform, target.GetRotationYZ(), duration);
        }

        #endregion


        #region Action Rotate

        /// <summary>
        /// Creates a TweenAction that rotates the transform eulerAngles to [v3].
        /// </summary>
        public static TweenAction ActionRotate(this Transform transform, in Vector3 v3, float duration)
        {
            return TweenAction.CreateVector3((out Vector3 vector3) => vector3 = transform.eulerAngles,
                (in Vector3 vector3) => transform.eulerAngles = vector3,
                v3,
                duration);
        }


        /// <summary>
        /// Creates a TweenAction that rotates the transform eulerAngles to [xyz].
        /// </summary>
        public static TweenAction ActionRotate(this Transform transform, float x, float y, float z, float duration)
        {
            return ActionRotate(transform, new Vector3(x, y, z), duration);
        }


        /// <summary>
        /// Creates a TweenAction that rotates the transform eulerAngles to [target].
        /// </summary>
        public static TweenAction ActionRotate(this Transform transform, Transform target, float duration)
        {
            return ActionRotate(transform, target.eulerAngles, duration);
        }

        #endregion


        #region Action Local Rotate X Y Z

        /// <summary>
        /// Creates a TweenAction that rotates the transform localEulerAngles x to [x].
        /// </summary>
        public static TweenAction ActionLocalRotateX(this Transform transform, float x, float duration)
        {
            return TweenAction.CreateFloat(() => transform.localEulerAngles.x, (value) => transform.SetLocalRotationX(value), x, duration);
        }


        /// <summary>
        /// Creates a TweenAction that rotates the transform localEulerAngles y to [y].
        /// </summary>
        public static TweenAction ActionLocalRotateY(this Transform transform, float y, float duration)
        {
            return TweenAction.CreateFloat(() => transform.localEulerAngles.y, (value) => transform.SetLocalRotationY(value), y, duration);
        }


        /// <summary>
        /// Creates a TweenAction that rotates the transform localEulerAngles z to [z].
        /// </summary>
        public static TweenAction ActionLocalRotateZ(this Transform transform, float z, float duration)
        {
            return TweenAction.CreateFloat(() => transform.localEulerAngles.z, (value) => transform.SetLocalRotationZ(value), z, duration);
        }

        #endregion


        #region Action Local Rotate XY

        /// <summary>
        /// Creates a TweenAction that rotates the transform localEulerAngles xy to [v2].
        /// </summary>
        public static TweenAction ActionLocalRotateXY(this Transform transform, in Vector2 v2, float duration)
        {
            return TweenAction.CreateVector2((out Vector2 vector2) => vector2 = transform.localEulerAngles,
                (in Vector2 vector2) => transform.SetLocalRotationXY(vector2),
                v2,
                duration);
        }


        /// <summary>
        /// Creates a TweenAction that rotates the transform localEulerAngles xy to [xy].
        /// </summary>
        public static TweenAction ActionLocalRotateXY(this Transform transform, float x, float y, float duration)
        {
            return ActionLocalRotateXY(transform, new Vector2(x, y), duration);
        }


        /// <summary>
        /// Creates a TweenAction that rotates the transform localEulerAngles xy to [target].
        /// </summary>
        public static TweenAction ActionLocalRotateXY(this Transform transform, Transform target, float duration)
        {
            return ActionLocalRotateXY(transform, target.GetLocalRotationXZ(), duration);
        }

        #endregion


        #region Action Local Rotate XZ

        /// <summary>
        /// Creates a TweenAction that rotates the transform localEulerAngles xz to [v2].
        /// </summary>
        public static TweenAction ActionLocalRotateXZ(this Transform transform, in Vector2 v2, float duration)
        {
            return TweenAction.CreateVector2((out Vector2 vector2) => transform.GetLocalRotationXZ(out vector2),
                (in Vector2 vector2) => transform.SetLocalRotationXZ(in vector2),
                v2,
                duration);
        }


        /// <summary>
        /// Creates a TweenAction that rotates the transform localEulerAngles xz to [xz].
        /// </summary>
        public static TweenAction ActionLocalRotateXZ(this Transform transform, float x, float z, float duration)
        {
            return ActionLocalRotateXZ(transform, new Vector2(x, z), duration);
        }


        /// <summary>
        /// Creates a TweenAction that rotates the transform localEulerAngles xz to [target].
        /// </summary>
        public static TweenAction ActionLocalRotateXZ(this Transform transform, Transform target, float duration)
        {
            return ActionLocalRotateXZ(transform, target.GetLocalRotationXZ(), duration);
        }

        #endregion


        #region Action Local Rotate YZ

        /// <summary>
        /// Creates a TweenAction that rotates the transform localEulerAngles yz to [v2].
        /// </summary>
        public static TweenAction ActionLocalRotateYZ(this Transform transform, in Vector2 v2, float duration)
        {
            return TweenAction.CreateVector2((out Vector2 vector2) => transform.GetLocalRotationYZ(out vector2),
                (in Vector2 vector2) => transform.SetLocalRotationYZ(in vector2),
                v2,
                duration);
        }


        /// <summary>
        /// Creates a TweenAction that rotates the transform localEulerAngles yz to [xz].
        /// </summary>
        public static TweenAction ActionLocalRotateYZ(this Transform transform, float y, float z, float duration)
        {
            return ActionLocalRotateYZ(transform, new Vector2(y, z), duration);
        }


        /// <summary>
        /// Creates a TweenAction that rotates the transform ActionLocalRotateYZ yz to [target].
        /// </summary>
        public static TweenAction ActionLocalRotateYZ(this Transform transform, Transform target, float duration)
        {
            return ActionLocalRotateYZ(transform, target.GetLocalRotationYZ(), duration);
        }

        #endregion


        #region Action Local Rotate

        /// <summary>
        /// Creates a TweenAction that rotates the transform localEulerAngles to [v3].
        /// </summary>
        public static TweenAction ActionLocalRotate(this Transform transform, in Vector3 v3, float duration)
        {
            return TweenAction.CreateVector3((out Vector3 vector3) => vector3 = transform.localEulerAngles,
                (in Vector3 vector3) => transform.localEulerAngles = vector3,
                v3,
                duration);
        }


        /// <summary>
        /// Creates a TweenAction that rotates the transform localEulerAngles to [xyz].
        /// </summary>
        public static TweenAction ActionLocalRotate(this Transform transform, float x, float y, float z, float duration)
        {
            return ActionLocalRotate(transform, new Vector3(x, y, z), duration);
        }


        /// <summary>
        /// Creates a TweenAction that rotates the transform localEulerAngles to [target].
        /// </summary>
        public static TweenAction ActionLocalRotate(this Transform transform, Transform target, float duration)
        {
            return ActionLocalRotate(transform, target.localEulerAngles, duration);
        }

        #endregion


        #region Action Shake Position

        /// <summary>
        /// Creates a TweenAction that shakes the transform position x by [amplitude] and [speed].
        /// Note: don't change the isRelative and TweenEase of this TweenAction.
        /// </summary>
        public static TweenAction ActionShakePositionX(this Transform transform, float amplitude, float speed, float duration)
        {
            return TweenAction.CreateFloat(() => transform.position.x, (value) => transform.SetPositionX(value), 0.0f, duration)
                .SetRelative(true)
                .SetEase(Ease.ShakeX)
                .SetExtraParams(amplitude, speed);
        }


        /// <summary>
        /// Creates a TweenAction that shakes the transform position y by [amplitude] and [speed].
        /// Note: don't change the isRelative and TweenEase of this TweenAction.
        /// </summary>
        public static TweenAction ActionShakePositionY(this Transform transform, float amplitude, float speed, float duration)
        {
            return TweenAction.CreateFloat(() => transform.position.y, (value) => transform.SetPositionY(value), 0.0f, duration)
                .SetRelative(true)
                .SetEase(Ease.ShakeY)
                .SetExtraParams(amplitude, speed);
        }


        /// <summary>
        /// Creates a TweenAction that shakes the transform position z by [amplitude] and [speed].
        /// Note: don't change the isRelative and TweenEase of this TweenAction.
        /// </summary>
        public static TweenAction ActionShakePositionZ(this Transform transform, float amplitude, float speed, float duration)
        {
            return TweenAction.CreateFloat(() => transform.position.z, (value) => transform.SetPositionZ(value), 0.0f, duration)
                .SetRelative(true)
                .SetEase(Ease.ShakeZ)
                .SetExtraParams(amplitude, speed);
        }


        /// <summary>
        /// Creates a TweenAction that shakes the transform position xy by [amplitude] and [speed].
        /// Note: don't change the isRelative and TweenEase of this TweenAction.
        /// </summary>
        public static TweenAction ActionShakePositionXY(this Transform transform, float amplitude, float speed, float duration)
        {
            return TweenAction
                .CreateVector2((out Vector2 vector2) => vector2 = transform.position, (in Vector2 vector2) => transform.SetPositionXY(vector2), Vector2.zero, duration)
                .SetRelative(true)
                .SetEaseAt(0, Ease.ShakeX)
                .SetEaseAt(1, Ease.ShakeY)
                .SetExtraParams(amplitude, speed);
        }


        /// <summary>
        /// Creates a TweenAction that shakes the transform position xz by [amplitude] and [speed].
        /// Note: don't change the isRelative and TweenEase of this TweenAction.
        /// </summary>
        public static TweenAction ActionShakePositionXZ(this Transform transform, float amplitude, float speed, float duration)
        {
            return TweenAction
                .CreateVector2((out Vector2 vector2) => transform.GetPositionXZ(out vector2),
                    (in Vector2 vector2) => transform.SetPositionXZ(in vector2),
                    Vector2.zero,
                    duration)
                .SetRelative(true)
                .SetEaseAt(0, Ease.ShakeX)
                .SetEaseAt(1, Ease.ShakeZ)
                .SetExtraParams(amplitude, speed);
        }


        /// <summary>
        /// Creates a TweenAction that shakes the transform position yz by [amplitude] and [speed].
        /// Note: don't change the isRelative and TweenEase of this TweenAction.
        /// </summary>
        public static TweenAction ActionShakePositionYZ(this Transform transform, float amplitude, float speed, float duration)
        {
            return TweenAction
                .CreateVector2((out Vector2 vector2) => transform.GetPositionYZ(out vector2),
                    (in Vector2 vector2) => transform.SetPositionYZ(in vector2),
                    Vector2.zero,
                    duration)
                .SetRelative(true)
                .SetEaseAt(0, Ease.ShakeY)
                .SetEaseAt(1, Ease.ShakeZ)
                .SetExtraParams(amplitude, speed);
        }


        /// <summary>
        /// Creates a TweenAction that shakes the transform position by [amplitude] and [speed].
        /// Note: don't change the isRelative and TweenEase of this TweenAction.
        /// </summary>
        public static TweenAction ActionShakePosition(this Transform transform, float amplitude, float speed, float duration)
        {
            return TweenAction
                .CreateVector3((out Vector3 vector3) => vector3 = transform.position, (in Vector3 vector3) => transform.position = vector3, Vector3.zero, duration)
                .SetRelative(true)
                .SetEaseAt(0, Ease.ShakeX)
                .SetEaseAt(1, Ease.ShakeY)
                .SetEaseAt(2, Ease.ShakeZ)
                .SetExtraParams(amplitude, speed);
        }

        #endregion


        #region Action Shake Scale

        /// <summary>
        /// Creates a TweenAction that shakes the transform scale x by [amplitude] and [speed].
        /// Note: don't change the isRelative and TweenEase of this TweenAction.
        /// </summary>
        public static TweenAction ActionShakeScaleX(this Transform transform, float amplitude, float speed, float duration)
        {
            return TweenAction.CreateFloat(() => transform.localScale.x, (value) => transform.SetScaleX(value), 0.0f, duration)
                .SetRelative(true)
                .SetEase(Ease.ShakeX)
                .SetExtraParams(amplitude, speed);
        }


        /// <summary>
        /// Creates a TweenAction that shakes the transform scale y by [amplitude] and [speed].
        /// Note: don't change the isRelative and TweenEase of this TweenAction.
        /// </summary>
        public static TweenAction ActionShakeScaleY(this Transform transform, float amplitude, float speed, float duration)
        {
            return TweenAction.CreateFloat(() => transform.localScale.y, (value) => transform.SetScaleY(value), 0.0f, duration)
                .SetRelative(true)
                .SetEase(Ease.ShakeY)
                .SetExtraParams(amplitude, speed);
        }


        /// <summary>
        /// Creates a TweenAction that shakes the transform scale z by [amplitude] and [speed].
        /// Note: don't change the isRelative and TweenEase of this TweenAction.
        /// </summary>
        public static TweenAction ActionShakeScaleZ(this Transform transform, float amplitude, float speed, float duration)
        {
            return TweenAction.CreateFloat(() => transform.localScale.z, (value) => transform.SetScaleZ(value), 0.0f, duration)
                .SetRelative(true)
                .SetEase(Ease.ShakeZ)
                .SetExtraParams(amplitude, speed);
        }


        /// <summary>
        /// Creates a TweenAction that shakes the transform scale xy by [amplitude] and [speed].
        /// Note: don't change the isRelative and TweenEase of this TweenAction.
        /// </summary>
        public static TweenAction ActionShakeScaleXY(this Transform transform, float amplitude, float speed, float duration)
        {
            return TweenAction
                .CreateVector2((out Vector2 vector2) => vector2 = transform.localScale, (in Vector2 vector2) => transform.SetScaleXY(vector2), Vector2.zero, duration)
                .SetRelative(true)
                .SetEaseAt(0, Ease.ShakeX)
                .SetEaseAt(1, Ease.ShakeY)
                .SetExtraParams(amplitude, speed);
        }


        /// <summary>
        /// Creates a TweenAction that shakes the transform scale xz by [amplitude] and [speed].
        /// Note: don't change the isRelative and TweenEase of this TweenAction.
        /// </summary>
        public static TweenAction ActionShakeScaleXZ(this Transform transform, float amplitude, float speed, float duration)
        {
            return TweenAction
                .CreateVector2((out Vector2 vector2) => transform.GetScaleXZ(out vector2),
                    (in Vector2 vector2) => transform.SetScaleXZ(in vector2),
                    Vector2.zero,
                    duration)
                .SetRelative(true)
                .SetEaseAt(0, Ease.ShakeX)
                .SetEaseAt(1, Ease.ShakeZ)
                .SetExtraParams(amplitude, speed);
        }


        /// <summary>
        /// Creates a TweenAction that shakes the transform scale yz by [amplitude] and [speed].
        /// Note: don't change the isRelative and TweenEase of this TweenAction.
        /// </summary>
        public static TweenAction ActionShakeScaleYZ(this Transform transform, float amplitude, float speed, float duration)
        {
            return TweenAction
                .CreateVector2((out Vector2 vector2) => transform.GetScaleYZ(out vector2),
                    (in Vector2 vector2) => transform.SetScaleYZ(in vector2),
                    Vector2.zero,
                    duration)
                .SetRelative(true)
                .SetEaseAt(0, Ease.ShakeY)
                .SetEaseAt(1, Ease.ShakeZ)
                .SetExtraParams(amplitude, speed);
        }


        /// <summary>
        /// Creates a TweenAction that shakes the transform scale by [amplitude] and [speed].
        /// Note: don't change the isRelative and TweenEase of this TweenAction.
        /// </summary>
        public static TweenAction ActionShakeScale(this Transform transform, float amplitude, float speed, float duration)
        {
            return TweenAction
                .CreateVector3((out Vector3 vector3) => vector3 = transform.localScale, (in Vector3 vector3) => transform.localScale = vector3, Vector3.zero, duration)
                .SetRelative(true)
                .SetEaseAt(0, Ease.ShakeX)
                .SetEaseAt(1, Ease.ShakeY)
                .SetEaseAt(2, Ease.ShakeZ)
                .SetExtraParams(amplitude, speed);
        }

        #endregion


        #region Action Shake Rotation

        /// <summary>
        /// Creates a TweenAction that shakes the transform rotation x by [amplitude] and [speed].
        /// Note: don't change the isRelative and TweenEase of this TweenAction.
        /// </summary>
        public static TweenAction ActionShakeRotationX(this Transform transform, float amplitude, float speed, float duration)
        {
            return TweenAction.CreateFloat(() => transform.eulerAngles.x, (value) => transform.SetRotationX(value), 0.0f, duration)
                .SetRelative(true)
                .SetEase(Ease.ShakeX)
                .SetExtraParams(amplitude, speed);
        }


        /// <summary>
        /// Creates a TweenAction that shakes the transform rotation y by [amplitude] and [speed].
        /// Note: don't change the isRelative and TweenEase of this TweenAction.
        /// </summary>
        public static TweenAction ActionShakeRotationY(this Transform transform, float amplitude, float speed, float duration)
        {
            return TweenAction.CreateFloat(() => transform.eulerAngles.y, (value) => transform.SetRotationY(value), 0.0f, duration)
                .SetRelative(true)
                .SetEase(Ease.ShakeY)
                .SetExtraParams(amplitude, speed);
        }


        /// <summary>
        /// Creates a TweenAction that shakes the transform rotation z by [amplitude] and [speed].
        /// Note: don't change the isRelative and TweenEase of this TweenAction.
        /// </summary>
        public static TweenAction ActionShakeRotationZ(this Transform transform, float amplitude, float speed, float duration)
        {
            return TweenAction.CreateFloat(() => transform.eulerAngles.z, (value) => transform.SetRotationZ(value), 0.0f, duration)
                .SetRelative(true)
                .SetEase(Ease.ShakeZ)
                .SetExtraParams(amplitude, speed);
        }


        /// <summary>
        /// Creates a TweenAction that shakes the transform rotation xy by [amplitude] and [speed].
        /// Note: don't change the isRelative and TweenEase of this TweenAction.
        /// </summary>
        public static TweenAction ActionShakeRotationXY(this Transform transform, float amplitude, float speed, float duration)
        {
            return TweenAction
                .CreateVector2((out Vector2 vector2) => vector2 = transform.eulerAngles, (in Vector2 vector2) => transform.SetRotationXY(vector2), Vector2.zero, duration)
                .SetRelative(true)
                .SetEase(Ease.ShakeX)
                .SetEase(Ease.ShakeY)
                .SetExtraParams(amplitude, speed);
        }


        /// <summary>
        /// Creates a TweenAction that shakes the transform rotation xz by [amplitude] and [speed].
        /// Note: don't change the isRelative and TweenEase of this TweenAction.
        /// </summary>
        public static TweenAction ActionShakeRotationXZ(this Transform transform, float amplitude, float speed, float duration)
        {
            return TweenAction
                .CreateVector2((out Vector2 vector2) => transform.GetRotationXZ(out vector2),
                    (in Vector2 vector2) => transform.SetRotationXZ(in vector2),
                    Vector2.zero,
                    duration)
                .SetRelative(true)
                .SetEase(Ease.ShakeX)
                .SetEase(Ease.ShakeZ)
                .SetExtraParams(amplitude, speed);
        }


        /// <summary>
        /// Creates a TweenAction that shakes the transform rotation yz by [amplitude] and [speed].
        /// Note: don't change the isRelative and TweenEase of this TweenAction.
        /// </summary>
        public static TweenAction ActionShakeRotationYZ(this Transform transform, float amplitude, float speed, float duration)
        {
            return TweenAction
                .CreateVector2((out Vector2 vector2) => transform.GetRotationYZ(out vector2),
                    (in Vector2 vector2) => transform.SetRotationYZ(in vector2),
                    Vector2.zero,
                    duration)
                .SetRelative(true)
                .SetEase(Ease.ShakeY)
                .SetEase(Ease.ShakeZ)
                .SetExtraParams(amplitude, speed);
        }


        /// <summary>
        /// Creates a TweenAction that shakes the transform rotation by [amplitude] and [speed].
        /// Note: don't change the isRelative and TweenEase of this TweenAction.
        /// </summary>
        public static TweenAction ActionShakeRotation(this Transform transform, float amplitude, float speed, float duration)
        {
            return TweenAction
                .CreateVector3((out Vector3 vector3) => vector3 = transform.eulerAngles, (in Vector3 vector3) => transform.eulerAngles = vector3, Vector3.zero, duration)
                .SetRelative(true)
                .SetEaseAt(0, Ease.ShakeX)
                .SetEaseAt(1, Ease.ShakeY)
                .SetEaseAt(2, Ease.ShakeZ)
                .SetExtraParams(amplitude, speed);
        }

        #endregion


        #region Action Bezier Quadratic Move XY

        /// <summary>
        /// Creates a TweenAction that moves the transform position xy to [v2] by bezier2 with [controlPos].
        /// Note: don't change the TweenEase of this TweenAction.
        /// </summary>
        public static TweenAction ActionBezier2MoveXY(this Transform transform, in Vector2 v2, in Vector2 controlPos, float duration)
        {
            return TweenAction
                .CreateVector2((out Vector2 vector2) => vector2 = transform.position, (in Vector2 vector2) => transform.SetPositionXY(vector2), v2, duration)
                .SetEaseAt(0, Ease.BezierQuadraticX)
                .SetEaseAt(1, Ease.BezierQuadraticY)
                .SetExtraParams(controlPos.x, controlPos.y);
        }


        /// <summary>
        /// Creates a TweenAction that moves the transform position xy to [xy] by bezier2 with [controlPos].
        /// Note: don't change the TweenEase of this TweenAction.
        /// </summary>
        public static TweenAction ActionBezier2MoveXY(this Transform transform, float x, float y, float controlPosX, float controlPosY, float duration)
        {
            return ActionBezier2MoveXY(transform, new Vector2(x, y), new Vector2(controlPosX, controlPosY), duration);
        }


        /// <summary>
        /// Creates a TweenAction that moves the transform position xy to [target] with bezier2 by [controlPos].
        /// Note: don't change the TweenEase of this TweenAction.
        /// </summary>
        public static TweenAction ActionBezier2MoveXY(this Transform transform, Transform target, Transform controlPos, float duration)
        {
            return ActionBezier2MoveXY(transform, target.position, controlPos.position, duration);
        }

        #endregion


        #region Action Bezier Quadratic Move XZ

        /// <summary>
        /// Creates a TweenAction that moves the transform position xz to [v2] by bezier2 with [controlPos].
        /// Note: don't change the TweenEase of this TweenAction.
        /// </summary>
        public static TweenAction ActionBezier2MoveXZ(this Transform transform, in Vector2 v2, in Vector2 controlPos, float duration)
        {
            return TweenAction
                .CreateVector2((out Vector2 vector2) => transform.GetPositionXZ(out vector2), (in Vector2 vector2) => transform.SetPositionXZ(in vector2), v2, duration)
                .SetEaseAt(0, Ease.BezierQuadraticX)
                .SetEaseAt(1, Ease.BezierQuadraticZ)
                .SetExtraParams(controlPos.x, controlPos.y);
        }


        /// <summary>
        /// Creates a TweenAction that moves the transform position xz to [xz] by bezier2 with [controlPos].
        /// Note: don't change the TweenEase of this TweenAction.
        /// </summary>
        public static TweenAction ActionBezier2MoveXZ(this Transform transform, float x, float z, float controlPosX, float controlPosZ, float duration)
        {
            return ActionBezier2MoveXZ(transform, new Vector2(x, z), new Vector2(controlPosX, controlPosZ), duration);
        }


        /// <summary>
        /// Creates a TweenAction that moves the transform position xz to [target] by bezier2 with [controlPos].
        /// Note: don't change the TweenEase of this TweenAction.
        /// </summary>
        public static TweenAction ActionBezier2MoveXZ(this Transform transform, Transform target, Transform controlPos, float duration)
        {
            return ActionBezier2MoveXZ(transform, target.GetPositionXZ(), controlPos.GetPositionXZ(), duration);
        }

        #endregion


        #region Action Bezier Quadratic Move YZ

        /// <summary>
        /// Creates a TweenAction that moves the transform position yz to [v2] by bezier2 with [controlPos].
        /// Note: don't change the TweenEase of this TweenAction.
        /// </summary>
        public static TweenAction ActionBezier2MoveYZ(this Transform transform, in Vector2 v2, in Vector2 controlPos, float duration)
        {
            return TweenAction
                .CreateVector2((out Vector2 vector2) => transform.GetPositionYZ(out vector2), (in Vector2 vector2) => transform.SetPositionYZ(in vector2), v2, duration)
                .SetEaseAt(0, Ease.BezierQuadraticY)
                .SetEaseAt(1, Ease.BezierQuadraticZ)
                .SetExtraParams(controlPos.x, controlPos.y);
        }


        /// <summary>
        /// Creates a TweenAction that moves the transform position yz to [yz] by bezier2 with [controlPos].
        /// Note: don't change the TweenEase of this TweenAction.
        /// </summary>
        public static TweenAction ActionBezier2MoveYZ(this Transform transform, float y, float z, float controlPosY, float controlPosZ, float duration)
        {
            return ActionBezier2MoveYZ(transform, new Vector2(y, z), new Vector2(controlPosY, controlPosZ), duration);
        }


        /// <summary>
        /// Creates a TweenAction that moves the transform position yz to [target] by bezier2 with [controlPos].
        /// Note: don't change the TweenEase of this TweenAction.
        /// </summary>
        public static TweenAction ActionBezier2MoveYZ(this Transform transform, Transform target, Transform controlPos, float duration)
        {
            return ActionBezier2MoveYZ(transform, target.GetPositionYZ(), controlPos.GetPositionYZ(), duration);
        }

        #endregion


        #region Action Bezier Quadratic Move

        /// <summary>
        /// Creates a TweenAction that moves the transform position to [v3] by bezier2 with [controlPos].
        /// Note: don't change the TweenEase of this TweenAction.
        /// </summary>
        public static TweenAction ActionBezier2Move(this Transform transform, in Vector3 v3, in Vector3 controlPos, float duration)
        {
            return TweenAction.CreateVector3((out Vector3 vector3) => vector3 = transform.position, (in Vector3 vector3) => transform.position = vector3, v3, duration)
                .SetEaseAt(0, Ease.BezierQuadraticX)
                .SetEaseAt(1, Ease.BezierQuadraticY)
                .SetEaseAt(2, Ease.BezierQuadraticZ)
                .SetExtraParams(controlPos.x, controlPos.y, controlPos.z);
        }


        /// <summary>
        /// Creates a TweenAction that moves the transform position to [xyz] by bezier2 with [controlPos].
        /// Note: don't change the TweenEase of this TweenAction.
        /// </summary>
        public static TweenAction ActionBezier2Move(
            this Transform transform,
            float x,
            float y,
            float z,
            float controlPosX,
            float controlPosY,
            float controlPosZ,
            float duration)
        {
            return ActionBezier2Move(transform, new Vector3(x, y, z), new Vector3(controlPosX, controlPosY, controlPosZ), duration);
        }


        /// <summary>
        /// Creates a TweenAction that moves the transform position to [target] by bezier2 with [controlPos].
        /// Note: don't change the TweenEase of this TweenAction.
        /// </summary>
        public static TweenAction ActionBezier2Move(this Transform transform, Transform target, Transform controlPos, float duration)
        {
            return ActionBezier2Move(transform, target.position, controlPos.position, duration);
        }

        #endregion


        #region Action Bezier Quadratic Local Move XY

        /// <summary>
        /// Creates a TweenAction that moves the transform localPosition xy to [v2] by bezier2 with [controlPos].
        /// Note: don't change the TweenEase of this TweenAction.
        /// </summary>
        public static TweenAction ActionBezier2LocalMoveXY(this Transform transform, in Vector2 v2, in Vector2 controlPos, float duration)
        {
            return TweenAction
                .CreateVector2((out Vector2 vector2) => vector2 = transform.localPosition, (in Vector2 vector2) => transform.SetLocalPositionXY(vector2), v2, duration)
                .SetEaseAt(0, Ease.BezierQuadraticX)
                .SetEaseAt(1, Ease.BezierQuadraticY)
                .SetExtraParams(controlPos.x, controlPos.y);
        }


        /// <summary>
        /// Creates a TweenAction that moves the transform localPosition xy to [xy] by bezier2 with [controlPos].
        /// Note: don't change the TweenEase of this TweenAction.
        /// </summary>
        public static TweenAction ActionBezier2LocalMoveXY(this Transform transform, float x, float y, float controlPosX, float controlPosY, float duration)
        {
            return ActionBezier2LocalMoveXY(transform, new Vector2(x, y), new Vector2(controlPosX, controlPosY), duration);
        }


        /// <summary>
        /// Creates a TweenAction that moves the transform localPosition xy to [target] by bezier2 with [controlPos].
        /// Note: don't change the TweenEase of this TweenAction.
        /// </summary>
        public static TweenAction ActionBezier2LocalMoveXY(this Transform transform, Transform target, Transform controlPos, float duration)
        {
            return ActionBezier2LocalMoveXY(transform, target.localPosition, controlPos.localPosition, duration);
        }

        #endregion


        #region Action Bezier Quadratic Local Move XZ

        /// <summary>
        /// Creates a TweenAction that moves the transform localPosition xz to [v2] by bezier2 with [controlPos].
        /// Note: don't change the TweenEase of this TweenAction.
        /// </summary>
        public static TweenAction ActionBezier2LocalMoveXZ(this Transform transform, in Vector2 v2, in Vector2 controlPos, float duration)
        {
            return TweenAction
                .CreateVector2((out Vector2 vector2) => transform.GetLocalPositionXZ(out vector2),
                    (in Vector2 vector2) => transform.SetLocalPositionXZ(in vector2),
                    v2,
                    duration)
                .SetEaseAt(0, Ease.BezierQuadraticX)
                .SetEaseAt(1, Ease.BezierQuadraticZ)
                .SetExtraParams(controlPos.x, controlPos.y);
        }


        /// <summary>
        /// Creates a TweenAction that moves the transform localPosition xz to [xz] by bezier2 with [controlPos].
        /// Note: don't change the TweenEase of this TweenAction.
        /// </summary>
        public static TweenAction ActionBezier2LocalMoveXZ(this Transform transform, float x, float z, float controlPosX, float controlPosZ, float duration)
        {
            return ActionBezier2LocalMoveXZ(transform, new Vector2(x, z), new Vector2(controlPosX, controlPosZ), duration);
        }


        /// <summary>
        /// Creates a TweenAction that moves the transform localPosition xz to [target] by bezier2 with [controlPos].
        /// Note: don't change the TweenEase of this TweenAction.
        /// </summary>
        public static TweenAction ActionBezier2LocalMoveXZ(this Transform transform, Transform target, Transform controlPos, float duration)
        {
            return ActionBezier2LocalMoveXZ(transform, target.GetLocalPositionXZ(), controlPos.GetLocalPositionXZ(), duration);
        }

        #endregion


        #region Action Bezier Quadratic Local Move YZ

        /// <summary>
        /// Creates a TweenAction that moves the transform localPosition yz to [v2] by bezier2 with [controlPos].
        /// Note: don't change the TweenEase of this TweenAction.
        /// </summary>
        public static TweenAction ActionBezier2LocalMoveYZ(this Transform transform, in Vector2 v2, in Vector2 controlPos, float duration)
        {
            return TweenAction
                .CreateVector2((out Vector2 vector2) => transform.GetLocalPositionYZ(out vector2),
                    (in Vector2 vector2) => transform.SetLocalPositionYZ(in vector2),
                    v2,
                    duration)
                .SetEaseAt(0, Ease.BezierQuadraticY)
                .SetEaseAt(1, Ease.BezierQuadraticZ)
                .SetExtraParams(controlPos.x, controlPos.y);
        }


        /// <summary>
        /// Creates a TweenAction that moves the transform localPosition yz to [yz] by bezier2 with [controlPos].
        /// Note: don't change the TweenEase of this TweenAction.
        /// </summary>
        public static TweenAction ActionBezier2LocalMoveYZ(this Transform transform, float y, float z, float controlPosY, float controlPosZ, float duration)
        {
            return ActionBezier2LocalMoveYZ(transform, new Vector2(y, z), new Vector2(controlPosY, controlPosZ), duration);
        }


        /// <summary>
        /// Creates a TweenAction that moves the transform localPosition yz to [target] by bezier2 with [controlPos].
        /// Note: don't change the TweenEase of this TweenAction.
        /// </summary>
        public static TweenAction ActionBezier2LocalMoveYZ(this Transform transform, Transform target, Transform controlPos, float duration)
        {
            return ActionBezier2LocalMoveYZ(transform, target.GetLocalPositionYZ(), controlPos.GetLocalPositionYZ(), duration);
        }

        #endregion


        #region Action Bezier Quadratic Local Move

        /// <summary>
        /// Creates a TweenAction that moves the transform localPosition to [v3] by bezier2 with [controlPos].
        /// Note: don't change the TweenEase of this TweenAction.
        /// </summary>
        public static TweenAction ActionBezier2LocalMove(this Transform transform, in Vector3 v3, in Vector3 controlPos, float duration)
        {
            return TweenAction
                .CreateVector3((out Vector3 vector3) => vector3 = transform.localPosition, (in Vector3 vector3) => transform.localPosition = vector3, v3, duration)
                .SetEaseAt(0, Ease.BezierQuadraticX)
                .SetEaseAt(1, Ease.BezierQuadraticY)
                .SetEaseAt(2, Ease.BezierQuadraticZ)
                .SetExtraParams(controlPos.x, controlPos.y, controlPos.z);
        }


        /// <summary>
        /// Creates a TweenAction that moves the transform localPosition to [xyz] by bezier2 with [controlPos].
        /// Note: don't change the TweenEase of this TweenAction.
        /// </summary>
        public static TweenAction ActionBezier2LocalMove(
            this Transform transform,
            float x,
            float y,
            float z,
            float controlPosX,
            float controlPosY,
            float controlPosZ,
            float duration)
        {
            return ActionBezier2LocalMove(transform, new Vector3(x, y, z), new Vector3(controlPosX, controlPosY, controlPosZ), duration);
        }


        /// <summary>
        /// Creates a TweenAction that moves the transform localPosition to [target] by bezier2 with [controlPos].
        /// Note: don't change the TweenEase of this TweenAction.
        /// </summary>
        public static TweenAction ActionBezier2LocalMove(this Transform transform, Transform target, Transform controlPos, float duration)
        {
            return ActionBezier2LocalMove(transform, target.localPosition, controlPos.localPosition, duration);
        }

        #endregion


        #region Action Bezier Cubic Move XY

        /// <summary>
        /// Creates a TweenAction that moves the transform position xy to [v2] by bezier3 with [controlPos1] and [controlPos2].
        /// Note: don't change the TweenEase of this TweenAction.
        /// </summary>
        public static TweenAction ActionBezier3MoveXY(this Transform transform, in Vector2 v2, in Vector2 controlPos1, in Vector2 controlPos2, float duration)
        {
            return TweenAction
                .CreateVector2((out Vector2 vector2) => vector2 = transform.position, (in Vector2 vector2) => transform.SetPositionXY(vector2), v2, duration)
                .SetEaseAt(0, Ease.BezierCubicX)
                .SetEaseAt(1, Ease.BezierCubicY)
                .SetExtraParams(controlPos1.x, controlPos2.x, controlPos1.y, controlPos2.y);
        }


        /// <summary>
        /// Creates a TweenAction that moves the transform position xy to [xy] by bezier3 with [controlPos1] and [controlPos2].
        /// Note: don't change the TweenEase of this TweenAction.
        /// </summary>
        public static TweenAction ActionBezier3MoveXY(
            this Transform transform,
            float x,
            float y,
            float controlPos1X,
            float controlPos1Y,
            float controlPos2X,
            float controlPos2Y,
            float duration)
        {
            return ActionBezier3MoveXY(transform,
                new Vector2(x, y),
                new Vector2(controlPos1X, controlPos1Y),
                new Vector2(controlPos2X, controlPos2Y),
                duration);
        }


        /// <summary>
        /// Creates a TweenAction that moves the transform position xy to [target] by bezier3 with [controlPos1] and [controlPos2].
        /// Note: don't change the TweenEase of this TweenAction.
        /// </summary>
        public static TweenAction ActionBezier3MoveXY(this Transform transform, Transform target, Transform controlPos1, Transform controlPos2, float duration)
        {
            return ActionBezier3MoveXY(transform,
                target.position,
                controlPos1.position,
                controlPos2.position,
                duration);
        }

        #endregion


        #region Action Bezier Cubic Move XZ

        /// <summary>
        /// Creates a TweenAction that moves the transform position xz to [v2] by bezier3 with [controlPos1] and [controlPos2].
        /// Note: don't change the TweenEase of this TweenAction.
        /// </summary>
        public static TweenAction ActionBezier3MoveXZ(this Transform transform, in Vector2 v2, in Vector2 controlPos1, in Vector2 controlPos2, float duration)
        {
            return TweenAction
                .CreateVector2((out Vector2 vector2) => transform.GetPositionXZ(out vector2), (in Vector2 vector2) => transform.SetPositionXZ(in vector2), v2, duration)
                .SetEaseAt(0, Ease.BezierCubicX)
                .SetEaseAt(1, Ease.BezierCubicZ)
                .SetExtraParams(controlPos1.x, controlPos2.x, controlPos1.y, controlPos2.y);
        }


        /// <summary>
        /// Creates a TweenAction that moves the transform position xz to [xz] by bezier3 with [controlPos1] and [controlPos2].
        /// Note: don't change the TweenEase of this TweenAction.
        /// </summary>
        public static TweenAction ActionBezier3MoveXZ(
            this Transform transform,
            float x,
            float z,
            float controlPos1X,
            float controlPos1Z,
            float controlPos2X,
            float controlPos2Z,
            float duration)
        {
            return ActionBezier3MoveXZ(transform,
                new Vector2(x, z),
                new Vector2(controlPos1X, controlPos1Z),
                new Vector2(controlPos2X, controlPos2Z),
                duration);
        }


        /// <summary>
        /// Creates a TweenAction that moves the transform position xz to [target] by bezier3 with [controlPos1] and [controlPos2].
        /// Note: don't change the TweenEase of this TweenAction.
        /// </summary>
        public static TweenAction ActionBezier3MoveXZ(this Transform transform, Transform target, Transform controlPos1, Transform controlPos2, float duration)
        {
            return ActionBezier3MoveXZ(transform,
                target.GetPositionXZ(),
                controlPos1.GetPositionXZ(),
                controlPos2.GetPositionXZ(),
                duration);
        }

        #endregion


        #region Action Bezier Cubic Move YZ

        /// <summary>
        /// Creates a TweenAction that moves the transform position yz to [v2] by bezier3 with [controlPos1] and [controlPos2].
        /// Note: don't change the TweenEase of this TweenAction.
        /// </summary>
        public static TweenAction ActionBezier3MoveYZ(this Transform transform, in Vector2 v2, in Vector2 controlPos1, in Vector2 controlPos2, float duration)
        {
            return TweenAction
                .CreateVector2((out Vector2 vector2) => transform.GetPositionYZ(out vector2), (in Vector2 vector2) => transform.SetPositionYZ(in vector2), v2, duration)
                .SetEaseAt(0, Ease.BezierCubicY)
                .SetEaseAt(1, Ease.BezierCubicZ)
                .SetExtraParams(controlPos1.x, controlPos2.x, controlPos1.y, controlPos2.y);
        }


        /// <summary>
        /// Creates a TweenAction that moves the transform position yz to [yz] with bezier3 by [controlPos1] and [controlPos2].
        /// Note: don't change the TweenEase of this TweenAction.
        /// </summary>
        public static TweenAction ActionBezier3MoveYZ(
            this Transform transform,
            float y,
            float z,
            float controlPos1Y,
            float controlPos1Z,
            float controlPos2Y,
            float controlPos2Z,
            float duration)
        {
            return ActionBezier3MoveYZ(transform,
                new Vector2(y, z),
                new Vector2(controlPos1Y, controlPos1Z),
                new Vector2(controlPos2Y, controlPos2Z),
                duration);
        }


        /// <summary>
        /// Creates a TweenAction that moves the transform position yz to [target] by bezier3 with [controlPos1] and [controlPos2].
        /// Note: don't change the TweenEase of this TweenAction.
        /// </summary>
        public static TweenAction ActionBezier3MoveYZ(this Transform transform, Transform target, Transform controlPos1, Transform controlPos2, float duration)
        {
            return ActionBezier3MoveYZ(transform,
                target.GetPositionYZ(),
                controlPos1.GetPositionYZ(),
                controlPos2.GetPositionYZ(),
                duration);
        }

        #endregion


        #region Action Bezier Cubic Move

        /// <summary>
        /// Creates a TweenAction that moves the transform position to [v3] by bezier3 with [controlPos1] and [controlPos2].
        /// Note: don't change the TweenEase of this TweenAction.
        /// </summary>
        public static TweenAction ActionBezier3Move(this Transform transform, in Vector3 v3, in Vector3 controlPos1, in Vector3 controlPos2, float duration)
        {
            return TweenAction.CreateVector3((out Vector3 vector3) => vector3 = transform.position, (in Vector3 vector3) => transform.position = vector3, v3, duration)
                .SetEaseAt(0, Ease.BezierCubicX)
                .SetEaseAt(1, Ease.BezierCubicY)
                .SetEaseAt(2, Ease.BezierCubicZ)
                .SetExtraParams(controlPos1.x,
                    controlPos2.x,
                    controlPos1.y,
                    controlPos2.y,
                    controlPos1.z,
                    controlPos2.z);
        }


        /// <summary>
        /// Creates a TweenAction that moves the transform position to [xyz] by bezier3 with [controlPos1] and [controlPos2].
        /// Note: don't change the TweenEase of this TweenAction.
        /// </summary>
        public static TweenAction ActionBezier3Move(
            this Transform transform,
            float x,
            float y,
            float z,
            float controlPos1X,
            float controlPos1Y,
            float controlPos1Z,
            float controlPos2X,
            float controlPos2Y,
            float controlPos2Z,
            float duration)
        {
            return ActionBezier3Move(transform,
                new Vector3(x, y, z),
                new Vector3(controlPos1X, controlPos1Y, controlPos1Z),
                new Vector3(controlPos2X, controlPos2Y, controlPos2Z),
                duration);
        }


        /// <summary>
        /// Creates a TweenAction that moves the transform position to [target] by bezier3 with [controlPos1] and [controlPos2].
        /// Note: don't change the TweenEase of this TweenAction.
        /// </summary>
        public static TweenAction ActionBezier3Move(this Transform transform, Transform target, Transform controlPos1, Transform controlPos2, float duration)
        {
            return ActionBezier3Move(transform,
                target.position,
                controlPos1.position,
                controlPos2.position,
                duration);
        }

        #endregion


        #region Action Bezier Cubic Local Move XY

        /// <summary>
        /// Creates a TweenAction that moves the transform localPosition xy to [v2] by bezier3 with [controlPos1] and [controlPos2].
        /// Note: don't change the TweenEase of this TweenAction.
        /// </summary>
        public static TweenAction ActionBezier3LocalMoveXY(this Transform transform, in Vector2 v2, in Vector2 controlPos1, in Vector2 controlPos2, float duration)
        {
            return TweenAction
                .CreateVector2((out Vector2 vector2) => vector2 = transform.localPosition, (in Vector2 vector2) => transform.SetLocalPositionXY(vector2), v2, duration)
                .SetEaseAt(0, Ease.BezierCubicX)
                .SetEaseAt(1, Ease.BezierCubicY)
                .SetExtraParams(controlPos1.x, controlPos2.x, controlPos1.y, controlPos2.y);
        }


        /// <summary>
        /// Creates a TweenAction that moves the transform localPosition xy to [xy] by bezier3 with [controlPos1] and [controlPos2].
        /// Note: don't change the TweenEase of this TweenAction.
        /// </summary>
        public static TweenAction ActionBezier3LocalMoveXY(
            this Transform transform,
            float x,
            float y,
            float controlPos1X,
            float controlPos1Y,
            float controlPos2X,
            float controlPos2Y,
            float duration)
        {
            return ActionBezier3LocalMoveXY(transform,
                new Vector2(x, y),
                new Vector2(controlPos1X, controlPos1Y),
                new Vector2(controlPos2X, controlPos2Y),
                duration);
        }


        /// <summary>
        /// Creates a TweenAction that move the transform localPosition xy to [target] by bezier3 with [controlPos1] and [controlPos2].
        /// Note: don't change the TweenEase of this TweenAction.
        /// </summary>
        public static TweenAction ActionBezier3LocalMoveXY(this Transform transform, Transform toTransform, Transform controlPos1, Transform controlPos2, float duration)
        {
            return ActionBezier3LocalMoveXY(transform,
                toTransform.localPosition,
                controlPos1.localPosition,
                controlPos2.localPosition,
                duration);
        }

        #endregion


        #region Action Bezier Cubic Local Move XZ

        /// <summary>
        /// Creates a TweenAction that moves the transform localPosition xz to [v2] by bezier3 with [controlPos1] and [controlPos2].
        /// Note: don't change the TweenEase of this TweenAction.
        /// </summary>
        public static TweenAction ActionBezier3LocalMoveXZ(this Transform transform, in Vector2 v2, in Vector2 controlPos1, in Vector2 controlPos2, float duration)
        {
            return TweenAction
                .CreateVector2((out Vector2 vector2) => transform.GetLocalPositionXZ(out vector2),
                    (in Vector2 vector2) => transform.SetLocalPositionXZ(in vector2),
                    v2,
                    duration)
                .SetEaseAt(0, Ease.BezierCubicX)
                .SetEaseAt(1, Ease.BezierCubicZ)
                .SetExtraParams(controlPos1.x, controlPos2.x, controlPos1.y, controlPos2.y);
        }


        /// <summary>
        /// Creates a TweenAction that moves the transform localPosition xz to [xz] by bezier3 with [controlPos1] and [controlPos2].
        /// Note: don't change the TweenEase of this TweenAction.
        /// </summary>
        public static TweenAction ActionBezier3LocalMoveXZ(
            this Transform transform,
            float x,
            float z,
            float controlPos1X,
            float controlPos1Z,
            float controlPos2X,
            float controlPos2Z,
            float duration)
        {
            return ActionBezier3LocalMoveXZ(transform,
                new Vector2(x, z),
                new Vector2(controlPos1X, controlPos1Z),
                new Vector2(controlPos2X, controlPos2Z),
                duration);
        }


        /// <summary>
        /// Creates a TweenAction that moves the transform localPosition xz to [target] by bezier3 with [controlPos1] and [controlPos2].
        /// Note: don't change the TweenEase of this TweenAction.
        /// </summary>
        public static TweenAction ActionBezier3LocalMoveXZ(this Transform transform, Transform target, Transform controlPos1, Transform controlPos2, float duration)
        {
            return ActionBezier3LocalMoveXZ(transform,
                target.GetLocalPositionXZ(),
                controlPos1.GetLocalPositionXZ(),
                controlPos2.GetLocalPositionXZ(),
                duration);
        }

        #endregion


        #region Action Bezier Cubic Local Move YZ

        /// <summary>
        /// Create a TweenAction that move the transform localPosition yz to [v2] with bezier3 by [controlPos1] and [controlPos2].
        /// Note: don't change the TweenEase of this TweenAction.
        /// </summary>
        public static TweenAction ActionBezier3LocalMoveYZ(this Transform transform, in Vector2 v2, in Vector2 controlPos1, in Vector2 controlPos2, float duration)
        {
            return TweenAction
                .CreateVector2((out Vector2 vector2) => transform.GetLocalPositionYZ(out vector2),
                    (in Vector2 vector2) => transform.SetLocalPositionYZ(in vector2),
                    v2,
                    duration)
                .SetEaseAt(0, Ease.BezierCubicY)
                .SetEaseAt(1, Ease.BezierCubicZ)
                .SetExtraParams(controlPos1.x, controlPos2.x, controlPos1.y, controlPos2.y);
        }


        /// <summary>
        /// Create a TweenAction that move the transform localPosition yz to [yz] with bezier3 by [controlPos1] and [controlPos2].
        /// Note: don't change the TweenEase of this TweenAction.
        /// </summary>
        public static TweenAction ActionBezier3LocalMoveYZ(
            this Transform transform,
            float y,
            float z,
            float controlPos1Y,
            float controlPos1Z,
            float controlPos2Y,
            float controlPos2Z,
            float duration)
        {
            return ActionBezier3LocalMoveYZ(transform,
                new Vector2(y, z),
                new Vector2(controlPos1Y, controlPos1Z),
                new Vector2(controlPos2Y, controlPos2Z),
                duration);
        }


        /// <summary>
        /// Creates a TweenAction that moves the transform localPosition yz to [target] by bezier3 with [controlPos1] and [controlPos2].
        /// Note: don't change the TweenEase of this TweenAction.
        /// </summary>
        public static TweenAction ActionBezier3LocalMoveYZ(this Transform transform, Transform target, Transform controlPos1, Transform controlPos2, float duration)
        {
            return ActionBezier3LocalMoveYZ(transform,
                target.GetLocalPositionYZ(),
                controlPos1.GetLocalPositionYZ(),
                controlPos2.GetLocalPositionYZ(),
                duration);
        }

        #endregion


        #region Action Bezier Cubic Local Move

        /// <summary>
        /// Create a TweenAction that move the transform localPosition to [v3] with bezier3 by [controlPos1] and [controlPos2].
        /// Note: don't change the TweenEase of this TweenAction.
        /// </summary>
        public static TweenAction ActionBezier3LocalMove(this Transform transform, in Vector3 v3, in Vector3 controlPos1, in Vector3 controlPos2, float duration)
        {
            return TweenAction
                .CreateVector3((out Vector3 vector3) => vector3 = transform.localPosition, (in Vector3 vector3) => transform.localPosition = vector3, v3, duration)
                .SetEaseAt(0, Ease.BezierCubicX)
                .SetEaseAt(1, Ease.BezierCubicY)
                .SetEaseAt(2, Ease.BezierCubicZ)
                .SetExtraParams(controlPos1.x,
                    controlPos2.x,
                    controlPos1.y,
                    controlPos2.y,
                    controlPos1.z,
                    controlPos2.z);
        }


        /// <summary>
        /// Creates a TweenAction that moves the transform localPosition to [xyz] by bezier3 with [controlPos1] and [controlPos2].
        /// Note: don't change the TweenEase of this TweenAction.
        /// </summary>
        public static TweenAction ActionBezier3LocalMove(
            this Transform transform,
            float x,
            float y,
            float z,
            float controlPos1X,
            float controlPos1Y,
            float controlPos1Z,
            float controlPos2X,
            float controlPos2Y,
            float controlPos2Z,
            float duration)
        {
            return ActionBezier3LocalMove(transform,
                new Vector3(x, y, z),
                new Vector3(controlPos1X, controlPos1Y, controlPos1Z),
                new Vector3(controlPos2X, controlPos2Y, controlPos2Z),
                duration);
        }


        /// <summary>
        /// Creates a TweenAction that moves the transform localPosition to [target] by bezier3 with [controlPos1] and [controlPos2].
        /// Note: don't change the TweenEase of this TweenAction.
        /// </summary>
        public static TweenAction ActionBezier3LocalMove(this Transform transform, Transform target, Transform controlPos1, Transform controlPos2, float duration)
        {
            return ActionBezier3LocalMove(transform,
                target.localPosition,
                controlPos1.localPosition,
                controlPos2.localPosition,
                duration);
        }

        #endregion


        #region Action Move Anchored X Y Z

        /// <summary>
        /// Creates a TweenAction that moves the rectTransform anchoredPosition x to [x].
        /// </summary>
        public static TweenAction ActionMoveAnchoredX(this RectTransform rectTransform, float x, float duration)
        {
            return TweenAction.CreateFloat(() => rectTransform.anchoredPosition.x, (value) => rectTransform.SetAnchoredPositionX(value), x, duration);
        }


        /// <summary>
        /// Creates a TweenAction that moves the rectTransform anchoredPosition y to [y].
        /// </summary>
        public static TweenAction ActionMoveAnchoredY(this RectTransform rectTransform, float y, float duration)
        {
            return TweenAction.CreateFloat(() => rectTransform.anchoredPosition.y, (value) => rectTransform.SetAnchoredPositionY(value), y, duration);
        }


        /// <summary>
        /// Creates a TweenAction that moves the rectTransform anchoredPosition3D z to [z].
        /// </summary>
        public static TweenAction ActionMoveAnchoredZ(this RectTransform rectTransform, float z, float duration)
        {
            return TweenAction.CreateFloat(() => rectTransform.anchoredPosition3D.z, (value) => rectTransform.SetAnchoredPositionZ(value), z, duration);
        }

        #endregion


        #region Action Move Anchored XY

        /// <summary>
        /// Creates a TweenAction that moves the rectTransform anchoredPosition xy to [v2].
        /// </summary>
        public static TweenAction ActionMoveAnchoredXY(this RectTransform rectTransform, in Vector2 v2, float duration)
        {
            return TweenAction.CreateVector2((out Vector2 vector2) => vector2 = rectTransform.anchoredPosition,
                (in Vector2 vector2) => rectTransform.anchoredPosition = vector2,
                v2,
                duration);
        }


        /// <summary>
        /// Creates a TweenAction that moves the rectTransform anchoredPosition xy to [xy].
        /// </summary>
        public static TweenAction ActionMoveAnchoredXY(this RectTransform rectTransform, float x, float y, float duration)
        {
            return ActionMoveAnchoredXY(rectTransform, new Vector2(x, y), duration);
        }


        /// <summary>
        /// Creates a TweenAction that moves the rectTransform anchoredPosition xy to [target].
        /// </summary>
        public static TweenAction ActionMoveAnchoredXY(this RectTransform rectTransform, RectTransform target, float duration)
        {
            return ActionMoveAnchoredXY(rectTransform, target.anchoredPosition, duration);
        }

        #endregion


        #region Action Move Anchored

        /// <summary>
        /// Creates a TweenAction that moves the rectTransform anchoredPosition3D to [v3].
        /// </summary>
        public static TweenAction ActionMoveAnchored(this RectTransform rectTransform, in Vector3 v3, float duration)
        {
            return TweenAction.CreateVector3((out Vector3 vector3) => vector3 = rectTransform.anchoredPosition3D,
                (in Vector3 vector3) => rectTransform.anchoredPosition3D = vector3,
                v3,
                duration);
        }


        /// <summary>
        /// Creates a TweenAction that moves the rectTransform anchoredPosition3D to [xyz].
        /// </summary>
        public static TweenAction ActionMoveAnchored(this RectTransform rectTransform, float x, float y, float z, float duration)
        {
            return ActionMoveAnchored(rectTransform, new Vector3(x, y, z), duration);
        }


        /// <summary>
        /// Creates a TweenAction that moves the rectTransform anchoredPosition3D to [target].
        /// </summary>
        public static TweenAction ActionMoveAnchored(this RectTransform rectTransform, RectTransform target, float duration)
        {
            return ActionMoveAnchored(rectTransform, target.anchoredPosition3D, duration);
        }

        #endregion


        #region Action OffsetMax

        /// <summary>
        /// Creates a TweenAction that changes the rectTransform offsetMax x to [x].
        /// </summary>
        public static TweenAction ActionOffsetMaxX(this RectTransform rectTransform, float x, float duration)
        {
            return TweenAction.CreateFloat(() => rectTransform.offsetMax.x, (value) => rectTransform.SetOffsetMaxX(value), x, duration);
        }


        /// <summary>
        /// Creates a TweenAction that changes the rectTransform offsetMax y to [y].
        /// </summary>
        public static TweenAction ActionOffsetMaxY(this RectTransform rectTransform, float y, float duration)
        {
            return TweenAction.CreateFloat(() => rectTransform.offsetMax.y, (value) => rectTransform.SetOffsetMaxY(value), y, duration);
        }


        /// <summary>
        /// Creates a TweenAction that changes the rectTransform OffsetMax to [v2].
        /// </summary>
        public static TweenAction ActionOffsetMax(this RectTransform rectTransform, in Vector2 v2, float duration)
        {
            return TweenAction.CreateVector2((out Vector2 vector2) => vector2 = rectTransform.offsetMax,
                (in Vector2 vector2) => rectTransform.offsetMax = vector2,
                v2,
                duration);
        }


        /// <summary>
        /// Creates a TweenAction that changes the rectTransform offsetMax to [xy].
        /// </summary>
        public static TweenAction ActionOffsetMax(this RectTransform rectTransform, float x, float y, float duration)
        {
            return ActionOffsetMax(rectTransform, new Vector2(x, y), duration);
        }


        /// <summary>
        /// Creates a TweenAction that changes the rectTransform offsetMax to [target].
        /// </summary>
        public static TweenAction ActionOffsetMax(this RectTransform rectTransform, RectTransform target, float duration)
        {
            return ActionOffsetMax(rectTransform, target.offsetMax, duration);
        }

        #endregion


        #region Action OffsetMin

        /// <summary>
        /// Creates a TweenAction that changes the rectTransform offsetMin x to [x].
        /// </summary>
        public static TweenAction ActionOffsetMinX(this RectTransform rectTransform, float x, float duration)
        {
            return TweenAction.CreateFloat(() => rectTransform.offsetMin.x, (value) => rectTransform.SetOffsetMinX(value), x, duration);
        }


        /// <summary>
        /// Creates a TweenAction that changes the rectTransform offsetMin y to [y].
        /// </summary>
        public static TweenAction ActionOffsetMinY(this RectTransform rectTransform, float y, float duration)
        {
            return TweenAction.CreateFloat(() => rectTransform.offsetMin.y, (value) => rectTransform.SetOffsetMinY(value), y, duration);
        }


        /// <summary>
        /// Creates a TweenAction that changes the rectTransform offsetMin to [v2].
        /// </summary>
        public static TweenAction ActionOffsetMin(this RectTransform rectTransform, in Vector2 v2, float duration)
        {
            return TweenAction.CreateVector2((out Vector2 vector2) => vector2 = rectTransform.offsetMin,
                (in Vector2 vector2) => rectTransform.offsetMin = vector2,
                v2,
                duration);
        }


        /// <summary>
        /// Creates a TweenAction that changes the rectTransform offsetMin to [xy].
        /// </summary>
        public static TweenAction ActionOffsetMin(this RectTransform rectTransform, float x, float y, float duration)
        {
            return ActionOffsetMin(rectTransform, new Vector2(x, y), duration);
        }


        /// <summary>
        /// Creates a TweenAction that changes the rectTransform offsetMin to [target].
        /// </summary>
        public static TweenAction ActionOffsetMin(this RectTransform rectTransform, RectTransform target, float duration)
        {
            return ActionOffsetMin(rectTransform, target.offsetMin, duration);
        }

        #endregion


        #region Action SizeDelta

        /// <summary>
        /// Creates a TweenAction that changes the rectTransform sizeDelta x to [x].
        /// </summary>
        public static TweenAction ActionSizeDeltaX(this RectTransform rectTransform, float x, float duration)
        {
            return TweenAction.CreateFloat(() => rectTransform.sizeDelta.x, (value) => rectTransform.SetSizeDeltaX(value), x, duration);
        }


        /// <summary>
        /// Creates a TweenAction that changes the rectTransform sizeDelta y to [y].
        /// </summary>
        public static TweenAction ActionSizeDeltaY(this RectTransform rectTransform, float y, float duration)
        {
            return TweenAction.CreateFloat(() => rectTransform.sizeDelta.y, (value) => rectTransform.SetSizeDeltaY(value), y, duration);
        }


        /// <summary>
        /// Creates a TweenAction that changes the rectTransform sizeDelta to [v2].
        /// </summary>
        public static TweenAction ActionSizeDelta(this RectTransform rectTransform, in Vector2 v2, float duration)
        {
            return TweenAction.CreateVector2((out Vector2 vector2) => vector2 = rectTransform.sizeDelta,
                (in Vector2 vector2) => rectTransform.sizeDelta = vector2,
                v2,
                duration);
        }


        /// <summary>
        /// Creates a TweenAction that changes the rectTransform sizeDelta to [xy].
        /// </summary>
        public static TweenAction ActionSizeDelta(this RectTransform rectTransform, float x, float y, float duration)
        {
            return ActionSizeDelta(rectTransform, new Vector2(x, y), duration);
        }


        /// <summary>
        /// Creates a TweenAction that changes the rectTransform sizeDelta to [target].
        /// </summary>
        public static TweenAction ActionSizeDelta(this RectTransform rectTransform, RectTransform target, float duration)
        {
            return ActionSizeDelta(rectTransform, target.sizeDelta, duration);
        }

        #endregion


        #region Action Graphic

        /// <summary>
        /// Creates a TweenAction that fades the Graphic color alpha to [alpha].
        /// </summary>a 
        public static TweenAction ActionFadeTo(this Graphic graphic, float alpha, float duration)
        {
            return TweenAction.CreateFloat(() => graphic.color.a, (value) => graphic.SetAlpha(value), alpha, duration);
        }


        /// <summary>
        /// Creates a TweenAction that fades the Graphic color alpha to [1.0f].
        /// </summary>
        public static TweenAction ActionFadeIn(this Graphic graphic, float duration) { return ActionFadeTo(graphic, 1.0f, duration); }


        /// <summary>
        /// Creates a TweenAction that fades the Graphic color alpha to [0.0f].
        /// </summary>
        public static TweenAction ActionFadeOut(this Graphic graphic, float duration) { return ActionFadeTo(graphic, 0.0f, duration); }


        /// <summary>
        /// Creates a TweenAction that changes the Graphic color to [color].
        /// </summary>
        public static TweenAction ActionColorTo(this Graphic graphic, in Color color, float duration)
        {
            return TweenAction.CreateVector4((out Vector4 vector4) => vector4 = graphic.color, (in Vector4 vector4) => graphic.color = vector4, color, duration);
        }


        /// <summary>
        /// Creates a TweenAction that changes the Graphic rgb to [color].
        /// </summary>
        public static TweenAction ActionRGBTo(this Graphic graphic, in Color color, float duration)
        {
            return TweenAction.CreateVector3((out Vector3 vector3) => vector3 = (Vector4) graphic.color,
                (in Vector3 vector3) => graphic.color = new Color(vector3.x, vector3.y, vector3.z),
                (Vector4) color,
                duration);
        }

        #endregion

        #region Action Image

        /// <summary>
        /// Creates a TweenAction that change the Image fillAmount value to [value].
        /// </summary>a 
        public static TweenAction ActionFillAmountTo(this Image img, float value, float duration)
        {
            return TweenAction.CreateFloat(() => img.fillAmount, (value) => img.fillAmount = value, value, duration);
        }

        #endregion

        #region Action CanvasGroup

        /// <summary>
        /// Creates a TweenAction that fades the CanvasGroup alpha to [alpha].
        /// </summary>a 
        public static TweenAction ActionFadeTo(this CanvasGroup canvasGroup, float alpha, float duration)
        {
            return TweenAction.CreateFloat(() => canvasGroup.alpha, (value) => canvasGroup.alpha = value, alpha, duration);
        }


        /// <summary>
        /// Creates a TweenAction that fades the CanvasGroup alpha to [1.0f].
        /// </summary>
        public static TweenAction ActionFadeIn(this CanvasGroup canvasGroup, float duration) { return ActionFadeTo(canvasGroup, 1.0f, duration); }


        /// <summary>
        /// Creates a TweenAction that fades the CanvasGroup alpha to [0.0f].
        /// </summary>
        public static TweenAction ActionFadeOut(this CanvasGroup canvasGroup, float duration) { return ActionFadeTo(canvasGroup, 0.0f, duration); }

        #endregion


        #region Action SpriteRenderer

        /// <summary>
        /// Creates a TweenAction that fades the SpriteRenderer color alpha to [alpha].
        /// </summary>
        public static TweenAction ActionFadeTo(this SpriteRenderer spriteRenderer, float alpha, float duration)
        {
            return TweenAction.CreateFloat(() => spriteRenderer.color.a, (value) => spriteRenderer.SetAlpha(value), alpha, duration);
        }


        /// <summary>
        /// Creates a TweenAction that fades the SpriteRenderer color alpha to [1.0f].
        /// </summary>
        public static TweenAction ActionFadeIn(this SpriteRenderer spriteRenderer, float duration) { return ActionFadeTo(spriteRenderer, 1.0f, duration); }


        /// <summary>
        /// Creates a TweenAction that fades the SpriteRenderer color alpha to [0.0f].
        /// </summary>
        public static TweenAction ActionFadeOut(this SpriteRenderer spriteRenderer, float duration) { return ActionFadeTo(spriteRenderer, 0.0f, duration); }


        /// <summary>
        /// Creates a TweenAction that changes the SpriteRenderer color to [color].
        /// </summary>
        public static TweenAction ActionColorTo(this SpriteRenderer spriteRenderer, in Color color, float duration)
        {
            return TweenAction.CreateVector4((out Vector4 vector4) => vector4 = spriteRenderer.color,
                (in Vector4 vector4) => spriteRenderer.color = vector4,
                color,
                duration);
        }


        /// <summary>
        /// Creates a TweenAction that changes the SpriteRenderer color rgb to [color].
        /// </summary>
        public static TweenAction ActionRGBTo(this SpriteRenderer spriteRenderer, in Color color, float duration)
        {
            return TweenAction.CreateVector3((out Vector3 vector3) => vector3 = (Vector4) spriteRenderer.color,
                (in Vector3 vector3) => spriteRenderer.color = new Color(vector3.x, vector3.y, vector3.z),
                (Vector4) color,
                duration);
        }

        #endregion


        #region Action AudioSource

        /// <summary>
        /// Creates a TweenAction that changes the AudioSource pitch to [value].
        /// </summary>
        public static TweenAction ActionPitchTo(this AudioSource source, float pitch, float duration)
        {
            return TweenAction.CreateFloat(() => source.pitch, (value) => source.pitch = value, pitch, duration);
        }

        /// <summary>
        /// Creates a TweenAction that changes the AudioSource volume to [volume].
        /// </summary>
        public static TweenAction ActionVolumeTo(this AudioSource source, float volume, float duration)
        {
            return TweenAction.CreateFloat(() => source.volume, (value) => source.volume = value, volume, duration);
        }


        /// <summary>
        /// Creates a TweenAction that changes the AudioSource volume to [1.0f].
        /// </summary>
        public static TweenAction ActionVolumeIn(this AudioSource source, float duration)
        {
            return TweenAction.CreateFloat(() => source.volume, (value) => source.volume = value, 1.0f, duration);
        }


        /// <summary>
        /// Creates a TweenAction that changes the AudioSource volume to [0.0f].
        /// </summary>
        public static TweenAction ActionVolumeOut(this AudioSource source, float duration)
        {
            return TweenAction.CreateFloat(() => source.volume, (value) => source.volume = value, 0.0f, duration);
        }

        #endregion


        #region Action Material

        /// <summary>
        /// Creates a TweenAction that changes the Material float to [value] by [name].
        /// </summary>
        public static TweenAction ActionFloatTo(this Material material, string name, float value, float duration)
        {
            return TweenAction.CreateFloat(() => material.GetFloat(name), (value) => material.SetFloat(name, value), value, duration);
        }


        /// <summary>
        /// Creates a TweenAction that changes the Material int to [value] by [name] .
        /// </summary>
        public static TweenAction ActionIntTo(this Material material, string name, int value, float duration)
        {
            return TweenAction.CreateFloat(() => material.GetInt(name), (value) => material.SetInt(name, (int) Mathf.Round(value)), value, duration);
        }


        /// <summary>
        /// Creates a TweenAction that changes the Material vector to [v4] by [name].
        /// </summary>
        public static TweenAction ActionVectorTo(this Material material, string name, in Vector4 v4, float duration)
        {
            return TweenAction.CreateVector4((out Vector4 vector4) => vector4 = material.GetVector(name),
                (in Vector4 vector4) => material.SetVector(name, vector4),
                v4,
                duration);
        }


        /// <summary>
        /// Creates a TweenAction that changes the Material color to [color] by [name].
        /// </summary>
        public static TweenAction ActionColorTo(this Material material, string name, in Color color, float duration)
        {
            return TweenAction.CreateVector4((out Vector4 vector4) => vector4 = material.GetColor(name),
                (in Vector4 vector4) => material.SetColor(name, vector4),
                color,
                duration);
        }

        #endregion


        #region Action Camera

        /// <summary>
        /// Creates a TweenAction that changes the Camera aspect to [value].
        /// </summary>
        public static TweenAction ActionAspectTo(this Camera source, float value, float duration)
        {
            return TweenAction.CreateFloat(() => source.aspect, (value) => source.aspect = value, value, duration);
        }

        /// <summary>
        /// Creates a TweenAction that changes the Camera fov to [value].
        /// </summary>
        public static TweenAction ActionFieldOfViewTo(this Camera source, float value, float duration)
        {
            return TweenAction.CreateFloat(() => source.fieldOfView, (value) => source.aspect = value, value, duration);
        }

        /// <summary>
        /// Creates a TweenAction that changes the Camera orthographicSize to [value].
        /// </summary>
        public static TweenAction ActionOrthographicSizeTo(this Camera source, float value, float duration)
        {
            return TweenAction.CreateFloat(() => source.orthographicSize, (value) => source.aspect = value, value, duration);
        }

        /// <summary>
        /// Creates a TweenAction that changes the Camera farClipPlane to [value].
        /// </summary>
        public static TweenAction ActionFarClipPlaneTo(this Camera source, float value, float duration)
        {
            return TweenAction.CreateFloat(() => source.farClipPlane, (value) => source.aspect = value, value, duration);
        }

        /// <summary>
        /// Creates a TweenAction that changes the Camera nearClipPlane to [value].
        /// </summary>
        public static TweenAction ActionNearClipPlaneTo(this Camera source, float value, float duration)
        {
            return TweenAction.CreateFloat(() => source.nearClipPlane, (value) => source.aspect = value, value, duration);
        }

        #endregion


        #region Action Light

        /// <summary>
        /// Creates a TweenAction that changes the Light color to [value].
        /// </summary>
        public static TweenAction ActionColorTo(this Light light, in Color color, float duration)
        {
            return TweenAction.CreateVector4((out Vector4 vector4) => vector4 = light.color, (in Vector4 vector4) => light.color = vector4, color, duration);
        }

        /// <summary>
        /// Creates a TweenAction that changes the Light intensity to [value].
        /// </summary>
        public static TweenAction ActionIntensityTo(this Light light, float value, float duration)
        {
            return TweenAction.CreateFloat(() => light.intensity, (value) => light.intensity = value, value, duration);
        }

        /// <summary>
        /// Creates a TweenAction that changes the Light shadowStrength to [value].
        /// </summary>
        public static TweenAction ActionShadowStrengthTo(this Light light, float value, float duration)
        {
            return TweenAction.CreateFloat(() => light.shadowStrength, (value) => light.shadowStrength = value, value, duration);
        }

        #endregion


        #region Action Linerenderer

        /// <summary>
        /// Creates a TweenAction that changes the LineRenderer startColor to [value].
        /// </summary>
        public static TweenAction ActionStartColorTo(this LineRenderer line, in Color color, float duration)
        {
            return TweenAction.CreateVector4((out Vector4 vector4) => vector4 = line.startColor, (in Vector4 vector4) => line.startColor = vector4, color, duration);
        }

        /// <summary>
        /// Creates a TweenAction that changes the LineRenderer endColor to [value].
        /// </summary>
        public static TweenAction ActionEndColorTo(this LineRenderer line, in Color color, float duration)
        {
            return TweenAction.CreateVector4((out Vector4 vector4) => vector4 = line.endColor, (in Vector4 vector4) => line.endColor = vector4, color, duration);
        }

        /// <summary>
        /// Creates a TweenAction that changes the LineRenderer startWidth to [value].
        /// </summary>
        public static TweenAction ActionStartWidthTo(this LineRenderer line, float value, float duration)
        {
            return TweenAction.CreateFloat(() => line.startWidth, (value) => line.startWidth = value, value, duration);
        }

        /// <summary>
        /// Creates a TweenAction that changes the LineRenderer endWidth to [value].
        /// </summary>
        public static TweenAction ActionEndWidthTo(this LineRenderer line, float value, float duration)
        {
            return TweenAction.CreateFloat(() => line.endWidth, (value) => line.endWidth = value, value, duration);
        }

        #endregion


        #region Action Rigidbody

        /// <summary>
        /// Creates a TweenAction that moves the rigidbody position to [v3].
        /// </summary>
        public static TweenAction ActionMove(this Rigidbody rigidbody, in Vector3 v3, float duration)
        {
            return TweenAction.CreateVector3((out Vector3 vector3) => vector3 = rigidbody.position,
                (in Vector3 vector3) => rigidbody.MovePosition(vector3),
                v3,
                duration);
        }

        /// <summary>
        /// Creates a TweenAction that rotates the rigidbody eulerAngles to [v3].
        /// </summary>
        public static TweenAction ActionRotate(this Rigidbody rigidbody, in Vector3 v3, float duration)
        {
            return TweenAction.CreateVector3((out Vector3 vector3) => vector3 = rigidbody.rotation.eulerAngles,
                (in Vector3 vector3) => rigidbody.MoveRotation(Quaternion.Euler(vector3)),
                v3,
                duration);
        }

        #endregion


        #region Action Rigidbody2D

        /// <summary>
        /// Creates a TweenAction that moves the rigidbody position to [v2].
        /// </summary>
        public static TweenAction ActionMove(this Rigidbody2D rigidbody, in Vector2 v2, float duration)
        {
            return TweenAction.CreateVector2((out Vector2 vector2) => vector2 = rigidbody.position,
                (in Vector2 vector2) => rigidbody.MovePosition(vector2),
                v2,
                duration);
        }

        #endregion


        #region Action Scroll

        /// <summary>
        /// Creates a TweenAction that moves the ScrollRect normalizedPosition to [v2].
        /// </summary>
        public static TweenAction ActionNormalizedPositionTo(this ScrollRect scroll, in Vector2 v2, float duration)
        {
            return TweenAction.CreateVector2((out Vector2 vector2) => vector2 = scroll.normalizedPosition,
                (in Vector2 vector2) => scroll.normalizedPosition = vector2,
                v2,
                duration);
        }


        /// <summary>
        /// Creates a TweenAction that moves the ScrollRect horizontalNormalizedPosition to [value].
        /// </summary>
        public static TweenAction ActionHorizontalNormalizedPositionTo(this ScrollRect scroll, float value, float duration)
        {
            return TweenAction.CreateFloat(() => scroll.horizontalNormalizedPosition, (value) => scroll.horizontalNormalizedPosition = value, value, duration);
        }


        /// <summary>
        /// Creates a TweenAction that moves the ScrollRect verticalNormalizedPosition to [value].
        /// </summary>
        public static TweenAction ActionVerticalNormalizedPositionTo(this ScrollRect scroll, float value, float duration)
        {
            return TweenAction.CreateFloat(() => scroll.verticalNormalizedPosition, (value) => scroll.verticalNormalizedPosition = value, value, duration);
        }

        #endregion
    }
}