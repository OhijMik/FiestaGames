using Mirror.Examples.MultipleAdditiveScenes;
using UnityEngine;

public class WinPlatform : MonoBehaviour
{
    private GameObject[] players;
    private int numPlayer;
    private float winCooldown = 1;


    void Start()
    {
        numPlayer = 0;
    }

    void Update()
    {
        players = GameObject.FindGameObjectsWithTag("Player");

        if (players.Length == numPlayer)
        {
            winCooldown -= Time.deltaTime;
        }

        if (winCooldown < 0)
        {
            print("You win");
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.tag == "Player")
        {
            numPlayer += 1;
        }
    }

    private void OnCollisionExit(Collision collision)
    {
        if (collision.gameObject.tag == "Player")
        {
            numPlayer -= 1;
        }
    }
}
