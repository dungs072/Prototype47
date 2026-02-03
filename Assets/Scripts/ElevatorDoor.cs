using UnityEngine;

public class ElevatorDoor : MonoBehaviour
{
    public int FloorIndex;
    public float FloorHeight = 4f;
    public int MaxFloors = 5;

    private bool playerInRange = false;
    private GameObject player;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            playerInRange = true;
            player = collision.gameObject;
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            playerInRange = false;
            player = null;
        }
    }

    private void Update()
    {
        if (playerInRange && player != null)
        {
            // Go UP (W or Up Arrow)
            if (Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.UpArrow))
            {
                if (FloorIndex < MaxFloors - 1)
                {
                    Teleport(FloorIndex + 1);
                }
            }
            // Go DOWN (S or Down Arrow)
            if (Input.GetKeyDown(KeyCode.S) || Input.GetKeyDown(KeyCode.DownArrow))
            {
                if (FloorIndex > 0)
                {
                    Teleport(FloorIndex - 1);
                }
            }
        }
    }

    void Teleport(int targetFloor)
    {
        // Simple Teleport: Keep X, Change Y
        float targetY = targetFloor * FloorHeight;
        // Offset Y slightly to stand on floor
        targetY += 1.0f; // Player center is 1 unit up from pivot usually
        // Actually player creation has pivot center. Floor is at Y-1. Floor surface is Y.
        // Player pos at Y=2 triggers feet at Y=1.
        // Wait, CreatePlayer: pos(0,2,0). BoxCollider size(1,2). Default pivot center.
        // So feet are at Y=1.
        // Floor at Y-1, size Y=2 => Top at Y=0.
        // So Player at Y=2 is floating? 
        // Let's stick to generating relative.
        
        // Target Y = TargetFloor * FloorHeight + 2 (Original spawn offset)
        float newY = (targetFloor * FloorHeight) + 2f;
        
        player.transform.position = new Vector3(player.transform.position.x, newY, player.transform.position.z);
        Debug.Log($"Teleporting to Floor {targetFloor}");
    }
}
