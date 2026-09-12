using System.Collections;
using UnityEngine;
using UnityEngine.AI; // Îáÿçàòåëüíî äëÿ ðàáîòû ñ NavMesh

[RequireComponent(typeof(NavMeshAgent))]
public class EnemyAI : MonoBehaviour
{

    [Header("Õàðàêòåðèñòèêè")]
    public float hp = 100f;
    public float moveSpeed = 2f;
    
    [Header("Äèñòàíöèè")]
    public float attackDistance = 3f; // Äèñòàíöèÿ, íà êîòîðîé íà÷èíàåòñÿ àòàêà
    public float chaseDistance = 25f; // Íà êàêîì ðàññòîÿíèè âðàã íà÷èíàåò ïðåñëåäîâàòü èãðîêà

    [Header("Íàñòðîéêè ñîñòîÿíèÿ ïîêîÿ (NavMesh)")]
    public float wanderRadius = 5f;       // Ðàäèóñ ñëó÷àéíîãî ïåðåäâèæåíèÿ
    public float wanderInterval = 4f;     // Ðàç âî ñêîëüêî ñåêóíä èñêàòü íîâóþ òî÷êó

    [Header("Òàéìèíãè àòàêè")]
    public float attackCooldown = 1.5f;   // Äëèòåëüíîñòü îñòàíîâêè/àòàêè (âðåìÿ ïîêîÿ ïîñëå óäàðà)
    public float damageActiveDuration = 0.5f; // Ñêîëüêî âðåìåíè àêòèâåí êîëëàéäåð óðîíà (13-é êàäð)

    [Header("Ññûëêè íà êîìïîíåíòû")]
    public Collider damageCollider;      // Ññûëêà íà ÄÎ×ÅÐÍÈÉ êîëëàéäåð óðîíà

    // Ïóáëè÷íûå ïåðåìåííûå äëÿ DoomBillboard
    [HideInInspector] public bool isDead = false;
    [HideInInspector] public bool isAtacking = false;

    private Transform target;             // Öåëü (èãðîê)
    private float wanderTimer;
    private bool isCooldown = false;      // Ôëàã çàäåðæêè/îñòàíîâêè ÈÈ âî âðåìÿ àòàêè
    private NavMeshAgent agent;           // Íàâèãàöèîííûé àãåíò

    void Start()
    {
        // Àâòîìàòè÷åñêè íàõîäèì èãðîêà ïî òåãó
        GameObject player = GameObject.FindWithTag("Player");
        if (player != null) target = player.transform;

        // Íàñòðàèâàåì NavMeshAgent
        agent = GetComponent<NavMeshAgent>();
        agent.speed = moveSpeed;

        wanderTimer = wanderInterval; // Íà÷àòü äâèæåíèå ñðàçó â ñîñòîÿíèè ïîêîÿ

        if (damageCollider != null) damageCollider.enabled = false;
    }

    void Update()
    {
        if (isDead) return;

        // Ïðîâåðêà çäîðîâüÿ
        if (hp <= 0)
        {
            Die();
            return;
        }

        if (target == null) return;

        // Äèñòàíöèÿ ðàññ÷èòûâàåòñÿ ïî NavMesh èëè ïî ïðÿìîé (äëÿ òî÷íîñòè èñïîëüçóåì Vector3.Distance)
        float distanceToTarget = Vector3.Distance(transform.position, target.position);

        // Åñëè èäåò àòàêà èëè êóëäàóí — àãåíò ïîëíîñòüþ ñòîèò íà ìåñòå
        if (isCooldown || isAtacking)
        {
            StopAgent();
            return;
        }
        // Ïðîâåðÿåì äèñòàíöèþ àòàêè
        if (distanceToTarget <= attackDistance)
        {
            // Èãðîê äîñòàòî÷íî áëèçêî äëÿ àòàêè
            if (IsPlayerInFront())
            {
                StartCoroutine(AttackRoutine());
            }
            else
            {
                // Èãðîê áëèçêî, íî íàõîäèòñÿ ñçàäè/ñáîêó — èä¸ì ê íåìó
                ResumeAgent();
                agent.SetDestination(target.position);
            }
        }
        else if (distanceToTarget <= chaseDistance)
        {
            // Èãðîê íàõîäèòñÿ â ïðåäåëàõ 25 ìåòðîâ — ïðåñëåäóåì åãî
            ResumeAgent();
            agent.SetDestination(target.position);
        }
        else
        {
            // Èãðîê äàëüøå 25 ìåòðîâ — ñëó÷àéíî áëóæäàåì
            ResumeAgent();
            WanderBehavior();
        }
    }

