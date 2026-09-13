using System.Collections;
using UnityEngine;
using UnityEngine.AI; 

[RequireComponent(typeof(NavMeshAgent))]
public class EnemyAI : MonoBehaviour
{

    public float hp = 100f;
    public float moveSpeed = 2f;
    
    public float attackDistance = 3f; 
    public float chaseDistance = 25f; 

    public float wanderRadius = 5f;       
    public float wanderInterval = 4f;     

    public float attackCooldown = 1.5f;   
    public float damageActiveDuration = 0.5f; 

    public Collider damageCollider;    

    [HideInInspector] public bool isDead = false;
    [HideInInspector] public bool isAtacking = false;

    private Transform target;            
    private float wanderTimer;
    private bool isCooldown = false;     
    private NavMeshAgent agent;          

    void Start()
    {
        GameObject player = GameObject.FindWithTag("Player");
        if (player != null) target = player.transform;

        agent = GetComponent<NavMeshAgent>();
        agent.speed = moveSpeed;

        wanderTimer = wanderInterval; 

        if (damageCollider != null) damageCollider.enabled = false;
    }

    void Update()
    {
        if (isDead) return;

        if (hp <= 0)
        {
            Die();
            return;
        }

        if (target == null) return;

        float distanceToTarget = Vector3.Distance(transform.position, target.position);

        if (isCooldown || isAtacking)
        {
            StopAgent();
            return;
        }
        if (distanceToTarget <= attackDistance)
        {
            if (IsPlayerInFront())
            {
                StartCoroutine(AttackRoutine());
            }
            else
            {
                ResumeAgent();
                agent.SetDestination(target.position);
            }
        }
        else if (distanceToTarget <= chaseDistance)
        {
            ResumeAgent();
            agent.SetDestination(target.position);
        }
        else
        {
            ResumeAgent();
            WanderBehavior();
        }
    }

    private void WanderBehavior()
    {
        wanderTimer += Time.deltaTime;

        if (wanderTimer >= wanderInterval || (agent.remainingDistance <= agent.stoppingDistance && !agent.pathPending))
        {
            Vector3 newTarget = GetRandomNavMeshPoint(transform.position, wanderRadius);
            agent.SetDestination(newTarget);
            wanderTimer = 0f;
        }
    }

    private Vector3 GetRandomNavMeshPoint(Vector3 center, float radius)
    {
        Vector3 randomDirection = Random.insideUnitSphere * radius;
        randomDirection += center;

        NavMeshHit hit;
        if (NavMesh.SamplePosition(randomDirection, out hit, radius, -1))
        {
            return hit.position;
        }

        return center; 
    }

    private bool IsPlayerInFront()
    {
        Vector3 lookDir = target.position - transform.position;
        lookDir.y = 0;

        float angle = Vector3.SignedAngle(transform.forward, lookDir, Vector3.up);
        if (angle < 0) angle += 360f;

        int directionIndex = Mathf.RoundToInt(angle / 60f) % 6;

        return directionIndex == 0 || directionIndex == 1 || directionIndex == 5;
    }

    private IEnumerator AttackRoutine()
    {
        isAtacking = true;
        isCooldown = true;
        StopAgent();

        Vector3 lookAtTarget = target.position - transform.position;
        lookAtTarget.y = 0;
        if (lookAtTarget != Vector3.zero) transform.forward = lookAtTarget.normalized;

        // Ждём 0.5 секунды после начала атаки
        yield return new WaitForSeconds(0.5f);

        // Включаем коллайдер
        if (damageCollider != null)
            damageCollider.enabled = true;

        // Коллайдер активен 0.5 секунды
        yield return new WaitForSeconds(damageActiveDuration);

        // Выключаем коллайдер
        if (damageCollider != null)
            damageCollider.enabled = false;
        isAtacking = false; 

        float remainingCooldown = Mathf.Max(0f, attackCooldown - damageActiveDuration);
        yield return new WaitForSeconds(remainingCooldown);

        isCooldown = false;
    }

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
        
        if (agent != null)
        {
            agent.enabled = false; 
        }

        Collider mainCollider = GetComponent<Collider>();
        if (mainCollider != null) mainCollider.enabled = false;

    }
}