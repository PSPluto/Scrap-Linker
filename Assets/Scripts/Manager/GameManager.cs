using System;
using System.Collections;
using System.Threading;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public static class GameManager
{
    // public static float time = 20;
    public enum GameState
    {
        Title,
        InGame,
        GameOver,
        Clear,
    };

    public static GameState gameState =  GameState.Title;
    public static float count;

    /// <summary>
    /// ゲーム状態が変化したときに発火するイベント。
    /// ScoreManager（タイマー停止）や ClearDirector（クリア演出）はこれを購読する。
    /// 引数は変化後の GameState。
    /// </summary>
    public static event Action<GameState> OnStateChanged;

    public static GameStateChangeLog ChangeGameState(GameState nextState)
    {
        GameStateChangeLog returnState = new GameStateChangeLog { beforeState = gameState, afterState = nextState};
        gameState = nextState;
        CheckState();
        return returnState;
    }
    public static void  StartTimer()
    {
    }
    public static IEnumerator TimeCount(float time)
    {
        count = time;
        while (true)
        {
            if (count <= 0)
            {
                ChangeGameState(GameState.GameOver);
                yield break;
            }
            count -= 1f;
            yield return new WaitForSeconds(1f);
        }
    }
    private static void CheckState()
    {
        // 状態が変わるたびにリスナー（ScoreManagerのタイマー停止処理、
        // ClearDirectorのクリア演出呼び出し）へ通知する
        OnStateChanged?.Invoke(gameState);

        if (gameState == GameState.GameOver)
        {
            Debug.Log("ゲームオーバー");
            ChangeGameState(GameState.Title);
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }
        else if (gameState == GameState.Clear)
        {
            // シーン再読み込みは ClearDirector.ShowClear() 側で
            // 数秒待ってから行う（OnStateChanged経由で呼ばれる）
        }
    }


}

public struct GameStateChangeLog
{
    public GameManager.GameState beforeState;
    public GameManager.GameState afterState;
}