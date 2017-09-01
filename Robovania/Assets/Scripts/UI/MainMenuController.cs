using System;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuController : MonoBehaviour
{
    public GameSettings gameSettingsTemplate;

    public Color[] availableColors;
    public ControlProfile[] availableControllerProfiles;
    
    public GameObject playersPanel;

    public UnityEngine.UI.Text numberOfRoundsLabel;
    public UnityEngine.UI.Slider numberOfRoundsSlider;

    public string SavedSettingsPath
    {
        get
        {
            return System.IO.Path.Combine(Application.persistentDataPath, "robosmash-settings.json");
        }
    }

    private void Start()
    {
        if(System.IO.File.Exists(SavedSettingsPath))
        {
            GameSettings.LoadFromJSON(SavedSettingsPath);
        }
        else
        {
            GameSettings.InitializeFromDefault(gameSettingsTemplate);
        }

        foreach (var info in GetComponentsInChildren<PlayerInfoController>())
            info.Refresh();

        numberOfRoundsSlider.value = GameSettings.Instance.numberOfRounds;
    }

    public void Play()
    {
        GameSettings.Instance.SaveToJSON(SavedSettingsPath);
        GameState.CreateFromSettings(GameSettings.Instance);
        SceneManager.LoadScene(1, LoadSceneMode.Single);
    }

    public void Quit()
    {
        Application.Quit();
    }

    public Color GetNextColor(Color color)
    {
        int existingColor = Array.FindIndex(availableColors, c => c == color);
        existingColor = (existingColor + 1) % availableColors.Length;
        return availableColors[existingColor];
    }

    public ControlProfile GetNextControllerProfile(ControlProfile controllerProfile)
    {
        if(controllerProfile == null)
            return availableControllerProfiles[0];

        int index = Array.FindIndex(availableControllerProfiles, b => b == controllerProfile);
        index++;
        return (index < availableControllerProfiles.Length) ? availableControllerProfiles[index] : null;
    }

    public void OnChangeNumberOfRounds(float value)
    {
        GameSettings.Instance.numberOfRounds = (int)value;
        numberOfRoundsLabel.text = GameSettings.Instance.numberOfRounds.ToString();
    }
}
