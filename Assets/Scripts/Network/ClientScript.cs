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
    public bool connected = false;
    public ChessGameMgr chessGameMgr = null;
    private bool teamSelected = false;

    public void Connect(string hostIPAddress)
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
            connected = true;
            StateObject stateObject = new StateObject();
            stateObject.workSocket = socket;

            socket.BeginReceive(stateObject.buffer, 0, StateObject.BUFFER_SIZE, 0,
                          new AsyncCallback(ReceiveCallBack), stateObject);
        }
        catch (Exception e)
        {
            connected = false;
            if (socket != null)
            {
                socket.Close();
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

    public void ReceiveCallBack(IAsyncResult result)
    {
        if (connected)
        {

            StateObject stateObject = (StateObject)result.AsyncState;

            Socket s = stateObject.workSocket;

            int read = s.EndReceive(result);

            if (read > 0)
            { 

                if (read != StateObject.BUFFER_SIZE)
                {
                    if (!teamSelected)
                    {
                        GetComponent<Player>().team = (ChessGameMgr.EChessTeam) SerializationTools.Deserialize(stateObject.buffer);
                    }

                    else
                    {
                        ChessGameMgr.Move move = (ChessGameMgr.Move)SerializationTools.Deserialize(stateObject.buffer);

                        if (move != null && chessGameMgr)
                        {
                            chessGameMgr.PlayTurn(move);

                            chessGameMgr.UpdatePieces();
                        }
                    }


                    string str = (string)SerializationTools.Deserialize(stateObject.buffer);

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
