using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;

public class PlayerAgent : Agent
{
    [Header("Réglages Physiques")]
    public float speed = 10.4f;
    public float jumpForce = 10f;
    public Transform startPoint;
    private float cameraOffsetX = 5f;
    private float cameraOffsetY = 2f;
    private float cameraOffsetZ = -10f;

    [Header("Reinforcement Settings")]
    //public float survivalRewardRate = 0.5f; // reward par seconde
    public float jumpPenalty = -0.2f; // pénalité par saut

    private Rigidbody2D rb;
    private bool isGrounded;

    private int startTime;
    private int maxEpisodeLength = 6000; // Limite de frames par épisode

    public override void Initialize()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.freezeRotation = true;
    }

    public override void OnEpisodeBegin()
    {
        transform.position = startPoint.position;
        rb.velocity = Vector2.zero;
        isGrounded = true;
        startTime = Time.frameCount;
    }

    public override void OnActionReceived(ActionBuffers actions)
    {
        updateCameraPosition();

        // Reward proportionnel au temps de survie
        //AddReward(survivalRewardRate * Time.fixedDeltaTime);

        // Limite de durée d’épisode
        if (Time.frameCount - startTime > maxEpisodeLength)
        {
            EndEpisode();
            return;
        }

        // Mouvement horizontal constant
        rb.velocity = new Vector2(speed, rb.velocity.y);

        // Décision de saut
        if (actions.DiscreteActions[0] == 1 && isGrounded)
        {
            AddReward(jumpPenalty); // pénalité pour éviter les sauts inutiles
            Jump();
        }
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        sensor.AddObservation(rb.velocity.y);
        sensor.AddObservation(isGrounded ? 1f : 0f);
    }

    void Jump()
    {
        rb.velocity = new Vector2(rb.velocity.x, jumpForce);
        isGrounded = false;
    }

    void updateCameraPosition()
    {
        Camera mainCamera = Camera.main;
        if (mainCamera != null)
        {
            Vector3 cameraPosition = startPoint.position;
            cameraPosition.x = transform.position.x + cameraOffsetX;
            cameraPosition.y = cameraPosition.y + cameraOffsetY;
            cameraPosition.z = cameraPosition.z + cameraOffsetZ;
            mainCamera.transform.position = cameraPosition;
        }
    }

    public override void Heuristic(in ActionBuffers actionsOut)
    {
        var discreteActions = actionsOut.DiscreteActions;
        discreteActions[0] = Input.GetKey(KeyCode.Space) ? 1 : 0;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        Rigidbody2D rb = GetComponent<Rigidbody2D>();

        if (collision.collider.CompareTag("Ground"))
        {
            float normalY = collision.GetContact(0).normal.y;

            if (normalY > 0.1f)
            {
                isGrounded = true;
            }
            else
            {
                AddReward(-3.0f);
                EndEpisode();
            }
        }

        if (collision.collider.CompareTag("Obstacle"))
        {
            AddReward(-3.0f);
            EndEpisode();
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Finish"))
        {
            AddReward(10.0f);
            Debug.Log("Succes !");
            EndEpisode();
        }

        if (other.CompareTag("Checkpoint"))
        {
            CheckpointBehavior checkpointBehavior = other.GetComponent<CheckpointBehavior>();
            if (!checkpointBehavior)
            {
                Debug.LogError("Checkpoint sans CheckpointBehavior detecte ! PAS NORMAL");
                return;
            }

            if (checkpointBehavior.IsActivated())
            {
                return;
            }

            AddReward(1.5f);
        }
    }
}