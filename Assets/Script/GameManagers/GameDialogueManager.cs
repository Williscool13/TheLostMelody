using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using TMPro;
public class GameDialogueManager : MonoBehaviour
{
    public static GameDialogueManager Instance { get; private set; }

    private void Awake() {
        if (GameDialogueManager.Instance != null) {
            Debug.LogWarning("2 data managers in scene, deleting new one");
            Destroy(this.gameObject);
        }
        else {
            GameDialogueManager.Instance = this;
        }
    }
    // Start is called before the first frame update
    void Start()
    {
        AllowPlayerInput_DialogueManager(true);

        nameArea.text = "";
        textArea.text = "";
        dialogueParent.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetButtonDown("Up")) {
            dialogueInputTimestamp = Time.time + 0.1f;
        }
        //dialogueInput = Input.GetButtonDown("Up");
    }

    float dialogueInputTimestamp;

    void AllowPlayerInput_DialogueManager(bool val) {
        RaycastEngine.Instance.InputValid_DialogueManager = val;
    }


    IEnumerator currentDialoguePayload;
    IEnumerator currentAnimation;
    [SerializeField] float cooldown;
    float cooldownTimestamp;
    public bool StartDialogue(DialoguePayload payload) {
        if (currentDialoguePayload != null || cooldownTimestamp > Time.time) {
            return false;
        } else {
            currentDialoguePayload = Dialogue(payload);
            StartCoroutine(currentDialoguePayload);
            // do dialogue
            return true;
        }
    }

    public bool StartFinalDialogue(DialoguePayload payload) {
        if (currentDialoguePayload != null || cooldownTimestamp > Time.time) {
            return false;
        }
        else {
            currentDialoguePayload = FinalDialogue(payload);
            StartCoroutine(currentDialoguePayload);
            // do dialogue
            return true;
        }
    }


    IEnumerator Dialogue(DialoguePayload payload) {
        AllowPlayerInput_DialogueManager(false);
        Queue<Dialogue> payloadQueue = new();
        foreach (Dialogue d in payload.Texts) {
            payloadQueue.Enqueue(d);
        }

        // First text line
        if (payloadQueue.Count > 0) {
            yield return null;
            dialogueInputTimestamp = 0;
        }
        while (payloadQueue.Count > 0) {
            string baseText = textArea.text;
            // enable textbox/background
            Dialogue currentTarget = payloadQueue.Dequeue();

            currentAnimation = AnimateText(currentTarget);
            dialogueParent.SetActive(true);
            nameArea.text = currentTarget.dialogueName;

            StartCoroutine(currentAnimation);
            while (currentAnimation != null) {
                if (dialogueInputTimestamp > Time.time) {
                    // end immediately
                    StopCoroutine(currentAnimation);
                    currentAnimation = null;

                    // set text to final state
                    if (currentTarget.dialogueType == DialogueType.append) {
                        textArea.text = baseText + currentTarget.text;
                    } else {
                        textArea.text = currentTarget.text;
                    }

                    dialogueInputTimestamp = 0;
                } else {
                    Debug.Log("waiting for text to finish");
                    yield return new WaitForSeconds(Time.fixedDeltaTime);
                }
            }

            while (dialogueInputTimestamp < Time.time) {
                Debug.Log("waiting for user input to continue to next text");
                yield return new WaitForSeconds(Time.fixedDeltaTime);
            }
            dialogueInputTimestamp = 0;

            //yield return new WaitForSeconds(Time.fixedDeltaTime);

        }

        // disable textbox/background
        //dialogueInputTimestamp = 0;
        dialogueParent.SetActive(false);
        textArea.text = "";
        nameArea.text = "";
        currentDialoguePayload = null;
        AllowPlayerInput_DialogueManager(true);
        cooldownTimestamp = Time.time + cooldown;
        yield return new WaitForSeconds(Time.fixedDeltaTime);
    }

    IEnumerator FinalDialogue(DialoguePayload payload) {
        AllowPlayerInput_DialogueManager(false);
        Queue<Dialogue> payloadQueue = new();
        foreach (Dialogue d in payload.Texts) {
            payloadQueue.Enqueue(d);
        }

        // First text line
        if (payloadQueue.Count > 0) {
            yield return null;
            dialogueInputTimestamp = 0;
        }
        while (payloadQueue.Count > 0) {
            string baseText = textArea.text;
            // enable textbox/background
            Dialogue currentTarget = payloadQueue.Dequeue();

            currentAnimation = AnimateText(currentTarget);
            dialogueParent.SetActive(true);
            nameArea.text = currentTarget.dialogueName;

            StartCoroutine(currentAnimation);
            while (currentAnimation != null) {
                if (dialogueInputTimestamp > Time.time) {
                    // end immediately
                    StopCoroutine(currentAnimation);
                    currentAnimation = null;

                    // set text to final state
                    if (currentTarget.dialogueType == DialogueType.append) {
                        textArea.text = baseText + currentTarget.text;
                    }
                    else {
                        textArea.text = currentTarget.text;
                    }

                    dialogueInputTimestamp = 0;
                }
                else {
                    Debug.Log("waiting for text to finish");
                    yield return new WaitForSeconds(Time.fixedDeltaTime);
                }
            }

            while (dialogueInputTimestamp < Time.time) {
                Debug.Log("waiting for user input to continue to next text");
                yield return new WaitForSeconds(Time.fixedDeltaTime);
            }
            dialogueInputTimestamp = 0;

            //yield return new WaitForSeconds(Time.fixedDeltaTime);

        }

        // disable textbox/background
        //dialogueInputTimestamp = 0;
        dialogueParent.SetActive(false);
        textArea.text = "";
        nameArea.text = "";
        currentDialoguePayload = null;
        cooldownTimestamp = Time.time + cooldown;

        // fade out audio
        AudioSystem.Instance.MusicFadeToQuiet(4.0f);
        yield return new WaitForSeconds(2.0f);

        GameSceneManager.Instance._sceneTransitionInformation = new SceneTransitionInformation(new Vector2(0,0), false);
        GameSceneManager.Instance.FinalChangeScene(10);

        yield return new WaitForSeconds(Time.fixedDeltaTime);
    }
    [SerializeField] TextMeshProUGUI textArea;
    [SerializeField] GameObject dialogueParent;
    [SerializeField] TextMeshProUGUI nameArea;
    IEnumerator AnimateText(Dialogue dialogue) {
        string baseText = textArea.text;
        for (int i = 0; i < dialogue.text.Length + 1; i++) {
            if (dialogue.dialogueType == DialogueType.append) { 
                textArea.text = baseText + dialogue.text.Substring(0, i);
            } else {
                textArea.text = dialogue.text.Substring(0, i);
            }
            yield return new WaitForSeconds(dialogue.speed);
        }

        currentAnimation = null;
    }
    void AppendText() {

    }

}

public enum DialogueType {
    clear=0,
    append=1,
}

[System.Serializable]
public class Dialogue {
    public string text;
    public float speed;
    public string dialogueName;
    public DialogueType dialogueType;
    public Dialogue(string name, string text, float speed, DialogueType dialogueType) {
        this.text = text;
        this.speed = speed;
        this.dialogueType = dialogueType;
        this.dialogueName = name;
    }
}

public struct DialoguePayload {

    public Dialogue[] Texts { get; private set; }

    public DialoguePayload(Dialogue[] texts) {
        this.Texts = texts;
    }
}