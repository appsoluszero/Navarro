using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Pathfinding;

public class CrawlerAI : MonoBehaviour, ArtificialIntelligence
{
    [Header("Pathfinding")]
    public Transform target;
    public float activateDistance = 50f;
    public float pathUpdateSeconds = 0.5f;
    private Path path;
    private int currentWaypoint = 0;
    RaycastHit2D isGrounded;
    Seeker seeker;
    Rigidbody2D rb;

    [Header("Physics")]
    public float speed = 700f;
    public float nextWaypointDistance = 3f;
    public float jumpNodeHeightRequirement = 0.8f;
    public float jumpModifier = 0.01f;
    public float jumpCheckOffset = 0.1f;
    public float fallingForce = 10f;
    public float shakeForce = 2f;

    public Bounds bounds;

    [Header("Custom Behavior")]

    public float attackDistance = 1f;

    public float attackRange = 1f;
    public bool followEnabled = true;
    public bool jumpEnabled = true;
    public bool directionLookEnabled = true;

    public bool isAttacking = false;
    public bool isHurting = false;
    public float xBoundMultiplier = 1.3f;
    [Header("Animation")]
    private Animator _animator;
    [Header("Audio")]
    private AudioSource _audio;
    public AudioClip CrawlerAttackSound;

    [Header("Rotation")]
    public float angleOffset = 30f;
    public float goalAngle;
    public float currentAngle;
    public float angleTurnPerFrame = 1;



    public void Start()
    {
        seeker = GetComponent<Seeker>();
        rb = GetComponent<Rigidbody2D>();
        _animator = GetComponent<Animator>();
        _audio = GetComponent<AudioSource>();
        currentAngle = this.transform.rotation.z;

        InvokeRepeating("UpdatePath", 0f, pathUpdateSeconds);
    }

    private void FixedUpdate()
    {
        bounds = this.GetComponent<BoxCollider2D>().bounds;

        // print("top: " + TargetOnTop());
        // print("under: " + TargetUnderFeet());
        ShakeOff();

        if (TargetInDistance() && followEnabled && !isAttacking)
        {
            PathFollow();
            RotateToMovingDirection();
            if (TargetInAttackDistance())
            {
                Attack();
            }
        }
        else if (!isAttacking && !isHurting)
        {
            _animator.Play("Crawler_Idle");
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
        Vector3 startOffset = transform.position - new Vector3(0f, GetComponent<Collider2D>().bounds.extents.y + jumpCheckOffset);
        isGrounded = Physics2D.Raycast(startOffset, -Vector3.up, 0.05f);

        // Direction Calculation
        Vector2 direction = ((Vector2)path.vectorPath[currentWaypoint] - rb.position).normalized;
        Vector2 force = direction * speed * Time.deltaTime;

        // Movement
        //rb.AddForce(force);

        if (isAttacking)
        {
            force = new Vector2(0, 0);
        }


        rb.velocity = force;

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
            // If player is above Runner
            if (target.position.y >= bounds.center.y + bounds.extents.y)
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
            // If player is under Runner
            if (target.position.y <= bounds.center.y - bounds.extents.y)
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
        _animator.Play("Crawler_Attack");
    }

    public void TakeDamage()
    {
        isAttacking = false;
        isHurting = true;
        _animator.Play("Crawler_Hurt");
    }

    public void CheckHitPlayer()
    {
        if (TargetInAttackRange() && target.GetComponent<PlayerStatus>().playerState != State.Rolling && target.GetComponent<PlayerStatus>().playerState != State.Hurt && target.GetComponent<PlayerStatus>().playerState != State.Death)
        {
            target.GetComponent<PlayerStatus>().DecreaseHealth(1);
            print("Health after attacked: " + target.GetComponent<PlayerStatus>().currentHealth);
        }
        else
        {
            print("Crawler missed");
        }
    }

    public void SetTarget(Transform target) {
        this.target = target;
    }

    public void ResetAttackState()
    {
        StartCoroutine(delayAttackCheck());
    }

    public void ResetHurtState() {
        isHurting = false;
        _animator.Play("Crawler_Idle");
    }

    IEnumerator delayAttackCheck()
    {
        yield return new WaitForSeconds(0.65f);
        isAttacking = false;
        _animator.Play("Crawler_Idle");
    }

    private void RotateToMovingDirection()
    {
        Vector2 direction = rb.velocity.normalized;

        goalAngle = Vector2.Angle(Vector2.right.normalized, direction);
        goalAngle += angleOffset;

        if (goalAngle > currentAngle && goalAngle - currentAngle >= angleTurnPerFrame)
        {
            currentAngle += angleTurnPerFrame;
        }
        else if (goalAngle < currentAngle && currentAngle - goalAngle >= angleTurnPerFrame)
        {
            currentAngle -= angleTurnPerFrame;
        }

        this.transform.rotation = Quaternion.Euler(0, 0, currentAngle);


    }
}