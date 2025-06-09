using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using System.Linq;

public class FinishLine : MonoBehaviour
{
    [SerializeField] private TrackCheckpoints trackCheckpoints;
    [SerializeField] private int Level2 = 2;

    private bool raceFinished = false;

    private void OnTriggerEnter(Collider other)
    {
        if (raceFinished) return;

        if (other.CompareTag("Car"))
        {
            if (trackCheckpoints.HasCompletedLap(other.transform))
            {
                raceFinished = true;

                // Haal de definitieve ranglijst op van de TrackCheckpoints manager.
                List<Transform> finalRanksTransforms = trackCheckpoints.GetRankings();

                // Converteer de lijst van Transforms naar een lijst van strings (de namen van de auto's).
                List<string> finalRankNames = finalRanksTransforms.Select(t => t.name).ToList();

                // Sla de data op in onze statische dataklas.
                RaceResultData.FinalRankings = finalRankNames;

                Debug.Log($"Race voltooid! Winnaar: {finalRankNames[0]}. Laad eindscherm...");

                // Laad de eindscherm-scene.
                SceneManager.LoadScene(Level2);
            }
        }
    }
}