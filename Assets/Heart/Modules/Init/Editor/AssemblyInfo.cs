using System.Runtime.CompilerServices;
using static Sisus.Init.AssemblyNames;

[assembly:
    InternalsVisibleTo(InitArgs.EditorInternal),
    InternalsVisibleTo(InitArgs.Search),

    InternalsVisibleTo(InitArgs.Odin),
    InternalsVisibleTo(InitArgs.uGUI),
    InternalsVisibleTo(InitArgs.UIToolkit),
    InternalsVisibleTo(InitArgs.UIToolkitEditor),

    InternalsVisibleTo(InitArgs.Tests.EditMode),
    InternalsVisibleTo(InitArgs.Tests.PlayMode)]