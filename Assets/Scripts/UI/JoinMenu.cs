using UnityEngine;
using UnityEngine.UI;

public class JoinMenu : MonoBehaviour
{
    [SerializeField] private Button joinButton = null;
    [SerializeField] private Button backButton = null;
    [SerializeField] private InputField serverIPAdress = null;

    [SerializeField] private GameObject mainMenu = null;
    [SerializeField] private ChessGameMgr chessGameManager = null;
    [SerializeField] private GameObject scoreCanvas = null;

    [SerializeField] public Player player;
    private void Start()
    {
        joinButton.onClick.AddListener(OnJoin);
        backButton.onClick.AddListener(OnBack);
    }

    private void OnJoin()
    {
        if (!CheckAdressIP())
            return;
        Debug.Log("Join game pressed.");

        ClientScript cs = player.gameObject.GetComponent<ClientScript>();

        if (cs.enabled)
        {
            cs.Connect(serverIPAdress.text);
        }

        else
        {
            cs.enabled = true;
        }
        
        if (cs.connected)
        {
            chessGameManager.enabled = true;
            scoreCanvas.SetActive(true);
            GetComponent<Canvas>().enabled = false;
        }


    }
    private void OnBack()
    {
        mainMenu.SetActive(true);
        gameObject.SetActive(false);
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
