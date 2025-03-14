using System;

namespace EBus
{
    public interface IEventBusLogable
    {
#if UNITY_EDITOR
        public void GetAllSignals();
#endif
    }
}