using System;

namespace RIS.Collections.Trees
{
    public interface IKDPoinTComparer<in TPoint>
    {
        int Dimensions { get; }
        int Compare(TPoint a, TPoint b, int dimension);
    }

    internal interface INeedsInitializationKDPoinTComparer<in TPoint> : IKDPoinTComparer<TPoint>
    {
        void InitializeFrom(TPoint point);
    }
}
