using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;
using UnityEngine;

[Serializable]
public class Player : ISerializable
{
    [System.NonSerialized] public string username = "Guest";
    [SerializeField] public ChessGameMgr.EChessTeam team;

    [System.NonSerialized] public bool isHost = true;

    public Player(SerializationInfo info, StreamingContext ctxt)
    {
        username    = (string)                  info.GetValue("username", typeof(string));
        team        = (ChessGameMgr.EChessTeam) info.GetValue("team"    , typeof(ChessGameMgr.EChessTeam));
        isHost      = (bool)                    info.GetValue("isHost"  , typeof(bool));
    }

    public void GetObjectData(SerializationInfo info, StreamingContext ctxt)
    {
        info.AddValue("username"    , typeof(string));
        info.AddValue("team"        , typeof(ChessGameMgr.EChessTeam));
        info.AddValue("isHost"      , typeof(bool));
    }
}