using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }
    
    private Department currentDept;
    
    // UI Rects
    private Rect industryPanelRect = new Rect(10, 140, 150, 200);
    private Rect employeePanelRect;
    private bool isEmployeeListOpen = true;
    private bool isTeamPanelOpen = false;
    private Rect teamPanelRect = new Rect(10, 350, 150, 200);

    private Vector2 scrollPosition;
    private Vector2 teamScrollPosition;

    // Hiring System
    private bool showHiringModal = false;
    private List<EmployeeData> currentCandidates = new List<EmployeeData>();

    // Team System
    private bool showTeamCreationModal = false;
    private string newTeamName = "";
    private bool showTeamAssignModal = false;
    private EmployeeData employeeToAssign;

    private void Awake()
    {
        Instance = this;
    }

    public void ShowDepartmentMenu(Department dept)
    {
        currentDept = dept;
        showHiringModal = false; // Reset
    }

    public void ShowEventModal(GameEvent e)
    {
        currentEvent = e;
        showEventModal = true;
        HideDepartmentMenu(); // Close other menus
    }

    public void HideEventModal()
    {
        currentEvent = null;
        showEventModal = false;
    }

    private GameEvent currentEvent;
    private bool showEventModal = false;

    public void HideDepartmentMenu()
    {
        currentDept = null;
        showHiringModal = false;
    }

    private void OnGUI()
    {
        // Global visual improvement: Darker skin if possible?
        // Basic IMGUI is limited.

        if (GameManager.Instance.IsGameOver)
        {
            GUIStyle style = new GUIStyle(GUI.skin.box);
            style.fontSize = 40;
            style.alignment = TextAnchor.MiddleCenter;
            style.normal.textColor = Color.red;

            GUI.Box(new Rect(0, 0, Screen.width, Screen.height), "GAME OVER\nBANKRUPT", style);
            return;
        }

        DrawHUD();
        DrawIndustryPanel();
        
        // Team Panel Toggle
        if (GUI.Button(new Rect(10, 560, 150, 30), isTeamPanelOpen ? "Hide Teams" : "Show Teams"))
        {
            isTeamPanelOpen = !isTeamPanelOpen;
        }

        if (isTeamPanelOpen)
        {
            DrawTeamPanel();
        }
        
        // Employee List Toggle
        float toggleBtnW = 120;
        float toggleBtnX = Screen.width - toggleBtnW - 10;
        if (GUI.Button(new Rect(toggleBtnX, 10, toggleBtnW, 30), isEmployeeListOpen ? "Hide Staff >>" : "<< Show Staff"))
        {
            isEmployeeListOpen = !isEmployeeListOpen;
        }

        if (isEmployeeListOpen)
        {
            employeePanelRect = new Rect(Screen.width - 260, 45, 250, Screen.height - 55);
            DrawEmployeeList();
        }

        if (showTeamCreationModal)
        {
            GUI.Box(new Rect(0,0,Screen.width, Screen.height), "");
            DrawTeamCreationModal();
        }
        else if (showTeamAssignModal)
        {
            GUI.Box(new Rect(0,0,Screen.width, Screen.height), "");
            DrawTeamAssignModal();
        }
        else if (showEventModal)
        {
            // Draw a dark background over scene for modal focus
            GUI.Box(new Rect(0,0,Screen.width, Screen.height), ""); 
            DrawEventModal();
        }
        else if (showHiringModal)
        {
             // Draw a dark background over scene for modal focus
            GUI.Box(new Rect(0,0,Screen.width, Screen.height), ""); 
            DrawHiringModal();
        }
        else
        {
            DrawDepartmentMenu();
        }
    }

    void DrawEventModal()
    {
        if (currentEvent == null) return;

        float w = 400;
        float h = 300;
        float x = Screen.width / 2 - w / 2;
        float y = Screen.height / 2 - h / 2;

        GUI.Box(new Rect(x, y, w, h), currentEvent.Title);

        // Color Title background red if negative?
        
        GUI.Label(new Rect(x + 20, y + 40, w - 40, 60), currentEvent.Description);

        float btnY = y + 120;
        foreach(var opt in currentEvent.Options)
        {
            if (GUI.Button(new Rect(x + 50, btnY, w - 100, 40), opt.Text))
            {
                opt.OnSelect?.Invoke();
            }
            btnY += 50;
        }
    }

    void DrawHUD()
    {
        GUI.Box(new Rect(10, 10, 260, 120), "Management Dashboard");
        GUI.Label(new Rect(20, 30, 240, 20), $"Money: ${GameManager.Instance.Money:F0}");
        GUI.Label(new Rect(20, 50, 240, 20), $"Time: {GameManager.Instance.TimeOfDay:00}:00 (Day {GameManager.Instance.Day})");
        
        float projExp = GameManager.Instance.CalculateTotalDailyExpenses();
        float projRev = GameManager.Instance.CalculateProjectedDailyRevenue();
        
        GUI.contentColor = Color.green;
        GUI.Label(new Rect(20, 70, 240, 20), $"Daily Revenue: +${projRev:F0}");
        GUI.contentColor = Color.red;
        GUI.Label(new Rect(20, 90, 240, 20), $"Daily Expenses: -${projExp:F0}");
        GUI.contentColor = Color.white;
    }

    void DrawIndustryPanel()
    {
        GUI.Box(industryPanelRect, "Industry Focus");
        
        float y = industryPanelRect.y + 25;
        foreach (var ind in GameManager.Instance.Industries)
        {
            bool isSelected = (ind == GameManager.Instance.CurrentIndustry);
            string prefix = isSelected ? ">> " : "";
            string name = $"{prefix}{ind.Name} (x{ind.Multiplier})";

            if (GUI.Button(new Rect(industryPanelRect.x + 5, y, industryPanelRect.width - 10, 25), name))
            {
                GameManager.Instance.SetIndustry(ind);
            }
            y += 30;
        }
    }

    void DrawDepartmentMenu()
    {
        if (currentDept == null) return;

        float w = 300;
        float h = 250;
        float x = Screen.width / 2 - w / 2;
        float y = Screen.height / 2 - h / 2;

        GUI.Box(new Rect(x, y, w, h), currentDept.DeptName);
        
        // Locked State
        if (!currentDept.IsUnlocked)
        {
            GUI.Label(new Rect(x + 20, y + 40, w - 40, 20), "This floor is currently LOCKED.");
            GUI.Label(new Rect(x + 20, y + 60, w - 40, 20), "Unlock to expand your business.");
            
            if (GUI.Button(new Rect(x + 50, y + 100, 200, 40), $"Unlock Floor (${currentDept.UnlockCost})"))
            {
                currentDept.UnlockFloorAction();
            }

            if (GUI.Button(new Rect(x + 50, y + 180, 200, 30), "Close"))
            {
                HideDepartmentMenu();
            }
            return;
        }

        // Stats
        int pcCount = currentDept.GetComputerCount();
        int empCount = currentDept.GetEmployeeCount();
        int maxCap = currentDept.Workstations.Count;

        GUI.Label(new Rect(x + 20, y + 30, w - 40, 20), $"Desks: {maxCap} | PCs: {pcCount} | Staff: {empCount}");

        // Buttons
        // Buy Computer
        if (GUI.Button(new Rect(x + 50, y + 60, 200, 30), $"Buy Computer (${currentDept.ComputerCost})"))
        {
            currentDept.BuyComputerAction();
        }

        // Hiring Button (Opens Modal)
        bool hasVacancy = currentDept.HasVacancy();
        if (hasVacancy)
        {
            if (GUI.Button(new Rect(x + 50, y + 100, 200, 30), "Recruit Employee..."))
            {
                GenerateCandidates();
                showHiringModal = true;
            }
        }
        else
        {
             GUI.Label(new Rect(x + 50, y + 100, 200, 30), "No Vacancy / Needs PC");
        }

        if (GUI.Button(new Rect(x + 50, y + 180, 200, 30), "Close"))
        {
            HideDepartmentMenu();
        }
    }

    void GenerateCandidates()
    {
        currentCandidates.Clear();
        for(int i=0; i<3; i++)
        {
            // Constructor now autogenerates skills based on GameManager industries
            currentCandidates.Add(new EmployeeData());
        }
    }

    void DrawHiringModal()
    {
        float w = 600; // Increased width
        float h = 450; // Increased height
        float x = Screen.width / 2 - w / 2;
        float y = Screen.height / 2 - h / 2;

        GUI.Box(new Rect(x, y, w, h), "Seeking Candidates");

        GUI.Label(new Rect(x+20, y+30, 300, 20), "Choose 1 candidate to hire:");

        float cardW = 180; // Wider cards
        float startX = x + 20;
        float cardY = y + 60;
        float cardH = 320; // Taller cards to fit skills

        for(int i=0; i<currentCandidates.Count; i++)
        {
            var c = currentCandidates[i];
            float cardX = startX + (i * (cardW + 10));
            
            GUI.Box(new Rect(cardX, cardY, cardW, cardH), "");
            GUI.Label(new Rect(cardX+5, cardY+5, cardW-10, 20), $"<b>{c.Name}</b>");
            
            // Trait Display
            GUI.contentColor = Color.cyan;
            GUI.Label(new Rect(cardX+5, cardY+25, cardW-10, 20), $"[{c.Personality}]");
            GUI.contentColor = Color.white;

            // Skill List
            float sy = cardY + 50;
            GUI.Label(new Rect(cardX+5, sy, cardW-10, 20), "Skills:");
            sy += 20;
            
            foreach(var kvp in c.Skills)
            {
                GUI.Label(new Rect(cardX+10, sy, cardW-15, 20), $"{kvp.Key}: <b>{kvp.Value}</b>");
                sy += 20; 
            }

            // Wage and Hire
            float bottomY = cardY + cardH - 70;
            GUI.Label(new Rect(cardX+5, bottomY, cardW-10, 20), $"Wage: ${c.DailyWage}/d");
            
            float hireFee = c.DailyWage * 3f;
            
            if(GUI.Button(new Rect(cardX+5, bottomY + 25, cardW-10, 35), $"Hire (-${hireFee})"))
            {
                currentDept.RecruitEmployee(c);
                showHiringModal = false; 
            }
        }

        if (GUI.Button(new Rect(x + 200, y + 400, 200, 30), "Cancel"))
        {
            showHiringModal = false;
        }
    }

    void DrawEmployeeList()
    {
        GUI.Box(employeePanelRect, "Staff Directory");

        Department[] depts = FindObjectsOfType<Department>();
        List<Workstation> allActive = new List<Workstation>();
        
        foreach(var d in depts)
        {
            foreach(var ws in d.Workstations)
            {
                if(ws.HasEmployee) allActive.Add(ws);
            }
        }

        float rowHeight = 210f; // Increased to fit team display and 3 buttons
        float contentHeight = allActive.Count * rowHeight;
        Rect viewRect = new Rect(0, 0, employeePanelRect.width - 20, contentHeight);
        Rect scrollRect = new Rect(employeePanelRect.x + 5, employeePanelRect.y + 25, employeePanelRect.width - 10, employeePanelRect.height - 30);

        scrollPosition = GUI.BeginScrollView(scrollRect, scrollPosition, viewRect);

        float y = 0;
        foreach(var ws in allActive)
        {
            EmployeeData emp = ws.CurrentEmployee;
            Department dept = ws.GetComponentInParent<Department>(); 
            float revenue = 0;
            string currentIndName = GameManager.Instance.CurrentIndustry.Name;

            if (dept != null)
            {
                int currentSkill = emp.GetSkill(currentIndName);
                if (emp.Personality == Trait.Perfectionist) currentSkill += 2; // UI Feedback for perk
                
                float industryMult = GameManager.Instance.CurrentIndustry.Multiplier;
                float traitMult = (emp.Personality == Trait.Workaholic) ? 1.3f : (emp.Personality == Trait.Perfectionist ? 0.8f : 1.0f);
                
                revenue = currentSkill * dept.BaseRevenuePerSkillPoint * dept.Level * industryMult * traitMult;
            }

            GUI.Box(new Rect(0, y, viewRect.width, rowHeight - 5), "");
            
            // First Row: Name and Personality
            GUI.Label(new Rect(5, y + 5, 120, 20), $"<b>{emp.Name}</b>");
            GUI.contentColor = Color.cyan;
            GUI.Label(new Rect(130, y + 5, 100, 20), $"{emp.Personality}");
            GUI.contentColor = Color.white;

            // Second Row: Team
            GUI.contentColor = Color.yellow;
            GUI.Label(new Rect(130, y + 25, 100, 15), $"[{emp.TeamName}]");
            GUI.contentColor = Color.white;

            // Skill List (starts from left side, below name)
            float sy = y + 30;
            foreach(var kvp in emp.Skills)
            {
                string prefix = (kvp.Key == currentIndName) ? ">> " : "";
                string suffix = (kvp.Key == currentIndName) ? " (Active)" : "";
                
                if(kvp.Key == currentIndName) GUI.contentColor = Color.green;
                GUI.Label(new Rect(10, sy, 220, 20), $"{prefix}{kvp.Key}: {kvp.Value}{suffix}");
                GUI.contentColor = Color.white;
                sy += 20;
            }

            // Stats
            float bottomY = y + rowHeight - 45;
            GUI.Label(new Rect(5, bottomY, 150, 20), $"Wage: ${emp.DailyWage}/d");
            
            GUI.contentColor = Color.green;
            GUI.Label(new Rect(5, bottomY + 20, 150, 20), $"Rev: +${revenue:F0}");
            GUI.contentColor = Color.white;

            GUI.backgroundColor = Color.red;
            if (GUI.Button(new Rect(160, bottomY, 50, 40), "FIRE"))
            {
                ws.FireEmployee();
            }
            GUI.backgroundColor = Color.yellow;
            if (GUI.Button(new Rect(160, bottomY - 45, 50, 40), "MOVE"))
            {
                MoveEmployee(ws);
            }
            GUI.backgroundColor = Color.cyan;
            if (GUI.Button(new Rect(160, bottomY - 90, 50, 40), "TEAM"))
            {
                employeeToAssign = emp;
                showTeamAssignModal = true;
            }
            GUI.backgroundColor = Color.white;

            y += rowHeight;
        }

        GUI.EndScrollView();
    }
    
    void DrawTeamPanel()
    {
        GUI.Box(teamPanelRect, "Teams");

        if (GUI.Button(new Rect(teamPanelRect.x + 10, teamPanelRect.y + 25, 130, 25), "+ New Team"))
        {
            showTeamCreationModal = true;
        }

        float contentHeight = GameManager.Instance.Teams.Count * 30;
        Rect viewRect = new Rect(0, 0, teamPanelRect.width - 20, contentHeight);
        Rect scrollRect = new Rect(teamPanelRect.x + 5, teamPanelRect.y + 55, teamPanelRect.width - 10, teamPanelRect.height - 60);

        teamScrollPosition = GUI.BeginScrollView(scrollRect, teamScrollPosition, viewRect);

        float y = 0;
        foreach(var team in GameManager.Instance.Teams)
        {
            GUI.Label(new Rect(5, y, viewRect.width - 10, 25), $"{team.Name} ({team.MemberNames.Count})");
            y += 30;
        }

        GUI.EndScrollView();
    }

    void DrawTeamCreationModal()
    {
        float w = 300;
        float h = 150;
        float x = Screen.width / 2 - w / 2;
        float y = Screen.height / 2 - h / 2;

        GUI.Box(new Rect(x, y, w, h), "Create New Team");
        
        GUI.Label(new Rect(x + 20, y + 40, 100, 20), "Team Name:");
        newTeamName = GUI.TextField(new Rect(x + 20, y + 65, w - 40, 25), newTeamName, 20);

        if (GUI.Button(new Rect(x + 20, y + 100, 120, 30), "Create"))
        {
            if (!string.IsNullOrEmpty(newTeamName))
            {
                GameManager.Instance.CreateTeam(newTeamName);
                newTeamName = "";
                showTeamCreationModal = false;
            }
        }

        if (GUI.Button(new Rect(x + 160, y + 100, 120, 30), "Cancel"))
        {
            newTeamName = "";
            showTeamCreationModal = false;
        }
    }

    void DrawTeamAssignModal()
    {
        float w = 300;
        float h = 300;
        float x = Screen.width / 2 - w / 2;
        float y = Screen.height / 2 - h / 2;

        GUI.Box(new Rect(x, y, w, h), $"Assign {employeeToAssign.Name}");

        GUI.Label(new Rect(x + 20, y + 30, w - 40, 20), "Select Team:");

        float btnY = y + 60;
        foreach(var team in GameManager.Instance.Teams)
        {
            if (GUI.Button(new Rect(x + 20, btnY, w - 40, 30), team.Name))
            {
                // Remove from old team
                Team oldTeam = GameManager.Instance.GetTeam(employeeToAssign.TeamName);
                if (oldTeam != null) oldTeam.RemoveMember(employeeToAssign.Name);

                // Add to new team
                employeeToAssign.TeamName = team.Name;
                team.AddMember(employeeToAssign.Name);

                showTeamAssignModal = false;
                Debug.Log($"Assigned {employeeToAssign.Name} to {team.Name}");
            }
            btnY += 35;
        }

        if (GUI.Button(new Rect(x + 80, y + h - 40, 140, 30), "Cancel"))
        {
            showTeamAssignModal = false;
        }
    }
    
    void MoveEmployee(Workstation currentWs)
    {
        Department[] depts = FindObjectsOfType<Department>();
        foreach(var d in depts)
        {
            if (d == currentWs.GetComponentInParent<Department>()) continue; // Skip current dept
            if (!d.IsUnlocked) continue;
            
            if (d.HasVacancy())
            {
                // Find vacant desk
                foreach(var targetWs in d.Workstations)
                {
                    if (targetWs.HasComputer && !targetWs.HasEmployee)
                    {
                        // Transfer Data
                        EmployeeData data = currentWs.CurrentEmployee;
                        currentWs.FireEmployee(); // Remove from old (Visuals handled in Fire)
                        targetWs.HireEmployee(data); // Add to new
                        Debug.Log($"Moved {data.Name} to {d.DeptName}");
                        return;
                    }
                }
            }
        }
        Debug.Log("No other vacancy found!");
    }
}
