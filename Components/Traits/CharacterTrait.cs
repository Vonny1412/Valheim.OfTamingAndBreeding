using Jotunn;
using Jotunn.Utils;
using OfTamingAndBreeding.Components.Base;
using OfTamingAndBreeding.Components.Extensions;
using OfTamingAndBreeding.Data.Models.SubData;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static UnityEngine.Networking.UnityWebRequest;

namespace OfTamingAndBreeding.Components.Traits
{
    public class CharacterTrait : OTABComponent<CharacterTrait>
    {

        private struct ConsumableItemDisplay
        {
            public string Name;
            public string Color;
        }

        private static Character lastHoverTarget = null;
        private static float lastHoverUpdateTime = 0;
        private static string lastHoverConsumeText = "";

        [Flags]
        public enum HostilityMask : byte
        {
            None = 0,
            Never = 1,
            Attack = 2,
            Skip = 4,
        }

        public HostilityMask TamedCanAttackPlayer { get; private set; } = HostilityMask.None;
        public HostilityMask TamedCanBeAttackedByPlayer { get; private set; } = HostilityMask.None;
        public HostilityMask TamedCanAttackTamed { get; private set; } = HostilityMask.None;
        public HostilityMask TamedCanBeAttackedByTamed { get; private set; } = HostilityMask.None;
        public HostilityMask TamedCanAttackWild { get; private set; } = HostilityMask.None;
        public HostilityMask TamedCanBeAttackedByWild { get; private set; } = HostilityMask.None;
        public HostilityMask TamedCanAttackGroup { get; private set; } = HostilityMask.None;
        public HostilityMask TamedCanBeAttackedByGroup { get; private set; } = HostilityMask.None;
        public HostilityMask TamedCanAttackFaction { get; private set; } = HostilityMask.None;
        public HostilityMask TamedCanBeAttackedByFaction { get; private set; } = HostilityMask.None;

        // set in Awake
        [NonSerialized] private ZNetView m_nview = null;
        [NonSerialized] private Character m_character = null;
        [NonSerialized] private MonsterAI m_monsterAI = null;
        [NonSerialized] private AnimalAITrait m_animalAITrait = null;
        [NonSerialized] private TameableTrait m_tameableTrait = null;
        [NonSerialized] private ProcreationTrait m_procreationTrait = null;
        [NonSerialized] private BaseAITrait m_baseAITrait = null;
        [NonSerialized] private GrowupTrait m_growupTrait = null;

        // set in registration
        [SerializeField] public bool m_changeGroupWhenTamed = false;
        [SerializeField] public string m_changeGroupWhenTamedTo = "";
        [SerializeField] public bool m_changeFactionWhenTamed = false;
        [SerializeField] public Character.Faction m_changeFactionWhenTamedTo = Character.Faction.Players;
        [SerializeField] public IsEnemyCondition m_tamedCanAttackPlayer = IsEnemyCondition.Default;
        [SerializeField] public IsEnemyCondition m_tamedCanBeAttackedByPlayer = IsEnemyCondition.Default;
        [SerializeField] public IsEnemyCondition m_tamedCanAttackTamed = IsEnemyCondition.Default;
        [SerializeField] public IsEnemyCondition m_tamedCanBeAttackedByTamed = IsEnemyCondition.Default;
        [SerializeField] public IsEnemyCondition m_tamedCanAttackWild = IsEnemyCondition.Default;
        [SerializeField] public IsEnemyCondition m_tamedCanBeAttackedByWild = IsEnemyCondition.Default;
        [SerializeField] public IsEnemyCondition m_tamedCanAttackGroup = IsEnemyCondition.Default;
        [SerializeField] public IsEnemyCondition m_tamedCanBeAttackedByGroup = IsEnemyCondition.Default;
        [SerializeField] public IsEnemyCondition m_tamedCanAttackFaction = IsEnemyCondition.Default;
        [SerializeField] public IsEnemyCondition m_tamedCanBeAttackedByFaction = IsEnemyCondition.Default;

