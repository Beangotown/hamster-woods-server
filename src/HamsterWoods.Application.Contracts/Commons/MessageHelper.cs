using System;
using System.Collections.Generic;
using FirebaseAdmin.Messaging;

namespace HamsterWoods.Commons;

public static class MessageHelper
{
    public static string GetId(string userId, string deviceId)
    {
        if (deviceId.IsNullOrEmpty() || userId.IsNullOrEmpty()) return string.Empty;

        return $"{userId}-{deviceId}";
    }

    public static Notification GetNotification(string title, string content, string image)
    {
        return new Notification()
        {
            Title = title,
            Body = content,
            ImageUrl = image,
        };
    }

    public static AndroidConfig GetAndroidConfig(int badge)
    {
        return new AndroidConfig()
        {
            Notification = new AndroidNotification()
            {
                // Icon = icon,
                NotificationCount = badge
            }
        };
    }

    public static ApnsConfig GetApnsConfig(int badge)
    {
        return new ApnsConfig()
        {
            CustomData = new Dictionary<string, object>()
            {
                ["badge"] = badge,
                ["aps"] = new Aps()
                {
                    Badge = badge
                }
            }
        };
    }
    
    public static WebpushConfig GetWebPushConfig(int badge)
    {
        return new WebpushConfig()
        {
            Notification = new WebpushNotification()
            {
                Badge = badge.ToString()
            }
        };
    }
}