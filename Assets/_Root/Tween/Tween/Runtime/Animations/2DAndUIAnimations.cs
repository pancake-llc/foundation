using UnityEngine;
using System;

#if PANCAKE_TMP_SUPPORT
using TMPro;
#endif

#if PANCAKE_UGUI_SUPPORT
using UnityEngine.UI;
#endif

namespace Pancake.Core.Tween
{
#if PANCAKE_UGUI_SUPPORT

    [Serializable, TweenAnimation("2D and UI/Canvas Group Alpha", "Canvas Group Alpha")]
    public class TweenCanvasGroupAlpha : TweenFloat<CanvasGroup>
    {
        public override float current
        {
            get => target ? target.alpha : 1f;
            set
            {
                if (target) target.alpha = value;
            }
        }
    }

    [Serializable, TweenAnimation("2D and UI/Graphic Color", "Graphic Color")]
    public class TweenGraphicColor : TweenColor<Graphic>
    {
        public override Color current
        {
            get => target ? target.color : Color.white;
            set
            {
                if (target) target.color = value;
            }
        }
    }

    [Serializable, TweenAnimation("2D and UI/Image Fill Amount", "Image Fill Amount")]
    public class TweenImageFillAmount : TweenFloat<Image>
    {
        public override float current
        {
            get => target ? target.fillAmount : 1;
            set
            {
                if (target) target.fillAmount = value;
            }
        }
    }


    [Serializable, TweenAnimation("2D and UI/Grid Layout Group Cell Size", "Grid Layout Group Cell Size")]
    public class TweenGridLayoutGroupCellSize : TweenVector2<GridLayoutGroup>
    {
        public override Vector2 current
        {
            get => target ? target.cellSize : default;
            set
            {
                if (target) target.cellSize = value;
            }
        }
    }

    [Serializable, TweenAnimation("2D and UI/Grid Layout Group Spacing", "Grid Layout Group Spacing")]
    public class TweenGridLayoutGroupSpacing : TweenVector2<GridLayoutGroup>
    {
        public override Vector2 current
        {
            get => target ? target.spacing : default;
            set
            {
                if (target) target.spacing = value;
            }
        }
    }

#endif

    [Serializable, TweenAnimation("2D and UI/Sprite Color", "Sprite Color")]
    public class TweenSpriteColor : TweenColor<SpriteRenderer>
    {
        public override Color current
        {
            get => target ? target.color : Color.white;
            set
            {
                if (target) target.color = value;
            }
        }
    }


#if PANCAKE_TMP_SUPPORT

    [Serializable, TweenAnimation("2D and UI/Text Mesh Pro Font Size", "Text Mesh Pro Font Size")]
    public class TextMeshProFontSize : TweenFloat<TMP_Text>
    {
        public override float current
        {
            get => target ? target.fontSize : 1f;
            set
            {
                if (target) target.fontSize = value;
            }
        }
    }

#endif
} // namespace Pancake.Core.Tween