using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ChatScript : MonoBehaviour
{
    [SerializeField] private GameObject inputText = null;
    [SerializeField] private InputField inputField = null;
    [SerializeField] private Player player = null;
    private Text text = null;
    private float timeSinceLastOSChat = 0;
    [SerializeField] private float osChatCooldown = 1;
    // Start is called before the first frame update
    void Start()
    {
        inputText = transform.GetChild(2).gameObject;
        inputField = inputText.GetComponent<InputField>();
        text = transform.GetChild(0).transform.GetChild(0).gameObject.GetComponent<Text>();
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetButton("SendMessage") && Time.realtimeSinceStartup - timeSinceLastOSChat >= osChatCooldown)
        {
            if (inputText.activeSelf)
            {
                AddAndSendText();
                inputText.SetActive(false);
            }

            else
            {
                inputText.SetActive(true);
            }

            timeSinceLastOSChat = Time.realtimeSinceStartup;
        }
    }

    void AddAndSendText()
    {
        AddText(player.playerData.username + ": " + inputField.text);
        inputField.text = "";

        if (player.playerData.isHost)
        {
            player.GetComponent<ServerClientScript>().BroadCastData(inputField.text);
        }

        else
        {
            player.GetComponent<ClientScript>().SendData(inputField.text);
        }
    }
    public void AddText(string text_)
    {
        text_ += '\n';

        text.text += text_;
    }
}
