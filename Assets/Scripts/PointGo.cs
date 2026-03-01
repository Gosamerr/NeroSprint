using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using Debug = UnityEngine.Debug;
using Random = UnityEngine.Random;

public class PointGo : MonoBehaviour
{
    Transform rt;
    Button b;
    Image iPoint;

    [Header("Основные параметры")]
    [SerializeField] int times;
    static public int score = 0;
    static public event Action<int> AddScore;

    [Header("Таймер исчезновения")]
    private float idleTimer = 0f;
    private float maxIdleTime = 1f;
    private float deltatime = 0;

    [Header("Статистика")]
    [SerializeField] public int count_go_impulse = 0;
    [SerializeField] public int count_no_go_impulse = 0;
    [SerializeField] public int count_go_loose = 0;
    [SerializeField] public int count_no_go_loose = 0;

    // Свойства для статистики
    public int SumImpulse => count_go_impulse + count_no_go_impulse;
    public int SumCorrect => SumImpulse - (count_go_loose + count_no_go_loose);
    public float ResultCount => SumImpulse > 0 ? (float)SumCorrect / SumImpulse : 0f;

    [Header("Отладка статистики")]
    [SerializeField] private int debugSumImpulse;
    [SerializeField] private int debugSumCorrect;
    [SerializeField] private float debugResultCount;

    [Header("NoGo параметры")]
    bool flag_no_go_impulse = false;
    float timer_nogo_impulse = 0f;
    [SerializeField] Sprite no_go_impulse;

    [Header("Спрайты Go")]
    [SerializeField] Sprite first_go_impulse;
    [SerializeField] Sprite second_go_impulse;
    [SerializeField] Sprite third_go_impulse;
    [SerializeField] Sprite fourth_go_impulse;
    [SerializeField] Sprite fifth_go_impulse;

    [Header("Время реакции (мс)")]
    [SerializeField] public List<float> reactionTimesMs = new List<float>(); // Массив времен реакции

    // Для замера времени
    private Stopwatch reactionStopwatch;
    private bool isTimingReaction = false;

    private void OnEnable()
    {
        CoverTimer.start_test += ActivatePoint;
        MainTimer.TimeOver += OnTestFinished;   // Подписка на окончание теста
    }

    private void OnDisable()
    {
        CoverTimer.start_test -= ActivatePoint;
        MainTimer.TimeOver -= OnTestFinished;
    }

    void ActivatePoint()
    {
        reactionStopwatch = new Stopwatch();
        rt = GetComponent<Transform>();
        b = GetComponent<Button>();
        iPoint = GetComponent<Image>();
        ChangePosition();
        b.onClick.AddListener(() => Move());
        deltatime = Time.deltaTime;

        // Сброс статистики перед новым тестом
        count_go_impulse = 0;
        count_no_go_impulse = 0;
        count_go_loose = 0;
        count_no_go_loose = 0;
        reactionTimesMs.Clear();
        score = 0;  // Обнуляем счёт
    }

    // Вызывается по окончании теста (событие TimeOver)
    private void OnTestFinished()
    {
        int totalGo = count_go_impulse;
        int omission = count_go_loose;
        int commission = count_no_go_loose;

        float avgReactionTimeMs = reactionTimesMs.Count > 0 ? reactionTimesMs.Average() : 0f;

        float reactionTimeVariability = 0f;
        if (reactionTimesMs.Count > 1)
        {
            float avg = avgReactionTimeMs;
            float sumOfSquares = reactionTimesMs.Sum(t => (t - avg) * (t - avg));
            reactionTimeVariability = Mathf.Sqrt(sumOfSquares / (reactionTimesMs.Count - 1));
        }

        float overallAccuracy = totalGo > 0 ? (float)(totalGo - omission) / totalGo : 0f;

        // ===== НОВАЯ СБАЛАНСИРОВАННАЯ ФОРМУЛА =====
        float accuracyPoints = overallAccuracy * 800f;                     // максимум 800
        float speedPoints = avgReactionTimeMs > 0
            ? 200f * Mathf.Exp(-avgReactionTimeMs / 400f)                  // максимум 200
            : 0f;
        float penalty = omission * 10f + commission * 5f;                  // мягкие штрафы
        float calculatedScore = accuracyPoints + speedPoints - penalty;
        int finalScore = Mathf.RoundToInt(Mathf.Clamp(calculatedScore, 0f, 1000f));

        Debug.Log($"Рейтинг: {finalScore} (точность: {overallAccuracy:F2}, " +
                  $"время: {avgReactionTimeMs:F2} мс, пропуски: {omission})");

        if (DatabaseManager.Instance != null && DatabaseManager.CurrentUserId != -1)
        {
            bool saved = DatabaseManager.Instance.SaveTestResult(
                DatabaseManager.CurrentUserId,
                "PopTap",
                avgReactionTimeMs,
                omission,
                commission,
                reactionTimeVariability,
                overallAccuracy,
                finalScore
            );

            if (saved)
                Debug.Log("Результат PopTap сохранён в БД");
            else
                Debug.LogError("Не удалось сохранить результат PopTap");
        }
        else
        {
            Debug.LogWarning("Пользователь не авторизован или DatabaseManager отсутствует");
        }
    }

