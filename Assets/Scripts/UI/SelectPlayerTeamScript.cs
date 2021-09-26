using System.Collections; using System.Collections.Generic; using UnityEngine; using UnityEngine.UI;  public class SelectPlayerTeamScript : MonoBehaviour {
    private Button whiteChoice = null;
    private Button randomChoice = null;
    private Button blackChoice = null;

    [SerializeField] private ServerClientScript server = null;
    // Start is called before the first frame update
    void Start()     {         whiteChoice = transform.GetChild(0).GetComponent<Button>();         randomChoice = transform.GetChild(1).GetComponent<Button>();         blackChoice = transform.GetChild(2).GetComponent<Button>();          whiteChoice.onClick.AddListener(WhiteClicked);         randomChoice.onClick.AddListener(RandomClicked);         blackChoice.onClick.AddListener(BlackClicked);     }
     private void WhiteClicked()
    {
        server.PrepareGame(ChessGameMgr.EChessTeam.White);
    }      private void RandomClicked()
    {
        System.Random rand = new System.Random();

        int intHostTeam = rand.Next(0, 2);

        server.PrepareGame((ChessGameMgr.EChessTeam)intHostTeam);
    }      private void BlackClicked()
    {
        server.PrepareGame(ChessGameMgr.EChessTeam.Black);
    } } 