using Mirror.Examples.Basic;
using TMPro;
using UnityEngine;

public class PlayerTPCooldownText : MonoBehaviour
{
    private GameObject[] players;
    private TextMeshProUGUI tMPro;

    void Start()
    {
        tMPro = GetComponent<TextMeshProUGUI>();
    }

    void Update()
    {
        players = GameObject.FindGameObjectsWithTag("Player");
        if (players.Length == 0)
        {
            return;
        }
        float minTime = players[0].GetComponent<Player>().playerTpCooldown;
        foreach (GameObject player in players)
        {
            if (player.GetComponent<Player>().getPlayerTpCurrCooldown() < minTime)
            {
                minTime = player.GetComponent<Player>().getPlayerTpCurrCooldown();
            }
        }
        if (minTime != players[0].GetComponent<Player>().playerTpCooldown)
        {
            tMPro.text = minTime.ToString("F1");
        }
        else
        {
            tMPro.text = "";
        }
    }
}
