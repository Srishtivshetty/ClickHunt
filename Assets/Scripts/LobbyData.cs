using System;

[System.Serializable]
public class LobbyData
{
    public int coins = 500;
    public int loginDayIndex = 0;
    public string lastClaimDate = "";
    public string nextRefillTime = "";
    public int remainingAttempts = 3; // optional if you want attempts here too
}
