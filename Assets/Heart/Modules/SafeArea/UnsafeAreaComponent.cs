using System;
using Pancake.Apex;
using UnityEngine;

namespace Pancake.SafeArea
{
    [EditorIcon("scriptable_area")]
    [HideMonoScript]
    public class UnsafeAreaComponent : SafeAreaBase
    {
        public enum EPosition
        {
            Top,
            Bottom,
            Left,
            Right,
        }

        [field: SerializeField] private EPosition Position { get; set; }

        protected override void UpdateRect(Rect safeArea, int width, int height)
        {
            if (safeArea.width.Approximately(width) && safeArea.height.Approximately(height))
            {
                ResetRect();
                return;
            }

            var anchorMin = Vector2.zero;
            var anchorMax = Vector2.one;

            switch (Position)
            {
                case EPosition.Top:
                    anchorMin = new Vector2(0, safeArea.height + safeArea.y) / height;
                    break;
                case EPosition.Bottom:
                    anchorMax = new Vector2(1, safeArea.y / height);
                    break;
                case EPosition.Left:
                    anchorMax = new Vector2(safeArea.x / width, 1);
                    break;
                case EPosition.Right:
                    anchorMin = new Vector2(safeArea.width + safeArea.x, 0) / width;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            RectT.anchorMin = anchorMin;
            RectT.anchorMax = anchorMax;
            RectT.anchoredPosition = Vector3.zero;
            RectT.sizeDelta = Vector2.zero;
        }
    }
}