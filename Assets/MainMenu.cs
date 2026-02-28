using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenu : MonoBehaviour
{
    [SerializeField] Button tests;
    [SerializeField] Button profile;
    [SerializeField] Button auth;
    void Start()
    {
        tests.onClick.AddListener(() => SceneManager.LoadScene(5));
        profile.onClick.AddListener(() => SceneManager.LoadScene(6));
        auth.onClick.AddListener(() => SceneManager.LoadScene(0));
    }
}
