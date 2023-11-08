using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Controller for the main spirit companion you meet after you ascend every time
/// </summary>
public class SpiritController : MonoBehaviour, IInteractible {
    public bool Interactable { get; set; }
    [SerializeField] GameObject arrow;

    [SerializeField] Dialogue[] firstDialogue;
    [SerializeField] Dialogue[] staminaDepleteDialogue;
    [SerializeField] Dialogue[] mementoOneDialogue;
    [SerializeField] Dialogue[] mementoTwoDialogue;
    [SerializeField] Dialogue[] mementoThreeDialogue;
    [SerializeField] Dialogue[] mementoFourDialogue;
    [SerializeField] Dialogue[] mementoFiveDialogue;

    float highlightTimestamp;

    public void Interact() {

        if (GameDataManager.Instance.PreviousDeathData.CauseOfDeath == "none") {
            if (!GameDataManager.Instance.SceneZeroData.spiritFirstInteracted) {
                GameDialogueManager.Instance.StartDialogue(new DialoguePayload(firstDialogue));
                GameDataManager.Instance.SceneZeroData.spiritFirstInteracted = true;
            }
        } 
        else if (GameDataManager.Instance.PreviousDeathData.CauseOfDeath == "stamina") {
            GameDialogueManager.Instance.StartDialogue(new DialoguePayload(staminaDepleteDialogue));
        }
        else if (GameDataManager.Instance.PreviousDeathData.CauseOfDeath == "memento") {
            int mementoIndex = 0;
            foreach (bool b in  GameDataManager.Instance.PlayerAbilityData.abilities) {
                if (b) {
                    mementoIndex += 1;
                }
            }
            switch (mementoIndex) {
                case 0:
                    Debug.LogError("Should not be possible");
                    break;
               case 1:
                    GameDialogueManager.Instance.StartDialogue(new DialoguePayload(mementoOneDialogue));
                    break;
               case 2:
                    GameDialogueManager.Instance.StartDialogue(new DialoguePayload(mementoTwoDialogue));
                    break;
               case 3:
                    GameDialogueManager.Instance.StartDialogue(new DialoguePayload(mementoThreeDialogue));
                    break;
               case 4:
                    GameDialogueManager.Instance.StartDialogue(new DialoguePayload(mementoFourDialogue));
                    break;
               case 5:
                    GameDialogueManager.Instance.StartDialogue(new DialoguePayload(mementoFiveDialogue));
                    break;
               

                
            }
        }
        else {
            // different response based on last death (timeout vs memento)
        }


        
        // speak to the player
    }


    public void Highlight(bool value) {
        // show that I'm the currently interactible target
        if (!RaycastEngine.Instance.InputValid_DialogueManager) { return; }
        if (GameDataManager.Instance.PreviousDeathData.CauseOfDeath == "none") {
            if (GameDataManager.Instance.SceneZeroData.spiritFirstInteracted) {
                return;
            }
        }

        if (value) {
            highlightTimestamp = Time.time + Time.fixedDeltaTime; //highlightTime;
        }
    }

    private void Awake() {
        Interactable = true;
    }

    void Update()
    {
        if (highlightTimestamp > Time.time) {
            arrow.SetActive(true);
        }
        else {
            arrow.SetActive(false);
        }
    }
}
