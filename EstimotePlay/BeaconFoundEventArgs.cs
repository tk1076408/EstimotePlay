using System;

using EstimoteSdk;

namespace EstimotePlay
{
    class BeaconFoundEventArgs : EventArgs
    {
        public BeaconFoundEventArgs(Beacon beacon)
        {
            FoundBeacon = beacon;
        }

        public Beacon FoundBeacon { get; private set; }
    }
}
