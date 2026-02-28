using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System;
using UnityEngine.Windows;
using Input = UnityEngine.Input;

public class LoginUI : MonoBehaviour
{
    public InputField emailInput;
    public InputField passwordInput;
    public Button loginButton;
    public Button registerButton;
    public Text statusText; // используйте TextMeshProUGUI, если нужно
    public bool flagPressAnyKey;

    public static event Action OnLoginSuccess;

    private void Start()
    {
        loginButton.onClick.AddListener(OnLoginClick);
        registerButton.onClick.AddListener(OnRegisterClick);
    }

    private void Update()
    {
        if (flagPressAnyKey)
        {
            if(Input.anyKey)
            {
                SceneManager.LoadScene(2);
            }
        }
    }

    private void OnLoginClick()
    {
        string email = emailInput.text.Trim();
        string password = passwordInput.text;

        if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
        {
            statusText.text = "Заполните все поля";
            return;
        }

        int userId = DatabaseManager.Instance.ValidateUser(email, password);
        if (userId != -1)
        {
            DatabaseManager.CurrentUserId = userId;
            OnLoginSuccess?.Invoke();
        }
        else
        {
            statusText.text = "Неверный email или пароль";
        }
    }

    private void OnRegisterClick()
    {
        // Переход на сцену регистрации
        SceneManager.LoadScene(1); // индекс сцены Register
    }

    private void OnEnable()
    {
        Moving.CanStartMain += PressAnyKey;
    }

    private void OnDisable()
    {
        Moving.CanStartMain -= PressAnyKey;
    }

    void PressAnyKey()
    {
        flagPressAnyKey = true;
    }

}