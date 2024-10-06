using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEngine;
using Event = UnityEngine.Event;

namespace Sisus.Init.EditorOnly.Internal
{
	internal sealed class TypeDropdownWindow : EditorWindow
	{
		private static class Styles
		{
			public static GUIStyle background = "grey_border";
			public static GUIStyle previewHeader = new GUIStyle(EditorStyles.label);
			public static GUIStyle previewText = new GUIStyle(EditorStyles.wordWrappedLabel);

			static Styles()
			{
				previewText.padding.left += 3;
				previewText.padding.right += 3;
				previewHeader.padding.left += 3 - 2;
				previewHeader.padding.right += 3;
				previewHeader.padding.top += 3;
				previewHeader.padding.bottom += 2;
			}
		}

		internal static TypeDropdownWindow instance = null;
		internal static Func<Type, (string fullPath, Texture icon)> itemContentGetter = x => ("", null);
		internal static IEnumerable<Type> types = Array.Empty<Type>();
		internal static IEnumerable<Type> selectedTypes = Array.Empty<Type>();
		internal static string menuTitle;
		internal static Action<Type> onTypeSelected;
		public AdvancedDropdownDataSource dataSource = new TypeDataSource(Array.Empty<Type>(), Array.Empty<Type>(), "", x => ("", null));

		private AdvancedDropdownGUI gui = new AdvancedDropdownGUI();
		private AdvancedDropdownItem currentlyRenderedTree;
		private string search = "";

		private AdvancedDropdownItem animationTree;
		private float newAnimTarget = 0;
		private long ticksLastFrame = 0;
		private bool scrollToSelected = true;

		[NonSerialized]
		private bool dirtyList = true;

		public bool ShowHeader { get; set; } = true;

		private bool HasSearch => !string.IsNullOrEmpty(search);

		private void OnEnable()
		{
			dirtyList = true;
			dataSource = new TypeDataSource(types, selectedTypes, menuTitle, itemContentGetter);
			instance = this;
			ShowHeader = true;
		}

		private void OnDisable() => instance = null;

		internal static bool Show(Rect rect, IEnumerable<Type> types, IEnumerable<Type> selectedTypes, Action<Type> onTypeSelected, string menuTitle, Func<Type, (string fullPath, Texture icon)> itemContentGetter = null)
		{
			CloseAllOpenWindows();

			if(Event.current != null && Event.current.type != EventType.Layout && Event.current.type != EventType.Repaint)
			{
				Event.current.Use();
			}

			TypeDropdownWindow.itemContentGetter = itemContentGetter;
			TypeDropdownWindow.types = types;
			TypeDropdownWindow.selectedTypes = selectedTypes;
			TypeDropdownWindow.menuTitle = menuTitle;
			TypeDropdownWindow.onTypeSelected = onTypeSelected;

			if(rect.width < 200f)
			{
				rect.width = 200f;
			}

			instance = CreateAndInit(rect);
			return true;
		}

		public void OnTypeSelected(Type type)
		{
			onTypeSelected?.Invoke(type);
			Close();
		}

		private static TypeDropdownWindow CreateAndInit(Rect rect)
		{
			var window = CreateInstance<TypeDropdownWindow>();
			window.Init(rect);
			return window;
		}

		private void Init(Rect buttonRect)
		{
			buttonRect = GUIUtility.GUIToScreenRect(buttonRect);
			OnDirtyList();
			currentlyRenderedTree = HasSearch ? dataSource.searchTree : dataSource.mainTree;
			float minHeight = currentlyRenderedTree.Children.Count == 0 || currentlyRenderedTree.Children.Any(c => c.HasChildren)
							? AdvancedDropdownGUI.WindowHeight
							: 58f + currentlyRenderedTree.Children.Count * currentlyRenderedTree.Children[0].lineStyle.CalcHeight(GUIContent.none, 0f);
			float height = Mathf.Min(minHeight, AdvancedDropdownGUI.WindowHeight);
			ShowAsDropDown(buttonRect, new Vector2(buttonRect.width, height));
			Focus();
			wantsMouseMove = true;
		}

		internal void OnGUI()
		{
			GUI.Label(new Rect(0f, 0f, Screen.width, position.height), GUIContent.none, Styles.background);

			if(dirtyList)
			{
				OnDirtyList();
			}

			HandleKeyboard();
			OnGUISearch();

			if(newAnimTarget != 0 && Event.current.type == EventType.Layout)
			{
				long now = DateTime.Now.Ticks;
				float deltaTime = (now - ticksLastFrame) / (float)TimeSpan.TicksPerSecond;
				ticksLastFrame = now;

				newAnimTarget = Mathf.MoveTowards(newAnimTarget, 0, deltaTime * 4);

				if(newAnimTarget == 0)
				{
					animationTree = null;
				}
				Repaint();
			}

			var anim = newAnimTarget;
			anim = Mathf.Floor(anim) + Mathf.SmoothStep(0, 1, Mathf.Repeat(anim, 1));

			if(anim == 0)
			{
				DrawDropdown(0, currentlyRenderedTree);
			}
			else if(anim < 0)
			{
				DrawDropdown(anim, currentlyRenderedTree);
				DrawDropdown(anim + 1, animationTree);
			}
			else
			{
				DrawDropdown(anim - 1, animationTree);
				DrawDropdown(anim, currentlyRenderedTree);
			}
		}

