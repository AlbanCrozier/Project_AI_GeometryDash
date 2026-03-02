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

    private Rigidbody2D rb;
    private bool isGrounded;

    private int startTime;
    private int maxEpisodeLength = 6000; // Limite de frames par épisode

    public override void Initialize()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.freezeRotation = true; // Empêche le cube de rouler
    }

    public override void OnEpisodeBegin()
    {
        // Reset position et physique
        transform.position = startPoint.position;
        rb.velocity = Vector2.zero;
        isGrounded = true;
        startTime = Time.frameCount; // Enregistre le temps de début de l'épisode
    }

    public override void OnActionReceived(ActionBuffers actions)
    {
        updateCameraPosition();

        // Check if episode is too long (par exemple, si le joueur stagne ou ne fait pas de progrès)
        if (Time.frameCount - startTime > maxEpisodeLength) // Limite de 1000 frames par épisode
        {
            SetReward(0); // Probablement une erreur du code, dans le doute on compte pas
            EndEpisode();
            return;
        }

        // 1. Vitesse constante (indispensable pour Geometry Dash)

        rb.velocity = new Vector2(speed, rb.velocity.y);

        // 2. Décision de saut
        if (actions.DiscreteActions[0] == 1 && isGrounded)
        {
            Jump();
        }

        // 3. Récompense de survie (très légère)
        AddReward(0.001f);
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
            // Move camera to follow the player
            Vector3 cameraPosition = startPoint.position;
            cameraPosition.x = transform.position.x + cameraOffsetX; // Suivre le joueur en X
            cameraPosition.y = cameraPosition.y + cameraOffsetY; // Suivre le joueur en Y
            cameraPosition.z = cameraPosition.z + cameraOffsetZ; // S'assurer que la caméra reste à une distance fixe en Z
            mainCamera.transform.position = cameraPosition;
        }
    }

    public override void Heuristic(in ActionBuffers actionsOut)
    {
        var discreteActions = actionsOut.DiscreteActions;
        // On peut tester avec Espace au clavier
        discreteActions[0] = Input.GetKey(KeyCode.Space) ? 1 : 0;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        Rigidbody2D rb = GetComponent<Rigidbody2D>();

        // Si on touche le sol
        if (collision.collider.CompareTag("Ground"))
        {
            float velocityY = rb.velocity.y;
            float normalY = collision.GetContact(0).normal.y;

            if (normalY > 0.5f)
            {
                isGrounded = true;
            }
            else // Si on touche le MUR d'un bloc de sol
            {
                Debug.Log("Collision avec le mur détectée !");
                AddReward(-3.0f); // Pénalité pour toucher un mur
                EndEpisode();
            }
        }

        // Si on touche un pic ou un danger
        if (collision.collider.CompareTag("Obstacle"))
        {
            AddReward(-3.0f); // Pénalité pour toucher un obstacle
            EndEpisode();
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Finish"))
        {
            AddReward(10.0f); // Récompense de fin de niveau
            Debug.Log("Succès !");
            EndEpisode();
        }

        if (other.CompareTag("Checkpoint"))
        {
            CheckpointBehavior checkpointBehavior = other.GetComponent<CheckpointBehavior>();
            if (!checkpointBehavior)
            {
                Debug.LogError("Checkpoint sans CheckpointBehavior détecté ! PAS NORMAL");
                return;
            }
            if (checkpointBehavior.IsActivated())
            {
                Debug.Log("Checkpoint déjà activé, pas de récompense supplémentaire.");
                return;
            }
            AddReward(1.5f);
            Debug.Log("Checkpoint atteint !");
        }
    }
}