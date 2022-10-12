using System;

namespace Pancake
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public sealed class ReferenceContent : PancakeAttribute
    {
        public readonly string path;
        public readonly string tooltip;

        public ReferenceContent(string path)
        {
            this.path = path;
            tooltip = string.Empty;
            this.Active = true;
            this.Hided = false;
        }

        public ReferenceContent(string path, string tooltip)
            : this(path)
        {
            this.tooltip = tooltip;
        }

        public string GetName()
        {
            string[] split = path.Split('/');
            return split?.Length > 0 ? split[split.Length - 1] : string.Empty;
        }

        #region [Optional Parameters]

        public bool Active { get; set; }

        public bool Hided { get; set; }

        #endregion
    }
}