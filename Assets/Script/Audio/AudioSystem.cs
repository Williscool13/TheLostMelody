using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.SceneManagement;
public class AudioSystem : MonoBehaviour
{

    [SerializeField] AudioSource menuMusic, piano, glock, flute, celesta, violin, ensemble;

    [SerializeField] AudioSource jumpSource, dashSource;
    public static AudioSystem Instance { get; private set; }
    private void Awake() {
        if (AudioSystem.Instance != null) {
            Debug.LogWarning("2 instances of audio system, deleting new one");
            Destroy(this.gameObject);
        }
        else {
            AudioSystem.Instance = this;
        }

        DontDestroyOnLoad(this.gameObject);
    }

    RaycastEngine target;
    void Start()
    {
        target = RaycastEngine.Instance;
        if (SceneManager.GetActiveScene().buildIndex == 0) {
            PlayMainMusic(-1);
        }
    }

    private void FixedUpdate() {
        if (RaycastEngine.Instance != null) {
            transform.position = RaycastEngine.Instance.transform.position;
        }
    }
    public void TestSFX() {
        int randomNum = UnityEngine.Random.Range(0, 2);
        switch (randomNum) {
            case 0:
                PlayJump();
                break;
            case 1:
                PlayDash();
                break;
        }
    }

    public void PlayJump() {
        jumpSource.Stop();
        jumpSource.time = 0.2f;
        jumpSource.Play();
    }

    public void PlayDash() {
        dashSource.Stop();
        dashSource.time = 0.08f;
        dashSource.Play();
    }


    List<AudioSource> activeSources = new();
    /// <summary>
    /// 
    /// </summary>
    /// <param name="speedMultiplier">1 = 1 second fade, 2 = 0.5 second fade, 3=0.33 second fade, etc.</param>
    public void MusicFadeToQuiet(float speedMultiplier) {
        StartCoroutine(VolumeFade(speedMultiplier));
        // fade from loud to quiet
    }

    IEnumerator VolumeFade(float mult) {
        float vol = 1;
        while (vol > 0) {
            vol -= Time.fixedDeltaTime * mult;
            foreach (AudioSource aus in activeSources) {
                aus.volume = Mathf.Max(0, vol);
            }
            yield return new WaitForSeconds(Time.fixedDeltaTime);
        }
        activeSources = new();
    }

    public void PlayMainMusic(int code) {
        switch (code) {
            case -1:
                // slow piano (main menu only)
                menuMusic.volume = 1;
                menuMusic.Play();
                activeSources.Add(menuMusic);
                break;
            case 0:
                // violin
                violin.volume = 1;
                violin.Play();
                activeSources.Add(violin);
                break;
            case 1:
                // flute
                flute.volume = 1;
                flute.Play();
                activeSources.Add(flute);
                break;
            case 2:
                // celesta
                celesta.volume = 1;
                celesta.Play();
                activeSources.Add(celesta);
                break;
            case 3:
                // glock
                glock.volume = 1;
                glock.Play();
                activeSources.Add(glock);
                break;
            case 4:
                // piano
                piano.volume = 1;
                piano.Play();
                activeSources.Add(piano);
                break;
        }
        // take stored base volume

        // fade from quiet to loud

    }

    public void Ensemble() {
        float currTime = violin.time;
        menuMusic.Stop();
        violin.Stop();
        flute.Stop();
        celesta.Stop();
        glock.Stop();
        piano.Stop();

        ensemble.volume = 1;
        //ensemble.time = currTime; 
        ensemble.Play();
    }
}
