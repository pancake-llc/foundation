using System.Collections.Generic;
using UnityEngine;

namespace  Pancake.SelectiveProfiling
{
	public interface IContextMenuItemProvider
	{
		void AddItems(Object[] context, int contextUserData, List<ContextItem> items);
	}
}