using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine.UIElements;

namespace Pancake.Database
{
    public abstract class GroupColumn : DashboardColumn
    {
        protected bool isSubscribed;
        protected static List<Type> allValidTypesCache;
        protected static List<IGroupButton> allButtonsCache;

        protected static Assembly assembly;
        protected static AssemblyName assemblyName;

        public abstract GroupFoldableButton SelectButtonByTitle(string title);
        public abstract void ScrollTo(VisualElement button);
        public abstract void Filter(string f);
        public abstract void FilterBySearchBar();
    }
}