    // Ëîãèêà áëóæäàíèÿ ïî NavMesh
    private void WanderBehavior()
    {
        wanderTimer += Time.deltaTime;

        // Èùåì íîâóþ òî÷êó, åñëè ïðîø¸ë èíòåðâàë ÈËÈ åñëè àãåíò óæå äîø¸ë äî ñòàðîé òî÷êè
        if (wanderTimer >= wanderInterval || (agent.remainingDistance <= agent.stoppingDistance && !agent.pathPending))
        {
            Vector3 newTarget = GetRandomNavMeshPoint(transform.position, wanderRadius);
            agent.SetDestination(newTarget);
            wanderTimer = 0f;
        }
    }

    // Ïîèñê âàëèäíîé òî÷êè èìåííî ÍÀ ñåòêå NavMesh
    private Vector3 GetRandomNavMeshPoint(Vector3 center, float radius)
    {
        Vector3 randomDirection = Random.insideUnitSphere * radius;
        randomDirection += center;

        NavMeshHit hit;
        // Èùåì áëèæàéøóþ òî÷êó íà NavMesh â ïðåäåëàõ ðàäèóñà (ìàñêà -1 îçíà÷àåò âñå ñëîè NavMesh)
        if (NavMesh.SamplePosition(randomDirection, out hit, radius, -1))
        {
            return hit.position;
        }

        return center; // Åñëè òî÷êó íå íàøëè, âîçâðàùàåì òåêóùóþ ïîçèöèþ
    }

    // Ïðîâåðêà óñëîâèé ïåðåäíåãî ñïðàéòà (ñîâïàäàåò ñ ëîãèêîé DoomBillboard)
    private bool IsPlayerInFront()
    {
        Vector3 lookDir = target.position - transform.position;
        lookDir.y = 0;

        float angle = Vector3.SignedAngle(transform.forward, lookDir, Vector3.up);
        if (angle < 0) angle += 360f;

        int directionIndex = Mathf.RoundToInt(angle / 60f) % 6;

        // Èíäåêñû 0, 1 è 5 ñîîòâåòñòâóþò ïåðåäíèì ðàêóðñàì
        return directionIndex == 0 || directionIndex == 1 || directionIndex == 5;
    }

    // Êîðóòèíà àòàêè
    private IEnumerator AttackRoutine()
    {
        isAtacking = true;
        isCooldown = true;
        StopAgent();

        // Ìãíîâåííî äîâîðà÷èâàåì âðàãà ëèöîì ê èãðîêó ïåðåä óäàðîì
        Vector3 lookAtTarget = target.position - transform.position;
        lookAtTarget.y = 0;
        if (lookAtTarget != Vector3.zero) transform.forward = lookAtTarget.normalized;

        // Âêëþ÷àåì òðèããåð íàíåñåíèÿ óðîíà
        if (damageCollider != null) damageCollider.enabled = true;

        // Æäåì âðåìÿ àêòèâíîé ôàçû óðîíà (0.5 ñåê — 13-é êàäð)
        yield return new WaitForSeconds(damageActiveDuration);

        // Âûêëþ÷àåì òðèããåð óðîíà (14-é êàäð, çàñòûâàíèå)
        if (damageCollider != null) damageCollider.enabled = false;
        isAtacking = false; 

        // Äîæèäàåìñÿ îêîí÷àíèÿ îáùåãî êóëäàóíà àòàêè
        float remainingCooldown = Mathf.Max(0f, attackCooldown - damageActiveDuration);
        yield return new WaitForSeconds(remainingCooldown);

        isCooldown = false;
    }

    // Âñïîìîãàòåëüíûå ìåòîäû óïðàâëåíèÿ NavMeshAgent
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
        
        // Ïîëíîñòüþ îòêëþ÷àåì NavMeshAgent, ÷òîáû îí íå ìåøàë ôèçèêå è äðóãèì îáúåêòàì
        if (agent != null)
        {
            agent.enabled = false; 
        }

        Collider mainCollider = GetComponent<Collider>();
        if (mainCollider != null) mainCollider.enabled = false;

        Debug.Log($"{gameObject.name} óíè÷òîæåí (NavMeshAgent îòêëþ÷åí).");
    }
}