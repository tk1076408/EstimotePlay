using System;

using Android.App;
using Android.Content;
using Android.Runtime;
using Android.Util;
using Android.Views;
using Android.Widget;
using Android.OS;
using EstimoteSdk;

using Microsoft.WindowsAzure;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Auth;
using Microsoft.WindowsAzure.Storage.Blob;

using Java.Util.Concurrent;
using System.Collections.Generic;
using System.Linq;

using JavaObject = Java.Lang.Object;
using System.IO;
using System.Text;
using Newtonsoft.Json;
using System.Threading.Tasks;

namespace EstimotePlay
{
    [Activity(Label = "Estimote Play", MainLauncher = true, Icon = "@drawable/icon")]
    public class MainActivity : Activity, BeaconManager.IServiceReadyCallback, BeaconManager.IRangingListener
    {
        static readonly string Tag = typeof(MainActivity).FullName;
        static readonly Region ALL_ESTIMOTE_BEACONS = new Region("rid", "B9407F30-F5F8-466E-AFF9-25556B57FE6D", null, null);
        static readonly string ContainerSas = "https://estimoteplay.blob.core.windows.net/estimoteplay?sr=c&sv=2015-02-21&st=2015-10-08T18%3A07%3A28Z&se=2015-10-15T19%3A07%3A28Z&sp=rw&sig=kAErfPtd2dZW%2Bx0NoqkMrtMWeIWJx0IjbA%2B%2Fv%2F0baYY%3D";

        private CloudBlobContainer container = new CloudBlobContainer(new Uri(ContainerSas));
        private Dictionary<string, double> dis = new Dictionary<string, double>();
        private Dictionary<string, string> macToName = new Dictionary<string, string>();

        BeaconManager _beaconManager;

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            this.macToName["F7:B7:89:83:A4:1D"] = "blueberry";
            this.macToName["D2:F5:46:E5:00:39"] = "mint";
            this.macToName["F5:9E:47:57:24:AA"] = "ice";

            // Set our view from the "main" layout resource
            SetContentView(Resource.Layout.Main);

            TextView text = FindViewById<TextView>(Resource.Id.textView1);

            _beaconManager = new BeaconManager(this);
            _beaconManager.SetRangingListener(this);

            _beaconManager.Connect(this);
        }

        public void OnBeaconsDiscovered(Region region, IList<Beacon> beacons)
        {
            Log.Debug(Tag, "Ranged beacons: " + beacons.Count);
            if (beacons.Count != 0)
            {
                foreach (var b in beacons)
                {
                    var distance = Utils.ComputeAccuracy(b);
                    Log.Debug("BeaconDetails:", b.MacAddress + " distance: " + distance);
                    dis[macToName[b.MacAddress]] = distance;
                }
            }

            var text = string.Empty;
            var keys = dis.Keys.ToList();
            keys.Sort();

            foreach (var k in keys)
            {
                text += k + " " + dis[k] + "\n";
            }

            CloudBlockBlob blob = this.container.GetBlockBlobReference("DistanceInfo");
            Task.Run(async () =>
            {
                MemoryStream msWrite = new MemoryStream(Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(dis)));
                msWrite.Position = 0;
                using (msWrite)
                {
                   await blob.UploadFromStreamAsync(msWrite);
                }
            }).ConfigureAwait(false);

            FindViewById<TextView>(Resource.Id.textView1).Text = text;
        }

        public void OnServiceReady()
        {
            // This method is called when BeaconManager is up and running.
            _beaconManager.StartRanging(ALL_ESTIMOTE_BEACONS);
        }

        protected override void OnDestroy()
        {
            // Make sure we disconnect from the Estimote.
            _beaconManager.Disconnect();
            base.OnDestroy();
        }
    }
}


