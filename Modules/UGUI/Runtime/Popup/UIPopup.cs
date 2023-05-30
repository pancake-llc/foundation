using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Pancake.Apex;
using Pancake.Tween;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Pancake.UI
{
    [HideMonoScript]
    [EditorIcon("script_mono")]
    public abstract class UIPopup : GameComponent
    {
        [Serializable]
        public class MotionData
        {
            public EPopupMotion motion = EPopupMotion.Scale;
            public float duration = 0.3f;
            public Vector2 scale;
            public Vector2 fromPosition;
            public Vector2 toPosition;
            public UIEase ease = UIEase.Smooth;
        }
        
        [SerializeField] protected Canvas canvas;
        [SerializeField] protected CanvasGroup canvasGroup;
        [SerializeField] protected RectTransform container;
        [SerializeField] protected UnityEvent onBeforeShow;
        [SerializeField] protected UnityEvent onAfterShow;
        [SerializeField] protected UnityEvent onBeforeClose;
        [SerializeField] protected UnityEvent onAfterClose;

        [SerializeField] protected bool closeByClickContainer;
        [SerializeField] protected bool closeByClickBackground;
        [SerializeField] protected bool closeByBackButton;
        [SerializeField] private List<Button> closeButtons = new List<Button>();

        [SerializeField] protected MotionData motionShowData = new MotionData() {motion = EPopupMotion.Scale, scale = Vector2.one};
        [SerializeField] protected MotionData motionCloseData = new MotionData() {motion = EPopupMotion.Scale, scale = Vector2.zero};


        public bool BackButtonPressed { get; private set; }
        public bool Active { get; protected set; }

        public int SortingOrder => canvas != null ? canvas.sortingOrder : 0;

        private CancellationTokenSource _tokenCheckPressButton;
        private bool _canActuallyClose;
        private Vector2 _startScale;
        private Vector3 _defaultScale;

        private void Awake() { _defaultScale = container.localScale; }

        protected override void Tick()
        {
            if (Input.GetKeyDown(KeyCode.Escape)) BackButtonPressed = true;
        }

        public virtual async void Show(CancellationToken token = default)
        {
            OnBeforeShow();
            ActivePopup();
            MotionShow();

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
                    if (Application.isPlaying)
                    {
                        //
                    }
                }
            }

            OnAfterShow();
        }

        public virtual void Close()
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

        public virtual void Refresh() { }

        public void ActivePopup()
        {
            Active = true;
            gameObject.SetActive(true);
            BackButtonPressed = false;
            _canActuallyClose = false;
        }

        public void DeactivePopup()
        {
            Active = false;
            gameObject.SetActive(false);
        }

        public virtual void Raise() { canvasGroup.alpha = 1; }

        public virtual void Collapse() { canvasGroup.alpha = 0; }

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
                    container.ActionScaleXY(motionShowData.scale, motionShowData.duration)
                        .SetEase((Ease) motionShowData.ease)
                        .OnComplete(() => canvasGroup.blocksRaycasts = true)
                        .Play();
                    break;
                case EPopupMotion.Position:
                    container.localScale = _defaultScale;
                    container.localPosition = motionShowData.fromPosition;
                    container.ActionLocalMoveXY(motionShowData.toPosition, motionShowData.duration)
                        .SetEase((Ease) motionShowData.ease)
                        .OnComplete(() => canvasGroup.blocksRaycasts = true)
                        .Play();
                    break;
                case EPopupMotion.PositionAndScale:
                    container.pivot = new Vector2(0.5f, 0.5f);
                    container.localScale = _startScale;
                    container.localPosition = motionShowData.fromPosition;
                    container.ActionScaleXY(motionShowData.scale, motionShowData.duration).SetEase((Ease) motionShowData.ease).Play();

                    container.ActionLocalMoveXY(motionShowData.toPosition, motionShowData.duration)
                        .SetEase((Ease) motionShowData.ease)
                        .OnComplete(() => canvasGroup.blocksRaycasts = true)
                        .Play();
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
                    container.ActionScaleXY(motionCloseData.scale, motionCloseData.duration).SetEase((Ease) motionCloseData.ease).OnComplete(End).Play();
                    break;
                case EPopupMotion.Position:
                    container.ActionLocalMoveXY(motionCloseData.toPosition, motionCloseData.duration).SetEase((Ease) motionCloseData.ease).OnComplete(End).Play();
                    break;
                case EPopupMotion.PositionAndScale:
                    container.pivot = new Vector2(0.5f, 0.5f);
                    container.ActionScaleXY(motionCloseData.scale, motionCloseData.duration).SetEase((Ease) motionCloseData.ease).Play();
                    container.ActionLocalMoveXY(motionCloseData.toPosition, motionCloseData.duration).SetEase((Ease) motionCloseData.ease).OnComplete(End).Play();
                    break;
            }
        }

        private void OnApplicationQuit()
        {
            _tokenCheckPressButton?.Cancel();
            _tokenCheckPressButton?.Dispose();
        }
    }
}