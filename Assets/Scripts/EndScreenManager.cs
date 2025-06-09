using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class EndScreenManager : MonoBehaviour
{
    // --- Referenties voor de UI ---
    [Header("UI Text")]
    [SerializeField] private TextMeshProUGUI firstPlaceText;
    [SerializeField] private TextMeshProUGUI secondPlaceText;

    // --- Scene Namen ---
    [Header("Scene Management")]
    [SerializeField] private int raceSceneName = 1; // Naam van je race-scene
    [SerializeField] private int NextLevel = 2; 

    void Start()
    {
        // Haal de opgeslagen ranglijst op uit onze statische dataklas.
        if (RaceResultData.FinalRankings != null)
        {
            // Vul de tekstvelden met de ontvangen data.
            if (RaceResultData.FinalRankings.Count > 0)
                firstPlaceText.text = "1. " + RaceResultData.FinalRankings[0];
            else
                firstPlaceText.text = "1. ???";

            if (RaceResultData.FinalRankings.Count > 1)
                secondPlaceText.text = "2. " + RaceResultData.FinalRankings[1];
            else
                secondPlaceText.text = "2. ???";
        }
        else
        {
            // Fallback voor als je de scene direct start zonder data.
            firstPlaceText.text = "Geen data ontvangen.";
            secondPlaceText.text = "";
        }
    }

    // Functie voor de "Opnieuw" knop
    public void OnResetButtonClicked()
    {
        SceneManager.LoadScene(raceSceneName);
    }

    public void OnMainMenuButtonClicked()
    {
        SceneManager.LoadScene(NextLevel);
    }
}