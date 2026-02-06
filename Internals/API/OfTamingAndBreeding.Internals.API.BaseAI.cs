using BaseAI_Alias = BaseAI;
using Character_Alias = Character;
using Tameable_Alias = Tameable;
using UnityEngine_Vector3_Alias = UnityEngine.Vector3;
using ZSyncAnimation_Alias = ZSyncAnimation;
using UnityEngine_GameObject_Alias = UnityEngine.GameObject;
using ZNetView_Alias = ZNetView;

namespace OfTamingAndBreeding.Internals.API
{
    public partial class BaseAI : UnityEngine.MonoBehaviour
    {
        public BaseAI(BaseAI_Alias instance) : base(instance)
        {
        }

        public new BaseAI_Alias __IAPI_GetInstance() => (BaseAI_Alias)__IAPI_instance;





        public bool IsAlerted() => ((BaseAI_Alias)__IAPI_instance).IsAlerted();



        public static readonly Core.Invokers.VoidMethodInvoker __IAPI_SetAlerted_Invoker1 = new Core.Invokers.VoidMethodInvoker(typeof(BaseAI_Alias), "SetAlerted", new Core.Signatures.ParamSig[] { new Core.Signatures.NonGenericParamSig(typeof(bool), false) });
        public virtual void SetAlerted(bool alert) => __IAPI_SetAlerted_Invoker1.Invoke(((BaseAI_Alias)__IAPI_instance), new object[] { alert });

        public static readonly Core.Invokers.FieldMutateInvoker<Tameable_Alias> __IAPI_m_tamable_Invoker = new Core.Invokers.FieldMutateInvoker<Tameable_Alias>(typeof(BaseAI_Alias), "m_tamable");
        public Tameable_Alias m_tamable
        {
            get => __IAPI_m_tamable_Invoker.Get(((BaseAI_Alias)__IAPI_instance));
            set => __IAPI_m_tamable_Invoker.Set(((BaseAI_Alias)__IAPI_instance), value);
        }

        public static readonly Core.Invokers.TypedMethodInvoker<bool> __IAPI_MoveTo_Invoker1 = new Core.Invokers.TypedMethodInvoker<bool>(typeof(BaseAI_Alias), "MoveTo", new Core.Signatures.ParamSig[] { new Core.Signatures.NonGenericParamSig(typeof(float), false), new Core.Signatures.NonGenericParamSig(typeof(UnityEngine_Vector3_Alias), false), new Core.Signatures.NonGenericParamSig(typeof(float), false), new Core.Signatures.NonGenericParamSig(typeof(bool), false) });
        public bool MoveTo(float dt, UnityEngine_Vector3_Alias point, float dist, bool run) => __IAPI_MoveTo_Invoker1.Invoke(((BaseAI_Alias)__IAPI_instance), new object[] { dt, point, dist, run });


        public static readonly Core.Invokers.VoidMethodInvoker __IAPI_LookAt_Invoker1 = new Core.Invokers.VoidMethodInvoker(typeof(BaseAI_Alias), "LookAt", new Core.Signatures.ParamSig[] { new Core.Signatures.NonGenericParamSig(typeof(UnityEngine_Vector3_Alias), false) });
        public void LookAt(UnityEngine_Vector3_Alias point) => __IAPI_LookAt_Invoker1.Invoke(((BaseAI_Alias)__IAPI_instance), new object[] { point });


        public static readonly Core.Invokers.TypedMethodInvoker<bool> __IAPI_HavePath_Invoker1 = new Core.Invokers.TypedMethodInvoker<bool>(typeof(BaseAI_Alias), "HavePath", new Core.Signatures.ParamSig[] { new Core.Signatures.NonGenericParamSig(typeof(UnityEngine_Vector3_Alias), false) });
        public bool HavePath(UnityEngine_Vector3_Alias target) => __IAPI_HavePath_Invoker1.Invoke(((BaseAI_Alias)__IAPI_instance), new object[] { target });

        public static readonly Core.Invokers.TypedMethodInvoker<bool> __IAPI_IsLookingAt_Invoker1 = new Core.Invokers.TypedMethodInvoker<bool>(typeof(BaseAI_Alias), "IsLookingAt", new Core.Signatures.ParamSig[] { new Core.Signatures.NonGenericParamSig(typeof(UnityEngine_Vector3_Alias), false), new Core.Signatures.NonGenericParamSig(typeof(float), false), new Core.Signatures.NonGenericParamSig(typeof(bool), false) });
        public bool IsLookingAt(UnityEngine_Vector3_Alias point, float minAngle, bool inverted = false) => __IAPI_IsLookingAt_Invoker1.Invoke(((BaseAI_Alias)__IAPI_instance), new object[] { point, minAngle, inverted });

