using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Pathfinding;

public class RangerAI : MonoBehaviour, ArtificialIntelligence
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
    public int fleeDuration = 200;
    public int fleeDurationCount = 0;
    public bool isFleeing = false;
    public float fleeRange = 4f;

    [Header("Detect Behavior")]
    public float detectRange = 10f;
    public LayerMask playerCollisionMask;
    public LayerMask wallCollisionMask;

    [Header("Attack Behavior")]
    public float attackDistance = 1f;
    public float attackRange = 1f;
    public float shootDistance = 10f;
    public float shootRange = 12f;
    public bool isAttacking = false;
    public bool isShooting = false;
    public bool isHurting = false;
    public int shootCooldown = 60;
    public int shootCooldownCount = 60;
    public float bulletForce = 10f;
    [Header("Animation")]
    private Animator _animator;
    [Header("Audio")]
    private AudioSource _audio;
    public AudioClip RangerAttackSound;

    private bool canShoot;
    private RaycastHit2D hitPlayer;
    private RaycastHit2D hitWall;

    RaycastHit2D isGrounded;
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
        CheckShoot();
        // print("top: " + TargetOnTop());
        // print("under: " + TargetUnderFeet());
        ShakeOff();
        Countdown();
        // If floating >> get on the ground
        if (!isGrounded)
        {
            rb.AddForce(Vector2.down * fallingForce);
        }
        // If player gets too close, go into fleeing mode
        if (TargetInFleeRange() && !isFleeing)
        {
            Flee();
        }
        // If distance is okay, stop fleeing
        if (TargetDistance() >= shootDistance)
        {
            isFleeing = false;
            fleeDurationCount = fleeDuration;
        }
        // Detected player
        if (TargetInDistance() && followEnabled)
        {
            // If not attacking
            if (!isAttacking && !isShooting)
            {
                // Can melee (close range)
                if (TargetInAttackDistance())
                {
                    Attack();
                    Flee();

                }
                // Can shoot (normal distance)
                else if (canShoot && shootCooldown == 60)
                {
                    Shoot();
                    shootCooldownCount = 0;
                }
            }

            // Walk if target out of range for shoot OR need to flee
            if ((!TargetInShootDistance() || isFleeing) && !isAttacking && !isShooting)
            {
                PathFollow();
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

        // Flee Mechanics
        if (isFleeing)
        {
            // if target is right
            if (target.position.x > this.transform.position.x)
            {
                direction = -transform.right;
            }
            else if (target.position.x < this.transform.position.x)
            {
                direction = transform.right;
            }
        }

        Vector2 force = direction * speed;



        // Jump
        if (jumpEnabled && isGrounded)
        {
            if (direction.y > jumpNodeHeightRequirement)
            {
                rb.AddForce(Vector2.up * speed * jumpModifier, ForceMode2D.Impulse);
            }
        }

        // Movement
        if (!(TargetDistance() >= shootDistance && isFleeing))
        {
            //rb.AddForce(force);
            rb.velocity = force;
        }


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

    private float TargetDistance()
    {
        return Vector2.Distance(transform.position, target.transform.position);
    }
    private bool TargetInDistance()
    {
        return Vector2.Distance(transform.position, target.transform.position) < activateDistance;
    }

    private bool TargetInAttackDistance()
    {
        return Vector2.Distance(transform.position, target.transform.position) <= attackDistance;
    }

    public void SetTarget(Transform target) {
        this.target = target;
    }

    private bool TargetInAttackRange()
    {
        return Vector2.Distance(transform.position, target.transform.position) <= attackRange;
    }

    private bool TargetInShootDistance()
    {
        return Vector2.Distance(transform.position, target.transform.position) <= shootDistance;
    }

    private bool TargetInFleeRange()
    {
        return Vector2.Distance(transform.position, target.transform.position) <= fleeRange;
    }

    private Vector2 TargetDirection()
    {
        return (((Vector2)target.position - rb.position)).normalized;
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

    public void TakeDamage()
    {
        isAttacking = false;
        isHurting = true;
        //_animator.Play("Runner_Hurt");
    }

    private void Shoot()
    {
        print("Shoot");
        isShooting = true;
        // Shooting stuffs
        Vector2 bulletSpawnPosition = this.transform.position;
        bulletSpawnPosition.y += 1;
        GameObject bulletObject = Instantiate(this.transform.GetChild(0).gameObject, bulletSpawnPosition, new Quaternion());

        Vector2 force = TargetDirection() * bulletForce * Time.deltaTime;
        bulletObject.GetComponent<Rigidbody2D>().AddForce(force, ForceMode2D.Impulse);

        StartCoroutine(ShootRoutine(bulletObject));

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
            shootCooldownCount++;
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

    private void CheckShoot()
    {
        hitPlayer = Physics2D.Raycast(this.GetComponent<Transform>().position, (target.GetComponent<Transform>().position - this.GetComponent<Transform>().position).normalized, shootDistance, playerCollisionMask);
        hitWall = Physics2D.Raycast(this.GetComponent<Transform>().position, (target.GetComponent<Transform>().position - this.GetComponent<Transform>().position).normalized, shootDistance, wallCollisionMask);


        if (hitPlayer && !hitWall)
        {
            canShoot = true;
        }

        else if (hitPlayer && hitWall)
        {
            float distancePlayerHit = ((Vector2)(hitWall.transform.position - this.transform.position)).magnitude;
            float distanceWallHit = ((Vector2)(hitWall.transform.position - this.transform.position)).magnitude;
            if (distancePlayerHit > distanceWallHit)
            {
                canShoot = false;
            }
            else if (distancePlayerHit < distanceWallHit)
            {
                canShoot = true;
            }
        }
        else
        {
            canShoot = false;
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

    IEnumerator ShootRoutine(GameObject bulletObject)
    {
        yield return new WaitForSeconds(1f);
        // Add somrthing here
        isShooting = false;
        yield return new WaitForSeconds(5f);
        Destroy(bulletObject);
    }

}