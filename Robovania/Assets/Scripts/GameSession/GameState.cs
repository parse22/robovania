using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Assertions;

/* GAMESTATE:
 * Current state of the current match, primarily transient
 * data used during match to help drive GameManager, and post match
 * to populate any player profile persistent data.
 * */
public class GameState : ScriptableObject
{
    #region Singleton
    private static GameState _instance;
    public static GameState Instance
    {
        get
        {
            if (!_instance)
            {
                GameState[] gamestates = Resources.FindObjectsOfTypeAll<GameState>();
                _instance = gamestates.FirstOrDefault();
            }

#if UNITY_EDITOR
            if (!_instance || _instance.players.Count == 0)
                CreateFromSettings(GameSettings.Instance);
#endif
            return _instance;
        }
    }
    #endregion

    #region Member Variables
    #region Public
    public List<PlayerState> players;

    public int roundNumber;
    #endregion
    #endregion

    public static void CreateFromSettings(GameSettings settings)
    {
        Assert.IsNotNull(settings);

        _instance = CreateInstance<GameState>();
        _instance.hideFlags = HideFlags.HideAndDontSave;

        _instance.players = new List<PlayerState>();
        foreach(var playerInfo in settings.players)
        {
            if (playerInfo.ControllerProfile)
            {
                var playerState = new PlayerState { playerInfo = playerInfo };
                _instance.players.Add(playerState);
            }
        }
    }

    public PlayerState GetPlayerState(RoboController controller)
    {
        return players.FirstOrDefault(p => p.controller == controller);
    }

#if UNITY_EDITOR
    [UnityEditor.MenuItem("Window/Game State")]
    public static void ShowGameState()
    {
        UnityEditor.Selection.activeObject = Instance;
    }
#endif

    public PlayerState GetPlayerWithMostWins()
    {
        players.Sort((a, b) => Comparer<int>.Default.Compare(b.totalWins, a.totalWins));
        if (players.Count > 1 && players[0].totalWins == players[1].totalWins)
            return null;
        return players[0];
    }
}
