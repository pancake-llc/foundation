﻿using System;
using System.Collections.Generic;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Pancake.Common
{
    public static partial class C
    {
        #region Position X Y Z

        public static void SetPositionX(this Transform transform, float x)
        {
            var v3 = transform.position;
            v3.x = x;
            transform.position = v3;
        }


        public static void SetPositionY(this Transform transform, float y)
        {
            var v3 = transform.position;
            v3.y = y;
            transform.position = v3;
        }


        public static void SetPositionZ(this Transform transform, float z)
        {
            var v3 = transform.position;
            v3.z = z;
            transform.position = v3;
        }

        #endregion


        #region Position XY

        public static void GetPositionXY(this Transform transform, out Vector2 v2)
        {
            var v3 = transform.position;
            v2.x = v3.x;
            v2.y = v3.y;
        }


        public static Vector2 GetPositionXY(this Transform transform)
        {
            var v3 = transform.position;
            return new Vector2(v3.x, v3.y);
        }

        public static void SetPositionXY(this Transform transform, in Vector2 v2) { transform.position = new Vector3(v2.x, v2.y, transform.position.z); }


        public static void SetPositionXY(this Transform transform, float x, float y) { transform.position = new Vector3(x, y, transform.position.z); }


        public static void SetPositionXY(this Transform transform, Transform target)
        {
            var v3 = target.position;
            v3.z = transform.position.z;
            transform.position = v3;
        }

        #endregion


        #region Position XZ

        public static void GetPositionXZ(this Transform transform, out Vector2 v2)
        {
            var v3 = transform.position;
            v2.x = v3.x;
            v2.y = v3.z;
        }


        public static Vector2 GetPositionXZ(this Transform transform)
        {
            var v3 = transform.position;
            return new Vector2(v3.x, v3.z);
        }


        public static void SetPositionXZ(this Transform transform, in Vector2 v2) { transform.position = new Vector3(v2.x, transform.position.y, v2.y); }


        public static void SetPositionXZ(this Transform transform, float x, float z) { transform.position = new Vector3(x, transform.position.y, z); }


        public static void SetPositionXZ(this Transform transform, Transform target)
        {
            var v3 = target.position;
            v3.y = transform.position.y;
            transform.position = v3;
        }

        #endregion


        #region Position YZ

        public static void GetPositionYZ(this Transform transform, out Vector2 v2)
        {
            var v3 = transform.position;
            v2.x = v3.y;
            v2.y = v3.z;
        }


        public static Vector2 GetPositionYZ(this Transform transform)
        {
            var v3 = transform.position;
            return new Vector2(v3.y, v3.z);
        }


        public static void SetPositionYZ(this Transform transform, in Vector2 v2) { transform.position = new Vector3(transform.position.x, v2.x, v2.y); }


        public static void SetPositionYZ(this Transform transform, float y, float z) { transform.position = new Vector3(transform.position.x, y, z); }


        public static void SetPositionYZ(this Transform transform, Transform target)
        {
            var v3 = target.position;
            v3.x = transform.position.x;
            transform.position = v3;
        }

        #endregion


        #region Relative Position X Y Z

        public static void SetRelativePositionX(this Transform transform, float x)
        {
            var v3 = transform.position;
            v3.x += x;
            transform.position = v3;
        }


        public static void SetRelativePositionY(this Transform transform, float y)
        {
            var v3 = transform.position;
            v3.y += y;
            transform.position = v3;
        }


        public static void SetRelativePositionZ(this Transform transform, float z)
        {
            var v3 = transform.position;
            v3.z += z;
            transform.position = v3;
        }

        #endregion


        #region Relative Position XY

        public static void SetRelativePositionXY(this Transform transform, in Vector2 v2)
        {
            var v3 = transform.position;
            v3.x += v2.x;
            v3.y += v2.y;
            transform.position = v3;
        }


        public static void SetRelativePositionXY(this Transform transform, float x, float y)
        {
            var v3 = transform.position;
            v3.x += x;
            v3.y += y;
            transform.position = v3;
        }


        public static void SetRelativePositionXY(this Transform transform, Transform target)
        {
            var v3 = transform.position;
            var targetV3 = target.position;
            v3.x += targetV3.x;
            v3.y += targetV3.y;
            transform.position = v3;
        }

        #endregion


        #region Relative Position XZ

        public static void SetRelativePositionXZ(this Transform transform, in Vector2 v2)
        {
            var v3 = transform.position;
            v3.x += v2.x;
            v3.z += v2.y;
            transform.position = v3;
        }


        public static void SetRelativePositionXZ(this Transform transform, float x, float z)
        {
            var v3 = transform.position;
            v3.x += x;
            v3.z += z;
            transform.position = v3;
        }


        public static void SetRelativePositionXZ(this Transform transform, Transform target)
        {
            var v3 = transform.position;
            var targetV3 = target.position;
            v3.x += targetV3.x;
            v3.z += targetV3.z;
            transform.position = v3;
        }

        #endregion


        #region Relative Position YZ

        public static void SetRelativePositionYZ(this Transform transform, in Vector2 v2)
        {
            var v3 = transform.position;
            v3.y += v2.x;
            v3.z += v2.y;
            transform.position = v3;
        }


        public static void SetRelativePositionYZ(this Transform transform, float y, float z)
        {
            var v3 = transform.position;
            v3.y += y;
            v3.z += z;
            transform.position = v3;
        }


        public static void SetRelativePositionYZ(this Transform transform, Transform target)
        {
            var v3 = transform.position;
            var targetV3 = target.position;
            v3.y += targetV3.y;
            v3.z += targetV3.z;
            transform.position = v3;
        }

        #endregion


        #region Local Position X Y Z

        public static void SetLocalPositionX(this Transform transform, float x)
        {
            var v3 = transform.localPosition;
            v3.x = x;
            transform.localPosition = v3;
        }


        public static void SetLocalPositionY(this Transform transform, float y)
        {
            var v3 = transform.localPosition;
            v3.y = y;
            transform.localPosition = v3;
        }


        public static void SetLocalPositionZ(this Transform transform, float z)
        {
            var v3 = transform.localPosition;
            v3.z = z;
            transform.localPosition = v3;
        }

        #endregion


        #region Local Position XY

        public static void SetLocalPositionXY(this Transform transform, in Vector2 v2) { transform.localPosition = new Vector3(v2.x, v2.y, transform.localPosition.z); }


        public static void SetLocalPositionXY(this Transform transform, float x, float y) { transform.localPosition = new Vector3(x, y, transform.localPosition.z); }


        public static void SetLocalPositionXY(this Transform transform, Transform target)
        {
            var v3 = target.localPosition;
            v3.z = transform.localPosition.z;
            transform.localPosition = v3;
        }

        #endregion


        #region Local Position XZ

        public static void GetLocalPositionXZ(this Transform transform, out Vector2 v2)
        {
            var v3 = transform.localPosition;
            v2.x = v3.x;
            v2.y = v3.z;
        }


        public static Vector2 GetLocalPositionXZ(this Transform transform)
        {
            var v3 = transform.localPosition;
            return new Vector2(v3.x, v3.z);
        }


        public static void SetLocalPositionXZ(this Transform transform, in Vector2 v2) { transform.localPosition = new Vector3(v2.x, transform.localPosition.y, v2.y); }


        public static void SetLocalPositionXZ(this Transform transform, float x, float z) { transform.localPosition = new Vector3(x, transform.localPosition.y, z); }


        public static void SetLocalPositionXZ(this Transform transform, Transform target)
        {
            var v3 = target.localPosition;
            v3.y = transform.localPosition.y;
            transform.localPosition = v3;
        }

        #endregion


        #region Local Position YZ

        public static void GetLocalPositionYZ(this Transform transform, out Vector2 v2)
        {
            var v3 = transform.localPosition;
            v2.x = v3.y;
            v2.y = v3.z;
        }


        public static Vector2 GetLocalPositionYZ(this Transform transform)
        {
            var v3 = transform.localPosition;
            return new Vector2(v3.y, v3.z);
        }


        public static void SetLocalPositionYZ(this Transform transform, in Vector2 v2) { transform.localPosition = new Vector3(transform.localPosition.x, v2.x, v2.y); }


        public static void SetLocalPositionYZ(this Transform transform, float y, float z) { transform.localPosition = new Vector3(transform.localPosition.x, y, z); }


        public static void SetLocalPositionYZ(this Transform transform, Transform target)
        {
            var v3 = target.localPosition;
            v3.x = transform.localPosition.x;
            transform.localPosition = v3;
        }

        #endregion


        #region Relative Local Position X Y Z

        public static void SetRelativeLocalPositionX(this Transform transform, float x)
        {
            var v3 = transform.localPosition;
            v3.x += x;
            transform.localPosition = v3;
        }


        public static void SetRelativeLocalPositionY(this Transform transform, float y)
        {
            var v3 = transform.localPosition;
            v3.y += y;
            transform.localPosition = v3;
        }


        public static void SetRelativeLocalPositionZ(this Transform transform, float z)
        {
            var v3 = transform.localPosition;
            v3.z += z;
            transform.localPosition = v3;
        }

        #endregion


        #region Relative Local Position XY

        public static void SetRelativeLocalPositionXY(this Transform transform, in Vector2 v2)
        {
            var v3 = transform.localPosition;
            v3.x += v2.x;
            v3.y += v2.y;
            transform.localPosition = v3;
        }


        public static void SetRelativeLocalPositionXY(this Transform transform, float x, float y)
        {
            var v3 = transform.localPosition;
            v3.x += x;
            v3.y += y;
            transform.localPosition = v3;
        }


        public static void SetRelativeLocalPositionXY(this Transform transform, Transform target)
        {
            var v3 = transform.localPosition;
            var targetV3 = target.localPosition;
            v3.x += targetV3.x;
            v3.y += targetV3.y;
            transform.localPosition = v3;
        }

        #endregion


        #region Relative Local Position XZ

        public static void SetRelativeLocalPositionXZ(this Transform transform, in Vector2 v2)
        {
            var v3 = transform.localPosition;
            v3.x += v2.x;
            v3.z += v2.y;
            transform.localPosition = v3;
        }


        public static void SetRelativeLocalPositionXZ(this Transform transform, float x, float z)
        {
            var v3 = transform.localPosition;
            v3.x += x;
            v3.z += z;
            transform.localPosition = v3;
        }


        public static void SetRelativeLocalPositionXZ(this Transform transform, Transform target)
        {
            var v3 = transform.localPosition;
            var targetV3 = target.localPosition;
            v3.x += targetV3.x;
            v3.z += targetV3.z;
            transform.localPosition = v3;
        }

        #endregion


        #region Relative Local Postion YZ

        public static void SetRelativeLocalPositionYZ(this Transform transform, in Vector2 v2)
        {
            var v3 = transform.localPosition;
            v3.y += v2.x;
            v3.z += v2.y;
            transform.localPosition = v3;
        }


        public static void SetRelativeLocalPositionYZ(this Transform transform, float y, float z)
        {
            var v3 = transform.localPosition;
            v3.y += y;
            v3.z += z;
            transform.localPosition = v3;
        }


        public static void SetRelativeLocalPositionYZ(this Transform transform, Transform target)
        {
            var v3 = transform.localPosition;
            var targetV3 = target.localPosition;
            v3.y += targetV3.y;
            v3.z += targetV3.z;
            transform.localPosition = v3;
        }

        #endregion


        #region Scale X Y Z

        public static void SetScaleX(this Transform transform, float x)
        {
            var v3 = transform.localScale;
            v3.x = x;
            transform.localScale = v3;
        }


        public static void SetScaleY(this Transform transform, float y)
        {
            var v3 = transform.localScale;
            v3.y = y;
            transform.localScale = v3;
        }


        public static void SetScaleZ(this Transform transform, float z)
        {
            var v3 = transform.localScale;
            v3.z = z;
            transform.localScale = v3;
        }

        #endregion


        #region Scale XY

        public static void SetScaleXY(this Transform transform, in Vector2 v2) { transform.localScale = new Vector3(v2.x, v2.y, transform.localScale.z); }


        public static void SetScaleXY(this Transform transform, float x, float y) { transform.localScale = new Vector3(x, y, transform.localScale.z); }


        public static void SetScaleXY(this Transform transform, float value) { transform.localScale = new Vector3(value, value, transform.localScale.z); }


        public static void SetScaleXY(this Transform transform, Transform target)
        {
            var v3 = target.localScale;
            v3.z = transform.localScale.z;
            transform.localScale = v3;
        }

        #endregion


        #region Scale XZ

        public static void GetScaleXZ(this Transform transform, out Vector2 v2)
        {
            var v3 = transform.localScale;
            v2.x = v3.x;
            v2.y = v3.z;
        }


        public static Vector2 GetScaleXZ(this Transform transform)
        {
            var v3 = transform.localScale;
            return new Vector2(v3.x, v3.z);
        }


        public static void SetScaleXZ(this Transform transform, in Vector2 v2) { transform.localScale = new Vector3(v2.x, transform.localScale.y, v2.y); }


        public static void SetScaleXZ(this Transform transform, float x, float z) { transform.localScale = new Vector3(x, transform.localScale.y, z); }


        public static void SetScaleXZ(this Transform transform, float value) { transform.localScale = new Vector3(value, transform.localScale.y, value); }


        public static void SetScaleXZ(this Transform transform, Transform target)
        {
            var v3 = target.localScale;
            v3.y = transform.localScale.y;
            transform.localScale = v3;
        }

        #endregion


        #region Scale YZ

        public static void GetScaleYZ(this Transform transform, out Vector2 v2)
        {
            var v3 = transform.localScale;
            v2.x = v3.y;
            v2.y = v3.z;
        }


        public static Vector2 GetScaleYZ(this Transform transform)
        {
            var v3 = transform.localScale;
            return new Vector2(v3.y, v3.z);
        }


        public static void SetScaleYZ(this Transform transform, in Vector2 v2) { transform.localScale = new Vector3(transform.localScale.x, v2.x, v2.y); }


        public static void SetScaleYZ(this Transform transform, float y, float z) { transform.localScale = new Vector3(transform.localScale.x, y, z); }


        public static void SetScaleYZ(this Transform transform, float value) { transform.localScale = new Vector3(transform.localScale.x, value, value); }


        public static void SetScaleYZ(this Transform transform, Transform target)
        {
            var v3 = target.localScale;
            v3.x = transform.localScale.x;
            transform.localScale = v3;
        }

        #endregion


        #region Scale

        public static void SetScale(this Transform transform, float value) { transform.localScale = new Vector3(value, value, value); }

        #endregion


        #region Relative Scale X Y Z

        public static void SetRelativeScaleX(this Transform transform, float x)
        {
            var v3 = transform.localScale;
            v3.x += x;
            transform.localScale = v3;
        }


        public static void SetRelativeScaleY(this Transform transform, float y)
        {
            var v3 = transform.localScale;
            v3.y += y;
            transform.localScale = v3;
        }


        public static void SetRelativeScaleZ(this Transform transform, float z)
        {
            var v3 = transform.localScale;
            v3.z += z;
            transform.localScale = v3;
        }

        #endregion


        #region Relative Scale XY

        public static void SetRelativeScaleXY(this Transform transform, in Vector2 v2)
        {
            var v3 = transform.localScale;
            v3.x += v2.x;
            v3.y += v2.y;
            transform.localScale = v3;
        }


        public static void SetRelativeScaleXY(this Transform transform, float x, float y)
        {
            var v3 = transform.localScale;
            v3.x += x;
            v3.y += y;
            transform.localScale = v3;
        }


        public static void SetRelativeScaleXY(this Transform transform, float value)
        {
            var v3 = transform.localScale;
            v3.x += value;
            v3.y += value;
            transform.localScale = v3;
        }


        public static void SetRelativeScaleXY(this Transform transform, Transform target)
        {
            var v3 = transform.localScale;
            var targetV3 = target.localScale;
            v3.x += targetV3.x;
            v3.y += targetV3.y;
            transform.localScale = v3;
        }

        #endregion


        #region Relative Scale XZ

        public static void SetRelativeScaleXZ(this Transform transform, in Vector2 v2)
        {
            var v3 = transform.localScale;
            v3.x += v2.x;
            v3.z += v2.y;
            transform.localScale = v3;
        }


        public static void SetRelativeScaleXZ(this Transform transform, float x, float z)
        {
            var v3 = transform.localScale;
            v3.x += x;
            v3.z += z;
            transform.localScale = v3;
        }


        public static void SetRelativeScaleXZ(this Transform transform, float value)
        {
            var v3 = transform.localScale;
            v3.x += value;
            v3.z += value;
            transform.localScale = v3;
        }


        public static void SetRelativeScaleXZ(this Transform transform, Transform target)
        {
            var v3 = transform.localScale;
            var targetV3 = target.localScale;
            v3.x += targetV3.x;
            v3.z += targetV3.z;
            transform.localScale = v3;
        }

        #endregion


        #region Relative Scale YZ

        public static void SetRelativeScaleYZ(this Transform transform, in Vector2 v2)
        {
            var v3 = transform.localScale;
            v3.y += v2.x;
            v3.z += v2.y;
            transform.localScale = v3;
        }


        public static void SetRelativeScaleYZ(this Transform transform, float y, float z)
        {
            var v3 = transform.localScale;
            v3.y += y;
            v3.z += z;
            transform.localScale = v3;
        }


        public static void SetRelativeScaleYZ(this Transform transform, float value)
        {
            var v3 = transform.localScale;
            v3.y += value;
            v3.z += value;
            transform.localScale = v3;
        }


        public static void SetRelativeScaleYZ(this Transform transform, Transform target)
        {
            var v3 = transform.localScale;
            var targetV3 = target.localScale;
            v3.y += targetV3.y;
            v3.z += targetV3.z;
            transform.localScale = v3;
        }

        #endregion


        #region Relative Scale

        public static void SetRelativeScale(this Transform transform, float value) { transform.localScale += new Vector3(value, value, value); }

        #endregion


        #region Rotation X Y Z

        public static void SetRotationX(this Transform transform, float x)
        {
            var v3 = transform.eulerAngles;
            v3.x = x;
            transform.eulerAngles = v3;
        }


        public static void SetRotationY(this Transform transform, float y)
        {
            var v3 = transform.eulerAngles;
            v3.y = y;
            transform.eulerAngles = v3;
        }


        public static void SetRotationZ(this Transform transform, float z)
        {
            var v3 = transform.eulerAngles;
            v3.z = z;
            transform.eulerAngles = v3;
        }

        #endregion


        #region Rotation XY

        public static void SetRotationXY(this Transform transform, in Vector2 v2) { transform.eulerAngles = new Vector3(v2.x, v2.y, transform.eulerAngles.z); }


        public static void SetRotationXY(this Transform transform, float x, float y) { transform.eulerAngles = new Vector3(x, y, transform.eulerAngles.z); }


        public static void SetRotationXY(this Transform transform, Transform target)
        {
            var v3 = target.eulerAngles;
            v3.z = transform.eulerAngles.z;
            transform.eulerAngles = v3;
        }

        #endregion


        #region Rotation XZ

        public static void GetRotationXZ(this Transform transform, out Vector2 v2)
        {
            var v3 = transform.eulerAngles;
            v2.x = v3.x;
            v2.y = v3.z;
        }


        public static Vector2 GetRotationXZ(this Transform transform)
        {
            var v3 = transform.eulerAngles;
            return new Vector2(v3.x, v3.z);
        }


        public static void SetRotationXZ(this Transform transform, in Vector2 v2) { transform.eulerAngles = new Vector3(v2.x, transform.eulerAngles.y, v2.y); }


        public static void SetRotationXZ(this Transform transform, float x, float z) { transform.eulerAngles = new Vector3(x, transform.eulerAngles.y, z); }


        public static void SetRotationXZ(this Transform transform, Transform target)
        {
            var v3 = target.eulerAngles;
            v3.y = transform.eulerAngles.y;
            transform.eulerAngles = v3;
        }

        #endregion


        #region Rotation YZ

        public static void GetRotationYZ(this Transform transform, out Vector2 v2)
        {
            var v3 = transform.eulerAngles;
            v2.x = v3.y;
            v2.y = v3.z;
        }


        public static Vector2 GetRotationYZ(this Transform transform)
        {
            var v3 = transform.eulerAngles;
            return new Vector2(v3.y, v3.z);
        }


        public static void SetRotationYZ(this Transform transform, in Vector2 v2) { transform.eulerAngles = new Vector3(transform.eulerAngles.x, v2.x, v2.y); }


        public static void SetRotationYZ(this Transform transform, float y, float z) { transform.eulerAngles = new Vector3(transform.eulerAngles.x, y, z); }


        public static void SetRotationYZ(this Transform transform, Transform target)
        {
            var v3 = target.eulerAngles;
            v3.x = transform.eulerAngles.x;
            transform.eulerAngles = v3;
        }

        #endregion


        #region Relative Rotation X Y Z

        public static void SetRelativeRotationX(this Transform transform, float x)
        {
            var v3 = transform.eulerAngles;
            v3.x += x;
            transform.eulerAngles = v3;
        }


        public static void SetRelativeRotationY(this Transform transform, float y)
        {
            var v3 = transform.eulerAngles;
            v3.y += y;
            transform.eulerAngles = v3;
        }


        public static void SetRelativeRotationZ(this Transform transform, float z)
        {
            var v3 = transform.eulerAngles;
            v3.z += z;
            transform.eulerAngles = v3;
        }

        #endregion


        #region Relative Rotation XY

        public static void SetRelativeRotationXY(this Transform transform, in Vector2 v2)
        {
            var v3 = transform.eulerAngles;
            v3.x += v2.x;
            v3.y += v2.y;
            transform.eulerAngles = v3;
        }


        public static void SetRelativeRotationXY(this Transform transform, float x, float y)
        {
            var v3 = transform.eulerAngles;
            v3.x += x;
            v3.y += y;
            transform.eulerAngles = v3;
        }


        public static void SetRelativeRotationXY(this Transform transform, Transform target)
        {
            var v3 = transform.eulerAngles;
            var targetV3 = target.eulerAngles;
            v3.x += targetV3.x;
            v3.y += targetV3.y;
            transform.eulerAngles = v3;
        }

        #endregion


        #region Relative Rotation XZ

        public static void SetRelativeRotationXZ(this Transform transform, in Vector2 v2)
        {
            var v3 = transform.eulerAngles;
            v3.x += v2.x;
            v3.z += v2.y;
            transform.eulerAngles = v3;
        }


        public static void SetRelativeRotationXZ(this Transform transform, float x, float z)
        {
            var v3 = transform.eulerAngles;
            v3.x += x;
            v3.z += z;
            transform.eulerAngles = v3;
        }


        public static void SetRelativeRotationXZ(this Transform transform, Transform target)
        {
            var v3 = transform.eulerAngles;
            var targetV3 = target.eulerAngles;
            v3.x += targetV3.x;
            v3.z += targetV3.z;
            transform.eulerAngles = v3;
        }

        #endregion


        #region Relative Rotation YZ

        public static void SetRelativeRotationYZ(this Transform transform, in Vector2 v2)
        {
            var v3 = transform.eulerAngles;
            v3.y += v2.x;
            v3.z += v2.y;
            transform.eulerAngles = v3;
        }


        public static void SetRelativeRotationYZ(this Transform transform, float y, float z)
        {
            var v3 = transform.eulerAngles;
            v3.y += y;
            v3.z += z;
            transform.eulerAngles = v3;
        }


        public static void SetRelativeRotationYZ(this Transform transform, Transform target)
        {
            var v3 = transform.eulerAngles;
            var targetV3 = target.eulerAngles;
            v3.y += targetV3.y;
            v3.z += targetV3.z;
            transform.eulerAngles = v3;
        }

        #endregion


        #region Local Rotation X Y Z

        public static void SetLocalRotationX(this Transform transform, float x)
        {
            var v3 = transform.localEulerAngles;
            v3.x = x;
            transform.localEulerAngles = v3;
        }


        public static void SetLocalRotationY(this Transform transform, float y)
        {
            var v3 = transform.localEulerAngles;
            v3.y = y;
            transform.localEulerAngles = v3;
        }


        public static void SetLocalRotationZ(this Transform transform, float z)
        {
            var v3 = transform.localEulerAngles;
            v3.z = z;
            transform.localEulerAngles = v3;
        }

        #endregion


        #region Local Rotation XY

        public static void SetLocalRotationXY(this Transform transform, in Vector2 v2)
        {
            transform.localEulerAngles = new Vector3(v2.x, v2.y, transform.localEulerAngles.z);
        }


        public static void SetLocalRotationXY(this Transform transform, float x, float y)
        {
            transform.localEulerAngles = new Vector3(x, y, transform.localEulerAngles.z);
        }


        public static void SetLocalRotationXY(this Transform transform, Transform target)
        {
            var v3 = target.localEulerAngles;
            v3.z = transform.localEulerAngles.z;
            transform.localEulerAngles = v3;
        }

        #endregion


        #region Local Rotation XZ

        public static void GetLocalRotationXZ(this Transform transform, out Vector2 v2)
        {
            var v3 = transform.localEulerAngles;
            v2.x = v3.x;
            v2.y = v3.z;
        }


        public static Vector2 GetLocalRotationXZ(this Transform transform)
        {
            var v3 = transform.localEulerAngles;
            return new Vector2(v3.x, v3.z);
        }


        public static void SetLocalRotationXZ(this Transform transform, in Vector2 v2)
        {
            transform.localEulerAngles = new Vector3(v2.x, transform.localEulerAngles.y, v2.y);
        }


        public static void SetLocalRotationXZ(this Transform transform, float x, float z)
        {
            transform.localEulerAngles = new Vector3(x, transform.localEulerAngles.y, z);
        }


        public static void SetLocalRotationXZ(this Transform transform, Transform target)
        {
            var v3 = target.localEulerAngles;
            v3.y = transform.localEulerAngles.y;
            transform.localEulerAngles = v3;
        }

        #endregion


        #region Local Rotation YZ

        public static void GetLocalRotationYZ(this Transform transform, out Vector2 v2)
        {
            var v3 = transform.localEulerAngles;
            v2.x = v3.y;
            v2.y = v3.z;
        }


        public static Vector2 GetLocalRotationYZ(this Transform transform)
        {
            var v3 = transform.localEulerAngles;
            return new Vector2(v3.y, v3.z);
        }


        public static void SetLocalRotationYZ(this Transform transform, in Vector2 v2)
        {
            transform.localEulerAngles = new Vector3(transform.localEulerAngles.x, v2.x, v2.y);
        }


        public static void SetLocalRotationYZ(this Transform transform, float y, float z)
        {
            transform.localEulerAngles = new Vector3(transform.localEulerAngles.x, y, z);
        }


        public static void SetLocalRotationYZ(this Transform transform, Transform target)
        {
            var v3 = target.localEulerAngles;
            v3.x = transform.localEulerAngles.x;
            transform.localEulerAngles = v3;
        }

        #endregion


        #region Relative Local Rotation X Y Z

        public static void SetRelativeLocalRotationX(this Transform transform, float x)
        {
            var v3 = transform.localEulerAngles;
            v3.x += x;
            transform.localEulerAngles = v3;
        }


        public static void SetRelativeLocalRotationY(this Transform transform, float y)
        {
            var v3 = transform.localEulerAngles;
            v3.y += y;
            transform.localEulerAngles = v3;
        }


        public static void SetRelativeLocalRotationZ(this Transform transform, float z)
        {
            var v3 = transform.localEulerAngles;
            v3.z += z;
            transform.localEulerAngles = v3;
        }

        #endregion


        #region Relative Local Rotation XY

        public static void SetRelativeLocalRotationXY(this Transform transform, in Vector2 v2)
        {
            var v3 = transform.localEulerAngles;
            v3.x += v2.x;
            v3.y += v2.y;
            transform.localEulerAngles = v3;
        }


        public static void SetRelativeLocalRotationXY(this Transform transform, float x, float y)
        {
            var v3 = transform.localEulerAngles;
            v3.x += x;
            v3.y += y;
            transform.localEulerAngles = v3;
        }


        public static void SetRelativeLocalRotationXY(this Transform transform, Transform target)
        {
            var v3 = transform.localEulerAngles;
            var targetV3 = target.localEulerAngles;
            v3.x += targetV3.x;
            v3.y += targetV3.y;
            transform.localEulerAngles = v3;
        }

        #endregion


        #region Relative Local Rotation XZ

        public static void SetRelativeLocalRotationXZ(this Transform transform, in Vector2 v2)
        {
            var v3 = transform.localEulerAngles;
            v3.x += v2.x;
            v3.z += v2.y;
            transform.localEulerAngles = v3;
        }


        public static void SetRelativeLocalRotationXZ(this Transform transform, float x, float z)
        {
            var v3 = transform.localEulerAngles;
            v3.x += x;
            v3.z += z;
            transform.localEulerAngles = v3;
        }


        public static void SetRelativeLocalRotationXZ(this Transform transform, Transform target)
        {
            var v3 = transform.localEulerAngles;
            var targetV3 = target.localEulerAngles;
            v3.x += targetV3.x;
            v3.z += targetV3.z;
            transform.localEulerAngles = v3;
        }

        #endregion


        #region Relative Local Rotation YZ

        public static void SetRelativeLocalRotationYZ(this Transform transform, in Vector2 v2)
        {
            var v3 = transform.localEulerAngles;
            v3.y += v2.x;
            v3.z += v2.y;
            transform.localEulerAngles = v3;
        }


        public static void SetRelativeLocalRotationYZ(this Transform transform, float y, float z)
        {
            var v3 = transform.localEulerAngles;
            v3.y += y;
            v3.z += z;
            transform.localEulerAngles = v3;
        }


        public static void SetRelativeLocalRotationYZ(this Transform transform, Transform target)
        {
            var v3 = transform.localEulerAngles;
            var targetV3 = target.localEulerAngles;
            v3.y += targetV3.y;
            v3.z += targetV3.z;
            transform.localEulerAngles = v3;
        }

        #endregion


        #region Anchored Position X Y Z

        public static void SetAnchoredPositionX(this RectTransform rectTransform, float x)
        {
            var v2 = rectTransform.anchoredPosition;
            v2.x = x;
            rectTransform.anchoredPosition = v2;
        }


        public static void SetAnchoredPositionY(this RectTransform rectTransform, float y)
        {
            var v2 = rectTransform.anchoredPosition;
            v2.y = y;
            rectTransform.anchoredPosition = v2;
        }


        public static void SetAnchoredPositionZ(this RectTransform rectTransform, float z)
        {
            var v3 = rectTransform.anchoredPosition3D;
            v3.z = z;
            rectTransform.anchoredPosition3D = v3;
        }

        #endregion


        #region Relative Anchored Position X Y Z

        public static void SetRelativeAnchoredPositionX(this RectTransform rectTransform, float x)
        {
            var v2 = rectTransform.anchoredPosition;
            v2.x += x;
            rectTransform.anchoredPosition = v2;
        }


        public static void SetRelativeAnchoredPositionY(this RectTransform rectTransform, float y)
        {
            var v2 = rectTransform.anchoredPosition;
            v2.y += y;
            rectTransform.anchoredPosition = v2;
        }


        public static void SetRelativeAnchoredPositionZ(this RectTransform rectTransform, float z)
        {
            var v3 = rectTransform.anchoredPosition3D;
            v3.z += z;
            rectTransform.anchoredPosition3D = v3;
        }

        #endregion


        #region OffsetMax

        public static void SetOffsetMaxX(this RectTransform rectTransform, float x)
        {
            var offsetMax = rectTransform.offsetMax;
            offsetMax.x = x;
            rectTransform.offsetMax = offsetMax;
        }


        public static void SetOffsetMaxY(this RectTransform rectTransform, float y)
        {
            var offsetMax = rectTransform.offsetMax;
            offsetMax.y = y;
            rectTransform.offsetMax = offsetMax;
        }

        #endregion


        #region Relative OffsetMax

        public static void SetRelativeOffsetMaxX(this RectTransform rectTransform, float x)
        {
            var offsetMax = rectTransform.offsetMax;
            offsetMax.x += x;
            rectTransform.offsetMax = offsetMax;
        }


        public static void SetRelativeOffsetMaxY(this RectTransform rectTransform, float y)
        {
            var offsetMax = rectTransform.offsetMax;
            offsetMax.y += y;
            rectTransform.offsetMax = offsetMax;
        }

        #endregion


        #region OffsetMin

        public static void SetOffsetMinX(this RectTransform rectTransform, float x)
        {
            var offsetMin = rectTransform.offsetMin;
            offsetMin.x = x;
            rectTransform.offsetMin = offsetMin;
        }


        public static void SetOffsetMinY(this RectTransform rectTransform, float y)
        {
            var offsetMin = rectTransform.offsetMin;
            offsetMin.y = y;
            rectTransform.offsetMin = offsetMin;
        }

        #endregion


        #region Relative OffsetMin

        public static void SetRelativeOffsetMinX(this RectTransform rectTransform, float x)
        {
            var offsetMin = rectTransform.offsetMin;
            offsetMin.x += x;
            rectTransform.offsetMin = offsetMin;
        }


        public static void SetSetRelativeOffsetMinY(this RectTransform rectTransform, float y)
        {
            var offsetMin = rectTransform.offsetMin;
            offsetMin.y += y;
            rectTransform.offsetMin = offsetMin;
        }

        #endregion


        #region SizeDelta

        /// <summary>
        /// Get the sizeDelta x with anchor by the sizeX.
        /// </summary>
        public static float GetSizeDeltaX(this RectTransform rectTransform, float sizeX)
        {
            // ReSharper disable once PossibleNullReferenceException
            var parentSize = (rectTransform.parent as RectTransform).rect.size;
            return sizeX - parentSize[0] * (rectTransform.anchorMax[0] - rectTransform.anchorMin[0]);
        }


        /// <summary>
        /// Get the sizeDelta y with anchor by the sizeY.
        /// </summary>
        public static float GetSizeDeltaY(this RectTransform rectTransform, float sizeY)
        {
            // ReSharper disable once PossibleNullReferenceException
            var parentSize = (rectTransform.parent as RectTransform).rect.size;
            return sizeY - parentSize[1] * (rectTransform.anchorMax[1] - rectTransform.anchorMin[1]);
        }


        /// <summary>
        /// Get the sizeDelta with anchor by the size.
        /// </summary>
        public static void GetSizeDelta(this RectTransform rectTransform, in Vector2 size, out Vector2 sizeDelta)
        {
            // ReSharper disable once PossibleNullReferenceException
            var parentSize = (rectTransform.parent as RectTransform).rect.size;
            sizeDelta = size - parentSize * (rectTransform.anchorMax - rectTransform.anchorMin);
        }


        /// <summary>
        /// Get the sizeDelta with anchor by the size.
        /// </summary>
        public static Vector2 GetSizeDelta(this RectTransform rectTransform, in Vector2 size)
        {
            // ReSharper disable once PossibleNullReferenceException
            var parentSize = (rectTransform.parent as RectTransform).rect.size;
            return size - parentSize * (rectTransform.anchorMax - rectTransform.anchorMin);
        }


        public static void SetSizeDeltaX(this RectTransform rectTransform, float x)
        {
            var sizeDelta = rectTransform.sizeDelta;
            sizeDelta.x = x;
            rectTransform.sizeDelta = sizeDelta;
        }


        public static void SetSizeDeltaY(this RectTransform rectTransform, float y)
        {
            var sizeDelta = rectTransform.sizeDelta;
            sizeDelta.y = y;
            rectTransform.sizeDelta = sizeDelta;
        }

        #endregion


        #region Relative SizeDelta

        public static void SetRelativeSizeDeltaX(this RectTransform rectTransform, float x)
        {
            var sizeDelta = rectTransform.sizeDelta;
            sizeDelta.x += x;
            rectTransform.sizeDelta = sizeDelta;
        }


        public static void SetRelativeSizeDeltaY(this RectTransform rectTransform, float y)
        {
            var sizeDelta = rectTransform.sizeDelta;
            sizeDelta.y += y;
            rectTransform.sizeDelta = sizeDelta;
        }

        #endregion

        #region Distance

        public static float Distance(this Transform transform, Transform target) => Vector3.Distance(transform.position, target.position);

        public static float Distance(this Transform transform, Vector3 target) => Vector3.Distance(transform.position, target);

        public static float DistanceWithoutY(this Transform transform, Transform target) => Vector3.Distance(transform.position.WithY(0), target.position.WithY(0));

        public static float DistanceWithoutY(this Transform transform, Vector3 target) => Vector3.Distance(transform.position.WithY(0), target.WithY(0));

        #endregion

        /// <summary>
        /// 
        /// </summary>
        /// <param name="source"></param>
        /// <param name="parent"></param>
        public static void FillWithParent(this RectTransform source, RectTransform parent)
        {
            source.SetParent(parent);
            source.Fill();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="source"></param>
        /// <param name="parent"></param>
        /// <param name="worldPositionStatys"></param>
        public static void FillWithParent(this RectTransform source, Transform parent, bool worldPositionStatys = true)
        {
            source.SetParent(parent, worldPositionStatys);
            source.Fill();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="source"></param>
        public static void Fill(this RectTransform source)
        {
            source.localPosition = Vector3.zero;
            source.anchorMin = Vector2.zero;
            source.anchorMax = Vector2.one;
            source.offsetMin = Vector2.zero;
            source.offsetMax = Vector2.zero;
            source.pivot = new Vector2(0.5f, 0.5f);
            source.localRotation = Quaternion.identity;
            source.localScale = Vector3.one;
        }

        /// <summary>
        /// Destroy your self
        /// </summary>
        /// <param name="gameObject"></param>
        public static void Destroy(this GameObject gameObject)
        {
            if (!gameObject) return;

#if UNITY_EDITOR
            if (!Application.isPlaying)
            {
                Object.DestroyImmediate(gameObject);
                return;
            }
#endif
            Object.Destroy(gameObject);
        }

        /// <summary>
        /// Safe destroy your self
        /// </summary>
        /// <param name="gameObject"></param>
        public static void SafeDestroy(ref GameObject gameObject)
        {
            gameObject.Destroy();
            gameObject = null;
        }

        /// <summary>
        /// Destroys all gameObject's children
        /// </summary>
        /// <param name="transform"></param>
        /// <param name="force"></param>
        public static void RemoveAllChildren(this Transform transform, bool force = false)
        {
#if UNITY_EDITOR
            UnityEditor.Undo.RecordObject(transform, "Transform RemoveAllChildren");
#endif

            for (int i = transform.childCount - 1; i >= 0; i--)
            {
                if (force || !Application.isPlaying)
                {
                    Object.DestroyImmediate(transform.GetChild(i).gameObject);
                    continue;
                }

                Object.Destroy(transform.GetChild(i).gameObject);
            }
        }

        /// <summary>
        /// Destroys all gameObject children
        /// </summary>
        /// <param name="gameObject"></param>
        public static void RemoveAllChildren(this GameObject gameObject) { gameObject.transform.RemoveAllChildren(); }

        /// <summary>
        /// Finds children by name, breadth first
        /// </summary>
        /// <param name="root"></param>
        /// <param name="transformName"></param>
        /// <returns></returns>
        public static Transform FindDeepChildBreadthFirst(this Transform root, string transformName)
        {
            var queue = new Queue<Transform>();
            queue.Enqueue(root);
            while (queue.Count > 0)
            {
                var child = queue.Dequeue();
                if (child.name == transformName)
                {
                    return child;
                }

                foreach (Transform t in child)
                {
                    queue.Enqueue(t);
                }
            }

            return null;
        }

        /// <summary>
        /// Finds children by name, depth first
        /// </summary>
        /// <param name="root"></param>
        /// <param name="transformName"></param>
        /// <returns></returns>
        public static Transform FindDeepChildDepthFirst(this Transform root, string transformName)
        {
            foreach (Transform child in root)
            {
                if (child.name == transformName)
                {
                    return child;
                }

                var result = child.FindDeepChildDepthFirst(transformName);
                if (result != null)
                {
                    return result;
                }
            }

            return null;
        }

        /// <summary>
        /// Changes the layer of a transform and all its children to the new one
        /// </summary>
        /// <param name="transform"></param>
        /// <param name="layerName"></param>
        public static void ChangeLayersRecursively(this Transform transform, string layerName)
        {
            transform.gameObject.layer = LayerMask.NameToLayer(layerName);
            foreach (Transform child in transform)
            {
                child.ChangeLayersRecursively(layerName);
            }
        }

        /// <summary>
        /// Changes the layer of a transform and all its children to the new one
        /// </summary>
        /// <param name="transform"></param>
        /// <param name="layerIndex"></param>
        public static void ChangeLayersRecursively(this Transform transform, int layerIndex)
        {
            transform.gameObject.layer = layerIndex;
            foreach (Transform child in transform)
            {
                child.ChangeLayersRecursively(layerIndex);
            }
        }

        /// <summary>
        /// Traverse transform tree (root node first).
        /// </summary>
        /// <param name="root"> The root node of transform tree. </param>
        /// <param name="operate"> A custom operation on every transform node. </param>
        /// <param name="depthLimit"> Negative value means no limit, zero means root only, positive value means maximum children depth </param>
        public static void TraverseHierarchy(this Transform root, Action<Transform> operate, int depthLimit = -1)
        {
            operate(root);

            if (depthLimit != 0)
            {
                int count = root.childCount;
                for (int i = 0; i < count; i++)
                {
                    TraverseHierarchy(root.GetChild(i), operate, depthLimit - 1);
                }
            }
        }

        /// <summary>
        /// Traverse transform tree (leaf node first).
        /// </summary>
        /// <param name="root"> The root node of transform tree. </param>
        /// <param name="operate"> A custom operation on every transform node. </param>
        /// <param name="depthLimit"> Negative value means no limit, zero means root only, positive value means maximum children depth </param>
        public static void InverseTraverseHierarchy(this Transform root, Action<Transform> operate, int depthLimit = -1)
        {
            if (depthLimit != 0)
            {
                int count = root.childCount;
                for (int i = 0; i < count; i++)
                {
                    InverseTraverseHierarchy(root.GetChild(i), operate, depthLimit - 1);
                }
            }

            operate(root);
        }

        /// <summary>
        /// Find a transform in the transform tree (root node first)
        /// </summary>
        /// <param name="root"> The root node of transform tree. </param>
        /// <param name="match"> match function. </param>
        /// <param name="depthLimit"> Negative value means no limit, zero means root only, positive value means maximum children depth </param>
        /// <returns> The matched node or null if no matched. </returns>
        public static Transform SearchHierarchy(this Transform root, Predicate<Transform> match, int depthLimit = -1)
        {
            if (match(root)) return root;
            if (depthLimit == 0) return null;

            int count = root.childCount;
            Transform result = null;

            for (int i = 0; i < count; i++)
            {
                result = SearchHierarchy(root.GetChild(i), match, depthLimit - 1);
                if (result) break;
            }

            return result;
        }

        public static T GetOrAddComponent<T>(this Component source) where T : Component
        {
            if (!source.TryGetComponent<T>(out var component)) component = source.gameObject.AddComponent<T>();
            return component;
        }

        public static T GetOrAddComponent<T>(this GameObject source) where T : Component
        {
            if (!source.TryGetComponent<T>(out var component)) component = source.AddComponent<T>();
            return component;
        }

        /// <summary>
        /// manually update Content Size Fitter immediately when it does not update automatically
        /// </summary>
        /// <param name="fitter"></param>
        public static void ManualUpdate(this UnityEngine.UI.ContentSizeFitter fitter)
        {
            fitter.SetLayoutHorizontal();
            fitter.SetLayoutVertical();
            UnityEngine.UI.LayoutRebuilder.ForceRebuildLayoutImmediate(fitter.GetComponent<RectTransform>());
        }

        /// <summary>
        /// Convert UI positon to world position
        /// </summary>
        /// <param name="transform">transform is transform in canvas space [RectTransfom]</param>
        /// <param name="camera"></param>
        /// <returns></returns>
        public static Vector3 ToWorldPosition(this RectTransform transform, Camera camera = null)
        {
            RectTransformUtility.ScreenPointToLocalPointInRectangle(transform, transform.position, camera, out var local);
            var world = transform.TransformPoint(new Vector3(local.x, local.y));
            return world;
        }

        /// <summary>
        /// Convert GameObject positon to UI position
        /// </summary>
        /// <param name="transform">transform is transform in world space</param>
        /// <param name="camera"></param>
        /// <returns></returns>
        public static Vector2 ToUIPosition(this Transform transform, Camera camera = null)
        {
            var world = transform.TransformPoint(Vector3.zero);
            return RectTransformUtility.WorldToScreenPoint(camera, world);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="scroll"></param>
        /// <param name="target"></param>
        /// <param name="isVertical"></param>
        public static void ScrollTo(this UnityEngine.UI.ScrollRect scroll, Transform target, bool isVertical = true)
        {
            if (isVertical)
            {
                scroll.normalizedPosition = new Vector2(0, 1 - (scroll.content.rect.height / 2 - target.localPosition.y) / scroll.content.rect.height);
                return;
            }

            scroll.normalizedPosition = new Vector2(1 - (scroll.content.rect.width / 2 - target.localPosition.x) / scroll.content.rect.width, 0);
        }

        /// <summary>
        /// m_MyGameObject is not truly null, but rather a sort of “null simulator” <br/>
        /// Then you can call: <br/>gameObject.OrNull()?.DoSomething();
        /// </summary>
        /// <param name="source"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static T OrNull<T>(this T source) where T : Object { return source == null ? null : source; }

        /// <summary>
        /// Check if the transform is within a certain distance and optionally within a certain angle (FOV) from the target transform.
        /// </summary>
        /// <param name="source">The transform to check.</param>
        /// <param name="target">The target transform to compare the distance and optional angle with.</param>
        /// <param name="maxDistance">The maximum distance allowed between the two transforms.</param>
        /// <param name="maxAngle">The maximum allowed angle between the transform's forward vector and the direction to the target (default is 360).</param>
        /// <returns>True if the transform is within range and angle (if provided) of the target, false otherwise.</returns>
        public static bool InRangeOf(this Transform source, Transform target, float maxDistance, float maxAngle = 360f)
        {
            var directionToTarget = target.position - source.position;
            directionToTarget.y = 0;
            return directionToTarget.magnitude <= maxDistance && Vector3.Angle(source.forward, directionToTarget) <= maxAngle / 2;
        }
    }
}