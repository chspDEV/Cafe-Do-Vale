using Tcp4;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class NotificationUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI titleText;
    [SerializeField] private TextMeshProUGUI messageText;
    [SerializeField] private TextMeshProUGUI timeText;
    [SerializeField] private Image iconImage;

    public void Setup(Notification notif)
    {
        titleText.text = notif.title;
        messageText.text = notif.message;
        timeText.text = notif.timestamp;
        iconImage.sprite = notif.icon;
    }
}
