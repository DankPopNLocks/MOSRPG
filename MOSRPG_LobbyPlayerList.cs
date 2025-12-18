using UdonSharp;
using UnityEngine;
using TMPro;
using VRC.SDKBase;
using VRC.Udon;

public class MOSRPG_LobbyPlayerList : UdonSharpBehaviour
{
    [Header("Dependencies")]
    public MOSRPG_RoleManager roleManager; // Updated type

    [Header("Display Settings")]
    public int roleIndex = 1;
    public TextMeshProUGUI outputText;

    // ---------------- Refresh ----------------

    public void RefreshList()
    {
        if (outputText == null || roleManager == null) return;

        int playerCount = VRCPlayerApi.GetPlayerCount();
        if (playerCount <= 0)
        {
            outputText.text = "";
            return;
        }

        VRCPlayerApi[] players = new VRCPlayerApi[playerCount];
        VRCPlayerApi.GetPlayers(players);

        string result = "";

        for (int i = 0; i < playerCount; i++)
        {
            VRCPlayerApi p = players[i];
            if (!Utilities.IsValid(p)) continue;

            if (roleManager.IsPlayerInRole(p.playerId, roleIndex))
            {
                if (string.IsNullOrEmpty(result))
                    result = p.displayName;
                else
                    result += "\n" + p.displayName;
            }
        }

        outputText.text = result;
        Debug.Log($"[MOSRPG_LobbyPlayerList] Role {roleIndex} players:\n{result}");
    }

    // ---------------- Udon Events ----------------

    public override void OnDeserialization()
    {
        RefreshList();
    }

    public override void OnPlayerJoined(VRCPlayerApi player)
    {
        RefreshList();
    }

    public override void OnPlayerLeft(VRCPlayerApi player)
    {
        RefreshList();
    }
}
