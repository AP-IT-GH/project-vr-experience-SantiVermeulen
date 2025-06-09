using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using System.Collections.Generic;

public class RaceUIManager : MonoBehaviour
{
    // --- Referenties voor het UI ---
    [Header("UI Panels")]
    [SerializeField] private GameObject finishPanel;
    [SerializeField] private GameObject liveScoreboardPanel;

    [Header("Finish Panel Text")]
    [SerializeField] private TextMeshProUGUI finalFirstPlaceText;
    [SerializeField] private TextMeshProUGUI finalSecondPlaceText;

    [Header("Scene Management")]
    [SerializeField] private int nextSceneName = 2;

    // --- Referenties voor de data ---
    [SerializeField] private TrackCheckpoints trackCheckpoints;

    void Start()
    {
        finishPanel.SetActive(false);
    }

    public void ShowFinishPanel()
    {
        if (liveScoreboardPanel != null)
        {
            liveScoreboardPanel.SetActive(false);
        }

        List<Transform> finalRankings = trackCheckpoints.GetRankings();

        if (finalRankings.Count > 0)
            finalFirstPlaceText.text = "1. " + finalRankings[0].name;
        else
            finalFirstPlaceText.text = "1. ???";

        if (finalRankings.Count > 1)
            finalSecondPlaceText.text = "2. " + finalRankings[1].name;
        else
            finalSecondPlaceText.text = "2. ???";

        finishPanel.SetActive(true);
    }

    // Functie voor de "Next Level" knop
    public void OnNextLevelButtonClicked()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(nextSceneName);
    }

    // --- NIEUW: Functie voor de "Reset" knop ---
    public void OnResetButtonClicked()
    {
        // Zet de tijd weer op normaal voordat je de scene herlaadt
        Time.timeScale = 1f;

        // Herlaad de HUIDIGE actieve scene
        Scene currentScene = SceneManager.GetActiveScene();
        SceneManager.LoadScene(currentScene.name);
    }
}