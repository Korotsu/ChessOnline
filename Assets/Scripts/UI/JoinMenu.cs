using UnityEngine;
using UnityEngine.UI;

public class JoinMenu : MonoBehaviour
{
    [SerializeField] private Button joinButton = null;
    [SerializeField] private Button backButton = null;
    [SerializeField] private InputField serverIPAdress = null;

    [SerializeField] private GameObject mainMenu = null;
    [SerializeField] private GameObject clientCanvas = null;
    [SerializeField] private GameObject board = null;
    [SerializeField] public Player player = null;
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

        if (!cs.enabled)
        {
            cs.enabled = true;
        }

        if (!cs.connected)
        {
            cs.Connect(serverIPAdress.text);
        }

        if (cs.connected)
        {
            cs.SendData(player.playerData);

            board.SetActive(true);
            clientCanvas.SetActive(true);
            clientCanvas.transform.GetChild(0).gameObject.SetActive(true);
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
