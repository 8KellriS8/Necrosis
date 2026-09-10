using System.Collections;
using UnityEngine;
using UnityEngine.AI; // Обязательно для работы с NavMesh

[RequireComponent(typeof(NavMeshAgent))]
public class EnemyAI : MonoBehaviour
{

    [Header("Характеристики")]
    public float hp = 100f;
    public float moveSpeed = 2f;
    
    [Header("Дистанции")]
    public float attackDistance = 3f; // Дистанция, на которой начинается атака
    public float chaseDistance = 25f; // На каком расстоянии враг начинает преследовать игрока

    [Header("Настройки состояния покоя (NavMesh)")]
    public float wanderRadius = 5f;       // Радиус случайного передвижения
    public float wanderInterval = 4f;     // Раз во сколько секунд искать новую точку

    [Header("Тайминги атаки")]
    public float attackCooldown = 1.5f;   // Длительность остановки/атаки (время покоя после удара)
    public float damageActiveDuration = 0.5f; // Сколько времени активен коллайдер урона (13-й кадр)

    [Header("Ссылки на компоненты")]
    public Collider damageCollider;      // Ссылка на ДОЧЕРНИЙ коллайдер урона

    // Публичные переменные для DoomBillboard
    [HideInInspector] public bool isDead = false;
    [HideInInspector] public bool isAtacking = false;

    private Transform target;             // Цель (игрок)
    private float wanderTimer;
    private bool isCooldown = false;      // Флаг задержки/остановки ИИ во время атаки
    private NavMeshAgent agent;           // Навигационный агент

    void Start()
    {
        // Автоматически находим игрока по тегу
        GameObject player = GameObject.FindWithTag("Player");
        if (player != null) target = player.transform;

        // Настраиваем NavMeshAgent
        agent = GetComponent<NavMeshAgent>();
        agent.speed = moveSpeed;

        wanderTimer = wanderInterval; // Начать движение сразу в состоянии покоя

        if (damageCollider != null) damageCollider.enabled = false;
    }

    void Update()
    {
        if (isDead) return;

        // Проверка здоровья
        if (hp <= 0)
        {
            Die();
            return;
        }

        if (target == null) return;

        // Дистанция рассчитывается по NavMesh или по прямой (для точности используем Vector3.Distance)
        float distanceToTarget = Vector3.Distance(transform.position, target.position);

        // Если идет атака или кулдаун — агент полностью стоит на месте
        if (isCooldown || isAtacking)
        {
            StopAgent();
            return;
        }
        // Проверяем дистанцию атаки
        if (distanceToTarget <= attackDistance)
        {
            // Игрок достаточно близко для атаки
            if (IsPlayerInFront())
            {
                StartCoroutine(AttackRoutine());
            }
            else
            {
                // Игрок близко, но находится сзади/сбоку — идём к нему
                ResumeAgent();
                agent.SetDestination(target.position);
            }
        }
        else if (distanceToTarget <= chaseDistance)
        {
            // Игрок находится в пределах 25 метров — преследуем его
            ResumeAgent();
            agent.SetDestination(target.position);
        }
        else
        {
            // Игрок дальше 25 метров — случайно блуждаем
            ResumeAgent();
            WanderBehavior();
        }
    }

    // Логика блуждания по NavMesh
    private void WanderBehavior()
    {
        wanderTimer += Time.deltaTime;

        // Ищем новую точку, если прошёл интервал ИЛИ если агент уже дошёл до старой точки
        if (wanderTimer >= wanderInterval || (agent.remainingDistance <= agent.stoppingDistance && !agent.pathPending))
        {
            Vector3 newTarget = GetRandomNavMeshPoint(transform.position, wanderRadius);
            agent.SetDestination(newTarget);
            wanderTimer = 0f;
        }
    }

    // Поиск валидной точки именно НА сетке NavMesh
    private Vector3 GetRandomNavMeshPoint(Vector3 center, float radius)
    {
        Vector3 randomDirection = Random.insideUnitSphere * radius;
        randomDirection += center;

        NavMeshHit hit;
        // Ищем ближайшую точку на NavMesh в пределах радиуса (маска -1 означает все слои NavMesh)
        if (NavMesh.SamplePosition(randomDirection, out hit, radius, -1))
        {
            return hit.position;
        }

        return center; // Если точку не нашли, возвращаем текущую позицию
    }

    // Проверка условий переднего спрайта (совпадает с логикой DoomBillboard)
    private bool IsPlayerInFront()
    {
        Vector3 lookDir = target.position - transform.position;
        lookDir.y = 0;

        float angle = Vector3.SignedAngle(transform.forward, lookDir, Vector3.up);
        if (angle < 0) angle += 360f;

        int directionIndex = Mathf.RoundToInt(angle / 60f) % 6;

        // Индексы 0, 1 и 5 соответствуют передним ракурсам
        return directionIndex == 0 || directionIndex == 1 || directionIndex == 5;
    }

    // Корутина атаки
    private IEnumerator AttackRoutine()
    {
        isAtacking = true;
        isCooldown = true;
        StopAgent();

        // Мгновенно доворачиваем врага лицом к игроку перед ударом
        Vector3 lookAtTarget = target.position - transform.position;
        lookAtTarget.y = 0;
        if (lookAtTarget != Vector3.zero) transform.forward = lookAtTarget.normalized;

        // Включаем триггер нанесения урона
        if (damageCollider != null) damageCollider.enabled = true;

        // Ждем время активной фазы урона (0.5 сек — 13-й кадр)
        yield return new WaitForSeconds(damageActiveDuration);

        // Выключаем триггер урона (14-й кадр, застывание)
        if (damageCollider != null) damageCollider.enabled = false;
        isAtacking = false; 

        // Дожидаемся окончания общего кулдауна атаки
        float remainingCooldown = Mathf.Max(0f, attackCooldown - damageActiveDuration);
        yield return new WaitForSeconds(remainingCooldown);

        isCooldown = false;
    }

    // Вспомогательные методы управления NavMeshAgent
    private void StopAgent()
    {
        if (agent.isOnNavMesh)
        {
            agent.isStopped = true;
            agent.velocity = Vector3.zero;
        }
    }

    private void ResumeAgent()
    {
        if (agent.isOnNavMesh)
        {
            agent.isStopped = false;
        }
    }

    public void TakeDamage(float amount)
    {
        if (isDead) return;
        
        hp -= amount;
        if (hp <= 0) Die();
    }

    private void Die()
    {
        isDead = true;
        isAtacking = false;
        isCooldown = true;

        if (damageCollider != null) damageCollider.enabled = false;
        
        // Полностью отключаем NavMeshAgent, чтобы он не мешал физике и другим объектам
        if (agent != null)
        {
            agent.enabled = false; 
        }

        Collider mainCollider = GetComponent<Collider>();
        if (mainCollider != null) mainCollider.enabled = false;

        Debug.Log($"{gameObject.name} уничтожен (NavMeshAgent отключен).");
    }
}