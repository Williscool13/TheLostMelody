using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InteractTransition : MonoBehaviour, IInteractible {


    [SerializeField] GameObject arrow;
    [SerializeField] Vector2 destinationVector;
    [SerializeField] bool faceLeft;
    [SerializeField] int sceneIndex;


    [SerializeField] int mementoCountTheshold;
    public bool Interactable { get; set; }

    public void Interact() {
        Interactable = false;

        GameSceneManager.Instance._sceneTransitionInformation = new SceneTransitionInformation(destinationVector, faceLeft);
        GameSceneManager.Instance.ChangeScene(sceneIndex);
    }
    float highlightTimestamp;

    DialoguePayload payload = new DialoguePayload(new Dialogue[] { new Dialogue("Barrel", "testing...", 0.1f, DialogueType.clear), new Dialogue("Barrel", "testing2...", 0.1f, DialogueType.append), new Dialogue("Barrel", "testing3...", 0.1f, DialogueType.clear) });
    public void Highlight(bool val) {
        if (val) {
            highlightTimestamp = Time.time + Time.fixedDeltaTime; //highlightTime;
        }
        // show that I'm the currently interactible target


    }
    void Start() {
        int mementoIndex = 0;
        foreach (bool b in GameDataManager.Instance.PlayerAbilityData.abilities) {
            if (b) {
                mementoIndex += 1;
            }
        }

        if (mementoIndex >= mementoCountTheshold) {
            this.Interactable = false;
        } else {
            this.Interactable = true;
        }
    }

    // Update is called once per frame
    void Update() {
        if (highlightTimestamp > Time.time) {
            arrow.SetActive(true);
            //spriteRenderer.color = Color.red;
        }
        else {
            arrow.SetActive(false);
            //spriteRenderer.color = Color.white;
        }
    }
}
