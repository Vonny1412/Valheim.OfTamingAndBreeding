using BaseAI_Alias = BaseAI;
using Character_Alias = Character;
using UnityEngine_GameObject_Alias = UnityEngine.GameObject;
using Procreation_Alias = Procreation;
using Tameable_Alias = Tameable;
using ZNetView_Alias = ZNetView;

namespace OfTamingAndBreeding.ValheimAPI.LowLevel
{
    public partial class Procreation : UnityEngine.MonoBehaviour
    {
        public Procreation(Procreation_Alias instance) : base(instance)
        {
        }

        public static readonly Core.Invokers.FieldMutateInvoker<UnityEngine_GameObject_Alias> __IAPI_m_myPrefab_Invoker = new Core.Invokers.FieldMutateInvoker<UnityEngine_GameObject_Alias>(typeof(Procreation_Alias), "m_myPrefab");

        public static readonly Core.Invokers.FieldMutateInvoker<UnityEngine_GameObject_Alias> __IAPI_m_offspringPrefab_Invoker = new Core.Invokers.FieldMutateInvoker<UnityEngine_GameObject_Alias>(typeof(Procreation_Alias), "m_offspringPrefab");

        public static readonly Core.Invokers.FieldMutateInvoker<ZNetView_Alias> __IAPI_m_nview_Invoker = new Core.Invokers.FieldMutateInvoker<ZNetView_Alias>(typeof(Procreation_Alias), "m_nview");

        public static readonly Core.Invokers.FieldMutateInvoker<BaseAI_Alias> __IAPI_m_baseAI_Invoker = new Core.Invokers.FieldMutateInvoker<BaseAI_Alias>(typeof(Procreation_Alias), "m_baseAI");

        public static readonly Core.Invokers.FieldMutateInvoker<Character_Alias> __IAPI_m_character_Invoker = new Core.Invokers.FieldMutateInvoker<Character_Alias>(typeof(Procreation_Alias), "m_character");

        public static readonly Core.Invokers.FieldMutateInvoker<Tameable_Alias> __IAPI_m_tameable_Invoker = new Core.Invokers.FieldMutateInvoker<Tameable_Alias>(typeof(Procreation_Alias), "m_tameable");

        public static readonly Core.Invokers.TypedMethodInvoker<bool> __IAPI_IsPregnant_Invoker1 = new Core.Invokers.TypedMethodInvoker<bool>(typeof(Procreation_Alias), "IsPregnant", new Core.Signatures.ParamSig[] { });

        public static readonly Core.Invokers.VoidMethodInvoker __IAPI_Procreate_Invoker1 = new Core.Invokers.VoidMethodInvoker(typeof(Procreation_Alias), "Procreate", new Core.Signatures.ParamSig[] { });

        public static readonly Core.Invokers.VoidMethodInvoker __IAPI_MakePregnant_Invoker1 = new Core.Invokers.VoidMethodInvoker(typeof(Procreation_Alias), "MakePregnant", new Core.Signatures.ParamSig[] { });
        
        public static readonly Core.Invokers.VoidMethodInvoker __IAPI_ResetPregnancy_Invoker1 = new Core.Invokers.VoidMethodInvoker(typeof(Procreation_Alias), "ResetPregnancy", new Core.Signatures.ParamSig[] { });

        public static readonly Core.Invokers.TypedMethodInvoker<bool> __IAPI_IsDue_Invoker1 = new Core.Invokers.TypedMethodInvoker<bool>(typeof(Procreation_Alias), "IsDue", new Core.Signatures.ParamSig[] { });

    }
}
