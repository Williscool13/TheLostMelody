using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VictimController : MonoBehaviour, IInteractible {
    [SerializeField] GameObject arrow;
    [SerializeField] Sprite targetSprite;
    [SerializeField] SpriteRenderer renderer;
    [SerializeField] int mementoIndex;


    public bool Interactable { get; set; }

    public void Interact() {
        if (GameDataManager.Instance.PlayerAbilityData.abilities[mementoIndex]) { return; }
        GameDataManager.Instance.PlayerAbilityData.abilities[mementoIndex] = true;
        renderer.sprite = targetSprite;
        Interactable = false;

        RaycastEngine.Instance.GetComponent<PlayerStaminaSystem>().DepleteStamina("memento");
    }
    float highlightTimestamp;

    DialoguePayload payload = new DialoguePayload(new Dialogue[] { new Dialogue("Barrel", "testing...", 0.1f, DialogueType.clear), new Dialogue("Barrel", "testing2...", 0.1f, DialogueType.append), new Dialogue("Barrel", "testing3...", 0.1f, DialogueType.clear) });
    public void Highlight(bool val) {
        if (GameDataManager.Instance.PlayerAbilityData.abilities[mementoIndex]) { return; }
        if (val) {
            highlightTimestamp = Time.time + Time.fixedDeltaTime; //highlightTime;
        }
        // show that I'm the currently interactible target
    }

    // Start is called before the first frame update
    void Start()
    {
        if (GameDataManager.Instance.PlayerAbilityData.abilities[mementoIndex]) {
            Destroy(this.gameObject);
        }
        this.Interactable = true;   
    }

    // Update is called once per frame
    void Update()
    {
        if (highlightTimestamp > Time.time) {
            arrow.SetActive(true);
            //spriteRenderer.color = Color.red;
        } else {
            arrow.SetActive(false);
            //spriteRenderer.color = Color.white;
        }


        // every COOLDOWN time play a random noise
    }
}
