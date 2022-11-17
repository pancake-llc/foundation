#if UNITY_EDITOR
using System.Runtime.CompilerServices;

// Expose internal members to editor assembly for inspectors, other editor windows or functions

[assembly: InternalsVisibleTo("pancake@editor")]
[assembly: InternalsVisibleTo("Tests.EditMode")]

#endif

[assembly: UnityEngine.Scripting.Preserve]