using System;

namespace RIS.Versioning
{
    internal enum CompareOperator : byte
    {
        Equal = 0,
        NotEqual = 1,
        LessThan = 2,
        LessThanOrEqual = 3,
        GreaterThan = 4,
        GreaterThanOrEqual = 5,
    }
}
