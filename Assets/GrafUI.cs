using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using static DatabaseManager;

public class GraphController : MonoBehaviour
{
    [Header("UI Elements")]
    public TMP_Dropdown testDropdown;
    public RectTransform reactionTimeGraphContainer;
    public RectTransform accuracyGraphContainer;
    public GameObject dotPrefab;
    public GameObject linePrefab;
    public Button backButton;
    public TextMeshProUGUI noDataText;
    public TextMeshProUGUI infoText;

    [Header("Graph Settings")]
    public Color reactionTimeColor = new Color(1f, 0.4f, 0.8f); // Розовый - только для времени
    public Color accuracyColor = new Color(1f, 0.9f, 0.1f); // Желтый - только для точности
    public float dotSize = 15f;
    public float lineWidth = 3f;
    public float graphHeight = 150f;
    public float pointSpacing = 2.5f;
    public int maxPointsOnGraph = 30;

    [Header("Text Settings")]
    public float dropdownTextSize = 24f;
    public float infoTextSize = 20f;

    private List<string> availableTests = new List<string>();
    private List<GameObject> reactionDots = new List<GameObject>();
    private List<GameObject> accuracyDots = new List<GameObject>();
    private List<GameObject> reactionLines = new List<GameObject>();
    private List<GameObject> accuracyLines = new List<GameObject>();
    private List<TestResultData> currentResults = new List<TestResultData>();

    void Start()
    {
        if (DatabaseManager.CurrentUserId == -1)
        {
            Debug.LogError("Пользователь не авторизован!");
            if (noDataText != null)
                noDataText.text = "Пользователь не авторизован!";
            return;
        }

        if (!ValidateComponents())
        {
            Debug.LogError("Не все компоненты назначены в GraphController!");
            return;
        }

        // Настраиваем pivot контейнеров на левый центр
        SetupContainers();

        EnsureDotHasButton();
        SetupDropdownTextSize();
        LoadAvailableTests();
        SetupDropdown();

        if (backButton != null)
            backButton.onClick.AddListener(GoBack);

        if (infoText != null)
        {
            infoText.text = "Нажмите на точку для просмотра деталей";
            infoText.fontSize = infoTextSize;
        }
    }

    void SetupContainers()
    {
        // Устанавливаем pivot в левый центр (0,0.5)
        if (reactionTimeGraphContainer != null)
        {
            reactionTimeGraphContainer.pivot = new Vector2(0, 0.5f);
            reactionTimeGraphContainer.anchorMin = new Vector2(0, 0.5f);
            reactionTimeGraphContainer.anchorMax = new Vector2(0, 0.5f);
            reactionTimeGraphContainer.anchoredPosition = Vector2.zero;
        }

        if (accuracyGraphContainer != null)
        {
            accuracyGraphContainer.pivot = new Vector2(0, 0.5f);
            accuracyGraphContainer.anchorMin = new Vector2(0, 0.5f);
            accuracyGraphContainer.anchorMax = new Vector2(0, 0.5f);
            accuracyGraphContainer.anchoredPosition = Vector2.zero;
        }
    }

    void EnsureDotHasButton()
    {
        if (dotPrefab != null)
        {
            Button existingButton = dotPrefab.GetComponent<Button>();
            if (existingButton == null)
            {
                Debug.LogWarning("dotPrefab не содержит компонент Button. Добавьте Button на префаб точки в инспекторе!");
            }
        }
    }

    void SetupDropdownTextSize()
    {
        if (testDropdown != null)
        {
            TMP_Text dropdownLabel = testDropdown.GetComponentInChildren<TMP_Text>();
            if (dropdownLabel != null)
            {
                dropdownLabel.fontSize = dropdownTextSize;
            }
        }
    }

    bool ValidateComponents()
    {
        bool isValid = true;

        if (testDropdown == null)
        {
            Debug.LogError("testDropdown не назначен!");
            isValid = false;
        }
        if (reactionTimeGraphContainer == null)
        {
            Debug.LogError("reactionTimeGraphContainer не назначен!");
            isValid = false;
        }
        if (accuracyGraphContainer == null)
        {
            Debug.LogError("accuracyGraphContainer не назначен!");
            isValid = false;
        }
        if (dotPrefab == null)
        {
            Debug.LogError("dotPrefab не назначен!");
            isValid = false;
        }
        if (linePrefab == null)
        {
            Debug.LogWarning("linePrefab не назначен. Линии не будут отображаться.");
        }

        return isValid;
    }

