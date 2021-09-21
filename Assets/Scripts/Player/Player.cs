using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    [System.NonSerialized] public string username = "Guest";
    [SerializeField] public ChessGameMgr.EChessTeam team;
}