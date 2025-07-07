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
        float initTime = players[0].GetComponent<Player>().playerTpCooldown;
        float minTime = initTime;
        foreach (GameObject player in players)
        {
            if (player.GetComponent<Player>().getPlayerTpCurrCooldown() < minTime)
            {
                minTime = player.GetComponent<Player>().getPlayerTpCurrCooldown();
            }
        }
        if (minTime != initTime)
        {
            tMPro.text = minTime.ToString("F1");
            byte ratio = (byte)(minTime / initTime * 255);
            tMPro.color = new Color32(255, ratio, ratio, 255);
        }
        else
        {
            tMPro.text = "";
        }
    }
}
