using UnityEngine;
using Tcp4;

[System.Serializable]
public class Notification
{
    public string title;
    public string message;
    public Sprite icon;
    public string timestamp;
    public bool isRead;
}
