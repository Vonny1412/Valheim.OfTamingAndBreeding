using Humanoid_Alias = Humanoid;
using ItemDrop_ItemData_Alias = ItemDrop.ItemData;
using ItemDrop_Alias = ItemDrop;
using MonsterAI_Alias = MonsterAI;
using Character_Alias = Character;
using StaticTarget_Alias = StaticTarget;

namespace OfTamingAndBreeding.ValheimAPI
{
    public partial class MonsterAI : BaseAI
    {
        public MonsterAI(MonsterAI_Alias instance) : base(instance)
        {
        }

        public static readonly Core.Invokers.TypedMethodInvoker<bool> __IAPI_UpdateConsumeItem_Invoker1 = new Core.Invokers.TypedMethodInvoker<bool>(typeof(MonsterAI_Alias), "UpdateConsumeItem", new Core.Signatures.ParamSig[] { new Core.Signatures.NonGenericParamSig(typeof(Humanoid_Alias), false), new Core.Signatures.NonGenericParamSig(typeof(float), false) });

        public static readonly Core.Invokers.TypedMethodInvoker<ItemDrop_Alias> __IAPI_FindClosestConsumableItem_Invoker1 = new Core.Invokers.TypedMethodInvoker<ItemDrop_Alias>(typeof(MonsterAI_Alias), "FindClosestConsumableItem", new Core.Signatures.ParamSig[] { new Core.Signatures.NonGenericParamSig(typeof(float), false) });

        public static readonly Core.Invokers.TypedMethodInvoker<bool> __IAPI_CanConsume_Invoker1 = new Core.Invokers.TypedMethodInvoker<bool>(typeof(MonsterAI_Alias), "CanConsume", new Core.Signatures.ParamSig[] { new Core.Signatures.NonGenericParamSig(typeof(ItemDrop_ItemData_Alias), false) });

        public static readonly Core.Invokers.FieldMutateInvoker<Character_Alias> __IAPI_m_targetCreature_Invoker = new Core.Invokers.FieldMutateInvoker<Character_Alias>(typeof(MonsterAI_Alias), "m_targetCreature");

        public static readonly Core.Invokers.FieldMutateInvoker<StaticTarget_Alias> __IAPI_m_targetStatic_Invoker = new Core.Invokers.FieldMutateInvoker<StaticTarget_Alias>(typeof(MonsterAI_Alias), "m_targetStatic");

    }
}
