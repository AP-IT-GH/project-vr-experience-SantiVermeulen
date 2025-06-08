using UnityEngine;

public class Checkpoint : MonoBehaviour
{
    private TrackCheckpoints trackCheckpoints;

    public void SetTrackManager(TrackCheckpoints manager)
    {
        this.trackCheckpoints = manager;
    }

    private void OnTriggerEnter(Collider other)
    {
        // Controleer of het een AI-auto is
        if (other.TryGetComponent<AgentCarController>(out AgentCarController agentCar))
        {
            // Laat de manager weten dat we een checkpoint hebben geraakt
            trackCheckpoints.AgentPassedCheckpoint(this, agentCar);
        }
    }
}