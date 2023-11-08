using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RaycastMoveDirection 
{

    public Vector2 raycastDirection;
    private Vector2[] offsetPoints;
    private LayerMask layerMask;
    private float addLength;

    public RaycastMoveDirection(Vector2 start, Vector2 end, Vector2 dir, LayerMask mask, Vector2 parallelInset, Vector2 perpendicularInset) {
        this.raycastDirection = dir;
        Vector2.Lerp(start, end, 0.5f);
        if (start.y == end.y) {
            Vector2 startPos = start + parallelInset + perpendicularInset;
            Vector2 endPos = end - parallelInset + perpendicularInset;
            this.offsetPoints = new Vector2[] {
            startPos,
            Vector2.Lerp(startPos, endPos, 0.25f),
            Vector2.Lerp(startPos, endPos, 0.5f),
            Vector2.Lerp(startPos, endPos, 0.75f),
            endPos,
            };
        } else {
            Vector2 startPos = start + parallelInset + perpendicularInset;
            Vector2 endPos = end - parallelInset + perpendicularInset;
            this.offsetPoints = new Vector2[] {
            startPos,
            Vector2.Lerp(startPos, endPos, 0.25f),
            Vector2.Lerp(startPos, endPos, 0.5f),
            Vector2.Lerp(startPos, endPos, 0.75f),
            endPos,
            };
        }

        this.addLength = perpendicularInset.magnitude;
        this.layerMask = mask;
    }

    public float DoRaycast(Vector2 origin, float distance, bool droppingDown) {
        float minDistance = distance;
        foreach (Vector2 offset in offsetPoints) {
            RaycastHit2D hit = Raycast(origin + offset, raycastDirection, distance + addLength, layerMask);
            if (hit.collider != null) {
                bool passThroughUp = (hit.collider.tag == "Platform_MoveThrough" || hit.collider.tag == "Platform_DropDown") && raycastDirection == Vector2.up;
                bool passThroughDown = false;
                if (droppingDown) {
                    passThroughDown = (hit.collider.tag == "Platform_DropDown") && raycastDirection == Vector2.down;
                }
                if (!(passThroughUp || passThroughDown)){
                    minDistance = Mathf.Min(minDistance, hit.distance - addLength);
                }
            }
        }
        return minDistance;
    }



    private RaycastHit2D Raycast(Vector2 start, Vector2 direction, float length, LayerMask layerMask) {
        Debug.DrawLine(start, start + direction * length, Color.blue);
        return Physics2D.Raycast(start, direction, length, layerMask); 
    }

}
