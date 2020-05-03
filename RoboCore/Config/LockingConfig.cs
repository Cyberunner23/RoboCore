using System;

namespace RoboCore.Config
{
    public class LockingConfig
    {
        private bool _isLocked = false;

        protected void Lock()
        {
            _isLocked = true;
        }

        protected void ThrowIfLocked()
        {
            if (_isLocked)
            {
                throw new InvalidOperationException("Config cannot be edited after start");
            }
        }
    }
}