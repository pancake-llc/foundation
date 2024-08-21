using System;
using UnityEditor;
using UnityEngine;

namespace FullscreenEditor
{
    [InitializeOnLoad]
    public class RestoreCursorState
    {
        static RestoreCursorState()
        {
            var cursorVisible = Cursor.visible;

            // this is where the magic happens
            Action<FullscreenContainer> magic = fs =>
            {
                if (!FullscreenPreferences.RestoreCursorLockAndHideState) return;

                // frame count doesn't seem to make much of a difference,
                // but I think it's best to do this after the view is
                // focused by the "FixGameViewMouseInput" class
                After.Frames(55,
                    () =>
                    {
                        var gameView = fs.m_dst.Window && fs.m_dst.Window.IsOfType(Types.GameView) ? fs.m_dst.Window :
                            fs.m_src.Window && fs.m_src.Window.IsOfType(Types.GameView) ? fs.m_src.Window : null;

                        if (!EditorApplication.isPaused && gameView && gameView.IsOfType(Types.GameView) && gameView.HasMethod("AllowCursorLockAndHide"))
                        {
                            gameView.InvokeMethod("AllowCursorLockAndHide", true);
                            Unsupported.SetAllowCursorHide(true);
                            Cursor.visible = cursorVisible;
                        }
                    });
            };

            Action<FullscreenContainer> storeCursorVisible = (_) => { cursorVisible = Cursor.visible; };

            FullscreenCallbacks.afterFullscreenOpen += magic;
            FullscreenCallbacks.afterFullscreenClose += magic;

            FullscreenCallbacks.beforeFullscreenOpen += storeCursorVisible;
            FullscreenCallbacks.beforeFullscreenClose += storeCursorVisible;
        }
    }
}