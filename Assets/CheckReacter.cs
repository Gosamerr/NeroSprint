using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;
using Debug = UnityEngine.Debug;

public class CheckReacter : MonoBehaviour
{
    // Start is called before the first frame update

    bool isTimingReaction;

    [Header("Статистика")]
    [SerializeField] public int count_go_impulse = 1;
    [SerializeField] public int count_no_go_impulse = 0;
    [SerializeField] public int count_go_loose = 0;
    [SerializeField] public int count_no_go_loose = 0;

    public int SumImpulse => count_go_impulse + count_no_go_impulse;
    public int SumCorrect => SumImpulse - (count_go_loose + count_no_go_loose);
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


    void Start()
    {
        reactionStopwatch = new Stopwatch();
        
    }

    private void OnEnable()
    {
        TestLight.GreenStart += StartTimer;
    }

    private void OnDisable()
    {
        TestLight.GreenStart -= StartTimer;
    }

    // Update is called once per frame
    void Update()
    {
        count_no_go_impulse = testLight.timesChangecolor;

        if (Input.anyKey)
        {
            if(isTimingReaction)
            {
                StopTimer();

                count_go_impulse = 1;
            }
            else
            {
                if (count_no_go_loose < count_no_go_impulse)
                {
                    count_no_go_loose += 1;
                }
            }
            
        }

        debugSumImpulse = SumImpulse;
        debugSumCorrect = SumCorrect;
        debugResultCount = ResultCount;

    }


    void StartTimer()
    {
        reactionStopwatch.Reset();
        reactionStopwatch.Start();
        isTimingReaction = true;
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
            coverpanel.active = true;
            PointGo.score = (int)reactionMs;
            UserReact?.Invoke();
        }
    }
}
