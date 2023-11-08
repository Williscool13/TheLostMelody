using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameSceneManager : MonoBehaviour
{
    [SerializeField] Image fadeImage;
    [SerializeField] float fadeMultiplier;

    [SerializeField] int respawnScene;
    [SerializeField] float deathSceneChangeDelay;
    [SerializeField] float despawnSceneChangeDelay;
    [SerializeField] float respawnControlDelay;

    public SceneTransitionInformation _sceneTransitionInformation;
    public SceneTransitionInformation SceneTransitionInformation { get { return _sceneTransitionInformation; } set { _sceneTransitionInformation = value; } }




    public static GameSceneManager Instance { get; private set; }

    IEnumerator currentCoroutine;


    private void Awake() {
        if (GameSceneManager.Instance != null) {
            Debug.LogWarning("2 data managers in scene, deleting new one");
            Destroy(this.gameObject);
        }
        else {
            GameSceneManager.Instance = this;
        }

        DontDestroyOnLoad(this.gameObject);
        currentCoroutine = FirstLoad();
        StartCoroutine(currentCoroutine);

    }

    float resetCount;
    float resetTotal = 1.0f;
    private void Update() {
        if (Input.GetKey(KeyCode.R)) {
            resetCount += Time.deltaTime;
            if (resetCount >= resetTotal) {
                if (RaycastEngine.Instance.InputValid_DialogueManager && RaycastEngine.Instance.InputValid_SceneManager && RaycastEngine.Instance.InputValid_StaminaManager) {
                    RaycastEngine.Instance.GetComponent<PlayerStaminaSystem>().DepleteStamina("stamina");
                    resetCount = 0;
                }
            }
        } else {
            resetCount = 0;
        }
    }
    public void ChangeScene(int id) {
        if (currentCoroutine != null) { return; }
        StartCoroutine(LoadScene(id));
    }
    public void FinalChangeScene(int id) {
        if (currentCoroutine != null) { return; }
        StartCoroutine(FinalLoadScene(id));
    }

    public void DeathChangeScene() {
        if (currentCoroutine != null) { return; }
        StartCoroutine(DeathLoad());
    }

    public void ReplaceChangeScene(int index, Vector2 position, bool flip, int staminaPenalty) {
        if (currentCoroutine != null) { return; }
        StartCoroutine(ReappearLoad(index, position, flip, staminaPenalty));
    }

    void AllowPlayerInput_SceneManager(bool val) {
        RaycastEngine.Instance.InputValid_SceneManager = val;
    }

    IEnumerator LoadScene(int sceneId) {
        AllowPlayerInput_SceneManager(false);
        // fade to black
        float alpha = 0;
        while (alpha < 1) {
            alpha += Time.deltaTime * fadeMultiplier;
            if (alpha > 1) {
                alpha = 1;
            }
            fadeImage.color = new Color(0, 0, 0, alpha);
            yield return null;
        }

        AsyncOperation asyncLoad = UnityEngine.SceneManagement.SceneManager.LoadSceneAsync(sceneId);

        while (!asyncLoad.isDone) {
            yield return null;
        }

        Debug.Log($"positions are {SceneTransitionInformation.PlayerPosition.x} {SceneTransitionInformation.PlayerPosition.y}");
        RaycastEngine.Instance.transform.position = SceneTransitionInformation.PlayerPosition;
        RaycastEngine.Instance.SetRendererXFlip(SceneTransitionInformation.PlayerFacingLeft);
        bool playerReenabled = false;
        alpha = 1.5f;
        // fade from black
        while (alpha > 0) {
            alpha -= Time.deltaTime * fadeMultiplier;
            if (alpha < 0) {
                alpha = 0;
            }
            if (alpha < 0.2f && !playerReenabled) {
                AllowPlayerInput_SceneManager(true);
                playerReenabled = true;
                currentCoroutine = null;
            }
            fadeImage.color = new Color(0, 0, 0, Mathf.Min(alpha, 1));

            yield return null;
        }
        currentCoroutine = null;
    }

    IEnumerator FinalLoadScene(int sceneId) {
        AsyncOperation asyncLoad = UnityEngine.SceneManagement.SceneManager.LoadSceneAsync(sceneId);

        while (!asyncLoad.isDone) {
            yield return null;
        }

        Debug.Log($"positions are {SceneTransitionInformation.PlayerPosition.x} {SceneTransitionInformation.PlayerPosition.y}");
        RaycastEngine.Instance.transform.position = SceneTransitionInformation.PlayerPosition;
        RaycastEngine.Instance.SetRendererXFlip(SceneTransitionInformation.PlayerFacingLeft);

        currentCoroutine = null;
    }
    IEnumerator FirstLoad() {
        yield return null;
        StartMainMusic();
        float alpha = 1.5f;
        bool playerReenabled = false;
        // fade from black
        while (alpha > 0) {
            alpha -= Time.deltaTime * fadeMultiplier;
            if (alpha < 0) {
                alpha = 0;
            }
            if (alpha < 0.2f && !playerReenabled) {
                AllowPlayerInput_SceneManager(true);
                playerReenabled = true;
                currentCoroutine = null;
            }
            fadeImage.color = new Color(0, 0, 0, alpha);
            yield return null;
        }
        currentCoroutine = null;
    }
    IEnumerator DeathLoad() {
        AllowPlayerInput_SceneManager(false);

        yield return new WaitForSeconds(deathSceneChangeDelay);

        // fade to black
        AudioSystem.Instance.MusicFadeToQuiet(2.0f);

        float alpha = 0;
        while (alpha < 1) {
            alpha += Time.deltaTime * fadeMultiplier;
            if (alpha > 1) {
                alpha = 1;
            }
            fadeImage.color = new Color(0, 0, 0, alpha);
            yield return null;
        }

        AsyncOperation asyncLoad = UnityEngine.SceneManagement.SceneManager.LoadSceneAsync(respawnScene);
        while (!asyncLoad.isDone) {
            yield return null;
        }

        PlayerPlatformSystem.Instance.ResetPlatformPosition();

        //Debug.Log($"positions are {SceneTransitionInformation.PlayerPosition.x} {SceneTransitionInformation.PlayerPosition.y}");
        RaycastEngine.Instance.transform.position = new Vector2(-18.0f, 6.2f);
        RaycastEngine.Instance.SetRendererXFlip(false);

        StartMainMusic();
        alpha = 1.5f;
        bool respawnAnimPlayed = false;
        // fade from black
        while (alpha > 0) {
            alpha -= Time.deltaTime * fadeMultiplier;
            if (alpha < 0) {
                alpha = 0;
            }
            if (alpha < 0.2f && !respawnAnimPlayed) {
                RaycastEngine.Instance.Respawn();
                respawnAnimPlayed = true;
            }
            fadeImage.color = new Color(0, 0, 0, Mathf.Min(alpha, 1));
            yield return null;
        }
        yield return new WaitForSeconds(respawnControlDelay);
        AllowPlayerInput_SceneManager(true);

        currentCoroutine = null;
    }
    IEnumerator ReappearLoad(int sceneId, Vector2 positionInScene, bool flip, int staminaPenalty) {
        AllowPlayerInput_SceneManager(false);

        yield return new WaitForSeconds(despawnSceneChangeDelay);

        float alpha = 0;
        while (alpha < 1) {
            alpha += Time.deltaTime * fadeMultiplier;
            if (alpha > 1) {
                alpha = 1;
            }
            fadeImage.color = new Color(0, 0, 0, alpha);
            yield return null;
        }

        AsyncOperation asyncLoad = UnityEngine.SceneManagement.SceneManager.LoadSceneAsync(sceneId);
        while (!asyncLoad.isDone) {
            yield return null;
        }
        RaycastEngine.Instance.transform.position = positionInScene;
        RaycastEngine.Instance.SetRendererXFlip(flip);
        RaycastEngine.Instance.Reappear(staminaPenalty);

        alpha = 1.5f;
        bool respawnAnimPlayed = false;
        // fade from black
        while (alpha > 0) {
            alpha -= Time.deltaTime * fadeMultiplier;
            if (alpha < 0) {
                alpha = 0;
            }/*
            if (alpha < 0.2f && !respawnAnimPlayed) {
                RaycastEngine.Instance.Respawn();
                respawnAnimPlayed = true;
            }*/
            fadeImage.color = new Color(0, 0, 0, Mathf.Min(alpha, 1));
            yield return null;
        }
        yield return new WaitForSeconds(respawnControlDelay);
        AllowPlayerInput_SceneManager(true);
    }


    public void FinalFadeActivate() {
        if (currentCoroutine != null) { return; }
        StartCoroutine(FinalFade());
    }

    [SerializeField] Image finalFadeImage;
    IEnumerator FinalFade() {
        finalFadeImage.gameObject.SetActive(true);
        float alpha = 0;
        while (alpha < 1) {
            alpha += Time.fixedDeltaTime * 0.5f;
            if (alpha > 1) {
                alpha = 1;
            }
            finalFadeImage.color = new Color(1, 1, 1, alpha);
            yield return new WaitForSeconds(Time.fixedDeltaTime);
        }

        GameDialogueManager.Instance.StartFinalDialogue(new DialoguePayload(new Dialogue[] { 
            new Dialogue("", "I've finally found... My voice...", 0.03f, DialogueType.clear), 
            new Dialogue("", "This realm...", 0.03f, DialogueType.clear),
            new Dialogue("", "Let the harmony of music guide you in your path",0.03f, DialogueType.clear),
            new Dialogue("", "Farewell, dear creatures, and may you never again...", 0.03f, DialogueType.clear),
            new Dialogue("", "lose your melody", 0.06f, DialogueType.append),
        }));
    }
    void StartMainMusic() {
        // enable base music (if any)

        if (GameDataManager.Instance.PlayerAbilityData.abilities[0]) {
            // play 0 music
            AudioSystem.Instance.PlayMainMusic(0);
        }
        if (GameDataManager.Instance.PlayerAbilityData.abilities[1]) {
            // play 1 music
            AudioSystem.Instance.PlayMainMusic(1);
        }
        if (GameDataManager.Instance.PlayerAbilityData.abilities[2]) {
            // play 2 music
            AudioSystem.Instance.PlayMainMusic(2);
        }
        if (GameDataManager.Instance.PlayerAbilityData.abilities[3]) {
            // play 3 music
            AudioSystem.Instance.PlayMainMusic(3);
        }
        if (GameDataManager.Instance.PlayerAbilityData.abilities[4]) {
            // play 4 music
            AudioSystem.Instance.PlayMainMusic(4);
        }

    }
}


public class SceneTransitionInformation {
    public Vector2 PlayerPosition { get; private set; }
    public bool PlayerFacingLeft { get; private set; }
    public SceneTransitionInformation(Vector2 playerPosition, bool faceLeft) {
        this.PlayerPosition = playerPosition;
        this.PlayerFacingLeft = faceLeft;
    }
}