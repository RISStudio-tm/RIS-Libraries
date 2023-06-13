// Copyright (c) RISStudio, 2020. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE file in the project root for license information.

using System;

namespace RIS.Collections.Nestable.Frames
{
    internal struct NestableCollectionEnumerateFrame<T>
    {
        public INestableCollection<T> Collection;
        public int Index;



        public NestableCollectionEnumerateFrame(
            INestableCollection<T> collection,
            int index = -1)
        {
            Collection = collection;
            Index = index;
        }
    }
}