    void Update()
    {
        idleTimer += deltatime;

        debugSumImpulse = SumImpulse;
        debugSumCorrect = SumCorrect;
        debugResultCount = ResultCount;

        ScoreDepens();

        if (idleTimer >= maxIdleTime)
        {
            PointDisappeared();
        }
    }

    IEnumerator SetActiveNoGo()
    {
        float sec = Convert.ToSingle(Random.Range(1, 5));
        yield return new WaitForSeconds(sec);
        flag_no_go_impulse = true;
        SetNoGo();
        while (timer_nogo_impulse < 2.0f)
        {
            timer_nogo_impulse += deltatime;
        }
        flag_no_go_impulse = false;
    }

    void Move()
    {
        if (isTimingReaction && !flag_no_go_impulse)
        {
            reactionStopwatch.Stop();
            float reactionMs = reactionStopwatch.ElapsedMilliseconds;
            reactionTimesMs.Add(reactionMs);
            Debug.Log($"Время реакции: {reactionMs:F2} мс");
            isTimingReaction = false;
        }

        idleTimer = 0f;

        if (!flag_no_go_impulse)
        {
            SetGoSprite(times);
            if (times < 4)
            {
                AddScore?.Invoke(1);
                ChangePosition();
                times++;
                score++;
            }
            else
            {
                times = 0;
                score += 5;
                AddScore?.Invoke(5);
                ChangePosition();
            }
        }
        else
        {
            times = 0;
            score -= 5;
            AddScore?.Invoke(-5);
            flag_no_go_impulse = false;
        }
    }

    void ChangePosition()
    {
        count_go_impulse++;
        rt.position = new Vector3(Random.Range(70, 1850), Random.Range(100, 950), rt.position.z);
        StartReactionTimer();
    }

    void PointDisappeared()
    {
        AddScore?.Invoke(-2);
        score = Mathf.Max(0, score - 1);
        times = 0;
        count_go_loose++;

        SetGoSprite(times);

        if (isTimingReaction)
        {
            reactionStopwatch.Stop();
            isTimingReaction = false;
        }

        ChangePosition();

        idleTimer = 0f;
    }

    void StartReactionTimer()
    {
        reactionStopwatch.Reset();
        reactionStopwatch.Start();
        isTimingReaction = true;
    }

    void SetGoSprite(int time)
    {
        switch (time)
        {
            case 0:
                iPoint.sprite = first_go_impulse;
                rt.localScale = new Vector3(1, 1, 1);
                break;
            case 1:
                iPoint.sprite = second_go_impulse;
                rt.localScale = new Vector3(1.2f, 1.2f, 1.2f);
                break;
            case 2:
                iPoint.sprite = third_go_impulse;
                rt.localScale = new Vector3(1.4f, 1.4f, 1.4f);
                break;
            case 3:
                iPoint.sprite = fourth_go_impulse;
                rt.localScale = new Vector3(1.6f, 1.6f, 1.6f);
                break;
            case 4:
                iPoint.sprite = fifth_go_impulse;
                rt.localScale = new Vector3(1.8f, 1.8f, 1.8f);
                break;
        }
    }

    void SetNoGo()
    {
        if (iPoint.sprite != no_go_impulse)
        {
            iPoint.sprite = no_go_impulse;
        }
    }

    void ScoreDepens()
    {
        switch (score)
        {
            case 10:
                maxIdleTime = 0.93f;
                break;
            case 20:
                maxIdleTime = 0.81f;
                //StartCoroutine(SetActiveNoGo());
                break;
            case 39:
                maxIdleTime = 0.65f;
                //StartCoroutine(SetActiveNoGo());
                break;
            case 49:
                maxIdleTime = 0.52f;
                break;
        }
    }
}