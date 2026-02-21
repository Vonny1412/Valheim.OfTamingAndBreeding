using BaseAI_Alias = BaseAI;
using Character_Alias = Character;
using Tameable_Alias = Tameable;
using UnityEngine_Vector3_Alias = UnityEngine.Vector3;
using ZSyncAnimation_Alias = ZSyncAnimation;
using UnityEngine_GameObject_Alias = UnityEngine.GameObject;
using ZNetView_Alias = ZNetView;

namespace OfTamingAndBreeding.ValheimAPI.LowLevel
{
    public partial class BaseAI : UnityEngine.MonoBehaviour
    {
        public BaseAI(BaseAI_Alias instance) : base(instance)
        {
        }

        public static readonly Core.Invokers.FieldMutateInvoker<UnityEngine_Vector3_Alias> __IAPI_m_spawnPoint_Invoker = new Core.Invokers.FieldMutateInvoker<UnityEngine_Vector3_Alias>(typeof(BaseAI_Alias), "m_spawnPoint");

        public static readonly Core.Invokers.VoidMethodInvoker __IAPI_RandomMovement_Invoker1 = new Core.Invokers.VoidMethodInvoker(typeof(BaseAI_Alias), "RandomMovement", new Core.Signatures.ParamSig[] { new Core.Signatures.NonGenericParamSig(typeof(float), false), new Core.Signatures.NonGenericParamSig(typeof(UnityEngine_Vector3_Alias), false), new Core.Signatures.NonGenericParamSig(typeof(bool), false) });

        public static readonly Core.Invokers.FieldMutateInvoker<System.Collections.Generic.List<BaseAI_Alias>> __IAPI_m_instances_Invoker = new Core.Invokers.FieldMutateInvoker<System.Collections.Generic.List<BaseAI_Alias>>(typeof(BaseAI_Alias), "m_instances");

        public static readonly Core.Invokers.FieldMutateInvoker<bool> __IAPI_m_alerted_Invoker = new Core.Invokers.FieldMutateInvoker<bool>(typeof(BaseAI_Alias), "m_alerted");

        public static readonly Core.Invokers.VoidMethodInvoker __IAPI_SetAlerted_Invoker1 = new Core.Invokers.VoidMethodInvoker(typeof(BaseAI_Alias), "SetAlerted", new Core.Signatures.ParamSig[] { new Core.Signatures.NonGenericParamSig(typeof(bool), false) });

        public static readonly Core.Invokers.FieldMutateInvoker<Tameable_Alias> __IAPI_m_tamable_Invoker = new Core.Invokers.FieldMutateInvoker<Tameable_Alias>(typeof(BaseAI_Alias), "m_tamable");

        public static readonly Core.Invokers.TypedMethodInvoker<bool> __IAPI_MoveTo_Invoker1 = new Core.Invokers.TypedMethodInvoker<bool>(typeof(BaseAI_Alias), "MoveTo", new Core.Signatures.ParamSig[] { new Core.Signatures.NonGenericParamSig(typeof(float), false), new Core.Signatures.NonGenericParamSig(typeof(UnityEngine_Vector3_Alias), false), new Core.Signatures.NonGenericParamSig(typeof(float), false), new Core.Signatures.NonGenericParamSig(typeof(bool), false) });

        public static readonly Core.Invokers.VoidMethodInvoker __IAPI_LookAt_Invoker1 = new Core.Invokers.VoidMethodInvoker(typeof(BaseAI_Alias), "LookAt", new Core.Signatures.ParamSig[] { new Core.Signatures.NonGenericParamSig(typeof(UnityEngine_Vector3_Alias), false) });

        public static readonly Core.Invokers.TypedMethodInvoker<bool> __IAPI_HavePath_Invoker1 = new Core.Invokers.TypedMethodInvoker<bool>(typeof(BaseAI_Alias), "HavePath", new Core.Signatures.ParamSig[] { new Core.Signatures.NonGenericParamSig(typeof(UnityEngine_Vector3_Alias), false) });

