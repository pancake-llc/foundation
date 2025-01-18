using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using Pancake.Common;
using Sirenix.OdinInspector;
using TMPro;
using UnityEngine.Events;

namespace Pancake.UI
{
    /// <summary>
    /// Add this bar to an object and link it to a bar (possibly the same object the script is on), and you'll be able to resize the bar object based on a current value, located between a min and max value.
    /// See the HealthBar.cs script for a use case
    /// </summary>
    [EditorIcon("icon_progressbar")]
    public class UIProgressBar : GameComponent
    {
        public Transform foregroundBar;
        public Transform delayedBarDecreasing;
        public Transform delayedBarIncreasing;

        [FoldoutGroup("Settings")] [Range(0f, 1f)] public float minimumBarFillValue;
        [FoldoutGroup("Settings")] [Range(0f, 1f)] public float maximumBarFillValue = 1f;
        [FoldoutGroup("Settings")] public bool setInitialFillValueOnStart;

        [FoldoutGroup("Settings")] [ShowIf(nameof(setInitialFillValueOnStart))] [Range(0f, 1f), Indent]
        public float initialFillValue;

        [FoldoutGroup("Settings")] public EBarDirections barDirection = EBarDirections.LeftToRight;
        [FoldoutGroup("Settings")] public EFillModes fillMode = EFillModes.LocalScale;
        [FoldoutGroup("Settings")] public ETimeMode timeMode = ETimeMode.Unscaled;
        [FoldoutGroup("Settings")] public EBarFillModes barFillMode = EBarFillModes.SpeedBased;


        [Space] [FoldoutGroup("Forceground Bar")] public bool lerpForegroundBar = true;

        [FoldoutGroup("Forceground Bar")] [ShowIf(nameof(lerpForegroundBar)), Indent, LabelText("Speed Increasing")]
        public float lerpForegroundBarSpeedIncreasing = 15f;

        [FoldoutGroup("Forceground Bar")] [ShowIf(nameof(lerpForegroundBar)), Indent, LabelText("Speed Decreasing")]
        public float lerpForegroundBarSpeedDecreasing = 15f;

        [FoldoutGroup("Forceground Bar")] [ShowIf(nameof(lerpForegroundBar)), Indent, LabelText("Duration Increasing")]
        public float lerpForegroundBarDurationIncreasing = 0.2f;

        [FoldoutGroup("Forceground Bar")] [ShowIf(nameof(lerpForegroundBar)), Indent, LabelText("Duration Decreasing")]
        public float lerpForegroundBarDurationDecreasing = 0.2f;

        [FoldoutGroup("Forceground Bar")] [ShowIf(nameof(lerpForegroundBar)), Indent, LabelText("Curve Increasing")]
        public AnimationCurve lerpForegroundBarCurveIncreasing = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

        [FoldoutGroup("Forceground Bar")] [ShowIf(nameof(lerpForegroundBar)), Indent, LabelText("Curve Decreasing")]
        public AnimationCurve lerpForegroundBarCurveDecreasing = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);


        [Space] [FoldoutGroup("Delayed Bar Decreasing")] public float decreasingDelay = 1f;
        [FoldoutGroup("Delayed Bar Decreasing")] public bool lerpDecreasingDelayedBar = true;

        [FoldoutGroup("Delayed Bar Decreasing")] [ShowIf(nameof(lerpDecreasingDelayedBar)), Indent, LabelText("Decreasing Speed")]
        public float lerpDecreasingDelayedBarSpeed = 15f;

        [FoldoutGroup("Delayed Bar Decreasing")] [ShowIf(nameof(lerpDecreasingDelayedBar)), Indent, LabelText("Decreasing Duration")]
        public float lerpDecreasingDelayedBarDuration = 0.2f;

