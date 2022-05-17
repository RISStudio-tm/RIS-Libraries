using System;

namespace RIS.Unions.Types
{
    public class TrueOrFalse : UnionBase<True, False> 
    {
        protected TrueOrFalse(Union<True, False> _)
            : base(_)
        {

        }

        public static implicit operator TrueOrFalse(True _)
        {
            return new TrueOrFalse(_);
        }
        public static implicit operator TrueOrFalse(False _)
        {
            return new TrueOrFalse(_);
        }
        public static implicit operator TrueOrFalse(bool value)
        {
            return new TrueOrFalse(
                value
                    ? new True()
                    : new False()
            );
        }
    }
}