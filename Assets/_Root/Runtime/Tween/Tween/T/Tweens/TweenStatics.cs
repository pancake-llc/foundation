using UnityEngine;

namespace Pancake.Tween
{
    public abstract partial class Tween : ITween
    {
        public static ITween To(
            Tweener<int>.Getter currValueGetter,
            Tweener<int>.Setter setter,
            Tweener<int>.Getter finalValueGetter,
            float duration,
            Tweener<int>.Validation validation = null)
        {
            InterpolationTween tween = new InterpolationTween();
            tween.Add(new IntTweener(currValueGetter,
                setter,
                finalValueGetter,
                duration,
                validation));
            return tween;
        }

        public static ITween To(
            Tweener<float>.Getter currValueGetter,
            Tweener<float>.Setter setter,
            Tweener<float>.Getter finalValueGetter,
            float duration,
            Tweener<float>.Validation validation = null)
        {
            InterpolationTween tween = new InterpolationTween();
            tween.Add(new FloatTweener(currValueGetter,
                setter,
                finalValueGetter,
                duration,
                validation));
            return tween;
        }

        public static ITween To(
            Tweener<Vector2>.Getter currValueGetter,
            Tweener<Vector2>.Setter setter,
            Tweener<Vector2>.Getter finalValueGetter,
            float duration,
            Tweener<Vector2>.Validation validation = null)
        {
            InterpolationTween tween = new InterpolationTween();
            tween.Add(new Vector2Tweener(currValueGetter,
                setter,
                finalValueGetter,
                duration,
                validation));
            return tween;
        }

        public static ITween To(
            Tweener<Vector3>.Getter currValueGetter,
            Tweener<Vector3>.Setter setter,
            Tweener<Vector3>.Getter finalValueGetter,
            float duration,
            Tweener<Vector3>.Validation validation = null)
        {
            InterpolationTween tween = new InterpolationTween();
            tween.Add(new Vector3Tweener(currValueGetter,
                setter,
                finalValueGetter,
                duration,
                validation));
            return tween;
        }

        public static ITween To(
            Tweener<Vector3>.Getter currValueGetter,
            Tweener<Vector3>.Setter setter,
            Tweener<Vector3>.Getter finalValueGetter,
            RotationMode rotationMode,
            float duration,
            Tweener<Vector3>.Validation validation = null)
        {
            InterpolationTween tween = new InterpolationTween();
            tween.Add(new Vector3RotationTweener(currValueGetter,
                setter,
                finalValueGetter,
                rotationMode,
                duration,
                validation));
            return tween;
        }

        public static ITween To(
            Tweener<Vector4>.Getter currValueGetter,
            Tweener<Vector4>.Setter setter,
            Tweener<Vector4>.Getter finalValueGetter,
            float duration,
            Tweener<Vector4>.Validation validation = null)
        {
            InterpolationTween tween = new InterpolationTween();
            tween.Add(new Vector4Tweener(currValueGetter,
                setter,
                finalValueGetter,
                duration,
                validation));
            return tween;
        }

        public static ITween To(
            Tweener<Rect>.Getter currValueGetter,
            Tweener<Rect>.Setter setter,
            Tweener<Rect>.Getter finalValueGetter,
            float duration,
            Tweener<Rect>.Validation validation = null)
        {
            InterpolationTween tween = new InterpolationTween();
            tween.Add(new RectTweener(currValueGetter,
                setter,
                finalValueGetter,
                duration,
                validation));
            return tween;
        }

        public static ITween To(
            Tweener<Color>.Getter currValueGetter,
            Tweener<Color>.Setter setter,
            Tweener<Color>.Getter finalValueGetter,
            float duration,
            Tweener<Color>.Validation validation = null)
        {
            InterpolationTween tween = new InterpolationTween();
            tween.Add(new ColorTweener(currValueGetter,
                setter,
                finalValueGetter,
                duration,
                validation));
            return tween;
        }

        public static ITween To(
            Tweener<Quaternion>.Getter currValueGetter,
            Tweener<Quaternion>.Setter setter,
            Tweener<Quaternion>.Getter finalValueGetter,
            float duration,
            Tweener<Quaternion>.Validation validation = null)
        {
            InterpolationTween tween = new InterpolationTween();
            tween.Add(new QuaternionTweener(currValueGetter,
                setter,
                finalValueGetter,
                duration,
                validation));
            return tween;
        }