        private void Awake()
        {
            m_nview = GetComponent<ZNetView>();
            m_character = GetComponent<Character>();
            m_monsterAI = GetComponent<MonsterAI>();
            m_animalAITrait = GetComponent<AnimalAITrait>();
            m_tameableTrait = GetComponent<TameableTrait>();
            m_procreationTrait = GetComponent<ProcreationTrait>();
            m_baseAITrait = GetComponent<BaseAITrait>();
            m_growupTrait = GetComponent<GrowupTrait>();

            Register(this);
        }

        private void Start()
        {
            if (m_character.IsTamed())
            {
                SetTamedCharacteristics();
            }
        }

        private void OnDestroy()
        {
            Unregister(this);
        }

        public Character GetCharacter()
        {
            return m_character;
        }

        public bool IsTamed()
        {
            return m_character.IsTamed();
        }

        public void UpdateHostilities()
        {
            bool isHungry = m_tameableTrait && m_tameableTrait.IsHungry();
            bool isStarving = isHungry && m_tameableTrait.IsStarving();

            switch (m_tamedCanAttackPlayer)
            {
                case IsEnemyCondition.Default: TamedCanAttackPlayer = HostilityMask.None; break;
                case IsEnemyCondition.Force: TamedCanAttackPlayer = HostilityMask.Attack; break;
                case IsEnemyCondition.Never: TamedCanAttackPlayer = HostilityMask.Never; break;
                case IsEnemyCondition.WhenFed: TamedCanAttackPlayer = !isHungry ? HostilityMask.Attack : HostilityMask.Skip; break;
                case IsEnemyCondition.WhenHungry: TamedCanAttackPlayer = isHungry ? HostilityMask.Attack : HostilityMask.Skip; break;
                case IsEnemyCondition.WhenStarving: TamedCanAttackPlayer = isStarving ? HostilityMask.Attack : HostilityMask.Skip; break;
            }
            switch (m_tamedCanBeAttackedByPlayer)
            {
                case IsEnemyCondition.Default: TamedCanBeAttackedByPlayer = HostilityMask.None; break;
                case IsEnemyCondition.Force: TamedCanBeAttackedByPlayer = HostilityMask.Attack; break;
                case IsEnemyCondition.Never: TamedCanBeAttackedByPlayer = HostilityMask.Never; break;
                case IsEnemyCondition.WhenFed: TamedCanBeAttackedByPlayer = !isHungry ? HostilityMask.Attack : HostilityMask.Skip; break;
                case IsEnemyCondition.WhenHungry: TamedCanBeAttackedByPlayer = isHungry ? HostilityMask.Attack : HostilityMask.Skip; break;
                case IsEnemyCondition.WhenStarving: TamedCanBeAttackedByPlayer = isStarving ? HostilityMask.Attack : HostilityMask.Skip; break;
            }
            switch (m_tamedCanAttackTamed)
            {
                case IsEnemyCondition.Default: TamedCanAttackTamed = HostilityMask.None; break;
                case IsEnemyCondition.Force: TamedCanAttackTamed = HostilityMask.Attack; break;
                case IsEnemyCondition.Never: TamedCanAttackTamed = HostilityMask.Never; break;
                case IsEnemyCondition.WhenFed: TamedCanAttackTamed = !isHungry ? HostilityMask.Attack : HostilityMask.Skip; break;
                case IsEnemyCondition.WhenHungry: TamedCanAttackTamed = isHungry ? HostilityMask.Attack : HostilityMask.Skip; break;
                case IsEnemyCondition.WhenStarving: TamedCanAttackTamed = isStarving ? HostilityMask.Attack : HostilityMask.Skip; break;
            }
            switch (m_tamedCanBeAttackedByTamed)
            {
                case IsEnemyCondition.Default: TamedCanBeAttackedByTamed = HostilityMask.None; break;
                case IsEnemyCondition.Force: TamedCanBeAttackedByTamed = HostilityMask.Attack; break;
                case IsEnemyCondition.Never: TamedCanBeAttackedByTamed = HostilityMask.Never; break;
                case IsEnemyCondition.WhenFed: TamedCanBeAttackedByTamed = !isHungry ? HostilityMask.Attack : HostilityMask.Skip; break;
                case IsEnemyCondition.WhenHungry: TamedCanBeAttackedByTamed = isHungry ? HostilityMask.Attack : HostilityMask.Skip; break;
                case IsEnemyCondition.WhenStarving: TamedCanBeAttackedByTamed = isStarving ? HostilityMask.Attack : HostilityMask.Skip; break;
            }
            switch (m_tamedCanAttackWild)
            {
                case IsEnemyCondition.Default: TamedCanAttackWild = HostilityMask.None; break;
                case IsEnemyCondition.Force: TamedCanAttackWild = HostilityMask.Attack; break;
                case IsEnemyCondition.Never: TamedCanAttackWild = HostilityMask.Never; break;
                case IsEnemyCondition.WhenFed: TamedCanAttackWild = !isHungry ? HostilityMask.Attack : HostilityMask.Skip; break;
                case IsEnemyCondition.WhenHungry: TamedCanAttackWild = isHungry ? HostilityMask.Attack : HostilityMask.Skip; break;
                case IsEnemyCondition.WhenStarving: TamedCanAttackWild = isStarving ? HostilityMask.Attack : HostilityMask.Skip; break;
            }
            switch (m_tamedCanBeAttackedByWild)
            {
                case IsEnemyCondition.Default: TamedCanBeAttackedByWild = HostilityMask.None; break;
                case IsEnemyCondition.Force: TamedCanBeAttackedByWild = HostilityMask.Attack; break;
                case IsEnemyCondition.Never: TamedCanBeAttackedByWild = HostilityMask.Never; break;
                case IsEnemyCondition.WhenFed: TamedCanBeAttackedByWild = !isHungry ? HostilityMask.Attack : HostilityMask.Skip; break;
                case IsEnemyCondition.WhenHungry: TamedCanBeAttackedByWild = isHungry ? HostilityMask.Attack : HostilityMask.Skip; break;
                case IsEnemyCondition.WhenStarving: TamedCanBeAttackedByWild = isStarving ? HostilityMask.Attack : HostilityMask.Skip; break;
            }
            switch (m_tamedCanAttackGroup)
            {
                case IsEnemyCondition.Default: TamedCanAttackGroup = HostilityMask.None; break;
                case IsEnemyCondition.Force: TamedCanAttackGroup = HostilityMask.Attack; break;
                case IsEnemyCondition.Never: TamedCanAttackGroup = HostilityMask.Never; break;
                case IsEnemyCondition.WhenFed: TamedCanAttackGroup = !isHungry ? HostilityMask.Attack : HostilityMask.Skip; break;
                case IsEnemyCondition.WhenHungry: TamedCanAttackGroup = isHungry ? HostilityMask.Attack : HostilityMask.Skip; break;
                case IsEnemyCondition.WhenStarving: TamedCanAttackGroup = isStarving ? HostilityMask.Attack : HostilityMask.Skip; break;
            }
            switch (m_tamedCanBeAttackedByGroup)
            {
                case IsEnemyCondition.Default: TamedCanBeAttackedByGroup = HostilityMask.None; break;
                case IsEnemyCondition.Force: TamedCanBeAttackedByGroup = HostilityMask.Attack; break;
                case IsEnemyCondition.Never: TamedCanBeAttackedByGroup = HostilityMask.Never; break;
                case IsEnemyCondition.WhenFed: TamedCanBeAttackedByGroup = !isHungry ? HostilityMask.Attack : HostilityMask.Skip; break;
                case IsEnemyCondition.WhenHungry: TamedCanBeAttackedByGroup = isHungry ? HostilityMask.Attack : HostilityMask.Skip; break;
                case IsEnemyCondition.WhenStarving: TamedCanBeAttackedByGroup = isStarving ? HostilityMask.Attack : HostilityMask.Skip; break;
            }
            switch (m_tamedCanAttackFaction)
            {
                case IsEnemyCondition.Default: TamedCanAttackFaction = HostilityMask.None; break;
                case IsEnemyCondition.Force: TamedCanAttackFaction = HostilityMask.Attack; break;
                case IsEnemyCondition.Never: TamedCanAttackFaction = HostilityMask.Never; break;
                case IsEnemyCondition.WhenFed: TamedCanAttackFaction = !isHungry ? HostilityMask.Attack : HostilityMask.Skip; break;
                case IsEnemyCondition.WhenHungry: TamedCanAttackFaction = isHungry ? HostilityMask.Attack : HostilityMask.Skip; break;
                case IsEnemyCondition.WhenStarving: TamedCanAttackFaction = isStarving ? HostilityMask.Attack : HostilityMask.Skip; break;
            }
            switch (m_tamedCanBeAttackedByFaction)
            {
                case IsEnemyCondition.Default: TamedCanBeAttackedByFaction = HostilityMask.None; break;
                case IsEnemyCondition.Force: TamedCanBeAttackedByFaction = HostilityMask.Attack; break;
                case IsEnemyCondition.Never: TamedCanBeAttackedByFaction = HostilityMask.Never; break;
                case IsEnemyCondition.WhenFed: TamedCanBeAttackedByFaction = !isHungry ? HostilityMask.Attack : HostilityMask.Skip; break;
                case IsEnemyCondition.WhenHungry: TamedCanBeAttackedByFaction = isHungry ? HostilityMask.Attack : HostilityMask.Skip; break;
                case IsEnemyCondition.WhenStarving: TamedCanBeAttackedByFaction = isStarving ? HostilityMask.Attack : HostilityMask.Skip; break;
            }

        }

