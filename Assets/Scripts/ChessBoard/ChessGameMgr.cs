using System;
using UnityEngine;
using System.Collections.Generic;
using System.Runtime.Serialization;

/*
 * This singleton manages the whole chess game
 *  - board data (see BoardState class)
 *  - piece models instantiation
 *  - player interactions (piece grab, drag and release)
 *  - AI update calls (see UpdateAITurn and ChessAI class)
 */

public partial class ChessGameMgr : MonoBehaviour
{

    #region singleton
    static ChessGameMgr instance = null;
    public static ChessGameMgr Instance {
        get
        {
            if (instance == null)
                instance = FindObjectOfType<ChessGameMgr>();
            return instance;
        }
    }
    #endregion

    //[SerializeField]
    //private bool IsAIEnabled = false;
    [SerializeField] private Player player = null;
    private ChessAI chessAI = null;
    private Transform boardTransform = null;
    private static int BOARD_SIZE = 8;
    private int pieceLayerMask;
    private int boardLayerMask;

    #region enums
    public enum EPieceType : uint
    {
        Pawn = 0,
        King,
        Queen,
        Rook,
        Knight,
        Bishop,
        NbPieces,
        None
    }

    public enum EChessTeam
    {
        White = 0,
        Black,
        None
    }

    public enum ETeamFlag : uint
    {
        None = 1 << 0,
        Friend = 1 << 1,
        Enemy = 1 << 2
    }
    #endregion

    #region structs and classes
    public struct BoardSquare
    {
        public EPieceType Piece;
        public EChessTeam Team;

        public BoardSquare(EPieceType p, EChessTeam t)
        {
            Piece = p;
            Team = t;
        }

        static public BoardSquare Empty()
        {
            BoardSquare res;
            res.Piece = EPieceType.None;
            res.Team = EChessTeam.None;
            return res;
        }
    }

    [Serializable]
    public struct Move : ISerializable
    {
        public int From;
        public int To;

        public override bool Equals(object o)
        {
            try
            {
                return (bool)(this == (Move)o);
            }
            catch
            {
                return false;
            }
        }

        public override int GetHashCode()
        {
            return From + To;
        }

        public static bool operator ==(Move move1, Move move2)
        {
            return move1.From == move2.From && move1.To == move2.To;
        }

        public static bool operator !=(Move move1, Move move2)
        {
            return move1.From != move2.From || move1.To != move2.To;
        }

        public Move(SerializationInfo info, StreamingContext ctxt)
        {
            From = (int)info.GetValue("From", typeof(int));
            To = (int)info.GetValue("To", typeof(int));
        }

        public void GetObjectData(SerializationInfo info, StreamingContext ctxt)
        {
            info.AddValue("From", From, typeof(int));
            info.AddValue("To", To, typeof(int));
        }
    }

    #endregion

    #region chess game methods

    BoardState boardState = null;
    public BoardState GetBoardState() { return boardState; }

    EChessTeam teamTurn;

    List<uint> scores;
    uint player1Score = 0;
    uint player2Score = 0;

    public delegate void PlayerTurnEvent(bool isWhiteMove);
    public event PlayerTurnEvent OnPlayerTurn = null;

    public delegate void ScoreUpdateEvent(uint player1Score, uint player2Score);
    public event ScoreUpdateEvent OnScoreUpdated = null;

    public void PrepareGame(bool resetScore = true)
    {
        chessAI = ChessAI.Instance;

        // Start game
        boardState.Reset();
        teamTurn = EChessTeam.White;
        if (scores == null)
        {
            scores = new List<uint>();
            scores.Add(0);
            scores.Add(0);
        }
        if (resetScore)
        {
            scores.Clear();
            scores.Add(0);
            scores.Add(0);
            player1Score = 0;
            player2Score = 0;
        }
    }

    public void PlayTurn(Move move, EChessTeam team)
    {
        if (boardState.IsValidMove(teamTurn, move))
        {
            BoardState.EMoveResult result = boardState.PlayUnsafeMove(move);
            if (result == BoardState.EMoveResult.Promotion)
            {
                // instantiate promoted queen gameobject
                AddQueenAtPos(move.To);
            }

            EChessTeam otherTeam = (teamTurn == EChessTeam.White) ? EChessTeam.Black : EChessTeam.White;
            if (boardState.DoesTeamLose(otherTeam))
            {
                // increase score and reset board
                if (teamTurn == player.team)
                    player1Score++;
                else
                    player2Score++;

                //scores[(int)teamTurn]++;
                if (OnScoreUpdated != null)
                    OnScoreUpdated(player1Score, player2Score);

                PrepareGame(false);
                // remove extra piece instances if pawn promotions occured
                teamPiecesArray[0].ClearPromotedPieces();
                teamPiecesArray[1].ClearPromotedPieces();
            }
            else
            {
                teamTurn = otherTeam;
            }
            // raise event
            if (OnPlayerTurn != null)
                OnPlayerTurn(teamTurn == player.team);
        }
    }

