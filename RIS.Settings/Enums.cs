using System;

namespace RIS.Settings
{
    [Flags]
    public enum SettingsLoadOptions : uint
    {
        None = 0,
        RemoveUnused = 1,
        DeduplicatePreserveValues = 2
    }
}
