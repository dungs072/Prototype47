using System.Collections.Generic;
using System;

public enum Trait { Regular, Lazy, Workaholic, Creative, Perfectionist }

[System.Serializable]
public class EmployeeData
{
    public string Name;
    public Dictionary<string, int> Skills = new Dictionary<string, int>();
    public float DailyWage;
    public Trait Personality;
    public string TeamName = "Unassigned"; // Default team

    public EmployeeData()
    {
        string[] names = { "Anh", "Binh", "Chi", "Dung", "Giang", "Huy", "Lan", "Minh", "Nam", "Phuc", "Quan", "Son", "Thao", "Tuan", "Vy", "Yen" };
        Name = names[UnityEngine.Random.Range(0, names.Length)];

        // Random Trait
        Array values = Enum.GetValues(typeof(Trait));
        Personality = (Trait)values.GetValue(UnityEngine.Random.Range(0, values.Length));

        // Generate skills

        // Generate skills for each Industry defined in GameManager
        // Notes: GameManager must be initialized first.
        var industries = GameManager.Instance.Industries;
        
        float totalSkill = 0;
        foreach(var ind in industries)
        {
            int skill = UnityEngine.Random.Range(1, 6);
            Skills[ind.Name] = skill;
            totalSkill += skill;
        }

        // Wage formula: Base $10 + ($2 * Total Skill Points) + Variation
        // Example: 5 industries * avg 3 = 15 points. Wage = 10 + 30 = 40.
        DailyWage = 10f + (2f * totalSkill) + UnityEngine.Random.Range(-5f, 5f);
        DailyWage = (int)DailyWage;
    }

    public int GetSkill(string industryName)
    {
        if (Skills.ContainsKey(industryName))
            return Skills[industryName];
        return 1; // Default
    }
}
