namespace Pancake.Tween
{
    using UnityEngine;

    public static partial class TweenExtension
    {
        public static ITween TweenPosition(this Rigidbody rigidbody, Vector3 to, float duration)
        {
            return Tween.To(() => rigidbody.position,
                rigidbody.MovePosition,
                () => to,
                duration,
                () => rigidbody != null);
        }

        public static ITween TweenRotation(this Rigidbody rigidbody, Vector3 to, float duration)
        {
            return Tween.To(() => rigidbody.rotation.eulerAngles,
                current => rigidbody.MoveRotation(Quaternion.Euler(current)),
                () => to,
                duration,
                () => rigidbody != null);
        }
    }
}