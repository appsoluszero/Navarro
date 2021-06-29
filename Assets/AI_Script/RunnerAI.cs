using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Pathfinding;

public class RunnerAI : MonoBehaviour
{
    [Header("Pathfinding")]
    public Transform target;
    public float activateDistance = 50f;
    public float pathUpdateSeconds = 0.5f;

    [Header("Physics")]
    public float speed = 700f;
    public float nextWaypointDistance = 3f;
    public float jumpNodeHeightRequirement = 0.8f;
    public float jumpModifier = 0.01f;
    public float jumpCheckOffset = 0.1f;
    public float fallingForce = 10f;
    public float shakeForce = 2f;
    public Bounds bounds;
    public LayerMask groundCheck;

    [Header("Custom Behavior")]

    public float attackDistance = 1f;
    public float attackRange = 1f;
    public bool followEnabled = true;
    public bool jumpEnabled = true;
    public bool directionLookEnabled = true;
    public bool isAttacking = false;
    public float xBoundMultiplier = 1.3f;
    public RaycastHit2D isGrounded;
    [Header("Animation")]
    private Animator _animator;
    [Header("Audio")]
    private AudioSource _audio;
    public AudioClip RunnerAttackSound;

    private Path path;
    private int currentWaypoint = 0;


    Seeker seeker;
    Rigidbody2D rb;

    public void Start()
    {
        seeker = GetComponent<Seeker>();
        rb = GetComponent<Rigidbody2D>();
        _animator = GetComponent<Animator>();
        _audio = GetComponent<AudioSource>();

        InvokeRepeating("UpdatePath", 0f, pathUpdateSeconds);
    }

    private void FixedUpdate()
    {
        bounds = this.GetComponent<BoxCollider2D>().bounds;
        // print("top: " + TargetOnTop());
        // print("under: " + TargetUnderFeet());
        ShakeOff();
        if (!isGrounded)
        {
            rb.AddForce(Vector2.down * fallingForce);
        }
        if (TargetInDistance() && followEnabled && !isAttacking)
        {
            PathFollow();
            if (TargetInAttackDistance())
            {
                Attack();
            }
        }
    }

    private void UpdatePath()
    {
        if (followEnabled && TargetInDistance() && seeker.IsDone())
        {
            seeker.StartPath(rb.position, target.position, OnPathComplete);
        }
    }

    private void PathFollow()
    {
        if (path == null)
        {
            return;
        }

        // Reached end of path
        if (currentWaypoint >= path.vectorPath.Count)
        {
            return;
        }

        // See if colliding with anything
        Vector3 startOffset = transform.position;
        //Debug.DrawRay(startOffset, -Vector3.up);
        RaycastHit2D boxCast = Physics2D.BoxCast(this.GetComponent<Collider2D>().bounds.center, this.GetComponent<Collider2D>().bounds.size, 0f, Vector2.down, 1f, groundCheck);

        isGrounded = Physics2D.Raycast(transform.position, -Vector3.up, GetComponent<Collider2D>().bounds.extents.y + 0.1f, groundCheck);
        isGrounded = boxCast;

        //Debug.DrawRay(transform.position, -Vector3.up * (GetComponent<Collider2D>().bounds.extents.y + 0.1f));
        //Debug.DrawRay(transform.position, -Vector3.up * (GetComponent<Collider2D>().bounds.extents.y + 0.1f));
        // Direction Calculation
        Vector2 direction = ((Vector2)path.vectorPath[currentWaypoint] - rb.position).normalized;
        Vector2 force = direction * speed * Time.deltaTime;

        // Jump
        if (jumpEnabled && isGrounded)
        {
            if (direction.y > jumpNodeHeightRequirement)
            {
                rb.AddForce(Vector2.up * speed * jumpModifier, ForceMode2D.Impulse);
                print("jump");
            }
        }

        // Movement
        rb.AddForce(force);

        // Next Waypoint
        float distance = Vector2.Distance(rb.position, path.vectorPath[currentWaypoint]);
        if (distance < nextWaypointDistance)
        {
            currentWaypoint++;
        }

        // Direction Graphics Handling
        if (directionLookEnabled)
        {
            if (rb.velocity.x > 0.05f)
            {
                transform.localScale = new Vector3(-1f * Mathf.Abs(transform.localScale.x), transform.localScale.y, transform.localScale.z);
            }
            else if (rb.velocity.x < -0.05f)
            {
                transform.localScale = new Vector3(Mathf.Abs(transform.localScale.x), transform.localScale.y, transform.localScale.z);
            }
        }
    }

    private bool TargetInDistance()
    {
        return Vector2.Distance(transform.position, target.transform.position) < activateDistance;
    }

    private bool TargetInAttackDistance()
    {
        return Vector2.Distance(transform.position, target.transform.position) <= attackDistance;
    }

    private bool TargetInAttackRange()
    {
        return Vector2.Distance(transform.position, target.transform.position) <= attackRange;
    }

    private bool TargetOnTop()
    {
        // If player position is in Runner's x-axis collider
        if (target.position.x <= bounds.center.x + bounds.extents.x * xBoundMultiplier && target.position.x >= bounds.center.x - bounds.extents.x * xBoundMultiplier)
        {
            // If player is above Runner AND not too far
            if (target.position.y >= bounds.center.y + bounds.extents.y && target.position.y <= bounds.center.y + 2 * bounds.extents.y)
            {
                return true;
            }
        }
        return false;
    }

    private bool TargetUnderFeet()
    {
        // If player position is in Runner's x-axis collider
        if (target.position.x <= bounds.center.x + bounds.extents.x * xBoundMultiplier && target.position.x >= bounds.center.x - bounds.extents.x * xBoundMultiplier)
        {
            // If player is under Runner AND not too far
            if (target.position.y <= bounds.center.y - bounds.extents.y && target.position.y >= bounds.center.y - 2 * bounds.extents.y)
            {
                return true;
            }
        }
        return false;
    }

    private void ShakeOff()
    {
        if (TargetOnTop() || TargetUnderFeet())
        {
            // 0 for left, 1 for right
            int direction = Random.Range(0, 2);

            switch (direction)
            {
                case 0:
                    rb.AddForce(Vector2.left * shakeForce, ForceMode2D.Impulse);
                    break;
                case 1:
                    rb.AddForce(Vector2.right * shakeForce, ForceMode2D.Impulse);
                    break;
                default:
                    break;
            }

        }
    }

    private void OnPathComplete(Path p)
    {
        if (!p.error)
        {
            path = p;
            currentWaypoint = 0;
        }
    }

    private void Attack()
    {
        print("Beginning to attack");
        isAttacking = true;
        _animator.Play("Runner_Attack");
    }

    public void CheckHitPlayer()
    {
        _audio.PlayOneShot(RunnerAttackSound);
        if (TargetInAttackRange())
        {
            target.GetComponent<PlayerStatus>().DecreaseHealth(1);
            print("Health after attacked: " + target.GetComponent<PlayerStatus>().currentHealth);
        }
        else
        {
            print("Runner missed");
        }
    }

    public void ResetAttackState()
    {
        isAttacking = false;
    }
}