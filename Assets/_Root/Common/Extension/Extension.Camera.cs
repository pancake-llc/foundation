using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

namespace Pancake.Core
{
    public static partial class C
    {
        public static PlatformMask ToFlag(this RuntimePlatform platform)
        {
            switch (platform)
            {
                case RuntimePlatform.WindowsEditor: return PlatformMask.WindowsEditor;
                case RuntimePlatform.OSXEditor: return PlatformMask.OSXEditor;
                case RuntimePlatform.LinuxEditor: return PlatformMask.LinuxEditor;

                case RuntimePlatform.WindowsPlayer: return PlatformMask.WindowsPlayer;
                case RuntimePlatform.OSXPlayer: return PlatformMask.OSXPlayer;
                case RuntimePlatform.LinuxPlayer: return PlatformMask.LinuxPlayer;

                case RuntimePlatform.PS4: return PlatformMask.PS4;
                case RuntimePlatform.XboxOne: return PlatformMask.XboxOne;
                case RuntimePlatform.Switch: return PlatformMask.Switch;

                case RuntimePlatform.IPhonePlayer: return PlatformMask.iPhone;
                case RuntimePlatform.Android: return PlatformMask.Android;

                case RuntimePlatform.WSAPlayerX86: return PlatformMask.WSAPlayerX86;
                case RuntimePlatform.WSAPlayerX64: return PlatformMask.WSAPlayerX64;
                case RuntimePlatform.WSAPlayerARM: return PlatformMask.WSAPlayerARM;

                case RuntimePlatform.WebGLPlayer: return PlatformMask.WebGLPlayer;

                case RuntimePlatform.tvOS: return PlatformMask.tvOS;
                //case RuntimePlatform.Stadia: return PlatformMask.Stadia;

                default: return PlatformMask.None;
            }
        }


        public static bool Contains(this PlatformMask mask, RuntimePlatform platform) { return (mask & platform.ToFlag()) != 0; }


        public static float ScreenToWorldSize(this Camera camera, float pixelSize, float clipPlane)
        {
            if (camera.orthographic)
            {
                return pixelSize * camera.orthographicSize * 2f / camera.pixelHeight;
            }
            else
            {
                return pixelSize * clipPlane * Mathf.Tan(camera.fieldOfView * 0.5f * Mathf.Deg2Rad) * 2f / camera.pixelHeight;
            }
        }


        public static float WorldToScreenSize(this Camera camera, float worldSize, float clipPlane)
        {
            if (camera.orthographic)
            {
                return worldSize * camera.pixelHeight * 0.5f / camera.orthographicSize;
            }
            else
            {
                return worldSize * camera.pixelHeight * 0.5f / (clipPlane * Mathf.Tan(camera.fieldOfView * 0.5f * Mathf.Deg2Rad));
            }
        }


        public static Vector4 GetClipPlane(this Camera camera, Vector3 point, Vector3 normal)
        {
            Matrix4x4 wtoc = camera.worldToCameraMatrix;
            point = wtoc.MultiplyPoint(point);
            normal = wtoc.MultiplyVector(normal).normalized;

            return new Vector4(normal.x, normal.y, normal.z, -Vector3.Dot(point, normal));
        }


        /// <summary>
        /// Calculate ZBufferParams, can used in compute shader 
        /// </summary>
        public static Vector4 GetZBufferParams(this Camera camera)
        {
            double f = camera.farClipPlane;
            double n = camera.nearClipPlane;

            double rn = 1f / n;
            double rf = 1f / f;
            double fpn = f / n;

            return SystemInfo.usesReversedZBuffer
                ? new Vector4((float) (fpn - 1.0), 1f, (float) (rn - rf), (float) rf)
                : new Vector4((float) (1.0 - fpn), (float) fpn, (float) (rf - rn), (float) rn);
        }


        public static void AddListener(this EventTrigger eventTrigger, EventTriggerType type, UnityAction<BaseEventData> callback)
        {
            var triggers = eventTrigger.triggers;
            var index = triggers.FindIndex(entry => entry.eventID == type);
            if (index < 0)
            {
                var entry = new EventTrigger.Entry();
                entry.eventID = type;
                entry.callback.AddListener(callback);
                triggers.Add(entry);
            }
            else
            {
                triggers[index].callback.AddListener(callback);
            }
        }


        public static void RemoveListener(this EventTrigger eventTrigger, EventTriggerType type, UnityAction<BaseEventData> callback)
        {
            var triggers = eventTrigger.triggers;
            var index = triggers.FindIndex(entry => entry.eventID == type);
            if (index >= 0)
            {
                triggers[index].callback.RemoveListener(callback);
            }
        }
    }
}