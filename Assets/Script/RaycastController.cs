using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(BoxCollider2D))]
public class RaycastController : MonoBehaviour
{
    [Header("Collision Detection")]
    public int horizontalRayCount = 4;
    public int verticalRayCount = 4;
    public float skinWidth = .015f;

    [HideInInspector] public float horizontalRaySpacing;
    [HideInInspector] public float verticalRaySpacing;
    
    [HideInInspector] public BoxCollider2D _collider;
    [HideInInspector] public RaycastOrigin raycastOriginPos;

    public virtual void Start() {
        _collider = GetComponent<BoxCollider2D>();
        CalculateRaySpacing();
    }

    public void UpdateRaycastOrigin() {
        Bounds bound = _collider.bounds;
        bound.Expand(skinWidth * -2);

        raycastOriginPos.bottomLeft = new Vector2(bound.min.x, bound.min.y);
        raycastOriginPos.bottomRight = new Vector2(bound.max.x, bound.min.y);
        raycastOriginPos.topLeft = new Vector2(bound.min.x, bound.max.y);
        raycastOriginPos.topRight = new Vector2(bound.max.x, bound.max.y);
    }

    public void CalculateRaySpacing() {
        Bounds bound = _collider.bounds;
        bound.Expand(skinWidth * -2);

        horizontalRayCount = Mathf.Clamp(horizontalRayCount, 2, int.MaxValue);
        verticalRayCount = Mathf.Clamp(verticalRayCount, 2, int.MaxValue);

        horizontalRaySpacing = bound.size.y / (horizontalRayCount - 1);
        verticalRaySpacing = bound.size.x / (verticalRayCount - 1);
    }

    public struct RaycastOrigin {
        public Vector2 topLeft, topRight;
        public Vector2 bottomLeft, bottomRight;
    }
}
