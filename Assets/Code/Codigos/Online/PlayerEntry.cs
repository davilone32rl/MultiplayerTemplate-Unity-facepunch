using TMPro;
using UnityEngine;

public class PlayerEntry : MonoBehaviour
{
    public TextMeshProUGUI playerNameText;
    public void SetPlayerName(string playerName)
    {
        playerNameText.text = playerName;
    }
}
