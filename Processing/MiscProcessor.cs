using OfTamingAndBreeding.Data.Models;
using OfTamingAndBreeding.Processing.Core;
using OfTamingAndBreeding.Processing.Registry;

namespace OfTamingAndBreeding.Processing
{
    internal class MiscProcessor : DataProcessor<MiscFile>
    {
        public override string DirectoryName => MiscFile.DirectoryName;

        public override string PrefabTypeName => null;

        public override string GetDataKey(string filePath) => null;

        public override bool LoadFromFile(string filePath) => LoadFromYamlFile(filePath);

        public override bool PrepareProcess()
        {
            return true;
        }
        
        public override bool ReservePrefabName(string objectName)
        {
            // currently no need to reserve prefab names
            return true;
        }

        public override bool ValidateData(string miscPrefabName, MiscFile data)
        {
            var model = $"{nameof(MiscFile)}.{miscPrefabName}";
            var valid = true;

            var miscPrefab = OTABPrefabRegistry.Instance.GetRegisteredPrefab(miscPrefabName);
            if (!miscPrefab)
            {
                Plugin.LogError($"{model}: Unknown prefab");
                return false;
            }

            if (data.Pickable != null)
            {
                if (!miscPrefab.GetComponent<Destructible>())
                {
                    Plugin.LogError($"{model}.{nameof(data.Pickable)}: Prefab has no Destructible component");
                    valid = false;
                }
                if (miscPrefab.GetComponent<Pickable>())
                {
                    Plugin.LogError($"{model}.{nameof(data.Pickable)}: Object already has Pickable component");
                    valid = false;
                }
                else
                {
                    if (data.Pickable.Item == null)
                    {
                        Plugin.LogError($"{model}.{nameof(data.Pickable)}.{nameof(data.Pickable.Item)}: Missing field");
                        valid = false;
                    }
                    else
                    {
                        // beeing checked in ProcessPrefab() because prefab might be custom
                    }

                    // here maybe more
                }
            }

            return valid;
        }

        public override bool RegisterPrefab(string miscPrefabName, MiscFile data)
        {
            var model = $"{nameof(MiscFile)}.{miscPrefabName}";
            var valid = true;

            // currently no need to register

            return valid;
        }

        public override bool ProcessPrefab(string miscPrefabName, MiscFile data)
        {
            var model = $"{nameof(MiscFile)}.{miscPrefabName}";
            var miscPrefab = OTABPrefabRegistry.Instance.GetRegisteredPrefab(miscPrefabName);

            if (data.Pickable != null)
            {
                var pickableItemPrefab = OTABPrefabRegistry.Instance.GetCustomPrefab(data.Pickable.Item)
                                      ?? OTABPrefabRegistry.Instance.GetRegisteredPrefab(data.Pickable.Item);

                if (!pickableItemPrefab)
                {
                    Plugin.LogError($"{model}.{nameof(data.Pickable)}.{nameof(data.Pickable.Item)}: Unknown prefab '{data.Pickable.Item}'");
                    return false;
                }

                if (!pickableItemPrefab.TryGetComponent<ItemDrop>(out _))
                {
                    Plugin.LogError($"{model}.{nameof(data.Pickable)}.{nameof(data.Pickable.Item)}: Prefab '{data.Pickable.Item}' has no ItemDrop");
                    return false;
                }

                var objectPickableItem = OTABPrefabRegistry.Instance.GetOrAddComponent<Pickable>(miscPrefabName, miscPrefab);
                objectPickableItem.m_itemPrefab = pickableItemPrefab;

                objectPickableItem.m_amount = 1; // todo: add option?
                objectPickableItem.m_dontScale = true; // todo: option?
                //m_minAmountScaled = 1

                objectPickableItem.m_maxLevelBonusChance = 0f; // todo: option?
                //m_bonusYieldAmount = 1

                //m_overrideName = "..."
                //m_respawnTimeMinutes = 0f
                //m_pickEffector

            }

            return true;
        }

        public override bool FinalizeProcess()
        {
            return true;
        }

        public override void RestorePrefab(string miscPrefabName)
        {
            OTABPrefabRegistry.Instance.RestorePrefab(miscPrefabName, (current, backup) => {
                PrefabUtils.RestoreComponent<Pickable>(current, backup);
            });
        }

        public override void CleanupProcess()
        {
        }

    }
}
