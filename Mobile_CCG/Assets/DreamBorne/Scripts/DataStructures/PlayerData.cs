using Unity.Netcode;

public class PlayerData
{
    private string playerName;
    private int cashAmount;

    public PlayerData(string playerName)
    {
        this.playerName = playerName;
        this.cashAmount = 0;
    }

    public int CashAmount
    {
        get { return this.cashAmount; } 
        set { this.cashAmount = value; }
    }

    public string PlayerName
    {
        get { return this.playerName; }
    }
}
