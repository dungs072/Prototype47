using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

[System.Serializable]
public class Industry
{
    public string Name;
    public float Multiplier;
    public string Description;

    public Industry(string n, float m, string d)
    {
        Name = n; Multiplier = m; Description = d;
    }
}

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    public float Money { get; private set; } = 2000f; // More start money
    public float TimeOfDay { get; private set; } = 8f; 
    public int Day { get; private set; } = 1;

    public float TimeScale = 1f;

    public bool IsGameOver { get; private set; } = false;
    public float LastDailyExpense { get; private set; } = 0f;

    // Config
    public float RentPerDept = 10f; // Reduced from 50
    public float SalaryPerEmployee = 10f; // Reduced from 20

    // Industries
    public List<Industry> Industries = new List<Industry>();
    public Industry CurrentIndustry;

    // Teams
    public List<Team> Teams = new List<Team>();

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            InitializeIndustries();
            InitializeTeams();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void InitializeTeams()
    {
        // Create default team
        Teams.Add(new Team("Unassigned"));
    }

    public void CreateTeam(string teamName)
    {
        if (!string.IsNullOrEmpty(teamName))
        {
            Teams.Add(new Team(teamName));
            Debug.Log($"Created team: {teamName}");
        }
    }

    public Team GetTeam(string teamName)
    {
        return Teams.Find(t => t.Name == teamName);
    }

    void InitializeIndustries()
    {
        Industries.Add(new Industry("General", 1.0f, "Standard Business"));
        Industries.Add(new Industry("Software", 1.2f, "High Margin"));
        Industries.Add(new Industry("Hardware", 1.1f, "Stable"));
        Industries.Add(new Industry("AI", 1.5f, "Booming"));
        Industries.Add(new Industry("Crypto", 2.0f, "Volatile & High Risk")); // High risk not implemented yet, just high reward
        
        CurrentIndustry = Industries[0];
    }

    public void SetIndustry(Industry ind)
    {
        CurrentIndustry = ind;
        Debug.Log($"Switching to {ind.Name} Industry. Multiplier: {ind.Multiplier}x");
    }

    private void Update()
    {
        if (IsGameOver) return;

        float prevTime = TimeOfDay;
        TimeOfDay += Time.deltaTime * TimeScale;
        
        if (prevTime < 18f && TimeOfDay >= 18f)
        {
            PayDailyExpenses();
        }

        if (TimeOfDay >= 24f)
        {
            TimeOfDay = 0f;
            Day++;
        }
    }

    public void AddMoney(float amount)
    {
        if (IsGameOver) return;
        Money += amount;
    }

    public bool SpendMoney(float amount)
    {
        if (IsGameOver) return false;
        if (Money >= amount)
        {
            Money -= amount;
            return true;
        }
        return false;
    }

    public float CalculateTotalDailyExpenses()
    {
        Department[] depts = FindObjectsOfType<Department>();
        float total = 0f;
        foreach (var d in depts)
        {
            if (!d.IsUnlocked) continue; // Skip locked floors for expenses

            total += RentPerDept * d.Level;
            
            // Calculate wages from actual employees
            foreach(var ws in d.Workstations)
            {
                if (ws.IsProductive())
                {
                    total += ws.CurrentEmployee.DailyWage;
                }
            }
        }
        return total;
    }

    void PayDailyExpenses()
    {
        float expense = CalculateTotalDailyExpenses();
        LastDailyExpense = expense;
        
        Money -= expense;
        Debug.Log($"End of Day {Day}. Expenses: ${expense}. Money: ${Money}");

        if (Money < 0)
        {
            GameOver();
        }
    }

    public float CalculateProjectedDailyRevenue()
    {
        Department[] depts = FindObjectsOfType<Department>();
        float totalPerCycle = 0f;
        float industryMult = CurrentIndustry.Multiplier;
        
        foreach (var d in depts)
        {
            if (!d.IsUnlocked) continue;

            foreach (var ws in d.Workstations)
            {
                if (ws.IsProductive())
                {
                    int skill = ws.CurrentEmployee.GetSkill(CurrentIndustry.Name);
                    totalPerCycle += skill * d.BaseRevenuePerSkillPoint * d.Level * industryMult;
                }
            }
        }

        // Logic: Working Hours = 10 (8 to 18).
        // TimeScale = 1 => 1 game hour = 1 real second.
        // CycleTime = 2 real seconds.
        // Cycles per Hour = (1 / 2) = 0.5 cycles per hour??
        // Wait: Timer increases by deltaTime (real seconds).
        // If TimeScale = 1, then deltaTime contributes to TimeOfDay directly.
        // In 1 real second, TimeOfDay increases by 1 hour.
        // In 1 real second, Timer increases by 1.
        // If CycleTime = 2, it takes 2 real seconds to complete a cycle.
        // In 2 real seconds, TimeOfDay increases by 2 hours.
        // So 1 cycle takes 2 game hours.
        // Working Day = 10 hours.
        // Total Cycles = 10 / 2 = 5 cycles.
        
        float cyclesPerDay = 10f / 2f; // Hardcoded derived from Start=8, End=18, Cycle=2, TimeScale=1
        // Ideally fetch these from variables but for prototype this is fine.
        
        return totalPerCycle * cyclesPerDay;
    }

    void GameOver()
    {
        IsGameOver = true;
        Debug.Log("GAME OVER! Bankrupt.");
        Time.timeScale = 0f; // Pause game
    }
}
