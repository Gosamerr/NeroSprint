using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class ProfileUI : MonoBehaviour
{
    [Header("Информация о пользователе")]
    public Text nameText;
    public Text emailText;

    [Header("Зелёный цвет (RidersTest)")]
    public Text greenTimeText;        // лучшее время реакции (мс)
    public Text greenAccuracyText;    // лучшая точность (%)

    [Header("Точка быстрая (PopTap)")]
    public Text popTimeText;          // лучшее время реакции (мс)
    public Text popAccuracyText;      // лучшая точность (%)
    public Text popScoreText;         // лучший счёт

    public Button backButton;

    private void Start()
    {
        backButton.onClick.AddListener(() => SceneManager.LoadScene("MainMenu"));
        LoadProfile();
    }

    private void LoadProfile()
    {
        if (DatabaseManager.CurrentUserId == -1)
        {
            SceneManager.LoadScene("Login");
            return;
        }

        // Информация о пользователе
        var (name, age, email) = DatabaseManager.Instance.GetUserInfo(DatabaseManager.CurrentUserId);
        nameText.text = name;
        emailText.text = email;

        // ---- Зелёный цвет (RidersTest) ----
        float greenAccuracy = DatabaseManager.Instance.GetBestResult(DatabaseManager.CurrentUserId, "RidersTest");
        float greenTime = DatabaseManager.Instance.GetBestReactionTime(DatabaseManager.CurrentUserId, "RidersTest");

        greenAccuracyText.text = (greenAccuracy * 100).ToString("F2") + "%";
        greenTimeText.text = greenTime > 0 ? greenTime.ToString("F0") + " мс" : "—";

        // ---- Точка быстрая (PopTap) ----
        float popAccuracy = DatabaseManager.Instance.GetBestResult(DatabaseManager.CurrentUserId, "PopTap");
        float popTime = DatabaseManager.Instance.GetBestReactionTime(DatabaseManager.CurrentUserId, "PopTap");
        int popScore = DatabaseManager.Instance.GetBestScore(DatabaseManager.CurrentUserId, "PopTap");

        popAccuracyText.text = (popAccuracy * 100).ToString("F2") + "%";
        popTimeText.text = popTime > 0 ? popTime.ToString("F0") + " мс" : "—";
        popScoreText.text = popScore > 0 ? popScore.ToString() : "—";
    }
}