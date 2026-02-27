using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class LoginUI : MonoBehaviour
{
    public InputField emailInput;
    public InputField passwordInput;
    public Button loginButton;
    public Button registerButton;
    public Text statusText; // используйте TextMeshProUGUI, если нужно

    private void Start()
    {
        loginButton.onClick.AddListener(OnLoginClick);
        registerButton.onClick.AddListener(OnRegisterClick);
    }

    private void OnLoginClick()
    {
        string email = emailInput.text.Trim();
        string password = passwordInput.text;

        // Простая валидация
        if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
        {
            statusText.text = "Заполните все поля";
            return;
        }

        // Проверяем в базе
        bool isValid = DatabaseManager.Instance.ValidateUser(email, password);

        if (isValid)
        {
            statusText.text = "Успешный вход!";
            // Переход в главное меню
            SceneManager.LoadScene(2); // индекс сцены MainMenu
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
}