    void LoadAvailableTests()
    {
        try
        {
            if (DatabaseManager.Instance == null)
            {
                Debug.LogError("DatabaseManager.Instance is null!");
                return;
            }

            availableTests = DatabaseManager.Instance.GetUserTests(DatabaseManager.CurrentUserId);

            if (availableTests == null)
            {
                availableTests = new List<string>();
            }

            Debug.Log($"Загружено {availableTests.Count} тестов");
        }
        catch (Exception e)
        {
            Debug.LogError($"Ошибка при загрузке тестов: {e.Message}");
            availableTests = new List<string>();
        }
    }

    void SetupDropdown()
    {
        if (testDropdown == null) return;

        testDropdown.ClearOptions();
        testDropdown.onValueChanged.RemoveAllListeners();

        if (availableTests != null && availableTests.Count > 0)
        {
            testDropdown.AddOptions(availableTests);
            testDropdown.onValueChanged.AddListener(OnTestSelected);
            testDropdown.interactable = true;
            OnTestSelected(0);
        }
        else
        {
            testDropdown.AddOptions(new List<string> { "Нет данных" });
            testDropdown.interactable = false;
            if (noDataText != null)
                noDataText.text = "Нет данных для отображения. Пройдите тесты!";
        }
    }

    void OnTestSelected(int index)
    {
        if (availableTests != null && index >= 0 && index < availableTests.Count)
        {
            string selectedTest = availableTests[index];
            Debug.Log($"Выбран тест: {selectedTest}");
            ShowGraphForTest(selectedTest);
        }
    }

    void ShowGraphForTest(string testName)
    {
        ClearGraphs();

        try
        {
            if (DatabaseManager.Instance == null)
            {
                Debug.LogError("DatabaseManager.Instance is null!");
                return;
            }

            currentResults = DatabaseManager.Instance.GetTestResultsForLastMonth(
                DatabaseManager.CurrentUserId, testName);

            if (currentResults == null)
            {
                currentResults = new List<TestResultData>();
            }

            Debug.Log($"Получено {currentResults.Count} результатов для теста {testName}");

            if (currentResults.Count == 0)
            {
                if (noDataText != null)
                    noDataText.text = $"Нет данных за последний месяц для теста {testName}";
                return;
            }
            else
            {
                if (noDataText != null)
                    noDataText.text = "";
            }

            // Фильтруем и сортируем данные
            currentResults = currentResults
                .Where(r => r != null && r.avgReactionTimeMs > 0)
                .OrderBy(r => r.completionDate)
                .ToList();

            if (currentResults.Count == 0)
            {
                if (noDataText != null)
                    noDataText.text = $"Нет корректных данных для отображения";
                return;
            }

            if (currentResults.Count > maxPointsOnGraph)
            {
                currentResults = currentResults.Skip(currentResults.Count - maxPointsOnGraph).ToList();
            }

            // Создаем отдельные графики
            CreateReactionTimeGraph();
            CreateAccuracyGraph();
        }
        catch (Exception e)
        {
            Debug.LogError($"Ошибка в ShowGraphForTest: {e.Message}\n{e.StackTrace}");
            if (noDataText != null)
                noDataText.text = $"Ошибка загрузки данных: {e.Message}";
        }
    }

