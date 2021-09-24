using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;
using UnityEngine;

public class Player : MonoBehaviour
{
    [HideInInspector] public PlayerData playerData = new PlayerData(); 
}

[Serializable]
public struct PlayerData : ISerializable
{
    public string username;   
    public ChessGameMgr.EChessTeam team;
    public bool isHost;

    public PlayerData(SerializationInfo info, StreamingContext ctxt)
    {
        team = (ChessGameMgr.EChessTeam)info.GetValue("team", typeof(ChessGameMgr.EChessTeam));
        isHost = (bool)info.GetValue("isHost", typeof(bool));
        username = (string)info.GetValue("username", typeof(string));
    }

    public void GetObjectData(SerializationInfo info, StreamingContext ctxt)
    {
        info.AddValue("username", typeof(string));
        info.AddValue("team", typeof(ChessGameMgr.EChessTeam));
        info.AddValue("isHost", typeof(bool));
    }
}