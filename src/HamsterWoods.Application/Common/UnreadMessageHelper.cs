using System;
using System.Collections.Generic;
using System.Linq;
using HamsterWoods.DeviceInfo;
using HamsterWoods.Entities.Redis;
using Volo.Abp;

namespace HamsterWoods.Common;

public static class UnreadMessageHelper
{
    public static string GetId(string userId, string messageType)
    {
        if (userId.IsNullOrEmpty() || messageType.IsNullOrEmpty())
        {
            throw new UserFriendlyException("invalid params");
        }

        return $"{userId}-{messageType}";
    }

    public static int GetUnreadCount(UnreadMessage unreadMessages)
    {
        if (unreadMessages == null) return 0;

        return unreadMessages.UnreadCount;
    }

    public static int GetUnreadCount(List<UnreadMessage> unreadMessages, MessageType messageType)
    {
        if (unreadMessages.IsNullOrEmpty()) return 0;

        var count = unreadMessages
            .FirstOrDefault(t => t.MessageType.Equals(messageType.ToString(), StringComparison.OrdinalIgnoreCase))
            ?.UnreadCount;

        return count ?? 0;
    }
}