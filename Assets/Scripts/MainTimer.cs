using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SocialPlatforms.Impl;
using UnityEngine.UI;

public class MainTimer : MonoBehaviour
{
    [Header("Основные настройки")]
    [SerializeField] private float timeInSeconds = 120f; // 2 минуты

    [Header("Настройки слайдера")]
    [SerializeField] private Slider timerSlider;
    [SerializeField] private bool smoothDecrease = true;

    public static event Action TimeOver;
    private bool isRunning;
    private float currentTime;
    private Coroutine timerCoroutine;

    [SerializeField]
    private GameObject point;
    [SerializeField]
    private GameObject mainTimer;
    [SerializeField]
    private GameObject score;
    [SerializeField]
    private GameObject coverpanel;

    void Start()
    {
        if (timerSlider == null)
            timerSlider = GetComponent<Slider>();

        InitializeTimer();
    }

    void OnEnable()
    {
        CoverTimer.start_test += StartTimer;
    }

    void OnDisable()
    {
        CoverTimer.start_test -= StartTimer;
        StopTimer();
    }

    void InitializeTimer()
    {
        if (timerSlider != null)
        {
            timerSlider.maxValue = timeInSeconds;
            timerSlider.value = timeInSeconds;
        }
        currentTime = timeInSeconds;
    }

    void StartTimer()
    {
        if (isRunning) return;

        isRunning = true;
        currentTime = timeInSeconds;

        if (timerSlider != null)
            timerSlider.value = timeInSeconds;

        // Останавливаем предыдущую корутину если была
        if (timerCoroutine != null)
            StopCoroutine(timerCoroutine);

        timerCoroutine = StartCoroutine(RunTimer());
    }

    IEnumerator RunTimer()
    {
        while (isRunning && currentTime > 0)
        {
            // Уменьшаем время
            currentTime -= Time.deltaTime;

            // Обновляем слайдер
            if (timerSlider != null)
            {
                if (smoothDecrease)
                {
                    // Плавное изменение
                    timerSlider.value = Mathf.Lerp(timerSlider.value, currentTime, Time.deltaTime * 5f);
                }
                else
                {
                    timerSlider.value = currentTime;
                }
            }

            // Проверка на окончание
            if (currentTime <= 0)
            {
                currentTime = 0;
                isRunning = false;
                OnTimerComplete();
            }

            yield return null; // Ждем каждый кадр
        }
    }

    void OnTimerComplete()
    {
        Debug.Log("Время вышло!");

        coverpanel.active = true;
        score.active = false;
        point.active = false;
        TimeOver?.Invoke();
        mainTimer.active = false;

    }

    public void StopTimer()
    {
        isRunning = false;
        if (timerCoroutine != null)
        {
            StopCoroutine(timerCoroutine);
            timerCoroutine = null;
        }
    }

    public void ResetTimer()
    {
        StopTimer();
        currentTime = timeInSeconds;
        if (timerSlider != null)
            timerSlider.value = timeInSeconds;
    }
}