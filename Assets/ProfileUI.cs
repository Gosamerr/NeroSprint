using UnityEngine;
using UnityEngine.UI;      // для Text и Button
using UnityEngine.SceneManagement;

public class ProfileUI : MonoBehaviour
{
    public Text nameText;               // отображение имени
    public Text emailText;              // отображение email

    public Text bestSplitMatchText;      // лучший результат для SplitMatch
    public Text bestPopTapText;          // лучший результат для PopTap
    public Text bestHealthMeterText;     // лучший результат для HealthMeter

    public Button backButton;            // кнопка "Главное меню"

    private void Start()
    {
        backButton.onClick.AddListener(() => SceneManager.LoadScene("MainMenu"));
        LoadProfile();
    }

    private void LoadProfile()
    {
        // Если пользователь не вошёл — отправляем на логин
        if (DatabaseManager.CurrentUserId == -1)
        {
            SceneManager.LoadScene("Login");
            return;
        }

        // Получаем информацию о пользователе
        var (name, age, email) = DatabaseManager.Instance.GetUserInfo(DatabaseManager.CurrentUserId);
        nameText.text = name;
        emailText.text = email;

        // Получаем лучшие результаты (максимальная точность) для каждой игры
        float splitMatchBest = DatabaseManager.Instance.GetBestResult(DatabaseManager.CurrentUserId, "SplitMatch");
        float popTapBest = DatabaseManager.Instance.GetBestResult(DatabaseManager.CurrentUserId, "PopTap");
        float healthMeterBest = DatabaseManager.Instance.GetBestResult(DatabaseManager.CurrentUserId, "HealthMeter");

        // Отображаем в процентах (предполагаем, что overall_accuracy от 0 до 1)
        bestSplitMatchText.text = (splitMatchBest * 100).ToString("F2") + "%";
        bestPopTapText.text = (popTapBest * 100).ToString("F2") + "%";
        bestHealthMeterText.text = (healthMeterBest * 100).ToString("F2") + "%";
    }
}