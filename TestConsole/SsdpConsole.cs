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
            locator.Devices.CollectionChanged += new System.Collections.Specialized.NotifyCollectionChangedEventHandler(DevicesCollectionChanged);
            locator.SendSsdpIdentificationRequest();
            Console.ReadLine();
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
