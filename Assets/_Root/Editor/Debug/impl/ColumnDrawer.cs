//#define DEBUG_SET_COLUMN_WIDTH
//#define DEBUG_COLUMN_REORDERING

using UnityEngine;

namespace Pancake.Debugging.Console
{
	using System;
	using System.Collections.Generic;
	using UnityEditor;
	using JetBrains.Annotations;

	[Serializable]
	public class ColumnDrawer
	{
		public delegate void OnItemClickedEventHandler(int columnId, int rowIndex, int clickCount, Rect drawRect);

		public const float RowHeight = 25f;

		private const float PreferredMinColumnWidthInPixels = 30f;

		[SerializeField]
		private List<ColumnData> columns = new List<ColumnData>();

		[NonSerialized]
		private List<ColumnData> visibleColumns = new List<ColumnData>();

		[NonSerialized]
		private Color lineColor;
		[NonSerialized]
		private Color lineColorSubtle;

		[NonSerialized]
		private GUIStyle stackTraceColumnHeaderStyle;
		[NonSerialized]
		private GUIStyle stackTraceEntryStyle;

		private event OnItemClickedEventHandler OnItemClicked;

		[NonSerialized]
		private ColumnData resizing;
		[NonSerialized]
		private ColumnData reordering;
		[NonSerialized]
		private int reorderingOverVisibleDropIndex;

		[SerializeField]
		private bool optimizeColumnsWidthsContinuously = true;
		[NonSerialized]
		private float columnWidthsOptimizedForWindowWidth;
		private bool columnsAreUnoptimized = true;

		public float Height
		{
			get
			{
				return (RowCount + 1) * RowHeight;
			}
		}

		public int RowCount
		{
			get
			{
				return columns.Count == 0 ? 0 : columns[0].contents.Count;
			}
		}

		public int VisibleColumnCount
		{
			get
			{
				return visibleColumns.Count;
			}
		}

		public int ColumnCount
		{
			get
			{
				return columns.Count;
			}
		}

		public int GetVisibleColumnAtIndex(int visibleColumnIndex)
        {
			return visibleColumnIndex < 0 || visibleColumnIndex >= visibleColumns.Count ? default : visibleColumns[visibleColumnIndex].id;
        }

		public float GetColumnWith(int columnId)
        {
			var data = GetColumnById(columnId);
			if(data == null)
			{ 
				#if DEV_MODE
				Debug.LogWarning("ColumnDrawer.GetColumnWith failed to find column by id " + columnId + ".");
				#endif
				return 0f;
			}
			return data.width;
        }

		public void SetColumnWith(int columnId, float width)
		{
			var data = GetColumnById(columnId);
			if(data == null)
            {
				#if DEV_MODE
				Debug.LogWarning("ColumnDrawer.SetColumnWith failed to find column by id " + columnId + ".");
				#endif
				return;
            }
			data.width = width;
		}

		public GUIContent GetContent(int columnId, int rowIndex)
		{
			if(rowIndex < 0)
			{
				#if DEV_MODE
				Debug.LogWarning("ColumnDrawer.GetContent called with rowIndex " + rowIndex + " that was less than zero.");
				#endif
				return GUIContent.none;
			}

			var column = GetColumnById(columnId);
			if(column == null)
			{
				return GUIContent.none;
			}

			if(column.contents.Count <= rowIndex)
			{
				#if DEV_MODE
				Debug.LogWarning("ColumnDrawer.GetContent called with rowIndex " + rowIndex + " but contents count was only "+column.contents.Count);
				#endif
				return GUIContent.none;
			}

			return column.contents[rowIndex].label;
		}

		public int GetNextVisibleColumnId(int startId, bool loopBack)
        {
			for(int n = 0, count = visibleColumns.Count; n < count; n++)
			{
				var column = visibleColumns[n];
				if(column.id != startId)
				{
					continue;
				}

				int next = n + 1;
				if(next < count)
				{
					return visibleColumns[next].id;
				}
				if(!loopBack)
				{
					return default;
				}
				return visibleColumns[0].id;
			}
			return default;
		}

		public int GetPreviousVisibleColumnId(int startId, bool loopBack)
        {
			for(int n = 0, count = visibleColumns.Count; n < count; n++)
			{
				var column = visibleColumns[n];
				if(column.id != startId)
				{
					continue;
				}

				int prev = n - 1;
				if(prev >= 0)
				{
					return visibleColumns[prev].id;
				}
				if(!loopBack)
				{
					return default;
				}
				return visibleColumns[count - 1].id;
			}
			return default;
		}

