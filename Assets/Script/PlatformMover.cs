using UnityEngine;
using System.Collections;

public class PlatformMover : MonoBehaviour {


    [SerializeField] GameObject targetObject;
    [SerializeField] float speed;

    Vector2 targetPosition;
    Vector2 basePosition;
    private float timer;
    float noise;
    void Start() {
        targetPosition = targetObject.transform.position;
        basePosition = transform.position;

        noise = Random.Range(0.0f, 1.0f);
        
    }


    private void FixedUpdate() {
        timer += Time.deltaTime;
        float offset = 1 / (speed * 2);
        float lerpValue = Mathf.Sin(speed * Mathf.PI * (timer+ noise + offset));
        lerpValue += 1;
        lerpValue /= 2;


        Vector2 current = Vector2.Lerp(basePosition, targetPosition, lerpValue);
        transform.position = current;
    }

    void OnDrawGizmos() {
        if (basePosition == null) {
            if (targetObject != null) {
                // Draws a blue line from this transform to the target
                Gizmos.color = Color.blue;
                Gizmos.DrawLine(transform.position, targetObject.transform.position);
            }
        }
        else {
            Gizmos.color = Color.blue;
            Gizmos.DrawLine(basePosition, targetPosition);
        }
        
    }
}