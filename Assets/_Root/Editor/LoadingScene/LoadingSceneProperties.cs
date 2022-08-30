using UnityEngine;

namespace Pancake.Editor
{
    public static class LoadingSceneProperties
    {
        public static Property background = new Property(new GUIContent("Background"));
        public static Property singleBackgroundSprite = new Property(new GUIContent("Sprite"));
        public static Property backgroundSprites = new Property(new GUIContent("Sprites"));
        public static Property autoChangeBackground = new Property(new GUIContent("Auto Change"));
        public static Property timeAutoChangeBg = new Property(new GUIContent("  Duration(s)"));
        public static Property backgroundFadingSpeed = new Property(new GUIContent("  Fade Speed"));
        public static Property backgroundAnimator = new Property(new GUIContent("Animator"));

        public static Property enablePressAnyKey = new Property(new GUIContent("Enable"));
        public static Property pakSize = new Property(new GUIContent("  Text Size"));
        public static Property pakFont = new Property(new GUIContent("  Text Font"));
        public static Property pakColor = new Property(new GUIContent("  Text Color"));
        public static Property pakText = new Property(new GUIContent("pakText"));
        public static Property txtPak = new Property(new GUIContent("Text"));
        public static Property txtCountdownPak = new Property(new GUIContent("Text Cooldown"));
        public static Property sliderCountdownPak = new Property(new GUIContent("Slider"));
        public static Property keyCode = new Property(new GUIContent("  Key Code"));
        public static Property useSpecificKey = new Property(new GUIContent("Use Specific Key"));
        public static Property pakCountdownTimer = new Property(new GUIContent("Countdown(s)"));

        public static Property spinnerColor = new Property(new GUIContent("Color"));
        public static Property spinnerItem = new Property(new GUIContent("spinnerItem"));
        public static Property spinnerParent = new Property(new GUIContent("Root Spinner"));
        public static Property spinnerIndex = new Property(new GUIContent("spinnerIndex"));

        public static Property enableStatusLabel = new Property(new GUIContent("Enable"));
        public static Property txtStatus = new Property(new GUIContent("Text"));
        public static Property statusFont = new Property(new GUIContent("  Font"));
        public static Property statusSize = new Property(new GUIContent("  Size"));
        public static Property statusColor = new Property(new GUIContent("  Color"));
        public static Property statusSchema = new Property(new GUIContent("  Schema"));

        public static Property isHints = new Property(new GUIContent("Enable"));
        public static Property txtHints = new Property(new GUIContent("Text"));
        public static Property hintsFont = new Property(new GUIContent("  Font"));
        public static Property hintsColor = new Property(new GUIContent("  Color"));
        public static Property hintsFontSize = new Property(new GUIContent("  Size"));
        public static Property isChangeHintsWithTimer = new Property(new GUIContent("Auto Change"));
        public static Property hintsLifeTime = new Property(new GUIContent("  Duration(s)"));
        public static Property hintsCollection = new Property(new GUIContent("Data"));

        public static Property canvasGroup = new Property(new GUIContent("Root"));
        public static Property progressBar = new Property(new GUIContent("ProgressBar"));
        public static Property mainLoadingAnimator = new Property(new GUIContent("Canvas Animator"));
        public static Property virtualLoadTime = new Property(new GUIContent("  Duration(s)"));
        public static Property enableVirtualLoading = new Property(new GUIContent("Virtual Loading"));
        public static Property enableRandomBackground = new Property(new GUIContent("Enable Random"));
        public static Property fadingAnimationSpeed = new Property(new GUIContent("Fade Speed"));
        public static Property timeDelayDestroy = new Property(new GUIContent("Delay Destroy"));
        public static Property onBeginEvents = new Property(new GUIContent("onBeginEvents"));
        public static Property onFinishEvents = new Property(new GUIContent("onFinishEvents"));
    }
}