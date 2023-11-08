using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RaycastCheckTouch
{
    public Vector2 raycastDirection;
    private Vector2[] offsetPoints;
    private LayerMask layerMask;
    private float raycastLength;


    public RaycastCheckTouch(Vector2 start, Vector2 end, Vector2 dir, LayerMask mask, Vector2 parallelInset, Vector2 perpendicularInset, float checkLength) {
        this.raycastDirection = dir;
        this.offsetPoints = new Vector2[] {
            start + parallelInset + perpendicularInset,
            end - parallelInset + perpendicularInset,
        };
        this.layerMask = mask;
        this.raycastLength = perpendicularInset.magnitude + checkLength; ;
    }


    public Collider2D DoRaycast(Vector2 origin) {
        foreach (Vector2 offset in offsetPoints) {
            RaycastHit2D hit = Raycast(origin + offset, raycastDirection, raycastLength, layerMask);
            if (hit.collider != null) {
                return hit.collider;
            }
        }
        return null;
    }


    private RaycastHit2D Raycast(Vector2 start, Vector2 direction, float length, LayerMask layerMask) {
        Debug.DrawLine(start, start + direction * length, Color.red);
        return Physics2D.Raycast(start, direction, length, layerMask);
    }

}
