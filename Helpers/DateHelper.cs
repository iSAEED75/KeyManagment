using System.Globalization;

namespace KeyManagment.Helpers
{
    public static class DateHelper
    {
        private static readonly PersianCalendar _pc = new PersianCalendar();

        public static string ToShamsi(this DateTime dt)
        {
            int year = _pc.GetYear(dt);
            int month = _pc.GetMonth(dt);
            int day = _pc.GetDayOfMonth(dt);
            return $"{year}/{month:00}/{day:00}";
        }

        public static string ToShamsiWithTime(this DateTime dt)
        {
            int year = _pc.GetYear(dt);
            int month = _pc.GetMonth(dt);
            int day = _pc.GetDayOfMonth(dt);
            return $"{year}/{month:00}/{day:00} — {dt:HH:mm}";
        }

        public static string? ToShamsiWithTime(this DateTime? dt)
        {
            if (dt == null) return null;
            return dt.Value.ToShamsiWithTime();
        }

        public static string? ToShamsi(this DateTime? dt)
        {
            if (dt == null) return null;
            return dt.Value.ToShamsi();
        }
    }
}