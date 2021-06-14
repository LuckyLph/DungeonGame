using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Pathfinding;

public class AIController : MonoBehaviour
{
    [SerializeField]
    private bool agentEnabled;

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

    private void Awake()
    {
        agent = GetComponent<AIPath>();
        animator = GetComponentInChildren<Animator>();
        spriteRenderer = GetComponentInChildren<SpriteRenderer>();
    }

    private void OnEnable()
    {
        agent.OnAIMove += OnAIMove;
    }

    private void OnDisable()
    {
        agent.OnAIMove -= OnAIMove;
    }

    void Update()
    {
        agent.IsEnabled = AgentEnabled;

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

    public void OnAIMove(Vector3 movement)
    {
        currentMovementVector = movement;
    }
}
