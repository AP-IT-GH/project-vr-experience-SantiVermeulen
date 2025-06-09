using UnityEngine;
using UnityEngine.SceneManagement; // **STAP 1: DEZE REGEL IS ESSENTIEEL**

public class FinishLine : MonoBehaviour
{
    // Sleep je TrackManager object hierin in de Unity Editor.
    [SerializeField] private TrackCheckpoints trackCheckpoints;

    // **NIEUW:** Vul de naam van je hoofdmenu-scene in de Editor in.
    [SerializeField] private string mainMenuSceneName = "MainMenu";

    private bool raceFinished = false;

    private void OnTriggerEnter(Collider other)
    {
        // Negeer alles als de race al voorbij is.
        if (raceFinished) return;

        // Controleer of het een object met de "Car" tag is.
        if (other.CompareTag("Car"))
        {
            // Vraag de manager of deze auto een geldige ronde heeft gereden.
            if (trackCheckpoints.HasCompletedLap(other.transform))
            {
                // Controleer of het de SPELER is die finisht (heeft hij een CarController script?).
                if (other.GetComponent<CarController>() != null)
                {
                    // De speler heeft gewonnen, start de procedure om naar het menu te gaan.
                    HandleFinish(other.transform, true);
                    Debug.Log($"RACE IS VOORBIJ! Winnaar is: Speler");
                }
                else
                {
                    // Een andere auto (de AI) heeft gewonnen, bevries alleen het spel.
                    HandleFinish(other.transform, false);
                }
            }
        }
    }

    // Een nieuwe, nette functie om de finish-logica af te handelen.
    private void HandleFinish(Transform winner, bool loadMainMenu)
    {
        raceFinished = true;
        Debug.Log($"RACE IS VOORBIJ! Winnaar is: {winner.name}");

        // Update het scorebord een laatste keer.
        FindFirstObjectByType<ScoreboardManager>()?.ShowFinalRankings();

        // Als we de scene moeten laden (omdat de speler won).
        if (loadMainMenu)
        {
            // Wacht 3 seconden voordat je de scene laadt.
            // Dit geeft de speler de tijd om de eindstand te zien.
            Invoke(nameof(LoadMainMenuScene), 3f);
        }
        else
        {
            // Als de AI wint, bevries het spel zodat de speler kan zien dat hij verloren heeft.
            Time.timeScale = 0f;
        }
    }

    // **STAP 2: DE FUNCTIE DIE DE SCENE DAADWERKELIJK LAADT**
    private void LoadMainMenuScene()
    {
        // Zorg ervoor dat de timescale weer normaal is voordat je een nieuwe scene laadt.
        Time.timeScale = 1f;
        SceneManager.LoadScene(0);
    }
}