		private void OnDirtyList()
		{
			dirtyList = false;
			dataSource.ReloadData();

			if(HasSearch)
			{
				dataSource.RebuildSearch(search);
			}
		}

		private void OnGUISearch()
		{
			gui.DrawSearchField(false, search, (newSearch) =>
			{
				dataSource.RebuildSearch(newSearch);
				currentlyRenderedTree =
					string.IsNullOrEmpty(newSearch) ? dataSource.mainTree : dataSource.searchTree;
				search = newSearch;
			});
		}

		private void HandleKeyboard()
		{
			var e = Event.current;
			if(e.type != EventType.KeyDown)
			{
				return;
			}

			switch(e.keyCode)
			{
				case KeyCode.DownArrow:
					currentlyRenderedTree.MoveDownSelection();
					scrollToSelected = true;
					e.Use();
					return;
				case KeyCode.UpArrow:
					currentlyRenderedTree.MoveUpSelection();
					scrollToSelected = true;
					e.Use();
					return;
				case KeyCode.Return:
				case KeyCode.KeypadEnter:
					e.Use();

					if(currentlyRenderedTree.GetSelectedChild().Children.Any())
					{
						GoToChild(currentlyRenderedTree);
						return;
					}

					if(currentlyRenderedTree.GetSelectedChild().OnAction())
					{
						Close();
					}
					return;
			}

			if(HasSearch)
			{
				return;
			}

			switch(e.keyCode)
			{
				case KeyCode.LeftArrow:
				case KeyCode.Backspace:
					GoToParent();
					e.Use();
					return;
				case KeyCode.RightArrow:
					if(currentlyRenderedTree.GetSelectedChild().Children.Any())
					{
						GoToChild(currentlyRenderedTree);
					}
					e.Use();
					return;
				case KeyCode.Escape:
					Close();
					e.Use();
					return;
			}
		}

		private void DrawDropdown(float anim, AdvancedDropdownItem group)
		{
			var areaPosition = position;
			areaPosition.height -= 1f;
			GUILayout.BeginArea(gui.GetAnimRect(areaPosition, anim));

			if(ShowHeader)
			{
				gui.DrawHeader(group, GoToParent);
			}

			DrawList(group);
			GUILayout.EndArea();
		}

		private void DrawList(AdvancedDropdownItem item)
		{
			item.scrollPosition = GUILayout.BeginScrollView(item.scrollPosition);
			EditorGUIUtility.SetIconSize(gui.IconSize);
			Rect selectedRect = new Rect();
			for(var i = 0; i < item.Children.Count; i++)
			{
				var child = item.Children[i];
				bool selected = i == item.selectedItem;
				gui.DrawItem(child, selected, HasSearch);
				var rect = GUILayoutUtility.GetLastRect();
				if(selected)
				{
					selectedRect = rect;
				}

				if((Event.current.type == EventType.MouseMove || Event.current.type == EventType.MouseDown) && !selected && rect.Contains(Event.current.mousePosition))
				{
					item.selectedItem = i;
					Event.current.Use();
				}

				if(Event.current.type == EventType.MouseDown && rect.Contains(Event.current.mousePosition))
				{
					item.selectedItem = i;
					if(currentlyRenderedTree.GetSelectedChild().Children.Any())
					{
						GoToChild(currentlyRenderedTree);
					}
					else if(currentlyRenderedTree.GetSelectedChild().OnAction())
					{
						Close();
						LayoutUtility.ExitGUI();
					}

					Event.current.Use();
				}
			}

			EditorGUIUtility.SetIconSize(Vector2.zero);

			GUILayout.EndScrollView();

			if(scrollToSelected && Event.current.type == EventType.Repaint)
			{
				scrollToSelected = false;
				Rect scrollRect = GUILayoutUtility.GetLastRect();
				if(selectedRect.yMax - scrollRect.height > item.scrollPosition.y)
				{
					item.scrollPosition.y = selectedRect.yMax - scrollRect.height;
					Repaint();
				}

				if(selectedRect.y < item.scrollPosition.y)
				{
					item.scrollPosition.y = selectedRect.y;
					Repaint();
				}
			}
		}

		private void GoToParent()
		{
			if(currentlyRenderedTree.Parent == null)
			{
				return;
			}
				
			ticksLastFrame = DateTime.Now.Ticks;
			newAnimTarget = newAnimTarget > 0 ? newAnimTarget - 1 : -1;
			animationTree = currentlyRenderedTree;
			currentlyRenderedTree = currentlyRenderedTree.Parent;
		}

		private void GoToChild(AdvancedDropdownItem parent)
		{
			ticksLastFrame = DateTime.Now.Ticks;
			newAnimTarget = newAnimTarget < 0 ? newAnimTarget + 1 : 1;
			currentlyRenderedTree = parent.GetSelectedChild();
			animationTree = parent;
		}

		[DidReloadScripts]
		private static void OnScriptReload() => CloseAllOpenWindows();

		private static void CloseAllOpenWindows()
		{
			foreach(var window in Resources.FindObjectsOfTypeAll<TypeDropdownWindow>())
			{
				try
				{
					window.Close();
				}
				catch
				{
					try
					{
						DestroyImmediate(window);
					}
					catch
					{

					}
				}
			}
		}
	}
}