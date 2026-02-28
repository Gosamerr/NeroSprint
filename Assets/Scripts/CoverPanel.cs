using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class CoverPanel : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    private void OnEnable()
    {
        CoverTimer.start_test += HidePanel;
        //MainTimer.TimeOver += ShowPanel;
        //CheckReacter.UserReact += ShowPanel;
    }

    private void OnDisable()
    {
        CoverTimer.start_test -= HidePanel;
        //MainTimer.TimeOver -= ShowPanel;
        //CheckReacter.UserReact -= ShowPanel;
    }
    void HidePanel()
    {
        gameObject.SetActive(false);
    }

    //void ShowPanel()
    //{
    //    gameObject.SetActive(true);
    //}
    // Update is called once per frame
    void Update()
    {
        if(Input.GetKey(KeyCode.Escape))
        {
            SceneManager.LoadScene(2);
        }
    }
}