        public static readonly Core.Invokers.TypedMethodInvoker<bool> __IAPI_IsLookingAt_Invoker1 = new Core.Invokers.TypedMethodInvoker<bool>(typeof(BaseAI_Alias), "IsLookingAt", new Core.Signatures.ParamSig[] { new Core.Signatures.NonGenericParamSig(typeof(UnityEngine_Vector3_Alias), false), new Core.Signatures.NonGenericParamSig(typeof(float), false), new Core.Signatures.NonGenericParamSig(typeof(bool), false) });

        public static readonly Core.Invokers.FieldMutateInvoker<ZSyncAnimation_Alias> __IAPI_m_animator_Invoker = new Core.Invokers.FieldMutateInvoker<ZSyncAnimation_Alias>(typeof(BaseAI_Alias), "m_animator");

        public static readonly Core.Invokers.FieldMutateInvoker<Character_Alias> __IAPI_m_character_Invoker = new Core.Invokers.FieldMutateInvoker<Character_Alias>(typeof(BaseAI_Alias), "m_character");

        public static readonly Core.Invokers.FieldMutateInvoker<float> __IAPI_m_fleeTargetUpdateTime_Invoker = new Core.Invokers.FieldMutateInvoker<float>(typeof(BaseAI_Alias), "m_fleeTargetUpdateTime");

        public static readonly Core.Invokers.VoidMethodInvoker __IAPI_Follow_Invoker1 = new Core.Invokers.VoidMethodInvoker(typeof(BaseAI_Alias), "Follow", new Core.Signatures.ParamSig[] { new Core.Signatures.NonGenericParamSig(typeof(UnityEngine_GameObject_Alias), false), new Core.Signatures.NonGenericParamSig(typeof(float), false) });

        public static readonly Core.Invokers.FieldMutateInvoker<ZNetView_Alias> __IAPI_m_nview_Invoker = new Core.Invokers.FieldMutateInvoker<ZNetView_Alias>(typeof(BaseAI_Alias), "m_nview");

        public static readonly Core.Invokers.VoidMethodInvoker __IAPI_SetPatrolPoint_Invoker2 = new Core.Invokers.VoidMethodInvoker(typeof(BaseAI_Alias), "SetPatrolPoint", new Core.Signatures.ParamSig[] { new Core.Signatures.NonGenericParamSig(typeof(UnityEngine_Vector3_Alias), false) });

        public static readonly Core.Invokers.TypedMethodInvoker<bool> __IAPI_GetPatrolPoint_Invoker1 = new Core.Invokers.TypedMethodInvoker<bool>(typeof(BaseAI_Alias), "GetPatrolPoint", new Core.Signatures.ParamSig[] { new Core.Signatures.NonGenericParamSig(typeof(UnityEngine_Vector3_Alias), true) });

        public static readonly Core.Invokers.FieldMutateInvoker<float> __IAPI_m_jumpTimer_Invoker = new Core.Invokers.FieldMutateInvoker<float>(typeof(BaseAI_Alias), "m_jumpTimer");

        public static readonly Core.Invokers.FieldMutateInvoker<float> __IAPI_m_randomMoveUpdateTimer_Invoker = new Core.Invokers.FieldMutateInvoker<float>(typeof(BaseAI_Alias), "m_randomMoveUpdateTimer");

        public static readonly Core.Invokers.FieldMutateInvoker<float> __IAPI_m_timeSinceHurt_Invoker = new Core.Invokers.FieldMutateInvoker<float>(typeof(BaseAI_Alias), "m_timeSinceHurt");

        public static readonly Core.Invokers.VoidMethodInvoker __IAPI_UpdateRegeneration_Invoker1 = new Core.Invokers.VoidMethodInvoker(typeof(BaseAI_Alias), "UpdateRegeneration", new Core.Signatures.ParamSig[] { new Core.Signatures.NonGenericParamSig(typeof(float), false) });

        public static readonly Core.Invokers.VoidMethodInvoker __IAPI_UpdateTakeoffLanding_Invoker1 = new Core.Invokers.VoidMethodInvoker(typeof(BaseAI_Alias), "UpdateTakeoffLanding", new Core.Signatures.ParamSig[] { new Core.Signatures.NonGenericParamSig(typeof(float), false) });


    }
}
