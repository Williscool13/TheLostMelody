using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeathZone : MonoBehaviour
{
    [SerializeField] int sceneIndex;
    [SerializeField] Vector2 positionInScene;
    [SerializeField] bool flip;
    [SerializeField] int staminaPenalty;
    bool resetted = false;
    private void OnTriggerStay2D(Collider2D collision) {
        // kill and return to set position
        if (resetted) { return; }
        if (!RaycastEngine.Instance.InputValid_SceneManager) { return; }
        if (collision.CompareTag("Player")) {
            RaycastEngine re = collision.GetComponent<RaycastEngine>();
            re.Disappear();
            GameSceneManager.Instance.ReplaceChangeScene(sceneIndex, positionInScene, flip, staminaPenalty);
            resetted = true;
        }
    }
}
