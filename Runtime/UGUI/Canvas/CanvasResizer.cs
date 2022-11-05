using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.UI;

namespace Pancake.UI
{
    [RequireComponent(typeof(CanvasScaler))]
    [ExecuteInEditMode]
    [PublicAPI]
    public class CanvasResizer : MonoBehaviour
    {
        /// <summary>
        /// Buffer for resolution calculation
        /// </summary>
        /// <remarks>Set up buffers to account for cases where either side is close to the minimum resolution</remarks>
        private const float CALCULATE_RESOLUTION_BUFFER = 10.0f;
        
        private static readonly Vector2 DefaultStandardResolution = new Vector2(2272.0f, 1536.0f);
        private static readonly Vector2 DefaultMinimumResolution = new Vector2(2048.0f, 1278.0f);
        private static readonly Vector2 DefaultMaximumResolution = new Vector2(2768.0f, 1536.0f);

        private CanvasScaler _canvasScaler;

        private CanvasScaler CanvasScaler
        {
            get
            {
                if (_canvasScaler == default(CanvasScaler))
                {
                    _canvasScaler = gameObject.GetComponent<CanvasScaler>();
                }

                return _canvasScaler;
            }
        }

        /// <summary>
        /// Standard definition entity
        /// </summary>
        [SerializeField] private Vector2 standardResolution = DefaultStandardResolution;

        /// <summary>
        /// standard resolution
        /// </summary>
        private Vector2 StandardResolution => standardResolution;

        /// <summary>
        /// minimum resolution entity
        /// </summary>
        [SerializeField] private Vector2 minimumResolution = DefaultMinimumResolution;

        /// <summary>
        /// Minimum resolution
        /// </summary>
        private Vector2 MinimumResolution => minimumResolution;

        /// <summary>
        /// full resolution entity
        /// </summary>
        [SerializeField] private Vector2 maximumResolution = DefaultMaximumResolution;

        /// <summary>
        /// Maximum resolution
        /// </summary>
        private Vector2 MaximumResolution => maximumResolution;

        /// <summary>
        /// resize
        /// </summary>
        /// <remarks>Internally, it just rewrites CanvasScaler.matchWidthOrHeight</remarks>
        public void Resize()
        {
            CanvasScaler.matchWidthOrHeight = CalculateMatchWidthOrHeight();

            var screenSize = new Vector2(Screen.width, Screen.height);
            var referenceResolution = StandardResolution;
            // Size when expanded based on standard resolution
            var extendedResolution = new Vector2(StandardResolution.y * Screen.width / Screen.height, StandardResolution.x * Screen.height / Screen.width);
            float matchWidthOrHeight;
            if (screenSize.y / screenSize.x > StandardResolution.y / StandardResolution.x)
            {
                /* 参考解像度よりも大きい場合 */
                /* iPad や正方形の端末 (あるのか！？) など */
                if (extendedResolution.x + CALCULATE_RESOLUTION_BUFFER > MinimumResolution.x)
                {
                    /* 最小解像度XよりもXが大きい場合 */
                    /* iPad など */
                    // 縦併せにする
                    matchWidthOrHeight = 1.0f;
                }
                else
                {
                    /* 最小解像度XよりもXが小さくなる場合 */
                    /* (無いとは思うけど) 正方形端末や Landscape 状態で縦長になる端末など */
                    if (extendedResolution.y > MaximumResolution.y)
                    {
                        /* 最大解像度YよりもYが大きい場合 */
                        /* Landscape 状態で縦長になる端末など */
                        // 縦併せにする
                        matchWidthOrHeight = 1.0f;
                        // 縦を最大解像度Yに制限する
                        referenceResolution = new Vector2(MaximumResolution.x, MaximumResolution.y);
                    }
                    else
                    {
                        /* 最大解像度YよりもYが小さい場合 */
                        /* 正方形な端末など */
                        // 横併せにする
                        matchWidthOrHeight = 0.0f;
                        // 縦を計算した解像度まで引き延ばす
                        referenceResolution = new Vector2(referenceResolution.x, extendedResolution.y);
                    }
                }
            }
            else
            {
                /* 参考参考解像度よりも小さい場合 */
                /* iPhone 4s, iPhone 5, iPhone X, Android など */
                if (extendedResolution.y + CALCULATE_RESOLUTION_BUFFER > MinimumResolution.y)
                {
                    /* 最小参考解像度YよりもYが大きい場合 */
                    /* iPhone 4s, iPhone 5, Android など */
                    matchWidthOrHeight = 0.0f;
                }
                else
                {
                    /* 最小解像度YよりもYが小さくなる場合 */
                    /* iPhone X など */
                    if (extendedResolution.x > MaximumResolution.x)
                    {
                        /* 最大解像度XよりもXが大きい場合 */
                        /* 相当細長い端末 (1 : 3 とか) など */
                        // 横併せにする
                        matchWidthOrHeight = 0.0f;
                        // 横を最大解像度Xに制限する
                        referenceResolution = new Vector2(MaximumResolution.x, MaximumResolution.y);
                    }
                    else
                    {
                        /* 最大解像度XよりもXが小さい場合 */
                        /* iPhone X など */
                        // 縦併せにする
                        matchWidthOrHeight = 1.0f;
                        // 横を計算した解像度まで引き延ばす
                        referenceResolution = new Vector2(extendedResolution.x, referenceResolution.y);
                    }
                }
            }

            CanvasScaler.referenceResolution = referenceResolution;
            CanvasScaler.matchWidthOrHeight = matchWidthOrHeight;
        }

