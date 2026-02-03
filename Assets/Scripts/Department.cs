using UnityEngine;
using System.Collections.Generic;

public class Department : Interactable
{
    public string DeptName = "StartUp Dept";
    public float BaseRevenuePerSkillPoint = 5f; // Revenue = Skill * 5 * Level
    public float CycleTime = 2f; 
    private float timer = 0f;

    public int Level = 1; 
    
    // Unlocking Logic
    public bool IsUnlocked = false;
    public float UnlockCost = 500f;

    public List<Workstation> Workstations = new List<Workstation>();
    public float ComputerCost = 200f;
    
    // Config for Hiring
    public float HiringCostBase = 50f; // One time fee

    private void Update()
    {
        if (GameManager.Instance.IsGameOver) return;
        if (!IsUnlocked) return; // Do not run cycles if locked

        if (GameManager.Instance.TimeOfDay >= 8f && GameManager.Instance.TimeOfDay < 18f)
        {
            timer += Time.deltaTime;
            if (timer >= CycleTime)
            {
                GenerateRevenue();
                timer = 0f;
            }
        }
    }

    void GenerateRevenue()
    {
        if (!IsUnlocked) return;
        
        float totalRevenue = 0f;
        float industryMult = GameManager.Instance.CurrentIndustry.Multiplier;

        foreach (var ws in Workstations)
        {
            if (ws.IsProductive())
            {
                EmployeeData emp = ws.CurrentEmployee;
                
                // --- Trait Logic ---
                float multiplier = 1.0f;
                int effectiveSkill = emp.GetSkill(GameManager.Instance.CurrentIndustry.Name);

                switch (emp.Personality)
                {
                    case Trait.Lazy:
                        // 20% chance to sleep (0 revenue)
                        if (UnityEngine.Random.value < 0.2f) return; 
                        break;
                    case Trait.Workaholic:
                        multiplier = 1.3f; // Work hard
                        // 1% Chance to Burnout (daily? No, this is per cycle. Make it VERY small)
                        // Cycle is 2s approx. Day is ~10s? 
                        // Let's keep it safe: just bonus for now. Burnout is complex state change.
                        break;
                    case Trait.Creative:
                        // 10% chance for flat bonus
                        if (UnityEngine.Random.value < 0.1f)
                        {
                            float bonus = 50f;
                            GameManager.Instance.AddMoney(bonus);
                            // Visual feedback?
                        }
                        break;
                    case Trait.Perfectionist:
                        multiplier = 0.8f; // Slow
                        effectiveSkill += 2; // But high quality
                        break;
                }

                // Revenue formula
                float empRev = effectiveSkill * BaseRevenuePerSkillPoint * Level * industryMult * multiplier;
                totalRevenue += empRev;
            }
        }

        if (totalRevenue > 0)
        {
            // Calculate Team Synergy Bonus
            float synergyMultiplier = CalculateTeamSynergy();
            totalRevenue *= synergyMultiplier;
            
            GameManager.Instance.AddMoney(totalRevenue);
        }
    }

    float CalculateTeamSynergy()
    {
        // Group employees by team
        Dictionary<string, List<Trait>> teamTraits = new Dictionary<string, List<Trait>>();
        
        foreach (var ws in Workstations)
        {
            if (ws.IsProductive())
            {
                string team = ws.CurrentEmployee.TeamName;
                if (team == "Unassigned") continue; // No synergy for unassigned
                
                if (!teamTraits.ContainsKey(team))
                    teamTraits[team] = new List<Trait>();
                    
                teamTraits[team].Add(ws.CurrentEmployee.Personality);
            }
        }

        float synergyBonus = 1.0f;

        // Check each team for synergies
        foreach (var team in teamTraits)
        {
            if (team.Value.Count < 2) continue; // Need at least 2 members for synergy
            
            // Check all pairs
            for (int i = 0; i < team.Value.Count; i++)
            {
                for (int j = i + 1; j < team.Value.Count; j++)
                {
                    Trait t1 = team.Value[i];
                    Trait t2 = team.Value[j];
                    
                    // Good Synergies
                    if ((t1 == Trait.Workaholic && t2 == Trait.Perfectionist) ||
                        (t1 == Trait.Perfectionist && t2 == Trait.Workaholic))
                    {
                        synergyBonus += 0.15f; // Quality meets Speed
                    }
                    else if (t1 == Trait.Creative && t2 == Trait.Creative)
                    {
                        synergyBonus += 0.15f; // Innovation Boost
                    }
                    else if (t1 == Trait.Regular || t2 == Trait.Regular)
                    {
                        // Regular is neutral, no change
                    }
                    // Bad Synergies
                    else if ((t1 == Trait.Lazy && t2 == Trait.Workaholic) ||
                             (t1 == Trait.Workaholic && t2 == Trait.Lazy))
                    {
                        synergyBonus -= 0.10f; // Work ethic conflict
                    }
                    else if (t1 == Trait.Perfectionist && t2 == Trait.Perfectionist)
                    {
                        synergyBonus -= 0.10f; // Overthinking
                    }
                    else if (t1 == Trait.Lazy && t2 == Trait.Lazy)
                    {
                        synergyBonus -= 0.10f; // Double slacking
                    }
                }
            }
        }

        // Clamp to reasonable range
        synergyBonus = Mathf.Max(0.5f, Mathf.Min(2.0f, synergyBonus));
        
        if (synergyBonus != 1.0f)
        {
            Debug.Log($"{DeptName} Team Synergy: {synergyBonus:F2}x");
        }
        
        return synergyBonus;
    }

