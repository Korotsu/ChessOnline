using UnityEngine;
using UnityEngine.UI;

public class JoinMenu : MonoBehaviour
{
    [SerializeField] private Button joinButton = null;
    [SerializeField] private Button backButton = null;
    [SerializeField] private InputField serverIPAdress = null;
    [SerializeField] private GameObject mainMenu = null;
    private void Start()
    {
        joinButton.onClick.AddListener(OnJoin);
        backButton.onClick.AddListener(OnBack);
    }

    private void OnJoin()
    { 
    }
    private void OnBack()
    {
        mainMenu.SetActive(true);
        gameObject.SetActive(false);
    }
}
