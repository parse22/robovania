using UnityEngine;
using System.Collections;
using System.Linq;
using System.Text;
using UnityEngine.SceneManagement;

public class GameOverController : MonoBehaviour
{

    public UnityEngine.UI.Text winnerLabel;
    public UnityEngine.UI.Text scores;

    public void OnBackToMainMenu()
    {
        DestroyImmediate(GameState.Instance);
        SceneManager.LoadScene(0);
    }

    public void Start()
    {
        var winner = GameSettings.Instance.GetGameWinner();
        if (winner != null && winner.playerInfo != null)
        {
            winnerLabel.text = winner.playerInfo.Name + " is the winner!";
        }
        else
        {
            winnerLabel.text = "It's a draw!";
        }

        var sb = new StringBuilder();
        foreach (var player in GameState.Instance.players.OrderByDescending(p => p.totalWins))
        {
            if(player != null && player.playerInfo != null)
            {
                sb.Append(player.playerInfo.Name + " " + player.totalWins + " wins\n");
            }
        }
        scores.text = sb.ToString();
    }
}