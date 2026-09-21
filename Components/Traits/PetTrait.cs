using OfTamingAndBreeding.Components.Core;
using OfTamingAndBreeding.Utilities;
using OfTamingAndBreeding.ValheimAPI;
using System;
using System.Collections.Generic;
using UnityEngine;


//todo: cleanup



namespace OfTamingAndBreeding.Components.Traits
{
    public class PetTrait : OTABComponent<PetTrait>
    {

        // set in awake
        [NonSerialized] private ZNetView m_nview = null;
        [NonSerialized] private Pet m_pet = null;
        [NonSerialized] private Procreation m_procreation = null;
        [NonSerialized] private MaterialVariation m_materialVariation = null;
        [NonSerialized] private Renderer m_renderer = null;
        [NonSerialized] private RandomSpeak m_randomSpeak = null;

        private void Awake()
        {
            m_nview = GetComponent<ZNetView>();
            m_pet = GetComponent<Pet>();

            m_procreation = GetComponent<Procreation>();
            m_materialVariation = GetComponentInChildren<MaterialVariation>();
            m_renderer = GetComponentInChildren<Renderer>();
            m_randomSpeak = GetComponent<RandomSpeak>();

            Register(this);
        }

        private void OnDestroy()
        {
            Unregister(this);
        }

        public void UpdateMaterial()
        {



            if (m_nview == null)
            {
                return;
            }

            int material = m_materialVariation.GetMaterial();
            if (m_randomSpeak != null)
            {
                m_randomSpeak.enabled = material != 5 && material != 6;
            }

            if (!m_nview.IsOwner() || m_renderer.isVisible)
            {
                return;
            }

            List<Player> allPlayers = Player.GetAllPlayers();
            float num = 99999f;
            float num2 = 0.5f;
            if (m_materialVariation.GetMaterial() == 5)
            {
                num2 = 0.07f;
            }

            foreach (Player item in allPlayers)
            {
                float num3 = Utils.DistanceXZ(item.transform.position, base.transform.position);
                if (num > num3)
                {
                    num = num3;
                }

                if (num3 > 10f)
                {
                    continue;
                }

                SEMan sEMan = item.GetSEMan();
                if (sEMan == null)
                {
                    continue;
                }

                if (sEMan.HaveStatusEffect(SEMan.s_statusEffectSoftDeath) && UnityEngine.Random.value > num2)
                {
                    m_pet.SetFace(1);
                    return;
                }

                if ((sEMan.HaveStatusEffect(SEMan.s_statusEffectRested) || sEMan.HaveStatusEffect(SEMan.s_statusEffectCampFire) || (m_procreation && m_procreation.GetLovePoints() > 2)) && UnityEngine.Random.value > num2)
                {
                    m_pet.SetFace(0);
                    return;
                }

                if (sEMan.HaveStatusEffect(SEMan.s_statusEffectBurning) || sEMan.HaveStatusEffect(SEMan.s_statusEffectFreezing) || (sEMan.HaveStatusEffect(SEMan.s_statusEffectPoison) && UnityEngine.Random.value > num2))
                {
                    m_pet.SetFace(3);
                    return;
                }

                if (sEMan.HaveStatusEffect(SEMan.s_statusEffectEncumbered) && UnityEngine.Random.value > num2)
                {
                    m_pet.SetFace(7);
                    return;
                }

                if (sEMan.HaveStatusEffect(SEMan.s_statusEffectSmoked) && UnityEngine.Random.value > num2)
                {
                    m_pet.SetFace(5);
                    return;
                }

                if (DateTime.Now - TimeSpan.FromSeconds(m_pet.m_UpdateRate) < Player.LastEmoteTime)
                {
                    if (Player.LastEmote == "cry" && UnityEngine.Random.value > num2)
                    {
                        m_pet.SetFace((!(UnityEngine.Random.value > 0.5f)) ? 1 : 3);
                        return;
                    }

                    if ((Player.LastEmote == "cheer" || Player.LastEmote == "toast" || Player.LastEmote == "flex" || Player.LastEmote == "laugh") && UnityEngine.Random.value > num2)
                    {
                        m_pet.SetFace((!(UnityEngine.Random.value > 0.5f)) ? 4 : 0);
                        return;
                    }

                    if ((Player.LastEmote == "blowkiss" || Player.LastEmote == "dance" || Player.LastEmote == "shrug" || Player.LastEmote == "roar") && UnityEngine.Random.value > num2)
                    {
                        m_pet.SetFace((UnityEngine.Random.value > 0.5f) ? 5 : 7);
                        return;
                    }

                    if ((Player.LastEmote == "kneel" || Player.LastEmote == "bow" || Player.LastEmote == "sit") && UnityEngine.Random.value > num2)
                    {
                        m_pet.SetFace((UnityEngine.Random.value > 0.5f) ? 4 : 2);
                        return;
                    }
                }
            }

            if (UnityEngine.Random.value < 0.1f && (allPlayers.Count == 1 || num > 20f))
            {
                m_pet.SetFace(UnityEngine.Random.Range(0, m_materialVariation.m_materials.Count));
            }

        }

    }
}
