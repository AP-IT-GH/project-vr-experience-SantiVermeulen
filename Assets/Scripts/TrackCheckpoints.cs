using System.Collections.Generic;
using UnityEngine;

public class TrackCheckpoints : MonoBehaviour
{
    [SerializeField] private List<Checkpoint> checkpointList;
    private Dictionary<AgentCarController, int> nextCheckpointIndexDict;

    private void Awake()
    {
        nextCheckpointIndexDict = new Dictionary<AgentCarController, int>();
        foreach (Checkpoint checkpoint in checkpointList)
        {
            checkpoint.SetTrackManager(this);
        }
    }

    public void ResetCheckpoints(AgentCarController agent)
    {
        nextCheckpointIndexDict[agent] = 0;
    }

    public Vector3 GetNextCheckpointPosition()
    {
        // Voor nu gaan we ervan uit dat er maar één agent is
        return checkpointList[nextCheckpointIndexDict[FindFirstObjectByType<AgentCarController>()]].transform.position;
    }

    public void AgentPassedCheckpoint(Checkpoint checkpoint, AgentCarController agent)
    {
        int nextCheckpointIndex = nextCheckpointIndexDict[agent];

        // Controleer of dit het juiste checkpoint is
        if (checkpointList.IndexOf(checkpoint) == nextCheckpointIndex)
        {
            agent.CheckpointPassed();
            nextCheckpointIndexDict[agent] = (nextCheckpointIndex + 1) % checkpointList.Count;

            // Als we alle checkpoints hebben gehad, hebben we een ronde voltooid
            if (nextCheckpointIndexDict[agent] == 0)
            {
                Debug.Log("Ronde voltooid!");
                // Geef een extra grote beloning voor het voltooien van een ronde
                agent.AddReward(5.0f);
                agent.EndEpisode(); // Start een nieuwe episode
            }
        }
    }
}