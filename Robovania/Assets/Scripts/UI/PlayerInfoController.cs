using UnityEngine;

public class PlayerInfoController : MonoBehaviour {

    public int PlayerIndex;
    public UnityEngine.UI.InputField PlayerName;
    public UnityEngine.UI.Button PlayerColor;
    public UnityEngine.UI.Button PlayerController;
    public MainMenuController mainMenu;

    private GameSettings.PlayerInfo _player;

    public void Refresh()
    {
        _player = GameSettings.Instance.players[PlayerIndex];

        PlayerName.text = _player.Name;

        var colorBlock = PlayerColor.colors;
        colorBlock.normalColor = _player.Color;
        colorBlock.highlightedColor = _player.Color;
        PlayerColor.colors = colorBlock;

        PlayerController.GetComponentInChildren<UnityEngine.UI.Text>().text = (_player.ControllerProfile != null)
            ? _player.ControllerProfile.name
            : "None";
    }

    public void OnNameChanged()
    {
        _player.Name = PlayerName.text;
    }

    public void OnCycleColor()
    {
        _player.Color = mainMenu.GetNextColor(_player.Color);
        Refresh();
    }

    public void OnCycleControllerProfile()
    {
        _player.ControllerProfile = mainMenu.GetNextControllerProfile(_player.ControllerProfile);
        Refresh();
    }
}
