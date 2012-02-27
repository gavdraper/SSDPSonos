using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SSDPSharp;

namespace TestConsole
{
    public class SsdpConsole
    {
        static SsdpLocator locator;

        static void Main()
        {
            locator = new SsdpLocator();
            //locator.Devices.CollectionChanged += new System.Collections.Specialized.NotifyCollectionChangedEventHandler(DevicesCollectionChanged);
            //locator.SendSsdpIdentificationRequest();
            ShowMenu();
        }

        static void ShowMenu()
        {
            Console.Clear();
            Console.WriteLine("1:Find New Sonos Devices");
            Console.WriteLine("2:List Discovered Sonos Devices");
            Console.WriteLine("3:Send Identification Request");
            var menuSelection = int.Parse(Console.ReadLine());
            switch (menuSelection)
            {
                case 1:
                    FindNewSonosDevices();
                    break;
                case 2:
                    ListDiscoveredSonosDevices();
                    break;
            }
        }


        static void FindNewSonosDevices()
        {
            Console.Clear();
            Console.WriteLine("Hit The Sync Buttons On The Sonos Device You Want To Discover Then Press Enter");
            Console.ReadLine();
            Console.WriteLine("Searching");
            locator.CreateSsdpListener();  
            Console.WriteLine("Finished");

            foreach(var d in locator.Devices)
                Console.WriteLine(d.Location);

            Console.ReadLine();
            ShowMenu();
        }

        private static void ListDiscoveredSonosDevices()
        {
            Console.Clear();
            if(locator.Devices.Count==0)
                Console.WriteLine("No Devices Found");
            else
            {
                foreach(var d in locator.Devices)
                    Console.WriteLine(d.Location);
            }
            Console.ReadLine();
            ShowMenu();
        }

        static void DevicesCollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            Console.Clear();
            foreach(var d in locator.Devices)
            {
                Console.WriteLine(d.Location);
            }
        }

    }
}
