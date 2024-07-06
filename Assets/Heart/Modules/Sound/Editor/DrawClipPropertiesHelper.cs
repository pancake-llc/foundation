using UnityEditor;
using UnityEngine;
using System;
using System.Collections.Generic;
using Pancake.Sound;

namespace PancakeEditor.Sound
{
    public class DrawClipPropertiesHelper
    {
        public struct DraggablePoint
        {
            public Rect rect;
            public Texture image;
            public Vector4 imageBorder;
            public Color colorTint;
            public Action<float> onSetPlaybackPosition;

            public DraggablePoint(Rect position)
                : this()
            {
                rect = position;
            }

            public void SetPlaybackPosition(float value) { onSetPlaybackPosition?.Invoke(value); }

            public bool IsDefault() { return rect == default; }
        }

        public const float DRAG_POINT_SIZE_LENGTH = 20f;

        private readonly GUIContent _playbackMainLabel = new("Playback Position");
        private readonly GUIContent _fadeMainLabel = new("Fade");
        private readonly GUIContent[] _fadeLabels = {new("   In"), new("Out")};
        private readonly GUIContent[] _playbackLabels = {new("Start"), new("End"), new("Delay")};
        private readonly Color _silentMaskColor = new(0.2f, 0.2f, 0.2f, 0.8f);
        private readonly Color _fadingMaskColor = new(0.2f, 0.2f, 0.2f, 0.5f);
        private readonly Color _startEndColor = Color.white;
        private readonly Color _fadingLineColor = Color.green;

        private float _clipPreviewHeight;
        private Dictionary<string, Dictionary<ETransportType, DraggablePoint>> _clipDraggablePointsDict = new();
        private KeyValuePair<string, DraggablePoint> _currDraggedPoint;
        private ETransportType[] _allTransportType = Enum.GetValues(typeof(ETransportType)) as ETransportType[];
        private WaveformRenderHelper _waveformHelper = new();
        private Action<string> _onPreviewingClip;

        public void DrawPlaybackPositionField(Rect position, ITransport transport)
        {
            transport.Update();
            EditorGUI.BeginChangeCheck();
            EditorAudioEx.DrawMultiFloatField(position, _playbackMainLabel, _playbackLabels, transport.PlaybackValues);
            if (EditorGUI.EndChangeCheck())
            {
                transport.SetValue(transport.PlaybackValues[0], ETransportType.Start);
                transport.SetValue(transport.PlaybackValues[1], ETransportType.End);
                transport.SetValue(transport.PlaybackValues[2], ETransportType.Delay);
            }
        }

        public void DrawFadingField(Rect position, ITransport transport)
        {
            transport.Update();
            EditorGUI.BeginChangeCheck();
            EditorAudioEx.DrawMultiFloatField(position, _fadeMainLabel, _fadeLabels, transport.FadingValues);
            if (EditorGUI.EndChangeCheck())
            {
                transport.SetValue(transport.FadingValues[0], ETransportType.FadeIn);
                transport.SetValue(transport.FadingValues[1], ETransportType.FadeOut);
            }
        }

