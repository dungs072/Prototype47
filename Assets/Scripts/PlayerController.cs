using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public float moveSpeed = 5f;
    public float jumpForce = 5f;
    private Rigidbody2D rb;
    private bool isGrounded;
    public LayerMask groundLayer;
    public Interactable currentInteractable;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    private float horizontalInput;
    private bool jumpRequest;

    private void FixedUpdate()
    {
        // Simple ground check
        isGrounded = Physics2D.Raycast(transform.position, Vector2.down, 1.1f, groundLayer);

        // Apply Movement
        rb.linearVelocity = new Vector2(horizontalInput * moveSpeed, rb.linearVelocity.y);

        // Apply Jump
        if (jumpRequest)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
            jumpRequest = false;
        }
    }

    private void Update()
    {
        // Input Processing
        horizontalInput = 0f;
        if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow)) horizontalInput = -1f;
        if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow)) horizontalInput = 1f;

        // Jump Input
        if ((Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.W)) && isGrounded)
        {
            jumpRequest = true;
        }

        // Interaction
        if (Input.GetKeyDown(KeyCode.E))
        {
            if (currentInteractable != null)
            {
                currentInteractable.OnInteract();
            }
        }
    }
    
    public void SetInteractable(Interactable interactable)
    {
        currentInteractable = interactable;
    }
}
