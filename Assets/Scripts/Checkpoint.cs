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
        // Meldt aan de manager welke auto door dit checkpoint is gereden.
        if (other.CompareTag("Car"))
        {
            trackCheckpoints.CarPassedCheckpoint(this, other.transform);
        }
    }
}