        public void DrawClipPreview(Rect previewRect, ITransport transport, AudioClip audioClip, float volume, string clipPath, Action<string> onPreviewClip)
        {
            _clipPreviewHeight = previewRect.height;
            var currEvent = Event.current;
            float exceedTime = Mathf.Max(transport.Delay - transport.StartPosition, 0f);
            var draggablePoints = GetOrCreateDraggablePoints(clipPath);

            DrawWaveformPreview();
            var points = new TransportVectorPoints(transport, new Vector2(previewRect.width, _clipPreviewHeight), audioClip.length + exceedTime);
            DrawClipPlaybackLine();
            DrawExtraSlience();
            DrawDraggable();
            DrawClipLengthLabel();
            HandleDraggable();
            HandlePlayback();

            void DrawWaveformPreview()
            {
                if (currEvent.type == EventType.Repaint)
                {
                    GUI.skin.window.Draw(previewRect,
                        false,
                        false,
                        false,
                        false);

                    var waveformRect = new Rect(previewRect);
                    // The following offset is measure by eyes. IDK where they came from, not GUI.skin.window.padding or margin for sure.
                    waveformRect.x += 2f;
                    waveformRect.width -= 2f;
                    if (transport.Delay > transport.StartPosition)
                    {
                        float exceedTimeInPixels = exceedTime / (exceedTime + audioClip.length) * waveformRect.width;
                        waveformRect.width -= exceedTimeInPixels;
                        waveformRect.x += exceedTimeInPixels;
                    }

                    _waveformHelper.RenderClipWaveform(waveformRect, audioClip);
                }
            }

            void DrawClipPlaybackLine()
            {
                GUI.BeginClip(previewRect);
                {
                    Handles.color = _fadingLineColor;
                    Handles.DrawAAPolyLine(2f, points.GetVectorsClockwise());

                    Handles.color = _startEndColor;
                    Handles.DrawAAPolyLine(1f, points.Start, new Vector3(points.Start.x, 0f));
                    Handles.DrawAAPolyLine(1f, points.End, new Vector3(points.End.x, 0f));

                    Handles.color = _silentMaskColor;
                    var silentToStart = new Vector3[4];
                    silentToStart[0] = Vector3.zero;
                    silentToStart[1] = new Vector3(points.Start.x, 0f);
                    silentToStart[2] = points.Start;
                    silentToStart[3] = new Vector3(0f, _clipPreviewHeight);
                    Handles.DrawAAConvexPolygon(silentToStart);

                    Handles.color = _fadingMaskColor;
                    var startToFadeIn = new Vector3[3];
                    startToFadeIn[0] = new Vector3(points.Start.x, 0f);
                    startToFadeIn[1] = points.FadeIn;
                    startToFadeIn[2] = points.Start;
                    Handles.DrawAAConvexPolygon(startToFadeIn);

                    Handles.color = _fadingMaskColor;
                    var fadeOutToEnd = new Vector3[3];
                    fadeOutToEnd[0] = points.FadeOut;
                    fadeOutToEnd[1] = new Vector3(points.End.x, 0f);
                    fadeOutToEnd[2] = points.End;
                    Handles.DrawAAConvexPolygon(fadeOutToEnd);

                    Handles.color = _silentMaskColor;
                    var endToSilent = new Vector3[4];
                    endToSilent[0] = new Vector3(points.End.x, 0f);
                    endToSilent[1] = new Vector3(previewRect.width, 0f, 0f);
                    endToSilent[2] = new Vector3(previewRect.width, _clipPreviewHeight, 0f);
                    endToSilent[3] = points.End;
                    Handles.DrawAAConvexPolygon(endToSilent);
                }
                GUI.EndClip();
            }

            void DrawDraggable()
            {
                var scaleMode = ScaleMode.ScaleToFit;
                foreach (var transportType in _allTransportType)
                {
                    if (transportType != ETransportType.Delay) // Delay dragging is not supported
                    {
                        var point = GetDraggablePoint(previewRect, points, transport, transportType);
                        draggablePoints[transportType] = point;
                        EditorGUIUtility.AddCursorRect(point.rect, MouseCursor.SlideArrow);
                        GUI.DrawTexture(point.rect,
                            point.image,
                            scaleMode,
                            true,
                            0f,
                            point.colorTint,
                            point.imageBorder,
                            0f);
                    }
                }
            }

            void DrawClipLengthLabel()
            {
                var labelRect = new Rect(previewRect) {height = EditorGUIUtility.singleLineHeight};
                labelRect.y = previewRect.yMax - labelRect.height;
                float currentLength = audioClip.length - transport.StartPosition - transport.EndPosition;
                var text = currentLength.ToString("0.000");
                text += transport.Delay > 0 ? " + " + transport.Delay.ToString("0.000") : string.Empty;
                EditorGUI.DropShadowLabel(labelRect, text + "s");
            }

            void HandleDraggable()
            {
                if (currEvent.type == EventType.MouseDown)
                {
                    foreach (var point in draggablePoints.Values)
                    {
                        if (point.rect.Contains(currEvent.mousePosition))
                        {
                            _currDraggedPoint = new KeyValuePair<string, DraggablePoint>(clipPath, point);
                            currEvent.Use();
                            break;
                        }
                    }
                }
                else if (currEvent.type == EventType.MouseDrag && _currDraggedPoint.Key == clipPath && !_currDraggedPoint.Value.IsDefault())
                {
                    float posInSeconds = currEvent.mousePosition.Scoping(previewRect).x / previewRect.width * audioClip.length;
                    _currDraggedPoint.Value.SetPlaybackPosition(posInSeconds);
                    currEvent.Use();
                }
                else if (currEvent.type == EventType.MouseUp)
                {
                    _currDraggedPoint = default;
                }
            }

            Rect DrawExtraSlience()
            {
                float delayInPixels = transport.Delay / (audioClip.length + exceedTime) * previewRect.width;
                var slientRect = new Rect(previewRect) {width = delayInPixels};
                slientRect.x += (transport.StartPosition + exceedTime) / (exceedTime + audioClip.length) * previewRect.width - delayInPixels;
                EditorGUI.DrawRect(slientRect, _silentMaskColor);
                EditorGUI.DropShadowLabel(slientRect, "Add Slience");
                return previewRect;
            }

            void HandlePlayback()
            {
                if ((currEvent.type == EventType.MouseDown || currEvent.type == EventType.MouseDrag) && previewRect.Contains(currEvent.mousePosition))
                {
                    float clickedPoint = currEvent.mousePosition.Scoping(previewRect).x / previewRect.width;
                    var startSample = (int) Math.Round(clickedPoint * audioClip.samples, MidpointRounding.AwayFromZero);
                    EditorPlayAudioClip.In.PlayClip(audioClip, startSample, 0);
                    EditorPlayAudioClip.In.OnFinished = () => _onPreviewingClip?.Invoke(null);
                    currEvent.Use();

                    if (EditorPlayAudioClip.In.PlaybackIndicator.IsPlaying)
                    {
                        var clip = new PreviewClip() {StartPosition = clickedPoint * audioClip.length, EndPosition = 0f, FullLength = audioClip.length,};
                        EditorPlayAudioClip.In.PlaybackIndicator.SetClipInfo(previewRect, clip);
                    }
                    
                    _onPreviewingClip = onPreviewClip;
                    _onPreviewingClip?.Invoke(clipPath);
                }
            }
        }

