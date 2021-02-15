using System;
using Unity.Notifications.Android;
using UnityEngine;

public class DailyNotifications : MonoBehaviour
{
    public string RuNotificationHeader;
    public string RuNotificationText;
    public string EnNotificationHeader;
    public string EnNotificationText;
    private string NotificationHeader;
    private string NotificationText;

    private void Start()
    {
        try
        {
            var c = new AndroidNotificationChannel
            {
                Id = "channel_id",
                Name = "Default Channel",
                Importance = Importance.High,
                Description = "Generic notifications",
            };

            AndroidNotificationCenter.RegisterNotificationChannel(c);
        }
        catch (Exception e)
        {
            Debug.Log(e);
        }
        
        if (PlayerPrefs.GetInt("Language") == 1)
        {
            NotificationHeader = RuNotificationHeader;
            NotificationText = RuNotificationText;
        }
        else
        {
            NotificationHeader = EnNotificationHeader;
            NotificationText = EnNotificationText;
        }
        ResetScheduledNotifications();
    }
    private void ResetScheduledNotifications()
    {
        var notification = new AndroidNotification(NotificationHeader, NotificationText,
                DateTime.Now.AddHours(8).AddMinutes(15), new TimeSpan(8, 15, 0), "icon_0");
        //Если оповещения не установлены
        if (!PlayerPrefs.HasKey("NotificationID"))
        {
            try
            {
                var id = AndroidNotificationCenter.SendNotification(notification, "channel_id");
                PlayerPrefs.SetInt("NotificationID", id);
            }
            catch { }
        }
    }
}
