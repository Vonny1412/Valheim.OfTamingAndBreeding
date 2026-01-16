
using EffectList_Alias = EffectList;
using EggGrow_Alias = EggGrow;
using UnityEngine_GameObject_Alias = UnityEngine.GameObject;
using ItemDrop_Alias = ItemDrop;
using ZNetView_Alias = ZNetView;

using OfTamingAndBreeding.Internals.API.Core;
using OfTamingAndBreeding.Internals.API.Core.Invokers;
using OfTamingAndBreeding.Internals.API.Core.Signatures;
namespace OfTamingAndBreeding.Internals.API
{
    public partial class EggGrow : UnityEngine.MonoBehaviour
    {
        public EggGrow(EggGrow_Alias instance) : base(instance)
        {
        }

        public UnityEngine_GameObject_Alias m_grownPrefab
        {
            get => ((EggGrow_Alias)__IAPI_instance).m_grownPrefab;
            set => ((EggGrow_Alias)__IAPI_instance).m_grownPrefab = value;
        }
        public bool m_tamed
        {
            get => ((EggGrow_Alias)__IAPI_instance).m_tamed;
            set => ((EggGrow_Alias)__IAPI_instance).m_tamed = value;
        }
        public EffectList_Alias m_hatchEffect
        {
            get => ((EggGrow_Alias)__IAPI_instance).m_hatchEffect;
            set => ((EggGrow_Alias)__IAPI_instance).m_hatchEffect = value;
        }


        public static readonly FieldMutateInvoker<ItemDrop_Alias> __IAPI_m_item_Invoker = new FieldMutateInvoker<ItemDrop_Alias>(typeof(EggGrow_Alias), "m_item");
        public ItemDrop_Alias m_item
        {
            get => __IAPI_m_item_Invoker.Get(((EggGrow_Alias)__IAPI_instance));
            set => __IAPI_m_item_Invoker.Set(((EggGrow_Alias)__IAPI_instance), value);
        }
        public static readonly FieldMutateInvoker<ZNetView_Alias> __IAPI_m_nview_Invoker = new FieldMutateInvoker<ZNetView_Alias>(typeof(EggGrow_Alias), "m_nview");
        public ZNetView_Alias m_nview
        {
            get => __IAPI_m_nview_Invoker.Get(((EggGrow_Alias)__IAPI_instance));
            set => __IAPI_m_nview_Invoker.Set(((EggGrow_Alias)__IAPI_instance), value);
        }

        public float m_growTime
        {
            get => ((EggGrow_Alias)__IAPI_instance).m_growTime;
            set => ((EggGrow_Alias)__IAPI_instance).m_growTime = value;
        }


        public float m_updateInterval
        {
            get => ((EggGrow_Alias)__IAPI_instance).m_updateInterval;
            set => ((EggGrow_Alias)__IAPI_instance).m_updateInterval = value;
        }
        public bool m_requireNearbyFire
        {
            get => ((EggGrow_Alias)__IAPI_instance).m_requireNearbyFire;
            set => ((EggGrow_Alias)__IAPI_instance).m_requireNearbyFire = value;
        }
        public bool m_requireUnderRoof
        {
            get => ((EggGrow_Alias)__IAPI_instance).m_requireUnderRoof;
            set => ((EggGrow_Alias)__IAPI_instance).m_requireUnderRoof = value;
        }
        public float m_requireCoverPercentige
        {
            get => ((EggGrow_Alias)__IAPI_instance).m_requireCoverPercentige;
            set => ((EggGrow_Alias)__IAPI_instance).m_requireCoverPercentige = value;
        }

        public UnityEngine_GameObject_Alias m_growingObject
        {
            get => ((EggGrow_Alias)__IAPI_instance).m_growingObject;
            set => ((EggGrow_Alias)__IAPI_instance).m_growingObject = value;
        }
        public UnityEngine_GameObject_Alias m_notGrowingObject
        {
            get => ((EggGrow_Alias)__IAPI_instance).m_notGrowingObject;
            set => ((EggGrow_Alias)__IAPI_instance).m_notGrowingObject = value;
        }



        public static readonly VoidMethodInvoker __IAPI_UpdateEffects_Invoker1 = new VoidMethodInvoker(typeof(EggGrow_Alias), "UpdateEffects", new ParamSig[] { new NonGenericParamSig(typeof(float), false) });
        public void UpdateEffects(float grow) => __IAPI_UpdateEffects_Invoker1.Invoke(((EggGrow_Alias)__IAPI_instance), new object[] { grow });


        public static readonly TypedMethodInvoker<bool> __IAPI_CanGrow_Invoker1 = new TypedMethodInvoker<bool>(typeof(EggGrow_Alias), "CanGrow", new ParamSig[] { });
        public bool CanGrow() => __IAPI_CanGrow_Invoker1.Invoke(((EggGrow_Alias)__IAPI_instance), new object[] { });




    }
}
