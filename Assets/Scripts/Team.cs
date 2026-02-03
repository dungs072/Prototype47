using System.Collections.Generic;

[System.Serializable]
public class Team
{
    public string Name;
    public List<string> MemberNames = new List<string>(); // Store employee names instead of references

    public Team(string name)
    {
        Name = name;
    }

    public void AddMember(string employeeName)
    {
        if (!MemberNames.Contains(employeeName))
        {
            MemberNames.Add(employeeName);
        }
    }

    public void RemoveMember(string employeeName)
    {
        MemberNames.Remove(employeeName);
    }
}
