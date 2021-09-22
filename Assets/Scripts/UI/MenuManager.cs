using UnityEngine;
using UnityEngine.UI;

public class MenuManager : MonoBehaviour
{
    [SerializeField] private Button hostGame = null;
    [SerializeField] private Button joinGame = null;
    [SerializeField] private Button quit = null;
    [SerializeField] private InputField username = null;
    [SerializeField] private InputField serverIPAdress = null;

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
    }

    private void OnJoinGame()
    {
        if (PlayerAsUsername())
        {
            joinMenu.SetActive(true);
            gameObject.SetActive(false);
        }
        if (!PlayerAsUsername() || !CheckAdressIP())
            return;

        Debug.Log("Join game pressed.");
        player.username = username.text;
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
    public bool CheckAdressIP()
    {
        if (serverIPAdress.text == "")
        {
            Debug.Log("IP adress is not valid.");
            return false;
        }
        string[] splitValues = serverIPAdress.text.Split('.');
        if (splitValues.Length != 4)
        {
            Debug.Log("IP adress is not valid.");
            return false;
        }
        foreach (string value in splitValues)
        {
            byte r;
            if (!byte.TryParse(value, out r))
            {
                Debug.Log("IP adress is not valid.");
                return false;
            }
        }
        return true;
    }
}