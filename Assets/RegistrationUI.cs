using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class RegistrationUI : MonoBehaviour
{
    public InputField usernameInput;
    public InputField ageInput;
    public InputField emailInput;
    public InputField passwordInput;
    public Button registerButton;
    public Text statusText; 


    private void OnRegisterClick()
    {
        string username = usernameInput.text.Trim();
        string ageStr = ageInput.text.Trim();
        string email = emailInput.text.Trim();
        string password = passwordInput.text;

        // Простейшая валидация
        if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
        {
            statusText.text = "Please fill all fields";
            return;
        }

        if (!int.TryParse(ageStr, out int age) || age <= 0)
        {
            statusText.text = "Invalid age";
            return;
        }

        // Вызов метода регистрации из DatabaseManager
        bool success = DatabaseManager.Instance.RegisterUser(username, age, email, password);

        if (success)
        {
            statusText.text = "Registration successful!";
            // Очистить поля или перейти на другую сцену
        }
        else
        {
            statusText.text = "Registration failed (email maybe already used)";
        }
    }

    public Button goToLoginButton; // новая кнопка

    void Start()
    {
        registerButton.onClick.AddListener(OnRegisterClick);
        if (goToLoginButton != null)
            goToLoginButton.onClick.AddListener(OnGoToLoginClick);
    }

    private void OnGoToLoginClick()
    {
        SceneManager.LoadScene(0); // индекс сцены Login
    }
}