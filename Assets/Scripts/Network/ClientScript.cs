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
    int port                                    = 11000;
    public bool connected                       = false;
    public ChessGameMgr chessGameMgr            = null;
    private bool teamSelected                   = false;
    [SerializeField] private Player player      = null;
    private bool shouldPlayTurn                 = false;
    private ChessGameMgr.Move lastServerMove    = new ChessGameMgr.Move();

    public void Connect(string hostIPAddress)
    {
        //create server socket
        IPHostEntry host = Dns.GetHostEntry(hostIPAddress);
        //Dns.GetHostAddresses(hostIPAddress);

        IPAddress ipAdress = null;
        
        foreach (IPAddress ip in host.AddressList)
        {
            if (ip.AddressFamily == AddressFamily.InterNetwork)
            {
                ipAdress = ip;
            }
        }

        if (ipAdress != null)
        {

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
            Debug.Log("error = " + e.ToString());
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
            Debug.Log("error = " + e.ToString());
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
                    checkConvertProcess(stateObject.buffer);
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

    private void Update()
    {
        if (shouldPlayTurn)
        {
            shouldPlayTurn = false;

            chessGameMgr.PlayTurn(lastServerMove, (player.team == ChessGameMgr.EChessTeam.White) ? ChessGameMgr.EChessTeam.Black : ChessGameMgr.EChessTeam.White);
            chessGameMgr.UpdatePieces();
        }
    }

    private void checkConvertProcess(byte[] buffer)
    {
        var value = SerializationTools.Deserialize(buffer);

        switch (value)
        {
            case ChessGameMgr.Move move:
                if (chessGameMgr && player)
                {
                    lastServerMove = move;
                    shouldPlayTurn = true;
                }
                break;

            case ChessGameMgr.EChessTeam team:
                if (player && !teamSelected)
                {
                    teamSelected = true;
                    player.team = team;
                }
                break;

            case Player player:
                GUIMgr.player2 = player;
                break;

            default:
                Debug.Log("Could not convert " + value.ToString());
                break;
        }
    }
}
