using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using UnityEngine;
using Debug = UnityEngine.Debug;

public class CheckReacter : MonoBehaviour
{
    bool isTimingReaction;

    [Header("Статистика")]
    [SerializeField] public int count_go_impulse = 0;      // количество целевых сигналов (Go)
    [SerializeField] public int count_no_go_impulse = 0;   // количество нецелевых сигналов (NoGo)
    [SerializeField] public int count_go_loose = 0;        // пропуски Go (omission)
    [SerializeField] public int count_no_go_loose = 0;     // ложные срабатывания на NoGo (commission)

    public int SumImpulse => count_go_impulse + count_no_go_impulse;
    public int SumCorrect => (count_go_impulse - count_go_loose) + (count_no_go_impulse - count_no_go_loose);
    public float ResultCount => SumImpulse > 0 ? (float)SumCorrect / SumImpulse : 0f;

    [Header("Отладка статистики")]
    [SerializeField] private int debugSumImpulse;
    [SerializeField] private int debugSumCorrect;
    [SerializeField] private float debugResultCount;

    [Header("Время реакции (мс)")]
    [SerializeField] public List<float> reactionTimesMs = new List<float>();
    private Stopwatch reactionStopwatch;

    [SerializeField] private TestLight testLight;
    [SerializeField] private GameObject coverpanel;

    public static event Action UserReact;

    [Header("Сохранение результатов")]
    [SerializeField] private string testName = "RidersTest";
    private bool reacted; // была ли реакция на текущий зелёный

    void Start()
    {
        reactionStopwatch = new Stopwatch();
    }

    private void OnEnable()
    {
        TestLight.GreenStart += StartTimer;
        TestLight.TestCompleted += OnTestCompleted;
        CoverTimer.start_test += ResetTest;
    }

    private void OnDisable()
    {
        TestLight.GreenStart -= StartTimer;
        TestLight.TestCompleted -= OnTestCompleted;
        CoverTimer.start_test -= ResetTest;
    }

    void Update()
    {
        // Определяем количество нецелевых стимулов (все смены цвета, кроме последней)
        if (testLight != null)
            count_no_go_impulse = Mathf.Max(0, testLight.timesChangecolor - 1);

        // Ловим нажатия
        if (Input.anyKeyDown)
        {
            if (isTimingReaction)
            {
                // Нажали на зелёный – правильная реакция
                StopTimer();
                count_go_impulse++;
                reacted = true;
            }
            else
            {
                // Нажали не на зелёный – commission error
                if (count_no_go_loose < count_no_go_impulse)
                {
                    count_no_go_loose++;
                }
            }
        }

        // Отладка
        debugSumImpulse = SumImpulse;
        debugSumCorrect = SumCorrect;
        debugResultCount = ResultCount;
    }

    void StartTimer()
    {
        reactionStopwatch.Reset();
        reactionStopwatch.Start();
        isTimingReaction = true;
        reacted = false; // сбрасываем флаг для нового зелёного
    }

    void StopTimer()
    {
        if (isTimingReaction)
        {
            reactionStopwatch.Stop();
            float reactionMs = reactionStopwatch.ElapsedMilliseconds;
            reactionTimesMs.Add(reactionMs);
            Debug.Log($"Время реакции: {reactionMs:F2} мс");
            isTimingReaction = false;
            coverpanel.SetActive(true);
            PointGo.score = (int)reactionMs;
            UserReact?.Invoke();
        }
    }

    private void ResetTest()
    {
        count_go_impulse = 0;
        count_no_go_impulse = 0;
        count_go_loose = 0;
        count_no_go_loose = 0;
        reactionTimesMs.Clear();
        reacted = false;
    }

    private void OnTestCompleted()
    {
        int totalStimuli = testLight != null ? testLight.timesChangecolor : 0;

        // Ошибка пропуска (omission) – количество пропущенных целевых сигналов
        int omissionErrors = count_go_loose; // если вы увеличиваете count_go_loose при пропуске

        // Если вы не увеличиваете count_go_loose, используйте флаг reacted:
        // int omissionErrors = reacted ? 0 : 1;

        // Среднее время реакции
        float avgReactionTimeMs = reactionTimesMs.Count > 0 ? reactionTimesMs.Average() : 0f;

        // Вариабельность (стандартное отклонение) – пока оставляем 0
        float reactionTimeVariability = 0f;
        if (reactionTimesMs.Count > 1)
        {
            float avg = avgReactionTimeMs;
            float sumOfSquares = reactionTimesMs.Sum(t => (t - avg) * (t - avg));
            reactionTimeVariability = Mathf.Sqrt(sumOfSquares / (reactionTimesMs.Count - 1));
        }

        // Правильные ответы
        int correctGo = count_go_impulse - count_go_loose;
        int correctNoGo = count_no_go_impulse - count_no_go_loose;
        int totalCorrect = correctGo + correctNoGo;
        float overallAccuracy = totalStimuli > 0 ? (float)totalCorrect / totalStimuli : 0f;

        // ========== РАСЧЁТ ИТОГОВОГО СЧЁТА (score) ==========
        float scoreValue = 0f;

        if (overallAccuracy > 0 || reactionTimesMs.Count > 0)
        {
            // Базовая часть за точность (максимум 700)
            float accuracyPoints = overallAccuracy * 700f;

            // Бонус за скорость (максимум 300)
            float speedPoints = 0f;
            if (avgReactionTimeMs > 0)
            {
                // Экспоненциальная формула: быстрое угасание бонуса с ростом времени
                // При 0 мс -> 300, при 400 мс -> ~110, при 800 мс -> ~40
                speedPoints = 300f * Mathf.Exp(-avgReactionTimeMs / 400f);

                // Альтернатива (линейная):
                // speedPoints = Mathf.Max(0, 300f * (1f - avgReactionTimeMs / 1000f));
            }

            // Штраф за ошибки
            float penalty = omissionErrors * 150f + count_no_go_loose * 75f;

            // Итог с ограничением от 0 до 1000
            scoreValue = accuracyPoints + speedPoints - penalty;
            scoreValue = Mathf.Clamp(scoreValue, 0f, 1000f);
        }

        int finalScore = Mathf.RoundToInt(scoreValue);
        Debug.Log($"Итоговый счёт: {finalScore} (точность: {overallAccuracy:F2}, время: {avgReactionTimeMs:F2} мс, ошибки: omission={omissionErrors}, commission={count_no_go_loose})");

        // Сохраняем результат в БД
        if (DatabaseManager.Instance != null && DatabaseManager.CurrentUserId != -1)
        {
            bool saved = DatabaseManager.Instance.SaveTestResult(
                DatabaseManager.CurrentUserId,
                testName,
                avgReactionTimeMs,
                omissionErrors,
                count_no_go_loose,
                reactionTimeVariability,
                overallAccuracy,
                finalScore   // ← передаём вычисленный счёт
            );

            if (saved)
                Debug.Log("Результат теста сохранён в БД");
            else
                Debug.LogError("Не удалось сохранить результат");
        }
        else
        {
            Debug.LogWarning("Пользователь не авторизован или DatabaseManager отсутствует");
        }
    }
}