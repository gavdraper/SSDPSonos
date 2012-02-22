using System;
using System.Collections.ObjectModel;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

//Based on draft SSDP spec at
//  ftp://ftp.pwg.org/pub/pwg/ipp/new_SSDP/draft-cai-ssdp-v1-03.txt

namespace SSDPSharp
{    
    public class SsdpLocator :IDisposable
    {
        readonly IPAddress multicastAddress = IPAddress.Parse("239.255.255.250");
        Socket listener;
        bool awaitingResponse;
        const int multicastPort = 1900;
        const int unicastPort = 1901;

        const string messageHeader = "M-SEARCH * HTTP/1.1";
        const string messageHost = "HOST: 239.255.255.250:1900";
        const string messageMan = "MAN: \"ssdp:discover\"";
        const string messageMx = "MX: 8";
        const string messageSt = "ST: ssdp:all";

        readonly string broadcastMessage =
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
            CreateSsdpListener();
        }
        
        void CreateSsdpListener()
        {
            listener = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            listener.Bind(new IPEndPoint(IPAddress.Any, unicastPort));
            listener.SetSocketOption(SocketOptionLevel.IP, SocketOptionName.AddMembership,
                              new MulticastOption(multicastAddress, IPAddress.Any));
            var t = new Thread(() =>
                                   {
                                       while (true)
                                       {
                                           var b = new byte[8000];
                                           awaitingResponse = true;
                                           listener.BeginReceive(b, 0, 8000, SocketFlags.None, new AsyncCallback(ListenerResponseReceived), null);
                                           awaitingResponse = false;
                                           Thread.Sleep(100);
                                       }
                                   });
            t.Start();
            
        }

        public void ListenerResponseReceived(IAsyncResult result)
        {
            //TODO Add code to read response
            awaitingResponse = false;
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

        public void Dispose()
        {
            if (listener != null)
                listener.Close();
        }
    }
}