        [FoldoutGroup("Delayed Bar Decreasing")] [ShowIf(nameof(lerpDecreasingDelayedBar)), Indent, LabelText("Decreasing Curve")]
        public AnimationCurve lerpDecreasingDelayedBarCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);


        [Space] [FoldoutGroup("Delayed Bar Increasing")] public float increasingDelay = 1f;
        [FoldoutGroup("Delayed Bar Increasing")] public bool lerpIncreasingDelayedBar = true;

        [FoldoutGroup("Delayed Bar Increasing")] [ShowIf(nameof(lerpIncreasingDelayedBar)), Indent, LabelText("Increasing Speed")]
        public float lerpIncreasingDelayedBarSpeed = 15f;

        [FoldoutGroup("Delayed Bar Increasing")] [ShowIf(nameof(lerpIncreasingDelayedBar)), Indent, LabelText("Increasing Duration")]
        public float lerpIncreasingDelayedBarDuration = 0.2f;

        [FoldoutGroup("Delayed Bar Increasing")] [ShowIf(nameof(lerpIncreasingDelayedBar)), Indent, LabelText("Increasing Curve")]
        public AnimationCurve lerpIncreasingDelayedBarCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

        [Space] [FoldoutGroup("Bump")] public bool bumpScaleOnChange = true;
        [FoldoutGroup("Bump"), LabelText("When Increase")] public bool bumpOnIncrease;
        [FoldoutGroup("Bump"), LabelText("When Decrease")] public bool bumpOnDecrease;
        [FoldoutGroup("Bump")] public float bumpDuration = 0.2f;
        [FoldoutGroup("Bump")] public bool changeColorWhenBumping = true;

        [FoldoutGroup("Bump")] [ShowIf(nameof(changeColorWhenBumping)), Indent]
        public Color bumpColor = Color.white;

        [FoldoutGroup("Bump")] public bool storeBarColorOnPlay = true;

        [FoldoutGroup("Bump")] public AnimationCurve bumpScaleAnimationCurve = new(new Keyframe(1, 1), new Keyframe(0.3f, 1.05f), new Keyframe(1, 1));
        [FoldoutGroup("Bump")] public AnimationCurve bumpColorAnimationCurve = new(new Keyframe(0, 0), new Keyframe(0.3f, 1f), new Keyframe(1, 0));
        [FoldoutGroup("Bump")] public bool applyBumpIntensityMultiplier;

        [FoldoutGroup("Bump")] [ShowIf(nameof(applyBumpIntensityMultiplier)), Indent]
        public AnimationCurve bumpIntensityMultiplier = new(new Keyframe(-1, 1), new Keyframe(1, 1));

        public virtual bool Bumping { get; protected set; }


        [Space] [FoldoutGroup("Event")] public UnityEvent onBump;
        [FoldoutGroup("Event")] public UnityEvent<float> onBumpIntensity;
        [FoldoutGroup("Event")] public UnityEvent onBarMovementDecreasingStart;
        [FoldoutGroup("Event")] public UnityEvent onBarMovementDecreasingStop;
        [FoldoutGroup("Event")] public UnityEvent onBarMovementIncreasingStart;
        [FoldoutGroup("Event")] public UnityEvent onBarMovementIncreasingStop;

        [Space] [FoldoutGroup("Text")] public TMP_Text percentageText;
        [FoldoutGroup("Text")] public string textPrefix;
        [FoldoutGroup("Text")] public string textSuffix;
        [FoldoutGroup("Text")] public float textValueMultiplier = 1f;
        [FoldoutGroup("Text")] public string textFormat = "{000}";
        [FoldoutGroup("Text")] public bool displayTotal;
        [FoldoutGroup("Text")] [ShowIf(nameof(displayTotal))] public string totalSeparator = " / ";


        [Space] [FoldoutGroup("Debug", expanded: true)] [Range(0f, 1f)]
        public float debugNewTargetValue;

        [Space] [FoldoutGroup("Debug", expanded: true)] [Range(0f, 1f), ReadOnly]
        public float barProgress;

        [FoldoutGroup("Debug", expanded: true)] [Range(0f, 1f), ReadOnly]
        public float barTarget;

        [FoldoutGroup("Debug", expanded: true)] [Range(0f, 1f), ReadOnly]
        public float delayBarIncreaseProgress;

        [FoldoutGroup("Debug", expanded: true)] [Range(0f, 1f), ReadOnly]
        public float delayBarDecreaseProgress;

        protected bool initialized;
        protected Vector2 initialBarSize;
        protected Color initialColor;
        protected Vector3 initialScale;
        protected Image foregroundImage;
        protected Image delayedDecreasingImage;
        protected Image delayedIncreasingImage;
        protected Vector3 targetLocalScale = Vector3.one;
        protected float newPercent;
        protected float percentLastTimeBarWasUpdated;
        protected float lastUpdateTimestamp;
        protected float time;
        protected float deltaTime;
        protected int direction;
        protected Coroutine coroutine;
        protected bool coroutineShouldRun;
        protected bool isDelayedBarIncreasingNotNull;
        protected bool isDelayedBarDecreasingNotNull;
        protected bool actualUpdate;
        protected Vector2 anchorVector;
        protected float currentDelayedBarDecreasingProgress;
        protected float currentdelayedBarIncreasingProgress;
        protected EProgressBarStates currentState = EProgressBarStates.Idle;
        protected string updatedText;
        protected string totalText;
        protected bool isForegroundBarNotNull;
        protected bool isForegroundImageNotNull;
        protected bool isPercentageTextNotNull;

        #region PUBLIC_API

        /// <summary>
        /// Updates the bar's values, using a normalized value
        /// </summary>
        /// <param name="normalizedValue"></param>
        public virtual void UpdateBar01(float normalizedValue) { UpdateBar(normalizedValue.Clamp01(), 0f, 1f); }

        /// <summary>
        /// Updates the bar's values based on the specified parameters
        /// </summary>
        /// <param name="currentValue">Current value.</param>
        /// <param name="minValue">Minimum value.</param>
        /// <param name="maxValue">Max value.</param>
        public virtual void UpdateBar(float currentValue, float minValue, float maxValue)
        {
            if (!initialized)
            {
                Initialization();
            }

            if (storeBarColorOnPlay)
            {
                StoreInitialColor();
            }

            if (!gameObject.activeInHierarchy)
            {
                gameObject.SetActive(true);
            }

            newPercent = Math.Remap(currentValue,
                minValue,
                maxValue,
                minimumBarFillValue,
                maximumBarFillValue);

            actualUpdate = !barTarget.Approximately(newPercent);

            if (!actualUpdate)
            {
                return;
            }

            if (currentState != EProgressBarStates.Idle)
            {
                if (currentState == EProgressBarStates.Decreasing || currentState == EProgressBarStates.InDecreasingDelay)
                {
                    if (newPercent >= barTarget)
                    {
                        StopCoroutine(coroutine);
                        SetBar01(barTarget);
                    }
                }

                if (currentState == EProgressBarStates.Increasing || currentState == EProgressBarStates.InIncreasingDelay)
                {
                    if (newPercent <= barTarget)
                    {
                        StopCoroutine(coroutine);
                        SetBar01(barTarget);
                    }
                }
            }

            percentLastTimeBarWasUpdated = barProgress;
            currentDelayedBarDecreasingProgress = delayBarDecreaseProgress;
            currentdelayedBarIncreasingProgress = delayBarIncreaseProgress;

            barTarget = newPercent;

            if (!percentLastTimeBarWasUpdated.Approximately(newPercent) && !Bumping)
            {
                Bump();
            }

            DetermineDeltaTime();
            lastUpdateTimestamp = time;

            DetermineDirection();
            if (direction < 0)
            {
                onBarMovementDecreasingStart?.Invoke();
            }
            else
            {
                onBarMovementIncreasingStart?.Invoke();
            }

            if (coroutine != null) StopCoroutine(coroutine);

            coroutineShouldRun = true;

            if (gameObject.activeInHierarchy)
            {
                coroutine = StartCoroutine(IeUpdateBars());
            }
            else
            {
                SetBar(currentValue, minValue, maxValue);
            }

            UpdateText();
        }

        /// <summary>
        /// Sets the bar value to the one specified 
        /// </summary>
        /// <param name="currentValue"></param>
        /// <param name="minValue"></param>
        /// <param name="maxValue"></param>
        public virtual void SetBar(float currentValue, float minValue, float maxValue)
        {
            float percent = Math.Remap(currentValue,
                minValue,
                maxValue,
                0f,
                1f);
            SetBar01(percent);
        }

        /// <summary>
        /// Sets the bar value to the normalized value set in parameter
        /// </summary>
        /// <param name="percent"></param>
        public virtual void SetBar01(float percent)
        {
            if (!initialized)
            {
                Initialization();
            }

            percent = Math.Remap(percent,
                0f,
                1f,
                minimumBarFillValue,
                maximumBarFillValue);
            barProgress = percent;
            delayBarDecreaseProgress = percent;
            delayBarIncreaseProgress = percent;
            barTarget = percent;
            percentLastTimeBarWasUpdated = percent;
            currentDelayedBarDecreasingProgress = delayBarDecreaseProgress;
            currentdelayedBarIncreasingProgress = delayBarIncreaseProgress;
            SetBarInternal(percent, foregroundBar, foregroundImage, initialBarSize);
            SetBarInternal(percent, delayedBarDecreasing, delayedDecreasingImage, initialBarSize);
            SetBarInternal(percent, delayedBarIncreasing, delayedIncreasingImage, initialBarSize);
            UpdateText();
            coroutineShouldRun = false;
            currentState = EProgressBarStates.Idle;
        }

        #endregion PUBLIC_API

        #region START

        protected virtual void Start()
        {
            if (!initialized)
            {
                Initialization();
            }
        }

        protected virtual void OnEnable()
        {
            if (!initialized)
            {
                return;
            }

            StoreInitialColor();
        }

        public virtual void Initialization()
        {
            barTarget = -1f;
            isForegroundBarNotNull = foregroundBar != null;
            isDelayedBarDecreasingNotNull = delayedBarDecreasing != null;
            isDelayedBarIncreasingNotNull = delayedBarIncreasing != null;
            isPercentageTextNotNull = percentageText != null;
            initialScale = transform.localScale;

            if (isForegroundBarNotNull)
            {
                foregroundImage = foregroundBar.GetComponent<Image>();
                isForegroundImageNotNull = foregroundImage != null;
                initialBarSize = foregroundImage.rectTransform.sizeDelta;
            }

            if (isDelayedBarDecreasingNotNull)
            {
                delayedDecreasingImage = delayedBarDecreasing.GetComponent<Image>();
            }

            if (isDelayedBarIncreasingNotNull)
            {
                delayedIncreasingImage = delayedBarIncreasing.GetComponent<Image>();
            }

            initialized = true;

            StoreInitialColor();

            percentLastTimeBarWasUpdated = barProgress;

            if (setInitialFillValueOnStart)
            {
                SetBar01(initialFillValue);
            }
        }

        protected virtual void StoreInitialColor()
        {
            if (!Bumping && isForegroundImageNotNull)
            {
                initialColor = foregroundImage.color;
            }
        }

        #endregion START

        #region TESTS

        [Button]
        protected virtual void DebugUpdateBar() { UpdateBar01(debugNewTargetValue); }

        [Button]
        protected virtual void DebugSetBar() { SetBar01(debugNewTargetValue); }

        [Button]
        public virtual void Plus10Percent()
        {
            float newProgress = barTarget + 0.1f;
            newProgress = newProgress.Clamp(0f, 1f);
            UpdateBar01(newProgress);
        }

        [Button]
        public virtual void Minus10Percent()
        {
            float newProgress = barTarget - 0.1f;
            newProgress = newProgress.Clamp(0f, 1f);
            UpdateBar01(newProgress);
        }

        [Button]
        public virtual void Plus20Percent()
        {
            float newProgress = barTarget + 0.2f;
            newProgress = newProgress.Clamp(0f, 1f);
            UpdateBar01(newProgress);
        }

        [Button]
        public virtual void Minus20Percent()
        {
            float newProgress = barTarget - 0.2f;
            newProgress = newProgress.Clamp(0f, 1f);
            UpdateBar01(newProgress);
        }

        #endregion TESTS

        /// <summary>
        /// Updates the text component of the progress bar
        /// </summary>
        protected virtual void UpdateText()
        {
            if (isPercentageTextNotNull) ComputeUpdatedText();

            if (isPercentageTextNotNull)
            {
                percentageText.text = updatedText;
            }
        }

        /// <summary>
        /// Computes the updated text value to display on the progress bar
        /// </summary>
        protected virtual void ComputeUpdatedText()
        {
            updatedText = textPrefix + (barTarget * textValueMultiplier).ToString(textFormat);
            if (displayTotal)
            {
                updatedText += totalSeparator + textValueMultiplier.ToString(textFormat);
            }

            updatedText += textSuffix;
        }

        /// <summary>
        /// On Update we update our bars
        /// </summary>
        protected virtual IEnumerator IeUpdateBars()
        {
            while (coroutineShouldRun)
            {
                DetermineDeltaTime();
                DetermineDirection();
                UpdateBars();
                yield return null;
            }

            currentState = EProgressBarStates.Idle;
        }

        protected virtual void DetermineDeltaTime()
        {
            deltaTime = App.DeltaTime(timeMode);
            time = App.Time(timeMode);
        }

        protected virtual void DetermineDirection() { direction = newPercent > percentLastTimeBarWasUpdated ? 1 : -1; }

        /// <summary>
        /// Updates the foreground bar's scale
        /// </summary>
        protected virtual void UpdateBars()
        {
            float newFill;
            float newFillDelayed;
            float t1, t2 = 0f;

            // if the value is decreasing
            if (direction < 0)
            {
                newFill = ComputeNewFill(lerpForegroundBar,
                    lerpForegroundBarSpeedDecreasing,
                    lerpForegroundBarDurationDecreasing,
                    lerpForegroundBarCurveDecreasing,
                    0f,
                    percentLastTimeBarWasUpdated,
                    out t1);
                SetBarInternal(newFill, foregroundBar, foregroundImage, initialBarSize);
                SetBarInternal(newFill, delayedBarIncreasing, delayedIncreasingImage, initialBarSize);

                barProgress = newFill;
                delayBarIncreaseProgress = newFill;

                currentState = EProgressBarStates.Decreasing;

                if (time - lastUpdateTimestamp > decreasingDelay)
                {
                    newFillDelayed = ComputeNewFill(lerpDecreasingDelayedBar,
                        lerpDecreasingDelayedBarSpeed,
                        lerpDecreasingDelayedBarDuration,
                        lerpDecreasingDelayedBarCurve,
                        decreasingDelay,
                        currentDelayedBarDecreasingProgress,
                        out t2);
                    SetBarInternal(newFillDelayed, delayedBarDecreasing, delayedDecreasingImage, initialBarSize);

                    delayBarDecreaseProgress = newFillDelayed;
                    currentState = EProgressBarStates.InDecreasingDelay;
                }
            }
            else // if the value is increasing
            {
                newFill = ComputeNewFill(lerpForegroundBar,
                    lerpIncreasingDelayedBarSpeed,
                    lerpIncreasingDelayedBarDuration,
                    lerpIncreasingDelayedBarCurve,
                    0f,
                    currentdelayedBarIncreasingProgress,
                    out t1);
                SetBarInternal(newFill, delayedBarIncreasing, delayedIncreasingImage, initialBarSize);

                delayBarIncreaseProgress = newFill;
                currentState = EProgressBarStates.Increasing;

                if (delayedBarIncreasing == null)
                {
                    newFill = ComputeNewFill(lerpForegroundBar,
                        lerpForegroundBarSpeedIncreasing,
                        lerpForegroundBarDurationIncreasing,
                        lerpForegroundBarCurveIncreasing,
                        0f,
                        percentLastTimeBarWasUpdated,
                        out t2);
                    SetBarInternal(newFill, delayedBarDecreasing, delayedDecreasingImage, initialBarSize);
                    SetBarInternal(newFill, foregroundBar, foregroundImage, initialBarSize);

                    barProgress = newFill;
                    delayBarDecreaseProgress = newFill;
                    currentState = EProgressBarStates.InDecreasingDelay;
                }
                else
                {
                    if (time - lastUpdateTimestamp > increasingDelay)
                    {
                        newFillDelayed = ComputeNewFill(lerpIncreasingDelayedBar,
                            lerpForegroundBarSpeedIncreasing,
                            lerpForegroundBarDurationIncreasing,
                            lerpForegroundBarCurveIncreasing,
                            increasingDelay,
                            currentDelayedBarDecreasingProgress,
                            out t2);

                        SetBarInternal(newFillDelayed, delayedBarDecreasing, delayedDecreasingImage, initialBarSize);
                        SetBarInternal(newFillDelayed, foregroundBar, foregroundImage, initialBarSize);

                        barProgress = newFillDelayed;
                        delayBarDecreaseProgress = newFillDelayed;
                        currentState = EProgressBarStates.InDecreasingDelay;
                    }
                }
            }

            if (t1 >= 1f && t2 >= 1f)
            {
                coroutineShouldRun = false;
                if (direction > 0)
                {
                    onBarMovementIncreasingStop?.Invoke();
                }
                else
                {
                    onBarMovementDecreasingStop?.Invoke();
                }
            }
        }

        protected virtual float ComputeNewFill(bool lerpBar, float barSpeed, float barDuration, AnimationCurve barCurve, float delay, float lastPercent, out float t)
        {
            float newFill;
            t = 0f;
            if (lerpBar)
            {
                float timeSpent = time - lastUpdateTimestamp - delay;
                float speed = barSpeed;
                if (speed == 0f)
                {
                    speed = 1f;
                }

                float duration = barFillMode == EBarFillModes.FixedDuration ? barDuration : (newPercent - lastPercent).Abs() / speed;

                float delta = Math.Remap(timeSpent,
                    0f,
                    duration,
                    0f,
                    1f);
                delta = delta.Clamp(0f, 1f);
                t = delta;
                if (t < 1f)
                {
                    delta = barCurve.Evaluate(delta);
                    newFill = Mathf.LerpUnclamped(lastPercent, newPercent, delta);
                }
                else
                {
                    newFill = newPercent;
                }
            }
            else
            {
                newFill = newPercent;
            }

            newFill = newFill.Clamp(0f, 1f);

            return newFill;
        }

        protected virtual void SetBarInternal(float newAmount, Transform bar, Image image, Vector2 initialSize)
        {
            if (bar == null)
            {
                return;
            }

            switch (fillMode)
            {
                case EFillModes.LocalScale:
                    targetLocalScale = Vector3.one;
                    switch (barDirection)
                    {
                        case EBarDirections.LeftToRight:
                            targetLocalScale.x = newAmount;
                            break;
                        case EBarDirections.RightToLeft:
                            targetLocalScale.x = 1f - newAmount;
                            break;
                        case EBarDirections.DownToUp:
                            targetLocalScale.y = newAmount;
                            break;
                        case EBarDirections.UpToDown:
                            targetLocalScale.y = 1f - newAmount;
                            break;
                    }

                    bar.localScale = targetLocalScale;
                    break;

                case EFillModes.Width:
                    if (image == null)
                    {
                        return;
                    }

                    float newSizeX = Math.Remap(newAmount,
                        0f,
                        1f,
                        0,
                        initialSize.x);
                    image.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, newSizeX);
                    break;

                case EFillModes.Height:
                    if (image == null)
                    {
                        return;
                    }

                    float newSizeY = Math.Remap(newAmount,
                        0f,
                        1f,
                        0,
                        initialSize.y);
                    image.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, newSizeY);
                    break;

                case EFillModes.FillAmount:
                    if (image == null)
                    {
                        return;
                    }

                    image.fillAmount = newAmount;
                    break;
                case EFillModes.Anchor:
                    if (image == null)
                    {
                        return;
                    }

                    switch (barDirection)
                    {
                        case EBarDirections.LeftToRight:
                            anchorVector.x = 0f;
                            anchorVector.y = 0f;
                            image.rectTransform.anchorMin = anchorVector;
                            anchorVector.x = newAmount;
                            anchorVector.y = 1f;
                            image.rectTransform.anchorMax = anchorVector;
                            break;
                        case EBarDirections.RightToLeft:
                            anchorVector.x = newAmount;
                            anchorVector.y = 0f;
                            image.rectTransform.anchorMin = anchorVector;
                            anchorVector.x = 1f;
                            anchorVector.y = 1f;
                            image.rectTransform.anchorMax = anchorVector;
                            break;
                        case EBarDirections.DownToUp:
                            anchorVector.x = 0f;
                            anchorVector.y = 0f;
                            image.rectTransform.anchorMin = anchorVector;
                            anchorVector.x = 1f;
                            anchorVector.y = newAmount;
                            image.rectTransform.anchorMax = anchorVector;
                            break;
                        case EBarDirections.UpToDown:
                            anchorVector.x = 0f;
                            anchorVector.y = newAmount;
                            image.rectTransform.anchorMin = anchorVector;
                            anchorVector.x = 1f;
                            anchorVector.y = 1f;
                            image.rectTransform.anchorMax = anchorVector;
                            break;
                    }

                    break;
            }
        }

        #region Bump

        /// <summary>
        /// Triggers a camera bump
        /// </summary>
        public virtual void Bump()
        {
            float delta = newPercent - percentLastTimeBarWasUpdated;
            float intensityMultiplier = bumpIntensityMultiplier.Evaluate(delta);

            var shouldBump = false;

            if (!initialized)
            {
                return;
            }

            DetermineDirection();

            if (bumpOnIncrease && direction > 0)
            {
                shouldBump = true;
            }

            if (bumpOnDecrease && direction < 0)
            {
                shouldBump = true;
            }

            if (bumpScaleOnChange)
            {
                shouldBump = true;
            }

            if (!shouldBump)
            {
                return;
            }

            if (gameObject.activeInHierarchy)
            {
                StartCoroutine(BumpCoroutine(intensityMultiplier));
            }

            onBump?.Invoke();
            onBumpIntensity?.Invoke(applyBumpIntensityMultiplier ? intensityMultiplier : 1f);
        }

        /// <summary>
        /// A coroutine that (usually quickly) changes the scale of the bar 
        /// </summary>
        /// <returns>The coroutine.</returns>
        protected virtual IEnumerator BumpCoroutine(float intensityMultiplier)
        {
            var journey = 0f;

            Bumping = true;

            while (journey <= bumpDuration)
            {
                journey += deltaTime;
                float percent = (journey / bumpDuration).Clamp01();

                float curvePercent = bumpScaleAnimationCurve.Evaluate(percent);

                if (applyBumpIntensityMultiplier)
                {
                    float multiplier = Mathf.Abs(1f - curvePercent) * intensityMultiplier;
                    curvePercent = 1 + multiplier;
                }

                float colorCurvePercent = bumpColorAnimationCurve.Evaluate(percent);
                transform.localScale = curvePercent * initialScale;

                if (changeColorWhenBumping && isForegroundImageNotNull)
                {
                    foregroundImage.color = Color.Lerp(initialColor, bumpColor, colorCurvePercent);
                }

                yield return null;
            }

            if (changeColorWhenBumping && isForegroundImageNotNull)
            {
                foregroundImage.color = initialColor;
            }

            Bumping = false;
            yield return null;
        }

        #endregion Bump

        #region ShowHide

        /// <summary>
        /// A simple method you can call to show the bar (set active true)
        /// </summary>
        public virtual void ShowBar() { gameObject.SetActive(true); }

        /// <summary>
        /// Hides (SetActive false) the progress bar object, after an optional delay
        /// </summary>
        /// <param name="delay"></param>
        public virtual void HideBar(float delay)
        {
            if (delay <= 0)
            {
                gameObject.SetActive(false);
            }
            else if (gameObject.activeInHierarchy)
            {
                _ = HideBarWithDelay(delay);
            }
        }

        /// <summary>
        /// An internal coroutine used to handle the disabling of the progress bar after a delay
        /// </summary>
        /// <param name="delay"></param>
        /// <returns></returns>
        protected virtual async Awaitable HideBarWithDelay(float delay)
        {
            await Awaitable.WaitForSecondsAsync(delay);
            gameObject.SetActive(false);
        }

        #endregion ShowHide
    }
}