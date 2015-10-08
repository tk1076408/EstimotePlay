using System;
using System.Collections.Generic;
using System.Linq;

using EstimoteSdk;

namespace EstimotePlay
{
    class BeaconsFoundEventArgs : EventArgs
    {
        public BeaconsFoundEventArgs(IEnumerable<Beacon> beacons)
        {
            Beacons = beacons == null ? new Beacon[0] : beacons.ToArray();
        }

        public Beacon[] Beacons { get; private set; }
    }
}
