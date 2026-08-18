using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using TMPro;
using UnityEngine;
using UnityEngine.Timeline;

public class ScoreManager : MonoBehaviour
{
    public static ScoreManager Instance{get; private set; }
    
    [SerializeField] private int sec = 180;
    private TMP_Text scoreText;
    private Coroutine timeCountCoroutine = null;

    // Start is called before the first frame update
    private void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        scoreText = GetComponent<TMP_Text>();
        scoreText.text = "移動でスタート！";
    }

    private void OnEnable()
    {
        GameManager.OnStateChanged += HandleStateChanged;
    }

    private void OnDisable()
    {
        GameManager.OnStateChanged -= HandleStateChanged;
    }

    public void StartCount()
    {
        if (timeCountCoroutine != null)
        {
            return;
        }
        timeCountCoroutine = StartCoroutine(GameManager.TimeCount(sec));
    }

    /// <summary>
    /// GameOver または Clear になったらタイマーを止める
    /// </summary>
    private void HandleStateChanged(GameManager.GameState state)
    {
        if (state == GameManager.GameState.GameOver || state == GameManager.GameState.Clear)
        {
            if (timeCountCoroutine != null)
            {
                StopCoroutine(timeCountCoroutine);
                timeCountCoroutine = null;
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (GameManager.gameState == GameManager.GameState.GameOver || GameManager.gameState == GameManager.GameState.Clear)
        {
            return;
        }
        if (timeCountCoroutine == null)
        {
            scoreText.text = ($"移動でタイマースタート！");
        }
        else
        {
            scoreText.text = ($"残り{GameManager.count.ToString()}秒");
        }
    }
}