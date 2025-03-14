namespace EBus
{
    /// <summary>
    /// Any invoked signal with this interface will be logged with the specified message.
    /// </summary>
    public interface IDebugableSignal
    {
        /// <summary>
        /// Debug message to log with signal invoke.
        /// </summary>
        /// <returns></returns>
        public string DebugMessage();
    }
}