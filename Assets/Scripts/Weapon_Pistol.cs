using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;

public class Weapon : MonoBehaviour
{
    public RawImage[] weaponImages; // [0] - Idle, [1] - Aim, [2] - Shoot
    public float shootDuration = 0.1f;
    public float dmg = 10f;
    public float accuracy = 1f;
    public Animator animator;
    [Header("Эффекты выстрела")]
    public ParticleSystem enemyHitParticles; // Ссылка на префаб или объект ParticleSystem на сцене
    public GameObject bulletHolePrefab;
    
    private enum WeaponState { Idle, Aim, Shoot }
    private bool isShooting = false;
    private float timer = 0f;

    void Start()
    {
        ShowState(WeaponState.Aim);
    }

    void Update()
    {
        Keyboard keyboard = Keyboard.current;
        if (keyboard == null) return;
        Mouse mouse = Mouse.current;
        if (mouse == null) return;
        // Shift зажат = Idle, иначе Aim
        if (keyboard.leftShiftKey.isPressed)
        {
            if (!isShooting)
            {
                ShowState(WeaponState.Idle);
                animator.SetBool("Targetting", false);
            }
        }
        else
        {
            if (!isShooting)
            {
                ShowState(WeaponState.Aim);
                animator.SetBool("Targetting", true);
            }
        }

        // Выстрел
        if (mouse.leftButton.wasPressedThisFrame && !isShooting)
        {
            Shoot();
        }

        // Таймер
        if (isShooting)
        {
            timer -= Time.deltaTime;
            if (timer <= 0f)
            {
                animator.SetBool("Shooting", false);
                isShooting = false;
                // Возврат
                if (keyboard.leftShiftKey.isPressed)
                    ShowState(WeaponState.Idle);
                else
                    ShowState(WeaponState.Aim);
            }
        }
    }

    void Shoot()
    {
        isShooting = true;
        animator.SetBool("Shooting", true);
        timer = shootDuration;
        ShowState(WeaponState.Shoot);
    
        // Raycast выстрел из центра экрана
        RaycastHit hit;
        Ray ray = Camera.main.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));

        // Проверяем, попал ли рейкаст во что-нибудь в пределах 100 метров
        if (Physics.Raycast(ray, out hit, 100f))
        {
            // 1. Если попали во врага
            if (hit.collider.CompareTag("enemy"))
            {
                // Пытаемся взять компонент EnemyAI у задетого объекта
                EnemyAI enemy = hit.collider.GetComponent<EnemyAI>();
            
                // Если не нашли на самом коллайдере, ищем в родительских объектах (полезно для составных моделей)
                if (enemy == null) enemy = hit.collider.GetComponentInParent<EnemyAI>();

                if (enemy != null)
                {
                    enemy.TakeDamage(dmg); // Наносим урон (замените 25f на ваш урон оружия)
                }

                // Перемещаем партиклы в точку попадания и запускаем их
                if (enemyHitParticles != null)
                {
                    enemyHitParticles.transform.position = hit.point;
                    // Поворачиваем партиклы в сторону, откуда прилетела пуля (на игрока)
                    enemyHitParticles.transform.forward = hit.normal; 
                    enemyHitParticles.Play();
                }
            }
            // 2. Если попали в окружение / объект
            else if (hit.collider.CompareTag("object"))
            {
                if (bulletHolePrefab != null)
                {
                    // Сдвигаем спавн спрайта чуть-чуть вперед от стены (на 0.01м), чтобы избежать мерцания текстур (Z-fighting)
                    Vector3 spawnPosition = hit.point + (hit.normal * 0.01f);

                    // Создаем квад/спрайт следа от пули и разворачиваем его по нормали (плоскости поверхности)
                    Quaternion spawnRotation = Quaternion.LookRotation(hit.normal);
                
                    GameObject bulletHole = Instantiate(bulletHolePrefab, spawnPosition, spawnRotation);

                    // Дополнительно: привязываем след к объекту, чтобы он двигался вместе с ним, если объект подвижный
                    bulletHole.transform.SetParent(hit.collider.transform);

                    // Рекомендуется: уничтожать след от пули через 10-20 секунд, чтобы не забивать память
                    Destroy(bulletHole, 20f);
                }
            }
        }
    }

    void ShowState(WeaponState state)
    {
        // Скрываем всё
        foreach (var img in weaponImages)
        {
            if (img != null) img.gameObject.SetActive(false);
        }
        
        // Показываем нужное
        if (weaponImages.Length > (int)state && weaponImages[(int)state] != null)
        {
            weaponImages[(int)state].gameObject.SetActive(true);
        }
    }
}