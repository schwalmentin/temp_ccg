using System.Collections.Generic;

public class ServerDataProxy
{
    private ServerData serverData;
    public int Mana => this.serverData.Mana;
    public PlayerPhase PlayerPhase => this.serverData.PlayerPhase;
    
    // rework maybe
    public IEnumerable<Card> Hand => this.serverData.Hand;
    public Card[,] Field => this.serverData.Field;
    public ServerDataProxy(ServerData serverData)
    {
        this.serverData = serverData;
    }
}
