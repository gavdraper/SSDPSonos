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
    public class SsdpLocator
    {
        readonly IPAddress multicastAddress = IPAddress.Parse("239.255.255.250");
        const int multicastPort = 1900;
        const int unicastPort = 1901;

        const string messageHeader = "M-SEARCH * HTTP/1.1";
        const string messageHost = "HOST: 239.255.255.250:1900";
        const string messageMan = "MAN: \"ssdp:discover\"";
        const string messageMx = "MX: 8";
        const string messageSt = "ST: urn:schemas-upnp-org:device:ZonePlayer:1";

        readonly byte[] broadcastMessage = Encoding.UTF8.GetBytes(
            string.Format("{1}{0}{2}{0}{3}{0}{4}{0}{5}{0}{0}",
                          "\r\n",
                          messageHeader,
                          messageHost,
                          messageMan,
                          messageMx,
                          messageSt));

        public ObservableCollection<SsdpDevice> Devices { get; set; }

        public SsdpLocator()
        {
            Devices = new ObservableCollection<SsdpDevice>();
        }

        public void CreateSsdpListener()
        {
            using (var socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp))
            {
                socket.Bind(new IPEndPoint(IPAddress.Any, unicastPort));
                socket.Connect(new IPEndPoint(multicastAddress, multicastPort));
                var thd = new Thread(()=>GetSocketResponse(socket));                
                socket.Send(broadcastMessage, 0, broadcastMessage.Length, SocketFlags.None);
                thd.Start();
                Thread.Sleep(20000);
                socket.Close();
            }
        }

        public void GetSocketResponse(Socket socket)
        {
                try
                {
                    while (true)
                    {
                        var response = new byte[8000];
                        EndPoint ep = new IPEndPoint(IPAddress.Any, unicastPort);
                        socket.ReceiveFrom(response, ref ep);
                        var str = Encoding.UTF8.GetString(response);
                        Devices.Add(new SsdpDevice() { Location = str });
                    }
                }
                catch
                {
                    //TODO handle exception for when connection closes
                }

        }





    }
}
