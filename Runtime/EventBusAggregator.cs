#if EBUS_LOG
using System;
using UnityEngine;

namespace EBus
{
    public static class EventBusAggregator
    {
#if UNITY_EDITOR
        public static event Action<IEventBusLogable, string, BusLogType> OnLog;
        public static event Action<IEventBusLogable, bool> OnNewEventBus;

        public static void Log(this IEventBusLogable sender, string message, BusLogType type)
        {
            OnLog?.Invoke(sender, message, type);
        }

        public static void NotifyNewEventBus(this IEventBusLogable sender, bool isActive)
        {
            OnNewEventBus?.Invoke(sender, isActive);
        }
#endif
    }
}
#endif