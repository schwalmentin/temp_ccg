
public interface IActionClient
{
    public void Execute(PlayerEngine playerEngine, string jsonParams, bool isOpponent);
}

public interface IActionServer
{
    public void Execute(ServerEngine serverEngine, ulong playerId, Card card);
}
