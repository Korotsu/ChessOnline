using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;
using UnityEngine;

public class Player : MonoBehaviour
{
    [System.NonSerialized] public PlayerData playerData = new PlayerData(); 
}

[Serializable]
public class PlayerData : ISerializable
{
    [System.NonSerialized] public string username = "Guest";
    [System.NonSerialized] public ChessGameMgr.EChessTeam team = ChessGameMgr.EChessTeam.None;
    [System.NonSerialized] public bool isHost = true;

    public PlayerData() { }

    public PlayerData(SerializationInfo info, StreamingContext ctxt)
    {
        username = (string)info.GetValue("username", typeof(string));
        team = (ChessGameMgr.EChessTeam)info.GetValue("team", typeof(ChessGameMgr.EChessTeam));
        isHost = (bool)info.GetValue("isHost", typeof(bool));
    }

    public void GetObjectData(SerializationInfo info, StreamingContext ctxt)
    {
        info.AddValue("username", typeof(string));
        info.AddValue("team", typeof(ChessGameMgr.EChessTeam));
        info.AddValue("isHost", typeof(bool));
    }
}