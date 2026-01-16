using BaseAI_Alias = BaseAI;
using Character_Alias = Character;
using Tameable_Alias = Tameable;
using UnityEngine_Vector3_Alias = UnityEngine.Vector3;
using ZSyncAnimation_Alias = ZSyncAnimation;

using OfTamingAndBreeding.Internals.API.Core;
using OfTamingAndBreeding.Internals.API.Core.Invokers;
using OfTamingAndBreeding.Internals.API.Core.Signatures;
namespace OfTamingAndBreeding.Internals.API
{
    public partial class BaseAI : UnityEngine.MonoBehaviour
    {

        public BaseAI(BaseAI_Alias instance) : base(instance)
        {
        }

        public bool IsAlerted() => ((BaseAI_Alias)__IAPI_instance).IsAlerted();



        public static readonly VoidMethodInvoker __IAPI_SetAlerted_Invoker1 = new VoidMethodInvoker(typeof(BaseAI_Alias), "SetAlerted", new ParamSig[] { new NonGenericParamSig(typeof(bool), false) });
        public virtual void SetAlerted(bool alert) => __IAPI_SetAlerted_Invoker1.Invoke(((BaseAI_Alias)__IAPI_instance), new object[] { alert });

        public static readonly FieldMutateInvoker<Tameable_Alias> __IAPI_m_tamable_Invoker = new FieldMutateInvoker<Tameable_Alias>(typeof(BaseAI_Alias), "m_tamable");
        public Tameable_Alias m_tamable
        {
            get => __IAPI_m_tamable_Invoker.Get(((BaseAI_Alias)__IAPI_instance));
            set => __IAPI_m_tamable_Invoker.Set(((BaseAI_Alias)__IAPI_instance), value);
        }

        public static readonly TypedMethodInvoker<bool> __IAPI_MoveTo_Invoker1 = new TypedMethodInvoker<bool>(typeof(BaseAI_Alias), "MoveTo", new ParamSig[] { new NonGenericParamSig(typeof(float), false), new NonGenericParamSig(typeof(UnityEngine_Vector3_Alias), false), new NonGenericParamSig(typeof(float), false), new NonGenericParamSig(typeof(bool), false) });
        public bool MoveTo(float dt, UnityEngine_Vector3_Alias point, float dist, bool run) => __IAPI_MoveTo_Invoker1.Invoke(((BaseAI_Alias)__IAPI_instance), new object[] { dt, point, dist, run });


        public static readonly VoidMethodInvoker __IAPI_LookAt_Invoker1 = new VoidMethodInvoker(typeof(BaseAI_Alias), "LookAt", new ParamSig[] { new NonGenericParamSig(typeof(UnityEngine_Vector3_Alias), false) });
        public void LookAt(UnityEngine_Vector3_Alias point) => __IAPI_LookAt_Invoker1.Invoke(((BaseAI_Alias)__IAPI_instance), new object[] { point });


        public static readonly TypedMethodInvoker<bool> __IAPI_HavePath_Invoker1 = new TypedMethodInvoker<bool>(typeof(BaseAI_Alias), "HavePath", new ParamSig[] { new NonGenericParamSig(typeof(UnityEngine_Vector3_Alias), false) });
        public bool HavePath(UnityEngine_Vector3_Alias target) => __IAPI_HavePath_Invoker1.Invoke(((BaseAI_Alias)__IAPI_instance), new object[] { target });

        public static readonly TypedMethodInvoker<bool> __IAPI_IsLookingAt_Invoker1 = new TypedMethodInvoker<bool>(typeof(BaseAI_Alias), "IsLookingAt", new ParamSig[] { new NonGenericParamSig(typeof(UnityEngine_Vector3_Alias), false), new NonGenericParamSig(typeof(float), false), new NonGenericParamSig(typeof(bool), false) });
        public bool IsLookingAt(UnityEngine_Vector3_Alias point, float minAngle, bool inverted = false) => __IAPI_IsLookingAt_Invoker1.Invoke(((BaseAI_Alias)__IAPI_instance), new object[] { point, minAngle, inverted });

        public static readonly FieldMutateInvoker<ZSyncAnimation_Alias> __IAPI_m_animator_Invoker = new FieldMutateInvoker<ZSyncAnimation_Alias>(typeof(BaseAI_Alias), "m_animator");
        public ZSyncAnimation_Alias m_animator
        {
            get => __IAPI_m_animator_Invoker.Get(((BaseAI_Alias)__IAPI_instance));
            set => __IAPI_m_animator_Invoker.Set(((BaseAI_Alias)__IAPI_instance), value);
        }


        public static readonly FieldMutateInvoker<Character_Alias> __IAPI_m_character_Invoker = new FieldMutateInvoker<Character_Alias>(typeof(BaseAI_Alias), "m_character");
        public Character_Alias m_character
        {
            get => __IAPI_m_character_Invoker.Get(((BaseAI_Alias)__IAPI_instance));
            set => __IAPI_m_character_Invoker.Set(((BaseAI_Alias)__IAPI_instance), value);
        }

        public static readonly FieldMutateInvoker<float> __IAPI_m_fleeTargetUpdateTime_Invoker = new FieldMutateInvoker<float>(typeof(BaseAI_Alias), "m_fleeTargetUpdateTime");
        public float m_fleeTargetUpdateTime
        {
            get => __IAPI_m_fleeTargetUpdateTime_Invoker.Get(((BaseAI_Alias)__IAPI_instance));
            set => __IAPI_m_fleeTargetUpdateTime_Invoker.Set(((BaseAI_Alias)__IAPI_instance), value);
        }


    }
}
