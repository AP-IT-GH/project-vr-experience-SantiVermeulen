using UnityEngine;
using UnityEngine.SceneManagement; // Nodig voor het wisselen van scenes.

public class FinishLine : MonoBehaviour
{
    // Sleep je TrackManager object hierin.
    [SerializeField] private TrackCheckpoints trackCheckpoints;
    // Vul de naam van je hoofdmenu-scene in de Editor in.
    [SerializeField] private string mainMenuSceneName = "MainMenu";

    private bool raceFinished = false;

    private void OnTriggerEnter(Collider other)
    {
        if (raceFinished) return;

        if (other.CompareTag("Car"))
        {
            // Controleer of de auto een geldige ronde heeft gereden.
            if (trackCheckpoints.HasCompletedLap(other.transform))
            {
                // Controleer of het de speler is die finisht.
                if (other.GetComponent<CarController>() != null)
                {
                    HandleFinish(other.transform, true); // laad hoofdmenu
                }
                else
                {
                    HandleFinish(other.transform, false); // laad geen scene
                }
            }
        }
    }

    private void HandleFinish(Transform winner, bool loadMainMenu)
    {
        raceFinished = true;
        Debug.Log($"RACE IS VOORBIJ! Winnaar is: {winner.name}");

        FindFirstObjectByType<ScoreboardManager>()?.ShowFinalRankings();

        if (loadMainMenu)
        {
            Invoke(nameof(LoadMainMenuScene), 3f); // Wacht 3 seconden.
        }
        else
        {
            Time.timeScale = 0f; // Bevries spel als de AI wint.
        }
    }

    private void LoadMainMenuScene()
    {
        SceneManager.LoadScene(mainMenuSceneName);
    }
}