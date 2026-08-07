using System.Collections;
using System.Threading;
using UnityEngine;
using UnityEngine.UI;

public static class GameManager
{
    public static float time = 20;
    public enum GameState
    {
        Title,
        InGame,
        GameOver,
        Clear,
    };

    public static GameState gameState =  GameState.Title;
    public static float count;

    public static GameStateChangeLog ChangeGameState(GameState nextState)
    {
        GameStateChangeLog returnState = new GameStateChangeLog { beforeState = gameState, afterState = nextState};
        gameState = nextState;
        return returnState;
    }
    public static void  StartTimer()
    {
    }
    public static IEnumerator TimeCount()
    {
        count = time;
        while (true)
        {
            if (count == 0)
            {
                GameManager.gameState = GameState.GameOver;
                yield break;
            }
            count -= 1f;
            yield return new WaitForSeconds(1f);
        }
    }

    private static void CheckState()
    {
        if (gameState == GameState.GameOver)
        {
            Time.timeScale = 0;
            Debug.Log("ゲームオーバー");
        }
    }


}

public struct GameStateChangeLog
{
    public GameManager.GameState beforeState;
    public GameManager.GameState afterState;
}

