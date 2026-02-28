using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using Unity.VisualScripting;
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

    // Update is called once per frame
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
        while (timer_nogo_impulse < 2.0f) {
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
            reactionTimesMs.Add(reactionMs); // Просто добавляем в список
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

    private void OnEnable()
    {
        CoverTimer.start_test += ActivatePoint;
    }

    private void OnDisable()
    {
        CoverTimer.start_test -= ActivatePoint;
    }

    void ActivatePoint()
    {
        reactionStopwatch = new Stopwatch();
        rt = this.GetComponent<Transform>();
        b = this.GetComponent<Button>();
        iPoint = this.GetComponent<Image>();
        ChangePosition();
        b.onClick.AddListener(() => Move());
        deltatime = Time.deltaTime;
    }
}
