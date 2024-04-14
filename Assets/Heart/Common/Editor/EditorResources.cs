using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace PancakeEditor.Common
{
    public static class EditorResources
    {
        private static readonly Dictionary<string, Texture2D> TextureCached = new();

        public static Texture2D BoxContentDark
        {
            get
            {
                TextureCached.TryGetValue(nameof(BoxContentDark), out var tex);

                if (tex != null) return tex;
                tex = Editor.ConvertToTexture(
                    "iVBORw0KGgoAAAANSUhEUgAAABAAAAAQBAMAAADt3eJSAAAAD1BMVEVEREQlJSUmJiYtLS04ODhTxJsAAAAAJklEQVQI12NQYIAAGIMZxmBiEIAwGKnOQFhhDKaBlLOiIBAImQAAdGYChuP2NCcAAAAASUVORK5CYII=");
                TextureCached.Add(nameof(BoxContentDark), tex);
                return tex;
            }
        }

        public static Texture2D BoxBackgroundDark
        {
            get
            {
                TextureCached.TryGetValue(nameof(BoxBackgroundDark), out var tex);

                if (tex != null) return tex;
                tex = Editor.ConvertToTexture(
                    "iVBORw0KGgoAAAANSUhEUgAAABAAAAAQBAMAAADt3eJSAAAAD1BMVEVEREQlJSUtLS0mJiY4ODhE6guiAAAAJ0lEQVQI12NwMhQEAmEVBiUGEABSBhAGM4MAhMFIdQbcCrilcGcAAP3gA3XIoRcnAAAAAElFTkSuQmCC");
                TextureCached.Add(nameof(BoxBackgroundDark), tex);
                return tex;
            }
        }

        public static Texture2D EvenBackground
        {
            get
            {
                TextureCached.TryGetValue(nameof(EvenBackground), out var tex);

                if (tex != null) return tex;
                tex = Editor.ConvertToTexture("iVBORw0KGgoAAAANSUhEUgAAAAUAAAAFAQMAAAC3obSmAAAAA1BMVEVEREQ1TRdOAAAACklEQVQI12OAAwAACgABaQY5MgAAAABJRU5ErkJggg==");
                TextureCached.Add(nameof(EvenBackground), tex);
                return tex;
            }
        }

        public static Texture2D EvenBackgroundBlue
        {
            get
            {
                TextureCached.TryGetValue(nameof(EvenBackgroundBlue), out var tex);

                if (tex != null) return tex;
                tex = Editor.ConvertToTexture("iVBORw0KGgoAAAANSUhEUgAAAAUAAAAFAQMAAAC3obSmAAAAA1BMVEUsXYfq5LfEAAAACklEQVQI12OAAwAACgABaQY5MgAAAABJRU5ErkJggg==");
                TextureCached.Add(nameof(EvenBackgroundBlue), tex);
                return tex;
            }
        }

        public static Texture2D EvenBackgroundDark
        {
            get
            {
                TextureCached.TryGetValue(nameof(EvenBackgroundDark), out var tex);

                if (tex != null) return tex;
                tex = Editor.ConvertToTexture("iVBORw0KGgoAAAANSUhEUgAAAAUAAAAFAQMAAAC3obSmAAAAA1BMVEU3Nzeu5Mv0AAAACklEQVQI12OAAwAACgABaQY5MgAAAABJRU5ErkJggg==");
                TextureCached.Add(nameof(EvenBackgroundDark), tex);
                return tex;
            }
        }

        public static Texture2D ReorderableArrayNormal(string theme)
        {
            TextureCached.TryGetValue($"{nameof(ReorderableArrayNormal)}{theme}", out var tex);

            if (tex != null) return tex;
            if (theme.Equals("DarkTheme"))
            {
                tex = Editor.ConvertToTexture(
                    "iVBORw0KGgoAAAANSUhEUgAAAAgAAAAIBAMAAAA2IaO4AAAAD1BMVEUZGRlMTExFRUVPT09ISEjRaqlGAAAAIUlEQVQI12MAA2ZjYwMGRkFBAQjB4uLiwMCkpKQAIUAAADJQAphR8G2mAAAAAElFTkSuQmCC");
            }
            else
            {
                tex = Editor.ConvertToTexture(
                    "iVBORw0KGgoAAAANSUhEUgAAAAgAAAAIBAMAAAA2IaO4AAAAD1BMVEVVVVXT09PMzMzW1tbQ0NB/bCfkAAAAIUlEQVQI12MAA2ZjYwMGRkFBAQjB4uLiwMCkpKQAIUAAADJQAphR8G2mAAAAAElFTkSuQmCC");
            }

            TextureCached.Add($"{nameof(ReorderableArrayNormal)}{theme}", tex);
            return tex;
        }

        public static Texture2D ReorderableArrayEntryBackground(string theme)
        {
            TextureCached.TryGetValue($"{nameof(ReorderableArrayEntryBackground)}{theme}", out var tex);

            if (tex != null) return tex;
            if (theme.Equals("DarkTheme"))
            {
                tex = Editor.ConvertToTexture("iVBORw0KGgoAAAANSUhEUgAAAAgAAAAIAQMAAAD+wSzIAAAABlBMVEUyMjIZGRmBil2CAAAAD0lEQVQI12P4z9CIBP8DACgwBQWtv/itAAAAAElFTkSuQmCC");
            }
            else
            {
                tex = Editor.ConvertToTexture("iVBORw0KGgoAAAANSUhEUgAAAAgAAAAIAQMAAAD+wSzIAAAABlBMVEWxsbFVVVVKIpF1AAAAD0lEQVQI12P4z9CIBP8DACgwBQWtv/itAAAAAElFTkSuQmCC");
            }

            TextureCached.Add($"{nameof(ReorderableArrayEntryBackground)}{theme}", tex);
            return tex;
        }

        public static Texture2D ReorderableArrayPress(string theme)
        {
            TextureCached.TryGetValue($"{nameof(ReorderableArrayPress)}{theme}", out var tex);

            if (tex != null) return tex;
            if (theme.Equals("DarkTheme"))
            {
                tex = Editor.ConvertToTexture(
                    "iVBORw0KGgoAAAANSUhEUgAAAAgAAAAIAgMAAAC5YVYYAAAADFBMVEUZGRlJYoFGX31DXHpK0zOCAAAAF0lEQVQI12MAAtEQENJaAUL2f4AIKAYAL9YE7Xs/oqYAAAAASUVORK5CYII=");
            }
            else
            {
                tex = Editor.ConvertToTexture(
                    "iVBORw0KGgoAAAANSUhEUgAAAAgAAAAIBAMAAAA2IaO4AAAAD1BMVEVVVVXx8fHq6ur09PTt7e2ZzNamAAAAIUlEQVQI12MAA2ZjYwMGRkFBAQjB4uLiwMCkpKQAIUAAADJQAphR8G2mAAAAAElFTkSuQmCC");
            }

            TextureCached.Add($"{nameof(ReorderableArrayPress)}{theme}", tex);
            return tex;
        }

        public static Texture2D ReorderableArrayHover(string theme)
        {
            TextureCached.TryGetValue($"{nameof(ReorderableArrayHover)}{theme}", out var tex);

            if (tex != null) return tex;
            if (theme.Equals("DarkTheme"))
            {
                tex = Editor.ConvertToTexture(
                    "iVBORw0KGgoAAAANSUhEUgAAAAgAAAAIBAMAAAA2IaO4AAAAD1BMVEUZGRlRUVFKSkpUVFRNTU320S0gAAAAIUlEQVQI12MAA2ZjYwMGRkFBAQjB4uLiwMCkpKQAIUAAADJQAphR8G2mAAAAAElFTkSuQmCC");
            }
            else
            {
                tex = Editor.ConvertToTexture(
                    "iVBORw0KGgoAAAANSUhEUgAAAAgAAAAIBAMAAAA2IaO4AAAAD1BMVEVVVVXi4uLb29vl5eXe3t53x1gGAAAAIUlEQVQI12MAA2ZjYwMGRkFBAQjB4uLiwMCkpKQAIUAAADJQAphR8G2mAAAAAElFTkSuQmCC");
            }

            TextureCached.Add($"{nameof(ReorderableArrayHover)}{theme}", tex);
            return tex;
        }

        public static Texture2D ReorderableArrayEntryEven(string theme)
        {
            TextureCached.TryGetValue($"{nameof(ReorderableArrayEntryEven)}{theme}", out var tex);

            if (tex != null) return tex;
            if (theme.Equals("DarkTheme"))
            {
                tex = Editor.ConvertToTexture("iVBORw0KGgoAAAANSUhEUgAAAAgAAAAIAQMAAAD+wSzIAAAABlBMVEU3NzcZGRkuebNzAAAAD0lEQVQI12P4z9CIBP8DACgwBQWtv/itAAAAAElFTkSuQmCC");
            }
            else
            {
                tex = Editor.ConvertToTexture("iVBORw0KGgoAAAANSUhEUgAAAAgAAAAIAQMAAAD+wSzIAAAABlBMVEXDw8NVVVVO7OYHAAAAD0lEQVQI12P4z9CIBP8DACgwBQWtv/itAAAAAElFTkSuQmCC");
            }

            TextureCached.Add($"{nameof(ReorderableArrayEntryEven)}{theme}", tex);
            return tex;
        }

        public static Texture2D ReorderableArrayEntryFocus(string theme)
        {
            TextureCached.TryGetValue($"{nameof(ReorderableArrayEntryFocus)}{theme}", out var tex);

            if (tex != null) return tex;
            if (theme.Equals("DarkTheme"))
            {
                tex = Editor.ConvertToTexture("iVBORw0KGgoAAAANSUhEUgAAAAgAAAAIAQMAAAD+wSzIAAAABlBMVEVGUFAZGRlj5AnwAAAAD0lEQVQI12P4z9CIBP8DACgwBQWtv/itAAAAAElFTkSuQmCC");
            }
            else
            {
                tex = Editor.ConvertToTexture("iVBORw0KGgoAAAANSUhEUgAAAAgAAAAIAQMAAAD+wSzIAAAABlBMVEXS3NxVVVVsrHCjAAAAD0lEQVQI12P4z9CIBP8DACgwBQWtv/itAAAAAElFTkSuQmCC");
            }

            TextureCached.Add($"{nameof(ReorderableArrayEntryFocus)}{theme}", tex);
            return tex;
        }

        public static Texture2D ReorderableArrayEntryOdd(string theme)
        {
            TextureCached.TryGetValue($"{nameof(ReorderableArrayEntryOdd)}{theme}", out var tex);

            if (tex != null) return tex;
            if (theme.Equals("DarkTheme"))
            {
                tex = Editor.ConvertToTexture("iVBORw0KGgoAAAANSUhEUgAAAAgAAAAIAQMAAAD+wSzIAAAABlBMVEUyMjIZGRmBil2CAAAAD0lEQVQI12P4z9CIBP8DACgwBQWtv/itAAAAAElFTkSuQmCC");
            }
            else
            {
                tex = Editor.ConvertToTexture("iVBORw0KGgoAAAANSUhEUgAAAAgAAAAIAQMAAAD+wSzIAAAABlBMVEWxsbFVVVVKIpF1AAAAD0lEQVQI12P4z9CIBP8DACgwBQWtv/itAAAAAElFTkSuQmCC");
            }

            TextureCached.Add($"{nameof(ReorderableArrayEntryOdd)}{theme}", tex);
            return tex;
        }

        public static Texture2D ReorderableArrayEntryActive(string theme)
        {
            TextureCached.TryGetValue($"{nameof(ReorderableArrayEntryActive)}{theme}", out var tex);

            if (tex != null) return tex;
            if (theme.Equals("DarkTheme"))
            {
                tex = Editor.ConvertToTexture("iVBORw0KGgoAAAANSUhEUgAAAAgAAAAIAQMAAAD+wSzIAAAABlBMVEU8RkYZGRlTj002AAAAD0lEQVQI12P4z9CIBP8DACgwBQWtv/itAAAAAElFTkSuQmCC");
            }
            else
            {
                tex = Editor.ConvertToTexture("iVBORw0KGgoAAAANSUhEUgAAAAgAAAAIAQMAAAD+wSzIAAAABlBMVEXI0tJVVVWRDpgMAAAAD0lEQVQI12P4z9CIBP8DACgwBQWtv/itAAAAAElFTkSuQmCC");
            }

            TextureCached.Add($"{nameof(ReorderableArrayEntryActive)}{theme}", tex);
            return tex;
        }


        private const string RELATIVE_PATH = "Modules/Apex/ExLib/Core/Editor/Misc/Icons";
        private const string RELATIVE_TEMPLATE_PATH = "Modules/Apex/ExLib/Core/Editor/Misc/Templates";

        public static Texture2D ScriptableEvent => ProjectDatabase.FindAssetWithPath<Texture2D>("scriptable_event.png", RELATIVE_PATH);
        public static Texture2D ScriptableEventListener => ProjectDatabase.FindAssetWithPath<Texture2D>("scriptable_event_listener.png", RELATIVE_PATH);
        public static Texture2D ScriptableList => ProjectDatabase.FindAssetWithPath<Texture2D>("scriptable_list.png", RELATIVE_PATH);
        public static Texture2D ScriptableVariable => ProjectDatabase.FindAssetWithPath<Texture2D>("scriptable_variable.png", RELATIVE_PATH);
        public static Texture2D StarEmpty => ProjectDatabase.FindAssetWithPath<Texture2D>("star_empty.png", RELATIVE_PATH);
        public static Texture2D StarFull => ProjectDatabase.FindAssetWithPath<Texture2D>("star_full.png", RELATIVE_PATH);
        public static Texture2D ScriptableAd => ProjectDatabase.FindAssetWithPath<Texture2D>("scriptable_ad.png", RELATIVE_PATH);
        public static Texture2D ScriptableIap => ProjectDatabase.FindAssetWithPath<Texture2D>("scriptable_iap.png", RELATIVE_PATH);
        public static Texture2D ScriptableFirebase => ProjectDatabase.FindAssetWithPath<Texture2D>("scriptable_firebase.png", RELATIVE_PATH);
        public static Texture2D ScriptableAdjust => ProjectDatabase.FindAssetWithPath<Texture2D>("scriptable_adjust.png", RELATIVE_PATH);
        public static Texture2D ScriptableNotification => ProjectDatabase.FindAssetWithPath<Texture2D>("scriptable_notification.png", RELATIVE_PATH);
        public static Texture2D ScriptableSetting => ProjectDatabase.FindAssetWithPath<Texture2D>("scriptable_setting.png", RELATIVE_PATH);
        public static Texture2D ScriptableEditorSetting => ProjectDatabase.FindAssetWithPath<Texture2D>("scriptable_editor_setting.png", RELATIVE_PATH);
        public static Texture2D ScriptableEditorSpine => ProjectDatabase.FindAssetWithPath<Texture2D>("scriptable_editor_spine.png", RELATIVE_PATH);
        public static Texture2D ScriptableInterface => ProjectDatabase.FindAssetWithPath<Texture2D>("scriptable_interface.png", RELATIVE_PATH);
        public static Texture2D ScriptableFactory => ProjectDatabase.FindAssetWithPath<Texture2D>("scriptable_factory.png", RELATIVE_PATH);
        public static Texture2D ScriptableUnity => ProjectDatabase.FindAssetWithPath<Texture2D>("scriptable_unity.png", RELATIVE_PATH);
        public static Texture2D IconEyeOpen => ProjectDatabase.FindAssetWithPath<Texture2D>("icon_eye_open.png", RELATIVE_PATH);
        public static Texture2D IconEyeClose => ProjectDatabase.FindAssetWithPath<Texture2D>("icon_eye_close.png", RELATIVE_PATH);

        public static Texture2D TreeMapCurrent
        {
            get
            {
                TextureCached.TryGetValue(nameof(TreeMapCurrent), out var tex);

                if (tex != null) return tex;
                tex = Editor.ConvertToTexture(
                    "iVBORw0KGgoAAAANSUhEUgAAAA4AAAAQCAYAAAAmlE46AAAAQ0lEQVQoFWNgwA+E/wMBUIkwujImdAFi+aMa8YQUCx45dCmUKCFKIzAq36CbQpRGRkZGEbI0QjW9RdY8mgCQQwONDQApiglJmB+fmgAAAABJRU5ErkJggg==");
                TextureCached.Add(nameof(TreeMapCurrent), tex);
                return tex;
            }
        }

        public static Texture2D TreeMapLast
        {
            get
            {
                TextureCached.TryGetValue(nameof(TreeMapLast), out var tex);

                if (tex != null) return tex;
                tex = Editor.ConvertToTexture(
                    "iVBORw0KGgoAAAANSUhEUgAAAA4AAAAQCAYAAAAmlE46AAAAPUlEQVQoFWNgwA+E/wMBUIkwujImdAFi+aMa8YQUCx45dCmUKCFKIzAq36CbwogugMbnBvI50MRGuTQLAQD/rQhHffk54gAAAABJRU5ErkJggg==");
                TextureCached.Add(nameof(TreeMapLast), tex);
                return tex;
            }
        }

        public static Texture2D TreeMapLevel
        {
            get
            {
                TextureCached.TryGetValue(nameof(TreeMapLevel), out var tex);

                if (tex != null) return tex;
                tex = Editor.ConvertToTexture(
                    "iVBORw0KGgoAAAANSUhEUgAAAA4AAAAQCAYAAAAmlE46AAAAJElEQVQoFWNgwA8k/wMBUIkkujImdAFi+aMa8YTUaOCM8MABAI00BE1+cZ4yAAAAAElFTkSuQmCC");
                TextureCached.Add(nameof(TreeMapLevel), tex);
                return tex;
            }
        }

        public static Texture2D TreeMapLevel4
        {
            get
            {
                TextureCached.TryGetValue(nameof(TreeMapLevel4), out var tex);

                if (tex != null) return tex;
                tex = Editor.ConvertToTexture(
                    "iVBORw0KGgoAAAANSUhEUgAAADgAAAAQCAYAAABDebxFAAAATklEQVRIDe2SMQoAMAgDpV/w/29t3QWzpnKOGiHmjJgrb1VJcpa1qc3eadaWNTjwd6AQhKB5AryoOSBpD4IyInMBBM0BSXsQlBGZC9YTfL7XEKcUdfHdAAAAAElFTkSuQmCC");
                TextureCached.Add(nameof(TreeMapLevel4), tex);
                return tex;
            }
        }

        public static Texture2D TreeMapLine
        {
            get
            {
                TextureCached.TryGetValue(nameof(TreeMapLine), out var tex);

                if (tex != null) return tex;
                tex = Editor.ConvertToTexture(
                    "iVBORw0KGgoAAAANSUhEUgAAAA4AAAAQCAMAAAARSr4IAAAACVBMVEX///8AAAD///9+749PAAAAAnRSTlMAE/ItjOYAAAAWSURBVHgBY6AbYEQAEJcJDhjRZWkJABQbACw6WoebAAAAAElFTkSuQmCC");
                TextureCached.Add(nameof(TreeMapLine), tex);
                return tex;
            }
        }

        public static Texture2D IconTrim => ProjectDatabase.FindAssetWithPath<Texture2D>("icon_trim.png", RELATIVE_PATH);
        public static Texture2D IconWarning => ProjectDatabase.FindAssetWithPath<Texture2D>("icon_warning.png", RELATIVE_PATH);
        public static Texture2D IconCategoryLayout => ProjectDatabase.FindAssetWithPath<Texture2D>("icon_category_layout.png", RELATIVE_PATH);
        public static Texture2D IconDelete => ProjectDatabase.FindAssetWithPath<Texture2D>("icon_delete.png", RELATIVE_PATH);
        public static Texture2D IconDuplicate => ProjectDatabase.FindAssetWithPath<Texture2D>("icon_duplicate.png", RELATIVE_PATH);
        public static Texture2D IconEdit => ProjectDatabase.FindAssetWithPath<Texture2D>("icon_edit.png", RELATIVE_PATH);
        public static Texture2D IconPing => ProjectDatabase.FindAssetWithPath<Texture2D>("icon_ping.png", RELATIVE_PATH);
        public static Texture2D IconCancel => ProjectDatabase.FindAssetWithPath<Texture2D>("icon_cancel.png", RELATIVE_PATH);

        public static Texture2D IconCopyComponent(string theme)
        {
            TextureCached.TryGetValue($"{nameof(IconCopyComponent)}{theme}", out var tex);

            if (tex != null) return tex;
            if (theme.Equals("DarkTheme"))
            {
                tex = Editor.ConvertToTexture(
                    "iVBORw0KGgoAAAANSUhEUgAAABAAAAAQCAYAAAAf8/9hAAAACXBIWXMAAAsTAAALEwEAmpwYAAAF8WlUWHRYTUw6Y29tLmFkb2JlLnhtcAAAAAAAPD94cGFja2V0IGJlZ2luPSLvu78iIGlkPSJXNU0wTXBDZWhpSHpyZVN6TlRjemtjOWQiPz4gPHg6eG1wbWV0YSB4bWxuczp4PSJhZG9iZTpuczptZXRhLyIgeDp4bXB0az0iQWRvYmUgWE1QIENvcmUgNi4wLWMwMDIgNzkuMTY0NDg4LCAyMDIwLzA3LzEwLTIyOjA2OjUzICAgICAgICAiPiA8cmRmOlJERiB4bWxuczpyZGY9Imh0dHA6Ly93d3cudzMub3JnLzE5OTkvMDIvMjItcmRmLXN5bnRheC1ucyMiPiA8cmRmOkRlc2NyaXB0aW9uIHJkZjphYm91dD0iIiB4bWxuczp4bXA9Imh0dHA6Ly9ucy5hZG9iZS5jb20veGFwLzEuMC8iIHhtbG5zOnhtcE1NPSJodHRwOi8vbnMuYWRvYmUuY29tL3hhcC8xLjAvbW0vIiB4bWxuczpzdEV2dD0iaHR0cDovL25zLmFkb2JlLmNvbS94YXAvMS4wL3NUeXBlL1Jlc291cmNlRXZlbnQjIiB4bWxuczpkYz0iaHR0cDovL3B1cmwub3JnL2RjL2VsZW1lbnRzLzEuMS8iIHhtbG5zOnBob3Rvc2hvcD0iaHR0cDovL25zLmFkb2JlLmNvbS9waG90b3Nob3AvMS4wLyIgeG1wOkNyZWF0b3JUb29sPSJBZG9iZSBQaG90b3Nob3AgMjIuMCAoV2luZG93cykiIHhtcDpDcmVhdGVEYXRlPSIyMDI0LTAxLTE2VDE0OjExOjI2KzA3OjAwIiB4bXA6TWV0YWRhdGFEYXRlPSIyMDI0LTAxLTE2VDE0OjExOjI2KzA3OjAwIiB4bXA6TW9kaWZ5RGF0ZT0iMjAyNC0wMS0xNlQxNDoxMToyNiswNzowMCIgeG1wTU06SW5zdGFuY2VJRD0ieG1wLmlpZDo1MDc2MGJmMy02MmRhLWI3NDMtYjhmMC0wY2UwZGM4OWU4NDMiIHhtcE1NOkRvY3VtZW50SUQ9ImFkb2JlOmRvY2lkOnBob3Rvc2hvcDo2M2EyYzg4NC02YzY5LWVhNGUtYTE0Zi03ZDdiY2U4MmU1NGQiIHhtcE1NOk9yaWdpbmFsRG9jdW1lbnRJRD0ieG1wLmRpZDpiYzE1M2RmMi00ZDc2LWM2NDMtODBiZi0yM2JjZDY5Nzk5M2YiIGRjOmZvcm1hdD0iaW1hZ2UvcG5nIiBwaG90b3Nob3A6Q29sb3JNb2RlPSIzIiBwaG90b3Nob3A6SUNDUHJvZmlsZT0ic1JHQiBJRUM2MTk2Ni0yLjEiPiA8eG1wTU06SGlzdG9yeT4gPHJkZjpTZXE+IDxyZGY6bGkgc3RFdnQ6YWN0aW9uPSJjcmVhdGVkIiBzdEV2dDppbnN0YW5jZUlEPSJ4bXAuaWlkOmJjMTUzZGYyLTRkNzYtYzY0My04MGJmLTIzYmNkNjk3OTkzZiIgc3RFdnQ6d2hlbj0iMjAyNC0wMS0xNlQxNDoxMToyNiswNzowMCIgc3RFdnQ6c29mdHdhcmVBZ2VudD0iQWRvYmUgUGhvdG9zaG9wIDIyLjAgKFdpbmRvd3MpIi8+IDxyZGY6bGkgc3RFdnQ6YWN0aW9uPSJzYXZlZCIgc3RFdnQ6aW5zdGFuY2VJRD0ieG1wLmlpZDo1MDc2MGJmMy02MmRhLWI3NDMtYjhmMC0wY2UwZGM4OWU4NDMiIHN0RXZ0OndoZW49IjIwMjQtMDEtMTZUMTQ6MTE6MjYrMDc6MDAiIHN0RXZ0OnNvZnR3YXJlQWdlbnQ9IkFkb2JlIFBob3Rvc2hvcCAyMi4wIChXaW5kb3dzKSIgc3RFdnQ6Y2hhbmdlZD0iLyIvPiA8L3JkZjpTZXE+IDwveG1wTU06SGlzdG9yeT4gPC9yZGY6RGVzY3JpcHRpb24+IDwvcmRmOlJERj4gPC94OnhtcG1ldGE+IDw/eHBhY2tldCBlbmQ9InIiPz7OY8sMAAAAYklEQVQ4jWP8//8/AyWAiSLdDAwMLDBGeHg4IacwwhgrV67ENABdERrAaTixXsBlMIYLCNqIbhguA3DZ+h+K4XKkxgKGoRRHI9UNIDlZIgcizqgixQUkA3zRSJR3GAc8NwIAX40QJpxWRGQAAAAASUVORK5CYII=");
            }
            else
            {
                tex = Editor.ConvertToTexture(
                    "iVBORw0KGgoAAAANSUhEUgAAABAAAAAQCAMAAAAoLQ9TAAADAFBMVEUAAACAAAAAgACAgAAAAICAAIAAgIDAwMDA3MCmyvAKAAALAAAMAAANAAAOAAAPAAAgAABAAABgAAB/AACgAADAAADgAAD+AAD/HBz/ODj/VFT/cHD/jIz/qKj/xMT/4OAAIAAAQAAAYAAAfwAAoAAAwAAA4AAA/gAc/xw4/zhU/1Rw/3CM/4yo/6jE/8Tg/+AAACAAAEAAAGAAAH8AAKAAAMAAAOAAAP4cHP84OP9UVP9wcP+MjP+oqP/ExP/g4P8gIABAQABgYAB/fwCgoADAwADg4AD+/gD//xz//zj//1T//3D//4z//6j//8T//+AAICAAQEAAYGAAf38AoKAAwMAA4OAA/v4c//84//9U//9w//+M//+o///E///g//8gACBAAEBgAGB/AH+gAKDAAMDgAOD+AP7/HP//OP//VP//cP//jP//qP//xP//4P/Pj0/fn1/vr2//v3//yY//05//3a//57+AQACPVx+fbz+vh1+/n3/Pt5/fz7/v598AgP8fj/8/n/9fr/9/v/+fz/+/3//f7/8AQIAfV48/b59fh69/n7+ft8+/z9/f5++AAP+PH/+fP/+vX/+/f//Pn//fv//v3/9AAIBXH49vP5+HX6+ff7+3n8/Pv9/n3++A/wCP/x+f/z+v/1+//3/P/5/f/7/v/99AgABXjx9vnz+Hr1+fv3+3z5/P37/n79//Ziv/djz/hk3/ll//p3D/t4L/x5P/2KWAQICPV4+fb5+vh6+/n7/Pt8/fz9/v5+8A/4Af/48//59f/69//7+f/8+//9/f/+8AgEAfj1c/n29fr4d/v5+fz7e/38/f7+cAUgAkSwBIRQBtPwCROAC2MgDaLAD/JgD/UAD/egD/pAD/zhD/+ED//2///6D//9AQEBAgICAwMDBAQEBQUFBgYGBwcHB/f3+QkJCgoKCwsLDAwMDQ0NDg4ODw8PD+/v74AAD9AAD8AAD7AAD6AAD5AAD/+/CgoKSAgID/AAAA/wD//wAAAP//AP8A///ExMTNPLzbAAAA/nRSTlP/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////ANjZDHEAAAA1SURBVHicY/iLBhiwCPyHAoQAhIUh8BdZAEkTVACi6T+awF/iBf6jCfxFV4FVAOEQbJ5DBQClPv1hGyIw6gAAAABJRU5ErkJggg==");
            }

            TextureCached.Add($"{nameof(IconCopyComponent)}{theme}", tex);
            return tex;
        }

        public static Texture2D IconMoveDown(string theme)
        {
            TextureCached.TryGetValue($"{nameof(IconMoveDown)}{theme}", out var tex);

            if (tex != null) return tex;
            if (theme.Equals("DarkTheme"))
            {
                tex = Editor.ConvertToTexture(
                    "iVBORw0KGgoAAAANSUhEUgAAABAAAAAQCAYAAAAf8/9hAAAACXBIWXMAAAsTAAALEwEAmpwYAAAF8WlUWHRYTUw6Y29tLmFkb2JlLnhtcAAAAAAAPD94cGFja2V0IGJlZ2luPSLvu78iIGlkPSJXNU0wTXBDZWhpSHpyZVN6TlRjemtjOWQiPz4gPHg6eG1wbWV0YSB4bWxuczp4PSJhZG9iZTpuczptZXRhLyIgeDp4bXB0az0iQWRvYmUgWE1QIENvcmUgNi4wLWMwMDIgNzkuMTY0NDg4LCAyMDIwLzA3LzEwLTIyOjA2OjUzICAgICAgICAiPiA8cmRmOlJERiB4bWxuczpyZGY9Imh0dHA6Ly93d3cudzMub3JnLzE5OTkvMDIvMjItcmRmLXN5bnRheC1ucyMiPiA8cmRmOkRlc2NyaXB0aW9uIHJkZjphYm91dD0iIiB4bWxuczp4bXA9Imh0dHA6Ly9ucy5hZG9iZS5jb20veGFwLzEuMC8iIHhtbG5zOnhtcE1NPSJodHRwOi8vbnMuYWRvYmUuY29tL3hhcC8xLjAvbW0vIiB4bWxuczpzdEV2dD0iaHR0cDovL25zLmFkb2JlLmNvbS94YXAvMS4wL3NUeXBlL1Jlc291cmNlRXZlbnQjIiB4bWxuczpkYz0iaHR0cDovL3B1cmwub3JnL2RjL2VsZW1lbnRzLzEuMS8iIHhtbG5zOnBob3Rvc2hvcD0iaHR0cDovL25zLmFkb2JlLmNvbS9waG90b3Nob3AvMS4wLyIgeG1wOkNyZWF0b3JUb29sPSJBZG9iZSBQaG90b3Nob3AgMjIuMCAoV2luZG93cykiIHhtcDpDcmVhdGVEYXRlPSIyMDI0LTAxLTE2VDE0OjEyOjM2KzA3OjAwIiB4bXA6TWV0YWRhdGFEYXRlPSIyMDI0LTAxLTE2VDE0OjEyOjM2KzA3OjAwIiB4bXA6TW9kaWZ5RGF0ZT0iMjAyNC0wMS0xNlQxNDoxMjozNiswNzowMCIgeG1wTU06SW5zdGFuY2VJRD0ieG1wLmlpZDpkNDBiMjg3OC00ZTMyLTc4NGYtYjBhNC01ZWYxMTEzOGI4ZTAiIHhtcE1NOkRvY3VtZW50SUQ9ImFkb2JlOmRvY2lkOnBob3Rvc2hvcDo4ODAwZGVjZC0xYTg4LTUwNDktYWQ5NC1iZmZlZDk4M2ZiOTMiIHhtcE1NOk9yaWdpbmFsRG9jdW1lbnRJRD0ieG1wLmRpZDo3YTk3NjcwYi1jNmYwLTYwNGEtOTg4Ny1jNmQ0ZDNjYWVlMGMiIGRjOmZvcm1hdD0iaW1hZ2UvcG5nIiBwaG90b3Nob3A6Q29sb3JNb2RlPSIzIiBwaG90b3Nob3A6SUNDUHJvZmlsZT0ic1JHQiBJRUM2MTk2Ni0yLjEiPiA8eG1wTU06SGlzdG9yeT4gPHJkZjpTZXE+IDxyZGY6bGkgc3RFdnQ6YWN0aW9uPSJjcmVhdGVkIiBzdEV2dDppbnN0YW5jZUlEPSJ4bXAuaWlkOjdhOTc2NzBiLWM2ZjAtNjA0YS05ODg3LWM2ZDRkM2NhZWUwYyIgc3RFdnQ6d2hlbj0iMjAyNC0wMS0xNlQxNDoxMjozNiswNzowMCIgc3RFdnQ6c29mdHdhcmVBZ2VudD0iQWRvYmUgUGhvdG9zaG9wIDIyLjAgKFdpbmRvd3MpIi8+IDxyZGY6bGkgc3RFdnQ6YWN0aW9uPSJzYXZlZCIgc3RFdnQ6aW5zdGFuY2VJRD0ieG1wLmlpZDpkNDBiMjg3OC00ZTMyLTc4NGYtYjBhNC01ZWYxMTEzOGI4ZTAiIHN0RXZ0OndoZW49IjIwMjQtMDEtMTZUMTQ6MTI6MzYrMDc6MDAiIHN0RXZ0OnNvZnR3YXJlQWdlbnQ9IkFkb2JlIFBob3Rvc2hvcCAyMi4wIChXaW5kb3dzKSIgc3RFdnQ6Y2hhbmdlZD0iLyIvPiA8L3JkZjpTZXE+IDwveG1wTU06SGlzdG9yeT4gPC9yZGY6RGVzY3JpcHRpb24+IDwvcmRmOlJERj4gPC94OnhtcG1ldGE+IDw/eHBhY2tldCBlbmQ9InIiPz5zLE/8AAAAXklEQVQ4jWP8//8/AyWAiSLdDAwMLDBGeHg4uhyy0xiRJVauXEk9F4wagBQLDKihjg7Q5eCxwoRNkABAUYfuBUKGYMhjCwNchmAVxxWI6IpxugxfLDCi0dgVDXhuBAB6FwwmpACldQAAAABJRU5ErkJggg==");
            }
            else
            {
                tex = Editor.ConvertToTexture(
                    "iVBORw0KGgoAAAANSUhEUgAAABAAAAAQCAMAAAAoLQ9TAAADAFBMVEUAAACAAAAAgACAgAAAAICAAIAAgIDAwMDA3MCmyvAKAAALAAAMAAANAAAOAAAPAAAgAABAAABgAAB/AACgAADAAADgAAD+AAD/HBz/ODj/VFT/cHD/jIz/qKj/xMT/4OAAIAAAQAAAYAAAfwAAoAAAwAAA4AAA/gAc/xw4/zhU/1Rw/3CM/4yo/6jE/8Tg/+AAACAAAEAAAGAAAH8AAKAAAMAAAOAAAP4cHP84OP9UVP9wcP+MjP+oqP/ExP/g4P8gIABAQABgYAB/fwCgoADAwADg4AD+/gD//xz//zj//1T//3D//4z//6j//8T//+AAICAAQEAAYGAAf38AoKAAwMAA4OAA/v4c//84//9U//9w//+M//+o///E///g//8gACBAAEBgAGB/AH+gAKDAAMDgAOD+AP7/HP//OP//VP//cP//jP//qP//xP//4P/Pj0/fn1/vr2//v3//yY//05//3a//57+AQACPVx+fbz+vh1+/n3/Pt5/fz7/v598AgP8fj/8/n/9fr/9/v/+fz/+/3//f7/8AQIAfV48/b59fh69/n7+ft8+/z9/f5++AAP+PH/+fP/+vX/+/f//Pn//fv//v3/9AAIBXH49vP5+HX6+ff7+3n8/Pv9/n3++A/wCP/x+f/z+v/1+//3/P/5/f/7/v/99AgABXjx9vnz+Hr1+fv3+3z5/P37/n79//Ziv/djz/hk3/ll//p3D/t4L/x5P/2KWAQICPV4+fb5+vh6+/n7/Pt8/fz9/v5+8A/4Af/48//59f/69//7+f/8+//9/f/+8AgEAfj1c/n29fr4d/v5+fz7e/38/f7+cAUgAkSwBIRQBtPwCROAC2MgDaLAD/JgD/UAD/egD/pAD/zhD/+ED//2///6D//9AQEBAgICAwMDBAQEBQUFBgYGBwcHB/f3+QkJCgoKCwsLDAwMDQ0NDg4ODw8PD+/v74AAD9AAD8AAD7AAD6AAD5AAD/+/CgoKSAgID/AAAA/wD//wAAAP//AP8A///ExMTNPLzbAAAA/nRSTlP/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////ANjZDHEAAAAsSURBVHicY/iLBhhwCPz//5/GAv8RAKoCmQ/RgsSHmoHg47TlL4yP03MIAACn+v1tSDvdKgAAAABJRU5ErkJggg==");
            }

            TextureCached.Add($"{nameof(IconMoveDown)}{theme}", tex);
            return tex;
        }

        public static Texture2D IconMoveUp(string theme)
        {
            TextureCached.TryGetValue($"{nameof(IconMoveUp)}{theme}", out var tex);

            if (tex != null) return tex;
            if (theme.Equals("DarkTheme"))
            {
                tex = Editor.ConvertToTexture(
                    "iVBORw0KGgoAAAANSUhEUgAAABAAAAAQCAYAAAAf8/9hAAAACXBIWXMAAAsTAAALEwEAmpwYAAAF8WlUWHRYTUw6Y29tLmFkb2JlLnhtcAAAAAAAPD94cGFja2V0IGJlZ2luPSLvu78iIGlkPSJXNU0wTXBDZWhpSHpyZVN6TlRjemtjOWQiPz4gPHg6eG1wbWV0YSB4bWxuczp4PSJhZG9iZTpuczptZXRhLyIgeDp4bXB0az0iQWRvYmUgWE1QIENvcmUgNi4wLWMwMDIgNzkuMTY0NDg4LCAyMDIwLzA3LzEwLTIyOjA2OjUzICAgICAgICAiPiA8cmRmOlJERiB4bWxuczpyZGY9Imh0dHA6Ly93d3cudzMub3JnLzE5OTkvMDIvMjItcmRmLXN5bnRheC1ucyMiPiA8cmRmOkRlc2NyaXB0aW9uIHJkZjphYm91dD0iIiB4bWxuczp4bXA9Imh0dHA6Ly9ucy5hZG9iZS5jb20veGFwLzEuMC8iIHhtbG5zOnhtcE1NPSJodHRwOi8vbnMuYWRvYmUuY29tL3hhcC8xLjAvbW0vIiB4bWxuczpzdEV2dD0iaHR0cDovL25zLmFkb2JlLmNvbS94YXAvMS4wL3NUeXBlL1Jlc291cmNlRXZlbnQjIiB4bWxuczpkYz0iaHR0cDovL3B1cmwub3JnL2RjL2VsZW1lbnRzLzEuMS8iIHhtbG5zOnBob3Rvc2hvcD0iaHR0cDovL25zLmFkb2JlLmNvbS9waG90b3Nob3AvMS4wLyIgeG1wOkNyZWF0b3JUb29sPSJBZG9iZSBQaG90b3Nob3AgMjIuMCAoV2luZG93cykiIHhtcDpDcmVhdGVEYXRlPSIyMDI0LTAxLTE2VDE0OjEzOjI0KzA3OjAwIiB4bXA6TWV0YWRhdGFEYXRlPSIyMDI0LTAxLTE2VDE0OjEzOjI0KzA3OjAwIiB4bXA6TW9kaWZ5RGF0ZT0iMjAyNC0wMS0xNlQxNDoxMzoyNCswNzowMCIgeG1wTU06SW5zdGFuY2VJRD0ieG1wLmlpZDo1NWRiMWZjYS1mMmRkLTc1NGEtOTI4Mi0wZDYwYTdmZDk1MWQiIHhtcE1NOkRvY3VtZW50SUQ9ImFkb2JlOmRvY2lkOnBob3Rvc2hvcDo4MGM5Yjk0Ny0zNzA0LWM2NGYtYTE2Yy00Y2M5ZTU3MTk0OTQiIHhtcE1NOk9yaWdpbmFsRG9jdW1lbnRJRD0ieG1wLmRpZDowYjJmMDM3Ny03ZWYyLWY0NGEtYTI2ZC1kN2YwZjc3NDdiYWIiIGRjOmZvcm1hdD0iaW1hZ2UvcG5nIiBwaG90b3Nob3A6Q29sb3JNb2RlPSIzIiBwaG90b3Nob3A6SUNDUHJvZmlsZT0ic1JHQiBJRUM2MTk2Ni0yLjEiPiA8eG1wTU06SGlzdG9yeT4gPHJkZjpTZXE+IDxyZGY6bGkgc3RFdnQ6YWN0aW9uPSJjcmVhdGVkIiBzdEV2dDppbnN0YW5jZUlEPSJ4bXAuaWlkOjBiMmYwMzc3LTdlZjItZjQ0YS1hMjZkLWQ3ZjBmNzc0N2JhYiIgc3RFdnQ6d2hlbj0iMjAyNC0wMS0xNlQxNDoxMzoyNCswNzowMCIgc3RFdnQ6c29mdHdhcmVBZ2VudD0iQWRvYmUgUGhvdG9zaG9wIDIyLjAgKFdpbmRvd3MpIi8+IDxyZGY6bGkgc3RFdnQ6YWN0aW9uPSJzYXZlZCIgc3RFdnQ6aW5zdGFuY2VJRD0ieG1wLmlpZDo1NWRiMWZjYS1mMmRkLTc1NGEtOTI4Mi0wZDYwYTdmZDk1MWQiIHN0RXZ0OndoZW49IjIwMjQtMDEtMTZUMTQ6MTM6MjQrMDc6MDAiIHN0RXZ0OnNvZnR3YXJlQWdlbnQ9IkFkb2JlIFBob3Rvc2hvcCAyMi4wIChXaW5kb3dzKSIgc3RFdnQ6Y2hhbmdlZD0iLyIvPiA8L3JkZjpTZXE+IDwveG1wTU06SGlzdG9yeT4gPC9yZGY6RGVzY3JpcHRpb24+IDwvcmRmOlJERj4gPC94OnhtcG1ldGE+IDw/eHBhY2tldCBlbmQ9InIiPz7WY0p7AAAAXUlEQVQ4je2TwQrAMAhDY+k/+o1+ZXYajEy7sh56aS6KqQ9RaiSxorbUDaDfibtnPgGYFiNiagJKTFUBtKmEZIDqcVpXwNdJXn4bmTOQ/sh12xx46QS/dACAbf+NFxNyFRTRJR1gAAAAAElFTkSuQmCC");
            }
            else
            {
                tex = Editor.ConvertToTexture(
                    "iVBORw0KGgoAAAANSUhEUgAAABAAAAAQCAMAAAAoLQ9TAAADAFBMVEUAAACAAAAAgACAgAAAAICAAIAAgIDAwMDA3MCmyvAKAAALAAAMAAANAAAOAAAPAAAgAABAAABgAAB/AACgAADAAADgAAD+AAD/HBz/ODj/VFT/cHD/jIz/qKj/xMT/4OAAIAAAQAAAYAAAfwAAoAAAwAAA4AAA/gAc/xw4/zhU/1Rw/3CM/4yo/6jE/8Tg/+AAACAAAEAAAGAAAH8AAKAAAMAAAOAAAP4cHP84OP9UVP9wcP+MjP+oqP/ExP/g4P8gIABAQABgYAB/fwCgoADAwADg4AD+/gD//xz//zj//1T//3D//4z//6j//8T//+AAICAAQEAAYGAAf38AoKAAwMAA4OAA/v4c//84//9U//9w//+M//+o///E///g//8gACBAAEBgAGB/AH+gAKDAAMDgAOD+AP7/HP//OP//VP//cP//jP//qP//xP//4P/Pj0/fn1/vr2//v3//yY//05//3a//57+AQACPVx+fbz+vh1+/n3/Pt5/fz7/v598AgP8fj/8/n/9fr/9/v/+fz/+/3//f7/8AQIAfV48/b59fh69/n7+ft8+/z9/f5++AAP+PH/+fP/+vX/+/f//Pn//fv//v3/9AAIBXH49vP5+HX6+ff7+3n8/Pv9/n3++A/wCP/x+f/z+v/1+//3/P/5/f/7/v/99AgABXjx9vnz+Hr1+fv3+3z5/P37/n79//Ziv/djz/hk3/ll//p3D/t4L/x5P/2KWAQICPV4+fb5+vh6+/n7/Pt8/fz9/v5+8A/4Af/48//59f/69//7+f/8+//9/f/+8AgEAfj1c/n29fr4d/v5+fz7e/38/f7+cAUgAkSwBIRQBtPwCROAC2MgDaLAD/JgD/UAD/egD/pAD/zhD/+ED//2///6D//9AQEBAgICAwMDBAQEBQUFBgYGBwcHB/f3+QkJCgoKCwsLDAwMDQ0NDg4ODw8PD+/v74AAD9AAD8AAD7AAD6AAD5AAD/+/CgoKSAgID/AAAA/wD//wAAAP//AP8A///ExMTNPLzbAAAA/nRSTlP/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////ANjZDHEAAAAwSURBVHicY/iLBhhwCfxHE/j//z+KwP//cBEGOB8qwoDgQ0QYkPhgERyG0kYAj28BrQb9bb8ibCAAAAAASUVORK5CYII=");
            }

            TextureCached.Add($"{nameof(IconMoveUp)}{theme}", tex);
            return tex;
        }

        public static Texture2D IconPasteComponentValues(string theme)
        {
            TextureCached.TryGetValue($"{nameof(IconPasteComponentValues)}{theme}", out var tex);

            if (tex != null) return tex;
            if (theme.Equals("DarkTheme"))
            {
                tex = Editor.ConvertToTexture(
                    "iVBORw0KGgoAAAANSUhEUgAAABAAAAAQCAYAAAAf8/9hAAAACXBIWXMAAAsTAAALEwEAmpwYAAAF8WlUWHRYTUw6Y29tLmFkb2JlLnhtcAAAAAAAPD94cGFja2V0IGJlZ2luPSLvu78iIGlkPSJXNU0wTXBDZWhpSHpyZVN6TlRjemtjOWQiPz4gPHg6eG1wbWV0YSB4bWxuczp4PSJhZG9iZTpuczptZXRhLyIgeDp4bXB0az0iQWRvYmUgWE1QIENvcmUgNi4wLWMwMDIgNzkuMTY0NDg4LCAyMDIwLzA3LzEwLTIyOjA2OjUzICAgICAgICAiPiA8cmRmOlJERiB4bWxuczpyZGY9Imh0dHA6Ly93d3cudzMub3JnLzE5OTkvMDIvMjItcmRmLXN5bnRheC1ucyMiPiA8cmRmOkRlc2NyaXB0aW9uIHJkZjphYm91dD0iIiB4bWxuczp4bXA9Imh0dHA6Ly9ucy5hZG9iZS5jb20veGFwLzEuMC8iIHhtbG5zOnhtcE1NPSJodHRwOi8vbnMuYWRvYmUuY29tL3hhcC8xLjAvbW0vIiB4bWxuczpzdEV2dD0iaHR0cDovL25zLmFkb2JlLmNvbS94YXAvMS4wL3NUeXBlL1Jlc291cmNlRXZlbnQjIiB4bWxuczpkYz0iaHR0cDovL3B1cmwub3JnL2RjL2VsZW1lbnRzLzEuMS8iIHhtbG5zOnBob3Rvc2hvcD0iaHR0cDovL25zLmFkb2JlLmNvbS9waG90b3Nob3AvMS4wLyIgeG1wOkNyZWF0b3JUb29sPSJBZG9iZSBQaG90b3Nob3AgMjIuMCAoV2luZG93cykiIHhtcDpDcmVhdGVEYXRlPSIyMDI0LTAxLTE2VDE0OjE4OjMxKzA3OjAwIiB4bXA6TWV0YWRhdGFEYXRlPSIyMDI0LTAxLTE2VDE0OjE4OjMxKzA3OjAwIiB4bXA6TW9kaWZ5RGF0ZT0iMjAyNC0wMS0xNlQxNDoxODozMSswNzowMCIgeG1wTU06SW5zdGFuY2VJRD0ieG1wLmlpZDo5MTlhMmQzZS04Y2E1LWE2NDEtOWFmZC1lODExOWQwYzU0M2IiIHhtcE1NOkRvY3VtZW50SUQ9ImFkb2JlOmRvY2lkOnBob3Rvc2hvcDozZjQxNGM3NS1mMGM5LTk5NGUtODU5OS02ZTBkMTI3Yjk5MzMiIHhtcE1NOk9yaWdpbmFsRG9jdW1lbnRJRD0ieG1wLmRpZDplM2VkYmM3Ny0yNWVlLWY5NDAtYTVjOC1mMzgxZGFmYWFjM2MiIGRjOmZvcm1hdD0iaW1hZ2UvcG5nIiBwaG90b3Nob3A6Q29sb3JNb2RlPSIzIiBwaG90b3Nob3A6SUNDUHJvZmlsZT0ic1JHQiBJRUM2MTk2Ni0yLjEiPiA8eG1wTU06SGlzdG9yeT4gPHJkZjpTZXE+IDxyZGY6bGkgc3RFdnQ6YWN0aW9uPSJjcmVhdGVkIiBzdEV2dDppbnN0YW5jZUlEPSJ4bXAuaWlkOmUzZWRiYzc3LTI1ZWUtZjk0MC1hNWM4LWYzODFkYWZhYWMzYyIgc3RFdnQ6d2hlbj0iMjAyNC0wMS0xNlQxNDoxODozMSswNzowMCIgc3RFdnQ6c29mdHdhcmVBZ2VudD0iQWRvYmUgUGhvdG9zaG9wIDIyLjAgKFdpbmRvd3MpIi8+IDxyZGY6bGkgc3RFdnQ6YWN0aW9uPSJzYXZlZCIgc3RFdnQ6aW5zdGFuY2VJRD0ieG1wLmlpZDo5MTlhMmQzZS04Y2E1LWE2NDEtOWFmZC1lODExOWQwYzU0M2IiIHN0RXZ0OndoZW49IjIwMjQtMDEtMTZUMTQ6MTg6MzErMDc6MDAiIHN0RXZ0OnNvZnR3YXJlQWdlbnQ9IkFkb2JlIFBob3Rvc2hvcCAyMi4wIChXaW5kb3dzKSIgc3RFdnQ6Y2hhbmdlZD0iLyIvPiA8L3JkZjpTZXE+IDwveG1wTU06SGlzdG9yeT4gPC9yZGY6RGVzY3JpcHRpb24+IDwvcmRmOlJERj4gPC94OnhtcG1ldGE+IDw/eHBhY2tldCBlbmQ9InIiPz43MyeMAAAAaElEQVQ4jd2TSw7AIAhES+MdOSOnxFUbHD7WsOusUMMbIEKqenU0noCZ7T1SyR5E5I3vAk6YWFYQuGoSL1ALcI+B3MCqFiI5A6xg64igHSByXaCnLTj9AIBDTD/MF0CWUG4bdbexPYMJPBYSKD9sm/UAAAAASUVORK5CYII=");
            }
            else
            {
                tex = Editor.ConvertToTexture(
                    "iVBORw0KGgoAAAANSUhEUgAAABAAAAAQCAMAAAAoLQ9TAAADAFBMVEUAAACAAAAAgACAgAAAAICAAIAAgIDAwMDA3MCmyvAKAAALAAAMAAANAAAOAAAPAAAgAABAAABgAAB/AACgAADAAADgAAD+AAD/HBz/ODj/VFT/cHD/jIz/qKj/xMT/4OAAIAAAQAAAYAAAfwAAoAAAwAAA4AAA/gAc/xw4/zhU/1Rw/3CM/4yo/6jE/8Tg/+AAACAAAEAAAGAAAH8AAKAAAMAAAOAAAP4cHP84OP9UVP9wcP+MjP+oqP/ExP/g4P8gIABAQABgYAB/fwCgoADAwADg4AD+/gD//xz//zj//1T//3D//4z//6j//8T//+AAICAAQEAAYGAAf38AoKAAwMAA4OAA/v4c//84//9U//9w//+M//+o///E///g//8gACBAAEBgAGB/AH+gAKDAAMDgAOD+AP7/HP//OP//VP//cP//jP//qP//xP//4P/Pj0/fn1/vr2//v3//yY//05//3a//57+AQACPVx+fbz+vh1+/n3/Pt5/fz7/v598AgP8fj/8/n/9fr/9/v/+fz/+/3//f7/8AQIAfV48/b59fh69/n7+ft8+/z9/f5++AAP+PH/+fP/+vX/+/f//Pn//fv//v3/9AAIBXH49vP5+HX6+ff7+3n8/Pv9/n3++A/wCP/x+f/z+v/1+//3/P/5/f/7/v/99AgABXjx9vnz+Hr1+fv3+3z5/P37/n79//Ziv/djz/hk3/ll//p3D/t4L/x5P/2KWAQICPV4+fb5+vh6+/n7/Pt8/fz9/v5+8A/4Af/48//59f/69//7+f/8+//9/f/+8AgEAfj1c/n29fr4d/v5+fz7e/38/f7+cAUgAkSwBIRQBtPwCROAC2MgDaLAD/JgD/UAD/egD/pAD/zhD/+ED//2///6D//9AQEBAgICAwMDBAQEBQUFBgYGBwcHB/f3+QkJCgoKCwsLDAwMDQ0NDg4ODw8PD+/v74AAD9AAD8AAD7AAD6AAD5AAD/+/CgoKSAgID/AAAA/wD//wAAAP//AP8A///ExMTNPLzbAAAA/nRSTlP/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////ANjZDHEAAAA1SURBVHicY/iLBhiwC/wHAlQBMIIJ/IcDmABMO4bAX2QBuB64AIyglgDIeGQBuG/+IwvAAQCk+P1r8jmFfQAAAABJRU5ErkJggg==");
            }

            TextureCached.Add($"{nameof(IconPasteComponentValues)}{theme}", tex);
            return tex;
        }

        public static Texture2D IconRemoveComponent(string theme)
        {
            TextureCached.TryGetValue($"{nameof(IconRemoveComponent)}{theme}", out var tex);

            if (tex != null) return tex;
            if (theme.Equals("DarkTheme"))
            {
                tex = Editor.ConvertToTexture(
                    "iVBORw0KGgoAAAANSUhEUgAAABAAAAAQCAYAAAAf8/9hAAAACXBIWXMAAAsTAAALEwEAmpwYAAAF8WlUWHRYTUw6Y29tLmFkb2JlLnhtcAAAAAAAPD94cGFja2V0IGJlZ2luPSLvu78iIGlkPSJXNU0wTXBDZWhpSHpyZVN6TlRjemtjOWQiPz4gPHg6eG1wbWV0YSB4bWxuczp4PSJhZG9iZTpuczptZXRhLyIgeDp4bXB0az0iQWRvYmUgWE1QIENvcmUgNi4wLWMwMDIgNzkuMTY0NDg4LCAyMDIwLzA3LzEwLTIyOjA2OjUzICAgICAgICAiPiA8cmRmOlJERiB4bWxuczpyZGY9Imh0dHA6Ly93d3cudzMub3JnLzE5OTkvMDIvMjItcmRmLXN5bnRheC1ucyMiPiA8cmRmOkRlc2NyaXB0aW9uIHJkZjphYm91dD0iIiB4bWxuczp4bXA9Imh0dHA6Ly9ucy5hZG9iZS5jb20veGFwLzEuMC8iIHhtbG5zOnhtcE1NPSJodHRwOi8vbnMuYWRvYmUuY29tL3hhcC8xLjAvbW0vIiB4bWxuczpzdEV2dD0iaHR0cDovL25zLmFkb2JlLmNvbS94YXAvMS4wL3NUeXBlL1Jlc291cmNlRXZlbnQjIiB4bWxuczpkYz0iaHR0cDovL3B1cmwub3JnL2RjL2VsZW1lbnRzLzEuMS8iIHhtbG5zOnBob3Rvc2hvcD0iaHR0cDovL25zLmFkb2JlLmNvbS9waG90b3Nob3AvMS4wLyIgeG1wOkNyZWF0b3JUb29sPSJBZG9iZSBQaG90b3Nob3AgMjIuMCAoV2luZG93cykiIHhtcDpDcmVhdGVEYXRlPSIyMDI0LTAxLTE2VDE0OjE5OjEwKzA3OjAwIiB4bXA6TWV0YWRhdGFEYXRlPSIyMDI0LTAxLTE2VDE0OjE5OjEwKzA3OjAwIiB4bXA6TW9kaWZ5RGF0ZT0iMjAyNC0wMS0xNlQxNDoxOToxMCswNzowMCIgeG1wTU06SW5zdGFuY2VJRD0ieG1wLmlpZDozODZjN2IzOC03YTk2LWQ1NGYtOGY2YS04YTFhZGM3Y2RlNjkiIHhtcE1NOkRvY3VtZW50SUQ9ImFkb2JlOmRvY2lkOnBob3Rvc2hvcDpjOWQ3MGI3Yi0xMDkzLWQ4NGEtYTY3NS1jYzI5MGIwMjBlODYiIHhtcE1NOk9yaWdpbmFsRG9jdW1lbnRJRD0ieG1wLmRpZDplOTZmMDdlMC0wN2U0LTAwNGYtYjJiMi02M2YwYzU2Njc0NWIiIGRjOmZvcm1hdD0iaW1hZ2UvcG5nIiBwaG90b3Nob3A6Q29sb3JNb2RlPSIzIiBwaG90b3Nob3A6SUNDUHJvZmlsZT0ic1JHQiBJRUM2MTk2Ni0yLjEiPiA8eG1wTU06SGlzdG9yeT4gPHJkZjpTZXE+IDxyZGY6bGkgc3RFdnQ6YWN0aW9uPSJjcmVhdGVkIiBzdEV2dDppbnN0YW5jZUlEPSJ4bXAuaWlkOmU5NmYwN2UwLTA3ZTQtMDA0Zi1iMmIyLTYzZjBjNTY2NzQ1YiIgc3RFdnQ6d2hlbj0iMjAyNC0wMS0xNlQxNDoxOToxMCswNzowMCIgc3RFdnQ6c29mdHdhcmVBZ2VudD0iQWRvYmUgUGhvdG9zaG9wIDIyLjAgKFdpbmRvd3MpIi8+IDxyZGY6bGkgc3RFdnQ6YWN0aW9uPSJzYXZlZCIgc3RFdnQ6aW5zdGFuY2VJRD0ieG1wLmlpZDozODZjN2IzOC03YTk2LWQ1NGYtOGY2YS04YTFhZGM3Y2RlNjkiIHN0RXZ0OndoZW49IjIwMjQtMDEtMTZUMTQ6MTk6MTArMDc6MDAiIHN0RXZ0OnNvZnR3YXJlQWdlbnQ9IkFkb2JlIFBob3Rvc2hvcCAyMi4wIChXaW5kb3dzKSIgc3RFdnQ6Y2hhbmdlZD0iLyIvPiA8L3JkZjpTZXE+IDwveG1wTU06SGlzdG9yeT4gPC9yZGY6RGVzY3JpcHRpb24+IDwvcmRmOlJERj4gPC94OnhtcG1ldGE+IDw/eHBhY2tldCBlbmQ9InIiPz7RyHfVAAAAXElEQVQ4jWP8//8/AyWAiSLdDAwMLDBGeHg4uhyy0xiRJVauXEm0CxgJyCNcgGYjuhi6HNxgFjTB/wRsxZDH5QV0m3FGFcWxMGoAbgMYcdAYgAWLGEm5i3HAcyMArIoTIGg0o9MAAAAASUVORK5CYII=");
            }
            else
            {
                tex = Editor.ConvertToTexture(
                    "iVBORw0KGgoAAAANSUhEUgAAABAAAAAQCAMAAAAoLQ9TAAADAFBMVEUAAACAAAAAgACAgAAAAICAAIAAgIDAwMDA3MCmyvAKAAALAAAMAAANAAAOAAAPAAAgAABAAABgAAB/AACgAADAAADgAAD+AAD/HBz/ODj/VFT/cHD/jIz/qKj/xMT/4OAAIAAAQAAAYAAAfwAAoAAAwAAA4AAA/gAc/xw4/zhU/1Rw/3CM/4yo/6jE/8Tg/+AAACAAAEAAAGAAAH8AAKAAAMAAAOAAAP4cHP84OP9UVP9wcP+MjP+oqP/ExP/g4P8gIABAQABgYAB/fwCgoADAwADg4AD+/gD//xz//zj//1T//3D//4z//6j//8T//+AAICAAQEAAYGAAf38AoKAAwMAA4OAA/v4c//84//9U//9w//+M//+o///E///g//8gACBAAEBgAGB/AH+gAKDAAMDgAOD+AP7/HP//OP//VP//cP//jP//qP//xP//4P/Pj0/fn1/vr2//v3//yY//05//3a//57+AQACPVx+fbz+vh1+/n3/Pt5/fz7/v598AgP8fj/8/n/9fr/9/v/+fz/+/3//f7/8AQIAfV48/b59fh69/n7+ft8+/z9/f5++AAP+PH/+fP/+vX/+/f//Pn//fv//v3/9AAIBXH49vP5+HX6+ff7+3n8/Pv9/n3++A/wCP/x+f/z+v/1+//3/P/5/f/7/v/99AgABXjx9vnz+Hr1+fv3+3z5/P37/n79//Ziv/djz/hk3/ll//p3D/t4L/x5P/2KWAQICPV4+fb5+vh6+/n7/Pt8/fz9/v5+8A/4Af/48//59f/69//7+f/8+//9/f/+8AgEAfj1c/n29fr4d/v5+fz7e/38/f7+cAUgAkSwBIRQBtPwCROAC2MgDaLAD/JgD/UAD/egD/pAD/zhD/+ED//2///6D//9AQEBAgICAwMDBAQEBQUFBgYGBwcHB/f3+QkJCgoKCwsLDAwMDQ0NDg4ODw8PD+/v74AAD9AAD8AAD7AAD6AAD5AAD/+/CgoKSAgID/AAAA/wD//wAAAP//AP8A///ExMTNPLzbAAAA/nRSTlP/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////ANjZDHEAAAAuSURBVHicY/iLBhhwCPz//x9N4C+SwH8EgKqASf7/CxeAQdoKoFuL5C6cnkMAAK0+/XfzoLuqAAAAAElFTkSuQmCC");
            }

            TextureCached.Add($"{nameof(IconRemoveComponent)}{theme}", tex);
            return tex;
        }

        public static TextAsset ScriptableEventListenerTemplate =>
            ProjectDatabase.FindAssetWithPath<TextAsset>("ScriptableEventListenerTemplate.cs.txt", RELATIVE_TEMPLATE_PATH);

        public static TextAsset ScreenViewTemplate => ProjectDatabase.FindAssetWithPath<TextAsset>("ScreenViewTemplate.cs.txt", RELATIVE_TEMPLATE_PATH);
        public static TextAsset ScreenPresenterTemplate => ProjectDatabase.FindAssetWithPath<TextAsset>("ScreenPresenterTemplate.cs.txt", RELATIVE_TEMPLATE_PATH);

        public static TextAsset ScriptableEventTemplate => ProjectDatabase.FindAssetWithPath<TextAsset>("ScriptableEventTemplate.cs.txt", RELATIVE_TEMPLATE_PATH);
        public static TextAsset ScriptableListTemplate => ProjectDatabase.FindAssetWithPath<TextAsset>("ScriptableListTemplate.cs.txt", RELATIVE_TEMPLATE_PATH);
        public static TextAsset ScriptableVariableTemplate => ProjectDatabase.FindAssetWithPath<TextAsset>("ScriptableVariableTemplate.cs.txt", RELATIVE_TEMPLATE_PATH);
        public static TextAsset MonoBehaviourTemplate => ProjectDatabase.FindAssetWithPath<TextAsset>("MonoBehaviourTemplate.cs.txt", RELATIVE_TEMPLATE_PATH);
        public static TextAsset ClassTemplate => ProjectDatabase.FindAssetWithPath<TextAsset>("ClassTemplate.cs.txt", RELATIVE_TEMPLATE_PATH);
        public static TextAsset AndroidManifestTemplate => ProjectDatabase.FindAssetWithPath<TextAsset>("AndroidManifest.xml.txt", RELATIVE_TEMPLATE_PATH);
        public static TextAsset MainGradleTemplate => ProjectDatabase.FindAssetWithPath<TextAsset>("mainTemplate.txt", RELATIVE_TEMPLATE_PATH);
        public static TextAsset GradlePropertiesTemplate => ProjectDatabase.FindAssetWithPath<TextAsset>("gradleTemplate.txt", RELATIVE_TEMPLATE_PATH);
        public static TextAsset GradleSettingsTemplate => ProjectDatabase.FindAssetWithPath<TextAsset>("settingsTemplate.txt", RELATIVE_TEMPLATE_PATH);
    }
}