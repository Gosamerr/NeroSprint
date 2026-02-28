using UnityEngine;
using UnityEngine.SceneManagement;

public class OpenProfile : MonoBehaviour
{
    // Этот метод будет вызываться при нажатии на кнопку
    public void GoToProfile()
    {
        // Загружаем сцену Profile по имени (убедитесь, что оно совпадает с названием в Build Settings)
        SceneManager.LoadScene("Profile");
    }
}