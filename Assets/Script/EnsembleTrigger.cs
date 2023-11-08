using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnsembleTrigger : MonoBehaviour
{
    bool started = false;

    private void OnTriggerStay2D(Collider2D col) {
        if (started) { return; }

        if (col.CompareTag("Player")) {
            AudioSystem.Instance.Ensemble();
        }
    }
}