        public static ITween To(
            Tweener<int>.Getter[] currValueGetter,
            Tweener<int>.Setter[] setter,
            Tweener<int>.Getter[] finalValueGetter,
            float duration,
            Tweener<int>.Validation validation = null)
        {
            InterpolationTween tween = new InterpolationTween();
            for (int i = 0; i < currValueGetter.Length; ++i)
            {
                if (setter.Length > i && finalValueGetter.Length > i)
                {
                    tween.Add(new IntTweener(currValueGetter[i],
                        setter[i],
                        finalValueGetter[i],
                        duration,
                        validation));
                }
            }

            return tween;
        }

        public static ITween To(
            Tweener<float>.Getter[] currValueGetter,
            Tweener<float>.Setter[] setter,
            Tweener<float>.Getter[] finalValueGetter,
            float duration,
            Tweener<float>.Validation validation = null)
        {
            InterpolationTween tween = new InterpolationTween();
            for (int i = 0; i < currValueGetter.Length; ++i)
            {
                if (setter.Length > i && finalValueGetter.Length > i)
                {
                    tween.Add(new FloatTweener(currValueGetter[i],
                        setter[i],
                        finalValueGetter[i],
                        duration,
                        validation));
                }
            }

            return tween;
        }

        public static ITween To(
            Tweener<Vector2>.Getter[] currValueGetter,
            Tweener<Vector2>.Setter[] setter,
            Tweener<Vector2>.Getter[] finalValueGetter,
            float duration,
            Tweener<Vector2>.Validation validation = null)
        {
            InterpolationTween tween = new InterpolationTween();
            for (int i = 0; i < currValueGetter.Length; ++i)
            {
                if (setter.Length > i && finalValueGetter.Length > i)
                {
                    tween.Add(new Vector2Tweener(currValueGetter[i],
                        setter[i],
                        finalValueGetter[i],
                        duration,
                        validation));
                }
            }

            return tween;
        }

        public static ITween To(
            Tweener<Vector3>.Getter[] currValueGetter,
            Tweener<Vector3>.Setter[] setter,
            Tweener<Vector3>.Getter[] finalValueGetter,
            float duration,
            Tweener<Vector3>.Validation validation = null)
        {
            InterpolationTween tween = new InterpolationTween();
            for (int i = 0; i < currValueGetter.Length; ++i)
            {
                if (setter.Length > i && finalValueGetter.Length > i)
                {
                    tween.Add(new Vector3Tweener(currValueGetter[i],
                        setter[i],
                        finalValueGetter[i],
                        duration,
                        validation));
                }
            }

            return tween;
        }

        public static ITween To(
            Tweener<Vector4>.Getter[] currValueGetter,
            Tweener<Vector4>.Setter[] setter,
            Tweener<Vector4>.Getter[] finalValueGetter,
            float duration,
            Tweener<Vector4>.Validation validation = null)
        {
            InterpolationTween tween = new InterpolationTween();
            for (int i = 0; i < currValueGetter.Length; ++i)
            {
                if (setter.Length > i && finalValueGetter.Length > i)
                {
                    tween.Add(new Vector4Tweener(currValueGetter[i],
                        setter[i],
                        finalValueGetter[i],
                        duration,
                        validation));
                }
            }

            return tween;
        }

        public static ITween To(
            Tweener<Rect>.Getter[] currValueGetter,
            Tweener<Rect>.Setter[] setter,
            Tweener<Rect>.Getter[] finalValueGetter,
            float duration,
            Tweener<Rect>.Validation validation = null)
        {
            InterpolationTween tween = new InterpolationTween();
            for (int i = 0; i < currValueGetter.Length; ++i)
            {
                if (setter.Length > i && finalValueGetter.Length > i)
                {
                    tween.Add(new RectTweener(currValueGetter[i],
                        setter[i],
                        finalValueGetter[i],
                        duration,
                        validation));
                }
            }

            return tween;
        }

        public static ITween To(
            Tweener<Color>.Getter[] currValueGetter,
            Tweener<Color>.Setter[] setter,
            Tweener<Color>.Getter[] finalValueGetter,
            float duration,
            Tweener<Color>.Validation validation = null)
        {
            InterpolationTween tween = new InterpolationTween();
            for (int i = 0; i < currValueGetter.Length; ++i)
            {
                if (setter.Length > i && finalValueGetter.Length > i)
                {
                    tween.Add(new ColorTweener(currValueGetter[i],
                        setter[i],
                        finalValueGetter[i],
                        duration,
                        validation));
                }
            }

            return tween;
        }
    }
}