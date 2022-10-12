using System;

namespace Pancake
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public class SearchableEditorAttribute : PancakeAttribute
    {
    }
}