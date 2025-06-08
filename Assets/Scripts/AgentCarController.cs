using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Actuators;

public class AgentCarController : Agent
{
    // === Auto Fysica Referenties ===
    [Header("Car Physics")]
    [SerializeField] private WheelCollider frontLeftWheelCollider;
    [SerializeField] private WheelCollider frontRightWheelCollider;
    [SerializeField] private WheelCollider rearLeftWheelCollider;
    [SerializeField] private WheelCollider rearRightWheelCollider;
    [SerializeField] private float motorForce = 1500f;
    [SerializeField] private float maxSteerAngle = 30f;
    private Rigidbody rb;

    // === Referenties voor Leren ===
    [Header("Learning & Tracking")]
    [SerializeField] private TrackCheckpoints trackCheckpoints; // De manager van de baan
    private Vector3 startPosition;
    private Quaternion startRotation;

    // === NIEUW: Timeout Logic ===
    [Header("Timeout Logic")]
    [Tooltip("Max tijd (in sec) tussen checkpoints voordat de episode reset.")]
    [SerializeField] private float timeBetweenCheckpointsTimeout = 20f;
    private float timeSinceLastCheckpoint;

    public override void Initialize()
    {
        rb = GetComponent<Rigidbody>();
        startPosition = transform.position;
        startRotation = transform.rotation;
    }

    public override void OnEpisodeBegin()
    {
        // Reset de auto
        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
        transform.position = startPosition;
        transform.rotation = startRotation;

        // Reset de checkpoints en de timer
        trackCheckpoints.ResetCheckpoints(this);
        timeSinceLastCheckpoint = 0f;
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        // Richting naar het volgende checkpoint (2 waarden)
        Vector3 dirToNextCheckpoint = transform.InverseTransformPoint(trackCheckpoints.GetNextCheckpointPosition()).normalized;
        sensor.AddObservation(dirToNextCheckpoint.x);
        sensor.AddObservation(dirToNextCheckpoint.z);

        // Snelheid van de auto (1 waarde)
        sensor.AddObservation(rb.linearVelocity.magnitude / 30f);
    }

    public override void OnActionReceived(ActionBuffers actions)
    {
        // Actie 0: Sturen (-1 tot +1)
        float steerAction = actions.ContinuousActions[0];
        // Actie 1: Gas/Rem (-1 tot +1). Negatief is achteruit.
        float moveAction = actions.ContinuousActions[1];

        HandleSteering(steerAction);
        HandleMotor(moveAction);

        // Kleine straf voor tijd die voorbij gaat, motiveert snelheid.
        AddReward(-0.001f);
    }

    private void FixedUpdate()
    {
        // === NIEUW: Timeout Check ===
        // Tel de tijd sinds het laatste checkpoint.
        timeSinceLastCheckpoint += Time.fixedDeltaTime;
        // Als het te lang duurt, is de agent vastgelopen. Reset.
        if (timeSinceLastCheckpoint > timeBetweenCheckpointsTimeout)
        {
            Debug.Log("Timeout! Te langzaam. Episode reset.");
            AddReward(-0.5f); // Straf voor vastzitten.
            EndEpisode();
        }
    }

    private void HandleMotor(float moveInput)
    {
        frontLeftWheelCollider.motorTorque = moveInput * motorForce;
        frontRightWheelCollider.motorTorque = moveInput * motorForce;
    }

    private void HandleSteering(float steerInput)
    {
        float currentSteerAngle = maxSteerAngle * steerInput;
        frontLeftWheelCollider.steerAngle = currentSteerAngle;
        frontRightWheelCollider.steerAngle = currentSteerAngle;
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Wall"))
        {
            AddReward(-0.5f);
            Debug.Log("Muur geraakt! -0.5 Reward");
            // We beëindigen de episode niet direct, zodat de agent kan leren herstellen.
        }
    }

    // === NIEUW: Fall Detector ===
    private void OnTriggerEnter(Collider other)
    {
        // Controleer of we het object met de tag "FallDetector" raken.
        if (other.CompareTag("FallDetector"))
        {
            Debug.Log("Van de baan gevallen! -1.0 Reward & Reset.");
            AddReward(-1.0f); // Grote straf voor vallen.
            EndEpisode();     // Essentieel: reset de episode onmiddellijk.
        }
    }

    public void CheckpointPassed()
    {
        // Beloon de agent en reset de timeout timer!
        AddReward(1.0f);
        timeSinceLastCheckpoint = 0f; // Heel belangrijk!
        Debug.Log("Checkpoint gehaald! +1.0 Reward. Timer gereset.");
    }

    public override void Heuristic(in ActionBuffers actionsOut)
    {
        var continuousActions = actionsOut.ContinuousActions;
        continuousActions[0] = Input.GetAxis("Horizontal");
        continuousActions[1] = Input.GetAxis("Vertical");
    }
}