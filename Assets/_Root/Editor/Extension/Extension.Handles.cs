using System.Collections.Generic;
using Pancake.Core;
using UnityEditor;
using UnityEngine;

namespace Pancake.Editor
{
    public static partial class InEditor
    {
        public static void DrawSprite(Sprite sprite, Vector3 worldSpace, Vector3 size)
        {
            if (sprite == null) return;
            Rect spriteTextureRect = sprite.LocalTextureRect();

            Handles.BeginGUI();

            Vector2 v0 = HandleUtility.WorldToGUIPoint(worldSpace - size / 2f);
            Vector2 v1 = HandleUtility.WorldToGUIPoint(worldSpace + size / 2f);
            Vector2 vMin = new Vector2(Mathf.Min(v0.x, v1.x), Mathf.Min(v0.y, v1.y));
            Vector2 vMax = new Vector2(Mathf.Max(v0.x, v1.x), Mathf.Max(v0.y, v1.y));
            Rect r = new Rect(vMin, vMax - vMin);
            GUI.DrawTextureWithTexCoords(r, sprite.texture, spriteTextureRect);

            Handles.EndGUI();
        }


        public static void DrawGrid(Vector3 start, Vector3 end, Vector3Int steps)
        {
            var vmin = Vector3.Min(start, end);
            var vmax = Vector3.Max(start, end);
            steps = new Vector3Int(Mathf.Abs(steps.x), Mathf.Abs(steps.y), Mathf.Abs(steps.z));

            List<Vector3> poss = new List<Vector3>();

            if (steps.x > 0)
                for (int y = 0; y <= steps.y; y++)
                for (int z = 0; z <= steps.z; z++)
                {
                    poss.Add(new Vector3(0, y, z));
                    poss.Add(new Vector3(steps.x, y, z));
                }

            if (steps.y > 0)
                for (int x = 0; x <= steps.x; x++)
                for (int z = 0; z <= steps.z; z++)
                {
                    poss.Add(new Vector3(x, 0, z));
                    poss.Add(new Vector3(x, steps.y, z));
                }

            if (steps.z > 0)
                for (int x = 0; x <= steps.x; x++)
                for (int y = 0; y <= steps.y; y++)
                {
                    poss.Add(new Vector3(x, y, 0));
                    poss.Add(new Vector3(x, y, steps.z));
                }

            for (int i = 0; i < poss.Count; i++)
            {
                Vector3 p = poss[i];
                p.x = Mathf.Lerp(vmin.x, vmax.x, steps.x > 0 ? p.x / steps.x : 0.5f);
                p.y = Mathf.Lerp(vmin.y, vmax.y, steps.y > 0 ? p.y / steps.y : 0.5f);
                p.z = Mathf.Lerp(vmin.z, vmax.z, steps.z > 0 ? p.z / steps.z : 0.5f);
                poss[i] = p;
            }

            Handles.DrawLines(poss.ToArray());
        }


        public static void SkipEvent()
        {
            var id = GUIUtility.GetControlID(FocusType.Passive);
            GUIUtility.hotControl = id;
            HandleUtility.AddDefaultControl(id);
            Event.current.Use();
        }
    }
}