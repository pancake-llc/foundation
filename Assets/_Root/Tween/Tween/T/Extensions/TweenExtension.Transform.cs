namespace Pancake.Core.Tween
{
    using UnityEngine;

    public static partial class TweenExtension
    {
        public static ITween TweenPosition(this Transform transform, Vector3 to, float duration)
        {
            return Tween.To(() => transform.position,
                current => transform.position = current,
                () => to,
                duration,
                () => transform != null);
        }

        public static ITween TweenPositionX(this Transform transform, float to, float duration)
        {
            return Tween.To(() => transform.position.x,
                current => transform.position = transform.position.Change(x: current),
                () => to,
                duration,
                () => transform != null);
        }

        public static ITween TweenPositionY(this Transform transform, float to, float duration)
        {
            return Tween.To(() => transform.position.y,
                current => transform.position = transform.position.Change(y: current),
                () => to,
                duration,
                () => transform != null);
        }

        public static ITween TweenPositionZ(this Transform transform, float to, float duration)
        {
            return Tween.To(() => transform.position.z,
                current => transform.position = transform.position.Change(z: current),
                () => to,
                duration,
                () => transform != null);
        }

        public static ITween TweenLocalPosition(this Transform transform, Vector3 to, float duration)
        {
            return Tween.To(() => transform.localPosition,
                current => transform.localPosition = current,
                () => to,
                duration,
                () => transform != null);
        }

        public static ITween TweenLocalPositionX(this Transform transform, float to, float duration)
        {
            return Tween.To(() => transform.localPosition.x,
                current => transform.localPosition = transform.localPosition.Change(x: current),
                () => to,
                duration,
                () => transform != null);
        }

        public static ITween TweenLocalPositionY(this Transform transform, float to, float duration)
        {
            return Tween.To(() => transform.localPosition.y,
                current => transform.localPosition = transform.localPosition.Change(y: current),
                () => to,
                duration,
                () => transform != null);
        }

        public static ITween TweenLocalPositionZ(this Transform transform, float to, float duration)
        {
            return Tween.To(() => transform.localPosition.z,
                current => transform.localPosition = transform.localPosition.Change(z: current),
                () => to,
                duration,
                () => transform != null);
        }

        public static ITween TweenRotation(this Transform transform, Vector3 to, float duration, RotationMode mode)
        {
            return Tween.To(() => transform.rotation.eulerAngles,
                current => transform.rotation = Quaternion.Euler(current),
                () => AngleUtils.GetDestinationAngle(transform.rotation.eulerAngles, to, mode),
                duration,
                () => transform != null);
        }

        public static ITween TweenRotationX(this Transform transform, float to, float duration, RotationMode mode)
        {
            return Tween.To(() => transform.rotation.eulerAngles.x,
                current => transform.rotation = Quaternion.Euler(current, transform.rotation.eulerAngles.y, transform.rotation.eulerAngles.z),
                () => AngleUtils.GetDestinationAngle(transform.rotation.eulerAngles.x, to, mode),
                duration,
                () => transform != null);
        }

        public static ITween TweenRotationY(this Transform transform, float to, float duration, RotationMode mode)
        {
            return Tween.To(() => transform.rotation.eulerAngles.y,
                current => transform.rotation = Quaternion.Euler(transform.rotation.eulerAngles.x, current, transform.rotation.eulerAngles.z),
                () => AngleUtils.GetDestinationAngle(transform.rotation.eulerAngles.y, to, mode),
                duration,
                () => transform != null);
        }

        public static ITween TweenRotationZ(this Transform transform, float to, float duration, RotationMode mode)
        {
            return Tween.To(() => transform.rotation.eulerAngles.z,
                current => transform.rotation = Quaternion.Euler(transform.rotation.eulerAngles.x, transform.rotation.eulerAngles.y, current),
                () => AngleUtils.GetDestinationAngle(transform.rotation.eulerAngles.z, to, mode),
                duration,
                () => transform != null);
        }

        public static ITween TweenLocalRotation(this Transform transform, Vector3 to, float duration, RotationMode mode)
        {
            return Tween.To(() => transform.localRotation.eulerAngles,
                current => transform.localRotation = Quaternion.Euler(current),
                () => AngleUtils.GetDestinationAngle(transform.localRotation.eulerAngles, to, mode),
                mode,
                duration,
                () => transform != null);
        }

        public static ITween TweenLocalRotationX(this Transform transform, float to, float duration, RotationMode mode)
        {
            return Tween.To(() => transform.localRotation.eulerAngles.x,
                current => transform.localRotation = Quaternion.Euler(current, transform.localRotation.eulerAngles.y, transform.localRotation.eulerAngles.z),
                () => AngleUtils.GetDestinationAngle(transform.localRotation.eulerAngles.x, to, mode),
                duration,
                () => transform != null);
        }

        public static ITween TweenLocalRotationY(this Transform transform, float to, float duration, RotationMode mode)
        {
            return Tween.To(() => transform.localRotation.eulerAngles.y,
                current => transform.localRotation = Quaternion.Euler(transform.localRotation.eulerAngles.x, current, transform.localRotation.eulerAngles.z),
                () => AngleUtils.GetDestinationAngle(transform.localRotation.eulerAngles.y, to, mode),
                duration,
                () => transform != null);
        }

        public static ITween TweenLocalRotationZ(this Transform transform, float to, float duration, RotationMode mode)
        {
            return Tween.To(() => transform.localRotation.eulerAngles.z,
                current => transform.localRotation = Quaternion.Euler(transform.localRotation.eulerAngles.x, transform.localRotation.eulerAngles.y, current),
                () => AngleUtils.GetDestinationAngle(transform.localRotation.eulerAngles.z, to, mode),
                duration,
                () => transform != null);
        }

        public static ITween TweenLocalScale(this Transform transform, Vector3 to, float duration)
        {
            return Tween.To(() => transform.localScale,
                current => transform.localScale = current,
                () => to,
                duration,
                () => transform != null);
        }

        public static ITween TweenLocalScaleX(this Transform transform, float to, float duration)
        {
            return Tween.To(() => transform.localScale.x,
                current => transform.localScale = transform.localScale.Change(x: current),
                () => to,
                duration,
                () => transform != null);
        }

        public static ITween TweenLocalScaleY(this Transform transform, float to, float duration)
        {
            return Tween.To(() => transform.localScale.y,
                current => transform.localScale = transform.localScale.Change(y: current),
                () => to,
                duration,
                () => transform != null);
        }

        public static ITween TweenLocalScaleZ(this Transform transform, float to, float duration)
        {
            return Tween.To(() => transform.localScale.z,
                current => transform.localScale = transform.localScale.Change(z: current),
                () => to,
                duration,
                () => transform != null);
        }
    }
}