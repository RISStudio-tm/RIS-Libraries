using System;

namespace RIS.Unions
{
    public interface IUnion
    { 
        object Value { get; }
        int Index { get; }
    }
}