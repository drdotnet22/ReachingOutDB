namespace ReachingOutDB.Data
{
    public class DateTimeUtilities
    {
        public string ToNewYorkTimeString(DateTime utcTime)
        {
            var tz = TimeZoneInfo.FindSystemTimeZoneById("America/New_York");
            var nyTime = TimeZoneInfo.ConvertTimeFromUtc(utcTime, tz);
            return nyTime.ToString("M/d/yyyy hh:mm tt");
        }

        public DateTime ToNewYorkTime(DateTime utcTime)
        {
            var tz = TimeZoneInfo.FindSystemTimeZoneById("America/New_York");
            return TimeZoneInfo.ConvertTimeFromUtc(utcTime, tz);
        }

        public DateTime ToUtcTime(DateTime localTime)
        {
            var tz = TimeZoneInfo.FindSystemTimeZoneById("America/New_York");
            return TimeZoneInfo.ConvertTimeToUtc(localTime, tz);
        }
    }
}
