using System.Runtime.CompilerServices;
using static Sisus.Init.AssemblyNames;

[assembly:
    InternalsVisibleTo(InitArgs.Editor),
    InternalsVisibleTo(InitArgs.EditorInternal),
    InternalsVisibleTo(InitArgs.Search),

    InternalsVisibleTo(InitArgs.Odin),
    InternalsVisibleTo(InitArgs.uGUI),
    InternalsVisibleTo(InitArgs.UIToolkit),
    InternalsVisibleTo(InitArgs.UIToolkitEditor),
    InternalsVisibleTo(InitArgs.PancakeLocalization),

    InternalsVisibleTo(InitArgs.Tests.EditMode),
    InternalsVisibleTo(InitArgs.Tests.PlayMode)]

namespace Sisus.Init
{
    internal static class AssemblyNames
    {
        public static class InitArgs
        {
            public const string Main = "InitArgs";
            public const string Editor = "InitArgs.Editor";
            public const string EditorInternal = "Unity.InternalAPIEditorBridgeDev.004";

            public const string uGUI = "InitArgs.uGUI";
            public const string UIToolkit = "InitArgs.UIToolkit";
            public const string UIToolkitEditor = "InitArgs.UIToolkit.Editor";

            public const string Odin = "InitArgs.Odin";
            public const string Search = "InitArgs.Search";
            public const string PancakeLocalization = "Pancake.Localization";

            public static class Tests
            {
                public const string EditMode = "InitArgs.Tests.EditMode";
                public const string PlayMode = "InitArgs.Tests.PlayMode";
            }
        }
    }
}