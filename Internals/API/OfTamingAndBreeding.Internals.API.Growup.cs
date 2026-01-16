using BaseAI_Alias = BaseAI;
using UnityEngine_GameObject_Alias = UnityEngine.GameObject;
using Growup_GrownEntry_Alias = Growup.GrownEntry;
using Growup_Alias = Growup;
using ZNetView_Alias = ZNetView;

using OfTamingAndBreeding.Internals.API.Core;
using OfTamingAndBreeding.Internals.API.Core.Invokers;
using OfTamingAndBreeding.Internals.API.Core.Signatures;
namespace OfTamingAndBreeding.Internals.API
{
    public partial class Growup : UnityEngine.MonoBehaviour
    {
        public Growup(Growup_Alias instance) : base(instance)
        {
        }

        public float m_growTime
        {
            get => ((Growup_Alias)__IAPI_instance).m_growTime;
            set => ((Growup_Alias)__IAPI_instance).m_growTime = value;
        }
        public bool m_inheritTame
        {
            get => ((Growup_Alias)__IAPI_instance).m_inheritTame;
            set => ((Growup_Alias)__IAPI_instance).m_inheritTame = value;
        }
        public UnityEngine_GameObject_Alias m_grownPrefab
        {
            get => ((Growup_Alias)__IAPI_instance).m_grownPrefab;
            set => ((Growup_Alias)__IAPI_instance).m_grownPrefab = value;
        }
        public System.Collections.Generic.List<Growup_GrownEntry_Alias> m_altGrownPrefabs
        {
            get => ((Growup_Alias)__IAPI_instance).m_altGrownPrefabs;
            set => ((Growup_Alias)__IAPI_instance).m_altGrownPrefabs = value;
        }

        public static readonly FieldMutateInvoker<BaseAI_Alias> __IAPI_m_baseAI_Invoker = new FieldMutateInvoker<BaseAI_Alias>(typeof(Growup_Alias), "m_baseAI");
        public BaseAI_Alias m_baseAI
        {
            get => __IAPI_m_baseAI_Invoker.Get(((Growup_Alias)__IAPI_instance));
            set => __IAPI_m_baseAI_Invoker.Set(((Growup_Alias)__IAPI_instance), value);
        }
        public static readonly FieldMutateInvoker<ZNetView_Alias> __IAPI_m_nview_Invoker = new FieldMutateInvoker<ZNetView_Alias>(typeof(Growup_Alias), "m_nview");
        public ZNetView_Alias m_nview
        {
            get => __IAPI_m_nview_Invoker.Get(((Growup_Alias)__IAPI_instance));
            set => __IAPI_m_nview_Invoker.Set(((Growup_Alias)__IAPI_instance), value);
        }



    }
}
