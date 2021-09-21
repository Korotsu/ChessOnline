﻿using UnityEngine;

/*
 * Simple static camera
 */

public class CameraLookAt : MonoBehaviour
{
    [SerializeField]
    private Transform lookAt = null;
    [SerializeField]
    private float lookAtZ = 0.5f;
    [SerializeField]
    private float zPosition = 12f;
    [SerializeField]
    private float height = 32f;
    [SerializeField]
    private Player player = null;

    void Update ()
    {
        Vector3 position = transform.position;
        position.y = height;
        if (player.team == ChessGameMgr.EChessTeam.White)
            position.z = zPosition;
        else
            position.z = -zPosition;


        transform.position = position;
        transform.LookAt(lookAt.position + Vector3.up * lookAtZ);
	}
}