        public static void DrawPlaybackIndicator(Rect scope, Vector2 positionOffset = default)
        {
            var indicator = EditorPlayAudioClip.In.PlaybackIndicator;
            if (indicator != null && indicator.IsPlaying)
            {
                GUI.BeginClip(scope);
                {
                    var indicatorRect = indicator.GetIndicatorPosition();
                    EditorGUI.DrawRect(new Rect(indicatorRect.position + positionOffset, indicatorRect.size), indicator.Color);
                }
                GUI.EndClip();
            }
        }

        private Dictionary<ETransportType, DraggablePoint> GetOrCreateDraggablePoints(string clipPath)
        {
            if (!_clipDraggablePointsDict.TryGetValue(clipPath, out var draggablePoints))
            {
                draggablePoints = new Dictionary<ETransportType, DraggablePoint>()
                {
                    {ETransportType.Start, default}, {ETransportType.FadeIn, default}, {ETransportType.FadeOut, default}, {ETransportType.End, default},
                };
                _clipDraggablePointsDict.Add(clipPath, draggablePoints);
            }

            return draggablePoints;
        }

        private DraggablePoint GetDraggablePoint(Rect waveformRect, TransportVectorPoints points, ITransport transport, ETransportType transportType)
        {
            var rect = GetDraggableRect(waveformRect, points, transportType);
            switch (transportType)
            {
                case ETransportType.Start:
                    return new DraggablePoint(rect)
                    {
                        image = EditorGUIUtility.IconContent("IN foldout focus on@2x").image,
                        imageBorder = new Vector4(DRAG_POINT_SIZE_LENGTH * 0.5f, 0f, 0f, 0f),
                        colorTint = _startEndColor,
                        onSetPlaybackPosition = posInSec => transport.SetValue(posInSec, transportType),
                    };
                case ETransportType.FadeIn:
                    return new DraggablePoint(rect)
                    {
                        image = EditorGUIUtility.IconContent("AudioHighPassFilter Icon").image,
                        colorTint = _fadingLineColor,
                        onSetPlaybackPosition = posInSec => transport.SetValue(posInSec - transport.StartPosition, transportType),
                    };
                case ETransportType.FadeOut:
                    return new DraggablePoint(rect)
                    {
                        image = EditorGUIUtility.IconContent("AudioLowPassFilter Icon").image,
                        colorTint = _fadingLineColor,
                        onSetPlaybackPosition = posInSec => transport.SetValue(transport.FullLength - transport.EndPosition - posInSec, transportType),
                    };
                case ETransportType.End:
                    return new DraggablePoint(rect)
                    {
                        image = EditorGUIUtility.IconContent("IN foldout focus on@2x").image,
                        imageBorder = new Vector4(0f, 0f, DRAG_POINT_SIZE_LENGTH * 0.5f, 0f),
                        colorTint = _startEndColor,
                        onSetPlaybackPosition = posInSec => transport.SetValue(transport.FullLength - posInSec, transportType),
                    };
                default:
                    Debug.LogError(AudioConstant.LOG_HEADER + $"No corresponding point for transport type {transportType}");
                    return default;
            }
        }

        private Rect GetDraggableRect(Rect waveformRect, TransportVectorPoints points, ETransportType transportType)
        {
            var offset = new Vector2(-DRAG_POINT_SIZE_LENGTH * 0.5f, -DRAG_POINT_SIZE_LENGTH);
            var dragPointSize = new Vector2(DRAG_POINT_SIZE_LENGTH, DRAG_POINT_SIZE_LENGTH);
            Vector2 position = default;
            switch (transportType)
            {
                case ETransportType.Start:
                    position = new Vector2(points.Start.x, 0f).DeScope(waveformRect, offset);
                    break;
                case ETransportType.FadeIn:
                    position = new Vector2(points.FadeIn.x, dragPointSize.y).DeScope(waveformRect, offset);
                    break;
                case ETransportType.FadeOut:
                    position = new Vector2(points.FadeOut.x, dragPointSize.y).DeScope(waveformRect, offset);
                    break;
                case ETransportType.End:
                    position = new Vector2(points.End.x, 0f).DeScope(waveformRect, offset);
                    break;
            }

            return new Rect(position, dragPointSize);
        }
    }
}