using System;

namespace OfTamingAndBreeding.OTABUtils
{
    internal static class StringUtils
    {

        public static string FormatRelativeTime(
            double secondsLeft,
            string labelPositive,
            string labelPositiveAlt,
            string labelNegative,
            string labelNegativeAlt,
            string colorPositive,
            string colorNegative)
        {
            bool isNegative = secondsLeft < 0;
            TimeSpan ts = TimeSpan.FromSeconds(Math.Abs(secondsLeft));

            string color = isNegative ? colorNegative : colorPositive;
            string label = isNegative ? labelNegative : labelPositive;
            string labelAlt = isNegative ? labelNegativeAlt : labelPositiveAlt;

            string timeString = "";
            var L = Localization.instance;

            if (Plugin.Configs.HoverUseIngameTime.Value)
            {
                double ingameSecondsPerDay = EnvMan.instance.m_dayLengthSec; // z.B. 1800 oder 3600
                double totalSeconds = ts.TotalSeconds;

                int days = (int)(totalSeconds / ingameSecondsPerDay);
                totalSeconds -= days * ingameSecondsPerDay;

                int hours = (int)(totalSeconds / 3600.0);
                totalSeconds -= hours * 3600.0;

                int minutes = (int)(totalSeconds / 60.0);
                int seconds = (int)(totalSeconds % 60.0);

                ts = new TimeSpan(days, hours, minutes, seconds);
            }

            if (ts.Days > 0)
            {
                var tsDays = ts.Days.ToString();
                timeString += L.Localize("$otab_hover_time_days", color, tsDays);
            }
            var timeFormat = L.Localize("$otab_hover_time_format");
            if (ts.Hours > 0 || timeString != "")
            {
                var tsHours = String.Format(timeFormat, ts.Hours);
                timeString += L.Localize("$otab_hover_time_hours", color, tsHours);
            }
            if (ts.Minutes > 0 || timeString != "")
            {
                var tsMinutes = String.Format(timeFormat, ts.Minutes);
                timeString += L.Localize("$otab_hover_time_minutes", color, tsMinutes);
            }
            if (Plugin.Configs.HoverShowSeconds.Value)
            {
                var tsSeconds = String.Format(timeFormat, ts.Seconds);
                timeString += L.Localize("$otab_hover_time_seconds", color, tsSeconds);
            }


            return timeString != ""
                ? L.Localize(label, color, timeString.Trim())
                : L.Localize(labelAlt, color);
        }

    }
}
