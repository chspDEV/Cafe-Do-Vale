using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class MissionUI : MonoBehaviour
{
    [SerializeField] private GameObject missionPanel;
    [SerializeField] private TextMeshProUGUI missionNameText;
    [SerializeField] private TextMeshProUGUI missionDescriptionText;
    [SerializeField] private Image missionIconImage;
    [SerializeField] private GameObject missionCompletePanel;
    [SerializeField] private TextMeshProUGUI allMissionsCompleteText;

    public void UpdateMissionUI(Mission mission)
    {
        missionPanel.SetActive(true);
        missionNameText.text = mission.missionName;
        missionDescriptionText.text = mission.missionDescription;
        missionIconImage.sprite = mission.missionIcon;
    }

    public void ShowMissionComplete(Mission mission)
    {
        missionCompletePanel.SetActive(true);
        // Você pode adicionar mais lógica aqui para exibir a conclusão
        Invoke("HideMissionComplete", 2f);
    }

    private void HideMissionComplete()
    {
        missionCompletePanel.SetActive(false);
    }

    public void ShowAllMissionsComplete()
    {
        allMissionsCompleteText.gameObject.SetActive(true);
        missionPanel.SetActive(false);
    }
}