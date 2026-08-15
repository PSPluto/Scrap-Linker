using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class AudioManager : MonoBehaviour
{
    private HashSet<AudioRequestElement> _frameAudioRequestElements = new HashSet<AudioRequestElement>();
    [SerializeField] private AudioSource audioSource;

    [Header("BGM")]
    [SerializeField] private AudioSource bgmSource;
    [SerializeField] private float defaultFadeDuration = 1.0f;

    [Header("Clear SE 設定")]
    [SerializeField] private float clearFadeOutDuration = 0.5f;

    private Coroutine _bgmFadeCoroutine;
    private float _bgmBaseVolume = 1f;

    public static AudioManager Instance { get; private set; }

    private void Awake()
    {
        Instance = this;

        if (bgmSource == null)
        {
            bgmSource = gameObject.AddComponent<AudioSource>();
        }
        bgmSource.loop = true; // BGMは常にループ
        bgmSource.playOnAwake = false;

        _bgmBaseVolume = bgmSource.volume;
    }

    private void LateUpdate()
    {
        foreach (AudioRequestElement frameAudioRequestElement in _frameAudioRequestElements)
        {
            audioSource.pitch = Random.Range(0.9f, 1f);
            PlaySound(frameAudioRequestElement.audioClip, frameAudioRequestElement.position);
        }
        _frameAudioRequestElements.Clear();
    }

    public void PlayAudioOneShot(AudioClip clip, Vector3 position)
    {
        _frameAudioRequestElements.Add(new AudioRequestElement { audioClip = clip, position = position });
    }

    public void PlaySound(AudioClip clip, Vector3 position)
    {
        transform.position = position;
        audioSource.PlayOneShot(clip);
    }

    // ----------------------------------------------------
    // BGM 再生系(常にループ再生)
    // ----------------------------------------------------

    /// <summary>
    /// BGMをクロスフェードしながら切り替えて再生する(常にループ)
    /// </summary>
    public void PlayBGM(AudioClip clip, float fadeDuration = -1f)
    {
        if (clip == null) return;
        if (fadeDuration < 0f) fadeDuration = defaultFadeDuration;

        if (_bgmFadeCoroutine != null)
            StopCoroutine(_bgmFadeCoroutine);

        _bgmFadeCoroutine = StartCoroutine(CrossFadeBGM(clip, fadeDuration));
    }

    /// <summary>
    /// BGMを止める(フェードアウト付き)
    /// </summary>
    public void StopBGM(float fadeDuration = -1f)
    {
        if (fadeDuration < 0f) fadeDuration = defaultFadeDuration;

        if (_bgmFadeCoroutine != null)
            StopCoroutine(_bgmFadeCoroutine);

        _bgmFadeCoroutine = StartCoroutine(FadeOutAndStop(fadeDuration));
    }

    private IEnumerator CrossFadeBGM(AudioClip newClip, float duration)
    {
        if (bgmSource.isPlaying)
        {
            yield return StartCoroutine(FadeVolume(bgmSource, bgmSource.volume, 0f, duration));
        }

        bgmSource.clip = newClip;
        bgmSource.volume = 0f;
        bgmSource.Play(); // loop = true は Awake で設定済み

        yield return StartCoroutine(FadeVolume(bgmSource, 0f, _bgmBaseVolume, duration));
    }

    private IEnumerator FadeOutAndStop(float duration)
    {
        yield return StartCoroutine(FadeVolume(bgmSource, bgmSource.volume, 0f, duration));
        bgmSource.Stop();
        bgmSource.volume = _bgmBaseVolume;
    }

    private IEnumerator FadeVolume(AudioSource source, float from, float to, float duration)
    {
        if (duration <= 0f)
        {
            source.volume = to;
            yield break;
        }

        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            source.volume = Mathf.Lerp(from, to, elapsed / duration);
            yield return null;
        }
        source.volume = to;
    }

    // ----------------------------------------------------
    // クリアSE(BGMをフェードアウトして鳴らすだけ)
    // ----------------------------------------------------

    /// <summary>
    /// BGMをフェードアウトしてクリアSEを再生する
    /// </summary>
    public void PlayClearSequence(AudioClip clearSE, Action onComplete = null)
    {
        StartCoroutine(ClearSequenceRoutine(clearSE, onComplete));
    }

    private IEnumerator ClearSequenceRoutine(AudioClip clearSE, Action onComplete)
    {
        // 1. BGMをフェードアウトして停止
        if (_bgmFadeCoroutine != null)
            StopCoroutine(_bgmFadeCoroutine);

        yield return StartCoroutine(FadeVolume(bgmSource, bgmSource.volume, 0f, clearFadeOutDuration));
        bgmSource.Stop();

        // 2. クリアSEを再生
        if (clearSE != null)
        {
            audioSource.pitch = 1f; // クリアSEはピッチランダム化しない
            audioSource.PlayOneShot(clearSE);
        }

        onComplete?.Invoke();
    }

    // ----------------------------------------------------
    // 音量調整
    // ----------------------------------------------------
    public void SetBGMVolume(float volume)
    {
        _bgmBaseVolume = Mathf.Clamp01(volume);
        bgmSource.volume = _bgmBaseVolume;
    }
}