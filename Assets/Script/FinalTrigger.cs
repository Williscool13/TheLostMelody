using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FinalTrigger : MonoBehaviour
{

    bool triggered = false;
    private void OnTriggerStay2D(Collider2D collision) {
        if (!triggered) {
            if (collision.CompareTag("Player")) {
                RaycastEngine re = collision.GetComponent<RaycastEngine>();
                re.Disappear();
                GameSceneManager.Instance.FinalFadeActivate();
                triggered = true;
            }
        }
    }
}
