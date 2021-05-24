using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(RaycastController))]
public class Controller2D : RaycastController
{
    [SerializeField] public LayerMask levelMask;
    [SerializeField] public LayerMask platformMask;
    [SerializeField] private float maxSlopeAngle = 60f;
    public CollisionInfo collision;

    public override void Start() {
        base.Start();
        collision.faceDir = 1;
    }
 
    public void Move(Vector3 velocity) {
        UpdateRaycastOrigin();

        collision.reset();
        collision.prevVelocity = velocity;

        if(velocity.y < 0) 
            DescendSlope(ref velocity);
        if(velocity.x != 0)
            collision.faceDir = (int)Mathf.Sign(velocity.x);
            
        HorizontalCollision(ref velocity);
        if(velocity.y != 0)
            VerticalCollision(ref velocity);

        transform.Translate(velocity);
    }

    void HorizontalCollision(ref Vector3 velocity) {
        float directionX = collision.faceDir;
        float rayLength = Mathf.Abs(velocity.x) + skinWidth;

        for(int i = 0 ; i < horizontalRayCount ; ++i) {
            Vector2 rayOrigin = (directionX == -1) ? raycastOriginPos.bottomLeft : raycastOriginPos.bottomRight;
            rayOrigin += Vector2.up * (horizontalRaySpacing * i);
            //Debug.DrawRay(rayOrigin, Vector2.right * directionX * rayLength, Color.red);
            RaycastHit2D hit = Physics2D.Raycast(rayOrigin, Vector2.right * directionX, rayLength, levelMask);

            if(hit) {
                float slopeAngle = Vector2.Angle(hit.normal, Vector2.up);

                if(i == 0 && slopeAngle <= maxSlopeAngle) {
                    if(collision.descendingSlope) {
                        collision.descendingSlope = false;
                        velocity = collision.prevVelocity;
                    }
                    float distanceToSlopeStart = 0;
                    if(slopeAngle != collision.prevSlopeAngle) {
                        distanceToSlopeStart = hit.distance - skinWidth;
                        velocity.x -= distanceToSlopeStart * directionX;
                    }
                    AscendSlope(ref velocity, slopeAngle);
                    velocity.x += distanceToSlopeStart * directionX;
                }
                
                if(!collision.ascendingSlope || slopeAngle > maxSlopeAngle) {
                    velocity.x = Mathf.Min(Mathf.Abs(velocity.x), (hit.distance - skinWidth)) * directionX;
                    rayLength = Mathf.Min(Mathf.Abs(velocity.x) + skinWidth, hit.distance);

                    if(collision.ascendingSlope) {
                        velocity.y = Mathf.Tan(collision.slopeAngle * Mathf.Deg2Rad) * velocity.x;
                    }

                    collision.left = directionX == -1;
                    collision.right = directionX == 1;
                }
            }
        }
    }

    void VerticalCollision(ref Vector3 velocity) {
        float directionY = Mathf.Sign(velocity.y);
        float rayLength = Mathf.Abs(velocity.y) + skinWidth;

        for(int i = 0 ; i < verticalRayCount ; ++i) {
            Vector2 rayOrigin = (directionY == -1) ? raycastOriginPos.bottomLeft : raycastOriginPos.topLeft;
            rayOrigin += Vector2.right * (verticalRaySpacing * i + velocity.x);
            Debug.DrawRay(rayOrigin, Vector2.up * directionY * rayLength, Color.red);
            RaycastHit2D hit = Physics2D.Raycast(rayOrigin, Vector2.up * directionY, rayLength, levelMask);
            RaycastHit2D hitPlatform = Physics2D.Raycast(rayOrigin, Vector2.up * directionY, rayLength, platformMask);

            if(hit) {
                velocity.y = (hit.distance - skinWidth) * directionY;
                rayLength = hit.distance;

                if(collision.ascendingSlope) {
                    velocity.x = velocity.y / Mathf.Tan(collision.slopeAngle * Mathf.Deg2Rad) * Mathf.Sign(velocity.x);
                }

                collision.below = directionY == -1;
                collision.above = directionY == 1;
            }
            if(hitPlatform) {
                if(directionY == -1 && !collision.droppingDown) 
                    velocity.y = (hitPlatform.distance - skinWidth) * directionY;
                rayLength = hitPlatform.distance;
            
                if(collision.ascendingSlope) {
                    velocity.x = velocity.y / Mathf.Tan(collision.slopeAngle * Mathf.Deg2Rad) * Mathf.Sign(velocity.x);
                }

                collision.below = (directionY == -1 && !collision.droppingDown);
                collision.above = false;
            }
            else {
                collision.droppingDown = false;
            }
        }

        if(collision.ascendingSlope) {
            float directionX = Mathf.Sign(velocity.x);
            rayLength = Mathf.Abs(velocity.x) + skinWidth;
            Vector2 rayOrigin = ((directionX == -1) ? raycastOriginPos.bottomLeft : raycastOriginPos.bottomRight) + Vector2.up * velocity.y;
            RaycastHit2D hit = Physics2D.Raycast(rayOrigin, Vector2.right * directionX, rayLength, levelMask);

            if(hit) {
                float slopeAngle = Vector2.Angle(hit.normal, Vector2.up);
                if(slopeAngle != collision.slopeAngle) {
                    velocity.x = Mathf.Min(Mathf.Abs(velocity.x), (hit.distance - skinWidth)) * directionX;
                    collision.slopeAngle = slopeAngle;
                }
            }

        }
    }

