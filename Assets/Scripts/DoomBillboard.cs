using UnityEngine;

public class DoomBillboard : MonoBehaviour
{
    private Transform playerCamera;       // Камера (теперь находится автоматически)
    public SpriteRenderer spriteRenderer; 
    
    // Массив из 8 спрайтов (по часовой стрелке: Спереди, Спереди-Слева, Слева...)
    public Sprite[] viewSprites; 

    void Start()
    {
        // Автоматически находим объект с тегом MainCamera
        GameObject cam = GameObject.FindWithTag("MainCamera");
        if (cam != null)
        {
            playerCamera = cam.transform;
        }
        else
        {
            Debug.LogWarning($"На сцене не найден объект с тегом 'MainCamera' для билборда {gameObject.name}");
        }

        // Если SpriteRenderer не привязан в инспекторе, пробуем взять его с этого же объекта
        if (spriteRenderer == null)
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
        }
    }

    void Update()
    {
        if (playerCamera == null || viewSprites.Length < 8) return;

        // 1. Разворачиваем спрайт к камере
        Vector3 lookDir = playerCamera.position - transform.position;
        lookDir.y = 0; 
        transform.rotation = Quaternion.LookRotation(-lookDir);

        // 2. Рассчитываем угол относительно направления родительского объекта
        Vector3 forward = transform.parent != null ? transform.parent.forward : transform.forward; 
        
        float angle = Vector3.SignedAngle(forward, lookDir, Vector3.up);

        if (angle < 0) angle += 360f; 

        // 3. Меняем спрайт в зависимости от угла
        int spriteIndex = Mathf.RoundToInt(angle / 45f) % 8;
        spriteRenderer.sprite = viewSprites[spriteIndex];
    }
}
