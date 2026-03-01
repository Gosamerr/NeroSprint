using System;
using System.Collections;
using TMPro;
using UnityEngine;

public class CoverTimer : MonoBehaviour
{
    private TextMeshProUGUI timer;
    public static event Action start_test;

    [SerializeField] private GameObject point;
    [SerializeField] private GameObject mainTimer;
    [SerializeField] private GameObject score;

    private bool testFinished = false; // флаг, что тест завершён

    void Start()
    {
        timer = GetComponent<TextMeshProUGUI>();
        StartCoroutine(ChangeNums());
    }

    private void OnEnable()
    {
        MainTimer.TimeOver += SetScore;
        CheckReacter.UserReact += SetScore;
        // Если тест уже завершён, обновляем текст при активации панели
        if (testFinished) SetScore();
    }

    private void OnDisable()
    {
        MainTimer.TimeOver -= SetScore;
        CheckReacter.UserReact -= SetScore;
    }

    private void SetScore()
    {
        testFinished = true;
        Debug.Log($"[{Time.time}] SetScore: PointGo.score = {PointGo.score}");
        timer.text = PointGo.score.ToString();
    }

    IEnumerator ChangeNums()
    {
        yield return new WaitForSeconds(0.9f);
        timer.text = "2";
        timer.transform.localScale = new Vector3(1.2f, 1.2f, 1.2f);
        yield return new WaitForSeconds(0.9f);
        timer.transform.localScale = new Vector3(1.5f, 1.5f, 1.5f);
        timer.text = "1";
        yield return new WaitForSeconds(0.3f);

        if (score != null)
        {
            score.SetActive(true);
            mainTimer.SetActive(true);
            point.SetActive(true);
        }

        // Сбрасываем флаг перед началом нового теста
        testFinished = false;
        start_test?.Invoke();
    }
}