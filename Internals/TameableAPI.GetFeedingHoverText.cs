using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace OfTamingAndBreeding.Internals
{

    internal partial class TameableAPI
    {
        public IReadOnlyList<string> GetFeedingHoverText()
        {
            var zdo = m_nview?.GetZDO();
            if (zdo == null)
                return Array.Empty<string>();

            var L = Localization.instance;
            var zTime = ZNet.instance.GetTime();

            var returnLines = new List<string>(capacity: 2);

            AddFedTimerLineIfEnabled(returnLines, zdo, zTime, L);

            return returnLines;
        }

        private void AddFedTimerLineIfEnabled(List<string> returnLines, ZDO zdo, DateTime zTime, Localization L)
        {
            if (!Plugin.Configs.HoverShowFedTimer.Value)
                return;

            if (m_fedDuration <= 0)
                return;

            long lastFedTimeLong = zdo.GetLong(ZDOVars.s_tameLastFeeding, 0L);

            double secLeft;
            if (lastFedTimeLong == 0)
            {
                // never fed -> treat as "starving since spawn"
                var baseAI = GetComponent<BaseAI>(); // need to use baseAI for tameable animals
                secLeft = -baseAI.GetTimeSinceSpawned().TotalSeconds;
            }
            else
            {
                var lastFedTime = new DateTime(lastFedTimeLong);
                secLeft = m_fedDuration - (zTime - lastFedTime).TotalSeconds;
            }

            if (!(secLeft > 0 || Plugin.Configs.HoverShowFedTimerStarving.Value))
                return;

            returnLines.Add(Helpers.StringHelper.FormatRelativeTime(
                secLeft,
                labelPositive: L.Localize("$otab_hover_fed"),
                labelNegative: L.Localize("$otab_hover_starving"),
                labelAltPositive: L.Localize("$otab_hover_fed_alt"),
                labelAltNegative: L.Localize("$otab_hover_starving_alt"),
                colorPositive: Plugin.Configs.HoverColorGood.Value,
                colorNegative: Plugin.Configs.HoverColorBad.Value
            ));
        }

    }
}
