using System;

namespace SealTest
{
    public class DateTimeEx
    {
        public static DateTime UtcNowRound
        {
            get
            {
                var n = DateTime.UtcNow;
                return new DateTime(n.Year, n.Month, n.Day, n.Hour, n.Minute, n.Second);
            }
        }
    }
}