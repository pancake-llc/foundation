namespace Pancake.Tween
{
    using TMPro;
    using UnityEngine;

    public static partial class TweenExtension
    {
        public static ITween TweenFontSize(this TextMeshProUGUI textMeshProUGUI, float to, float duration)
        {
            return Tween.To(() => textMeshProUGUI.fontSize,
                current => textMeshProUGUI.fontSize = current,
                () => to,
                duration,
                () => textMeshProUGUI != null);
        }

        public static ITween TweenColor(this TextMeshProUGUI textMeshProUGUI, Color to, float duration)
        {
            return Tween.To(() => textMeshProUGUI.color,
                current => textMeshProUGUI.color = current,
                () => to,
                duration,
                () => textMeshProUGUI != null);
        }

        public static ITween TweenColorAlpha(this TextMeshProUGUI textMeshProUGUI, float to, float duration)
        {
            float to255 = to * 255.0f;

            return Tween.To(() => textMeshProUGUI.color.a,
                current => textMeshProUGUI.color = textMeshProUGUI.color.ChangeAlpha(current),
                () => to,
                duration,
                () => textMeshProUGUI != null);
        }

        public static ITween TweenFaceColor(this TextMeshProUGUI textMeshProUGUI, Color to, float duration)
        {
            return Tween.To(() => textMeshProUGUI.faceColor,
                current => textMeshProUGUI.faceColor = current,
                () => to,
                duration,
                () => textMeshProUGUI != null);
        }

        public static ITween TweenFaceColorAlpha(this TextMeshProUGUI textMeshProUGUI, float to, float duration)
        {
            float to255 = to * 255.0f;

            return Tween.To(() => textMeshProUGUI.faceColor.a,
                current => textMeshProUGUI.faceColor = C.ChangeAlpha(textMeshProUGUI.faceColor, current),
                () => to,
                duration,
                () => textMeshProUGUI != null);
        }

        public static ITween TweenMaxVisibleCharacters(this TextMeshProUGUI textMeshProUGUI, int to, float duration)
        {
            return Tween.To(() => textMeshProUGUI.maxVisibleCharacters,
                current => textMeshProUGUI.maxVisibleCharacters = current,
                () => to,
                duration,
                () => textMeshProUGUI != null);
        }

        public static ITween TweenCharacterSpacing(this TextMeshProUGUI textMeshProUGUI, float to, float duration)
        {
            return Tween.To(() => textMeshProUGUI.characterSpacing,
                current => textMeshProUGUI.characterSpacing = current,
                () => to,
                duration,
                () => textMeshProUGUI != null);
        }

        public static ITween TweenWordSpacing(this TextMeshProUGUI textMeshProUGUI, float to, float duration)
        {
            return Tween.To(() => textMeshProUGUI.wordSpacing,
                current => textMeshProUGUI.wordSpacing = current,
                () => to,
                duration,
                () => textMeshProUGUI != null);
        }

        public static ITween TweenLineSpacing(this TextMeshProUGUI textMeshProUGUI, float to, float duration)
        {
            return Tween.To(() => textMeshProUGUI.lineSpacing,
                current => textMeshProUGUI.lineSpacing = current,
                () => to,
                duration,
                () => textMeshProUGUI != null);
        }

        public static ITween TweenParagraphSpacingSpacing(this TextMeshProUGUI textMeshProUGUI, float to, float duration)
        {
            return Tween.To(() => textMeshProUGUI.paragraphSpacing,
                current => textMeshProUGUI.paragraphSpacing = current,
                () => to,
                duration,
                () => textMeshProUGUI != null);
        }
    }
}