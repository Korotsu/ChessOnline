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
    private     Socket          socket;
    private     IPEndPoint      localEP;
    private     int             port                = 11000;
    private     List<Socket>    clientSocketList    = new List<Socket>();
    private     bool            connected           = false;
    public      ChessGameMgr    chessGameMgr        = null;
    private     int             nbPlayer            = 0;

    // Start is called before the first frame update
    void Start()
    {
        //create server socket
        IPAddress ipAdress = NetworkHelper.GetLocalIPAddress();
        localEP = new IPEndPoint(ipAdress, port);
        socket = new Socket(ipAdress.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

        Console.WriteLine("Starting Server ...");
        socket.Bind(localEP);
        socket.Listen(1);
        connected = true;
        nbPlayer++;

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
            connected = false;
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

    public void SendData<T>(T _data, Socket client)
    {
        Byte[] dataMSG = SerializationTools.Serialize<T>(_data);

        try
        {
            client.Send(dataMSG);
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

    public void BroadCastData<T>(T _data)
    {
        foreach (Socket client in clientSocketList)
        {
            SendData<T>(_data, client);
        }
    }

    public void AcceptCallBack(IAsyncResult result)
    {
        if (connected && nbPlayer < 2)
        {
            Debug.Log("Client connected !");
            nbPlayer++;

            // Get the socket that handles the client request.
            Socket listener = (Socket)result.AsyncState;
            Socket handler = listener.EndAccept(result);
            
            StateObject stateObject = new StateObject();
            stateObject.workSocket = handler;

            clientSocketList.Add(handler);
            chessGameMgr.enabled = true;

            System.Random rand = new System.Random();
            
            int hostTeam    = rand.Next(0, 2);
            int clientTeam  = 1 - hostTeam;

            GetComponent<Player>().team = (ChessGameMgr.EChessTeam)hostTeam;

            SendData(clientTeam, handler);

            handler.BeginReceive(stateObject.buffer, 0, StateObject.BUFFER_SIZE, 0,
                           new AsyncCallback(ReceiveCallBack), stateObject);

            socket.BeginAccept(new AsyncCallback(AcceptCallBack), socket);
        }
    }

    public void ReceiveCallBack(IAsyncResult result)
    {
        if (connected)
        {

            StateObject stateObject = (StateObject)result.AsyncState;

            Socket s = stateObject.workSocket;

            int read = s.EndReceive(result);

            if (read > 0)
            {

                //stateObject.stringBuilder.Append(Encoding.ASCII.GetString(stateObject.buffer, 0, read));  

                if (read != StateObject.BUFFER_SIZE)
                {

                    ChessGameMgr.Move move = (ChessGameMgr.Move) SerializationTools.Deserialize(stateObject.buffer);

                    if (move != null && chessGameMgr)
                    {
                        chessGameMgr.PlayTurn(move);

                        chessGameMgr.UpdatePieces();
                    }

                    string str = (string) SerializationTools.Deserialize(stateObject.buffer);

                    if (str != null)
                    {
                        //strContent = stateObject.stringBuilder.ToString();
                        Console.WriteLine(String.Format("Read {0} byte from socket" +
                                     "data = {1} ", stateObject.buffer.Length, str));
                    }

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
}

public class StateObject
{
    public Socket workSocket = null;
    public const int BUFFER_SIZE = 1024;
    public byte[] buffer = new byte[BUFFER_SIZE];
    public StringBuilder stringBuilder = new StringBuilder();
}