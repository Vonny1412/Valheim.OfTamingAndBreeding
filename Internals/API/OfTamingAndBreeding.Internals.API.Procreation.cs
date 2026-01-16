
using BaseAI_Alias = BaseAI;
using Character_Alias = Character;
using EffectList_Alias = EffectList;
using UnityEngine_GameObject_Alias = UnityEngine.GameObject;
using Procreation_Alias = Procreation;
using Tameable_Alias = Tameable;
using ZNetView_Alias = ZNetView;

using OfTamingAndBreeding.Internals.API.Core;
using OfTamingAndBreeding.Internals.API.Core.Invokers;
using OfTamingAndBreeding.Internals.API.Core.Signatures;
namespace OfTamingAndBreeding.Internals.API
{
    public partial class Procreation : UnityEngine.MonoBehaviour
    {
        public Procreation(Procreation_Alias instance) : base(instance)
        {
        }





        public float m_updateInterval
        {
            get => ((Procreation_Alias)__IAPI_instance).m_updateInterval;
            set => ((Procreation_Alias)__IAPI_instance).m_updateInterval = value;
        }
        public float m_totalCheckRange
        {
            get => ((Procreation_Alias)__IAPI_instance).m_totalCheckRange;
            set => ((Procreation_Alias)__IAPI_instance).m_totalCheckRange = value;
        }
        public int m_maxCreatures
        {
            get => ((Procreation_Alias)__IAPI_instance).m_maxCreatures;
            set => ((Procreation_Alias)__IAPI_instance).m_maxCreatures = value;
        }
        public float m_partnerCheckRange
        {
            get => ((Procreation_Alias)__IAPI_instance).m_partnerCheckRange;
            set => ((Procreation_Alias)__IAPI_instance).m_partnerCheckRange = value;
        }
        public float m_pregnancyChance
        {
            get => ((Procreation_Alias)__IAPI_instance).m_pregnancyChance;
            set => ((Procreation_Alias)__IAPI_instance).m_pregnancyChance = value;
        }
        public float m_pregnancyDuration
        {
            get => ((Procreation_Alias)__IAPI_instance).m_pregnancyDuration;
            set => ((Procreation_Alias)__IAPI_instance).m_pregnancyDuration = value;
        }
        public int m_requiredLovePoints
        {
            get => ((Procreation_Alias)__IAPI_instance).m_requiredLovePoints;
            set => ((Procreation_Alias)__IAPI_instance).m_requiredLovePoints = value;
        }
        public UnityEngine_GameObject_Alias m_offspring
        {
            get => ((Procreation_Alias)__IAPI_instance).m_offspring;
            set => ((Procreation_Alias)__IAPI_instance).m_offspring = value;
        }
        public int m_minOffspringLevel
        {
            get => ((Procreation_Alias)__IAPI_instance).m_minOffspringLevel;
            set => ((Procreation_Alias)__IAPI_instance).m_minOffspringLevel = value;
        }
        public float m_spawnOffset
        {
            get => ((Procreation_Alias)__IAPI_instance).m_spawnOffset;
            set => ((Procreation_Alias)__IAPI_instance).m_spawnOffset = value;
        }
        public float m_spawnOffsetMax
        {
            get => ((Procreation_Alias)__IAPI_instance).m_spawnOffsetMax;
            set => ((Procreation_Alias)__IAPI_instance).m_spawnOffsetMax = value;
        }
        public bool m_spawnRandomDirection
        {
            get => ((Procreation_Alias)__IAPI_instance).m_spawnRandomDirection;
            set => ((Procreation_Alias)__IAPI_instance).m_spawnRandomDirection = value;
        }
        public UnityEngine_GameObject_Alias m_seperatePartner
        {
            get => ((Procreation_Alias)__IAPI_instance).m_seperatePartner;
            set => ((Procreation_Alias)__IAPI_instance).m_seperatePartner = value;
        }
        public UnityEngine_GameObject_Alias m_noPartnerOffspring
        {
            get => ((Procreation_Alias)__IAPI_instance).m_noPartnerOffspring;
            set => ((Procreation_Alias)__IAPI_instance).m_noPartnerOffspring = value;
        }
        public EffectList_Alias m_birthEffects
        {
            get => ((Procreation_Alias)__IAPI_instance).m_birthEffects;
            set => ((Procreation_Alias)__IAPI_instance).m_birthEffects = value;
        }
        public EffectList_Alias m_loveEffects
        {
            get => ((Procreation_Alias)__IAPI_instance).m_loveEffects;
            set => ((Procreation_Alias)__IAPI_instance).m_loveEffects = value;
        }


