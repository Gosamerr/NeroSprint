using System;
using System.Collections;
using UnityEngine;

public class Moving : MonoBehaviour
{
    [SerializeField] float speed = 500f;
    [SerializeField] GameObject targetGameObject;

    [Header("Target Y Positions")]
    [SerializeField] float firstObjectY = 1500f;
    [SerializeField] float secondObjectY = 500f;

    public static event Action CanStartMain;

    private void OnEnable()
    {
        LoginUI.OnLoginSuccess += MoveNameAndTEam;
    }

    private void OnDisable()
    {
        LoginUI.OnLoginSuccess -= MoveNameAndTEam;
    }

    private void MoveNameAndTEam()
    {
        StartCoroutine(MoveFirstObject());
    }

    IEnumerator MoveFirstObject()
    {
        Debug.Log($"Moving first object from Y:{transform.position.y} to Y:{firstObjectY}");

        // Двигаем первый объект
        yield return StartCoroutine(MoveToY(this.gameObject, firstObjectY));

        // Двигаем второй объект
        yield return StartCoroutine(MoveToY(targetGameObject, secondObjectY));

        CanStartMain?.Invoke();
    }

    IEnumerator MoveToY(GameObject obj, float targetY)
    {
        while (Mathf.Abs(obj.transform.position.y - targetY) > 0.5f)
        {
            float step = speed * Time.deltaTime;
            float newY = Mathf.MoveTowards(obj.transform.position.y, targetY, step);
            obj.transform.position = new Vector3(obj.transform.position.x, newY, obj.transform.position.z);
            yield return null;
        }

        // Финальная точная установка
        obj.transform.position = new Vector3(obj.transform.position.x, targetY, obj.transform.position.z);
        Debug.Log($"{obj.name} reached Y: {targetY}");
    }
}