    void CreateReactionTimeGraph()
    {
        try
        {
            if (reactionTimeGraphContainer == null || dotPrefab == null || currentResults == null || currentResults.Count == 0)
            {
                Debug.LogWarning("Невозможно создать график времени реакции");
                return;
            }

            // Фильтруем только для времени реакции
            var validResults = currentResults
                .Where(r => !float.IsNaN(r.avgReactionTimeMs) && !float.IsInfinity(r.avgReactionTimeMs))
                .ToList();

            if (validResults.Count == 0)
            {
                Debug.LogWarning("Нет данных времени реакции");
                return;
            }

            float maxReactionTime = validResults.Max(r => r.avgReactionTimeMs);
            float minReactionTime = validResults.Min(r => r.avgReactionTimeMs);

            Debug.Log($"Reaction Time Graph - Min: {minReactionTime}, Max: {maxReactionTime}");

            // Добавляем отступы для масштабирования
            float range = maxReactionTime - minReactionTime;
            if (range < 0.1f) range = 10f;

            float padding = range * 0.1f;
            maxReactionTime += padding;
            minReactionTime = Math.Max(0, minReactionTime - padding * 0.5f);

            float containerWidth = reactionTimeGraphContainer.rect.width;
            float leftMargin = 20f;
            float rightMargin = 20f;
            float graphAreaWidth = containerWidth - leftMargin - rightMargin;

            float xStep = (validResults.Count > 1) ? graphAreaWidth / (validResults.Count - 1) * pointSpacing : 0;
            xStep = Mathf.Abs(xStep);

            List<Vector2> dotPositions = new List<Vector2>();

            for (int i = 0; i < validResults.Count; i++)
            {
                float normalizedValue = (validResults[i].avgReactionTimeMs - minReactionTime) /
                                       (maxReactionTime - minReactionTime);
                normalizedValue = Mathf.Clamp01(normalizedValue);

                float xPos = leftMargin + (i * xStep);
                float yPos = -normalizedValue * graphHeight; // Отрицательный Y для движения вверх

                GameObject dot = CreateDot(reactionTimeGraphContainer, xPos, yPos, reactionTimeColor);
                dotPositions.Add(new Vector2(xPos, yPos));

                if (dot != null)
                {
                    SetupDotInteraction(dot, validResults[i], "reaction");
                    reactionDots.Add(dot);
                }
            }

            // Создаем линии только для времени реакции
            if (linePrefab != null && dotPositions.Count > 1)
            {
                for (int i = 0; i < dotPositions.Count - 1; i++)
                {
                    CreateLine(reactionTimeGraphContainer, dotPositions[i], dotPositions[i + 1], reactionTimeColor, reactionLines);
                }
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"Ошибка в CreateReactionTimeGraph: {e.Message}");
        }
    }

    void CreateAccuracyGraph()
    {
        try
        {
            if (accuracyGraphContainer == null || dotPrefab == null || currentResults == null || currentResults.Count == 0)
            {
                Debug.LogWarning("Невозможно создать график точности");
                return;
            }

            // Фильтруем только для точности
            var validResults = currentResults
                .Where(r => !float.IsNaN(r.overallAccuracy) && !float.IsInfinity(r.overallAccuracy))
                .ToList();

            if (validResults.Count == 0)
            {
                Debug.LogWarning("Нет данных точности");
                return;
            }

            // Используем динамический диапазон вместо фиксированного 0-100
            float maxAccuracy = validResults.Max(r => r.overallAccuracy);
            float minAccuracy = validResults.Min(r => r.overallAccuracy);

            Debug.Log($"Accuracy Graph - Min: {minAccuracy:F3}, Max: {maxAccuracy:F3}");

            // Добавляем небольшие отступы для лучшей визуализации
            float range = maxAccuracy - minAccuracy;
            if (range < 0.01f) range = 0.1f; // Минимальный диапазон

            float padding = range * 0.1f;
            maxAccuracy += padding;
            minAccuracy = Math.Max(0, minAccuracy - padding * 0.5f);

            float containerWidth = accuracyGraphContainer.rect.width;
            float leftMargin = 30f;
            float rightMargin = 30f;
            float graphAreaWidth = containerWidth - leftMargin - rightMargin;

            float xStep = (validResults.Count > 1) ? graphAreaWidth / (validResults.Count - 1) * pointSpacing: 0;
            xStep = Mathf.Abs(xStep);

            List<Vector2> dotPositions = new List<Vector2>();

            for (int i = 0; i < validResults.Count; i++)
            {
                // Нормализуем значение относительно динамического диапазона
                float normalizedValue = (validResults[i].overallAccuracy - minAccuracy) /
                                       (maxAccuracy - minAccuracy);
                normalizedValue = Mathf.Clamp01(normalizedValue);

                float xPos = leftMargin + (i * xStep);
                float yPos = -normalizedValue * graphHeight; // Отрицательный Y для движения вверх

                GameObject dot = CreateDot(accuracyGraphContainer, xPos, yPos, accuracyColor);
                dotPositions.Add(new Vector2(xPos, yPos));

                if (dot != null)
                {
                    SetupDotInteraction(dot, validResults[i], "accuracy");
                    accuracyDots.Add(dot);
                }
            }

            // Создаем линии только для точности
            if (linePrefab != null && dotPositions.Count > 1)
            {
                for (int i = 0; i < dotPositions.Count - 1; i++)
                {
                    CreateLine(accuracyGraphContainer, dotPositions[i], dotPositions[i + 1], accuracyColor, accuracyLines);
                }
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"Ошибка в CreateAccuracyGraph: {e.Message}");
        }
    }

