using System.Collections;
using System.Collections.Generic;
using System.Threading;
using TMPro;
using UnityEngine;
using UnityEngine.Timeline;

public class ScoreManager : MonoBehaviour
{
    private TMP_Text scoreText;
    // Start is called before the first frame update
    void Start()
    {
        scoreText = GetComponent<TMP_Text>();
        scoreText.text = "score:0";
        StartCount();
    }

    void StartCount()
    {
        StartCoroutine(GameManager.TimeCount(60*3));
    }
    // Update is called once per frame
    void Update()
    {
        scoreText.text = GameManager.count.ToString();
    }
}