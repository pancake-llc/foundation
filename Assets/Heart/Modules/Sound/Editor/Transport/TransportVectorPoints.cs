using UnityEngine;

namespace PancakeEditor.Sound
{
    public struct TransportVectorPoints
    {
        public readonly ITransport transport;
        public readonly Vector2 drawingSize;
        public readonly float clipLength;

        public TransportVectorPoints(ITransport transport, Vector2 drawingSize, float clipLength)
        {
            this.transport = transport;
            this.drawingSize = drawingSize;
            this.clipLength = clipLength;
        }

        public Vector3 Start => new(Mathf.Lerp(0f, drawingSize.x, (transport.StartPosition + GetExceededTime()) / clipLength), drawingSize.y);
        public Vector3 FadeIn => new(Mathf.Lerp(0f, drawingSize.x, (transport.StartPosition + transport.FadeIn + GetExceededTime()) / clipLength), 0f);
        public Vector3 FadeOut => new(Mathf.Lerp(0f, drawingSize.x, (clipLength - transport.EndPosition - transport.FadeOut) / clipLength), 0f);
        public Vector3 End => new(Mathf.Lerp(0f, drawingSize.x, (clipLength - transport.EndPosition) / clipLength), drawingSize.y);
        public Vector3[] GetVectorsClockwise() { return new Vector3[] {Start, FadeIn, FadeOut, End}; }

        public float GetExceededTime() { return Mathf.Max(0f, transport.Delay - transport.StartPosition); }
    }
}