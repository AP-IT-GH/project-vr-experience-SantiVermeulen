using System.Collections.Generic;
using UnityEngine;

public class TrackCheckpoints : MonoBehaviour
{
    // Deze lijst moet in de Unity Editor worden gevuld met de checkpoints in de juiste volgorde
    [SerializeField] private List<Checkpoint> checkpointList;
    
    // Een dictionary om bij te houden welke agent bij welk checkpoint is
    private Dictionary<AgentCarController, int> nextCheckpointIndexDict;

    private void Awake()
    {
        nextCheckpointIndexDict = new Dictionary<AgentCarController, int>();
        // Zorg ervoor dat elk checkpoint-script een referentie naar deze manager heeft
        foreach (Checkpoint checkpoint in checkpointList)
        {
            checkpoint.SetTrackManager(this);
        }
    }

    // Reset de voortgang van een specifieke agent
    public void ResetCheckpoints(AgentCarController agent)
    {
        nextCheckpointIndexDict[agent] = 0;
    }

    // **AANPASSING:** Accepteert nu de agent als parameter
    public Vector3 GetNextCheckpointPosition(AgentCarController agent)
    {
        // Controleer of we deze agent wel kennen
        if (nextCheckpointIndexDict.ContainsKey(agent))
        {
            int checkpointIndex = nextCheckpointIndexDict[agent];
            return checkpointList[checkpointIndex].transform.position;
        }
        else
        {
            // Foutafhandeling: als de agent om een of andere reden niet in de lijst staat
            Debug.LogError($"Agent {agent.gameObject.name} niet gevonden in de dictionary! Kan checkpoint niet bepalen.");
            return transform.position; // Geef een veilige, neutrale positie terug
        }
    }

    // Wordt aangeroepen door een Checkpoint wanneer een agent erdoorheen rijdt
    public void AgentPassedCheckpoint(Checkpoint checkpoint, AgentCarController agent)
    {
        // Controleer of de agent wel bekend is
        if (!nextCheckpointIndexDict.ContainsKey(agent)) return;
        
        int nextCheckpointIndex = nextCheckpointIndexDict[agent];

        // Controleer of dit het juiste checkpoint in de reeks is
        if (checkpointList.IndexOf(checkpoint) == nextCheckpointIndex)
        {
            agent.CheckpointPassed(); // Geef de agent zijn beloning
            
            // Ga naar het volgende checkpoint in de lijst
            nextCheckpointIndexDict[agent] = (nextCheckpointIndex + 1) % checkpointList.Count;

            // Als we weer bij het begin zijn, hebben we een volledige ronde voltooid
            if (nextCheckpointIndexDict[agent] == 0)
            {
                Debug.Log($"Agent {agent.gameObject.name} heeft een ronde voltooid!");
                agent.AddReward(5.0f); // Geef een grote bonusbeloning voor een volledige ronde
                agent.EndEpisode();    // Start een nieuwe episode na een succesvolle ronde
            }
        }
    }
}