    void AscendSlope(ref Vector3 velocity, float slopeAngle) {
        float slopeMoveDistance = Mathf.Abs(velocity.x);
        float ascendVelocityY = Mathf.Sin(slopeAngle * Mathf.Deg2Rad) * slopeMoveDistance;
        
        if(velocity.y <= ascendVelocityY) {
            velocity.y = ascendVelocityY;
            velocity.x = Mathf.Cos(slopeAngle * Mathf.Deg2Rad) * slopeMoveDistance * Mathf.Sign(velocity.x);
            
            collision.below = true;
            collision.ascendingSlope = true;
            collision.slopeAngle = slopeAngle;
        }
    }

    void DescendSlope(ref Vector3 velocity) {
        RaycastHit2D maxSlopeHitLeft = Physics2D.Raycast(raycastOriginPos.bottomLeft, Vector2.down, Mathf.Abs(velocity.y) + skinWidth, levelMask);
        RaycastHit2D maxSlopeHitRight = Physics2D.Raycast(raycastOriginPos.bottomRight, Vector2.down, Mathf.Abs(velocity.y) + skinWidth, levelMask);
        SlideDownMaxSlope(maxSlopeHitLeft, ref velocity);
        SlideDownMaxSlope(maxSlopeHitRight, ref velocity);

        if(!collision.slidingDownMaxSlope) {
            float directionX = Mathf.Sign(velocity.x);
            Vector2 rayOrigin = (directionX == -1) ? raycastOriginPos.bottomRight : raycastOriginPos.bottomLeft;
            RaycastHit2D hit = Physics2D.Raycast(rayOrigin, -Vector2.up, Mathf.Infinity, levelMask);

            if(hit) {
                float slopeAngle = Vector2.Angle(hit.normal, Vector2.up);
                if(slopeAngle != 0 && slopeAngle <= maxSlopeAngle) {
                    if(Mathf.Sign(hit.normal.x) == directionX) {
                        if(hit.distance - skinWidth <= Mathf.Tan(slopeAngle * Mathf.Deg2Rad) * Mathf.Abs(velocity.x)) {
                            float slopeMoveDistance = Mathf.Abs(velocity.x);
                            float descendVelocityY = Mathf.Sin(slopeAngle * Mathf.Deg2Rad) * slopeMoveDistance;
                            velocity.x = Mathf.Cos(slopeAngle * Mathf.Deg2Rad) * slopeMoveDistance * Mathf.Sign(velocity.x);
                            velocity.y -= descendVelocityY;

                            collision.below = true;
                            collision.descendingSlope = true;
                            collision.slopeAngle = slopeAngle;
                        }
                    }
                }
            }
        }
    }

    void SlideDownMaxSlope(RaycastHit2D hit, ref Vector3 velocity) {
        if(hit) {
            float slopeAngle = Vector2.Angle(hit.normal, Vector2.up);
            if(slopeAngle > maxSlopeAngle) {
                velocity.x = hit.normal.x * ((Mathf.Abs(velocity.y) - hit.distance) / Mathf.Tan(slopeAngle * Mathf.Deg2Rad));
                collision.slopeAngle = slopeAngle;
                collision.slidingDownMaxSlope = true;
            }
        }
    }

    public struct CollisionInfo {
        public bool above, below;
        public bool left, right;

        public bool ascendingSlope, descendingSlope;
        public bool slidingDownMaxSlope;
        public bool rolling, droppingDown, crouching;

        public float slopeAngle, prevSlopeAngle;

        public Vector3 prevVelocity;
        public int faceDir;

        public void reset() {
            above = below = false;
            left = right = false;
            ascendingSlope = descendingSlope = false;
            slidingDownMaxSlope = false;
            prevSlopeAngle = slopeAngle;
            slopeAngle = 0f;
        }
    }
}
