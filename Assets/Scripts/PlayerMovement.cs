using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;

public class PlayerAgent : Agent
{
    [Header("Réglages Physiques")]
    public float speed = 10.4f;
    public float jumpForce = 15f;
    public Transform startPoint;

    private Rigidbody2D rb;
    private bool isGrounded;

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
        isGrounded = false;
    }

    public override void OnActionReceived(ActionBuffers actions)
    {
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

    void Jump()
    {
        rb.velocity = new Vector2(rb.velocity.x, jumpForce);
        isGrounded = false;
    }

    public override void Heuristic(in ActionBuffers actionsOut)
    {
        var discreteActions = actionsOut.DiscreteActions;
        // On peut tester avec Espace au clavier
        discreteActions[0] = Input.GetKey(KeyCode.Space) ? 1 : 0;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        // Si on touche le sol
        if (collision.collider.CompareTag("Ground"))
        {
            // Vérification : on ne valide le sol que si on le touche par le dessus
            if (collision.GetContact(0).normal.y > 0.5f)
            {
                isGrounded = true;
            }
            else // Si on touche le MUR d'un bloc de sol
            {
                SetReward(-1.0f);
                EndEpisode();
            }
        }

        // Si on touche un pic ou un danger
        if (collision.collider.CompareTag("Obstacle"))
        {
            SetReward(-1.0f);
            EndEpisode();
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Finish"))
        {
            SetReward(2.0f);
            Debug.Log("Succès !");
            EndEpisode();
        }
    }
}