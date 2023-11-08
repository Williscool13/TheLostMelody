using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlowerScript : MonoBehaviour, IInteractible
{
    [SerializeField] int flowerIndex;

    [SerializeField] GameObject arrow;

    [SerializeField] Dialogue[] dialogues;
    [SerializeField] Dialogue[] postInteractionDialogues;
    // Start is called before the first frame update

    [SerializeField] AudioSource mainSource;
    [SerializeField] AudioClip[] clips;

    public bool Interactable { get; set; }

    float highlightTimestamp;

    void Start()
    {
        // if ability unlocked, set sprite to be interactable 
        interacted = GameDataManager.Instance.SceneZeroData.flowerInteracted[flowerIndex];
        int mementoIndex = 0;
        foreach (bool b in GameDataManager.Instance.PlayerAbilityData.abilities) {
            if (b) {
                mementoIndex += 1;
            }
        }
        if (mementoIndex >= flowerIndex + 1) {
            this.Interactable = true;
        } else {
            Destroy(this.gameObject);
        }
    }


    bool interacted;
    // Update is called once per frame
    void Update()
    {
        if (highlightTimestamp > Time.time) {
            arrow.SetActive(true);
            //spriteRenderer.color = Color.red;
        }
        else {
            arrow.SetActive(false);
            //spriteRenderer.color = Color.white;
        }
    }

    public void Interact() {
        if (!RaycastEngine.Instance.InputValid_DialogueManager) { return; }
        int number = 0;
        if (clips.Length > 1) {
            number = UnityEngine.Random.Range(0, clips.Length - 1);
        }

        if (!interacted) {
            GameDialogueManager.Instance.StartDialogue(new DialoguePayload(dialogues));
            //Interactable = false;
            GameDataManager.Instance.SceneZeroData.flowerInteracted[flowerIndex] = true;
            interacted = true;
        } else {
            GameDialogueManager.Instance.StartDialogue(new DialoguePayload(postInteractionDialogues));
        }
        mainSource.Stop();
        mainSource.clip = clips[number];
        mainSource.Play();
    }


    public void Highlight(bool value) {
        // show that I'm the currently interactible target
        if (!RaycastEngine.Instance.InputValid_DialogueManager) { return; }
        if (value) {
            highlightTimestamp = Time.time + Time.fixedDeltaTime; //highlightTime;
        }
    }

}
