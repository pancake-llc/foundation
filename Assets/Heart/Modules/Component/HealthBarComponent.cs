using UnityEngine;
using System.Collections;
using Pancake.Common;
using Pancake.UI;
using Sirenix.OdinInspector;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Pancake.Component
{
    /// <summary>
    /// Add this component to an object, and it will show a healthbar above it,
    /// You can either use a prefab for it, or have the component draw one at the start
    /// </summary>
    public class HealthBarComponent : GameComponent
    {
        public enum HealthBarTypes
        {
            Prefab,
            Drawn,
            Existing
        }

        public HealthBarTypes healthBarType = HealthBarTypes.Drawn;

        [ShowIf(nameof(healthBarType), HealthBarTypes.Prefab), LabelText("Prefab"), Indent]
        public UIProgressBar healthBarPrefab;

        [ShowIf(nameof(healthBarType), HealthBarTypes.Existing), LabelText("Progress Bar"), Indent]
        public UIProgressBar targetProgressBar;

        // defines whether the bar will work on scaled or unscaled time (whether it'll keep moving if time is slowed down for example)
        public ETimeMode timeMode = ETimeMode.Unscaled;


        [ShowIf(nameof(healthBarType), HealthBarTypes.Drawn), FoldoutGroup("Draw Settings")]
        public Vector2 size = new(1f, 0.2f);

        // if the healthbar is drawn, the padding to apply to the foreground, in world units
        [ShowIf(nameof(healthBarType), HealthBarTypes.Drawn), FoldoutGroup("Draw Settings")]
        public Vector2 backgroundPadding = new(0.01f, 0.01f);

        // the rotation to apply to the MMHealthBarContainer when drawing it
        [ShowIf(nameof(healthBarType), HealthBarTypes.Drawn), FoldoutGroup("Draw Settings")]
        public Vector3 initialRotationAngles;

        // if the healthbar is drawn, the color of its foreground
        [ShowIf(nameof(healthBarType), HealthBarTypes.Drawn), FoldoutGroup("Draw Settings")]
        public Gradient foregroundColor = new()
        {
            colorKeys = new GradientColorKey[] {new(new Color(1f, 0.09f, 0f), 0), new(new Color(1f, 0.09f, 0f), 1f)},
            alphaKeys = new GradientAlphaKey[] {new(1, 0), new(1, 1)}
        };

        // if the healthbar is drawn, the color of its delayed bar
        [ShowIf(nameof(healthBarType), HealthBarTypes.Drawn), FoldoutGroup("Draw Settings")]
        public Gradient delayedColor = new()
        {
            colorKeys = new GradientColorKey[] {new(new Color(1f, 0.65f, 0f), 0), new(new Color(1f, 0.65f, 0f), 1f)},
            alphaKeys = new GradientAlphaKey[] {new(1, 0), new(1, 1)}
        };

        // if the healthbar is drawn, the color of its border
        [ShowIf(nameof(healthBarType), HealthBarTypes.Drawn), FoldoutGroup("Draw Settings")]
        public Gradient borderColor = new()
        {
            colorKeys = new GradientColorKey[] {new(new Color(0.98f, 0.92f, 0.84f), 0), new(new Color(0.98f, 0.92f, 0.84f), 1f)},
            alphaKeys = new GradientAlphaKey[] {new(1, 0), new(1, 1)}
        };

        // if the healthbar is drawn, the color of its background
        [ShowIf(nameof(healthBarType), HealthBarTypes.Drawn), FoldoutGroup("Draw Settings")]
        public Gradient backgroundColor = new()
        {
            colorKeys = new GradientColorKey[] {new(Color.black, 0), new(Color.black, 1f)}, alphaKeys = new GradientAlphaKey[] {new(1, 0), new(1, 1)}
        };

        [ShowIf(nameof(healthBarType), HealthBarTypes.Drawn), FoldoutGroup("Draw Settings")]
        public string sortingLayerName = "UI";

        [ShowIf(nameof(healthBarType), HealthBarTypes.Drawn), FoldoutGroup("Draw Settings")]
        public float delay = 0.5f;

        [ShowIf(nameof(healthBarType), HealthBarTypes.Drawn), FoldoutGroup("Draw Settings")]
        public bool lerpFrontBar = true;

        [ShowIf(nameof(healthBarType), HealthBarTypes.Drawn), FoldoutGroup("Draw Settings")]
        public float lerpFrontBarSpeed = 15f;

        [ShowIf(nameof(healthBarType), HealthBarTypes.Drawn), FoldoutGroup("Draw Settings")]
        public bool lerpDelayedBar = true;

        [ShowIf(nameof(healthBarType), HealthBarTypes.Drawn), FoldoutGroup("Draw Settings")]
        public float lerpDelayedBarSpeed = 15f;

        [ShowIf(nameof(healthBarType), HealthBarTypes.Drawn), FoldoutGroup("Draw Settings")]
        public bool bumpScaleOnChange = true;

        [ShowIf(nameof(healthBarType), HealthBarTypes.Drawn), FoldoutGroup("Draw Settings")]
        public float bumpDuration = 0.2f;

        [ShowIf(nameof(healthBarType), HealthBarTypes.Drawn), FoldoutGroup("Draw Settings")]
        public AnimationCurve bumpAnimationCurve = AnimationCurve.Constant(0, 1, 1);

        [ShowIf(nameof(healthBarType), HealthBarTypes.Drawn), FoldoutGroup("Draw Settings")]
        public EGameLoopType followTargetMode = EGameLoopType.LateUpdate;

        [ShowIf(nameof(healthBarType), HealthBarTypes.Drawn), FoldoutGroup("Draw Settings")]
        public bool followRotation;

        [ShowIf(nameof(healthBarType), HealthBarTypes.Drawn), FoldoutGroup("Draw Settings")]
        public bool followScale = true;

        [ShowIf(nameof(healthBarType), HealthBarTypes.Drawn), FoldoutGroup("Draw Settings")]
        public bool nestDrawnHealthBar;

        /// if this is true, a Billboard component will be added to the progress bar to make sure it always looks towards the camera
        [ShowIf(nameof(healthBarType), HealthBarTypes.Drawn), FoldoutGroup("Draw Settings")]
        public bool billboard;

        [Header("Death")]
        // a gameobject (usually a particle system) to instantiate when the healthbar reaches zero
        public GameObject instantiatedOnDeath;

        [Header("Offset")]
        // the offset to apply to the healthbar compared to the object's center
        public Vector3 healthBarOffset = new(0f, 1f, 0f);

        [Header("Display")]
        // whether or not the bar should be permanently displayed
        public bool alwaysVisible = true;

        /// the duration (in seconds) during which to display the bar
        [HideIf(nameof(alwaysVisible)), Indent] public float displayDurationOnHit = 1f;

        /// if this is set to true the bar will hide itself when it reaches zero
        public bool hideBarAtZero = true;

        /// the delay (in seconds) after which to hide the bar
        [ShowIf(nameof(hideBarAtZero)), Indent] public float hideBarAtZeroDelay = 1f;

        [Header("Test")]
        // a test value to use when pressing the TestUpdateHealth button
        public float testMinHealth;

        /// a test value to use when pressing the TestUpdateHealth button
        public float testMaxHealth = 100f;

        /// a test value to use when pressing the TestUpdateHealth button
        public float testCurrentHealth = 25f;


        protected UIProgressBar progressBar;
        protected FollowTargetComponent followTarget;
        protected float lastShowTimestamp;
        protected bool showBar;
        protected Image backgroundImage;
        protected Image borderImage;
        protected Image foregroundImage;
        protected Image delayedImage;
        protected bool finalHideStarted;

        /// <summary>
        /// On Start, creates or sets the health bar up
        /// </summary>
        protected virtual void Awake() { Initialization(); }

        /// <summary>
        /// On enable, initializes the bar again
        /// </summary>
        protected void OnEnable()
        {
            finalHideStarted = false;

            SetInitialActiveState();
        }

        /// <summary>
        /// Forces the bar into its initial active state (hiding it if AlwaysVisible is false)
        /// </summary>
        public virtual void SetInitialActiveState()
        {
            if (!alwaysVisible && progressBar != null) ShowBar(false);
        }

        /// <summary>
        /// Shows or hides the bar by changing its object's active state
        /// </summary>
        /// <param name="state"></param>
        public virtual void ShowBar(bool state) { progressBar.gameObject.SetActive(state); }

        /// <summary>
        /// Whether the bar is currently active
        /// </summary>
        /// <returns></returns>
        public virtual bool BarIsShown() { return progressBar.gameObject.activeInHierarchy; }

        /// <summary>
        /// Initializes the bar (handles visibility, parenting, initial value
        /// </summary>
        public virtual void Initialization()
        {
            finalHideStarted = false;

            if (progressBar != null)
            {
                ShowBar(alwaysVisible);
                return;
            }

            switch (healthBarType)
            {
                case HealthBarTypes.Prefab:
                    if (healthBarPrefab == null)
                    {
                        Debug.LogWarning(name + " : the HealthBar has no prefab associated to it, nothing will be displayed.");
                        return;
                    }

                    progressBar = Instantiate(healthBarPrefab, transform.position + healthBarOffset, transform.rotation);
                    SceneManager.MoveGameObjectToScene(progressBar.gameObject, gameObject.scene);
                    progressBar.transform.SetParent(transform);
                    progressBar.gameObject.name = "HealthBar";
                    break;
                case HealthBarTypes.Drawn:
                    DrawHealthBar();
                    UpdateDrawnColors();
                    break;
                case HealthBarTypes.Existing:
                    progressBar = targetProgressBar;
                    break;
            }

            if (!alwaysVisible) ShowBar(false);

            if (progressBar != null) progressBar.SetBar(100f, 0f, 100f);
        }


        /// <summary>
        /// Draws the health bar.
        /// </summary>
        protected virtual void DrawHealthBar()
        {
            var newGameObject = new GameObject();
            SceneManager.MoveGameObjectToScene(newGameObject, gameObject.scene);
            newGameObject.name = "HealthBar|" + gameObject.name;

            if (nestDrawnHealthBar) newGameObject.transform.SetParent(transform);

            progressBar = newGameObject.AddComponent<UIProgressBar>();

            followTarget = newGameObject.AddComponent<FollowTargetComponent>();
            followTarget.offset = healthBarOffset;
            followTarget.target = transform;
            followTarget.followRotation = followRotation;
            followTarget.followScale = followScale;
            followTarget.interpolatePosition = false;
            followTarget.interpolateRotation = false;
            followTarget.SwitchGameLoop(followTargetMode);

            var newCanvas = newGameObject.AddComponent<Canvas>();
            newCanvas.renderMode = RenderMode.WorldSpace;
            newCanvas.transform.localScale = Vector3.one;
            newCanvas.GetComponent<RectTransform>().sizeDelta = size;
            if (!string.IsNullOrEmpty(sortingLayerName))
            {
                newCanvas.sortingLayerName = sortingLayerName;
            }

            var container = new GameObject();
            container.transform.SetParent(newGameObject.transform);
            container.name = "container";
            container.transform.localScale = Vector3.one;

            var borderImageGameObject = new GameObject();
            borderImageGameObject.transform.SetParent(container.transform);
            borderImageGameObject.name = "border";
            borderImage = borderImageGameObject.AddComponent<Image>();
            borderImage.transform.position = Vector3.zero;
            borderImage.transform.localScale = Vector3.one;
            borderImage.GetComponent<RectTransform>().sizeDelta = size;
            borderImage.GetComponent<RectTransform>().anchoredPosition = Vector3.zero;

            var bgImageGameObject = new GameObject();
            bgImageGameObject.transform.SetParent(container.transform);
            bgImageGameObject.name = "background";
            backgroundImage = bgImageGameObject.AddComponent<Image>();
            backgroundImage.transform.position = Vector3.zero;
            backgroundImage.transform.localScale = Vector3.one;
            backgroundImage.GetComponent<RectTransform>().sizeDelta = size - backgroundPadding * 2;
            backgroundImage.GetComponent<RectTransform>().anchoredPosition = -backgroundImage.GetComponent<RectTransform>().sizeDelta / 2;
            backgroundImage.GetComponent<RectTransform>().pivot = Vector2.zero;

            var delayedImageGameObject = new GameObject();
            delayedImageGameObject.transform.SetParent(container.transform);
            delayedImageGameObject.name = "delayed";
            delayedImage = delayedImageGameObject.AddComponent<Image>();
            delayedImage.transform.position = Vector3.zero;
            delayedImage.transform.localScale = Vector3.one;
            delayedImage.GetComponent<RectTransform>().sizeDelta = size - backgroundPadding * 2;
            delayedImage.GetComponent<RectTransform>().anchoredPosition = -delayedImage.GetComponent<RectTransform>().sizeDelta / 2;
            delayedImage.GetComponent<RectTransform>().pivot = Vector2.zero;

            var frontImageGameObject = new GameObject();
            frontImageGameObject.transform.SetParent(container.transform);
            frontImageGameObject.name = "foreground";
            foregroundImage = frontImageGameObject.AddComponent<Image>();
            foregroundImage.transform.position = Vector3.zero;
            foregroundImage.transform.localScale = Vector3.one;
            foregroundImage.color = foregroundColor.Evaluate(1);
            foregroundImage.GetComponent<RectTransform>().sizeDelta = size - backgroundPadding * 2;
            foregroundImage.GetComponent<RectTransform>().anchoredPosition = -foregroundImage.GetComponent<RectTransform>().sizeDelta / 2;
            foregroundImage.GetComponent<RectTransform>().pivot = Vector2.zero;

            if (billboard)
            {
                var temp = progressBar.gameObject.AddComponent<BillboardComponent>();
                temp.nestObject = !nestDrawnHealthBar;
            }

            progressBar.lerpDecreasingDelayedBar = lerpDelayedBar;
            progressBar.lerpForegroundBar = lerpFrontBar;
            progressBar.lerpDecreasingDelayedBarSpeed = lerpDelayedBarSpeed;
            progressBar.lerpForegroundBarSpeedIncreasing = lerpFrontBarSpeed;
            progressBar.foregroundBar = foregroundImage.transform;
            progressBar.delayedBarDecreasing = delayedImage.transform;
            progressBar.decreasingDelay = delay;
            progressBar.bumpScaleOnChange = bumpScaleOnChange;
            progressBar.bumpDuration = bumpDuration;
            progressBar.bumpScaleAnimationCurve = bumpAnimationCurve;
            progressBar.timeMode = timeMode;
            container.transform.localEulerAngles = initialRotationAngles;
            progressBar.Initialization();
        }

        /// <summary>
        /// On Update, we hide or show our healthbar based on our current status
        /// </summary>
        protected virtual void Update()
        {
            if (progressBar == null) return;

            if (finalHideStarted) return;

            UpdateDrawnColors();

            if (alwaysVisible) return;

            if (showBar)
            {
                ShowBar(true);
                float currentTime = App.Time(timeMode);
                if (currentTime - lastShowTimestamp > displayDurationOnHit) showBar = false;
            }
            else
            {
                if (BarIsShown()) ShowBar(false);
            }
        }

        /// <summary>
        /// Hides the bar when it reaches zero
        /// </summary>
        /// <returns>The hide bar.</returns>
        protected virtual IEnumerator FinalHideBar()
        {
            finalHideStarted = true;
            if (instantiatedOnDeath != null)
            {
                var obj = Instantiate(instantiatedOnDeath, transform.position + healthBarOffset, transform.rotation);
                SceneManager.MoveGameObjectToScene(obj.gameObject, gameObject.scene);
            }

            if (hideBarAtZeroDelay == 0)
            {
                showBar = false;
                ShowBar(false);
                yield return null;
            }
            else
            {
                progressBar.HideBar(hideBarAtZeroDelay);
            }
        }

        /// <summary>
        /// Updates the colors of the different bars
        /// </summary>
        protected virtual void UpdateDrawnColors()
        {
            if (healthBarType != HealthBarTypes.Drawn) return;

            if (progressBar.Bumping) return;

            if (borderImage != null) borderImage.color = borderColor.Evaluate(progressBar.barProgress);

            if (backgroundImage != null) backgroundImage.color = backgroundColor.Evaluate(progressBar.barProgress);

            if (delayedImage != null) delayedImage.color = delayedColor.Evaluate(progressBar.barProgress);

            if (foregroundImage != null) foregroundImage.color = foregroundColor.Evaluate(progressBar.barProgress);
        }

        /// <summary>
        /// Updates the bar
        /// </summary>
        /// <param name="currentHealth">Current health.</param>
        /// <param name="minHealth">Minimum health.</param>
        /// <param name="maxHealth">Max health.</param>
        /// <param name="show">Whether we should show the bar.</param>
        public virtual void UpdateBar(float currentHealth, float minHealth, float maxHealth, bool show)
        {
            // if the healthbar isn't supposed to be always displayed, we turn it on for the specified duration
            if (!alwaysVisible && show)
            {
                showBar = true;
                lastShowTimestamp = App.Time(timeMode);
            }

            if (progressBar != null)
            {
                progressBar.UpdateBar(currentHealth, minHealth, maxHealth);

                if (hideBarAtZero && progressBar.barTarget <= 0) StartCoroutine(FinalHideBar());

                if (bumpScaleOnChange) progressBar.Bump();
            }
        }

        /// <summary>
        /// A test method used to update the bar when pressing the TestUpdateHealth button in the inspector
        /// </summary>
        [Button]
        protected virtual void TestUpdateHealth() { UpdateBar(testCurrentHealth, testMinHealth, testMaxHealth, true); }
    }
}