using UnityEngine;
using UnityEngine.UI;

public class MenuManager : MonoBehaviour
{
    [SerializeField] private Button hostGame = null;
    [SerializeField] private Button joinGame = null;
    [SerializeField] private Button quit = null;
    [SerializeField] private InputField username = null;

    [SerializeField] private ChessGameMgr chessGameManager = null;
    [SerializeField] private GameObject scoreCanvas = null;
    [SerializeField] private GameObject joinMenu = null;


    [SerializeField] public Player player = null;

    private void Start()
    {
        chessGameManager.enabled = false;
        scoreCanvas.SetActive(false);
        joinMenu.SetActive(false);
        hostGame.onClick.AddListener(OnHostGame);
        joinGame.onClick.AddListener(OnJoinGame);
        quit.onClick.AddListener(OnQuit);
    }

    private void OnHostGame()
    {
        if (!PlayerAsUsername())
            return;

        Debug.Log("Host game pressed.");
        player.playerData.username = username.text;
        //chessGameManager.SetActive(true);
        scoreCanvas.SetActive(true);
        GetComponent<Canvas>().enabled = false;
        player.gameObject.GetComponent<ServerClientScript>().enabled = true;
        player.playerData.isHost = true;
        player.gameObject.GetComponent<ServerClientScript>().BroadCastData(player);
    }

    private void OnJoinGame()
    {
        if (!PlayerAsUsername())
            return;

        Debug.Log("Join game pressed.");
        player.playerData.username = username.text;
        joinMenu.SetActive(true);
        gameObject.SetActive(false);
        player.playerData.isHost = false;

        player.gameObject.GetComponent<ClientScript>().SendData(player.playerData);
    }
    private void OnQuit()
    {
        Debug.Log("Quit pressed.");
        Application.Quit();
    }

    public bool PlayerAsUsername()
    {
        if (username.text == "")
        {
            Debug.Log("Player has no username.");
            return false;
        }

        return true;
    }
}