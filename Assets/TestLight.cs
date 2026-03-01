using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class TestLight : MonoBehaviour
{
    [SerializeField] Image firstLight;
    [SerializeField] Image secondLight;
    [SerializeField] Image thirdLight;
    [SerializeField] Image fourthLight;
    private Image[] allLights;

    [SerializeField] Color[] colors = new Color[] { Color.yellow, Color.blue, Color.grey, Color.magenta };
    [SerializeField] public int timesChangecolor;
    [SerializeField] Color[] colorsLight;

    public static event Action GreenStart;
    public static event Action TestCompleted; // событие окончания теста

    void Start()
    {
        allLights = new Image[4] { firstLight, secondLight, thirdLight, fourthLight };
    }

    private void OnEnable()
    {
        CoverTimer.start_test += SetRidersTestActive;
    }

    private void OnDisable()
    {
        CoverTimer.start_test -= SetRidersTestActive;
    }

    void SetRidersTestActive()
    {
        timesChangecolor = Random.Range(1, 3);
        colorsLight = new Color[timesChangecolor];

        for (int i = 0; i < colorsLight.Length; i++)
        {
            int color = Random.Range(0, 4);
            colorsLight[i] = colors[color];
        }
        colorsLight[colorsLight.Length - 1] = Color.green;

        StartCoroutine(ChangeColorsLigth());
    }

    IEnumerator ChangeColorsLigth()
    {
        for (int i = 0; i < colorsLight.Length; i++)
        {
            float sec = Random.Range(3, 10);
            yield return new WaitForSeconds(sec);

            foreach (Image j in allLights)
            {
                j.color = colorsLight[i];
            }

            if (colorsLight[i] == Color.green)
            {
                GreenStart?.Invoke();
                // Даём игроку 2 секунды на реакцию
                yield return new WaitForSeconds(2f);
            }
        }

        // Тест завершён
        TestCompleted?.Invoke();
    }
}