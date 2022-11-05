using System;
using UnityEngine;

namespace Pancake.UI
{
    [Serializable]
    public class RectTransformData
    {
        public static readonly RectTransformData Invalid = new RectTransformData();
        public static readonly RectTransformData Identity = new RectTransformData() {Rotation = Quaternion.identity, scale = Vector3.one,};


        public Vector3 localPosition;
        public Vector2 anchoredPosition;
        public Vector2 sizeDelta;
        public Vector2 anchorMin;
        public Vector2 anchorMax;
        public Vector2 pivot;
        public Vector3 scale;

        [SerializeField] private Quaternion rotation;

        public Vector3 eulerAngles;

        public Quaternion Rotation
        {
            get => saveRotationAsEuler ? Quaternion.Euler(eulerAngles) : rotation;
            set
            {
                rotation = value;

                if (saveRotationAsEuler)
                {
                    eulerAngles = rotation.eulerAngles;
                }
            }
        }

        [SerializeField] private bool saveRotationAsEuler = false;

        public bool SaveRotationAsEuler
        {
            get => saveRotationAsEuler;
            set
            {
                if (saveRotationAsEuler == value) return;

                saveRotationAsEuler = value;
            }
        }

        /// <summary>
        ///   <para>The offset of the upper right corner of the rectangle relative to the upper right anchor.</para>
        /// </summary>
        public Vector2 OffsetMax
        {
            get => anchoredPosition + Vector2.Scale(sizeDelta, Vector2.one - pivot);
            set
            {
                Vector2 v = value - (anchoredPosition + Vector2.Scale(sizeDelta, Vector2.one - pivot));
                sizeDelta = sizeDelta + v;
                anchoredPosition = anchoredPosition + Vector2.Scale(v, pivot);
            }
        }

        /// <summary>
        ///   <para>The offset of the lower left corner of the rectangle relative to the lower left anchor.</para>
        /// </summary>
        public Vector2 OffsetMin
        {
            get => anchoredPosition - Vector2.Scale(sizeDelta, pivot);
            set
            {
                Vector2 v = value - (anchoredPosition - Vector2.Scale(sizeDelta, pivot));
                sizeDelta = sizeDelta - v;
                anchoredPosition = anchoredPosition + Vector2.Scale(v, Vector2.one - pivot);
            }
        }

        [SerializeField] private string screenConfigName;
        public string ScreenConfigName { get => screenConfigName; set => screenConfigName = value; }

        public RectTransformData() { }

        public RectTransformData(RectTransform rectTransform) { PullFromTransform(rectTransform); }

        public static RectTransformData Combine(RectTransformData original, RectTransformData addition)
        {
            RectTransformData result = new RectTransformData();

            result.anchoredPosition = original.anchoredPosition + addition.anchoredPosition;
            result.anchorMin = original.anchorMin + addition.anchorMin;
            result.anchorMax = original.anchorMax + addition.anchorMax;
            result.pivot = original.pivot + addition.pivot;
            result.sizeDelta = original.sizeDelta + addition.sizeDelta;
            result.localPosition = original.localPosition + addition.localPosition;
            result.scale = new Vector3(original.scale.x * addition.scale.x, original.scale.y * addition.scale.y, original.scale.z * addition.scale.z);

            result.saveRotationAsEuler = original.saveRotationAsEuler;
            result.Rotation = original.Rotation * addition.Rotation;

            return result;
        }

        public static RectTransformData Separate(RectTransformData original, RectTransformData subtraction)
        {
            RectTransformData result = new RectTransformData();
            result.anchoredPosition = original.anchoredPosition - subtraction.anchoredPosition;
            result.anchorMin = original.anchorMin - subtraction.anchorMin;
            result.anchorMax = original.anchorMax - subtraction.anchorMax;
            result.pivot = original.pivot - subtraction.pivot;
            result.sizeDelta = original.sizeDelta - subtraction.sizeDelta;
            result.localPosition = original.localPosition - subtraction.localPosition;
            result.scale = new Vector3(original.scale.x / subtraction.scale.x, original.scale.y / subtraction.scale.y, original.scale.z / subtraction.scale.z);

            result.saveRotationAsEuler = original.saveRotationAsEuler;
            result.Rotation = original.Rotation * Quaternion.Inverse(subtraction.Rotation);

            return result;
        }

        public RectTransformData PullFromData(RectTransformData transformData)
        {
            localPosition = transformData.localPosition;
            anchorMin = transformData.anchorMin;
            anchorMax = transformData.anchorMax;
            pivot = transformData.pivot;
            anchoredPosition = transformData.anchoredPosition;
            sizeDelta = transformData.sizeDelta;
            scale = transformData.scale;

            saveRotationAsEuler = transformData.saveRotationAsEuler;
            Rotation = transformData.Rotation;
            eulerAngles = transformData.eulerAngles;

            return this;
        }

        public void PullFromTransform(RectTransform transform)
        {
            localPosition = transform.localPosition;
            anchorMin = transform.anchorMin;
            anchorMax = transform.anchorMax;
            pivot = transform.pivot;
            anchoredPosition = transform.anchoredPosition;
            sizeDelta = transform.sizeDelta;
            scale = transform.localScale;

            Rotation = transform.localRotation;
            eulerAngles = transform.localEulerAngles;
        }


