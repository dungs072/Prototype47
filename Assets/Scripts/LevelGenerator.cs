using UnityEngine;

public class LevelGenerator : MonoBehaviour
{
    public int FloorCount = 5;
    public float FloorHeight = 4f;

    void Start()
    {
        SetupCamera();
        CreateManagers();
        CreatePlayer();

        for (int i = 0; i < FloorCount; i++)
        {
            CreateFloor(i);
        }

        // CreateElevator(); // Removed in favor of doors
    }

    void SetupCamera()
    {
        Camera.main.transform.position = new Vector3(0, 6, -10); 
        Camera.main.orthographic = true;
        Camera.main.orthographicSize = 12; 
        Camera.main.backgroundColor = new Color(44/255f, 62/255f, 80/255f); // Midnight Blue Background
    }

    void CreateManagers()
    {
        new GameObject("GameManager").AddComponent<GameManager>();
        new GameObject("UIManager").AddComponent<UIManager>();
        new GameObject("EventManager").AddComponent<EventManager>(); 
    }

    void CreatePlayer()
    {
        GameObject player = new GameObject("Player");
        player.tag = "Player";
        player.transform.position = new Vector3(0, 2, 0);

        SpriteRenderer sr = player.AddComponent<SpriteRenderer>();
        // Alizarin Red Player
        sr.sprite = CreateSprite(new Color(231/255f, 76/255f, 60/255f), 32, 64);
        sr.color = Color.white; // Tint is handled by sprite now

        BoxCollider2D col = player.AddComponent<BoxCollider2D>();
        col.size = new Vector2(1, 2);

        Rigidbody2D rb = player.AddComponent<Rigidbody2D>();
        rb.freezeRotation = true;
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous; // Better collision
        rb.gravityScale = 2.5f; // Snappier gravity!
        
        // Zero Friction Material
        PhysicsMaterial2D noFriction = new PhysicsMaterial2D("NoFriction");
        noFriction.friction = 0f;
        noFriction.bounciness = 0f;
        rb.sharedMaterial = noFriction;
        
        PlayerController pc = player.AddComponent<PlayerController>();
        pc.moveSpeed = 10f; // Slightly faster run
        pc.jumpForce = 12f; // Stronger jump due to higher gravity 
        
        pc.groundLayer = LayerMask.GetMask("Default", "TransparentFX", "Water", "UI"); 
        if (pc.groundLayer == 0) pc.groundLayer = -1;
    }

    void CreateFloor(int index)
    {
        float yPos = index * FloorHeight;
        
        // 1. Floor Base
        GameObject floor = new GameObject($"Floor_{index}");
        floor.transform.position = new Vector3(0, yPos - 1, 0);
        floor.layer = LayerMask.NameToLayer("Default");

        SpriteRenderer sr = floor.AddComponent<SpriteRenderer>();
        // Clouds Color for Floor
        sr.sprite = CreateSprite(new Color(236/255f, 240/255f, 241/255f), 200, 20); 
        
        BoxCollider2D col = floor.AddComponent<BoxCollider2D>();
        col.size = new Vector2(20, 2);

        CreateDepartment(index, new Vector3(0, yPos, 0));
        CreateElevatorDoor(index, new Vector3(6, yPos + 1.5f, 0));
    }

    void CreateDepartment(int floorIndex, Vector3 pos)
    {
        GameObject deptObj = new GameObject($"Department_F{floorIndex}");
        deptObj.transform.position = pos;
        deptObj.layer = LayerMask.NameToLayer("Ignore Raycast");

        // Visual for Dept Zone
        GameObject zone = new GameObject("ZoneVisual");
        zone.transform.SetParent(deptObj.transform);
        zone.transform.localPosition = new Vector3(0, 1.5f, 0);
        SpriteRenderer sr = zone.AddComponent<SpriteRenderer>();
        // Faint Blue Zone
        sr.sprite = CreateSprite(new Color(52/255f, 152/255f, 219/255f, 0.2f), 100, 40); 
        
        // Interactable Trigger
        BoxCollider2D col = deptObj.AddComponent<BoxCollider2D>();
        col.isTrigger = true;
        col.size = new Vector2(10, 4);
        col.offset = new Vector3(0, 1.5f, 0);

        Department dept = deptObj.AddComponent<Department>();
        dept.DeptName = $"Floor {floorIndex} Devs";
        dept.IsUnlocked = (floorIndex == 0); 
        
        // Add Walls at ends of department?
        CreateWall(deptObj.transform, new Vector3(-5.5f, 1.5f, 0));
        CreateWall(deptObj.transform, new Vector3(5.5f, 1.5f, 0));

        // Create empty workstations
        for (int i = 0; i < 3; i++) 
        {
            GameObject desk = new GameObject($"Desk_{i}");
            desk.transform.SetParent(deptObj.transform);
            desk.transform.localPosition = new Vector3(-3 + (i * 3), 0.5f, 0);
            
            Workstation ws = desk.AddComponent<Workstation>();
            ws.Init(new Color(211/255f, 84/255f, 0/255f)); // Pumpkin Wood Color
            
            dept.AddWorkstation(ws);
        }
    }

    void CreateWall(Transform parent, Vector3 localPos)
    {
        GameObject wall = new GameObject("Wall");
        wall.transform.SetParent(parent);
        wall.transform.localPosition = localPos;
        SpriteRenderer sr = wall.AddComponent<SpriteRenderer>();
        sr.sprite = CreateSprite(new Color(127/255f, 140/255f, 141/255f), 10, 40); // Grey Concrete
    }

    void CreateElevatorDoor(int floorIndex, Vector3 pos)
    {
        GameObject door = new GameObject($"ElevatorDoor_F{floorIndex}");
        door.transform.position = pos;
        
        // Visual
        SpriteRenderer sr = door.AddComponent<SpriteRenderer>();
        sr.sprite = CreateSprite(new Color(189/255f, 195/255f, 199/255f), 32, 64); // Silver Door
        
        // Trigger
        BoxCollider2D col = door.AddComponent<BoxCollider2D>();
        col.isTrigger = true;
        col.size = new Vector2(2, 3);
        
        // Script
        ElevatorDoor ed = door.AddComponent<ElevatorDoor>();
        ed.FloorIndex = floorIndex;
        ed.FloorHeight = FloorHeight;
        ed.MaxFloors = FloorCount;
    }

    Sprite CreateSprite(Color color, int w, int h)
    {
        Texture2D texture = new Texture2D(w, h);
        Color[] pixels = new Color[w * h];
        for (int i = 0; i < pixels.Length; i++) pixels[i] = color;
        texture.SetPixels(pixels);
        texture.Apply();
        
        // Set Filter Mode to Point for crisp pixel art look
        texture.filterMode = FilterMode.Point; 
        
        return Sprite.Create(texture, new Rect(0, 0, w, h), new Vector2(0.5f, 0.5f), 32);
    }
}
