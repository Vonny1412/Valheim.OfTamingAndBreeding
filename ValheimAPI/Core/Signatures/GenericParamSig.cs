namespace OfTamingAndBreeding.ValheimAPI.Core.Signatures
{
    public class GenericParamSig : CommonParamSig
    {
        public GenericParamSig(int GenericIndex, bool IsByRef)
        {
            IsGeneric = true;
            base.GenericIndex = GenericIndex;
            ConcreteType = null;
            base.IsByRef = IsByRef;
        }
    }
}