        public void PushToTransform(RectTransform transform)
        {
            transform.localPosition = localPosition;
            transform.anchorMin = anchorMin;
            transform.anchorMax = anchorMax;
            transform.pivot = pivot;
            transform.anchoredPosition = anchoredPosition;
            transform.sizeDelta = sizeDelta;
            transform.localScale = scale;

            if (SaveRotationAsEuler)
            {
                transform.eulerAngles = eulerAngles;
            }
            else
            {
                transform.localRotation = Rotation;
            }
        }

        public Rect ToRect(Rect parentRect, bool relativeSpace = false)
        {
            float xMin = anchorMin.x * parentRect.width + anchoredPosition.x - pivot.x * sizeDelta.x;
            float xMax = anchorMax.x * parentRect.width + anchoredPosition.x + (1 - pivot.x) * sizeDelta.x;

            float yMin = anchorMin.y * parentRect.height + anchoredPosition.y - pivot.y * sizeDelta.y;
            float yMax = anchorMax.y * parentRect.height + anchoredPosition.y + (1 - pivot.y) * sizeDelta.y;

            if (relativeSpace)
            {
                xMin /= parentRect.width;
                xMax /= parentRect.width;
                yMin /= parentRect.height;
                yMax /= parentRect.height;
            }

            return new Rect(xMin, yMin, xMax - xMin, yMax - yMin);
        }

        public static RectTransformData Lerp(RectTransformData a, RectTransformData b, float amount)
        {
            return Lerp(a, b, amount, a.SaveRotationAsEuler || b.SaveRotationAsEuler);
        }

        public static RectTransformData Lerp(RectTransformData a, RectTransformData b, float amount, bool eulerRotation)
        {
            return new RectTransformData()
            {
                anchoredPosition = Vector2.Lerp(a.anchoredPosition, b.anchoredPosition, amount),
                anchorMax = Vector2.Lerp(a.anchorMax, b.anchorMax, amount),
                anchorMin = Vector2.Lerp(a.anchorMin, b.anchorMin, amount),
                localPosition = Vector3.Lerp(a.localPosition, b.localPosition, amount),
                pivot = Vector2.Lerp(a.pivot, b.pivot, amount),
                scale = Vector3.Lerp(a.scale, b.scale, amount),
                sizeDelta = Vector2.Lerp(a.sizeDelta, b.sizeDelta, amount),
                Rotation = Quaternion.Lerp(a.Rotation, b.Rotation, amount),
                eulerAngles = Vector3.Lerp(a.eulerAngles, b.eulerAngles, amount),
                SaveRotationAsEuler = eulerRotation
            };
        }

        public static RectTransformData LerpUnclamped(RectTransformData a, RectTransformData b, float amount)
        {
            return LerpUnclamped(a, b, amount, a.SaveRotationAsEuler || b.SaveRotationAsEuler);
        }

        public static RectTransformData LerpUnclamped(RectTransformData a, RectTransformData b, float amount, bool eulerRotation)
        {
            return new RectTransformData()
            {
                anchoredPosition = Vector2.LerpUnclamped(a.anchoredPosition, b.anchoredPosition, amount),
                anchorMax = Vector2.LerpUnclamped(a.anchorMax, b.anchorMax, amount),
                anchorMin = Vector2.LerpUnclamped(a.anchorMin, b.anchorMin, amount),
                localPosition = Vector3.LerpUnclamped(a.localPosition, b.localPosition, amount),
                pivot = Vector2.LerpUnclamped(a.pivot, b.pivot, amount),
                scale = Vector3.LerpUnclamped(a.scale, b.scale, amount),
                sizeDelta = Vector2.LerpUnclamped(a.sizeDelta, b.sizeDelta, amount),
                Rotation = Quaternion.LerpUnclamped(a.Rotation, b.Rotation, amount),
                eulerAngles = Vector3.LerpUnclamped(a.eulerAngles, b.eulerAngles, amount),
                SaveRotationAsEuler = eulerRotation
            };
        }

        public override int GetHashCode()
        {
            return anchoredPosition.GetHashCode() ^ anchorMin.GetHashCode() ^ anchorMax.GetHashCode() ^ localPosition.GetHashCode() ^
                   pivot.GetHashCode() ^ scale.GetHashCode() ^ sizeDelta.GetHashCode() ^ rotation.GetHashCode() ^
                   saveRotationAsEuler.GetHashCode() ^ eulerAngles.GetHashCode();
        }
        
        public override bool Equals(object obj) { return GetHashCode() == obj.GetHashCode(); }

        public static bool operator ==(RectTransformData a, RectTransformData b)
        {
            bool aIsNull = ReferenceEquals(a, null);
            bool bIsNull = ReferenceEquals(b, null);

            if (aIsNull || bIsNull)
                return aIsNull && bIsNull;

            return a.anchoredPosition == b.anchoredPosition && a.anchorMin == b.anchorMin && a.anchorMax == b.anchorMax && a.pivot == b.pivot &&
                   a.sizeDelta == b.sizeDelta && a.localPosition == b.localPosition && a.scale == b.scale && a.Rotation.Equals(b.Rotation);
        }

        public static bool operator !=(RectTransformData a, RectTransformData b) { return !(a == b); }

        public override string ToString()
        {
            return string.Format("RectTransformData: sizeDelta {{{0}, {1}}} - anchoredPosition {{{2}, {3}}}",
                sizeDelta.x,
                sizeDelta.y,
                anchoredPosition.x,
                anchoredPosition.y);
        }
    }
}