        public static readonly Core.Invokers.FieldMutateInvoker<ZSyncAnimation_Alias> __IAPI_m_animator_Invoker = new Core.Invokers.FieldMutateInvoker<ZSyncAnimation_Alias>(typeof(BaseAI_Alias), "m_animator");
        public ZSyncAnimation_Alias m_animator
        {
            get => __IAPI_m_animator_Invoker.Get(((BaseAI_Alias)__IAPI_instance));
            set => __IAPI_m_animator_Invoker.Set(((BaseAI_Alias)__IAPI_instance), value);
        }


        public static readonly Core.Invokers.FieldMutateInvoker<Character_Alias> __IAPI_m_character_Invoker = new Core.Invokers.FieldMutateInvoker<Character_Alias>(typeof(BaseAI_Alias), "m_character");
        public Character_Alias m_character
        {
            get => __IAPI_m_character_Invoker.Get(((BaseAI_Alias)__IAPI_instance));
            set => __IAPI_m_character_Invoker.Set(((BaseAI_Alias)__IAPI_instance), value);
        }

        public static readonly Core.Invokers.FieldMutateInvoker<float> __IAPI_m_fleeTargetUpdateTime_Invoker = new Core.Invokers.FieldMutateInvoker<float>(typeof(BaseAI_Alias), "m_fleeTargetUpdateTime");
        public float m_fleeTargetUpdateTime
        {
            get => __IAPI_m_fleeTargetUpdateTime_Invoker.Get(((BaseAI_Alias)__IAPI_instance));
            set => __IAPI_m_fleeTargetUpdateTime_Invoker.Set(((BaseAI_Alias)__IAPI_instance), value);
        }



        public static readonly Core.Invokers.VoidMethodInvoker __IAPI_Follow_Invoker1 = new Core.Invokers.VoidMethodInvoker(typeof(BaseAI_Alias), "Follow", new Core.Signatures.ParamSig[] { new Core.Signatures.NonGenericParamSig(typeof(UnityEngine_GameObject_Alias), false), new Core.Signatures.NonGenericParamSig(typeof(float), false) });
        public void Follow(UnityEngine_GameObject_Alias go, float dt) => __IAPI_Follow_Invoker1.Invoke(((BaseAI_Alias)__IAPI_instance), new object[] { go, dt });




        public static readonly Core.Invokers.FieldMutateInvoker<ZNetView_Alias> __IAPI_m_nview_Invoker = new Core.Invokers.FieldMutateInvoker<ZNetView_Alias>(typeof(BaseAI_Alias), "m_nview");
        public ZNetView_Alias m_nview
        {
            get => __IAPI_m_nview_Invoker.Get(((BaseAI_Alias)__IAPI_instance));
            set => __IAPI_m_nview_Invoker.Set(((BaseAI_Alias)__IAPI_instance), value);

        }




        public void SetPatrolPoint() => ((BaseAI_Alias)__IAPI_instance).SetPatrolPoint();
        public static readonly Core.Invokers.VoidMethodInvoker __IAPI_SetPatrolPoint_Invoker2 = new Core.Invokers.VoidMethodInvoker(typeof(BaseAI_Alias), "SetPatrolPoint", new Core.Signatures.ParamSig[] { new Core.Signatures.NonGenericParamSig(typeof(UnityEngine_Vector3_Alias), false) });
        public void SetPatrolPoint(UnityEngine_Vector3_Alias point) => __IAPI_SetPatrolPoint_Invoker2.Invoke(((BaseAI_Alias)__IAPI_instance), new object[] { point });
        public void ResetPatrolPoint() => ((BaseAI_Alias)__IAPI_instance).ResetPatrolPoint();
        public static readonly Core.Invokers.TypedMethodInvoker<bool> __IAPI_GetPatrolPoint_Invoker1 = new Core.Invokers.TypedMethodInvoker<bool>(typeof(BaseAI_Alias), "GetPatrolPoint", new Core.Signatures.ParamSig[] { new Core.Signatures.NonGenericParamSig(typeof(UnityEngine_Vector3_Alias), true) });
        public bool GetPatrolPoint(out UnityEngine_Vector3_Alias point)
        {
            object[] args = new object[] { default(UnityEngine_Vector3_Alias) };
            bool result = __IAPI_GetPatrolPoint_Invoker1.Invoke(((BaseAI_Alias)__IAPI_instance), args);
            point = (UnityEngine_Vector3_Alias)args[0];
            return result;
        }


    }
}
