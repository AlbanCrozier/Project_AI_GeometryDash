using UnityEngine;

public class ManualPlayerTests : MonoBehaviour
{
    [Header("Réglages Physiques")]
    public float speed = 8f;
    public float jumpForce = 12f;

    [Header("Connexions")]
    public Transform startPoint;

    private Rigidbody2D rb;
    private bool isGrounded;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();

        // Empêche le cube de basculer sur lui-même
        rb.freezeRotation = true;

        // Position de départ
        if (startPoint != null) transform.position = startPoint.position;
    }

    void Update()
    {
        // 1. Déplacement constant (Vitesse X forcée, Vitesse Y libre pour le saut)
        rb.velocity = new Vector2(speed, rb.velocity.y);

        // 2. Saut (Espace ou Clic Gauche)
        if ((Input.GetKeyDown(KeyCode.Space) || Input.GetMouseButtonDown(0)) && isGrounded)
        {
            Jump();
        }
    }

    void Jump()
    {
        rb.velocity = new Vector2(rb.velocity.x, jumpForce);
        isGrounded = false;
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
                
            }
            // Si la normale est horizontale (contact sur le côté)
            else if (Mathf.Abs(contactNormal.x) > 0.5f)
            {

                RestartPosition(); // Mort !
            }
        }
        if (collision.collider.CompareTag("Obstacle"))
        {

            RestartPosition();
        }
    }
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Finish"))
        {
            
            Debug.Log("Niveau terminé ! Bravo l'IA.");
            RestartPosition();    // On recommence pour renforcer l'apprentissage
        }
    }

    void RestartPosition()
    {
        if (startPoint != null)
        {
            transform.position = startPoint.position;
            rb.velocity = Vector2.zero; // On stop net le mouvement lors du reset
        }
    }
}