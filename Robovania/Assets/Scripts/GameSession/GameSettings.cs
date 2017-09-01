using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/* GAMESETTINGS:
 * Json backed ScriptableObject that is set in Main Menu
 * and holds Player settings as well as match preferences
 * like rounds per match. Could also be extended to store
 * map selections or game rule preferences. Serializes settings
 * so that the next time the player returns to the menu the 
 * settings are retained.
 * */
[CreateAssetMenu]
public class GameSettings : ScriptableObject
{
    #region Class Definitions
    [Serializable]
	public class PlayerInfo
    {
        public string Name;
        public Color Color;

        [SerializeField] private string ControllerProfileName;

        private ControlProfile _controllerProfile;
        public ControlProfile ControllerProfile
        {
            get
            {
                if(!_controllerProfile && !String.IsNullOrEmpty(ControllerProfileName))
                {
                    ControlProfile[] availableControllerProfiles;

#if UNITY_EDITOR
                    availableControllerProfiles = UnityEditor.AssetDatabase.FindAssets("t:ControlProfile")
                        .Select(guid => UnityEditor.AssetDatabase.GUIDToAssetPath(guid))
                        .Select(path => UnityEditor.AssetDatabase.LoadAssetAtPath<ControlProfile>(path))
                        .Where(b => b).ToArray();
#else
                    availableControllerProfiles = Resources.FindObjectsOfTypeAll<ControlProfile>();
#endif
                    _controllerProfile = availableControllerProfiles.FirstOrDefault(b => b.name == ControllerProfileName);
                }
                return _controllerProfile;
            }

            set
            {
                _controllerProfile = value;
                ControllerProfileName = value ? value.name : String.Empty;
            }
        }
    }
    #endregion

    #region Singleton
    private static GameSettings _instance;
    public static GameSettings Instance
    {
        get
        {
            if (!_instance)
                _instance = Resources.FindObjectsOfTypeAll<GameSettings>().FirstOrDefault();
#if UNITY_EDITOR
            if (!_instance)
                InitializeFromDefault(UnityEditor.AssetDatabase.LoadAssetAtPath<GameSettings>("Assets/Data/TestGameSettings.asset"));
#endif
            return _instance;
        }
    }
    #endregion

    #region Member Variables
    public List<PlayerInfo> players;

    public int numberOfRounds;
    #endregion

    public static void LoadFromJSON(string path)
    {
        if (_instance)
        {
            DestroyImmediate(_instance);
        }
        _instance = ScriptableObject.CreateInstance<GameSettings>();
        JsonUtility.FromJsonOverwrite(System.IO.File.ReadAllText(path), _instance);
        _instance.hideFlags = HideFlags.HideAndDontSave;
    }

    public void SaveToJSON(string path)
    {
        Debug.LogFormat("Saving game settings to {0}", path);
        System.IO.File.WriteAllText(path, JsonUtility.ToJson(this, true));
    }

    public static void InitializeFromDefault(GameSettings settings)
    {
        if (_instance)
        {
            DestroyImmediate(_instance);
        }
        _instance = Instantiate(settings);
        _instance.hideFlags = HideFlags.HideAndDontSave;
    }

#if UNITY_EDITOR
    [UnityEditor.MenuItem("Window/Game Settings")]
    public static void ShowGameSettings()
    {
        UnityEditor.Selection.activeObject = Instance;
    }
#endif

    public bool ShouldFinishGame()
    {
        return GameState.Instance.roundNumber >= numberOfRounds;
    }

    public void OnBeginRound()
    {
        ++GameState.Instance.roundNumber;
    }

    public RoboController OnEndRound()
    {
        var winner = GameState.Instance.players.FirstOrDefault(t => t.IsAlive);

        if (winner != null)
            winner.totalWins++;

        return winner != null ? winner.controller : null;
    }

    public bool ShouldFinishRound()
    {
        return GameState.Instance.players.Count(p => p.IsAlive) <= 1;
    }

    public PlayerState GetGameWinner()
    {
        return GameState.Instance.GetPlayerWithMostWins();
    }
}
