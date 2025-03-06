using System;

namespace EBus
{
    [Flags]
    public enum BusLogType
    {
        Log = 1,
        Warning = 2,
        Error = 4
    }
}