using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.Net.Sockets;
using UnityEngine;
using SerializationTool;

public class ClientScript : MonoBehaviour
{
    Socket socket;
    int port = 11000;
    public bool connectionFailed = false;
    public string hostIPAddress = null;

    // Start is called before the first frame update
    void Start()
    {
        if (hostIPAddress != null)
        {
            //create server socket
            IPHostEntry host = Dns.GetHostEntry(hostIPAddress);
            //Dns.GetHostAddresses(hostIPAddress);
            IPAddress ipAdress = host.AddressList[1];
            socket = new Socket(ipAdress.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
            IPEndPoint serverEP = new IPEndPoint(ipAdress, port);

            try
            {
                socket.Connect(serverEP);
            }
            catch (Exception e)
            {
                connectionFailed = true;
                if (socket != null)
                {
                    socket.Close();
                }
            }
        }
    }

    private void OnDisable()
    {
        Disconnect();
    }

    public void Disconnect()
    {
        if (socket != null)
        {
            socket.Shutdown(SocketShutdown.Both);
            socket.Close();
        }
    }

    public void SendData<T>(T _data)
    {
        Byte[] dataMSG = SerializationTools.Serialize<T>(_data);

        try
        {
            socket.Send(dataMSG);
        }
        catch (Exception e)
        {
            Console.WriteLine("error = " + e.ToString());
        }
    }

    public string ReceiveMessage()
    {
        try
        {
            Byte[] msg = new Byte[1024];
            int nbBytes = socket.Receive(msg);
            return Encoding.ASCII.GetString(msg, 0, nbBytes);
        }

        catch (Exception e)
        {
            Console.WriteLine("error = " + e.ToString());
        }

        return String.Empty;
    }
}
