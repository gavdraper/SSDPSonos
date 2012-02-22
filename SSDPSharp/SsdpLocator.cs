using System.Collections.ObjectModel;
using System.Net.Sockets;

namespace SSDPSharp
{    
    public class SsdpLocator
    {
        const string multicastAddress = "239.255.255.250";
        const int multicastPort = 1900;
        const int unicastPort = 1901;

        const string messageHeader = "M-SEARCH * HTTP/1.1";
        const string messageHost = "HOST: 239.255.255.250:1900";
        const string messageMan = "MAN: \"ssdp:discover\"";
        const string messageMx = "MX: 8";
        const string messageSt = "ST: ssdp:all";

        string broadcastMessage =
            string.Format("{1}{0}{2}{0}{3}{0}{4}{0}{5}{0}{0}",
                          "/r/n",
                          messageHeader,
                          messageHost,
                          messageMan,
                          messageMx,
                          messageSt);

        public ObservableCollection<SsdpDevice> Devices { get; set; }

        public SsdpLocator()
        {
            Devices = new ObservableCollection<SsdpDevice>();
        }

        public void SendSsdpIdentificationRequest()
        {
            var SsdpSocket = new Socket(
                AddressFamily.InterNetwork,
                SocketType.Dgram,
                ProtocolType.Udp);
        }

    }
}
