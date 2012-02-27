using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
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
        const int searchTimeOut = 3000;
        
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
                socket.SetSocketOption(SocketOptionLevel.IP, SocketOptionName.AddMembership, new MulticastOption(multicastAddress, IPAddress.Any));
                var thd = new Thread(() => GetSocketResponse(socket));
                socket.SendTo(broadcastMessage, 0, broadcastMessage.Length, SocketFlags.None, new IPEndPoint(multicastAddress, multicastPort));
                thd.Start();
                Thread.Sleep(searchTimeOut);
                socket.Close();
            }
        }

        public string GetLocation(string str)
        {
            if (str.StartsWith("HTTP/1.1 200 OK"))
            {
                var reader = new StringReader(str);
                var lines = new List<string>();
                for (;;)
                {
                    var line = reader.ReadLine();
                    if (line == null) break; 
                    if (line != "") lines.Add(line);
                }
                var location = lines.Where(lin => lin.ToLower().StartsWith("location:")).First();
                if (!string.IsNullOrEmpty(location) && 
                        (
                            Devices.Count == 0 ||
                            (from d in Devices where d.Location == location select d).FirstOrDefault() == null)
                        )
                {
                    return location.Replace("LOCATION: ","");
                }
            }
            return "";
        }

        public void GetSocketResponse(Socket socket)
        {
            try
            {
                while (true)
                {
                    var response = new byte[8000];
                    EndPoint ep = new IPEndPoint(IPAddress.Any, multicastPort);
                    socket.ReceiveFrom(response, ref ep);
                    var str = Encoding.UTF8.GetString(response);
                    var location = GetLocation(str);
                    if(!string.IsNullOrEmpty(location))
                        Devices.Add(new SsdpDevice() { Location = location });
                }
            }
            catch
            {
                //TODO handle exception for when connection closes
            }

        }





    }
}
