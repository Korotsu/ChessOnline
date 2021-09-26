using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;
using UnityEngine;

public class Player : MonoBehaviour
{
    [HideInInspector] public PlayerData playerData = new PlayerData();
    [SerializeField] private GameObject chatBox = null;
    [SerializeField] private GameObject inputField = null;
    [SerializeField] private float ocCooldown = 0;
    private float lastTimeSinceOC = 0;

    private void Update()
    {
        if (Input.GetButton("Exit"))
        {
            Application.Quit();
        }

        if (Input.GetButton("Open/CloseChat") && Time.realtimeSinceStartup - lastTimeSinceOC >= ocCooldown && !inputField.activeSelf)
        {
            if (chatBox.activeSelf)
            {
                chatBox.SetActive(false);
            }

            else
            {
                chatBox.SetActive(true);
            }
            lastTimeSinceOC = Time.realtimeSinceStartup;
        }
    }
}

[Serializable]
public struct PlayerData : ISerializable
{
    public string username;
    public ChessGameMgr.EChessTeam team;
    public bool isHost;

    public PlayerData(SerializationInfo info, StreamingContext ctxt)
    {
        username = (string)info.GetValue("username", typeof(string));
        team = (ChessGameMgr.EChessTeam)info.GetValue("team", typeof(ChessGameMgr.EChessTeam));
        isHost = (bool)info.GetValue("isHost", typeof(bool));
    }

    public void GetObjectData(SerializationInfo info, StreamingContext ctxt)
    {
        info.AddValue("username", username, typeof(string));
        info.AddValue("team", team, typeof(ChessGameMgr.EChessTeam));
        info.AddValue("isHost", isHost, typeof(bool));
    }
}

[Serializable]
public struct PlayerMessage : ISerializable
{
    public string username;
    public string message;
    public PlayerMessage(SerializationInfo info, StreamingContext ctxt)
    {
        username = (string)info.GetValue("username", typeof(string));
        message = (string)info.GetValue("message", typeof(string));
    }

    public void GetObjectData(SerializationInfo info, StreamingContext ctxt)
    {
        info.AddValue("username", username, typeof(string));
        info.AddValue("message", message, typeof(string));
    }
}