        public void RPC_SetTamed(bool tamed)
        {
            if (tamed)
            {
                // re-setting spawn point is important for tamed creatures that use stay-near-spawn-point
                m_baseAITrait.SetSpawnPoint();

                // is this even neccessary?
                // alerted kreatues wont get tamed after all
                m_baseAITrait.StopPlayerHunt();

                SetTamedCharacteristics();
            }
        }
        
        public void SetTamedCharacteristics()
        {
            if (m_character.m_boss == true)
            {
                m_character.m_boss = false;
                m_character.m_bossEvent = "";
                EnemyHud.instance.RemoveCharacterHud(m_character);
            }

            if (m_changeGroupWhenTamed == true)
            {
                m_character.m_group = m_changeGroupWhenTamedTo;
            }
            if (m_changeFactionWhenTamed == true)
            {
                m_character.m_faction = m_changeFactionWhenTamedTo;
            }

            // todo: create yaml option "DisableIdleSounds"
            if (gameObject.name.StartsWith("Hatchling"))
            {
                var m_baseAI = GetComponent<BaseAI>();
                m_baseAI.m_idleSoundChance = 0;
            }
        }

        public string GetHoverName()
        {
            if (!m_nview.IsValid())
            {
                return "";
            }

            var text = "";
            var textSpacing = false;
            var precision = 1f / Plugin.Configs.HudProgressPrecision.Value;
            int decimals = Mathf.Max(0, Mathf.RoundToInt(-Mathf.Log10(precision)));

            if ((bool)m_tameableTrait && Plugin.Configs.HudShowTamingProgress.Value)
            {
                var tamingProgress = m_tameableTrait.GetTamingProgress(precision, decimals);
                if (tamingProgress.Length != 0)
                {
                    text += tamingProgress;
                    textSpacing = true;
                }
            }

            if ((bool)m_growupTrait && Plugin.Configs.HudShowOffspringGrowProgress.Value)
            {
                var growupProgress = m_growupTrait.GetGrowupProgress(precision, decimals);
                if (growupProgress.Length != 0)
                {
                    if (textSpacing)
                    {
                        text += " ";
                    }
                    text += growupProgress;
                }
            }

            return text;
        }

