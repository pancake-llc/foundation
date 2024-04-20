using System;

namespace Pancake.BakingSheet.Internal
{
    // Prevent Unity's code stripper
    [AttributeUsage(AttributeTargets.All)]
    public class PreserveAttribute : Attribute
    {
    }
}