using UnityEngine;

public class Workstation : MonoBehaviour
{
    public bool HasComputer { get; private set; }
    public bool HasEmployee { get; private set; }
    public EmployeeData CurrentEmployee { get; private set; }

    private SpriteRenderer sr;
    private GameObject computerVisual;
    private GameObject employeeVisual;

    private void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
    }

    public void Init(Color deskColor)
    {
        if (sr == null) sr = gameObject.AddComponent<SpriteRenderer>();
        
        // Desk visual
        Texture2D tex = new Texture2D(32, 32);
        Color[] colors = new Color[32*32];
        for(int i=0; i<colors.Length; i++) colors[i] = deskColor;
        tex.SetPixels(colors);
        tex.Apply();
        sr.sprite = Sprite.Create(tex, new Rect(0,0,32,32), new Vector2(0.5f,0f), 32); // Pivot bottom center
    }

    public void BuyComputer()
    {
        if (HasComputer) return;
        HasComputer = true;
        
        // Create Computer Visual
        computerVisual = new GameObject("Computer");
        computerVisual.transform.SetParent(transform);
        computerVisual.transform.localPosition = new Vector3(0, 0.6f, 0); 
        
        SpriteRenderer csr = computerVisual.AddComponent<SpriteRenderer>();
        // White box
        Texture2D tex = new Texture2D(16, 16);
        for(int i=0; i<tex.GetPixels().Length; i++) tex.SetPixel(i%16, i/16, Color.white);
        tex.Apply();
        csr.sprite = Sprite.Create(tex, new Rect(0,0,16,16), new Vector2(0.5f, 0.5f), 32);
    }

    public void HireEmployee(EmployeeData data)
    {
        if (!HasComputer || HasEmployee) return;
        HasEmployee = true;
        CurrentEmployee = data;

        // Create Employee Visual
        employeeVisual = new GameObject("Employee");
        employeeVisual.transform.SetParent(transform);
        employeeVisual.transform.localPosition = new Vector3(0, 0.5f, -0.1f); 
        
        SpriteRenderer esr = employeeVisual.AddComponent<SpriteRenderer>();
        // Color based on skill? 
        // 1=Yellow, 5=Purple?
        // Use current industry skill or average? Let's use General for visual or average.
        int avgSkill = 0;
        foreach(var sk in data.Skills.Values) avgSkill += sk;
        avgSkill /= data.Skills.Count > 0 ? data.Skills.Count : 1;

        Color c = Color.Lerp(Color.yellow, Color.magenta, (avgSkill - 1) / 4f);

        Texture2D tex = new Texture2D(20, 20);
        for(int i=0; i<tex.GetPixels().Length; i++) tex.SetPixel(i%20, i/20, c);
        tex.Apply();
        esr.sprite = Sprite.Create(tex, new Rect(0,0,20,20), new Vector2(0.5f, 0.5f), 32);
    }

    public bool IsProductive()
    {
        return HasComputer && HasEmployee;
    }

    public void FireEmployee()
    {
        if (!HasEmployee) return;
        HasEmployee = false;
        CurrentEmployee = null;
        
        if (employeeVisual != null)
        {
            Destroy(employeeVisual);
            employeeVisual = null;
        }
        
        Debug.Log("Employee Fired.");
    }
}