    public void CheckOnlineState(Move move)
    {
        PlayTurn(move,player.team);

        UpdatePieces();

        if (player.isHost == true)
        {
            player.gameObject.GetComponent<ServerClientScript>().BroadCastData(move);    
        }

        else
        {
            player.gameObject.GetComponent<ClientScript>().SendData(move);
        }
    }

    // used to instantiate newly promoted queen
    private void AddQueenAtPos(int pos)
    {
        teamPiecesArray[(int)teamTurn].AddPiece(EPieceType.Queen);
        GameObject[] crtTeamPrefabs = (teamTurn == EChessTeam.White) ? WhitePiecesPrefab : BlackPiecesPrefab;
        GameObject crtPiece = Instantiate(crtTeamPrefabs[(uint)EPieceType.Queen]);
        teamPiecesArray[(int)teamTurn].StorePiece(crtPiece, EPieceType.Queen);
        crtPiece.transform.position = GetWorldPos(pos);
    }

    public bool IsPlayerTurn()
    {
        return teamTurn == player.team;
    }

    public BoardSquare GetSquare(int pos)
    {
        return boardState.Squares[pos];
    }

    public uint GetScore(EChessTeam team)
    {
        return scores[(int)team];
    }

    private void UpdateBoardPiece(Transform pieceTransform, int destPos)
    {
        pieceTransform.position = GetWorldPos(destPos);
    }

    private Vector3 GetWorldPos(int pos)
    {
        Vector3 piecePos = boardTransform.position;
        piecePos.y += zOffset;
        piecePos.x = -widthOffset + pos % BOARD_SIZE;
        piecePos.z = -widthOffset + pos / BOARD_SIZE;

        return piecePos;
    }

    private int GetBoardPos(Vector3 worldPos)
    {
        int xPos = Mathf.FloorToInt(worldPos.x + widthOffset) % BOARD_SIZE;
        int zPos = Mathf.FloorToInt(worldPos.z + widthOffset);

        return xPos + zPos * BOARD_SIZE;
    }

    #endregion

    #region MonoBehaviour

    private TeamPieces[] teamPiecesArray = new TeamPieces[2];
    private float zOffset = 0.5f;
    private float widthOffset = 3.5f;

    void Start()
    {
        pieceLayerMask = 1 << LayerMask.NameToLayer("Piece");
        boardLayerMask = 1 << LayerMask.NameToLayer("Board");

        boardTransform = GameObject.FindGameObjectWithTag("Board").transform;

        LoadPiecesPrefab();

        boardState = new BoardState();

        PrepareGame();

        teamPiecesArray[0] = null;
        teamPiecesArray[1] = null;

        CreatePieces();



        if (OnPlayerTurn != null)
            OnPlayerTurn(teamTurn == player.team);
        if (OnScoreUpdated != null)
            OnScoreUpdated(scores[0], scores[1]);
    }

    void Update()
    {
       if(teamTurn == player.team)
            UpdatePlayerTurn();
    }
    #endregion

    #region pieces

    GameObject[] WhitePiecesPrefab = new GameObject[6];
    GameObject[] BlackPiecesPrefab = new GameObject[6];

