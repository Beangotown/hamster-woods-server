using System;

namespace HamsterWoods.Rank;

public static class SeasonWeekHelper
{
    public static int GetRankWeekNum(RankSeasonConfigIndex rankSeasonIndex, DateTime compareTime)
    {
        if (rankSeasonIndex != null)
        {
            var i = 0;
            foreach (var weekInfo in rankSeasonIndex.WeekInfos)
            {
                i++;
                if (compareTime.CompareTo(weekInfo.RankBeginTime) > -1 &&
                    weekInfo.RankEndTime.CompareTo(compareTime) > -1)
                    return i;
            }
        }

        return -1;
    }

    public static int GetShowWeekNum(RankSeasonConfigIndex rankSeasonIndex, DateTime compareTime)
    {
        if (rankSeasonIndex != null)
        {
            var i = 0;
            foreach (var weekInfo in rankSeasonIndex.WeekInfos)
            {
                i++;
                if (compareTime.CompareTo(weekInfo.ShowBeginTime) > -1 &&
                    weekInfo.ShowEndTime.CompareTo(compareTime) > -1)
                    return i;
            }
        }

        return -1;
    }

    public static int GetWeekNum(RankSeasonConfigIndex rankSeasonIndex, DateTime compareTime)
    {
        return Math.Max(GetRankWeekNum(rankSeasonIndex, compareTime), GetShowWeekNum(rankSeasonIndex, compareTime));
    }

    public static void GetWeekStatusAndRefreshTime(RankSeasonConfigIndex rankSeasonIndex, DateTime compareTime,
        out int status, out DateTime? refreshTime)
    {
        var showWeek = GetShowWeekNum(rankSeasonIndex, compareTime);
        var rankWeek = GetRankWeekNum(rankSeasonIndex, compareTime);
        if (rankWeek > -1)
        {
            status = 0;
            refreshTime = rankSeasonIndex.WeekInfos[rankWeek - 1].RankEndTime;
            return;
        }

        if (showWeek > -1)
        {
            if (rankSeasonIndex.WeekInfos.Count == showWeek)
            {
                status = 2;
                refreshTime = null;
                return;
            }

            status = 1;
            refreshTime = rankSeasonIndex.WeekInfos[showWeek].RankBeginTime;
            return;
        }

        status = 0;
        refreshTime = null;
    }

    public static void GetSeasonStatusAndRefreshTime(RankSeasonConfigIndex rankSeasonIndex, DateTime compareTime,
        out int status, out DateTime? refreshTime)
    {
        var showWeek = GetShowWeekNum(rankSeasonIndex, compareTime);
        var rankWeek = GetRankWeekNum(rankSeasonIndex, compareTime);
        if (rankWeek > -1)
        {
            status = 0;
            refreshTime = rankSeasonIndex.WeekInfos[rankWeek - 1].ShowBeginTime;
            return;
        }

        if (showWeek > -1)
        {
            if (rankSeasonIndex.WeekInfos.Count == showWeek)
            {
                status = 1;
                refreshTime = null;
                return;
            }

            status = 0;
            refreshTime = rankSeasonIndex.WeekInfos[showWeek].ShowBeginTime;
            return;
        }

        status = 0;
        refreshTime = null;
    }
}