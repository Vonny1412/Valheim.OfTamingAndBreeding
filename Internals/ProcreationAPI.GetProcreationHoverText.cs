using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OfTamingAndBreeding.Internals
{
    internal partial class ProcreationAPI
    {

        public IReadOnlyList<string> GetProcreationHoverText()
        {
            var zdo = m_nview?.GetZDO();
            if (zdo == null)
                return Array.Empty<string>();

            var returnLines = new List<string>(capacity: 1);
            var L = Localization.instance;

            if (IsPregnant())
            {
                AddPregnancyLinesIfEnabled(returnLines, zdo, L);
                return returnLines; // pregnant -> no lovepoints line
            }

            AddLovePointsLinesIfEnabled(returnLines, L);
            return returnLines;
        }

        private void AddPregnancyLinesIfEnabled(List<string> returnLines, ZDO zdo, Localization L)
        {
            if (!Plugin.Configs.HoverShowPregnancy.Value)
                return;

            long pregnantLong = zdo.GetLong(ZDOVars.s_pregnant, 0L);
            if (!Plugin.Configs.HoverShowPregnancyTimer.Value || pregnantLong == 0L)
            {
                returnLines.Add(string.Format(
                    L.Localize("$otab_hover_pregnancy"),
                    Plugin.Configs.HoverColorGood.Value
                ));
                return;
            }

            var zTime = ZNet.instance.GetTime();
            var dateTime = new DateTime(pregnantLong);
            var duration = _realPregnancyDuration;
            double secLeft = duration - (zTime - dateTime).TotalSeconds;

            returnLines.Add(Helpers.StringHelper.FormatRelativeTime(
                secLeft,
                labelPositive: L.Localize("$otab_hover_pregnancy_due"),
                labelNegative: L.Localize("$otab_hover_pregnancy_overdue"),
                labelAltPositive: L.Localize("$otab_hover_pregnancy_due_alt"),
                labelAltNegative: L.Localize("$otab_hover_pregnancy_overdue_alt"),
                colorPositive: Plugin.Configs.HoverColorGood.Value,
                colorNegative: Plugin.Configs.HoverColorBad.Value
            ));
        }

        private void AddLovePointsLinesIfEnabled(List<string> returnLines, Localization L)
        {
            if (!Plugin.Configs.HoverShowLovePoints.Value)
                return;

            string partnerName = TryGetPartnerName();
            int lPoints = GetLovePoints();

            var color = lPoints > 0
                ? Plugin.Configs.HoverColorGood.Value
                : Plugin.Configs.HoverColorBad.Value;

            if (!string.IsNullOrEmpty(partnerName))
            {
                returnLines.Add(string.Format(
                    L.Localize("$otab_hover_love_points_with_partner"),
                    color,
                    lPoints,
                    m_requiredLovePoints,
                    L.Localize(partnerName)
                ));
            }
            else
            {
                returnLines.Add(string.Format(
                    L.Localize("$otab_hover_love_points"),
                    color,
                    lPoints,
                    m_requiredLovePoints
                ));
            }
        }

        private string TryGetPartnerName()
        {
            if (m_seperatePartner == null)
                return null;

            var partnerCharacter = m_seperatePartner.GetComponent<Character>();
            if (partnerCharacter == null)
                return null;

            return partnerCharacter.m_name;
        }


    }
}
