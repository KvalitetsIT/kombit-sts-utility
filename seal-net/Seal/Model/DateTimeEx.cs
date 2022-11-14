using System;

namespace dk.nsi.seal.Model
{
    public static class DateTimeEx
    {
        public static DateTime UtcNowRound
        {
            get
            {
                var n = DateTime.UtcNow;
                return new DateTime(n.Year, n.Month, n.Day, n.Hour, n.Minute, n.Second);
            }
        }

        public static DateTime RoundFiveMinutesAgoUtc
        {
            get
            {
                var n = DateTime.UtcNow.AddMinutes(-5);
                return new DateTime(n.Year, n.Month, n.Day, n.Hour, n.Minute, n.Second);
            }
        }

	    public static string FormattedDateTimeNow => UtcNowRound.ToString("u").Replace(' ', 'T');

	    public static string FormatDateTimeXml(this DateTime dateTime)
	    {
		    return dateTime.ToString("u").Replace(' ', 'T');
		}
    }
}
