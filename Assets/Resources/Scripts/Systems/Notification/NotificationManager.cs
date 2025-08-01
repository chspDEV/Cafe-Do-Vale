using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Collections;
using Tcp4.Assets.Resources.Scripts.Managers;

public class NotificationManager : MonoBehaviour
{
    public static NotificationManager Instance;

    [Header("Notification System")]
    [SerializeField] private Transform notificationHolder;
    [SerializeField] private GameObject pfNotification;
    [SerializeField] private float notificationDuration = 30f;
    [SerializeField] private GameObject notificationMenu;

    [Header("Button")]
    [SerializeField] private Button toggleButton;
    [SerializeField] private Sprite spriteNormal;
    [SerializeField] private Sprite spriteNew;

    [Header("Popup")]
    [SerializeField] private Transform popupSpawnAnchor;
    [SerializeField] private float popupDuration = 2f;
    private bool isShowingPopup = false;

    private readonly List<Notification> notifications = new();

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    public void Show(string title, string message, Sprite icon = null)
    {
        Notification notif = new Notification
        {
            title = title,
            message = message,
            icon = icon,
            timestamp = $"{TimeManager.Instance.CurrentHour:00}:{System.DateTime.Now.Minute:00}",
            isRead = false
        };

        notifications.Add(notif);

        GameObject go = Instantiate(pfNotification, notificationHolder);
        go.transform.SetAsFirstSibling();

        NotificationUI notifUI = go.GetComponent<NotificationUI>();
        notifUI.Setup(notif);

        UpdateButtonVisual();

        if (!notificationMenu.activeSelf)
            StartCoroutine(ShowPopup(title, message, icon));

        StartCoroutine(AutoDestroy(go, notificationDuration));
    }

    private IEnumerator AutoDestroy(GameObject go, float delay)
    {
        yield return new WaitForSeconds(delay);
        if (go != null) Destroy(go);
    }

    public void ToggleNotificationMenu()
    {
        bool isActive = notificationMenu.activeSelf;
        notificationMenu.SetActive(!isActive);

        if (!isActive)
        {
            MarkAllAsRead();
            UpdateButtonVisual();
        }
    }

    private void MarkAllAsRead()
    {
        foreach (var notif in notifications)
        {
            notif.isRead = true;
        }
    }

    private void UpdateButtonVisual()
    {
        bool hasUnread = notifications.Exists(n => !n.isRead);
        if (toggleButton != null)
        {
            Image img = toggleButton.GetComponent<Image>();
            img.sprite = hasUnread ? spriteNew : spriteNormal;
        }
    }

    private IEnumerator ShowPopup(string title, string message, Sprite icon)
    {
        while (isShowingPopup)
            yield return new WaitForSeconds(5f);

        isShowingPopup = true;

        yield return new WaitForSeconds(0.5f);

        GameObject popupGO = Instantiate(pfNotification, popupSpawnAnchor);
        popupGO.transform.SetAsLastSibling();

        Notification notif = new Notification
        {
            title = title,
            message = message,
            icon = icon,
            timestamp = $"{TimeManager.Instance.CurrentHour:00}:{System.DateTime.Now.Minute:00}",
            isRead = false
        };

        NotificationUI notifUI = popupGO.GetComponent<NotificationUI>();
        notifUI.Setup(notif);

        popupGO.SetActive(true);

        yield return new WaitForSeconds(popupDuration);

        Destroy(popupGO);
        isShowingPopup = false;
    }

}
