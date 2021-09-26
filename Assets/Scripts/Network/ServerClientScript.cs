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
    #region Variables

    private Socket socket;
    private IPEndPoint localEP;
    private int port = 11000;
    private List<Socket> clientSocketList = new List<Socket>();
    private bool connected = false;
    public ChessGameMgr chessGameMgr = null;
    private int nbPlayer = 0;
    private bool initialized = false;
    private ChessGameMgr.EChessTeam hostTeam = ChessGameMgr.EChessTeam.None;
    private bool shouldPlayTurn = false;
    private ChessGameMgr.Move lastClientMove = new ChessGameMgr.Move();
    private bool shouldProcessDataBuffer = false;
    private List<byte[]> dataBufferList = new List<byte[]>();

    [SerializeField] private GUIMgr guiMgr = null;
    [SerializeField] private Player player1 = null;
    [SerializeField] private Player player2 = null;
    [SerializeField] private GameObject scoreCanvas = null;

    #endregion

    #region MonoBehaviors

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
            Debug.Log("error = " + e.ToString());
        }
    }
    private void Update()
    {
        if (connected && nbPlayer == 2 && !initialized)
        {
            PrepareGame();
        }

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
    #region Message
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
    #endregion

    #region Data
    public void SendData<T>(T _data, Socket client)
    {
        Byte[] dataMSG = SerializationTools.Serialize<T>(_data);

        try
        {
            client.Send(dataMSG);
        }
        catch (Exception e)
        {
            Debug.Log("error = " + e.ToString());
        }
    }
    public void BroadCastData<T>(T _data)
    {
        foreach (Socket client in clientSocketList)
        {
            SendData<T>(_data, client);
        }
    }
    #endregion

    #region Callback
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

                if (read <= StateObject.BUFFER_SIZE)
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

    #region Other
    private void PrepareGame()
    {
        System.Random rand = new System.Random();

        int intHostTeam = rand.Next(0, 2);
        hostTeam = (ChessGameMgr.EChessTeam) intHostTeam; 

        player2.playerData.team = (ChessGameMgr.EChessTeam)(1 - intHostTeam);
        player2.playerData.username = player1.playerData.username;

        GetComponent<Player>().playerData.team = hostTeam;
        BroadCastData(player2.playerData);
        
        chessGameMgr.enabled = true;
        scoreCanvas.SetActive(true);
        
        initialized = true;
    }
    private void PlayTurn()
    {
        chessGameMgr.PlayTurn(lastClientMove, (hostTeam == ChessGameMgr.EChessTeam.White) ? ChessGameMgr.EChessTeam.Black : ChessGameMgr.EChessTeam.White);
        chessGameMgr.UpdatePieces();
        
        shouldPlayTurn = false;
    }
    private void ProcessDataBuffer()
    {
        foreach (byte[] buffer in dataBufferList)
        {
            checkConvertProcess(buffer);
        }
        dataBufferList.Clear();
        
        shouldProcessDataBuffer = false;
    }
    private void checkConvertProcess(byte[] buffer)
    {
        var value = SerializationTools.Deserialize(buffer);

        switch (value)
        {
            case ChessGameMgr.Move move:
                if (chessGameMgr)
                {
                    lastClientMove = move;
                    shouldPlayTurn = true;
                }
                break;

            case PlayerData player:
                player2.playerData = player;
                guiMgr.shouldUpdateUI = true;
                break;

            default:
                Debug.Log("Could not convert " + value.ToString());
                break;
        }
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
                    Debug.Log("error = " + e.ToString());
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
    #endregion
    #endregion
}

#region OtherClasses
public class StateObject
{
    public Socket workSocket = null;
    public const int BUFFER_SIZE = 1024;
    public byte[] buffer = new byte[BUFFER_SIZE];
    public StringBuilder stringBuilder = new StringBuilder();
}
#endregion