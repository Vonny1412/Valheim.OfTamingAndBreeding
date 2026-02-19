using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OfTamingAndBreeding.Helpers
{

    internal static class StringHelper
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
                timeString += String.Format(L.Localize("$otab_hover_time_days"), color, ts.Days);
            if (ts.Hours > 0 || timeString != "")
                timeString += String.Format(L.Localize("$otab_hover_time_hours"), color, ts.Hours);
            if (ts.Minutes > 0 || timeString != "")
                timeString += String.Format(L.Localize("$otab_hover_time_minutes"), color, ts.Minutes);
            if (Plugin.Configs.HoverShowSeconds.Value)
                timeString += String.Format(L.Localize("$otab_hover_time_seconds"), color, ts.Seconds);


            return timeString != ""
                ? String.Format(label, color, timeString.Trim())
                : String.Format(labelAlt, color);
        }

    }




}
