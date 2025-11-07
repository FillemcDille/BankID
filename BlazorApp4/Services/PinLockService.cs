namespace BlazorApp4.Services
{        
    /// <summary>
    /// Provides functionality to lock and unlock access using a personal identification number (PIN).
    /// </summary>
    /// <remarks>Use this service to control access to resources that require PIN-based authentication. The
    /// service maintains an unlocked state that can be queried or reset. Thread safety is not guaranteed; if used in
    /// multi-threaded scenarios, external synchronization may be required.</remarks>
    public class PinLockService
    {
        private const string CorrectPin = "1234";
        public bool IsUnlocked { get; private set; }

        /// <summary>
        /// Attempts to unlock the service using the provided PIN.
        /// </summary>
        /// <param name="pin">The PIN code to validate against the stored correct PIN.</param>
        /// <returns><c>true</c> if the PIN is correct and the service is unlocked; otherwise, <c>false</c>.</returns>
        public bool TryUnlock(string pin)
        {
            if(pin == CorrectPin)
            {
                IsUnlocked = true;
                return true;
            }
            return false;
        }

        /// <summary>
        /// Locks the object, preventing further modifications until it is unlocked.
        /// </summary>
        public void Lock() => IsUnlocked = false;
    }
}
