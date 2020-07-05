using System;

namespace RIS.Collections.Trees
{
    public interface IKDPointComparer<in TPoint>
    {
        int Dimensions { get; }
        int Compare(TPoint a, TPoint b, int dimension);
    }

    internal interface INeedsInitializationKDPointComparer<in TPoint> : IKDPointComparer<TPoint>
    {
        void InitializeFrom(TPoint point);
    }
}
