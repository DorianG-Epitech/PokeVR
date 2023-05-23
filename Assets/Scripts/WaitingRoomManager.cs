using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class WaitingRoomManager : MonoBehaviour
{
    private int _playerCount = 0;

    private void Start()
    {
        NetworkServer.OnConnectedEvent += OnConnected;
    }

    private void OnConnected(NetworkConnection conn)
    {
        _playerCount++;
        if (_playerCount == 2)
        {
            ChangeToGameScene();
        }
    }

    private void ChangeToGameScene()
    {
        NetworkManager.singleton.ServerChangeScene("Map");
    }
}
