// Copyright (c) RISStudio, 2020. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE file in the project root for license information. 

using System;

namespace RIS.Collections.Nestable.Frames
{
    internal struct NestableCollectionUnstringifyFrame<T>
    {
        public INestableCollection<T> Collection;
        public int StartIndex;
        public int EndIndex;
        public int Length;
        public int DivideIndex;

        public CollectionGeneralType GeneralType;



        public NestableCollectionUnstringifyFrame(
            INestableCollection<T> collection,
            int startIndex, int endIndex,
            int divideIndex = -1)
        {
            Collection = collection;
            StartIndex = startIndex;
            EndIndex = endIndex;
            Length = endIndex - startIndex + 1;
            DivideIndex = divideIndex;

            GeneralType = NestableHelper.GetGeneralType(
                collection);
        }
    }
}
