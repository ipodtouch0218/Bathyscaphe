using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

public class GameManager : MonoBehaviour
{
    public static GameManager INSTANCE;
    public AudioMixer mixer;
    public int totalScore;
    public int engineUpgrade, armorUpgrade, lightUpgrade, depthUpgrade;
    public bool insuranceUpgrade, finderUpgrade;
    public int seed;
    public float volume;

    void Awake() {
        if (INSTANCE != null) {
            Destroy(gameObject);
            return;
        }
        INSTANCE = this;
        DontDestroyOnLoad(gameObject);

        engineUpgrade = PlayerPrefs.GetInt("engine", 0);
        armorUpgrade = PlayerPrefs.GetInt("armor", 0);
        lightUpgrade = PlayerPrefs.GetInt("light", 0);
        depthUpgrade = PlayerPrefs.GetInt("depth", 0);
        insuranceUpgrade = PlayerPrefs.GetInt("insurance", 0) == 1;
        finderUpgrade = PlayerPrefs.GetInt("finder", 0) == 1;
        totalScore = PlayerPrefs.GetInt("money", 0);
        volume = PlayerPrefs.GetFloat("volume", 1);
    }

    public void SetAudio(float vol) {
        volume = vol;
        mixer.SetFloat("Volume", Mathf.Log10(volume) * 20f);
    }

    void OnApplicationQuit() {
        Save();
    }

    public void Save() {
        PlayerPrefs.SetInt("engine", engineUpgrade);
        PlayerPrefs.SetInt("armor", armorUpgrade);
        PlayerPrefs.SetInt("light", lightUpgrade);
        PlayerPrefs.SetInt("depth", depthUpgrade);
        PlayerPrefs.SetInt("insurance", (insuranceUpgrade ? 1 : 0));
        PlayerPrefs.SetInt("finder", (finderUpgrade ? 1 : 0));
        PlayerPrefs.SetInt("money", totalScore);
        PlayerPrefs.SetFloat("volume", volume);
        PlayerPrefs.Save();
    }
}
