using UnityEngine;
using System.Collections.Generic;

public class GameEvent
{
    public string Title;
    public string Description;
    public List<EventOption> Options = new List<EventOption>();
    public bool IsNegative; 

    public GameEvent(string t, string d, bool neg) { Title = t; Description = d; IsNegative = neg; }
}

public class EventOption
{
    public string Text;
    public System.Action OnSelect;

    public EventOption(string text, System.Action action) { Text = text; OnSelect = action; }
}

public class EventManager : MonoBehaviour
{
    public static EventManager Instance { get; private set; }

    public float EventIntervalHours = 12f; // Every 12 game hours
    private float nextEventTime = 12f;

    public GameEvent CurrentEvent;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        // First event at 12:00 Day 1
        nextEventTime = 12f;
    }

    private void Update()
    {
        if (GameManager.Instance.IsGameOver) return;
        if (CurrentEvent != null) return; // Wait for player to handle current event

        // Check time from GameManager
        // GameManager TimeOfDay loops 0-24. We need to track cumulative or check daily 12h/24h.
        // Let's check against GameManager TimeOfDay passing thresholds?
        // Simpler: Just accumulate pure delta time in EventManager independent of Day cycle, 
        // converting game hours to seconds?
        // GameManager: 1 real sec = 1 game hour (TimeScale=1)
        
        // Wait, GameManager loops TimeOfDay. 
        // Let's trigger roughly halfway through day (12:00) and Start of day (0:00/24:00)? 
        // User said "about half a day". Let's do it at 13:00 daily (Lunch Surprise).
        
        float time = GameManager.Instance.TimeOfDay;
        if (Mathf.Abs(time - 13f) < 0.1f && !eventTriggeredToday)
        {
            TriggerRandomEvent();
            eventTriggeredToday = true;
        }

        if (time < 1f) eventTriggeredToday = false; // Reset for new day
    }

    private bool eventTriggeredToday = false;

    public void TriggerRandomEvent()
    {
        float multiplier = GameManager.Instance.CurrentIndustry.Multiplier;
        
        // Risk Calculation
        // Base 10% + 50% for every 1.0 over 1.0 multiplier
        // 1.0 -> 10%
        // 1.5 -> 10 + 25 = 35%
        // 2.0 -> 10 + 50 = 60%
        float badChance = 0.1f + (multiplier - 1.0f) * 0.5f;
        
        bool isBad = Random.value < badChance;

        if (isBad)
            CreateBadEvent();
        else
            CreateGoodEvent();
            
        UIManager.Instance.ShowEventModal(CurrentEvent);
        Time.timeScale = 0f; // PAUSE GAME
    }

    void CreateBadEvent()
    {
        int r = Random.Range(0, 5); // Increased to include 3 and 4
        switch (r)
        {
            case 0:
                CurrentEvent = new GameEvent("Server Crash", "Your servers are down! This Industry is unstable.", true);
                CurrentEvent.Options.Add(new EventOption("Emergency Repair (-$500)", () => {
                    GameManager.Instance.SpendMoney(500);
                    CloseEvent();
                }));
                CurrentEvent.Options.Add(new EventOption("Ignore (Lost Revenue)", () => {
                    // Logic to lose revenue
                    GameManager.Instance.AddMoney(-200f); 
                    CloseEvent();
                }));
                break;
            case 1:
                CurrentEvent = new GameEvent("Market Correction", "The market bubble popped slightly.", true);
                CurrentEvent.Options.Add(new EventOption("Liquidate Assets (-$300)", () => {
                    GameManager.Instance.SpendMoney(300);
                    CloseEvent();
                }));
                break;
            case 2:
                CurrentEvent = new GameEvent("Hacker Attack", "Security breach detected!", true);
                CurrentEvent.Options.Add(new EventOption("Pay Ransom (-$800)", () => {
                    GameManager.Instance.SpendMoney(800);
                    CloseEvent();
                }));
                CurrentEvent.Options.Add(new EventOption("Fight Back (50% chance to save money)", () => {
                    if(Random.value > 0.5f) { /* Saved! */ }
                    else { GameManager.Instance.AddMoney(-1000f); Debug.Log("Failed defense!"); }
                    CloseEvent();
                }));
                break;
            case 3:
                // NEW: Office Theft
                CurrentEvent = new GameEvent("Office Theft", "Thieves broke in and stole equipment!", true);
                CurrentEvent.Options.Add(new EventOption("Buy Replacements (-$400)", () => {
                     GameManager.Instance.AddMoney(-400f);
                     CloseEvent();
                }));
                CurrentEvent.Options.Add(new EventOption("Claim Insurance (Get $200 back)", () => {
                     GameManager.Instance.AddMoney(200f - 400f); // Net -200 but maybe premiums go up? Simplified for now.
                     // Or maybe chance to be rejected?
                     if (Random.value > 0.3f) { Debug.Log("Claim Approved"); }
                     else { GameManager.Instance.AddMoney(-100f); Debug.Log("Claim Partially Denied! Lost extra time."); }
                     CloseEvent();
                }));
                break;
            case 4:
                // NEW: Power Outage
                CurrentEvent = new GameEvent("Power Outage", "The building has lost power.", true);
                CurrentEvent.Options.Add(new EventOption("Call Electrician (-$300)", () => {
                     GameManager.Instance.AddMoney(-300f);
                     CloseEvent();
                }));
                CurrentEvent.Options.Add(new EventOption("Wait (Lose 4 Hours Revenue)", () => {
                     // Simulate loss of revenue? 
                     // Or just advance time without revenue? 
                     // Let's just deduct revenue equivalent.
                     float lostRev = GameManager.Instance.CalculateProjectedDailyRevenue() * 0.4f; // ~40% of day
                     GameManager.Instance.AddMoney(-lostRev);
                     CloseEvent();
                }));
                break;
        }
    }

    void CreateGoodEvent()
    {
        int r = Random.Range(0, 4); // Added case 3
        switch (r)
        {
            case 0:
                CurrentEvent = new GameEvent("Angel Investor", "An investor likes your pivot.", false);
                CurrentEvent.Options.Add(new EventOption("Accept Funding (+$500)", () => {
                    GameManager.Instance.AddMoney(500);
                    CloseEvent();
                }));
                break;
            case 1:
                CurrentEvent = new GameEvent("Viral Product", "Everyone loves what you do.", false);
                CurrentEvent.Options.Add(new EventOption("Nice! (+$300)", () => {
                    GameManager.Instance.AddMoney(300);
                    CloseEvent();
                }));
                break;
            case 2:
                CurrentEvent = new GameEvent("Tax Rename", "Government grant for tech.", false);
                CurrentEvent.Options.Add(new EventOption("Claim (+$200)", () => {
                    GameManager.Instance.AddMoney(200);
                    CloseEvent();
                }));
                break;
            case 3:
                // NEW: Hard Choice Event
                CurrentEvent = new GameEvent("Corporate Restructuring", "Consultants suggest 'Trimming the fat' to boost profits.", false);
                CurrentEvent.Options.Add(new EventOption("Refuse (No Change)", () => {
                    CloseEvent();
                }));
                CurrentEvent.Options.Add(new EventOption("Execute Cuts (+$1000, Fire 1 Staff)", () => {
                    GameManager.Instance.AddMoney(1000);
                    FireRandomEmployee(); // Fire someone
                    CloseEvent();
                }));
                break;
        }
    }

    void FireRandomEmployee()
    {
        Department[] depts = FindObjectsOfType<Department>();
        List<Workstation> activeStats = new List<Workstation>();
        
        foreach(var d in depts)
        {
            foreach(var ws in d.Workstations)
            {
                if(ws.IsProductive()) activeStats.Add(ws);
            }
        }

        if (activeStats.Count > 0)
        {
            // Pick random
            int r = Random.Range(0, activeStats.Count);
            Workstation victim = activeStats[r];
            
            Debug.Log($"Restructuring fired: {victim.CurrentEmployee.Name}");
            victim.FireEmployee();
        }
        else
        {
            Debug.Log("No one to fire!");
        }
    }

    public void CloseEvent()
    {
        CurrentEvent = null;
        UIManager.Instance.HideEventModal();
        Time.timeScale = 1f; // RESUME
    }
}
