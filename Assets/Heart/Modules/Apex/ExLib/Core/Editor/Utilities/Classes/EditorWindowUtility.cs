using UnityEditor;
using UnityEngine;

namespace Pancake.ExLibEditor.Windows
{
    public static class EditorWindowUtility
    {
        #region [Extension Methods]

        public static void MoveToCenter(this EditorWindow window)
        {
            Rect position = window.position;
            Rect mainWindowPosition = ScreenUtility.GetCenter();
            if (mainWindowPosition != Rect.zero)
            {
                float width = (mainWindowPosition.width - position.width) * 0.5f;
                float height = (mainWindowPosition.height - position.height) * 0.5f;
                position.x = mainWindowPosition.x + width;
                position.y = mainWindowPosition.y + height;
                window.position = position;
            }
            else
            {
                window.position = new Rect(new Vector2(Screen.width, Screen.height), new Vector2(position.width, position.height));
            }
        }

        #endregion
    }
}