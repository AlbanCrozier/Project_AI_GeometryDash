using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;

public class PlayerAgent : Agent
{
    public float speed = 8f;
    public float jumpForce = 12f;
    public Transform startPoint; // Un objet vide placé au début du niveau

    private Rigidbody2D rb;
    private bool isGrounded;

    // Remplace Start()
    public override void Initialize()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    // Appelé à chaque fois que le cube meurt (Reset)
    public override void OnEpisodeBegin()
    {
        transform.position = startPoint.position;
        rb.velocity = Vector2.zero;
        isGrounded = false;
    }

    // Pour le mouvement constant et les décisions de l'IA
    public override void OnActionReceived(ActionBuffers actions)
    {
        // Vitesse constante sur X
        rb.velocity = new Vector2(speed, rb.velocity.y);

        // L'IA décide de sauter (Action 1)
        int jumpAction = actions.DiscreteActions[0];
        if (jumpAction == 1 && isGrounded)
        {
            Jump();
        }

        // Récompense : on encourage l'IA à rester en vie le plus longtemps possible
        AddReward(0.01f);
    }

    void Jump()
    {
        rb.velocity = new Vector2(rb.velocity.x, jumpForce);
        isGrounded = false;
    }

    // Permet de tester au clavier même avec ML-Agents
    public override void Heuristic(in ActionBuffers actionsOut)
    {
        var discreteActions = actionsOut.DiscreteActions;
        discreteActions[0] = Input.GetKey(KeyCode.Space) ? 1 : 0;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.collider.CompareTag("Block")) // Donne le tag "Block" à tes carrés
        {
            // On récupère le point de contact
            Vector2 contactNormal = collision.GetContact(0).normal;

            // Si la normale pointe vers le haut (0, 1), on est sur le dessus
            if (contactNormal.y > 0.5f)
            {
                isGrounded = true;
                AddReward(0.05f); // Petit bonus pour avoir atterri proprement
            }
            // Si la normale est horizontale (contact sur le côté)
            else if (Mathf.Abs(contactNormal.x) > 0.5f)
            {
                SetReward(-1.0f);
                EndEpisode(); // Mort !
            }
        }
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
            SetReward(2.0f); // Une grosse récompense positive !
            Debug.Log("Niveau terminé ! Bravo l'IA.");
            EndEpisode();    // On recommence pour renforcer l'apprentissage
        }
    }
}