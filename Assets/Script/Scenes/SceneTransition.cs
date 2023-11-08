using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SceneTransition : MonoBehaviour
{

    [SerializeField] Vector2 destinationVector;
    [SerializeField] int destinationSceneId;
    [SerializeField] bool faceLeft;
    [SerializeField] int mementoCountThreshold;
    [SerializeField] Collider2D col;
    bool transitioning = false;

    void MainTransition() {
        Debug.Log("SCENE TRANSITION ACTIVATING, moving to scene: " + destinationSceneId);
        GameSceneManager.Instance._sceneTransitionInformation = new SceneTransitionInformation(destinationVector, faceLeft);
        GameSceneManager.Instance.ChangeScene(destinationSceneId);
    }

    private void OnTriggerStay2D(Collider2D collision) {
        if (collision.CompareTag("Player")) {
            if (!transitioning) {
                MainTransition();
                transitioning = true;
            }

        }
    }


    private void Start() {
        int mementoIndex = 0;
        foreach (bool b in GameDataManager.Instance.PlayerAbilityData.abilities) {
            if (b) {
                mementoIndex += 1;
            }
        }
        if (mementoIndex >= mementoCountThreshold) {
            this.gameObject.SetActive(false);
            //col.enabled = false;
        }
    }

}
