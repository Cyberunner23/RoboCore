using System;

namespace RoboCore.Config
{
    public abstract class DataTransportConfigBase : LockingConfig
    {
        /// <summary>
        /// ID of the RoboCore Network.
        /// Multiple RoboCore Networks can be present on a given LAN network.
        /// </summary>
        private string _networkID = "default";

        public string NetworkID
        {
            get => _networkID;
            set
            {
                ThrowIfLocked();
                _networkID = value;
            }
        }

        /// <summary>
        /// Validates the config values.
        /// </summary>
        public void Validate()
        {

            Lock();
        }

        protected abstract void ValidateInternal();
    }
}