using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ComplexPlatformMover : MonoBehaviour
{
    /// <summary>
    /// 0 is base position
    /// </summary>
    [SerializeField] Vector2[] coordinates;
    [SerializeField] float[] speed;

    int previousIndex;
    int currentIndex;
    // Start is called before the first frame update
    void Start()
    {
        previousIndex = 0;
        currentIndex = 1;
        reached = false;
        if (coordinates.Length < 2) {
            Debug.LogError("need more than 1 coordinate");
            return;
        }
        currBasePos = coordinates[0];
        currTargetPos = coordinates[1];
        Vector2 rawDirection = currTargetPos - currBasePos;
        direction = new Vector2(GetSign(rawDirection.x), GetSign(rawDirection.y));

    }

    Vector2 currBasePos;
    Vector2 currTargetPos;
    Vector2 direction;
    bool reached;

    private int GetSign(float v) {
        if (Mathf.Approximately(v, 0)) {
            return 0;
        }
        else if (v > 0) {
            return 1;
        }
        else {
            return -1;
        }
    }

    private void FixedUpdate() {
        if (!reached) {
            Vector2 candidateTargetPosition = (Vector2)transform.position + direction * speed[previousIndex] * Time.deltaTime;

            float candidateTargetDistanceFromBase = Vector2.Distance(candidateTargetPosition, currBasePos);
            float finalTargetDistanceFromBase = Vector2.Distance(currTargetPos, currBasePos);

            Vector2 targetMovement;

            if (candidateTargetDistanceFromBase > finalTargetDistanceFromBase) {
                reached = true;
                targetMovement = currTargetPos - (Vector2)transform.position;
            } else {
                targetMovement = candidateTargetPosition - (Vector2)transform.position;
            }
            transform.Translate(targetMovement);

        } else {

            previousIndex = currentIndex;
            currentIndex += 1;
            
            if (currentIndex == coordinates.Length) {
                currentIndex = 0;
            }
            currBasePos = coordinates[previousIndex];
            currTargetPos = coordinates[currentIndex];
            Vector2 rawDirection = currTargetPos - currBasePos;
            direction = new Vector2(GetSign(rawDirection.x), GetSign(rawDirection.y));
            reached = false;;
        }
    }
}
