using System;

namespace EBus.Editor
{
    internal sealed class LogEntry
    {
        public string Message { get; }
        public BusLogType Type { get; }
        public DateTime Timestamp { get; }
        public IEventBusLogable Bus { get; }


        public LogEntry(string message, BusLogType type, IEventBusLogable bus)
        {
            Message = message;
            Type = type;
            Timestamp = DateTime.Now;
            Bus = bus;
        }
    }
}