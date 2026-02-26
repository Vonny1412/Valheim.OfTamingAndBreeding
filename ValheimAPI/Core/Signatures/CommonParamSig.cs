using System;
using System.Reflection;

namespace OfTamingAndBreeding.ValheimAPI.Core.Signatures
{
    public abstract class CommonParamSig : ParamSig
    {
        public override bool Matches(ParameterInfo p)
        {
            // ref / out
            if (p.IsOut != IsByRef && p.ParameterType.IsByRef != IsByRef)
            {
                return false;
            }

            Type pt = p.ParameterType;
            if (pt.IsByRef)
                pt = pt.GetElementType();

            if (IsGeneric)
            {
                if (!pt.IsGenericParameter || pt.GenericParameterPosition != GenericIndex)
                {
                    return false;
                }
            }
            else
            {
                if (pt != ConcreteType)
                {
                    return false;
                }
            }

            return true;
        }
    }
}
