using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class Score : MonoBehaviour
{
    int score = 0;
    // Start is called before the first frame update
    TextMeshProUGUI text;
    void Start()
    {
        text = this.GetComponent<TextMeshProUGUI>();
    }

    // Update is called once per frame
    void Update()
    {
        score = PointGo.score;
        text.text = Convert.ToString( score);
    }
}
