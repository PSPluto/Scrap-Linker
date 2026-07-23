using UnityEngine;
using System.Collections;
using static GameManager;

public class TimeManager : MonoBehaviour
{
    public float Time;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        Time = 5;
        StartCoroutine(TimeCount());
    }
    IEnumerator TimeCount()
    {
        for (int Count = 0; Count <= Time; Count++)
        {
            Debug.Log(Count);
            yield return new WaitForSeconds(1); // 1秒待機
            if (Count == Time)
            {
                GameManager.gameState = GameState.GameOver;
            }
        }
    }
}
