using System;

using Android.App;
using Android.Content;
using Android.Runtime;
using Android.Util;
using Android.Views;
using Android.Widget;
using Android.OS;
using EstimoteSdk;

using Java.Util.Concurrent;
using System.Collections.Generic;
using System.Linq;

using JavaObject = Java.Lang.Object;

namespace EstimotePlay
{
	[Activity (Label = "Estimote Play", MainLauncher = true, Icon = "@drawable/icon")]
	public class MainActivity : Activity, BeaconManager.IServiceReadyCallback, BeaconManager.IRangingListener
	{
		static readonly string Tag = typeof(MainActivity).FullName;
		static readonly Region ALL_ESTIMOTE_BEACONS = new Region("rid", "B9407F30-F5F8-466E-AFF9-25556B57FE6D", null, null);

		private Dictionary<string, double> Dis = new Dictionary<string, double>();
		private Dictionary<string, string> MacToName = new Dictionary<string, string>();

		int count = 1;

		BeaconManager _beaconManager;

		protected override void OnCreate (Bundle bundle)
		{
			base.OnCreate (bundle);

			this.MacToName ["F7:B7:89:83:A4:1D"] = "Blueberry";
			this.MacToName ["D2:F5:46:E5:00:39"] = "Mint";
			this.MacToName ["F5:9E:47:57:24:AA"] = "Ice";

			// Set our view from the "main" layout resource
			SetContentView (Resource.Layout.Main);

			// Get our button from the layout resource,
			// and attach an event to it
			Button button = FindViewById<Button> (Resource.Id.myButton);
			
			button.Click += delegate {
				button.Text = string.Format ("{0} clicks!", count++);
			};

			TextView text = FindViewById<TextView> (Resource.Id.textView1);

			_beaconManager = new BeaconManager (this);
			_beaconManager.SetRangingListener (this);

			_beaconManager.Connect (this);

			/*_findBeacon = new FindSpecificBeacon(this);
			_findBeacon.BeaconFound += (sender, e) => {
				Log.Debug(Tag, "Found the beacon!");
			};*/
		}

		public void OnBeaconsDiscovered (Region region, IList<Beacon> beacons)
		{
			Log.Debug (Tag, "Ranged beacons: " + beacons.Count);
			if (beacons.Count != 0) 
			{
				foreach (var b in beacons) {
					var distance = Utils.ComputeAccuracy (b);
					Log.Debug ("BeaconDetails:", b.MacAddress + " distance: " + distance);
					Dis [b.MacAddress] = distance;
				}
			}

			var text = string.Empty;
			var keys = Dis.Keys.ToList ();
			keys.Sort ();

			foreach (var k in keys) 
			{
				text += MacToName[k] + " " + Dis [k] + "\n";
			}

			FindViewById<TextView> (Resource.Id.textView1).Text = text;
		}

		public void OnServiceReady()
		{
			// This method is called when BeaconManager is up and running.
			_beaconManager.StartRanging(ALL_ESTIMOTE_BEACONS);
		}

		protected override void OnDestroy()
		{
			// Make sure we disconnect from the Estimote.
			_beaconManager.Disconnect ();
			base.OnDestroy ();
		}
	}
}


