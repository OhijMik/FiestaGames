using UnityEngine;
using Mirror;
using System.Threading;
using Mirror.Examples.Basic;

public class CameraController : MonoBehaviour
{
    MyNetworkManager networkManager;
    GameObject[] players;

    private void Start()
    {
        DontDestroyOnLoad(this.gameObject);
        networkManager = GameObject.Find("Network Manager Kcp").GetComponent<MyNetworkManager>();
    }

    private void Update()
    {
        players = networkManager.GetPlayers();
        int playerCount = networkManager.GetPlayerCount();
        if (playerCount != 0)
        {
            Vector3 totalPos = new Vector3(0, 0, 0);
            for (int i = 0; i < playerCount; i++)
            {
                totalPos += players[i].transform.position;
            }
            Vector3 pos = totalPos / playerCount;
            pos.z -= 25;
            pos.y += 15;
            transform.position = pos;
        }

    }
}
