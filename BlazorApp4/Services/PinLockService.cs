namespace BlazorApp4.Services
{
    public class PinLockService
    {
        private const string CorrectPin = "1234";
        public bool IsUnlocked { get; private set; }
        
        public bool TryUnlock(string pin)
        {
            if(pin == CorrectPin)
            {
                IsUnlocked = true;
                return true;
            }
            return false;
        }

        public void Lock() => IsUnlocked = false;
    }
}
