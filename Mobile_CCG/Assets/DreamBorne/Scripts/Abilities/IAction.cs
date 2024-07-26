
public interface IActionClient
{
    public void Execute(PlayerEngine playerEngine, string jsonParams);
}

public interface IActionServer
{
    public void Execute(ServerEngine serverEngine, ServerData serverData, Card card);
}
