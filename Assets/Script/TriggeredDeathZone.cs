using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TriggeredDeathZone : MonoBehaviour
{
    [SerializeField] GameObject targetDeathzone;

    private void Start() {
        targetDeathzone.SetActive(false);
    }
    bool triggered = false;
    private void OnTriggerStay2D(Collider2D col) {
        if (!triggered) {
            if (col.CompareTag("Player")) {
                triggered = true;
                targetDeathzone.SetActive(true);
            }
        }



    }
}
