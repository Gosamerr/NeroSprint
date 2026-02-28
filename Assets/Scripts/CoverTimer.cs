using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CoverTimer : MonoBehaviour
{
    // Start is called before the first frame update
    TextMeshProUGUI timer;
    public static event Action start_test;

    [SerializeField]
    private GameObject point;
    [SerializeField]
    private GameObject mainTimer;
    [SerializeField]
    private GameObject score;

    void Start()
    {
        timer = GetComponent<TextMeshProUGUI>();
        StartCoroutine(ChangeNums());
    }
    private void OnDisable()
    {
        MainTimer.TimeOver -= SetScore;
        CheckReacter.UserReact -= SetScore;
    }

    private void OnEnable()
    {
        MainTimer.TimeOver += SetScore;
        CheckReacter.UserReact += SetScore;
    }

    private void SetScore()
    {
        timer.text = Convert.ToString(PointGo.score);
    }
    void Update()
    {
        
    }


    IEnumerator ChangeNums()
    {
        yield return new WaitForSeconds(0.9f);
        timer.text = "2";
        timer.transform.localScale = new Vector3 (1.2f, 1.2f, 1.2f);
        yield return new WaitForSeconds(0.9f);
        timer.transform.localScale = new Vector3(1.5f, 1.5f, 1.5f);
        timer.text = "1";
        yield return new WaitForSeconds(0.3f);

        if (score != null)
        {
            score.active = true;
            mainTimer.active = true;
            point.active = true;
        }
        start_test?.Invoke();
        
    }
}
