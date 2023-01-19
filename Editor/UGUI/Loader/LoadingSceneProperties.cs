using UnityEngine;

namespace Pancake.Editor
{
    public static class LoadingSceneProperties
    {
        public static Field background = new Field(new GUIContent("Background"));
        public static Field singleBackgroundSprite = new Field(new GUIContent("Sprite"));
        public static Field backgroundSprites = new Field(new GUIContent("Sprites"));
        public static Field autoChangeBackground = new Field(new GUIContent("Auto Change"));
        public static Field timeAutoChangeBg = new Field(new GUIContent("  Duration(s)"));
        public static Field backgroundFadingSpeed = new Field(new GUIContent("  Fade Speed"));
        public static Field backgroundAnimator = new Field(new GUIContent("Animator"));

        public static Field spinnerColor = new Field(new GUIContent("Color"));
        public static Field spinnerItem = new Field(new GUIContent("spinnerItem"));
        public static Field spinnerParent = new Field(new GUIContent("Root Spinner"));
        public static Field spinnerIndex = new Field(new GUIContent("spinnerIndex"));

        public static Field enableStatusLabel = new Field(new GUIContent("Enable"));
        public static Field txtStatus = new Field(new GUIContent("Text"));
        public static Field statusFont = new Field(new GUIContent("  Font"));
        public static Field statusSize = new Field(new GUIContent("  Size"));
        public static Field statusColor = new Field(new GUIContent("  Color"));
        public static Field statusSchema = new Field(new GUIContent("  Schema"));

        public static Field isHints = new Field(new GUIContent("Enable"));
        public static Field txtHints = new Field(new GUIContent("Text"));
        public static Field hintsFont = new Field(new GUIContent("  Font"));
        public static Field hintsColor = new Field(new GUIContent("  Color"));
        public static Field hintsFontSize = new Field(new GUIContent("  Size"));
        public static Field isChangeHintsWithTimer = new Field(new GUIContent("Auto Change"));
        public static Field hintsLifeTime = new Field(new GUIContent("  Duration(s)"));
        public static Field hintsCollection = new Field(new GUIContent("Data"));

        public static Field canvasGroup = new Field(new GUIContent("Root"));
        public static Field progressBar = new Field(new GUIContent("ProgressBar"));
        public static Field mainLoadingAnimator = new Field(new GUIContent("Canvas Animator"));
        public static Field virtualLoadTime = new Field(new GUIContent("  Duration(s)"));
        public static Field enableVirtualLoading = new Field(new GUIContent("Virtual Loading"));
        public static Field enableRandomBackground = new Field(new GUIContent("Enable Random"));
        public static Field fadingAnimationSpeed = new Field(new GUIContent("Fade Speed"));
        public static Field timeDelayDestroy = new Field(new GUIContent("Delay Destroy"));
        public static Field onBeginEvents = new Field(new GUIContent("onBeginEvents"));
        public static Field onFinishEvents = new Field(new GUIContent("onFinishEvents"));
    }
}