		public ColumnDrawer() { }

		public void ResetContents()
		{
			for(int n = columns.Count - 1; n >= 0; n--)
			{
				columns[n].contents.Clear();
			}
			GUI.changed = true;
		}

		public void OnEnable([NotNull]GUIStyle textAreaStyle, Color lineColor, OnItemClickedEventHandler OnItemClicked)
		{
			this.lineColor = lineColor;
			lineColorSubtle = lineColor;
			lineColorSubtle.a = 0.5f;

			stackTraceEntryStyle = new GUIStyle(textAreaStyle);
			stackTraceEntryStyle.wordWrap = false;
			stackTraceEntryStyle.alignment = TextAnchor.MiddleLeft;

			stackTraceColumnHeaderStyle = new GUIStyle();
			stackTraceColumnHeaderStyle.fontStyle = FontStyle.Bold;
			stackTraceColumnHeaderStyle.alignment = TextAnchor.MiddleLeft;
			stackTraceColumnHeaderStyle.wordWrap = false;
			stackTraceColumnHeaderStyle.clipping = TextClipping.Clip;
			stackTraceColumnHeaderStyle.padding = new RectOffset(5, 5, -1, 0);

			this.OnItemClicked = OnItemClicked;

			RebuildVisibleColumns();
		}

		public void AddColumn(int columnId, string name, string tooltip, float initialWidth, bool autoAdjustWidth, bool shown, string nameInMenus)
		{
			columns.Add(new ColumnData(columnId, name, tooltip, initialWidth, autoAdjustWidth, shown, nameInMenus));
		}

		public void AddContent(int columnId, string text, string tooltip, OnGUIEvent overrideOnGUI = null)
		{
			var column = GetColumnById(columnId);
			if(column != null)
			{
				column.contents.Add(new ColumnContent(text, tooltip, overrideOnGUI));
			}
			#if DEV_MODE
			else { Debug.LogWarning("ColumnDrawer.AddContent - failed to find column by id " + columnId); }
			#endif

			columnsAreUnoptimized = true;
			GUI.changed = true;
		}

		[CanBeNull]
		private ColumnData GetColumnById(int columnId)
		{
			for(int n = columns.Count - 1; n >= 0; n--)
			{
				if(columns[n].id == columnId)
				{
					return columns[n];
				}
			}
			return null;
		}

		public void OnFinishedBuildingColumns()
		{
			columnsAreUnoptimized = true;
			GUI.changed = true;

			RebuildVisibleColumns();
			UpdateColumnWidths();
		}

		private void UpdateColumnWidths()
		{
			int count = visibleColumns.Count;
			if(count == 0)
			{
				return;
			}

			float setWidth = 1f / count;
			float totalWidth = 0f;
			for(int n = count - 2; n >= 0; n--)
			{
				var column = visibleColumns[n];
				totalWidth += column.width;
			}

			if(totalWidth < 1f)
			{
				visibleColumns[count - 1].width = 1f - totalWidth;
			}
			else
			{
				for(int n = count - 1; n >= 0; n--)
				{
					var column = visibleColumns[n];
					column.width = setWidth;
				}
			}
		}

