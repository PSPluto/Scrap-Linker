using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

public class ClearDirector : MonoBehaviour
{
    public static ClearDirector Instance { get; private set; }
    [SerializeField] private AudioClip clearSE;

    [Header("表示するテキスト（TextMeshProUGUI）")]
    [SerializeField] private TMP_Text clearText;
    [SerializeField] private RectTransform timeText;

    [Header("テキスト表示後、シーン再読み込みまでの待機秒数")]
    [SerializeField] private float waitSeconds = 3.0f;

    [Header("フェードインにかける秒数")]
    [SerializeField] private float fadeDuration = 0.5f;

    private bool isPlaying = false;

    private void Awake()
    {
        // シングルトン化（他スクリプトから呼びやすくするため）
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        if (clearText != null)
        {
            clearText.gameObject.SetActive(false);
        }
    }

    private void OnEnable()
    {
        GameManager.OnStateChanged += HandleStateChanged;
    }

    private void OnDisable()
    {
        GameManager.OnStateChanged -= HandleStateChanged;
    }

    /// <summary>
    /// GameManagerの状態がClearになったタイミングで自動的に演出を開始する
    /// </summary>
    private void HandleStateChanged(GameManager.GameState state)
    {
        if (state == GameManager.GameState.Clear)
        {
            ShowClear();
        }
    }

    /// <summary>
    /// クリア演出を開始する（手動で呼び出したい場合はこちらもOK）
    /// </summary>
    public void ShowClear()
    {
        if (isPlaying) return; // 二重再生防止
        isPlaying = true;
        StartCoroutine(ClearSequence());
        AudioManager.Instance.PlayClearSequence(clearSE);
    }

    private System.Collections.IEnumerator ClearSequence()
    {
        if (clearText == null)
        {
            Debug.LogWarning("ClearDirector: clearText が設定されていません。");
        }
        else
        {
            clearText.gameObject.SetActive(true);

            // フェードイン
            Color c = clearText.color;
            float t = 0f;
            c.a = 0f;
            clearText.color = c;

            while (t < fadeDuration)
            {
                t += Time.deltaTime;
                c.a = Mathf.Clamp01(t / fadeDuration);
                clearText.color = c;
                timeText.anchoredPosition = Vector2.Lerp(timeText.anchoredPosition ,new Vector2(0,-100), 0.1f);

                yield return null;
            }

            c.a = 1f;
            clearText.color = c;
            Time.timeScale = 0f;
        }

        // 数秒待機
        yield return new WaitForSeconds(waitSeconds);

        // シーン再読み込み
        Scene currentScene = SceneManager.GetActiveScene();
        SceneManager.LoadScene(currentScene.name);
    }
}
