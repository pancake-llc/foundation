using System;

namespace Pancake.SceneFlow
{
    using UnityEngine;

    [Serializable]
    public class LeaderboardElementColor
    {
        public Color colorBackground;
        public Color colorOverlay;
        public Color colorBoder;
        public Color colorHeader;
        public Color colorText;

        public LeaderboardElementColor(Color colorBackground, Color colorOverlay, Color colorBoder, Color colorHeader, Color colorText)
        {
            this.colorBackground = colorBackground;
            this.colorOverlay = colorOverlay;
            this.colorBoder = colorBoder;
            this.colorHeader = colorHeader;
            this.colorText = colorText;
        }

        public LeaderboardElementColor()
        {
            colorBackground = new Color(0.99f, 0.96f, 0.82f);
            colorOverlay = new Color(0.8f, 0.66f, 0.33f);
            colorBoder = new Color(0.99f, 0.96f, 0.82f);
            colorHeader = new Color(1f, 0.67f, 0.26f);
            colorText = new Color(0.68f, 0.3f, 0.01f);
        }
    }
}