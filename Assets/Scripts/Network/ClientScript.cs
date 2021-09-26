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
    #region Variables
    Socket socket;
    int port = 11000;
    [System.NonSerialized] public bool connected = false;
    public ChessGameMgr chessGameMgr = null;
    private bool shouldPlayTurn = false;
    private ChessGameMgr.Move lastServerMove = new ChessGameMgr.Move();
    private bool shouldProcessDataBuffer = false;
    private List<byte[]> dataBufferList = new List<byte[]>();

    [SerializeField] private Player player = null;
    [SerializeField] private Player player2 = null;
    [SerializeField] private ChessGameMgr chessGameManager = null;
    [SerializeField] private GameObject scoreCanvas = null;
    [SerializeField] private GameObject clientCanvas = null;

    #endregion

    #region MonoBehaviors
    private void Update()
    {
        if (shouldPlayTurn)
        {
            PlayTurn();
        }

        if (shouldProcessDataBuffer)
        {
            ProcessDataBuffer();
        }
    }
    private void OnDisable()
    {
        Disconnect();
    }
    #endregion

    #region Functions
    #region CallBacks
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
                    dataBufferList.Add(stateObject.buffer);
                    shouldProcessDataBuffer = true;
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
    #endregion

    #region Connection
    public void Connect(string hostIPAddress)
    {
        //create server socket
        IPHostEntry host = Dns.GetHostEntry(hostIPAddress);
        IPAddress ipAdress = FindIPV4Adress(host);

        if (ipAdress != null)
        {
            socket = new Socket(ipAdress.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
            IPEndPoint serverEP = new IPEndPoint(ipAdress, port);

            try
            {
                socket.Connect(serverEP);
            }

            catch (Exception e)
            {
                Debug.Log("error = " + e.ToString());

                connected = false;
                if (socket != null)
                {
                    socket.Close();
                }

                return;
            }

            finally
            {
                connected = true;
                StateObject stateObject = new StateObject();
                stateObject.workSocket = socket;

                socket.BeginReceive(stateObject.buffer, 0, StateObject.BUFFER_SIZE, 0,
                              new AsyncCallback(ReceiveCallBack), stateObject);
            }
        }
    }
    public void Disconnect()
    {
        if (socket != null && socket.Connected)
        {
            //shutdown client socket
            try
            {
                socket.Shutdown(SocketShutdown.Both);
            }
            catch (Exception e)
            {
                Debug.Log("error = " + e.ToString());
            }

            finally
            {
                socket.Close();
            }
        }
    }
    #endregion

    #region Data
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
    #endregion

    #region Messages
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
    #endregion

    #region Other
    private void PrepareGame(PlayerData hostPlayer)
    {
        player2.playerData.username = hostPlayer.username;
        player.playerData.team = hostPlayer.team;

        scoreCanvas.SetActive(true);
        chessGameManager.enabled = true;

        clientCanvas.transform.GetChild(0).gameObject.SetActive(false);
        clientCanvas.SetActive(false);
    }
    private void PlayTurn()
    {
        chessGameMgr.PlayTurn(lastServerMove, (player.playerData.team == ChessGameMgr.EChessTeam.White) ? ChessGameMgr.EChessTeam.Black : ChessGameMgr.EChessTeam.White);
        chessGameMgr.UpdatePieces();

        shouldPlayTurn = false;
    }
    private void ProcessDataBuffer()
    {
        foreach (byte[] buffer in dataBufferList)
        {
            CheckConvertProcess(buffer);
        }
        dataBufferList.Clear();

        shouldProcessDataBuffer = false;
    }
    private void CheckConvertProcess(byte[] buffer)
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

            case PlayerData hostPlayer:
                PrepareGame(hostPlayer);
                break;

            default:
                Debug.Log("Could not convert " + value.ToString());
                break;
        }
    }
    private IPAddress FindIPV4Adress(IPHostEntry host)
    {
        foreach (IPAddress ip in host.AddressList)
        {
            if (ip.AddressFamily == AddressFamily.InterNetwork)
            {
                return ip;
            }
        }

        return null;
    }
    #endregion
    #endregion
}
