using System;
using System.Reflection;
using UnityEngine; // for Debug? optional
// Character is from Valheim
// ZNetView / ZDO usage is inside CLLC, not needed here

namespace OfTamingAndBreeding.ThirdParty.Mods
{
    internal static partial class CllCBridge
    {
        public const string PluginGUID = "org.bepinex.plugins.creaturelevelcontrol";

        public static bool IsRegistered { get; private set; } = false;

        private static Type apiType;
        private static Type infusionEnumType;      // CreatureLevelControl.CreatureInfusion
        private static Type extraEffectEnumType;   // CreatureLevelControl.CreatureExtraEffect

        private static MethodInfo miIsEnabled;
        private static MethodInfo miIsInfusionEnabled;
        private static MethodInfo miIsExtraEffectEnabled;

        private static MethodInfo miHasInfusionCreature;
        private static MethodInfo miGetInfusionCreature;
        private static MethodInfo miSetInfusionCreature0; // SetInfusionCreature(Character)
        private static MethodInfo miSetInfusionCreature1; // SetInfusionCreature(Character, CreatureInfusion)

        private static MethodInfo miHasExtraEffectCreature;
        private static MethodInfo miGetExtraEffectCreature;
        private static MethodInfo miSetExtraEffectCreature0; // SetExtraEffectCreature(Character)
        private static MethodInfo miSetExtraEffectCreature1; // SetExtraEffectCreature(Character, CreatureExtraEffect)

        public sealed class Registrator : ThirdPartyPluginRegistrator
        {
            public override string PluginGUID => CllCBridge.PluginGUID;

            public override void OnRegistered(string guid, Assembly asm)
            {
                // Public API type
                var _apiType = asm.GetType("CreatureLevelControl.API", throwOnError: false);
                if (_apiType == null) return;

                // Enums (for SetX(Character, enum))
                var _infusionEnum = asm.GetType("CreatureLevelControl.CreatureInfusion", throwOnError: false);
                var _extraEnum = asm.GetType("CreatureLevelControl.CreatureExtraEffect", throwOnError: false);

                // Methods we care about
                MethodInfo Find(string name, params Type[] args)
                    => _apiType.GetMethod(name, BindingFlags.Static | BindingFlags.Public, null, args, null);

                var _isEnabled = Find("IsEnabled");
                var _isInfusionEnabled = Find("IsInfusionEnabled");
                var _isExtraEnabled = Find("IsExtraEffectEnabled");

                var _hasInf = Find("HasInfusionCreature", typeof(Character));
                var _getInf = Find("GetInfusionCreature", typeof(Character));
                var _setInf0 = Find("SetInfusionCreature", typeof(Character));
                var _setInf1 = (_infusionEnum != null)
                    ? Find("SetInfusionCreature", typeof(Character), _infusionEnum)
                    : null;

                var _hasEff = Find("HasExtraEffectCreature", typeof(Character));
                var _getEff = Find("GetExtraEffectCreature", typeof(Character));
                var _setEff0 = Find("SetExtraEffectCreature", typeof(Character));
                var _setEff1 = (_extraEnum != null)
                    ? Find("SetExtraEffectCreature", typeof(Character), _extraEnum)
                    : null;

                // Minimal requirements to call safely:
                if (_isEnabled == null) return;

                apiType = _apiType;
                infusionEnumType = _infusionEnum;
                extraEffectEnumType = _extraEnum;

                miIsEnabled = _isEnabled;
                miIsInfusionEnabled = _isInfusionEnabled;
                miIsExtraEffectEnabled = _isExtraEnabled;

                miHasInfusionCreature = _hasInf;
                miGetInfusionCreature = _getInf;
                miSetInfusionCreature0 = _setInf0;
                miSetInfusionCreature1 = _setInf1;

                miHasExtraEffectCreature = _hasEff;
                miGetExtraEffectCreature = _getEff;
                miSetExtraEffectCreature0 = _setEff0;
                miSetExtraEffectCreature1 = _setEff1;

                IsRegistered = true;
            }
        }

        public static bool IsEnabled()
        {
            if (!IsRegistered || miIsEnabled == null) return false;
            try
            {
                return (bool)miIsEnabled.Invoke(null, null);
            }
            catch
            {
                return false;
            }
        }

        public static bool IsInfusionEnabled()
        {
            if (!IsRegistered || miIsInfusionEnabled == null) return false;
            try
            {
                return (bool)miIsInfusionEnabled.Invoke(null, null);
            }
            catch
            {
                return false;
            }
        }

