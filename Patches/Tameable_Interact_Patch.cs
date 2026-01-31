using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace OfTamingAndBreeding.Patches
{
    [HarmonyPatch(typeof(Tameable), "Interact")]
    static class Tameable_Interact_Patch
    {
        static bool Prefix(Tameable __instance, Humanoid user, bool hold, bool alt, ref bool __result)
        {
            var tameableAPI = Internals.TameableAPI.GetOrCreate(__instance);
            if (tameableAPI.m_nview == null || !tameableAPI.m_nview.IsValid())
            {
                __result = false;
                return false; // skip vanilla
            }

            // Shift+E rename (keep vanilla behavior)
            if (alt)
            {
                tameableAPI.SetName();
                __result = true;
                return false;
            }

            // only tamed creatures react
            if (!tameableAPI.IsTamed())
            {
                __result = false;
                return false;
            }

            // HOLD = command (separated from pet)
            if (hold)
            {

                if (!tameableAPI.m_commandable)
                {
                    __result = false; // abort
                    return false;
                }

                if (UnityEngine.Time.time - tameableAPI.m_lastPetTime < 0.33f)
                {
                    __result = true; // delay, but continue
                    return false;
                }

                if (UnityEngine.Time.time - tameableAPI.lastCommandTime < 1.0f)
                {
                    __result = true; // delay, but continue
                    return false;
                }

                tameableAPI.lastCommandTime = UnityEngine.Time.time;
                tameableAPI.Command(user);
                __result = false; // stop
                return false;
            }

            // TAP = pet + message (no command)
            if (UnityEngine.Time.time - tameableAPI.m_lastPetTime <= 1f)
            {
                __result = false;
                return false;
            }

            tameableAPI.m_lastPetTime = UnityEngine.Time.time;
            tameableAPI.m_petEffect?.Create(__instance.transform.position, __instance.transform.rotation);

            // stop tiny whiny tamy from beeing alerted
            var baseAI = __instance.GetComponent<BaseAI>();
            if (baseAI != null)
            {
                Internals.API.BaseAI.__IAPI_m_fleeTargetUpdateTime_Invoker.Set(baseAI, UnityEngine.Time.time);

                var monsterAI = __instance.GetComponent<MonsterAI>();
                if (monsterAI != null)
                {
                    Internals.API.MonsterAI.__IAPI_SetAlerted_Invoker1.Invoke(monsterAI, new object[] { false });
                    monsterAI.StopMoving();
                }
                else
                {
                    var animalAI = __instance.GetComponent<AnimalAI>();
                    if (animalAI != null)
                    {
                        Internals.API.AnimalAI.__IAPI_SetAlerted_Invoker1.Invoke(animalAI, new object[] { false });
                        animalAI.StopMoving();
                    }
                    else
                    {
                        Internals.API.BaseAI.__IAPI_SetAlerted_Invoker1.Invoke(baseAI, new object[] { false });
                        baseAI.StopMoving();
                    }
                }
            }
            else
            {
                // not monster and not animal? weird
                // i dont care
            }

            // vanilla-like message selection
            string msg = null;
            if (tameableAPI.m_tameTextGetter != null)
            {
                var text = tameableAPI.m_tameTextGetter();
                if (!string.IsNullOrEmpty(text))
                {
                    msg = text;
                }
            }
            if (string.IsNullOrEmpty(msg))
            {
                var hoverName = __instance.GetHoverName();
                var tameText = tameableAPI.m_tameText ?? "";
                msg = tameableAPI.m_nameBeforeText ? (hoverName + " " + tameText) : tameText;
            }
            if (!string.IsNullOrEmpty(msg))
            {
                user.Message(MessageHud.MessageType.Center, msg);
            }

            __result = true;
            return false; // always skip vanilla
        }
    }

    /** original method
    public bool Interact(Humanoid user, bool hold, bool alt)
    {
        if (!m_nview.IsValid())
        {
            return false;
        }

        if (hold)
        {
            return false;
        }

        if (alt)
        {
            SetName();
            return true;
        }

        string hoverName = GetHoverName();
        object msg;
        if (IsTamed())
        {
            if (Time.time - m_lastPetTime > 1f)
            {
                m_lastPetTime = Time.time;
                m_petEffect.Create(base.transform.position, base.transform.rotation);
                if (m_commandable)
                {
                    Command(user);
                    goto IL_00da;
                }

                if (m_tameTextGetter != null)
                {
                    string text = m_tameTextGetter();
                    if (text != null && text.Length > 0)
                    {
                        msg = text;
                        goto IL_00d3;
                    }
                }

                msg = (m_nameBeforeText ? (hoverName + " " + m_tameText) : m_tameText);
                goto IL_00d3;
            }

            return false;
        }

        return false;
    IL_00da:
        return true;
    IL_00d3:
        user.Message(MessageHud.MessageType.Center, (string)msg);
        goto IL_00da;
    }
    **/

}
