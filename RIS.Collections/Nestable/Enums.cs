using System;

namespace RIS.Collections.Nestable
{
    public enum NestedType : byte
    {
        Element = 1,
        Array = 2,
        NestableCollection = 3
    }

    public enum NestableCollectionType : byte
    {
        NestableArrayCAL = 1,
        NestableDictionaryL = 2,
        NestableListL = 3
    }
}
