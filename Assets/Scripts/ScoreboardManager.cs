using UnityEngine;
using TMPro;
using System.Collections.Generic;

public class ScoreboardManager : MonoBehaviour
{
    // Sleep je TrackManager en UI Text-objecten hierin.
    [SerializeField] private TrackCheckpoints trackCheckpoints;
    [SerializeField] private TextMeshProUGUI firstPlaceText;
    [SerializeField] private TextMeshProUGUI secondPlaceText;

    private void Update()
    {
        UpdateScoreboard();
    }

    void UpdateScoreboard()
    {
        if (trackCheckpoints == null) return;
        List<Transform> rankings = trackCheckpoints.GetRankings();

        if (rankings.Count > 0)
            firstPlaceText.text = "1. " + rankings[0].name;
        else
            firstPlaceText.text = "1. ...";

        if (rankings.Count > 1)
            secondPlaceText.text = "2. " + rankings[1].name;
        else
            secondPlaceText.text = "2. ...";
    }

    public void ShowFinalRankings()
    {
        UpdateScoreboard();
        Debug.Log("Eindstand wordt getoond.");
    }
}