        public string GetHoverText(string text)
        {
            var isTamed = m_character.IsTamed();

            if (m_tameableTrait)
            {

                if (!isTamed && m_tameableTrait.IsTamingDisabled())
                {
                    text = m_tameableTrait.GetName();
                }
                else if (!isTamed && m_tameableTrait.CanBeTamed() == false)
                {
                    text = m_tameableTrait.GetName() + "\n" + m_tameableTrait.GetNotTameableReason();
                }
                else
                {
                    int newlineIndex = text.IndexOf('\n');
                    string firstLine;
                    string rest;

                    if (newlineIndex >= 0)
                    {
                        firstLine = text[..newlineIndex];
                        rest = text[newlineIndex..]; // includes '\n'
                    }
                    else
                    {
                        firstLine = text;
                        rest = "";
                    }

                    if (m_tameableTrait.IsFedTimerDisabled())
                    {
                        var hungry = Localization.instance.Localize("$hud_tamehungry");
                        if (!string.IsNullOrEmpty(hungry))
                        {
                            // remove hungry token only from first line
                            firstLine = firstLine.Replace(", " + hungry, "");
                            firstLine = firstLine.Replace(hungry + ", ", "");
                            firstLine = firstLine.Replace(hungry, "");

                            // cleanup spacing / punctuation artifacts
                            firstLine = firstLine.Replace(",  ", ", ");
                            firstLine = firstLine.Replace("  )", " )");
                            firstLine = firstLine.Replace("(  ", "( ");

                            // remove empty parentheses variants
                            firstLine = firstLine.Replace(" ( )", "");
                            firstLine = firstLine.Replace("( )", "");
                            firstLine = firstLine.Replace("()", "");

                            firstLine = firstLine.TrimEnd();
                            text = firstLine + rest;
                        }
                    }

                    if (m_tameableTrait.m_petCommand.Length != 0)
                    {
                        var pet1 = "] " + Localization.instance.Localize("$hud_pet");
                        var pet2 = "] " + Localization.instance.Localize(m_tameableTrait.m_petCommand);

                        // only replace in first line
                        text = text.Replace(pet1, pet2);
                    }

                }
            }

            var consumeText = GetConsumeHoverText();
            if (consumeText.Length != 0)
            {
                text += "\n" + consumeText;
            }

            if (m_tameableTrait)
            {
                var fedTimer = m_tameableTrait.GetFedTimerHoverText();
                if (fedTimer.Length != 0)
                {
                    text += "\n" + fedTimer;
                }
            }

            if (m_procreationTrait && isTamed)
            {
                var procreationText = m_procreationTrait.GetProcreationHoverText();
                if (procreationText.Length != 0)
                {
                    text += "\n" + procreationText;
                }
            }

            if (Plugin.IsAdmin() && Plugin.Configs.HoverShowAdminInfo.Value)
            {

                if (m_baseAITrait)
                {
                    var info = m_baseAITrait.GetAdminHoverInfoText();
                    if (info.Length > 0)
                    {
                        text += "\n" + info;
                    }
                }

                if (m_tameableTrait)
                {
                    var info = m_tameableTrait.GetAdminHoverInfoText();
                    if (info.Length > 0)
                    {
                        text += "\n" + info;
                    }
                }

                if (m_procreationTrait && isTamed)
                {
                    var info = m_procreationTrait.GetAdminHoverInfoText();
                    if (info.Length > 0)
                    {
                        text += "\n" + info;
                    }
                }

            }

            return text;
        }

