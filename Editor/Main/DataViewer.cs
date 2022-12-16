using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Pancake.Linq;
using UnityEditor;
using UnityEngine;

namespace Pancake.Editor
{
    public class DataViewer : EditorWindow
    {
        public List<int> profiles;
        private int _currentProfile;
        private string _path;
        private Vector2 _scrollPosition;
        private Dictionary<string, Data.DataSegment> _datas;

        private static readonly Color[] ColorSchemas = new[] {new Color(0.43f, 1f, 0.33f), new Color(1f, 0.71f, 0.36f), new Color(1f, 0.33f, 0.14f)};

        private void OnEnable() { LoadProfiles(); }

        private void OnDisable()
        {
            _datas?.Clear();
            profiles?.Clear();
            _datas = null;
            profiles = null;
        }

        private void OnGUI()
        {
            Uniform.SpaceOneLine();
            InternalDrawDataView();
        }

        private void InternalDrawDataView()
        {
            Uniform.DrawGroupFoldoutWithRightClick("DATA_VIEWER_DRAW", "DATA", View, Refresh);

            void View()
            {
                Uniform.Vertical(() =>
                {
                    _currentProfile = EditorGUILayout.Popup("Select Profile", _currentProfile, profiles.Map(_ => $"Profile {_}").ToArray());
                    if (EditorGUI.EndChangeCheck())
                    {
                        _path = Path.Combine(Application.persistentDataPath, $"masterdata_{profiles[_currentProfile]}.data");
                    }
                });

                Uniform.SpaceThreeLine();

                Uniform.Vertical(() =>
                {
                    byte[] bytes = File.ReadAllBytes(_path);
                    _datas = Deserialize<Dictionary<string, Data.DataSegment>>(bytes);

                    long totalSize = 0;
                    foreach (var dataSegment in _datas)
                    {
                        totalSize += dataSegment.Value.value.Length;
                    }

                    if (_datas.Count == 0)
                    {
                        Uniform.HelpBox("Profile Empty!", MessageType.Info);
                    }
                    else
                    {
                        Uniform.Horizontal(() =>
                        {
                            EditorGUILayout.LabelField("Key", GUILayout.Width(150));
                            EditorGUILayout.LabelField("Size");
                            GUILayout.FlexibleSpace();
                        });
                        Uniform.SpaceOneLine();
                        _scrollPosition = GUILayout.BeginScrollView(_scrollPosition, GUILayout.Height(position.height - 108));
                        foreach (var dataSegment in _datas.ToArray())
                        {
                            Uniform.Horizontal(() =>
                            {
                                EditorGUILayout.LabelField(dataSegment.Key, GUILayout.Width(120));
                                var rect = GUILayoutUtility.GetLastRect();
                                var percent = dataSegment.Value.value.Length / (float) totalSize;

                                Color color;
                                if (percent <= 0.33f)
                                {
                                    color = ColorSchemas[0];
                                }
                                else if (percent <= 0.66)
                                {
                                    color = ColorSchemas[1];
                                }
                                else
                                {
                                    color = ColorSchemas[2];
                                }

                                var size = position.width - 200;
                                var pos = new Vector2(rect.position.x + 150, rect.position.y);
                                var fillRect = new Rect(pos, new Vector2(size * percent, rect.height));
                                EditorGUI.DrawRect(new Rect(pos, new Vector2(size, rect.height)), new Color(0.13f, 0.13f, 0.13f));
                                EditorGUI.DrawRect(fillRect, color);

                                // set alignment and cache the default
                                var align = GUI.skin.label.alignment;
                                GUI.skin.label.alignment = TextAnchor.UpperCenter;

                                // set the color and cache the default
                                var c = GUI.contentColor;
                                GUI.contentColor = Color.white;

                                // calculate the position
                                var labelRect = new Rect(position.width / 2, rect.y - 2, rect.width, rect.height);

                                EditorGUI.DropShadowLabel(labelRect, InEditor.GetSizeInMemory(dataSegment.Value.value.Length));

                                // reset color and alignment
                                GUI.contentColor = c;
                                GUI.skin.label.alignment = align;

                                GUILayout.FlexibleSpace();

                                if (GUILayout.Button(Uniform.IconContent("d_TreeEditor.Trash", "Uninstall"), GUILayout.Width(35)))
                                {
                                    if (EditorUtility.DisplayDialog("Remove Data",
                                            $"Are you sure you wish to delete data of {dataSegment.Key}?\nThis action cannot be reversed.",
                                            "Remove",
                                            "Cancel"))
                                    {
                                        _datas.Remove(dataSegment.Key);
                                        byte[] bytes = OdinSerializer.SerializationUtility.SerializeValue(_datas, OdinSerializer.DataFormat.Binary);
                                        File.WriteAllBytes(_path, bytes);
                                    }
                                }
                            });
                        }

                        GUILayout.EndScrollView();
                    }

                    Uniform.SpaceTwoLine();
                });
            }

            void Refresh()
            {
                var menu = new GenericMenu();
                menu.AddItem(new GUIContent("Refresh Profile"), false, LoadProfiles);
                menu.ShowAsContext();
            }
        }

        private static T Deserialize<T>(byte[] bytes)
        {
            var data = OdinSerializer.SerializationUtility.DeserializeValue<T>(bytes, OdinSerializer.DataFormat.Binary);
            return data;
        }

        private void LoadProfiles()
        {
            profiles = new List<int>();

            string[] results = Directory.GetFiles(Application.persistentDataPath, "masterdata_*.data");

            foreach (string fileName in results)
            {
                string str = fileName.Split('.')[0].Split('_')[1];

                try
                {
                    int profile = int.Parse(str);
                    profiles.Add(profile);
                }
                catch (Exception)
                {
                    //
                }
            }
        }
    }
}