		public void OnGUI(Rect position, int clickCount)
		{
			if(position.width <= 5f || position.height <= 5f)
			{
				return;
			}

			int visibleColumnCount = visibleColumns.Count;
			if(visibleColumnCount == 0)
			{
				RebuildVisibleColumns();
				if(visibleColumnCount == 0)
				{
					return;
				}
			}

			#if DEV_MODE // Dev mode only: allow toggling optimizeColumnsWidthsContinuously on/off with ctrl + shift + A
			if(Event.current.type == EventType.KeyDown && Event.current.keyCode == KeyCode.A && Event.current.shift && Event.current.control)
			{
				optimizeColumnsWidthsContinuously = !optimizeColumnsWidthsContinuously;
				Debug.Log("autoOptimizeConstantly = "+ optimizeColumnsWidthsContinuously);
			}
			#endif

			if(optimizeColumnsWidthsContinuously && (columnWidthsOptimizedForWindowWidth != position.width || columnsAreUnoptimized) && resizing == null)
			{
				OptimizeAllColumnsWidths(position.width);
			}

			var headerRowRect = position;
			headerRowRect.height = RowHeight;

			var colorBgRect = headerRowRect;
			colorBgRect.y -= 2f;
			EditorGUI.DrawRect(colorBgRect, lineColorSubtle);

			float rowWidth = headerRowRect.width;

			var tooltipLabel = new GUIContent("");
			var verticalDividerRect = position;
			verticalDividerRect.y -= 2f;
			verticalDividerRect.width = 1f;
			verticalDividerRect.x -= 1f;
			int contentRowCount = visibleColumns[0].contents.Count;
			int totalRowCount = contentRowCount + 1;
			verticalDividerRect.height = RowHeight * totalRowCount;
			var drawRect = headerRowRect;

			for(int c = 0; c < visibleColumnCount; c++)
			{
				var column = visibleColumns[c];

				float widthPixels = column.width * rowWidth;
				drawRect.width = widthPixels;
				verticalDividerRect.x += widthPixels;

				HandleReordering(drawRect, headerRowRect, column, clickCount);

				if(clickCount == 1 && Event.current.rawType == EventType.MouseUp && drawRect.Contains(Event.current.mousePosition))
				{
					OnHeaderClicked(column);
					GUIUtility.ExitGUI();
				}

				GUI.Label(drawRect, column.label, stackTraceColumnHeaderStyle);

				var rowContents = column.contents;

				if(c < visibleColumnCount - 1)
				{
					HandleResizing(position.width, verticalDividerRect, column, drawRect.x, clickCount);
				}

				for(int r = 0, rowCount = rowContents.Count; r < rowCount; r++)
				{
					drawRect.y += RowHeight;

					var content = rowContents[r];

					if(clickCount == 1 && Event.current.rawType == EventType.MouseUp && drawRect.Contains(Event.current.mousePosition))
					{
						if(OnItemClicked != null)
						{
							OnItemClicked(column.id, r, clickCount, drawRect);
							GUIUtility.ExitGUI();
						}

						if(Event.current.button == 1 && !EditorGUIUtility.textFieldHasSelection)
						{
							var menu = new GenericMenu();

							bool showMenu = false;
							if(content.label.text.Length > 0)
							{
								menu.AddItem(new GUIContent("Copy"), false, ()=> EditorGUIUtility.systemCopyBuffer = content.label.text);
								showMenu = true;
							}
							if(content.label.tooltip.Length > 0 && content.label.tooltip != content.label.text)
							{
								menu.AddItem(new GUIContent("Copy Tooltip"), false, ()=> EditorGUIUtility.systemCopyBuffer = content.label.tooltip);
								showMenu = true;
							}

							if(showMenu)
							{
								if(Event.current.type == EventType.MouseUp)
								{
									Event.current.Use();
								}
								menu.ShowAsContext();
								GUIUtility.ExitGUI();
							}
						}
					}
					
					tooltipLabel.tooltip = content.label.tooltip;
					GUI.Label(drawRect, tooltipLabel);
					if(content.overrideOnGUI != null && !content.overrideOnGUI.IsEmpty)
					{
						content.overrideOnGUI.Invoke(drawRect, c, r, clickCount);
					}
					else
					{
						EditorGUI.SelectableLabel(drawRect, content.label.text, stackTraceEntryStyle);
					}
				}

				if(c < visibleColumnCount - 1)
				{
					EditorGUI.DrawRect(verticalDividerRect, lineColor);
				}

				drawRect.y = position.y;
				drawRect.x += widthPixels;
			}

			var horizontalDividerRect = position;
			horizontalDividerRect.y -= 3f;
			horizontalDividerRect.height = 1f;
			for(int c = 0; c <= totalRowCount; c++)
			{
				EditorGUI.DrawRect(horizontalDividerRect, lineColorSubtle);
				horizontalDividerRect.y += RowHeight;
			}
		}

