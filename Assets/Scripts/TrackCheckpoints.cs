using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class TrackCheckpoints : MonoBehaviour
{
    [SerializeField] private List<Checkpoint> checkpointList;
    private Dictionary<Transform, CheckpointData> carCheckpointDataDict = new Dictionary<Transform, CheckpointData>();

    private class CheckpointData { public int lap = 0; public int nextCheckpointIndex = 0; public float distanceToNextCheckpoint = 0f; }

    private void Start()
    {
        foreach (Checkpoint checkpoint in checkpointList) { checkpoint.SetTrackManager(this); }
        GameObject[] cars = GameObject.FindGameObjectsWithTag("Car");
        foreach (GameObject car in cars) { RegisterCar(car.transform); }
    }

    private void LateUpdate()
    {
        if (carCheckpointDataDict == null) return;
        foreach (var entry in carCheckpointDataDict)
        {
            if (entry.Key != null && checkpointList != null && checkpointList.Count > 0)
            {
                int checkpointIndex = entry.Value.nextCheckpointIndex;
                Vector3 checkpointPos = checkpointList[checkpointIndex].transform.position;
                entry.Value.distanceToNextCheckpoint = Vector3.Distance(entry.Key.position, checkpointPos);
            }
        }
    }

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

        if (checkpointList.IndexOf(checkpoint) == data.nextCheckpointIndex)
        {
            data.nextCheckpointIndex = (data.nextCheckpointIndex + 1) % checkpointList.Count;

            if (data.nextCheckpointIndex == 0)
            {
                data.lap++;
                Debug.Log($"{carTransform.name} heeft ronde {data.lap} voltooid!");
            }

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
        if (carCheckpointDataDict.ContainsKey(carTransform))
        {
            int checkpointIndex = carCheckpointDataDict[carTransform].nextCheckpointIndex;
            return checkpointList[checkpointIndex].transform.position;
        }
        return transform.position;
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