        public static bool IsExtraEffectEnabled()
        {
            if (!IsRegistered || miIsExtraEffectEnabled == null) return false;
            try
            {
                return (bool)miIsExtraEffectEnabled.Invoke(null, null);
            }
            catch
            {
                return false;
            }
        }

        // -------------------------
        // ExtraEffect (CreatureExtraEffect)
        // Return values as int to avoid hard compile dependency on CLLC enums.
        // CLLC uses None=0 in its enum.
        // -------------------------

        public static bool HasExtraEffect(Character character)
        {
            if (character == null) return false;
            if (!IsEnabled() || !IsExtraEffectEnabled()) return false;
            if (miHasExtraEffectCreature == null) return false;

            try
            {
                return (bool)miHasExtraEffectCreature.Invoke(null, new object[] { character });
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Returns CLLC CreatureExtraEffect as int (0 = None).
        /// </summary>
        public static int GetExtraEffect(Character character)
        {
            if (character == null) return 0;
            if (!IsEnabled() || !IsExtraEffectEnabled()) return 0;
            if (miGetExtraEffectCreature == null) return 0;

            try
            {
                object res = miGetExtraEffectCreature.Invoke(null, new object[] { character });
                return res == null ? 0 : (int)res;
            }
            catch
            {
                return 0;
            }
        }

        /// <summary>
        /// Lets CLLC pick a random extra effect (based on its own config).
        /// No-op if CLLC is not available/enabled.
        /// </summary>
        public static void SetExtraEffectRandom(Character character)
        {
            if (character == null) return;
            if (!IsEnabled() || !IsExtraEffectEnabled()) return;
            if (miSetExtraEffectCreature0 == null) return;

            try
            {
                miSetExtraEffectCreature0.Invoke(null, new object[] { character });
            }
            catch
            {
                // swallow: bridge should be safe to call
            }
        }

        /// <summary>
        /// Sets extra effect by int (CLLC enum value). 0 = None.
        /// No-op if enum type/method not available.
        /// </summary>
        public static void SetExtraEffect(Character character, int effect)
        {
            if (character == null) return;
            if (!IsEnabled() || !IsExtraEffectEnabled()) return;
            if (miSetExtraEffectCreature1 == null || extraEffectEnumType == null) return;

            try
            {
                object enumObj = Enum.ToObject(extraEffectEnumType, effect);
                miSetExtraEffectCreature1.Invoke(null, new object[] { character, enumObj });
            }
            catch
            {
                // swallow
            }
        }

        // -------------------------
        // Infusion (CreatureInfusion)
        // Return values as int to avoid hard compile dependency on CLLC enums.
        // CLLC uses None=0 in its enum.
        // -------------------------

        public static bool HasInfusion(Character character)
        {
            if (character == null) return false;
            if (!IsEnabled() || !IsInfusionEnabled()) return false;
            if (miHasInfusionCreature == null) return false;

            try
            {
                return (bool)miHasInfusionCreature.Invoke(null, new object[] { character });
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Returns CLLC CreatureInfusion as int (0 = None).
        /// </summary>
        public static int GetInfusion(Character character)
        {
            if (character == null) return 0;
            if (!IsEnabled() || !IsInfusionEnabled()) return 0;
            if (miGetInfusionCreature == null) return 0;

            try
            {
                object res = miGetInfusionCreature.Invoke(null, new object[] { character });
                return res == null ? 0 : (int)res;
            }
            catch
            {
                return 0;
            }
        }

        /// <summary>
        /// Lets CLLC pick a random infusion (based on its own config).
        /// No-op if CLLC is not available/enabled.
        /// </summary>
        public static void SetInfusionRandom(Character character)
        {
            if (character == null) return;
            if (!IsEnabled() || !IsInfusionEnabled()) return;
            if (miSetInfusionCreature0 == null) return;

            try
            {
                miSetInfusionCreature0.Invoke(null, new object[] { character });
            }
            catch
            {
                // swallow
            }
        }

        /// <summary>
        /// Sets infusion by int (CLLC enum value). 0 = None.
        /// No-op if enum type/method not available.
        /// </summary>
        public static void SetInfusion(Character character, int infusion)
        {
            if (character == null) return;
            if (!IsEnabled() || !IsInfusionEnabled()) return;
            if (miSetInfusionCreature1 == null || infusionEnumType == null) return;

            try
            {
                object enumObj = Enum.ToObject(infusionEnumType, infusion);
                miSetInfusionCreature1.Invoke(null, new object[] { character, enumObj });
            }
            catch
            {
                // swallow
            }
        }

    }
}
