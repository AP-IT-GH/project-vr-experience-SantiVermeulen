using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class TrackCheckpoints : MonoBehaviour
{
    // Sleep al je checkpoint objecten hierin in de Unity Editor, in de juiste volgorde.
    [SerializeField] private List<Checkpoint> checkpointList;

    // De dictionary die de voortgang van elke auto bijhoudt.
    private Dictionary<Transform, CheckpointData> carCheckpointDataDict = new Dictionary<Transform, CheckpointData>();

    // Een interne class om de data per auto op te slaan.
    private class CheckpointData
    {
        public int lap = 0;
        public int nextCheckpointIndex = 0;
        public float distanceToNextCheckpoint = 0f;
    }

    // Gebruik Start() om zeker te weten dat alle objecten zijn geladen voordat we ze zoeken.
    private void Start()
    {
        // Koppel de checkpoints aan deze manager.
        foreach (Checkpoint checkpoint in checkpointList)
        {
            checkpoint.SetTrackManager(this);
        }

        // Ga actief op zoek naar alle auto's met de "Car" tag en registreer ze.
        GameObject[] cars = GameObject.FindGameObjectsWithTag("Car");
        foreach (GameObject car in cars)
        {
            RegisterCar(car.transform);
        }
    }

    // Update de afstand tot het volgende checkpoint voor een accurate ranglijst.
    private void LateUpdate()
    {
        if (carCheckpointDataDict == null) return;
        foreach (var entry in carCheckpointDataDict)
        {
            if (entry.Key != null && checkpointList != null && checkpointList.Count > 0)
            {
                int checkpointIndex = entry.Value.nextCheckpointIndex;
                if (checkpointIndex < checkpointList.Count)
                {
                    Vector3 checkpointPos = checkpointList[checkpointIndex].transform.position;
                    entry.Value.distanceToNextCheckpoint = Vector3.Distance(entry.Key.position, checkpointPos);
                }
            }
        }
    }

    // Registreert een nieuwe auto voor de race.
    public void RegisterCar(Transform carTransform)
    {
        if (!carCheckpointDataDict.ContainsKey(carTransform))
        {
            carCheckpointDataDict.Add(carTransform, new CheckpointData());
            Debug.Log($"Auto '{carTransform.name}' is geregistreerd voor de race.");
        }
    }

    // Reset de voortgang voor een specifieke auto (voor ML-Agents).
    public void ResetCheckpoints(Transform carTransform)
    {
        if (carCheckpointDataDict.ContainsKey(carTransform))
        {
            carCheckpointDataDict[carTransform].nextCheckpointIndex = 0;
            carCheckpointDataDict[carTransform].lap = 0;
        }
    }

    // Verwerkt een gepasseerd checkpoint.
    public void CarPassedCheckpoint(Checkpoint checkpoint, Transform carTransform)
    {
        if (!carCheckpointDataDict.ContainsKey(carTransform)) return;

        CheckpointData data = carCheckpointDataDict[carTransform];
        int passedCheckpointIndex = checkpointList.IndexOf(checkpoint);

        // Controleer of de auto het juiste checkpoint in de reeks passeert.
        if (passedCheckpointIndex == data.nextCheckpointIndex)
        {
            // **NIEUW: De Debug.Log voor het debuggen van checkpoints.**
            // Toont een duidelijk bericht in de Console.
            // We doen +1 omdat programmeurs tellen vanaf 0, maar mensen vanaf 1.
            Debug.Log($"{carTransform.name} heeft checkpoint {passedCheckpointIndex + 1} / {checkpointList.Count} gehaald.");

            // Update de voortgang van de auto.
            data.nextCheckpointIndex = (data.nextCheckpointIndex + 1) % checkpointList.Count;

            // Als we weer bij het begin zijn, hebben we een ronde voltooid.
            if (data.nextCheckpointIndex == 0)
            {
                data.lap++;
                // Gebruik Warning voor een opvallende gele kleur in de Console.
                Debug.LogWarning($"{carTransform.name} heeft ronde {data.lap} voltooid!");
            }

            // Geef de AI zijn beloning als het een AI is.
            if (carTransform.TryGetComponent<AgentCarController>(out AgentCarController agent))
            {
                agent.CheckpointPassed();
                if (data.lap > 0 && data.nextCheckpointIndex == 0)
                {
                    agent.AddReward(5.0f);
                }
            }
        }
    }

    // Geeft de positie van het volgende checkpoint voor een specifieke auto.
    public Vector3 GetNextCheckpointPosition(Transform carTransform)
    {
        if (carCheckpointDataDict.ContainsKey(carTransform) && checkpointList.Count > 0)
        {
            int checkpointIndex = carCheckpointDataDict[carTransform].nextCheckpointIndex;
            if (checkpointIndex < checkpointList.Count)
            {
                return checkpointList[checkpointIndex].transform.position;
            }
        }
        return transform.position; // Veilige fallback positie.
    }

    // Berekent en retourneert de huidige ranglijst.
    public List<Transform> GetRankings()
    {
        return carCheckpointDataDict.Keys
            .OrderByDescending(car => carCheckpointDataDict[car].lap)
            .ThenByDescending(car => carCheckpointDataDict[car].nextCheckpointIndex)
            .ThenBy(car => carCheckpointDataDict[car].distanceToNextCheckpoint)
            .ToList();
    }

    // Controleert of een auto een geldige ronde heeft voltooid om te finishen.
    public bool HasCompletedLap(Transform carTransform)
    {
        if (carCheckpointDataDict.ContainsKey(carTransform))
        {
            var data = carCheckpointDataDict[carTransform];
            return data.lap >= 1 && data.nextCheckpointIndex == 0;
        }
        return false;
    }
}