		private void OptimizeAllColumnsWidths(float windowWidth)
		{
			columnsAreUnoptimized = false;
			columnWidthsOptimizedForWindowWidth = windowWidth;

			int count = visibleColumns.Count;
			if(count == 0)
			{
				return;
			}

			float[] optimalSpaces = new float[count];
			float totalOptimalSpace = 0f;

			int lastIndex = count - 1;
			for(int n = 0; n < lastIndex; n++)
			{
				float optimalSpace = GetOptimalColumnWidth(visibleColumns[n], windowWidth);
				optimalSpaces[n] = optimalSpace;
				totalOptimalSpace += optimalSpace;
			}

			float remainingSpace = 1f - totalOptimalSpace;
			// If there isn't enough space to optimize all column widths then leave them basically as they were,
			// just ensure they are not less wide than minimum accepted width.
			if(remainingSpace < GetOptimalColumnWidth(visibleColumns[lastIndex], windowWidth))
			{
				EnsureAllColumnsAreWideEnough(windowWidth);
				return;
			}

			for(int n = 0; n < lastIndex; n++)
			{
				visibleColumns[n].width = optimalSpaces[n];
			}
			visibleColumns[lastIndex].width = remainingSpace;
		}

		private void EnsureAllColumnsAreWideEnough(float windowWidth)
		{
			int count = visibleColumns.Count;
			if(count == 0)
			{
				return;
			}

			for(int n = 0; n < count; n++)
			{
				var column = visibleColumns[n];
				float minWidth = GetMinColumnWidth(column, windowWidth);
				if(column.width < minWidth)
				{
					SetColumnWidth(column, minWidth, true, windowWidth);
				}
			}

			EnsureColumnWidthsAddUpToOne();
		}

