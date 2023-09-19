using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Pancake.Apex;
using Pancake.Scriptable;
using PrimeTween;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Pancake.UI
{
    [HideMonoScript]
    [EditorIcon("script_popup")]
    public abstract class UIPopup : GameComponent
    {
        [Serializable]
        public class MotionData
        {
            public EPopupMotion motion = EPopupMotion.Scale;
            public float duration = 0.3f;
            [ShowIf("ShowScaleProperty")] public Vector2 scale;
            [ShowIf("ShowPotionProperty")] public Vector2 fromPosition;
            [ShowIf("ShowPotionProperty")] public Vector2 toPosition;
            public Ease ease = Ease.Default;

#if UNITY_EDITOR
            private bool ShowScaleProperty() => motion != EPopupMotion.Position;
            private bool ShowPotionProperty() => motion != EPopupMotion.Scale;
#endif
        }

        [SerializeField] protected ScriptableEventNoParam closePopupEvent;
        [SerializeField] protected Canvas canvas;
        [SerializeField] protected CanvasGroup canvasGroup;
        [SerializeField] protected RectTransform background;
        [SerializeField] protected RectTransform container;

#if UNITY_EDITOR
        [SerializeField, Foldout("Close Button", Style = "Group"), OnValueChanged("OnCloseByClickContainerChanged")]
        protected bool closeByClickContainer;

        [SerializeField, Foldout("Close Button", Style = "Group"), OnValueChanged("OnCloseByClickBackgroundChanged")]
        protected bool closeByClickBackground;
#endif
        [SerializeField, Foldout("Close Button", Style = "Group")] protected bool closeByBackButton;

        [SerializeField, Array, Foldout("Close Button", Style = "Group")]
        private List<Button> closeButtons = new List<Button>();

        [SerializeField, InlineEditor, Foldout("Motion Show", Style = "Group")]
        protected MotionData motionShowData = new MotionData() {motion = EPopupMotion.Scale, scale = Vector2.one, ease = Ease.OutBack};

        [SerializeField, InlineEditor, Foldout("Motion Close", Style = "Group")]
        protected MotionData motionCloseData = new MotionData() {motion = EPopupMotion.Scale, scale = Vector2.zero};

        [SerializeField, Foldout("Event", Style = "Group")] protected UnityEvent onBeforeShow;
        [SerializeField, Foldout("Event", Style = "Group")] protected UnityEvent onAfterShow;
        [SerializeField, Foldout("Event", Style = "Group")] protected UnityEvent onBeforeClose;
        [SerializeField, Foldout("Event", Style = "Group")] protected UnityEvent onAfterClose;

        public bool BackButtonPressed { get; private set; }
        public bool Active { get; protected set; }
        public int SortingOrder => canvas != null ? canvas.sortingOrder : 0;

        internal UnityEvent OnBeforeShowInternal() => onBeforeShow;
        internal UnityEvent OnAfterShowInternal() => onAfterShow;
        internal UnityEvent OnBeforeCloseInternal() => onBeforeClose;
        internal UnityEvent OnAfterCloseInternal() => onAfterClose;

        /// <summary>
        /// Optional ignore process check press back button
        /// </summary>
        protected virtual bool EnableTrackBackButton() => true;

        private CancellationTokenSource _tokenCheckPressButton;
        private bool _canActuallyClose;
        private Vector2 _startScale;
        private Vector3 _defaultScale;

#if UNITY_EDITOR
        private void OnCloseByClickBackgroundChanged()
        {
            background.TryGetComponent<Button>(out var btn);

            if (closeByClickBackground)
            {
                if (btn == null) btn = background.gameObject.AddComponent<Button>();
                btn.transition = Selectable.Transition.None;
                if (!closeButtons.Contains(btn)) closeButtons.Add(btn);
            }
            else
            {
                if (btn == null) return;
                DestroyImmediate(btn);
                closeButtons?.Remove(btn);
            }
        }

        private void OnCloseByClickContainerChanged()
        {
            container.TryGetComponent<Button>(out var btn);

            if (closeByClickContainer)
            {
                if (btn == null) btn = container.gameObject.AddComponent<Button>();
                btn.transition = Selectable.Transition.None;
                if (!closeButtons.Contains(btn)) closeButtons.Add(btn);
            }
            else
            {
                if (btn == null) return;
                DestroyImmediate(btn);
                closeButtons?.Remove(btn);
            }
        }
#endif

        private void Awake() { _defaultScale = container.localScale; }

        public virtual void Init() { }

        protected override void Tick()
        {
            if (Input.GetKeyDown(KeyCode.Escape)) BackButtonPressed = true;
        }

        internal virtual async void Show(CancellationToken token = default)
        {
            OnBeforeShow();
            ActivePopup();
            MotionShow();

            if (EnableTrackBackButton())
            {
                using (_tokenCheckPressButton = CancellationTokenSource.CreateLinkedTokenSource(token))
                {
                    try
                    {
                        var linkToken = _tokenCheckPressButton.Token;
                        var task = PopupHelper.SelectButton(linkToken, closeButtons.ToArray());
                        Task finishTask;
                        if (closeByBackButton)
                        {
                            var pressTask = PopupHelper.WaitForPressBackButton(linkToken, this);
                            finishTask = await Task.WhenAny(task, pressTask);
                        }
                        else
                        {
                            finishTask = await Task.WhenAny(task);
                        }

                        await finishTask; // Propagate exception if the task finished because of exceptio
                        if (_tokenCheckPressButton != null && !_tokenCheckPressButton.IsCancellationRequested) _tokenCheckPressButton.Cancel();
                    }
                    finally
                    {
                        _tokenCheckPressButton?.Dispose();
                        if (Application.isPlaying) closePopupEvent.Raise();
                    }
                }
            }

            OnAfterShow();
        }

        internal virtual void Close()
        {
            OnBeforeClose();
            MotionClose();
            ActuallyClose();
        }

        protected virtual async void ActuallyClose()
        {
            while (!_canActuallyClose) await Task.Yield();

            DeactivePopup();
            OnAfterClose();
        }

        public virtual void UpdateSortingOrder(int order) { canvas.sortingOrder = order; }

        private void ActivePopup()
        {
            Active = true;
            gameObject.SetActive(true);
            BackButtonPressed = false;
            _canActuallyClose = false;
        }

        private void DeactivePopup()
        {
            Active = false;
            gameObject.SetActive(false);
        }

        internal virtual void Raise() { canvasGroup.alpha = 1; }

        internal virtual void Collapse() { canvasGroup.alpha = 0; }

        protected virtual void OnBeforeShow() { onBeforeShow?.Invoke(); }
        protected virtual void OnAfterShow() { onAfterShow?.Invoke(); }
        protected virtual void OnBeforeClose() { onBeforeClose?.Invoke(); }

        protected virtual void OnAfterClose()
        {
            onAfterClose?.Invoke();
            _tokenCheckPressButton?.Dispose();
        }

        protected virtual void MotionShow()
        {
            canvasGroup.blocksRaycasts = false;
            container.gameObject.SetActive(true);
            switch (motionShowData.motion)
            {
                case EPopupMotion.Scale:
                    container.pivot = new Vector2(0.5f, 0.5f);
                    container.localScale = _startScale;
                    Tween.Scale(container, motionShowData.scale, motionShowData.duration, motionShowData.ease).OnComplete(() => canvasGroup.blocksRaycasts = true);
                    break;
                case EPopupMotion.Position:
                    container.localScale = _defaultScale;
                    container.localPosition = motionShowData.fromPosition;
                    Tween.LocalPosition(container, motionShowData.toPosition, motionShowData.duration, motionShowData.ease)
                        .OnComplete(() => canvasGroup.blocksRaycasts = true);
                    break;
                case EPopupMotion.PositionAndScale:
                    container.pivot = new Vector2(0.5f, 0.5f);
                    container.localScale = _startScale;
                    container.localPosition = motionShowData.fromPosition;
                    Tween.Scale(container, motionShowData.scale, motionShowData.duration, motionShowData.ease);
                    Tween.LocalPosition(container, motionShowData.toPosition, motionShowData.duration, motionShowData.ease)
                        .OnComplete(() => canvasGroup.blocksRaycasts = true);
                    break;
            }
        }

        protected virtual void MotionClose()
        {
            canvasGroup.blocksRaycasts = false;

            void End()
            {
                container.gameObject.SetActive(false);
                _canActuallyClose = true;
            }

            switch (motionCloseData.motion)
            {
                case EPopupMotion.Scale:
                    container.pivot = new Vector2(0.5f, 0.5f);
                    Tween.Scale(container, motionCloseData.scale, motionCloseData.duration, motionCloseData.ease).OnComplete(End);
                    break;
                case EPopupMotion.Position:
                    Tween.Scale(container, motionCloseData.toPosition, motionCloseData.duration, motionCloseData.ease).OnComplete(End);
                    break;
                case EPopupMotion.PositionAndScale:
                    container.pivot = new Vector2(0.5f, 0.5f);
                    Tween.Scale(container, motionCloseData.scale, motionCloseData.duration, motionCloseData.ease);
                    Tween.Scale(container, motionCloseData.toPosition, motionCloseData.duration, motionCloseData.ease).OnComplete(End);
                    break;
            }
        }
        
        /// <summary>
        /// Only use this method when you not add any button in list close button
        /// </summary>
        protected void CancelToken()
        {
            try
            {
                if (_tokenCheckPressButton != null)
                {
                    _tokenCheckPressButton.Cancel();
                    _tokenCheckPressButton.Dispose();
                }
            }
            catch (Exception)
            {
                // ignored
            }
        }
        
        
        private void OnApplicationQuit()
        {
            CancelToken();
        }
    }
}