        private void Start() { Resize(); }

#if UNITY_EDITOR

        private Vector2Int CurrentScreenResolution { get; set; } = Vector2Int.zero;

        private void Update()
        {
            if (CurrentScreenResolution.x == Screen.width && CurrentScreenResolution.y == Screen.height)
            {
                return;
            }

            CurrentScreenResolution = new Vector2Int(Screen.width, Screen.height);
            Resize();
        }
#endif

        /// <summary>
        /// Calculate whether to match Width or Height
        /// </summary>
        /// <returns>computation result</returns>
        [SuppressMessage("ReSharper", "RedundantCast")]
        private float CalculateMatchWidthOrHeight()
        {
            if ((float) Screen.height / (float) Screen.width < CanvasScaler.referenceResolution.y / CanvasScaler.referenceResolution.x)
            {
                return 0.0f;
            }

            return 1.0f;
        }

        [ContextMenu("Convert To Portrait")]
        private void ToPortrait()
        {
            var temp = standardResolution;
            if (temp.x > temp.y) standardResolution = new Vector2(temp.y, temp.x);
            
            var temp2 = minimumResolution;
            if (temp2.x > temp2.y) minimumResolution = new Vector2(temp2.y, temp2.x);
            
            var temp3 = maximumResolution;
            if (temp3.x > temp3.y) maximumResolution = new Vector2(temp3.y, temp3.x);
        }
        
        [ContextMenu("Convert To Landscape")]
        private void ToLandscape()
        {
            var temp = standardResolution;
            if (temp.x < temp.y) standardResolution = new Vector2(temp.y, temp.x);
            
            var temp2 = minimumResolution;
            if (temp2.x < temp2.y) minimumResolution = new Vector2(temp2.y, temp2.x);
            
            var temp3 = maximumResolution;
            if (temp3.x < temp3.y) maximumResolution = new Vector2(temp3.y, temp3.x);
        }

#if UNITY_EDITOR

        /// <summary>
        /// Color list of gizmos to draw
        /// </summary>
        /// <remarks>
        /// Cycle through this content
        /// </remarks>
        private static readonly List<Color> ColorList = new List<Color>()
        {
            new Color(0.16f, 0.66f, 0.52f),
            new Color(0.82f, 0.12f, 0.23f),
            new Color(0.98f, 0.87f, 0.02f),
            new Color(0.58f, 0.78f, 0.85f),
            new Color(0.69f, 0.4f, 0.93f)
        };

        /// <summary>
        /// List of screen ratios
        /// </summary>
        private static readonly List<Vector2> SizeList = new List<Vector2>()
        {
            new Vector2(812.0f, 375.0f), // iPhone X and later iOS handsets
            new Vector2(16.0f, 9.0f), // iPhone 5 or newer iOS handset
            new Vector2(16.0f, 10.0f), // Major Android handsets/tablets
            new Vector2(3.0f, 2.0f), // iOS handsets up to iPhone 4S
            new Vector2(4.0f, 3.0f), // iOS tablet such as iPad
        };

        /// <summary>
        /// Unity lifecycle: OnGUI
        /// </summary>
        /// <remarks>Run resizing process when editing editor</remarks>
        private void OnGUI() { Resize(); }

        /// <summary>
        /// draw the gizmo
        /// </summary>
        /// <remarks>
        /// I thought about using the [DrawGizmo()] attribute as a dedicated editor extension class, but
        /// Reconsidering that the role is different, it has reached the current form.
        /// </remarks>
        /// <remarks>Due to Unity's specifications, the gizmo must draw every frame, but it might be okay to cache the calculation results. </remarks>
        private void OnDrawGizmos()
        {
            var originalColor = Gizmos.color;
            var canvasResizerTransform = gameObject.transform;
            bool landscape = !(CanvasScaler.referenceResolution.x < CanvasScaler.referenceResolution.y);
            
            for (var i = 0; i < SizeList.Count; i++)
            {
                Gizmos.color = ColorList[i % ColorList.Count];
                var size = SizeList[i];
                if (!landscape)
                {
                    size = new Vector2(size.y, size.x);
                }

                var extended = new Vector2(StandardResolution.y * size.x / size.y, StandardResolution.x * size.y / size.x);
                float unitSize;
                if (size.y / size.x > StandardResolution.y / StandardResolution.x)
                {
                    if (extended.x + CALCULATE_RESOLUTION_BUFFER > MinimumResolution.x)
                    {
                        unitSize = StandardResolution.y / size.y;
                    }
                    else
                    {
                        if (extended.y > MaximumResolution.y)
                        {
                            unitSize = MaximumResolution.y / size.y;
                        }
                        else
                        {
                            unitSize = StandardResolution.x / size.x;
                        }
                    }
                }
                else
                {
                    if (extended.y + CALCULATE_RESOLUTION_BUFFER > MinimumResolution.y)
                    {
                        unitSize = StandardResolution.x / size.x;
                    }
                    else
                    {
                        if (extended.x > MaximumResolution.x)
                        {
                            unitSize = MaximumResolution.x / size.x;
                        }
                        else
                        {
                            unitSize = StandardResolution.y / size.y;
                        }
                    }
                }

                Gizmos.DrawWireCube(canvasResizerTransform.TransformPoint(canvasResizerTransform.localPosition),
                    // ReSharper disable once Unity.InefficientPropertyAccess
                    new Vector3(size.x * unitSize * canvasResizerTransform.localScale.x, size.y * unitSize * canvasResizerTransform.localScale.y, 0.0f));
            }

            Gizmos.color = originalColor;
        }

#endif
    }
}