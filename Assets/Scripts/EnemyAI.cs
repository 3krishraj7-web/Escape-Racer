using UnityEngine;
using System.Collections;

public class EnemyAI : MonoBehaviour
{
    public enum EnemyState
    {
        Patrol,
        Chase,
        Attack,
        Block,
        Surround
    }

    [Header("Enemy State")]
    public EnemyState currentState = EnemyState.Patrol;

    [Header("Speed Settings")]
    public float patrolSpeed = 12f;
    public float chaseSpeed = 25f;
    public float attackSpeed = 35f;
    public float blockSpeed = 30f;

    [Header("Detection")]
    public float detectionRange = 50f;
    public float attackRange = 15f;
    public float blockRange = 40f;

    [Header("Combat")]
    public float ramDamage = 15f;
    public float health = 50f;

    [Header("Behavior")]
    public float steerSpeed = 80f;
    public float stateChangeInterval = 3f;

    // Private
    private Rigidbody rb;
    private Transform player;
    private PlayerCarController playerController;
    private GameUI gameUI;
    private float stateTimer = 0f;
    private float currentSpeed;
    private bool hasBeenSpotted = false;

    private string[] attackTaunts = {
        "YOU CANT ESCAPE ME!",
        "NOWHERE TO RUN!",
        "I WILL CRUSH YOU!",
        "THE ROAD IS MINE!",
        "FEEL THE FURY!",
        "NO MERCY!",
        "PREPARE TO DIE!"
    };

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        gameUI = FindObjectOfType<GameUI>();

        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            player = playerObj.transform;
            playerController = playerObj.GetComponent<PlayerCarController>();
        }

        currentSpeed = patrolSpeed;
        currentState = EnemyState.Patrol;
    }

    void Update()
    {
        if (player == null) return;
        if (playerController != null && !playerController.isAlive) return;

        float distanceToPlayer = Vector3.Distance(
            transform.position, player.position);

        stateTimer += Time.deltaTime;
        UpdateState(distanceToPlayer);

        // Auto destroy if too far behind
        if (transform.position.z < player.position.z - 100f)
        {
            Destroy(gameObject);
        }
    }

    void UpdateState(float distance)
    {
        if (distance > detectionRange)
        {
            SetState(EnemyState.Patrol);
        }
        else if (distance <= attackRange)
        {
            SetState(EnemyState.Attack);
        }
        else if (distance <= blockRange && stateTimer > stateChangeInterval)
        {
            stateTimer = 0f;
            int decision = Random.Range(0, 3);

            if (decision == 0)
                SetState(EnemyState.Block);
            else if (decision == 1)
                SetState(EnemyState.Surround);
            else
                SetState(EnemyState.Chase);
        }
        else if (distance <= detectionRange)
        {
            SetState(EnemyState.Chase);
        }
    }

    void SetState(EnemyState newState)
    {
        if (currentState == newState) return;
        currentState = newState;

        switch (newState)
        {
            case EnemyState.Patrol:
                currentSpeed = patrolSpeed;
                break;

            case EnemyState.Chase:
                currentSpeed = chaseSpeed;
                if (!hasBeenSpotted)
                {
                    hasBeenSpotted = true;
                    ShowTaunt();
                }
                break;

            case EnemyState.Attack:
                currentSpeed = attackSpeed;
                ShowTaunt();
                break;

            case EnemyState.Block:
                currentSpeed = blockSpeed;
                break;

            case EnemyState.Surround:
                currentSpeed = chaseSpeed;
                break;
        }
    }

    void FixedUpdate()
    {
        if (player == null) return;
        if (playerController != null && !playerController.isAlive) return;

        switch (currentState)
        {
            case EnemyState.Patrol:
                DoPatrol();
                break;
            case EnemyState.Chase:
                DoChase();
                break;
            case EnemyState.Attack:
                DoAttack();
                break;
            case EnemyState.Block:
                DoBlock();
                break;
            case EnemyState.Surround:
                DoSurround();
                break;
        }
    }

    void DoPatrol()
    {
        rb.linearVelocity = transform.forward * currentSpeed;
    }

    void DoChase()
    {
        Vector3 direction = (player.position - transform.position).normalized;
        Quaternion targetRot = Quaternion.LookRotation(direction);
        transform.rotation = Quaternion.Slerp(
            transform.rotation,
            targetRot,
            3f * Time.fixedDeltaTime
        );
        rb.linearVelocity = transform.forward * currentSpeed;
    }

    void DoAttack()
    {
        Vector3 direction = (player.position - transform.position).normalized;
        Quaternion targetRot = Quaternion.LookRotation(direction);
        transform.rotation = Quaternion.Slerp(
            transform.rotation,
            targetRot,
            8f * Time.fixedDeltaTime
        );
        rb.linearVelocity = transform.forward * currentSpeed;
    }

    void DoBlock()
    {
        Vector3 blockTarget = new Vector3(
            player.position.x,
            transform.position.y,
            player.position.z + 20f
        );

        Vector3 direction = (blockTarget - transform.position).normalized;
        Quaternion targetRot = Quaternion.LookRotation(direction);
        transform.rotation = Quaternion.Slerp(
            transform.rotation,
            targetRot,
            4f * Time.fixedDeltaTime
        );
        rb.linearVelocity = transform.forward * currentSpeed;
    }

    void DoSurround()
    {
        float sideOffset = transform.position.x < player.position.x ? -3f : 3f;
        Vector3 surroundTarget = new Vector3(
            player.position.x + sideOffset,
            transform.position.y,
            player.position.z + 5f
        );

        Vector3 direction = (surroundTarget - transform.position).normalized;
        Quaternion targetRot = Quaternion.LookRotation(direction);
        transform.rotation = Quaternion.Slerp(
            transform.rotation,
            targetRot,
            4f * Time.fixedDeltaTime
        );
        rb.linearVelocity = transform.forward * currentSpeed;
    }

    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            PlayerCarController pc =
                collision.gameObject.GetComponent<PlayerCarController>();

            if (pc != null)
            {
                float impactSpeed = collision.relativeVelocity.magnitude;
                pc.TakeDamage(ramDamage);

                if (impactSpeed > 8f)
                {
                    TakeDamage(impactSpeed * 1.5f);
                }
            }
        }
    }

    public void TakeDamage(float damage)
    {
        health -= damage;
        StartCoroutine(FlashWhite());
        Debug.Log("Enemy HP: " + health);

        if (health <= 0)
        {
            DestroyEnemy();
        }
    }

    void DestroyEnemy()
    {
        if (gameUI != null)
            gameUI.AddScore(100f);

        SpawnExplosion();
        Destroy(gameObject);
    }

    void SpawnExplosion()
    {
        for (int i = 0; i < 5; i++)
        {
            GameObject part = GameObject.CreatePrimitive(PrimitiveType.Cube);
            part.transform.position = transform.position +
                Random.insideUnitSphere * 1f;
            part.transform.localScale = Vector3.one * 0.3f;

            Renderer r = part.GetComponent<Renderer>();
            if (r != null) r.material.color = Color.yellow;

            Rigidbody partRb = part.AddComponent<Rigidbody>();
            partRb.AddForce(Random.insideUnitSphere * 300f);

            Destroy(part, 0.5f);
        }
    }

    void ShowTaunt()
    {
        string taunt = attackTaunts[Random.Range(0, attackTaunts.Length)];
        Debug.Log("ENEMY: " + taunt);

        if (gameUI != null)
            gameUI.ShowEnemyTaunt(taunt);
    }

    IEnumerator FlashWhite()
    {
        Renderer rend = GetComponent<Renderer>();
        if (rend != null)
        {
            Color original = rend.material.color;
            rend.material.color = Color.white;
            yield return new WaitForSeconds(0.1f);
            rend.material.color = original;
        }
    }
}