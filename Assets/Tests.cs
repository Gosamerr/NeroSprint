using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class Tests : MonoBehaviour
{
    // Start is called before the first frame update

    [SerializeField] Button greenLight;
    [SerializeField] Button poptap;
    [SerializeField] Button main;
    void Start()
    {
        greenLight.onClick.AddListener(() => SceneManager.LoadScene(4));
        poptap.onClick.AddListener(() => SceneManager.LoadScene(3));
        main.onClick.AddListener(() => SceneManager.LoadScene(2));
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
