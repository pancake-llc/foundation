using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Needle.Console
{
	[Serializable]
	public class ObjectIdFilter : FilterBase<int>
	{
		public ObjectIdFilter(ref List<FilterEntry> ids) : base(ref ids)
		{
		}

		public override string GetLabel(int index)
		{
			var id = this[index];
			var obj = EditorUtility.InstanceIDToObject(id);
			return obj ? (obj.GetType().Name + " on " + obj.name) : "Missing Object? InstanceId=" + id; 
		} 

		protected override (FilterResult result, int index) OnFilter(string message, int mask, int row, LogEntryInfo info)
		{
			if (info.instanceID == 0) return (FilterResult.Keep, -1);
			return base.OnFilter(message, mask, row, info);
		}

		protected override bool MatchFilter(int entry, int index, string message, int mask, int row, LogEntryInfo info)
		{
			return entry == info.instanceID;
		}

		public override void AddLogEntryContextMenuItems(GenericMenu menu, LogEntryInfo clickedLog, string preview)
		{
			if (clickedLog.instanceID == 0) return;
			var obj = EditorUtility.InstanceIDToObject(clickedLog.instanceID);
			if (!obj) return;
			var text = "Instance " + obj.GetType().Name + " on " + obj.name;
			AddContextMenuItem_Hide(menu, HideMenuItemPrefix + text, clickedLog.instanceID);
			AddContextMenuItem_Solo(menu, SoloMenuItemPrefix + text, clickedLog.instanceID);
		}
	}
}