    void SetupDotInteraction(GameObject dot, TestResultData resultData, string dotType)
    {
        DotData dotData = dot.GetComponent<DotData>();
        if (dotData == null)
        {
            dotData = dot.AddComponent<DotData>();
        }
        dotData.resultData = resultData;
        dotData.dotType = dotType;

        Button dotButton = dot.GetComponent<Button>();
        if (dotButton != null)
        {
            dotButton.onClick.RemoveAllListeners();

            TestResultData capturedData = resultData;
            string capturedType = dotType;

            dotButton.onClick.AddListener(() =>
            {
                Debug.Log($"Клик по {capturedType} точке");
                OnDotClicked(capturedData, capturedType);
            });
        }
    }

    void CreateLine(RectTransform container, Vector2 startPos, Vector2 endPos, Color color, List<GameObject> linesList)
    {
        if (linePrefab == null) return;

        GameObject line = Instantiate(linePrefab, container);
        RectTransform lineRect = line.GetComponent<RectTransform>();

        Vector2 direction = endPos - startPos;
        float distance = direction.magnitude;
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

        lineRect.anchoredPosition = startPos + direction * 0.5f;
        lineRect.sizeDelta = new Vector2(distance, lineWidth);
        lineRect.rotation = Quaternion.Euler(0, 0, angle);

        Image lineImage = line.GetComponent<Image>();
        if (lineImage != null)
        {
            lineImage.color = color;
        }

        line.transform.SetAsFirstSibling();
        linesList.Add(line);
    }

    GameObject CreateDot(RectTransform container, float x, float y, Color color)
    {
        try
        {
            GameObject dot = Instantiate(dotPrefab, container);
            if (dot == null) return null;

            RectTransform dotRect = dot.GetComponent<RectTransform>();
            if (dotRect == null)
            {
                Destroy(dot);
                return null;
            }

            dotRect.anchoredPosition = new Vector2(x, y);
            dotRect.sizeDelta = new Vector2(dotSize, dotSize);

            Image dotImage = dot.GetComponent<Image>();
            if (dotImage != null)
            {
                dotImage.color = color;
            }

            return dot;
        }
        catch (Exception e)
        {
            Debug.LogError($"Ошибка в CreateDot: {e.Message}");
            return null;
        }
    }

    void OnDotClicked(TestResultData resultData, string dotType)
    {
        try
        {
            if (infoText != null && resultData != null)
            {
                if (dotType == "reaction")
                {
                    infoText.text = $"Время реакции: {resultData.avgReactionTimeMs:F0} мс\n" +
                                   $"Дата: {resultData.completionDate:dd.MM.yyyy HH:mm}";
                }
                else // accuracy
                {
                    infoText.text = $"Точность: {resultData.overallAccuracy:F1}%</size>\n" +
                                   $"Дата: {resultData.completionDate:dd.MM.yyyy HH:mm}";
                }

                infoText.gameObject.SetActive(true);
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"Ошибка в OnDotClicked: {e.Message}");
        }
    }

    void ClearGraphs()
    {
        foreach (var dot in reactionDots) if (dot != null) Destroy(dot);
        reactionDots.Clear();
        foreach (var dot in accuracyDots) if (dot != null) Destroy(dot);
        accuracyDots.Clear();
        foreach (var line in reactionLines) if (line != null) Destroy(line);
        reactionLines.Clear();
        foreach (var line in accuracyLines) if (line != null) Destroy(line);
        accuracyLines.Clear();
    }

    void GoBack()
    {
        SceneManager.LoadScene("MainMenu");
    }
}

public class DotData : MonoBehaviour
{
    public TestResultData resultData;
    public string dotType;
}