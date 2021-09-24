using UnityEngine;
using UnityEngine.UI;
using System.Collections;

/*
 * Simple GUI display : scores and team turn
 */

public class GUIMgr : MonoBehaviour
{

    #region singleton
    static GUIMgr instance = null;
    public static GUIMgr Instance
    {
        get
        {
            if (instance == null)
                instance = FindObjectOfType<GUIMgr>();
            return instance;
        }
    }
    #endregion

    Transform playerTurnTr = null;
    Text player1ScoreText = null;
    Text player2ScoreText = null;

    Text playerTurnText = null;

    [SerializeField] public Player player1 = null;
    [SerializeField] public Player player2 = null;

    // Use this for initialization
    void Awake()
    {
        playerTurnTr = transform.Find("PlayerTurnText");

        playerTurnText = playerTurnTr.GetComponent<Text>();

        playerTurnTr.gameObject.SetActive(false);

        player1ScoreText = transform.Find("Player1ScoreText").GetComponent<Text>();
        player2ScoreText = transform.Find("Player2ScoreText").GetComponent<Text>();

        ChessGameMgr.Instance.OnPlayerTurn += DisplayTurn;
        ChessGameMgr.Instance.OnScoreUpdated += UpdateScore;
    }
	
    void DisplayTurn(bool isPlayerMove)
    {
        playerTurnTr.gameObject.SetActive(true);
        if(isPlayerMove)
            playerTurnText.text = player1.playerData.username + " Turn.";
        else
            playerTurnText.text = player2.playerData.username + " Turn.";
    }

    void UpdateScore(uint player1Score, uint player2Score)
    {
        player1ScoreText.text = string.Format(player1.playerData.username + " : {0}", player1Score);
        player2ScoreText.text = string.Format(player2.playerData.username + " : {0}", player2Score);
    }
}
