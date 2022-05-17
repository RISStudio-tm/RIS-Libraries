using System;

namespace RIS.Unions.Types
{
    public class YesNoOrMaybe : UnionBase<Yes, No, Maybe>
    {
        protected YesNoOrMaybe(Union<Yes, No, Maybe> _)
            : base(_)
        {

        }

        public static implicit operator YesNoOrMaybe(Yes _)
        {
            return new YesNoOrMaybe(_);
        }
        public static implicit operator YesNoOrMaybe(No _)
        {
            return new YesNoOrMaybe(_);
        }
        public static implicit operator YesNoOrMaybe(Maybe _)
        {
            return new YesNoOrMaybe(_);
        }
        public static implicit operator YesNoOrMaybe(bool? value)
        {
            return new YesNoOrMaybe(
                value is null
                    ? new Maybe()
                    : value.Value
                        ? new Yes()
                        : new No()
            );
        }
    }
}