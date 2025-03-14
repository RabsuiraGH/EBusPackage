using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
#if UNITY_EDITOR
using System.Text;
#endif

namespace EBus
{
    [Serializable]
    public sealed class EventBus : IEventBus, IDisposable, IEventBusLogable
    {
        private Dictionary<string, List<CallbackWithPriority>> _signalCallbacks = new();

#if UNITY_EDITOR
        public void GetAllSignals()
        {
#if EBUS_LOG
            if (!_signalCallbacks.Any())
                LogToConsole("No any signals available", BusLogType.Warning);

            foreach (var pair in _signalCallbacks)
            {
                foreach (CallbackWithPriority callback in _signalCallbacks[pair.Key])
                {
                    if (callback.Callback is Delegate d)
                    {
                        LogToConsole($" SIGNAL: <color=Cyan>{pair.Key}</color> --> " +
                                     $"CALLBACK: <color=Cyan>{d.Method.Name} | {d.Method.DeclaringType} </color>");
                    }
                    else
                    {
                        LogToConsole($" SIGNAL: <color=Cyan>{pair.Key}</color> --> CALLBACK: <color=Cyan>{callback.Callback}</color>");
                    }
                }
            }
#endif
        }
#endif

#if UNITY_EDITOR && EBUS_LOG
        public EventBus()
        {
            this.NotifyNewEventBus(true);
        }
#endif


        public void Subscribe<T>(Action<T> callback, int priority = 0)
        {
            string key = typeof(T).Name;

            if (_signalCallbacks.ContainsKey(key))
            {
                _signalCallbacks[key].Add(new CallbackWithPriority(priority, callback));
            }
            else
            {
                _signalCallbacks.Add(key, new List<CallbackWithPriority> { new(priority, callback) });
            }
#if EBUS_LOG
            LogToConsole($"<color=Green>{callback.Method.Name} | {callback.Method.DeclaringType}</color> " +
                         $"subscribed to <color=Green>{typeof(T).Name}</color>");
#endif
            _signalCallbacks[key] = _signalCallbacks[key].OrderByDescending(x => x.Priority).ToList();
        }


        public void Invoke<T>(T signal)
        {
            string key = typeof(T).Name;

            if (_signalCallbacks.ContainsKey(key))
            {
#if EBUS_ADVANCED_LOG
                StringBuilder sb = new();
#endif
                foreach (CallbackWithPriority obj in _signalCallbacks[key])
                {
                    var callback = obj.Callback as Action<T>;
                    callback?.Invoke(signal);
#if EBUS_ADVANCED_LOG
                    sb.Append($"{callback?.Method.DeclaringType}.{callback?.Method.Name}\n");
#endif
                }

#if EBUS_ADVANCED_LOG && EBUS_LOG
                if(signal is IDebugableSignal debugable)
                {
                    LogToConsole($"<color=Green>{key}</color> was Invoked.\nMessage: {debugable.DebugMessage()}."+
                                 $"\n<color=Cyan>Signals:\n{sb}</color>");
                }
                else
                {
                    LogToConsole($"<color=Green>{key}</color> was Invoked. <color=Cyan>Signals:\n{sb}</color>");
                }

#elif EBUS_LOG
                LogToConsole($"Signal <color=Green>{key}</color> was Invoked");
#endif
            }
            else
            {
#if EBUS_LOG
                LogToConsole($"No any listeners to <color=Green>{key}</color> signal!");
#endif
            }
        }


        public void Unsubscribe<T>(Action<T> callback)
        {
            string key = typeof(T).Name;

            if (_signalCallbacks.ContainsKey(key))
            {
                CallbackWithPriority callbackToDelete =
                    _signalCallbacks[key].FirstOrDefault(x => x.Callback.Equals(callback));
                if (callbackToDelete != null)
                {
#if EBUS_LOG
                    LogToConsole($"<color=Red>{callback.Method.Name} | {callback.Method.DeclaringType}</color> " +
                                 $"unsubscribed to <color=Red>{typeof(T).Name}</color>");
#endif
                    _signalCallbacks[key].Remove(callbackToDelete);
                }
            }
            else
            {
#if EBUS_LOG
                LogToConsole($"Trying to unsubscribe for not existing key {key}!", BusLogType.Error);
#endif
            }
        }


        public void UnsubscribeAll<T>()
        {
            string key = typeof(T).Name;

            if (_signalCallbacks.ContainsKey(key))
            {
                _signalCallbacks.Remove(key);
#if EBUS_LOG
                LogToConsole($"Signal {key} was absolutely unsubscribed!");
#endif
            }
            else
            {
#if EBUS_LOG
                LogToConsole($"Trying to unsubscribe for not existing key {key}!", BusLogType.Error);
#endif
            }
        }

#if EBUS_LOG
        private void LogToConsole(string message, BusLogType logType = BusLogType.Log)
        {
#if UNITY_EDITOR
            this.Log(message, logType);
#else
            switch (logType)
            {
                case BusLogType.Warning:
                    Debug.LogWarning(message);
                    break;
                case BusLogType.Error:
                    Debug.LogError(message);
                    break;
                default:
                    Debug.Log(message);
                    break;
            }
#endif
        }
#endif

        ~EventBus()
        {
            Dispose();
        }


        public void Dispose()
        {
            _signalCallbacks.Clear();
#if EBUS_LOG
            LogToConsole("Event bus was cleared!");
#if UNITY_EDITOR
            this.NotifyNewEventBus(false);
#endif
#endif
        }
    }
}