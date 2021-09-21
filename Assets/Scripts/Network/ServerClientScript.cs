using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.Net.Sockets;
using UnityEngine;
using SerializationTool;

public class ServerClientScript : MonoBehaviour
{
    Socket socket;
    IPEndPoint localEP;
    int port = 11000;
    public List<Socket> clientSocketList = new List<Socket>();
    public bool hasToDisconnect = false;

    // Start is called before the first frame update
    void Start()
    {
        //create server socket
        IPHostEntry host = Dns.GetHostEntry("localhost");
        IPAddress ipAdress = host.AddressList[0];
        localEP = new IPEndPoint(ipAdress, port);
        socket = new Socket(ipAdress.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
    }

    private void Awake()
    {
        Console.WriteLine("Starting Server ...");
        socket.Bind(localEP);
        socket.Listen(1);

        try
        {
            //Console.WriteLine("Waiting for connection ...");

            socket.BeginAccept(new AsyncCallback(AcceptCallBack), socket);

            //Console.WriteLine("Accepted Client !");
        }

        catch (Exception e)
        {
            Console.WriteLine("error = " + e.ToString());
        }
    }

    private void OnDisable()
    {
        Disconnect();
    }

    public void Disconnect()
    {
        foreach (Socket clientSocket in clientSocketList)
        {
            if (clientSocket != null)
            {
                //shutdown client socket
                try
                {
                    clientSocket.Shutdown(SocketShutdown.Both);
                }
                catch (Exception e)
                {
                    Console.WriteLine("error = " + e.ToString());
                }

                finally
                {
                    clientSocket.Close();
                }
            }
        }

        if (socket != null)
        {
            //no shutdown necessary
            socket.Close();
        }
    }

    public void SendMessage(string message, Socket client)
    {
        Byte[] msg = Encoding.ASCII.GetBytes(message);
        try
        {
            client.Send(msg);
        }
        catch (Exception e)
        {
            Console.WriteLine("error = " + e.ToString());
        }
    }

    public string ReceiveMessage(int id)
    {

        try
        {
            Byte[] msg = new Byte[StateObject.BUFFER_SIZE];
            int nbBytes = clientSocketList[id].Receive(msg);
            return Encoding.ASCII.GetString(msg, 0, nbBytes);
        }

        catch (Exception e)
        {
            Console.WriteLine("error = " + e.ToString());
        }

        return String.Empty;
    }

    public void BroadCastMessage(string message)
    {
        foreach (Socket client in clientSocketList)
        {
            SendMessage(message, client);
        }
    }

    public void AcceptCallBack(IAsyncResult result)
    {
        // Get the socket that handles the client request.
        Socket listener = (Socket)result.AsyncState;
        Socket handler = listener.EndAccept(result);
        StateObject stateObject = new StateObject();
        stateObject.workSocket = handler;

        clientSocketList.Add(handler);

        handler.BeginReceive(stateObject.buffer, 0, StateObject.BUFFER_SIZE, 0,
                       new AsyncCallback(ReceiveCallBack), stateObject);

        socket.BeginAccept(new AsyncCallback(AcceptCallBack), socket);
    }

    public void ReceiveCallBack(IAsyncResult result)
    {
        StateObject stateObject = (StateObject)result.AsyncState;

        Socket s = stateObject.workSocket;

        int read = s.EndReceive(result);

        if (read > 0)
        {

            //stateObject.stringBuilder.Append(Encoding.ASCII.GetString(stateObject.buffer, 0, read));  

            if (read != StateObject.BUFFER_SIZE)
            {

                object obj = SerializationTools.Deserialize(stateObject.buffer);

                string strContent = obj.ToString();
                //strContent = stateObject.stringBuilder.ToString();
                Console.WriteLine(String.Format("Read {0} byte from socket" +
                             "data = {1} ", stateObject.buffer.Length, strContent));

            }

            s.BeginReceive(stateObject.buffer, 0, StateObject.BUFFER_SIZE, 0,
                                     new AsyncCallback(ReceiveCallBack), stateObject);
        }
        else
        {
            if (stateObject.stringBuilder.Length > 1)
            {
                //All of the data has been read, so displays it to the console
                string strContent;
                strContent = stateObject.stringBuilder.ToString();
                Console.WriteLine(String.Format("Read {0} byte from socket" +
                                 "data = {1} ", strContent.Length, strContent));
            }
            s.Shutdown(SocketShutdown.Both);
            s.Close();
        }
    }
}

public class StateObject
{
    public Socket workSocket = null;
    public const int BUFFER_SIZE = 1024;
    public byte[] buffer = new byte[BUFFER_SIZE];
    public StringBuilder stringBuilder = new StringBuilder();
}