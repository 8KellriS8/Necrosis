using UnityEngine;

public class DoomBillboardSimple : MonoBehaviour
{
    private Transform playerCamera;

    void Start()
    {
        // Находим главную камеру (в Unity "глазами" игрока обычно является именно она)
        GameObject cam = GameObject.FindWithTag("MainCamera");
        if (cam != null)
        {
            playerCamera = cam.transform;
        }
        else
        {
            Debug.LogError("Не найдена MainCamera! Убедитесь, что у камеры стоит тег 'MainCamera'.");
        }
    }

    void Update()
    {
        if (playerCamera == null) return;

        // Получаем направление от объекта к камере
        Vector3 lookDir = playerCamera.position - transform.position;
        
        // Обнуляем Y, чтобы спрайт не наклонялся вверх/вниз, если камера выше или ниже
        lookDir.y = 0;

        // Поворачиваем объект лицом к камере
        // Используем -lookDir, так как плоскость спрайта изначально смотрит вперед (Z+)
        if (lookDir != Vector3.zero)
        {
            transform.rotation = Quaternion.LookRotation(-lookDir);
        }
    }
}