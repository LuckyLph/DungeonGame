using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Pathfinding;

public class AIController : MonoBehaviour
{
    [SerializeField]
    private bool agentEnabled;

    [SerializeField] private int damage;

    private PlayerSensor playerSensor;

    public bool AgentEnabled
    {
        get { return agentEnabled; }
        set { agentEnabled = value; }
    }

    private AIPath agent;
    private bool facingRight = false;
    private Vector3 currentMovementVector;
    private Animator animator;
    private SpriteRenderer spriteRenderer;
    private bool hasMoved;
    private float switchTime = float.PositiveInfinity;
    private float damageDelay = 1f;
    private SimpleCharacterController player;

    private void Awake()
    {
        agent = GetComponent<AIPath>();
        animator = GetComponentInChildren<Animator>();
        spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        playerSensor = GetComponentInChildren<PlayerSensor>();
    }

    private void OnEnable()
    {
        agent.OnAIMove += OnAIMove;
        playerSensor.OnPlayerSensorEntered += OnPlayerSensorEntered;
        playerSensor.OnPlayerSensorExited += OnPlayerSensorExited;
    }

    private void OnDisable()
    {
        agent.OnAIMove -= OnAIMove;
        playerSensor.OnPlayerSensorEntered -= OnPlayerSensorEntered;
        playerSensor.OnPlayerSensorExited -= OnPlayerSensorExited;
    }

    void Update()
    {
        agent.IsEnabled = AgentEnabled;

        if (player != null)
        {
            if (float.IsPositiveInfinity(switchTime))
            {
                switchTime = Time.time + damageDelay;
            }
            if (Time.time >= switchTime)
            {
                DamagePlayer();
                switchTime = float.PositiveInfinity;
            }
        }

        if (!agent.isStopped && currentMovementVector.normalized != Vector3.zero)
        {
            hasMoved = true;
            animator.SetBool("isWalking", true);
            animator.SetFloat("inputX", currentMovementVector.normalized.x);
            animator.SetFloat("inputY", currentMovementVector.normalized.y);
            animator.speed = Mathf.Max(Vector3.Distance(Vector3.zero, currentMovementVector.normalized), 0.2f);
            if ((currentMovementVector.normalized.x > 0 && facingRight) || (currentMovementVector.normalized.x < 0 && !facingRight))
            {
                Flip();
            }
        }
    }

    private void LateUpdate()
    {
        if (!hasMoved)
        {
            animator.SetBool("isWalking", false);
        }
        hasMoved = false;
    }

    private void Flip()
    {
        facingRight = !facingRight;

        // Multiply the player's x local scale by -1
        Vector3 theScale = spriteRenderer.transform.localScale;
        theScale.x *= -1;
        spriteRenderer.transform.localScale = theScale;
    }

    private void DamagePlayer()
    {
        player.GiveDamage(damage);
    }

    public void OnAIMove(Vector3 movement)
    {
        currentMovementVector = movement;
    }

    private void OnPlayerSensorEntered(GameObject player)
    {
        this.player = player.transform.parent.GetComponent<SimpleCharacterController>();
        DamagePlayer();
    }

    private void OnPlayerSensorExited(GameObject player)
    {
        this.player = null;
    }
}
