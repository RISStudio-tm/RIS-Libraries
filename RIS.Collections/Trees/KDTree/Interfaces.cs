// Copyright (c) RISStudio, 2020. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE file in the project root for license information.

using System;

namespace RIS.Collections.Trees
{
    public interface IKDPointComparer<in TPoint>
    {
        int Dimensions { get; }
        int Compare(TPoint a, TPoint b, int dimension);
    }

    internal interface IInitializableKDPointComparer<in TPoint> : IKDPointComparer<TPoint>
    {
        void InitializeFrom(TPoint point);
    }
}
