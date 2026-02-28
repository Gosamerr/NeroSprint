using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.SocialPlatforms.Impl;

public class CurAddScore : MonoBehaviour
{
    // Start is called before the first frame update

    TextMeshProUGUI text;
    void Start()
    {
        text = this.GetComponent<TextMeshProUGUI>();
    }

    private void OnEnable()
    {
        PointGo.AddScore += AddScore;
    }

    private void OnDisable()
    {
        PointGo.AddScore -= AddScore;
    }

    void AddScore(int score)
    {
        if (score < 0)
        {
            text.text = Convert.ToString(score);
            text.color = Color.red;
        }
        else {
            text.text = "+" + score;
            text.color = Color.green;
        }
        
        StartCoroutine(ShowAddScore());
    }

    IEnumerator ShowAddScore()
    {
        yield return new WaitForSeconds(0.3f);
        text.text = string.Empty;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
