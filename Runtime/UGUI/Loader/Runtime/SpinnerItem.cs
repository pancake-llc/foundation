using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Pancake.Loader
{
    [ExecuteInEditMode]
    public class SpinnerItem : MonoBehaviour
    {
#if UNITY_EDITOR
        [Header("RESOURCES")] public List<Image> foreground = new List<Image>();
        public List<Image> background = new List<Image>();

        public void UpdateColor(Color color)
        {
            foreach (var image in foreground)
            {
                image.color = color;
            }

            foreach (var image in background)
            {
                image.color = new Color(color.r, color.g, color.b, 0.08f);
            }
        }
#endif

        public virtual void UpdateValue(float value) { }
    }
}