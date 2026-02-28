using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class TestLight : MonoBehaviour
{
    // Start is called before the first frame update

    [SerializeField] Image firstLight;
    [SerializeField] Image secondLight;
    [SerializeField] Image thirdLight;
    [SerializeField] Image fourthLight;
    [SerializeField] Image[] allLights = new Image[4];

    [SerializeField]  Color[] colors = new Color[] { Color.yellow, Color.blue, Color.grey, Color.magenta };

    [SerializeField] public int timesChangecolor;
    [SerializeField] Color[] colorsLight;
    public static event Action GreenStart;
    
    void Start()
    {
        allLights = new Image[4] {firstLight, secondLight, thirdLight, fourthLight };
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnEnable()
    {
        CoverTimer.start_test += SetRidersTestACtive;
    }

    private void OnDisable()
    {
        CoverTimer.start_test -= SetRidersTestACtive;
    }

    void SetRidersTestACtive()
    {
        timesChangecolor = Random.Range(1, 3);

        colorsLight = new Color[timesChangecolor];

        for (int i = 0; i < colorsLight.Length; i++)
        {
            int color = Random.Range(0,4);
            colorsLight[i] = colors[color];
        }
        colorsLight[colorsLight.Length-1] = Color.green;

        
        StartCoroutine(ChangeColorsLigth());
            
    }

    IEnumerator ChangeColorsLigth()
    {
        for (int i = 0; i < colorsLight.Length; i++)
        {
            float sec = Random.Range(3, 10);
            yield return new WaitForSeconds(sec);
            // Меняем цвет
            foreach (Image j in allLights)
            {
                j.color = colorsLight[i];
            }

            // Если зеленый - вызываем событие
            if (colorsLight[i] == Color.green)
            {
                GreenStart?.Invoke();
            }
            
        }
    }
}
