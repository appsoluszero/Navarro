using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Pathfinding;

public class RunnerAI : MonoBehaviour, ArtificialIntelligence
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
    public bool isHurting = false;
    public float xBoundMultiplier = 1.3f;
    public RaycastHit2D isGrounded;
    public RaycastHit2D rightGroundCheck;
    public RaycastHit2D leftGroundCheck;

    [Header("Animation")]
    private Animator _animator;
    [Header("Audio")]
    private AudioSource _audio;
    public AudioClip RunnerAttackSound;
    bool isFollowPath = false;

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

    private void Update()
    {
        isFollowPath = false;
        //print(isAttacking);
        bounds = this.GetComponent<BoxCollider2D>().bounds;
        // print("top: " + TargetOnTop());
        // print("under: " + TargetUnderFeet());
        ShakeOff();
        if (!isGrounded)
        {
            rb.AddForce(Vector2.down * fallingForce);
        }
        if (TargetInDistance() && followEnabled && !isAttacking && !isHurting)
        {
            isFollowPath = true;
            if (TargetInAttackDistance())
            {
                Attack();
            }
        }
        else if (!isAttacking && !isHurting)
        {
            _animator.Play("Runner_Idle");
        }
    }

    private void FixedUpdate()
    {
        if (isFollowPath)
            PathFollow();
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
        RaycastHit2D boxCast = Physics2D.BoxCast(this.GetComponent<Collider2D>().bounds.center, this.GetComponent<Collider2D>().bounds.size, 0f, Vector2.down, 0.1f, groundCheck);

        isGrounded = Physics2D.Raycast(transform.position, -Vector3.up, GetComponent<Collider2D>().bounds.extents.y + 0.1f, groundCheck);
        isGrounded = boxCast;

        //Debug.DrawRay(transform.position, -Vector3.up * (GetComponent<Collider2D>().bounds.extents.y + 0.1f));
        //Debug.DrawRay(transform.position, -Vector3.up * (GetComponent<Collider2D>().bounds.extents.y + 0.1f));
        // Direction Calculation

        Vector2 direction = ((Vector2)path.vectorPath[currentWaypoint] - rb.position).normalized;
        Vector2 forceDirection = FourDirectionConvert(direction);
        //forceDirection = EdgeHandler(forceDirection);
        Debug.DrawRay(transform.position, forceDirection * 2, Color.red);
        Debug.DrawRay(transform.position, direction * 2, Color.blue);
        //Vector2 direction = GetDirection();
        Vector2 force = forceDirection * speed * Time.fixedDeltaTime;

        // Jump
        if (jumpEnabled && isGrounded)
        {
            if (direction.y > jumpNodeHeightRequirement)
            {
                Jump();
            }
        }

        // Movement
        //rb.velocity = force;
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
    public Vector2 Rotate(Vector2 v, float degrees)
    {
        float sin = Mathf.Sin(degrees * Mathf.Deg2Rad);
        float cos = Mathf.Cos(degrees * Mathf.Deg2Rad);

        float tx = v.x;
        float ty = v.y;
        v.x = (cos * tx) - (sin * ty);
        v.y = (sin * tx) + (cos * ty);
        return v;
    }

    private Vector2 GetDirection()
    {
        isGrounded = Physics2D.Raycast(transform.position, -Vector3.up, GetComponent<Collider2D>().bounds.extents.y + 0.1f, groundCheck);

        Vector2 rightPosition = new Vector2(transform.position.x + GetComponent<Collider2D>().bounds.extents.x, transform.position.y);
        Vector2 leftPosition = new Vector2(transform.position.x - GetComponent<Collider2D>().bounds.extents.x, transform.position.y);

        rightGroundCheck = Physics2D.Raycast(rightPosition, -Vector3.up, GetComponent<Collider2D>().bounds.extents.y + 0.1f, groundCheck);
        leftGroundCheck = Physics2D.Raycast(leftPosition, -Vector3.up, GetComponent<Collider2D>().bounds.extents.y + 0.1f, groundCheck);

        Vector2 rightNormal = rightGroundCheck.normal;
        Vector2 leftNormal = leftGroundCheck.normal;

        Vector2 rightDirection = Rotate(rightNormal, 90);
        Vector2 leftDirection = Rotate(rightNormal, -90);
        Vector2 defaultDirection = ((Vector2)path.vectorPath[currentWaypoint] - rb.position).normalized;

        Debug.DrawRay(transform.position, rightDirection);
        Debug.DrawRay(transform.position, leftDirection);

        if (defaultDirection.x - target.position.x > 0 && rightGroundCheck)
        {
            return rightDirection;
        }
        else if (target.position.x - defaultDirection.x > 0 && leftGroundCheck)
        {
            return leftDirection;
        }
        else
        {
            return defaultDirection;
        }
    }

    private Vector2 FourDirectionConvert(Vector2 direction)
    {
        float size = direction.magnitude;

        if (direction.y != 0 && direction.x != 0)
        {
            return new Vector2(direction.x, 0);
        }
        return direction;

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

    private void Jump()
    {
        rb.AddForce(Vector2.up * speed * jumpModifier, ForceMode2D.Impulse);
        print("jump");
    }

    private Vector2 EdgeHandler(Vector2 movingDirection)
    {
        Vector2 rightPosition = new Vector2(transform.position.x + GetComponent<Collider2D>().bounds.extents.x, transform.position.y);
        Vector2 leftPosition = new Vector2(transform.position.x - GetComponent<Collider2D>().bounds.extents.x, transform.position.y);

        rightGroundCheck = Physics2D.Raycast(rightPosition, -Vector3.up, GetComponent<Collider2D>().bounds.extents.y + 0.1f, groundCheck);
        leftGroundCheck = Physics2D.Raycast(leftPosition, -Vector3.up, GetComponent<Collider2D>().bounds.extents.y + 0.1f, groundCheck);

        if (movingDirection.x == 0 && movingDirection.y != 0)
        {
            if (rightGroundCheck && !leftGroundCheck)
            {
                return new Vector2(-movingDirection.magnitude, 0);
            }
            else if (!rightGroundCheck && leftGroundCheck)
            {
                return new Vector2(movingDirection.magnitude, 0);
            }
        }
        return movingDirection;
    }

    private void OnPathComplete(Path p)
    {
        if (!p.error)
        {
            path = p;
            currentWaypoint = 0;
        }
    }

    public void SetTarget(Transform target) {
        this.target = target;
    }

    private void Attack()
    {
        print("Beginning to attack");
        isAttacking = true;
        _animator.Play("Runner_Attack");
    }

    public void TakeDamage() {
        isAttacking = false;
        isHurting = true;
        _animator.Play("Runner_Hurt");
    }

    public void CheckHitPlayer()
    {
        _audio.PlayOneShot(RunnerAttackSound);
        if (TargetInAttackRange() && target.GetComponent<PlayerStatus>().playerState != State.Rolling && target.GetComponent<PlayerStatus>().playerState != State.Hurt && target.GetComponent<PlayerStatus>().playerState != State.Death)
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
        StartCoroutine(delayAttackCheck());
    }

    public void ResetHurtState() {
        isHurting = false;
        _animator.Play("Runner_Idle");
    }

    IEnumerator delayAttackCheck()
    {
        yield return new WaitForSeconds(0.65f);
        isAttacking = false;
        _animator.Play("Runner_Idle");
    }

}