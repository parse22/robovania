using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/* GAMEMANAGER:
 * Monobehaviour that exists on a GameObject that is saved
 * into each play mode scene. Primary play mode driver logic
 * that handles match start, camera, match end and scene transitions.
 * */
public class GameManager : MonoBehaviour
{
    public float startDelay = 3f;
    public float endDelay = 3f;
    public Transform[] spawnPoints;
    public CameraManager cameraManager;
    public GameObject robotPrefab;
    public Text messageText;

    private WaitForSeconds startWait;
    private WaitForSeconds endWait;
    private List<RoboController> controllers;

    void Start()
    {
        startWait = new WaitForSeconds(startDelay);
        endWait = new WaitForSeconds(endDelay);

        SpawnPlayers();
        SetCameraTargets();

        StartCoroutine(GameLoop());
    }

    private IEnumerator GameLoop()
    {
        GameSettings.Instance.OnBeginRound();

        yield return StartCoroutine(StartOfRound());

        yield return StartCoroutine(RoundPlaying());

        yield return StartCoroutine(RoundEnding());

        if(GameSettings.Instance.ShouldFinishGame())
        {
            SceneManager.LoadScene(2);
        }
        else
        {
            SceneManager.LoadScene(1, LoadSceneMode.Single);
        }
    }

    private IEnumerator StartOfRound()
    {
        DisableControllers();

        cameraManager.SetStartPositionAndSize();

        messageText.text = "ROUND " + GameState.Instance.roundNumber;

        yield return startWait;
    }

    private IEnumerator RoundPlaying()
    {
        EnableControllers();

        messageText.text = string.Empty;

        while(!GameSettings.Instance.ShouldFinishRound())
        {
            yield return null;
        }
    }

    private IEnumerator RoundEnding()
    {
        DisableControllers();

        var winner = GameSettings.Instance.OnEndRound();

        messageText.text = EndMessage(winner);

        yield return endWait;
    }

    private string EndMessage(RoboController winner)
    {
        return winner != null ? winner.player.playerInfo.Name + " WINS THE ROUND!" : "DRAW!";
    }

    private void SpawnPlayers()
    {
        var points = new List<Transform>(spawnPoints);
        controllers = new List<RoboController>();

        foreach(PlayerState playerState in GameState.Instance.players)
        {
            var spawnPointIndex = Random.Range(0, points.Count);

            var controller = gameObject.AddComponent<RoboController>();
            playerState.controller = controller;
            controllers.Add(controller);
            var pawnObject = (GameObject)Instantiate(robotPrefab);
            RoboPawn pawn = pawnObject.GetComponent<RoboPawn>();
            controller.Setup(pawn, playerState, points[spawnPointIndex]);

            points.RemoveAt(spawnPointIndex);
        }
    }

    private void SetCameraTargets()
    {
        foreach(RoboController controller in controllers)
        {
            cameraManager.SetTarget(controller.Pawn.transform);
        }
    }

    private void EnableControllers()
    {
        foreach(RoboController controller in controllers)
        {
            controller.enabled = true;
        }
    }

    private void DisableControllers()
    {
        foreach(RoboController controller in controllers)
        {
            controller.enabled = false;
        }
    }
}
