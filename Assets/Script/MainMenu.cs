using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Audio;

public class MainMenu : MonoBehaviour
{
    [SerializeField] Button okButton;
    [SerializeField] Button testSFXButton;
    [SerializeField] Slider master;
    [SerializeField] Slider sfx;
    [SerializeField] Slider music;

    [SerializeField] Image fade;

    [SerializeField] AudioMixer mixer;


    [SerializeField] int targetScene;

    float masterVol;
    float sfxVol;
    float musicVol;

    // Start is called before the first frame update
    void Start()
    {
        mixer.GetFloat("Master", out masterVol);
        mixer.GetFloat("SFX", out sfxVol);
        mixer.GetFloat("Music", out musicVol);


        master.value = Mathf.InverseLerp(-30.0f, -0.0f, masterVol);
        sfx.value = Mathf.InverseLerp(-30.0f, -10.0f, sfxVol);
        music.value = Mathf.InverseLerp(-30.0f, -10.0f, musicVol);

        /*mixer.SetFloat("Master", Mathf.Lerp(-20, 0, masterVol));
        mixer.SetFloat("SFX", Mathf.Lerp(-20, 0, sfxVol));
        mixer.SetFloat("Music", Mathf.Lerp(-20, 0, musicVol));*/

        // add listener to ok button   
        okButton.onClick.AddListener(OnClick_OkButton);

        testSFXButton.onClick.AddListener(OnClick_TestSFX);
    }

    // Update is called once per frame
    void Update()
    {
        mixer.SetFloat("Master", Mathf.Lerp(-30.0f, -0.0f, master.value));
        mixer.SetFloat("SFX", Mathf.Lerp(-30.0f, -10.0f, sfx.value));
        mixer.SetFloat("Music", Mathf.Lerp(-30.0f, -10.0f, music.value));
    }


    public void OnClick_TestSFX() {
        Debug.Log("testing sfx");
        AudioSystem.Instance.TestSFX();
    }

    public void OnClick_OkButton() {
        Debug.Log("OK Clicked, moving to first scene");
        StartCoroutine(LoadScene());
    }


    IEnumerator LoadScene() {
        fade.gameObject.SetActive(true);
        float alpha = 0;
        AudioSystem.Instance.MusicFadeToQuiet(1.5f);
        while (alpha < 1) {
            alpha += Time.fixedDeltaTime * 1;
            if (alpha > 1) {
                alpha = 1;
            }
            fade.color = new Color(0, 0, 0, alpha);
            yield return new WaitForSeconds(Time.fixedDeltaTime);
        }

        AsyncOperation asyncLoad = UnityEngine.SceneManagement.SceneManager.LoadSceneAsync(targetScene);

        while (!asyncLoad.isDone) {
            yield return null;
        }

    }
}
