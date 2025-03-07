/// <summary>
/// Messages that can be sent from a client to the server
/// </summary>
public static class ClientMessages
{
    /// <summary>
    /// Returns the QUEUEUP message with the stored player identity
    /// </summary>
    public static string GetQueueUp()
    {
        return $"QUEUEUP|{PlayerIdentity.PlayerId}|{PlayerIdentity.PlayerName}";
    }
}