    public override void OnInteract()
    {
        if (GameManager.Instance.IsGameOver) return;
        Debug.Log("Interacting with " + DeptName);
        UIManager.Instance.ShowDepartmentMenu(this);
    }

    public void AddWorkstation(Workstation ws)
    {
        Workstations.Add(ws);
    }

    public int GetComputerCount()
    {
        int count = 0;
        foreach(var ws in Workstations) if(ws.HasComputer) count++;
        return count;
    }

    public int GetEmployeeCount()
    {
        int count = 0;
        foreach(var ws in Workstations) if(ws.HasEmployee) count++;
        return count;
    }

    public bool HasVacancy()
    {
        foreach(var ws in Workstations) if(ws.HasComputer && !ws.HasEmployee) return true;
        return false;
    }

    public void BuyComputerAction()
    {
        // First check if there is an empty desk
        Workstation emptyDesk = null;
        foreach (var ws in Workstations)
        {
            if (!ws.HasComputer)
            {
                emptyDesk = ws;
                break;
            }
        }

        if (emptyDesk == null)
        {
            Debug.Log("No empty desks for computers!");
            return;
        }

        if (GameManager.Instance.SpendMoney(ComputerCost))
        {
            emptyDesk.BuyComputer();
            Debug.Log("Bought Computer!");
        }
        else
        {
            Debug.Log("Not enough money to buy computer!");
        }
    }

    // New Hire Action taking data
    public void RecruitEmployee(EmployeeData candidate)
    {
        // One time hiring fee based on wage (approx 5x daily wage?)
        float fee = candidate.DailyWage * 3f;  // New formula
        if (GameManager.Instance.SpendMoney(fee))
        {
            foreach (var ws in Workstations)
            {
                if (ws.HasComputer && !ws.HasEmployee)
                {
                    ws.HireEmployee(candidate);
                    Debug.Log($"Recruited {candidate.Name} (Wage: ${candidate.DailyWage})!");
                    return;
                }
            }
        }
        else
        {
            Debug.Log("Not enough money to hire!");
        }
    }

    private void Start()
    {
        UpdateVisuals();
    }

    private void UpdateVisuals()
    {
        // 1. Zone Visual
        Transform zone = transform.Find("ZoneVisual");
        if (zone != null)
        {
            SpriteRenderer sr = zone.GetComponent<SpriteRenderer>();
            if (sr != null)
            {
                if (IsUnlocked)
                    sr.color = new Color(0, 0, 1, 0.2f); // Blue transparent
                else
                    sr.color = new Color(0.2f, 0.2f, 0.2f, 0.8f); // Dark Grey opaque
            }
        }

        // 2. Workstations
        foreach (var ws in Workstations)
        {
            // Disable interactions/visuals of workstations if locked?
            // Or just make them dark. Let's disable the whole gameObject to hide them.
            ws.gameObject.SetActive(IsUnlocked);
        }
    }

    public void UnlockFloorAction()
    {
        if (IsUnlocked) return;
        
        if (GameManager.Instance.SpendMoney(UnlockCost))
        {
            IsUnlocked = true;
            Debug.Log("Floor Unlocked!");
            UpdateVisuals();
        }
    }
}
