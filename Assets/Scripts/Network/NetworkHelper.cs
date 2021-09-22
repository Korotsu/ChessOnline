using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using UnityEngine;

public class NetworkHelper
{
    public static IPAddress GetLocalIPAddress()
    {
        IPHostEntry Host = Dns.GetHostEntry(Dns.GetHostName());

        foreach (IPAddress ip in Host.AddressList)
        {
            if (ip.AddressFamily == AddressFamily.InterNetwork)
            {
                return ip;
            }
        }

        return null;
    }
}
