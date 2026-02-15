using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace OfTamingAndBreeding.ValheimAPI
{
    internal static class AnimalAIExtensions
    {
        internal sealed class AnimalAIExtraData : Lifecycle.ExtraData<AnimalAI, AnimalAIExtraData>
        {
            public List<ItemDrop> m_consumeItems = null;
            public float m_consumeRange = 2f;
            public float m_consumeSearchRange = 5f;
            public float m_consumeSearchInterval = 10f;
            public ItemDrop m_consumeTarget = null;
            public float m_consumeSearchTimer;
            public Action<ItemDrop> m_onConsumedItem = null;
            public GameObject m_follow;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void SetAlerted(this AnimalAI that, bool alert)
            => LowLevel.AnimalAI.__IAPI_SetAlerted_Invoker1.Invoke(that, alert);

        public static AnimalAIExtraData CreateExtraData(this AnimalAI animalAI)
        {
            return AnimalAIExtraData.GetOrCreate(animalAI);
        }

        public static void Awake_PatchPostfix(this AnimalAI animalAI)
        {
            // we need this because we wanna pass settings from prefab to instance
            var prefabName = Utils.GetPrefabName(animalAI.gameObject.name);
            var prefab = ZNetScene.instance.GetPrefab(prefabName);
            if (!prefab) return; // should not happen but whatever
            var prefabAnimalAI = prefab.GetComponent<AnimalAI>();
            if (!prefabAnimalAI) return; // should not happen but whatever

            // check if extra data of prefab is stored
            if (AnimalAIExtraData.TryGet(prefabAnimalAI, out var animalAIPrefabData))
            {
                var animalAIData = AnimalAIExtraData.GetOrCreate(animalAI);
                // take and set values: animalAIPrefabData -> animalAIData
                animalAIData.m_consumeRange = animalAIPrefabData.m_consumeRange;
                animalAIData.m_consumeSearchRange = animalAIPrefabData.m_consumeSearchRange;
                animalAIData.m_consumeSearchInterval = animalAIPrefabData.m_consumeSearchInterval;
                animalAIData.m_onConsumedItem = animalAIPrefabData.m_onConsumedItem;
                animalAIData.m_consumeItems = new List<ItemDrop>(animalAIPrefabData.m_consumeItems);
            }
        }

        public static bool IdleMovement_PatchPrefix(this AnimalAI animalAI, float dt)
        {
            var m_nview = animalAI.GetZNetView();
            if (!m_nview.IsValid() || !m_nview.IsOwner())
            {
                // we dont even need to check for owner
                // because IdleMovement only gets called when it is the owner
                // but whatever
                return true; // i dont care
            }

            if (AnimalAIExtraData.TryGet(animalAI, out var data))
            {
                Humanoid humanoid = animalAI.GetComponent<Character>() as Humanoid;
                if (animalAI.UpdateConsumeItem(data, humanoid, dt)) return false;
                if (animalAI.UpdateFollowTarget(data, dt)) return false;
            }

            return true;
        }

        public static bool UpdateConsumeItem(this AnimalAI animalAI, AnimalAIExtraData data, Humanoid humanoid, float dt)
        {
            if (animalAI.IsAlerted() || data.m_consumeItems == null || data.m_consumeItems.Count == 0)
            {
                return false;
            }

            var m_tameable = animalAI.GetTameable();

            data.m_consumeSearchTimer += dt;
            if (data.m_consumeSearchTimer > data.m_consumeSearchInterval)
            {
                data.m_consumeSearchTimer = 0f;
                if ((bool)m_tameable && !m_tameable.IsHungry())
                {
                    return false;
                }

                if (Plugin.Configs.UseBetterSearchForFood.Value == true)
                {
                    data.m_consumeTarget = animalAI.FindNearbyConsumableItem(data.m_consumeSearchRange, data.m_consumeItems);
                }
                else
                {
                    data.m_consumeTarget = animalAI.FindClosestConsumableItem(data.m_consumeSearchRange, data.m_consumeItems);
                }
            }

            if ((bool)data.m_consumeTarget)
            {
                if (animalAI.MoveTo(dt, data.m_consumeTarget.transform.position, data.m_consumeRange, run: false))
                {
                    animalAI.LookAt(data.m_consumeTarget.transform.position);
                    if (animalAI.IsLookingAt(data.m_consumeTarget.transform.position, 20f) && data.m_consumeTarget.RemoveOne())
                    {
                        data.m_onConsumedItem?.Invoke(data.m_consumeTarget);

                        //humanoid?.m_consumeItemEffects?.Create(animalAI.transform.position, Quaternion.identity);
                        // todo: broadcast via rpc
                        animalAI.GetZSyncAnimation()?.SetTrigger("consume");
                        data.m_consumeTarget = null;
                    }
                }

                return true;
            }

            return false;
        }

        public static void MakeTame(this AnimalAI animalAI)
        {
            animalAI.GetCharacter()?.SetTamed(tamed: true);
            animalAI.SetAlerted(alert: false);
        }

        public static bool UpdateFollowTarget(this AnimalAI animalAI, AnimalAIExtraData data, float dt)
        {
            if (animalAI.IsAlerted() || data.m_follow == null)
            {
                return false;
            }
            animalAI.Follow(data.m_follow, dt);
            return true;
        }

    }
}
