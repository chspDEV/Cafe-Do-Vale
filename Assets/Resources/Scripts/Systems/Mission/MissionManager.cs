using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

public class MissionManager : MonoBehaviour
{
    public static MissionManager Instance { get; private set; }
    [SerializeField] private List<Mission> missions = new List<Mission>();
    [SerializeField] private MissionUI missionUI;

    private int currentMissionIndex = 0;
    private bool isMissionActive = false;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        if (missions.Count > 0)
        {
            StartMission(missions[0]);
        }
    }

    private void Update()
    {
        if (isMissionActive)
        {
            Mission currentMission = missions[currentMissionIndex];
            if (currentMission.CheckCompletion())
            {
                CompleteCurrentMission();
            }
        }
    }

    private void StartMission(Mission mission)
    {
        isMissionActive = true;
        missionUI.UpdateMissionUI(mission);
    }

    private void CompleteCurrentMission()
    {
        Mission completedMission = missions[currentMissionIndex];
        completedMission.OnComplete();

        isMissionActive = false;
        missionUI.ShowMissionComplete(completedMission);

        currentMissionIndex++;

        if (currentMissionIndex < missions.Count)
        {
            StartMission(missions[currentMissionIndex]);
        }
        else
        {
            missionUI.ShowAllMissionsComplete();
        }
    }

    public void AddMission(Mission mission)
    {
        missions.Add(mission);
        if (!isMissionActive)
        {
            StartMission(mission);
        }
    }

    public Mission GetCurrentMission()
    {
        if (currentMissionIndex < missions.Count)
        {
            return missions[currentMissionIndex];
        }
        return null;
    }
}