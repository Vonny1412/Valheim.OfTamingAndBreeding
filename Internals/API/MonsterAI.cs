using Humanoid_Alias = Humanoid;
using ItemDrop_ItemData_Alias = ItemDrop.ItemData;
using ItemDrop_Alias = ItemDrop;
using MonsterAI_Alias = MonsterAI;
using Character_Alias = Character;
using StaticTarget_Alias = StaticTarget;

namespace OfTamingAndBreeding.Internals.API
{
    public partial class MonsterAI : BaseAI
    {
        public MonsterAI(MonsterAI_Alias instance) : base(instance)
        {
        }

        public static readonly Core.Invokers.TypedMethodInvoker<bool> __IAPI_UpdateConsumeItem_Invoker1 = new Core.Invokers.TypedMethodInvoker<bool>(typeof(MonsterAI_Alias), "UpdateConsumeItem", new Core.Signatures.ParamSig[] { new Core.Signatures.NonGenericParamSig(typeof(Humanoid_Alias), false), new Core.Signatures.NonGenericParamSig(typeof(float), false) });
        public bool UpdateConsumeItem(Humanoid_Alias humanoid, float dt) => __IAPI_UpdateConsumeItem_Invoker1.Invoke(((MonsterAI_Alias)__IAPI_instance), new object[] { humanoid, dt });
        public static readonly Core.Invokers.TypedMethodInvoker<ItemDrop_Alias> __IAPI_FindClosestConsumableItem_Invoker1 = new Core.Invokers.TypedMethodInvoker<ItemDrop_Alias>(typeof(MonsterAI_Alias), "FindClosestConsumableItem", new Core.Signatures.ParamSig[] { new Core.Signatures.NonGenericParamSig(typeof(float), false) });
        public ItemDrop_Alias FindClosestConsumableItem(float maxRange) => __IAPI_FindClosestConsumableItem_Invoker1.Invoke(((MonsterAI_Alias)__IAPI_instance), new object[] { maxRange });
        public static readonly Core.Invokers.TypedMethodInvoker<bool> __IAPI_CanConsume_Invoker1 = new Core.Invokers.TypedMethodInvoker<bool>(typeof(MonsterAI_Alias), "CanConsume", new Core.Signatures.ParamSig[] { new Core.Signatures.NonGenericParamSig(typeof(ItemDrop_ItemData_Alias), false) });
        public bool CanConsume(ItemDrop_ItemData_Alias item) => __IAPI_CanConsume_Invoker1.Invoke(((MonsterAI_Alias)__IAPI_instance), new object[] { item });



        public static readonly Core.Invokers.FieldMutateInvoker<Character_Alias> __IAPI_m_targetCreature_Invoker = new Core.Invokers.FieldMutateInvoker<Character_Alias>(typeof(MonsterAI_Alias), "m_targetCreature");
        public Character_Alias m_targetCreature
        {
            get => __IAPI_m_targetCreature_Invoker.Get(((MonsterAI_Alias)__IAPI_instance));
            set => __IAPI_m_targetCreature_Invoker.Set(((MonsterAI_Alias)__IAPI_instance), value);
        }
        public static readonly Core.Invokers.FieldMutateInvoker<StaticTarget_Alias> __IAPI_m_targetStatic_Invoker = new Core.Invokers.FieldMutateInvoker<StaticTarget_Alias>(typeof(MonsterAI_Alias), "m_targetStatic");
        public StaticTarget_Alias m_targetStatic
        {
            get => __IAPI_m_targetStatic_Invoker.Get(((MonsterAI_Alias)__IAPI_instance));
            set => __IAPI_m_targetStatic_Invoker.Set(((MonsterAI_Alias)__IAPI_instance), value);
        }
    }
}
