using Sirenix.OdinInspector;

namespace Pancake.Component
{
    using System;
    using UnityEngine;
    using UnityEngine.UI;

    [RequireComponent(typeof(CanvasRenderer))]
    public class UIHealthBarManager : MaskableGraphic, ILayoutElement
    {
        int ILayoutElement.layoutPriority => 0;
        float ILayoutElement.flexibleHeight => -1;
        float ILayoutElement.flexibleWidth => -1;
        float ILayoutElement.minHeight => 0;
        float ILayoutElement.minWidth => 0;
        float ILayoutElement.preferredHeight => Screen.height;
        float ILayoutElement.preferredWidth => Screen.width;

        void ILayoutElement.CalculateLayoutInputHorizontal() { }
        void ILayoutElement.CalculateLayoutInputVertical() { }

        [SerializeField, Required] private Camera cam;
        [SerializeField] private int height = 8;
        [SerializeField] private int border = 2;
        [SerializeField] private Color colorBackground;

        private UIVertex[][] _quadsBg;
        private UIVertex[][] _quadsFg;
        private RectTransform _canvasRectTransform;

        protected override void Awake()
        {
            base.Awake();
            Init();
            raycastTarget = false;
        }

        private void Update() { SetVerticesDirty(); }

        protected override void OnPopulateMesh(VertexHelper vh)
        {
            vh.Clear();

            Init();

            if (_canvasRectTransform == null) _canvasRectTransform = canvas.GetComponent<RectTransform>();
            var canvasRect = _canvasRectTransform.rect;

            var index = 0;

            foreach (var hb in HealthBar.ActivesHealthBar)
            {
                var pos = hb.CachedTransform.position + new Vector3(hb.Offset.x, hb.Offset.y, 0);

                DrawHealthBarWorld(vh,
                    cam,
                    canvasRect,
                    index,
                    pos,
                    hb.Percentage,
                    hb.Width,
                    hb.Color,
                    hb.Opacity);

                ++index;
            }
        }

        private void DrawHealthBarWorld(VertexHelper vh, Camera camera, Rect r, int index, Vector3 world, float fill, float width, Color friendly, float alpha)
        {
            DrawHealthBarScreen(vh,
                r,
                index,
                camera.WorldToViewportPoint(world),
                fill,
                width,
                friendly,
                alpha);
        }

        private void DrawHealthBarScreen(VertexHelper vh, Rect r, int index, Vector2 screen, float fill, float width, Color color, float alpha)
        {
            var bg = colorBackground;
            bg.a = alpha;

            var fg = color;
            fg.a = alpha;

            const int botLeft = 0;
            const int botRight = 1;
            const int topRight = 2;
            const int topLeft = 3;

            screen.x = r.width * (screen.x);
            screen.y = r.height * (screen.y);

            _quadsBg[index][botLeft].color = bg;
            _quadsBg[index][botRight].color = bg;
            _quadsBg[index][topRight].color = bg;
            _quadsBg[index][topLeft].color = bg;

            _quadsFg[index][botLeft].color = fg;
            _quadsFg[index][botRight].color = fg;
            _quadsFg[index][topRight].color = fg;
            _quadsFg[index][topLeft].color = fg;

            var botLeftPos = screen + new Vector2(-width / 2, -height / 2f);
            var botRightPos = screen + new Vector2(+width / 2, -height / 2f);
            var topRightPos = screen + new Vector2(+width / 2, +height / 2f);
            var topLeftPos = screen + new Vector2(-width / 2, +height / 2f);

            _quadsBg[index][botLeft].position = botLeftPos;
            _quadsBg[index][botRight].position = botRightPos;
            _quadsBg[index][topRight].position = topRightPos;
            _quadsBg[index][topLeft].position = topLeftPos;

            vh.AddUIVertexQuad(_quadsBg[index]);

            // only add foreground if we have any fill value
            // otherwise we might get 1px errors when healthbar is empty
            if (fill > 0)
            {
                botLeftPos += new Vector2(+border, +border);
                botRightPos += new Vector2(-border, +border);
                topRightPos += new Vector2(-border, -border);
                topLeftPos += new Vector2(+border, -border);

                botRightPos.x = botLeftPos.x + ((botRightPos.x - botLeftPos.x) * fill);
                topRightPos.x = topLeftPos.x + ((topRightPos.x - topLeftPos.x) * fill);

                _quadsFg[index][botLeft].position = botLeftPos;
                _quadsFg[index][botRight].position = botRightPos;
                _quadsFg[index][topRight].position = topRightPos;
                _quadsFg[index][topLeft].position = topLeftPos;

                vh.AddUIVertexQuad(_quadsFg[index]);
            }
        }

        private void Init()
        {
            if (_quadsBg == null || _quadsFg == null || _quadsBg.Length < HealthBar.ActivesHealthBar.Count)
            {
                _quadsBg = new UIVertex[Math.Max(64, HealthBar.ActivesHealthBar.Count)][];
                _quadsFg = new UIVertex[_quadsBg.Length][];

                for (int i = 0; i < _quadsBg.Length; ++i)
                {
                    _quadsBg[i] = new UIVertex[4];
                    _quadsFg[i] = new UIVertex[4];
                }
            }
        }
    }
}