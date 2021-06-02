using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Controller2D : RaycastController
{
    [Header("Layer Mask")]
    [SerializeField] public LayerMask collisionMask;
    [SerializeField] public LayerMask platformMask;
    [SerializeField] public LayerMask levelMask;

    [Header("Slope Handling")]
    [SerializeField, Range(0f, 90f)] private float maxSlopeAngle = 50f;
    public CollisionInfo collision;
    public RaycastHit2D hitPlatform;

    //public RaycastHit2D maxSlopeHitLeft, maxSlopeHitRight;

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

        if(Mathf.Abs(velocity.x) < skinWidth) 
            rayLength = 2*skinWidth;

        for(int i = 0 ; i < horizontalRayCount ; ++i) {
            Vector2 rayOrigin = (directionX == -1) ? raycastOriginPos.bottomLeft : raycastOriginPos.bottomRight;
            rayOrigin += Vector2.up * (horizontalRaySpacing * i);
            RaycastHit2D hit = Physics2D.Raycast(rayOrigin, Vector2.right * directionX, rayLength, levelMask);

            Debug.DrawRay(rayOrigin, Vector2.right * directionX, Color.red);

            if(hit) {
                if(hit.distance == 0)
                    continue;

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
                    AscendSlope(ref velocity, slopeAngle, hit.normal);
                    velocity.x += distanceToSlopeStart * directionX;
                }
                
                if(!collision.ascendingSlope || slopeAngle > maxSlopeAngle) {
                    //velocity.x = (hit.distance - skinWidth) * directionX;
                    //rayLength = hit.distance;
                    velocity.x = Mathf.Min(Mathf.Abs(velocity.x), (hit.distance - skinWidth)) * directionX;
                    rayLength = Mathf.Min(Mathf.Abs(velocity.x) + skinWidth, hit.distance);

                    if(collision.ascendingSlope) {
                        velocity.y = Mathf.Tan(collision.slopeAngle * Mathf.Deg2Rad) * Mathf.Abs(velocity.x);
                    }

                    collision.left = directionX == -1;
                    collision.right = directionX == 1;
                }
            }
        }
    }

    void VerticalCollision(ref Vector3 velocity) {
        float directionX = collision.faceDir;
        float directionY = Mathf.Sign(velocity.y);
        float rayLength = Mathf.Abs(velocity.y) + skinWidth;

        for(int i = 0 ; i < verticalRayCount ; ++i) {
            Vector2 rayOrigin = (directionY == -1) ? ((directionX == -1) ? raycastOriginPos.bottomRight : raycastOriginPos.bottomLeft) : ((directionX == -1) ? raycastOriginPos.topRight : raycastOriginPos.topLeft);
            if(directionX == 1)
                rayOrigin += Vector2.right * (verticalRaySpacing * i + velocity.x);
            else
                rayOrigin -= Vector2.right * (verticalRaySpacing * i - velocity.x);
            RaycastHit2D hit = Physics2D.Raycast(rayOrigin, Vector2.up * directionY, rayLength, levelMask);
            hitPlatform = Physics2D.Raycast(rayOrigin, Vector2.up * directionY, rayLength, platformMask);

            Debug.DrawRay(rayOrigin, Vector2.up * directionY, Color.red);

            if(hit && !collision.onPlatform) {
                velocity.y = Mathf.Min(Mathf.Abs(velocity.y), (hit.distance - skinWidth)) * directionY;
                //velocity.y = (hit.distance - skinWidth) * directionY;
                rayLength = Mathf.Min(Mathf.Abs(velocity.y) + skinWidth, hit.distance);
                //rayLength = hit.distance;

                if(collision.ascendingSlope) {
                    velocity.x = velocity.y / Mathf.Tan(collision.slopeAngle * Mathf.Deg2Rad) * Mathf.Sign(velocity.x);
                }

                collision.below = directionY == -1;
                collision.above = directionY == 1;
            }
            if(hitPlatform || collision.onPlatform) {
                if(!collision.onPlatform) {
                    collision.onPlatform = true;
                    if(directionY == -1 && !collision.droppingDown) {
                        velocity.y = Mathf.Min(Mathf.Abs(velocity.y), (hitPlatform.distance - skinWidth)) * directionY;
                        collision.prevVelocity.y = velocity.y;
                    }      
                }
                else {
                    if(directionY == -1 && !collision.droppingDown) 
                        velocity.y = collision.prevVelocity.y;
                }
                    
                //velocity.y = (hitPlatform.distance - skinWidth) * directionY;
                //rayLength = hitPlatform.distance;
                rayLength = Mathf.Min(Mathf.Abs(velocity.y) + skinWidth, hitPlatform.distance);

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
            rayLength = Mathf.Abs(velocity.x) + skinWidth;
            Vector2 rayOrigin = ((directionX == -1) ? raycastOriginPos.bottomLeft : raycastOriginPos.bottomRight) + Vector2.up * velocity.y;
            RaycastHit2D hit = Physics2D.Raycast(rayOrigin, Vector2.right * directionX, rayLength, collisionMask);

            if(hit) {
                float slopeAngle = Vector2.Angle(hit.normal, Vector2.up);
                if(slopeAngle != collision.slopeAngle) {
                    //velocity.x = (hit.distance - skinWidth) * directionX;
                    velocity.x = Mathf.Min(Mathf.Abs(velocity.x), (hit.distance - skinWidth)) * directionX;
                    collision.slopeAngle = slopeAngle;
                    collision.slopeNormal = hit.normal;
                }
            }

        }
    }

    void AscendSlope(ref Vector3 velocity, float slopeAngle, Vector2 slopeNormal) {
        float slopeMoveDistance = Mathf.Abs(velocity.x);
        float ascendVelocityY = Mathf.Sin(slopeAngle * Mathf.Deg2Rad) * slopeMoveDistance;
        
        if(velocity.y <= ascendVelocityY) {
            velocity.y = ascendVelocityY;
            velocity.x = Mathf.Cos(slopeAngle * Mathf.Deg2Rad) * slopeMoveDistance * Mathf.Sign(velocity.x);
            
            collision.below = true;
            collision.ascendingSlope = true;
            collision.slopeAngle = slopeAngle;
            collision.slopeNormal = slopeNormal;
        }
    }

    void DescendSlope(ref Vector3 velocity) {
        /*maxSlopeHitLeft = Physics2D.Raycast(raycastOriginPos.bottomLeft, Vector2.down, Mathf.Abs(velocity.y) + skinWidth, levelMask);
        maxSlopeHitRight = Physics2D.Raycast(raycastOriginPos.bottomRight, Vector2.down, Mathf.Abs(velocity.y) + skinWidth, levelMask);
        if(maxSlopeHitLeft ^ maxSlopeHitRight) {
            SlideDownMaxSlope(maxSlopeHitLeft, ref velocity);
            SlideDownMaxSlope(maxSlopeHitRight, ref velocity);
        }*/

        if(!collision.slidingDownMaxSlope) {
            float directionX = collision.faceDir;
            Vector2 rayOrigin = (directionX == -1) ? raycastOriginPos.bottomRight : raycastOriginPos.bottomLeft;
            RaycastHit2D hit = Physics2D.Raycast(rayOrigin, -Vector2.up, Mathf.Infinity, collisionMask);

            if(hit) {
                float slopeAngle = Vector2.Angle(hit.normal, Vector2.up);
                if(slopeAngle > 0 && slopeAngle <= maxSlopeAngle) {
                    if(Mathf.Sign(hit.normal.x) == directionX) {
                        if(hit.distance - skinWidth <= Mathf.Tan(slopeAngle * Mathf.Deg2Rad) * Mathf.Abs(velocity.x)) {
                            float slopeMoveDistance = Mathf.Abs(velocity.x);
                            float descendVelocityY = Mathf.Sin(slopeAngle * Mathf.Deg2Rad) * slopeMoveDistance;
                            velocity.x = Mathf.Cos(slopeAngle * Mathf.Deg2Rad) * slopeMoveDistance * Mathf.Sign(velocity.x);
                            velocity.y -= descendVelocityY;

                            collision.below = true;
                            collision.descendingSlope = true;
                            collision.slopeAngle = slopeAngle;
                            collision.slopeNormal = hit.normal;
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
                velocity.x = Mathf.Sign(hit.normal.x) * (Mathf.Abs(velocity.y) - hit.distance) / Mathf.Tan(slopeAngle * Mathf.Deg2Rad);
                collision.slopeAngle = slopeAngle;
                collision.slidingDownMaxSlope = true;
                collision.slopeNormal = hit.normal;
            }
        }
    }

    public struct CollisionInfo {
        public bool above, below;
        public bool left, right;

        public bool ascendingSlope, descendingSlope;
        public bool slidingDownMaxSlope, onPlatform;
        public bool droppingDown;

        public float slopeAngle, prevSlopeAngle;
        public Vector2 slopeNormal;

        public Vector3 prevVelocity;
        public int faceDir;

        public void reset() {
            above = below = false;
            left = right = false;
            ascendingSlope = descendingSlope = false;
            slidingDownMaxSlope = false;
            onPlatform = false;
            slopeNormal = Vector2.zero;
            prevSlopeAngle = slopeAngle;
            slopeAngle = 0f;
        }
    }
}
