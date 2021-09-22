using UnityEngine;
using UnityEngine.UI;

public class MenuManager : MonoBehaviour
{
    [SerializeField] private Button hostGame = null;
    [SerializeField] private Button joinGame = null;
    [SerializeField] private Button quit = null;
    [SerializeField] private InputField username = null;
    //[SerializeField] private InputField serverIPAdress = null;

    [SerializeField] private GameObject chessGameManager = null;
    [SerializeField] private GameObject scoreCanvas = null;
    [SerializeField] private GameObject joinMenu = null;


    [SerializeField] public Player player;

    private void Start()
    {
        chessGameManager.SetActive(false);
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
        player.username = username.text;
        chessGameManager.SetActive(true);
        scoreCanvas.SetActive(true);
        GetComponent<Canvas>().enabled = false;
        player.gameObject.GetComponent<ServerClientScript>().enabled = true;
    }

    private void OnJoinGame()
    {
        if (!PlayerAsUsername())
            return;

        Debug.Log("Join game pressed.");
        player.username = username.text;
        joinMenu.SetActive(true);
        gameObject.SetActive(false);
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