using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using System.Text;

namespace Downloader
{
    public class NetworkHelper
    {
        private bool _isNetworkOnline;

        public static bool IsNetworkAvailable
        {
            get { return System.Net.NetworkInformation.NetworkInterface.GetIsNetworkAvailable(); }
        }

        public NetworkHelper()
        {
           NetworkChange.NetworkAvailabilityChanged += new NetworkAvailabilityChangedEventHandler(NetworkChange_NetworkAvailabilityChanged);
           _isNetworkOnline = NetworkInterface.GetIsNetworkAvailable();
        }

        void NetworkChange_NetworkAvailabilityChanged(object sender, NetworkAvailabilityEventArgs e)
        {
            _isNetworkOnline  = e.IsAvailable;
        }
        ~NetworkHelper()
        {
            NetworkChange.NetworkAvailabilityChanged -= NetworkChange_NetworkAvailabilityChanged;
        }
    }
}
