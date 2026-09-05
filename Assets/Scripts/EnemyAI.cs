using UnityEngine;
using UnityEngine.AI;

public class EnemyAI : MonoBehaviour
{
    private Transform target;          // Цель (теперь находится автоматически)
    public float attackRange = 2f;    // Дистанция атаки
    public float attackCooldown = 1.5f; // Перезарядка атаки
    public int damage = 10;           // Урон

    private NavMeshAgent agent;
    private float nextAttackTime;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();

        // Автоматически находим объект с тегом Player
        GameObject player = GameObject.FindWithTag("Player");
        if (player != null)
        {
            target = player.transform;
        }
        else
        {
            Debug.LogWarning($"На сцене не найден объект с тегом 'Player' для врага {gameObject.name}");
        }
    }

    void Update()
    {
        // Если игрок уничтожен или не найден, прекращаем выполнение
        if (target == null) return;

        float distance = Vector3.Distance(transform.position, target.position);

        if (distance <= attackRange)
        {
            agent.isStopped = true; 
            if (Time.time >= nextAttackTime)
            {
                Attack();
                nextAttackTime = Time.time + attackCooldown;
            }
        }
        else
        {
            agent.isStopped = false;
            agent.SetDestination(target.position);
        }
    }

    void Attack()
    {
        Debug.Log("Враг атакует игрока!");
        // Пример нанесения урона:
        // target.GetComponent<PlayerHealth>().TakeDamage(damage);
    }
}