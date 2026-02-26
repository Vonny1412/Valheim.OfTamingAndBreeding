using System;

namespace OfTamingAndBreeding.ValheimAPI.Core.Signatures
{
    public class NonGenericParamSig : CommonParamSig
    {
        public NonGenericParamSig(Type ConcreteType, bool IsByRef)
        {
            IsGeneric = false;
            GenericIndex = -1;
            base.ConcreteType = ConcreteType;
            base.IsByRef = IsByRef;
        }
    }
}
