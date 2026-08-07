using System.Collections;
using UnityEngine;

public static class GameManager
{
    public static float Time = 240;
    public enum GameState
    {
        Title,
        InGame,
        GameOver,
        Clear,
    };

    public static GameState gameState =  GameState.Title;

    public static GameStateChangeLog ChangeGameState(GameState nextState)
    {
        GameStateChangeLog returnState = new GameStateChangeLog { beforeState = gameState, afterState = nextState};
        gameState = nextState;
        return returnState;
    }
    public static void  StartTimer()
    {
    }
    static IEnumerator TimeCount()
    {
        float count = Time;
        while (true)
        {
            if (count == 0)
            {
                GameManager.gameState = GameState.GameOver;
                yield break;
            }
            count -= 0.1f;
            yield return new WaitForSeconds(0.1f);
        }
    }
    
}

public struct GameStateChangeLog
{
    public GameManager.GameState beforeState;
    public GameManager.GameState afterState;
}

