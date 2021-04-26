using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class MenuManager : MonoBehaviour
{
    AudioSource source;
    GameManager manager;
    public GameObject mainPanel, storePanel, playPanel, optionsPanel;
    public TMP_Text money, engine, hull, spotlight, tracker, insurance, depth, description;
    public TMP_InputField seedField;
    public int[] enginePrices, hullPrices, spotlightPrices, depthPrices;
    public int treasurePrice;
    public int insurancePrice;
    public AudioClip cantBuy, purchased;
    public Slider slider;
    void Start() {
        manager = GameManager.INSTANCE;
        source = GetComponent<AudioSource>();
        slider.value = manager.volume;
    }

    public void OpenMainMenu() {
        mainPanel.SetActive(true);
        storePanel.SetActive(false);
        playPanel.SetActive(false);
        optionsPanel.SetActive(false);
    }

    public void OpenStoreMenu() {
        mainPanel.SetActive(false);
        storePanel.SetActive(true);
        playPanel.SetActive(false);
        optionsPanel.SetActive(false);
        ReloadStore();
    }
    public void OpenPlayMenu() {
        mainPanel.SetActive(false);
        storePanel.SetActive(false);
        playPanel.SetActive(true);
        optionsPanel.SetActive(false);
    }

    public void OpenOptionsMenu() {
        mainPanel.SetActive(false);
        storePanel.SetActive(false);
        playPanel.SetActive(false);
        optionsPanel.SetActive(true);
    }

    public void StartGame() {
        if (seedField.text != "") {
            int number = 0;
            if (int.TryParse(seedField.text, out number)) {
                GameManager.INSTANCE.seed = number;
            } else {
                GameManager.INSTANCE.seed = seedField.text.GetHashCode();
            }
        } else {
            GameManager.INSTANCE.seed = (int) System.DateTime.Now.Ticks;
        }
        SceneManager.LoadScene("Diving");
    }

    public void ResetData() {
       manager.armorUpgrade = 0;
       manager.lightUpgrade = 0;
       manager.engineUpgrade = 0;
       manager.finderUpgrade = false;
       manager.insuranceUpgrade = false;
       manager.totalScore = 0;
       manager.Save(); 
    }

    public void Exit() {
        Application.Quit();
    }

    public void ReloadStore() {
        money.text = "You have $" + manager.totalScore;
        engine.text = "Engine Speed\n" + manager.engineUpgrade + "/3 - " + (manager.engineUpgrade < 3 ? "$" + enginePrices[manager.engineUpgrade] : "N/A");
        hull.text = "Hull Strength\n" + manager.armorUpgrade + "/3 - " + (manager.armorUpgrade < 3 ? "$" + hullPrices[manager.armorUpgrade] : "N/A");
        spotlight.text = "Improved Spotlight\n" + manager.lightUpgrade + "/3 - " + (manager.lightUpgrade < 3 ? "$" + spotlightPrices[manager.lightUpgrade] : "N/A");
        tracker.text = "Treasure Tracker\n" + (manager.finderUpgrade ? 1 : 0) + "/1 - " + (!manager.finderUpgrade ? "$" + treasurePrice : "N/A");
        depth.text = "Pressure Resistant Glass\n" + manager.depthUpgrade + "/5 - " +(manager.depthUpgrade < 5 ? "$" + depthPrices[manager.depthUpgrade] : "N/A");
        insurance.text = "Insurance\n" + (manager.insuranceUpgrade ? 1 : 0) + "/1 - " + (!manager.insuranceUpgrade ? "$" + insurancePrice : "N/A");    
    }

    public void PurchaseInsurance() {
        if (manager.insuranceUpgrade || manager.totalScore < insurancePrice) {
            source.PlayOneShot(cantBuy);
            return;
        }
        manager.totalScore -= insurancePrice;
        manager.insuranceUpgrade = true;
        source.PlayOneShot(purchased);
        ReloadStore();
    }

    public void PurchaseTreasureTracker() {
        if (manager.finderUpgrade || manager.totalScore < treasurePrice) {
            source.PlayOneShot(cantBuy);
            return;
        }
        manager.totalScore -= treasurePrice;
        manager.finderUpgrade = true;
        source.PlayOneShot(purchased);
        ReloadStore();
    }

    public void PurchaseImprovedSpotlight() {
        int i = manager.lightUpgrade;
        if (i >= 3 || manager.totalScore < spotlightPrices[i]) {
            source.PlayOneShot(cantBuy);
            return;
        }
        manager.totalScore -= spotlightPrices[i];
        manager.lightUpgrade++;
        source.PlayOneShot(purchased);
        ReloadStore();
    }
    
    public void PurchaseHullStrength() {
        int i = manager.armorUpgrade;
        if (i >= 3 || manager.totalScore < hullPrices[i]) {
            source.PlayOneShot(cantBuy);
            return;
        }
        manager.totalScore -= hullPrices[i];
        manager.armorUpgrade++;
        source.PlayOneShot(purchased);
        ReloadStore();
    }

    public void PurchaseEngineHorsePower() {
        int i = manager.engineUpgrade;
        if (i >= 3 || manager.totalScore < enginePrices[i]) {
            source.PlayOneShot(cantBuy);
            return;
        }
        manager.totalScore -= enginePrices[i];
        manager.engineUpgrade++;
        source.PlayOneShot(purchased);
        ReloadStore();
    }

    public void PurchaseDepth() {
        int i = manager.depthUpgrade;
        if (i >= 3 || manager.totalScore < depthPrices[i]) {
            source.PlayOneShot(cantBuy);
            return;
        }
        manager.totalScore -= depthPrices[i];
        manager.depthUpgrade++;
        source.PlayOneShot(purchased);
        ReloadStore();
    }

    public void SetDescription(string txt) {
        description.text = txt;
    }
}
