using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Pathfinding;

public class RangerAI : MonoBehaviour
{
    [Header("Pathfinding")]
    public Transform target;
    public float activateDistance = 50f;
    public float pathUpdateSeconds = 0.5f;
    private Path path;
    private int currentWaypoint = 0;
    public bool directionLookEnabled = true;

    [Header("Physics")]
    public float speed = 700f;
    public float nextWaypointDistance = 3f;
    public float jumpNodeHeightRequirement = 0.8f;
    public float jumpModifier = 0.01f;
    public float jumpCheckOffset = 0.1f;
    public float fallingForce = 10f;
    public float shakeForce = 2f;

    public Bounds bounds;

    [Header("Movement Behavior")]
    public bool followEnabled = true;
    public bool jumpEnabled = true;

    public float xBoundMultiplier = 1.3f;
    public float fleeSpeed = 1000f;
    public int fleeDuration = 60;
    public int fleeDurationCount = 0;
    public bool isFleeing = false;
    public float fleeRange = 4f;

    [Header("Detect Behavior")]
    public float detectRange = 10f;
    public LayerMask collisionMask;

    [Header("Attack Behavior")]
    public float attackDistance = 1f;
    public float attackRange = 1f;
    public float shootDistance = 10f;
    public float shootRange = 12f;
    public bool isAttacking = false;
    public bool isShooting = false;
    public int shootCooldown = 60;
    public int shootCooldownCount = 60;

    private RaycastHit2D canShoot;

    RaycastHit2D isGrounded;
    Seeker seeker;
    Rigidbody2D rb;

    public void Start()
    {
        seeker = GetComponent<Seeker>();
        rb = GetComponent<Rigidbody2D>();

        InvokeRepeating("UpdatePath", 0f, pathUpdateSeconds);
    }

    private void FixedUpdate()
    {
        bounds = this.GetComponent<BoxCollider2D>().bounds;
        canShoot = Physics2D.Raycast(this.GetComponent<Transform>().position, (target.GetComponent<Transform>().position - this.GetComponent<Transform>().position).normalized, shootDistance, collisionMask);
        // print("top: " + TargetOnTop());
        // print("under: " + TargetUnderFeet());
        ShakeOff();
        Countdown();
        if (!isGrounded)
        {
            rb.AddForce(Vector2.down * fallingForce);
        }
        if (TargetInDistance() && followEnabled && !isAttacking && !isShooting)
        {
            if (!TargetInShootDistance())
            {
                PathFollow();
            }

            if (TargetInAttackDistance())
            {
                Attack();
                Flee();

            }
            else if (canShoot && shootCooldown == 60)
            {
                Shoot();
                shootCooldownCount = 0;
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
        Vector3 startOffset = transform.position - new Vector3(0f, GetComponent<Collider2D>().bounds.extents.y + jumpCheckOffset);
        isGrounded = Physics2D.Raycast(startOffset, -Vector3.up, 0.05f);

        // Direction Calculation
        Vector2 direction = ((Vector2)path.vectorPath[currentWaypoint] - rb.position).normalized;
        if (isFleeing)
        {
            direction *= -1;
        }
        Vector2 force = direction * speed * Time.deltaTime;

        // Jump
        if (jumpEnabled && isGrounded)
        {
            if (direction.y > jumpNodeHeightRequirement)
            {
                rb.AddForce(Vector2.up * speed * jumpModifier, ForceMode2D.Impulse);
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

    private bool TargetInShootDistance()
    {
        return Vector2.Distance(transform.position, target.transform.position) <= shootDistance;
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
        // Attack stuffs
        StartCoroutine(AttackRoutine());

    }

    private void Shoot()
    {
        print("Shoot");
        isShooting = true;
        // Shooting stuffs
        StartCoroutine(ShootRoutine());

    }

    private void Flee()
    {
        isFleeing = true;
        fleeDurationCount = 0;
    }

    private void Countdown()
    {
        if (shootCooldownCount < shootCooldown)
        {
            shootCooldown++;
        }
        if (fleeDurationCount < fleeDuration)
        {
            fleeDurationCount++;
        }
        if (fleeDurationCount == fleeDuration)
        {
            isFleeing = false;
        }
    }

    IEnumerator AttackRoutine()
    {
        yield return new WaitForSeconds(1f);
        if (TargetInAttackRange())
        {
            target.GetComponent<PlayerStatus>().DecreaseHealth(1);
            print("Health after attacked: " + target.GetComponent<PlayerStatus>().currentHealth);
        }
        else
        {
            print("Runner missed");
        }
        isAttacking = false;
    }
    IEnumerator RunAwayRoutine()
    {
        yield return new WaitForSeconds(1f);
        if (TargetInAttackRange())
        {
            target.GetComponent<PlayerStatus>().DecreaseHealth(1);
            print("Health after attacked: " + target.GetComponent<PlayerStatus>().currentHealth);
        }
        else
        {
            print("Runner missed");
        }
        isAttacking = false;
    }


    IEnumerator ShootRoutine()
    {
        yield return new WaitForSeconds(1f);
        // Add somrthing here
        isShooting = false;
    }

}