using UnityEngine;

/// <summary>
/// Responsible for assigning a persistent, unique ID key to clients
//  Also assigns a player name that can be changed at will.
/// </summary>
public static class PlayerIdentity
{
    private const string PlayerIdKey = "player_id";
    private const string PlayerNameKey = "player_name";

    public static string PlayerId { get; private set; }
    public static string PlayerName { get; private set; }

    /// <summary>
    /// Fetches the player's info if it exists, creates it if it does not
    /// Called at MainMenuManager's Awake()
    /// </summary>
    public static void InitializeIdentity()
    {
        // 1) Check if we already have an ID in PlayerPrefs
        if (PlayerPrefs.HasKey(PlayerIdKey))
        {
            PlayerId = PlayerPrefs.GetString(PlayerIdKey);
        }
        else
        {
            PlayerId = System.Guid.NewGuid().ToString();
            PlayerPrefs.SetString(PlayerIdKey, PlayerId);
            PlayerPrefs.Save();
        }

        // 2) Load or set default name
        if (PlayerPrefs.HasKey(PlayerNameKey))
        {
            PlayerName = PlayerPrefs.GetString(PlayerNameKey);
        }
        else
        {
            // Could set a default display name, e.g. "Player1234"
            PlayerName = "Player_" + PlayerId.Substring(0, 8);
            PlayerPrefs.SetString(PlayerNameKey, PlayerName);
            PlayerPrefs.Save();
        }
    }

    public static void SetPlayerName(string newName)
    {
        PlayerName = newName;
        PlayerPrefs.SetString(PlayerNameKey, newName);
        PlayerPrefs.Save();
    }
}
