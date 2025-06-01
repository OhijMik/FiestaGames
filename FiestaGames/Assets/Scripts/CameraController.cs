using UnityEngine;
using Mirror;

public class CameraController : NetworkBehaviour
{
    MyNetworkManager networkManager;
    GameObject[] players;

    private Vector3 syncedCameraPosition;

    private void Start()
    {
        // DontDestroyOnLoad(this.gameObject);
        networkManager = GameObject.Find("Network Manager Kcp").GetComponent<MyNetworkManager>();
    }

    private void Update()
    {
        if (!isServer) { return; }
        print("hi");
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
            syncedCameraPosition = pos;
        }
        transform.position = syncedCameraPosition;

    }
}