    void LoadPiecesPrefab()
    {
        GameObject prefab = Resources.Load<GameObject>("Prefabs/Pieces/WhitePawn");
        WhitePiecesPrefab[(uint)EPieceType.Pawn] = prefab;
        prefab = Resources.Load<GameObject>("Prefabs/Pieces/WhiteKing");
        WhitePiecesPrefab[(uint)EPieceType.King] = prefab;
        prefab = Resources.Load<GameObject>("Prefabs/Pieces/WhiteQueen");
        WhitePiecesPrefab[(uint)EPieceType.Queen] = prefab;
        prefab = Resources.Load<GameObject>("Prefabs/Pieces/WhiteRook");
        WhitePiecesPrefab[(uint)EPieceType.Rook] = prefab;
        prefab = Resources.Load<GameObject>("Prefabs/Pieces/WhiteKnight");
        WhitePiecesPrefab[(uint)EPieceType.Knight] = prefab;
        prefab = Resources.Load<GameObject>("Prefabs/Pieces/WhiteBishop");
        WhitePiecesPrefab[(uint)EPieceType.Bishop] = prefab;

        prefab = Resources.Load<GameObject>("Prefabs/Pieces/BlackPawn");
        BlackPiecesPrefab[(uint)EPieceType.Pawn] = prefab;
        prefab = Resources.Load<GameObject>("Prefabs/Pieces/BlackKing");
        BlackPiecesPrefab[(uint)EPieceType.King] = prefab;
        prefab = Resources.Load<GameObject>("Prefabs/Pieces/BlackQueen");
        BlackPiecesPrefab[(uint)EPieceType.Queen] = prefab;
        prefab = Resources.Load<GameObject>("Prefabs/Pieces/BlackRook");
        BlackPiecesPrefab[(uint)EPieceType.Rook] = prefab;
        prefab = Resources.Load<GameObject>("Prefabs/Pieces/BlackKnight");
        BlackPiecesPrefab[(uint)EPieceType.Knight] = prefab;
        prefab = Resources.Load<GameObject>("Prefabs/Pieces/BlackBishop");
        BlackPiecesPrefab[(uint)EPieceType.Bishop] = prefab;
    }

    void CreatePieces()
    {
        // Instantiate all pieces according to board data
        if (teamPiecesArray[0] == null)
            teamPiecesArray[0] = new TeamPieces();
        if (teamPiecesArray[1] == null)
            teamPiecesArray[1] = new TeamPieces();

        GameObject[] crtTeamPrefabs = null;
        int crtPos = 0;
        foreach (BoardSquare square in boardState.Squares)
        {
            crtTeamPrefabs = (square.Team == EChessTeam.White) ? WhitePiecesPrefab : BlackPiecesPrefab;
            if (square.Piece != EPieceType.None)
            {
                GameObject crtPiece = Instantiate(crtTeamPrefabs[(uint)square.Piece]);
                teamPiecesArray[(int)square.Team].StorePiece(crtPiece, square.Piece);

                // set position
                Vector3 piecePos = boardTransform.position;
                piecePos.y += zOffset;
                piecePos.x = -widthOffset + crtPos % BOARD_SIZE;
                piecePos.z = -widthOffset + crtPos / BOARD_SIZE;
                crtPiece.transform.position = piecePos;
            }
            crtPos++;
        }
    }

    public void UpdatePieces()
    {
        teamPiecesArray[0].Hide();
        teamPiecesArray[1].Hide();

        for (int i = 0; i < boardState.Squares.Count; i++)
        {
            BoardSquare square = boardState.Squares[i];
            if (square.Team == EChessTeam.None)
                continue;

            int teamId = (int)square.Team;
            EPieceType pieceType = square.Piece;

            teamPiecesArray[teamId].SetPieceAtPos(pieceType, GetWorldPos(i));
        }
    }

    #endregion

    #region gameplay

    Transform grabbed = null;
    float maxDistance = 100f;
    int startPos = 0;
    int destPos = 0;

    void UpdateAITurn()
    {
        Move move = chessAI.ComputeMove();
        //PlayTurn(move);

        UpdatePieces();
    }

    void UpdatePlayerTurn()
    {
        if (Input.GetMouseButton(0))
        {
            if (grabbed)
                ComputeDrag();
            else
                ComputeGrab();
        }
        else if (grabbed != null)
        {
            // find matching square when releasing grabbed piece
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit, maxDistance, boardLayerMask))
            {
                grabbed.root.position = hit.transform.position + Vector3.up * zOffset;
            }

            destPos = GetBoardPos(grabbed.root.position);
            if (startPos != destPos)
            {
                Move move = new Move();
                move.From = startPos;
                move.To = destPos;

                CheckOnlineState(move);
            }
            else
            {
                grabbed.root.position = GetWorldPos(startPos);
            }
            grabbed = null;
        }
    }

    void ComputeDrag()
    {
        // drag grabbed piece on board
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit, maxDistance, boardLayerMask))
        {
            grabbed.root.position = hit.point;
        }
    }

    void ComputeGrab()
    {
        // grab a new chess piece from board
        RaycastHit hit;
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out hit, maxDistance, pieceLayerMask))
        {
            grabbed = hit.transform;
            startPos = GetBoardPos(hit.transform.position);
        }
    }

    #endregion
}
