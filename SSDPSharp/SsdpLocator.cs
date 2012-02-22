using System.Collections.ObjectModel;
using System.Net;
using System.Net.Sockets;
using System.Text;

//Based on draft SSDP spec at
//  ftp://ftp.pwg.org/pub/pwg/ipp/new_SSDP/draft-cai-ssdp-v1-03.txt

namespace SSDPSharp
{    
    public class SsdpLocator
    {
        IPAddress multicastAddress = IPAddress.Parse("239.255.255.250");
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
            using (var identificationRequest = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp))
            {
                identificationRequest.SetSocketOption(SocketOptionLevel.IP, SocketOptionName.AddMembership,new MulticastOption(multicastAddress));
                identificationRequest.SetSocketOption(SocketOptionLevel.IP, SocketOptionName.MulticastTimeToLive, 2);
                identificationRequest.Connect(new IPEndPoint(multicastAddress, multicastPort));
                var message = Encoding.UTF8.GetBytes(broadcastMessage);
                identificationRequest.Send(message, message.Length, SocketFlags.None);
                identificationRequest.Close();
            }
        }

    }
}