		private void HandleReordering(Rect drawRect, Rect headerRowRect, ColumnData column, int clickCount)
		{
			if(visibleColumns.Count <= 1)
			{
				reordering = null;
				reorderingOverVisibleDropIndex = -1;
				return;
			}

			var e = Event.current;

			if(e.rawType == EventType.MouseUp && e.button == 0)
			{
				if(reorderingOverVisibleDropIndex != -1 && reordering != null && clickCount == 0)
				{
					e.Use();

					#if DEV_MODE && DEBUG_REORDERING
					Debug.Log("Dragged " + reordering.label.text + " from " + (visibleColumns.IndexOf(reordering)) + " to " + reorderingOverVisibleDropIndex);
					#endif

					int removeFromIndex = columns.IndexOf(reordering);
					int moveToIndex;
					if(reorderingOverVisibleDropIndex < visibleColumns.Count)
					{
						var moveBefore = visibleColumns[reorderingOverVisibleDropIndex];
						moveToIndex = columns.IndexOf(moveBefore);
					}
					else
					{
						moveToIndex = columns.Count;
					}

					columns.RemoveAt(removeFromIndex);

					if(moveToIndex > removeFromIndex)
					{
						moveToIndex--;
					}

					columns.Insert(moveToIndex, reordering);
					RebuildVisibleColumns();
				}
				reordering = null;
				reorderingOverVisibleDropIndex = -1;
				return;
			}

			if(e.type == EventType.MouseDown && e.button == 0 && drawRect.Contains(e.mousePosition) && resizing == null)
			{
				reordering = column;
				reorderingOverVisibleDropIndex = -1;
				return;
			}

			if(reordering != column)
			{
				return;
			}

			reorderingOverVisibleDropIndex = -1;

			#if DEV_MODE && DEBUG_COLUMN_REORDERING
			var green = Color.green;
			green.a = 0.5f;
			var red = Color.red;
			red.a = 0.5f;
			#endif

			var mousePos = e.mousePosition;
			var dropZone = headerRowRect;
			dropZone.x -= 20f;
			dropZone.width = 40f;
			dropZone.y -= 10f;
			dropZone.height += 20f;

			for(int n = 0, count = visibleColumns.Count; n < count; n++)
			{
				#if DEV_MODE && DEBUG_COLUMN_REORDERING
				EditorGUI.DrawRect(dropZone, n == visibleColumns.IndexOf(reordering) || n == visibleColumns.IndexOf(reordering) + 1 ? red : green);
				#endif

				if(!dropZone.Contains(mousePos))
				{
					dropZone.x += visibleColumns[n].width * headerRowRect.width;
					continue;
				}

				int currentIndex = visibleColumns.IndexOf(reordering);
				reorderingOverVisibleDropIndex = n == currentIndex || n == currentIndex + 1 ? -1 : n;
				break;
			}

			#if DEV_MODE && DEBUG_COLUMN_REORDERING
			EditorGUI.DrawRect(dropZone, reordering != visibleColumns[visibleColumns.Count - 1] ? green : red);
			#endif

			if(reorderingOverVisibleDropIndex == -1 && dropZone.Contains(mousePos))
			{
				int lastIndex = visibleColumns.Count - 1;
				if(reordering != visibleColumns[lastIndex])
				{
					reorderingOverVisibleDropIndex = lastIndex + 1;
				}
			}

			GUI.changed = true;

			if(reorderingOverVisibleDropIndex == -1)
			{
				EditorGUIUtility.AddCursorRect(headerRowRect, MouseCursor.Pan);
				return;
			}

			EditorGUIUtility.AddCursorRect(dropZone, MouseCursor.Link);
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="windowWidth"> Width of the entire region inside which all the columns are drawn. </param>
		/// <param name="dividerRect"> Position and bounds of the vertical divider line which can be dragged to resize the column. </param>
		/// <param name="column"> Column that is on the left side of the dragged divider. </param>
		/// <param name="xStart"> Position on horizontal axis where <paramref name="column"/> starts. </param>
		/// <param name="clickCount"> Number of click events that have occurred within a short time frame since last mouse movement. </param>
		private void HandleResizing(float windowWidth, Rect dividerRect, ColumnData column, float xStart, int clickCount)
		{
			var resizerRect = dividerRect;
			resizerRect.x -= 3f;
			resizerRect.width += 6f;

			if(reordering == null)
			{
				EditorGUIUtility.AddCursorRect(resizerRect, MouseCursor.ResizeHorizontal);
			}

			if(clickCount == 2 && resizerRect.Contains(Event.current.mousePosition) && Event.current.rawType == EventType.MouseUp)
			{
				Event.current.Use();
				OptimizeColumnWidth(column, windowWidth);
			}

			switch(Event.current.rawType)
			{
				case EventType.MouseDown:
					if(Event.current.button == 0 && resizerRect.Contains(Event.current.mousePosition))
					{
						resizing = column;
						reordering = null;
						Event.current.Use();
					}
					return;
				case EventType.MouseUp:
					resizing = null;
					return;
				default:
					return;
				case EventType.MouseDrag:
				case EventType.MouseMove:
					if(resizing != column)
					{
						return;
					}
					break; // continue below
			}

			float setWidthPixels = Event.current.mousePosition.x - xStart;

			// If width for column is more than minimum acceptable amount, simply set column width (SetColumnWidth will handle reducing size of columns to the right).
			if(setWidthPixels >= PreferredMinColumnWidthInPixels)
			{
				SetColumnWidth(column, setWidthPixels / windowWidth, true, windowWidth);
				return;
			}

			// If column width is already at minimum yet cursor has been dragged even more to left side then shrink / push columns on the left to make way.

			int columnIndex = visibleColumns.IndexOf(column);
			if(columnIndex <= 0)
			{
				return;
			}

			// This is how many pixels we want to remove from the previous columns.
			float reminderPixels = PreferredMinColumnWidthInPixels - setWidthPixels;

			// Find first column to the left that is still wide enough to be shrunken.
			float preferredMinColumnWidth = PreferredMinColumnWidthInPixels / windowWidth;
			var shrinkableColumnIndex = visibleColumns.FindLastIndex(columnIndex - 1, (c)=>c.width > preferredMinColumnWidth);
			if(shrinkableColumnIndex == -1)
			{
				return;
			}

			var shrinkColumn = visibleColumns[shrinkableColumnIndex];

			// Determine new width for shrunken column.
			float setShrunkenColumnWidthPixels = Mathf.Max(shrinkColumn.width * windowWidth - reminderPixels, PreferredMinColumnWidthInPixels);

			// Determine how many pixels of width we are removing from the column.
			float shrinkAmountPixels = shrinkColumn.width * windowWidth - setShrunkenColumnWidthPixels;

			// Apply new width for shrunken column.
			SetColumnWidth(shrinkColumn, setShrunkenColumnWidthPixels / windowWidth, false, windowWidth);

			// Push all the gained width to column on *right* side of dragged divider.
			var expandColumn = visibleColumns[columnIndex + 1];
			SetColumnWidth(expandColumn, expandColumn.width + shrinkAmountPixels / windowWidth, false, windowWidth);
		}

		private void SetColumnWidth([NotNull]ColumnData column, float width, bool adjustAdjascent, float windowWidth)
		{
			float min = GetMinColumnWidth(column, windowWidth);
			float max = GetMaxColumnWidth(column, windowWidth);

			// Sometimes min value for the last column can be a tiny fraction larger than the caluclated max value.
			if(min > max)
            {
				#if DEV_MODE
				Debug.Assert(min - max < 0.0000001f, min + " / " + max);
				#endif
				min = max;
            }

			#if DEV_MODE
			Debug.Assert(min >= 0f, min);
			Debug.Assert(max >= 0f, max);
			Debug.Assert(min <= max, min + " / " + max);
			#endif

			#if DEV_MODE && DEBUG_SET_COLUMN_WIDTH
			Debug.Log("SetColumnWidth(" + column.label.text + ", " + width + ", "+ adjustAdjascent + ") with min="+min+", max="+max+ ", windowWidth="+ windowWidth+", mouse.x=" +Mathf.RoundToInt(Event.current.mousePosition.x));
			#endif

			width = Mathf.Clamp(width, min, max);

			float currentWidth = column.width;
			float increasedAmount = width - currentWidth;
			if(increasedAmount == 0f)
			{
				return;
			}

			column.width = width;
			GUI.changed = true;

			if(!adjustAdjascent)
			{
				return;
			}

			int count = visibleColumns.Count;

			if(count < 2)
			{
				return;
			}

			int columnIndex = visibleColumns.IndexOf(column);
			if(columnIndex == -1)
			{
				return;
			}

			float preferredMinColumnWidth = PreferredMinColumnWidthInPixels / windowWidth;

			// If it's the last column we don't need to worry about adjusting adjascent column width.
			// Last column should always be the one getting the remainder of other columns widths basically.
			if(columnIndex == count - 1)
			{
				return;
			}

			if(increasedAmount < 0f)
			{
				var nextColumn = visibleColumns[columnIndex + 1];
				SetColumnWidth(nextColumn, nextColumn.width - increasedAmount, false, windowWidth);
				return;
			}

			for(int n = columnIndex + 1; n < count; n++)
			{
				var shrinkColumn = visibleColumns[n];
				float shrinkToWidthTarget = shrinkColumn.width - increasedAmount;
				if(shrinkToWidthTarget > preferredMinColumnWidth)
				{
					SetColumnWidth(shrinkColumn, shrinkToWidthTarget, false, windowWidth);
					EnsureColumnWidthsAddUpToOne();
					return;
				}
				if(shrinkColumn.width <= preferredMinColumnWidth)
				{
					continue;
				}
				float actualShrinkAmount = shrinkColumn.width - preferredMinColumnWidth;
				SetColumnWidth(shrinkColumn, preferredMinColumnWidth, false, windowWidth);
				increasedAmount -= actualShrinkAmount;
			}
		}

		private float GetMinColumnWidth([NotNull]ColumnData column, float windowWidth)
		{
			return PreferredMinColumnWidthInPixels / windowWidth;
		}

		private float GetMaxColumnWidth([NotNull]ColumnData column, float windowWidth)
		{
			int columnIndex = visibleColumns.IndexOf(column);
			if(columnIndex == -1)
			{
				return 0f;
			}

			int resizingIndex = resizing == null ? -1 : visibleColumns.IndexOf(resizing);

			float spaceOccupiedByPreviousColumns = 0f;
			for(int n = 0; n < columnIndex; n++)
			{
				if(resizingIndex == n)
				{
					spaceOccupiedByPreviousColumns += GetMinColumnWidth(visibleColumns[n], windowWidth);
				}
				else
				{
					spaceOccupiedByPreviousColumns += visibleColumns[n].width;
				}
			}

			float minSpaceForFollowingColumns = 0f;
			for(int n = visibleColumns.Count - 1; n > columnIndex; n--)
			{
				minSpaceForFollowingColumns += GetMinColumnWidth(visibleColumns[n], windowWidth);
			}

			float remainingSpace = 1f - spaceOccupiedByPreviousColumns - minSpaceForFollowingColumns;

			return remainingSpace;
		}

		private void OptimizeColumnWidth(ColumnData column, float windowWidth)
		{
			float optimalWidth = GetOptimalColumnWidth(column, windowWidth);
			SetColumnWidth(column, optimalWidth, true, windowWidth);
		}

		private float GetOptimalColumnWidth(ColumnData column, float windowWidth)
		{
			float optimalWidth = stackTraceColumnHeaderStyle.CalcSize(column.label).x;
			for(int n = column.contents.Count - 1; n >= 0; n--)
			{
				float width = stackTraceEntryStyle.CalcSize(column.contents[n].label).x;
				if(width > optimalWidth)
				{
					optimalWidth = width;
				}
			}
			return Mathf.Max(optimalWidth / windowWidth, 0.02f);
		}

		private void EnsureColumnWidthsAddUpToOne()
		{
			int count = visibleColumns.Count;
			int lastIndex = count - 1;
			float totalWidth = 0f;
			for(int n = 0; n < lastIndex; n++)
			{
				var column = visibleColumns[n];
				totalWidth += column.width;
			}

			if(totalWidth < 1f)
			{
				visibleColumns[lastIndex].width = 1f - totalWidth;
				return;
			}

			#if DEV_MODE
			Debug.LogWarning("Total width before last column: " + totalWidth);
			#endif

			float setWidthForAll = 1f / count;
			for(int n = 0; n < count; n++)
			{
				#if DEV_MODE
				Debug.LogWarning(visibleColumns[n].label.text + ".width = " + setWidthForAll);
				#endif
				visibleColumns[n].width = setWidthForAll;
			}
		}

		private void OnHeaderClicked(ColumnData _)
		{
			if(Event.current.button == 1)
			{
				var menu = new GenericMenu();

				if(visibleColumns.Count == 1)
				{
					for(int n = 0, count = columns.Count; n < count; n++)
					{
						var column = columns[n];
						if(column.shown)
						{
							// Don't allow disabling last visible column.
							menu.AddDisabledItem(new GUIContent(column.nameInMenus), true);
							continue;
						}
						menu.AddItem(new GUIContent(column.nameInMenus), false, ToggleShowColumn, column);
					}
				}
				else
				{
					for(int n = 0, count = columns.Count; n < count; n++)
					{
						var column = columns[n];
						menu.AddItem(new GUIContent(column.nameInMenus), column.shown, ToggleShowColumn, column);
					}
				}
				menu.ShowAsContext();
			}
		}

		private void ToggleShowColumn(object columnParameter)
		{
			var column = columnParameter as ColumnData;
			column.shown = !column.shown;

			RebuildVisibleColumns();
			EnsureColumnWidthsAddUpToOne();

			EditorPrefs.SetBool(Key.Console + "Show" + (StackTraceDrawer.Column)column.id, column.shown);
		}

		private void RebuildVisibleColumns()
		{
			columnsAreUnoptimized = true;

			visibleColumns.Clear();

			for(int n = 0, count = columns.Count; n < count; n++)
			{
				var column = columns[n];
				if(column.shown)
				{
					visibleColumns.Add(column);
				}
			}

			// make sure at least one column is always shown
			if(visibleColumns.Count == 0 && columns.Count > 0)
			{
				columns[0].shown = true;
				visibleColumns.Add(columns[0]);
			}
		}

		[Serializable]
		private class ColumnData
		{
			public int id = -1;
			[Min(0f)]
			public float width = 0f;
			public GUIContent label = new GUIContent("");
			public string nameInMenus;
			public bool autoAdjustWidth = true;
			public bool shown = true;

			public List<ColumnContent> contents = new List<ColumnContent>();

			public ColumnData() { }
			
			public ColumnData(int id, string text, string tooltip, float initialWidth, bool autoAdjustWidth, bool shown, string nameInMenus)
			{
				label = new GUIContent(text, tooltip);
				this.id = id;
				width = initialWidth;
				this.autoAdjustWidth = autoAdjustWidth;
				this.shown = shown;
				this.nameInMenus = nameInMenus;
			}
		}

		[Serializable]
		private class ColumnContent
		{
			public GUIContent label = new GUIContent("");
			public OnGUIEvent overrideOnGUI;

			public ColumnContent() { }

			public ColumnContent(string text, string tooltip)
			{
				label = new GUIContent(text, tooltip);
			}

			public ColumnContent(string text, string tooltip, OnGUIEvent onGUICallback)
			{
				overrideOnGUI = onGUICallback;
				label = new GUIContent(text, tooltip);
			}
		}
	}
}