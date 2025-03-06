using System;

namespace EBus
{
    /// <summary>
    /// Event bus used to manage signals between services
    /// </summary>
    public interface IEventBus
    {
        /// <summary>
        /// Subscribes event to T signal.
        /// </summary>
        /// <param name="callback">Some event to be executed when signal invoked.</param>
        /// <param name="priority">Higher priority makes event execute earlier than other with same signal.</param>
        /// <typeparam name="T">Type of signal.</typeparam>
        public void Subscribe<T>(Action<T> callback, int priority = 0);

        /// <summary>
        /// Invokes new signal, causing to execute all subscribed events.
        /// </summary>
        /// <param name="signal">Use <b>new Signal()</b> construction to pass signal.</param>
        /// <typeparam name="T">Type of signal.</typeparam>
        public void Invoke<T>(T signal);

        /// <summary>
        /// Unsubscribe event from signal T.
        /// </summary>
        /// <param name="callback">Event to unsubscribe. Will not work with any lambda expressions.</param>
        /// <typeparam name="T">Type of signal.</typeparam>
        public void Unsubscribe<T>(Action<T> callback);

        /// <summary>
        /// Unsubscribe all subscribed events from signal T.
        /// </summary>
        /// <typeparam name="T">Type of signal.</typeparam>
        public void UnsubscribeAll<T>();
    }
}