        public string GetConsumeHoverText()
        {
            if (!Plugin.Configs.HoverShowConsumeItems.Value)
            {
                return "";
            }

            var L = Localization.instance;


            var tSecs = Time.time;
            
            if (lastHoverTarget != m_character || (tSecs - lastHoverUpdateTime) > 1f)
            {
                lastHoverTarget = m_character;
                lastHoverUpdateTime = tSecs;
                lastHoverConsumeText = "";

                var displayItems = CollectConsumableDisplayItems(L);
                if (displayItems.Count == 0)
                {
                    return ""; // todo: add special prefab: "@otab:NoItem;$otab_no_item" to display "Eats nothing" text
                }

                //return string.Format(l_consumeItems, Plugin.Configs.HoverColorNormal.Value, l_empty);
                // disabled because we want to know "what creatures can eat what food" and not "what creatures cannot eat"
                // todo: remove "$otab_hover_food_empty" from translations?
                //var l_empty = L.Localize("$otab_hover_food_empty");

                // Split into multiple lines based on approx length
                const int maxLineLen = 30; // TODO: config?
                var separator = L.Localize("$otab_hover_food_separator");
                var displayLines = BuildWrappedItemLines(displayItems, separator, maxLineLen);

                // First line gets the bullet, following lines get transparent bullet
                lastHoverConsumeText = string.Join("\n", displayLines.Select((line, i) =>
                    L.Localize(
                        "$otab_hover_food",
                        i == 0 ? Plugin.Configs.HoverColorNormal.Value : "#00000000",
                        line
                    )
                ));
            }

            return lastHoverConsumeText;
        }

