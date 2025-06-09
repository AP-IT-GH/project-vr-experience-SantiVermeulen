using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Actuators;

public class AgentCarController : Agent
{
    // Deze variabele bepaalt of de AI beslissingen mag nemen.
    public bool canControl = false;

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
    [SerializeField] private TrackCheckpoints trackCheckpoints;
    private Vector3 startPosition;
    private Quaternion startRotation;

    // === Timeout & Breadcrumb Logic ===
    [Header("Training Helpers")]
    [Tooltip("Max tijd (in sec) tussen checkpoints voordat de episode reset.")]
    [SerializeField] private float timeBetweenCheckpointsTimeout = 20f;
    private float timeSinceLastCheckpoint;
    private float lastDistanceToCheckpoint;

    // === Variabelen voor Slimme Observaties ===
    private float lastSteerAction;
    private float lastMoveAction;

    public override void Initialize()
    {
        rb = GetComponent<Rigidbody>();
        startPosition = transform.position;
        startRotation = transform.rotation;
        Time.timeScale = 1.0f;
    }

    public override void OnEpisodeBegin()
    {
        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
        transform.position = startPosition;
        transform.rotation = startRotation;

        if (trackCheckpoints != null)
        {
            trackCheckpoints.ResetCheckpoints(this.transform);
            lastDistanceToCheckpoint = Vector3.Distance(transform.position, trackCheckpoints.GetNextCheckpointPosition(this.transform));
        }
        timeSinceLastCheckpoint = 0f;
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        Vector3 dirToNextCheckpoint = transform.InverseTransformPoint(trackCheckpoints.GetNextCheckpointPosition(this.transform)).normalized;
        sensor.AddObservation(dirToNextCheckpoint.x);
        sensor.AddObservation(dirToNextCheckpoint.z);

        Vector3 localVelocity = transform.InverseTransformDirection(rb.linearVelocity);
        sensor.AddObservation(localVelocity.x / 30f);
        sensor.AddObservation(localVelocity.z / 30f);
        sensor.AddObservation(rb.angularVelocity.y / 2f);
        sensor.AddObservation(lastSteerAction);
        sensor.AddObservation(lastMoveAction);
    }

    public override void OnActionReceived(ActionBuffers actions)
    {
        // Als de AI geen controle heeft, doe niets.
        if (!canControl)
        {
            // Zorg ervoor dat de AI auto stopt.
            HandleMotor(0);
            HandleSteering(0);
            return;
        }

        float steerAction = actions.ContinuousActions[0];
        float moveAction = actions.ContinuousActions[1];

        HandleSteering(steerAction);
        HandleMotor(moveAction);

        this.lastSteerAction = steerAction;
        this.lastMoveAction = moveAction;

        AddReward(-0.001f);

        float currentDistance = Vector3.Distance(transform.position, trackCheckpoints.GetNextCheckpointPosition(this.transform));
        if (currentDistance < lastDistanceToCheckpoint)
        {
            AddReward(0.01f);
        }
        lastDistanceToCheckpoint = currentDistance;
    }

    private void FixedUpdate()
    {
        timeSinceLastCheckpoint += Time.fixedDeltaTime;
        if (timeSinceLastCheckpoint > timeBetweenCheckpointsTimeout)
        {
            AddReward(-0.5f);
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
            AddReward(-1.0f);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("FallDetector"))
        {
            AddReward(-1.0f);
            EndEpisode();
        }
    }

    public void CheckpointPassed()
    {
        AddReward(1.0f);
        timeSinceLastCheckpoint = 0f;
    }

    public override void Heuristic(in ActionBuffers actionsOut)
    {
        var continuousActions = actionsOut.ContinuousActions;
        continuousActions[0] = Input.GetAxis("Horizontal");
        continuousActions[1] = Input.GetAxis("Vertical");
    }
}