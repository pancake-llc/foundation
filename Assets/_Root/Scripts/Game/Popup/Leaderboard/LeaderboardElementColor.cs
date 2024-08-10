using System;
using UnityEngine;

namespace Pancake.Game.UI
{
    [Serializable]
    public class LeaderboardElementColor
    {
        public Color colorBackground;
        public Color colorRank;
        public Color colorText;

        public LeaderboardElementColor(Color colorBackground, Color colorRank, Color colorText)
        {
            this.colorBackground = colorBackground;
            this.colorRank = colorRank;
            this.colorText = colorText;
        }

        public LeaderboardElementColor()
        {
            colorBackground = new Color(0.99f, 0.96f, 0.82f);
            colorRank = new Color(1f, 0.67f, 0.26f);
            colorText = new Color(0.68f, 0.3f, 0.01f);
        }
    }
}