        private List<ConsumableItemDisplay> CollectConsumableDisplayItems(Localization L)
        {
            var displayItems = new List<ConsumableItemDisplay>();
            if ((bool)m_baseAITrait == false)
            {
                // no ai afterall? weird
                return displayItems;
            }

            // otab feature
            // no need to check if otab data has been loaded
            // if no data loaded HasCustomConsumeItems() will return false
            if (m_baseAITrait.HasCustomConsumeItems(out var customItems))
            {
                if (customItems.Length == 0)
                {
                    return displayItems;
                }
                
                // hard values seems to be better
                float min = 0.33f; // consumeItems.Last().fedDurationFactor;
                float max = 3.00f; // consumeItems.First().fedDurationFactor;

                var fedTimerDisabled = m_tameableTrait && m_tameableTrait.IsFedTimerDisabled();

                foreach (var item in customItems)
                {
                    var displayName = L.Localize(item.itemDrop.m_itemData.m_shared.m_name);
                    var displayColor = OTABUtils.ColorUtils.GetColorBetween(
                        Plugin.Configs.HoverColorBad.Value,
                        Plugin.Configs.HoverColorNormal.Value,
                        Plugin.Configs.HoverColorGood.Value,
                        Plugin.Configs.HoverColorPassive.Value,
                        fedTimerDisabled ? 0 : item.fedDurationFactor,
                        min,
                        max
                    );
                    displayItems.Add(new ConsumableItemDisplay { Name = displayName, Color = displayColor });
                }

                return displayItems;
            }

            List<ItemDrop> consumeItems = null;
            if (m_monsterAI)
            {
                consumeItems = m_monsterAI.m_consumeItems;
            }
            else if (m_animalAITrait)
            {
                // otab feature
                // no need to check if otab data has been loaded
                // if no data loaded m_consumeItems is just empty
                consumeItems = m_animalAITrait.m_consumeItems;
                // wait... if its an animalAI it should be handled by HasCustomConsumeItems()
                // well... use this as fallback
            }

            if (consumeItems != null && consumeItems.Count > 0)
            {
                string color = Plugin.Configs.HoverColorNormal.Value;
                foreach (var itemDrop in consumeItems)
                {
                    var displayName = L.Localize(itemDrop.m_itemData.m_shared.m_name);
                    displayItems.Add(new ConsumableItemDisplay { Name = displayName, Color = color });
                }
            }

            return displayItems;
        }

        private List<string> BuildWrappedItemLines(List<ConsumableItemDisplay> displayItems, string separator, int maxLineLen)
        {
            var displayLines = new List<string>();
            var displayLineItems = new List<string>();
            int lineLength = 0;

            foreach (var displayItem in displayItems)
            {
                string displayName = displayItem.Name;
                string displayColor = displayItem.Color;

                if (string.IsNullOrEmpty(displayName))
                {
                    continue;
                }

                // only count name length (no separator)
                lineLength += displayName.Length;

                displayLineItems.Add($"<color={displayColor}>{displayName}</color>");

                // flush AFTER adding
                if (lineLength >= maxLineLen)
                {
                    displayLines.Add(string.Join(separator, displayLineItems));
                    displayLineItems.Clear();
                    lineLength = 0;
                }
            }

            // flush remaining
            if (displayLineItems.Count > 0)
            {
                displayLines.Add(string.Join(separator, displayLineItems));
                displayLineItems.Clear();
                lineLength = 0;
            }

            return displayLines;
        }

    }
}
