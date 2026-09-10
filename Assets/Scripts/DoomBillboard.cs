using UnityEngine;

public class DoomBillboard : MonoBehaviour
{
    private Transform playerCamera;
    public SpriteRenderer spriteRenderer;
    
    [Header("Спрайты (Должно быть ровно 15 элементов: 0-11 ходьба, 12-13 атака, 14 смерть)")]
    public Sprite[] viewSprites;

    private EnemyAI enemyAI;
    private float animationTimer;
    private int walkFrame = 0; // 0 или 1 для зацикливания ходьбы

    // Переменные для отслеживания состояния атаки
    private bool wasAttacking;
    private float attackTimer;
    private int attackFrame = 12;

    void Start()
    {
        // Автоматически находим камеру
        GameObject cam = GameObject.FindWithTag("MainCamera");
        if (cam != null) playerCamera = cam.transform;

        if (spriteRenderer == null) spriteRenderer = GetComponent<SpriteRenderer>();

        // Ищем скрипт EnemyAI у родительского объекта
        if (transform.parent != null)
        {
            enemyAI = transform.parent.GetComponent<EnemyAI>();
        }
        
        if (enemyAI == null)
        {
            Debug.LogError($"Скрипт EnemyAI не найден на родительском объекте для {gameObject.name}");
        }
    }

    void Update()
    {
        if (playerCamera == null || viewSprites.Length < 15 || enemyAI == null) return;

        // 1. Поворот к камере
        Vector3 lookDir = playerCamera.position - transform.position;
        lookDir.y = 0;
        transform.rotation = Quaternion.LookRotation(-lookDir);

        // 2. Проверка состояния смерти (самый высокий приоритет)
        if (enemyAI.isDead)
        {
            spriteRenderer.sprite = viewSprites[14];
            return; // Прерываем выполнение, спрайт застывает на 15-м кадре
        }

        // 3. Проверка состояния атаки
        if (enemyAI.isAtacking)
        {
            HandleAttackAnimation();
            return;
        }
        else
        {
            // Сбрасываем флаги атаки, когда она прекратилась
            wasAttacking = false;
        }

        // 4. Логика ходьбы (если не мертв и не атакует)
        HandleWalkAnimation(lookDir);
    }

    private void HandleWalkAnimation(Vector3 lookDir)
    {
        // Таймер смены кадров ходьбы (каждые 0.5 сек)
        animationTimer += Time.deltaTime;
        if (animationTimer >= 0.5f)
        {
            animationTimer = 0f;
            walkFrame = walkFrame == 0 ? 1 : 0; // Переключаем между 0 и 1
        }

        // Рассчитываем угол (6 направлений)
        Vector3 forward = transform.parent != null ? transform.parent.forward : transform.forward;
        float angle = Vector3.SignedAngle(forward, lookDir, Vector3.up);
        if (angle < 0) angle += 360f;

        // Индекс направления (от 0 до 5)
        int directionIndex = Mathf.RoundToInt(angle / 60f) % 6;

        // Формула для 12 кадров ходьбы: каждые 6 направлений имеют по 2 кадра
        int finalSpriteIndex = (directionIndex * 2) + walkFrame;
        
        spriteRenderer.sprite = viewSprites[finalSpriteIndex];
    }

    private void HandleAttackAnimation()
    {
        // Если атака только началась, инициализируем первый кадр (13-й по счету, индекс 12)
        if (!wasAttacking)
        {
            wasAttacking = true;
            attackTimer = 0f;
            attackFrame = 12; // Индекс 12 — это 13-й по счету спрайт
        }

        if (attackFrame == 12)
        {
            attackTimer += Time.deltaTime;
            if (attackTimer >= 0.5f)
            {
                attackFrame = 13; // Индекс 13 — это 14-й по счету спрайт (застывает)
            }
        }

        spriteRenderer.sprite = viewSprites[attackFrame];
    }
}