        public static readonly FieldMutateInvoker<UnityEngine_GameObject_Alias> __IAPI_m_myPrefab_Invoker = new FieldMutateInvoker<UnityEngine_GameObject_Alias>(typeof(Procreation_Alias), "m_myPrefab");
        public UnityEngine_GameObject_Alias m_myPrefab
        {
            get => __IAPI_m_myPrefab_Invoker.Get(((Procreation_Alias)__IAPI_instance));
            set => __IAPI_m_myPrefab_Invoker.Set(((Procreation_Alias)__IAPI_instance), value);
        }
        public static readonly FieldMutateInvoker<UnityEngine_GameObject_Alias> __IAPI_m_offspringPrefab_Invoker = new FieldMutateInvoker<UnityEngine_GameObject_Alias>(typeof(Procreation_Alias), "m_offspringPrefab");
        public UnityEngine_GameObject_Alias m_offspringPrefab
        {
            get => __IAPI_m_offspringPrefab_Invoker.Get(((Procreation_Alias)__IAPI_instance));
            set => __IAPI_m_offspringPrefab_Invoker.Set(((Procreation_Alias)__IAPI_instance), value);
        }
        public static readonly FieldMutateInvoker<ZNetView_Alias> __IAPI_m_nview_Invoker = new FieldMutateInvoker<ZNetView_Alias>(typeof(Procreation_Alias), "m_nview");
        public ZNetView_Alias m_nview
        {
            get => __IAPI_m_nview_Invoker.Get(((Procreation_Alias)__IAPI_instance));
            set => __IAPI_m_nview_Invoker.Set(((Procreation_Alias)__IAPI_instance), value);
        }

        public static readonly FieldMutateInvoker<BaseAI_Alias> __IAPI_m_baseAI_Invoker = new FieldMutateInvoker<BaseAI_Alias>(typeof(Procreation_Alias), "m_baseAI");
        public BaseAI_Alias m_baseAI
        {
            get => __IAPI_m_baseAI_Invoker.Get(((Procreation_Alias)__IAPI_instance));
            set => __IAPI_m_baseAI_Invoker.Set(((Procreation_Alias)__IAPI_instance), value);
        }
        public static readonly FieldMutateInvoker<Character_Alias> __IAPI_m_character_Invoker = new FieldMutateInvoker<Character_Alias>(typeof(Procreation_Alias), "m_character");
        public Character_Alias m_character
        {
            get => __IAPI_m_character_Invoker.Get(((Procreation_Alias)__IAPI_instance));
            set => __IAPI_m_character_Invoker.Set(((Procreation_Alias)__IAPI_instance), value);
        }
        public static readonly FieldMutateInvoker<Tameable_Alias> __IAPI_m_tameable_Invoker = new FieldMutateInvoker<Tameable_Alias>(typeof(Procreation_Alias), "m_tameable");
        public Tameable_Alias m_tameable
        {
            get => __IAPI_m_tameable_Invoker.Get(((Procreation_Alias)__IAPI_instance));
            set => __IAPI_m_tameable_Invoker.Set(((Procreation_Alias)__IAPI_instance), value);
        }




        public int GetLovePoints() => ((Procreation_Alias)__IAPI_instance).GetLovePoints();


        public static readonly TypedMethodInvoker<bool> __IAPI_IsPregnant_Invoker1 = new TypedMethodInvoker<bool>(typeof(Procreation_Alias), "IsPregnant", new ParamSig[] { });
        public bool IsPregnant() => __IAPI_IsPregnant_Invoker1.Invoke(((Procreation_Alias)__IAPI_instance), new object[] { });



        public static readonly VoidMethodInvoker __IAPI_Procreate_Invoker1 = new VoidMethodInvoker(typeof(Procreation_Alias), "Procreate", new ParamSig[] { });
        public void Procreate() => __IAPI_Procreate_Invoker1.Invoke(((Procreation_Alias)__IAPI_instance), new object[] { });

        public bool ReadyForProcreation() => ((Procreation_Alias)__IAPI_instance).ReadyForProcreation();
        public static readonly VoidMethodInvoker __IAPI_MakePregnant_Invoker1 = new VoidMethodInvoker(typeof(Procreation_Alias), "MakePregnant", new ParamSig[] { });
        public void MakePregnant() => __IAPI_MakePregnant_Invoker1.Invoke(((Procreation_Alias)__IAPI_instance), new object[] { });
        public static readonly VoidMethodInvoker __IAPI_ResetPregnancy_Invoker1 = new VoidMethodInvoker(typeof(Procreation_Alias), "ResetPregnancy", new ParamSig[] { });
        public void ResetPregnancy() => __IAPI_ResetPregnancy_Invoker1.Invoke(((Procreation_Alias)__IAPI_instance), new object[] { });
        public static readonly TypedMethodInvoker<bool> __IAPI_IsDue_Invoker1 = new TypedMethodInvoker<bool>(typeof(Procreation_Alias), "IsDue", new ParamSig[] { });
        public bool IsDue() => __IAPI_IsDue_Invoker1.Invoke(((Procreation_Alias)__IAPI_instance), new object[] { });



    }
}
