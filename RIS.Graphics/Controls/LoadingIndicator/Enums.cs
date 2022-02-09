// Copyright (c) RISStudio, 2020. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE file in the project root for license information. 

using System.ComponentModel;

namespace RIS.Graphics.Controls
{
    public enum LoadingIndicatorMode
    {
        [Description("LoadingWave")]
        Wave,
        [Description("LoadingArc")]
        Arc,
        [Description("LoadingArcs")]
        Arcs,
        [Description("LoadingArcsRing")]
        ArcsRing,
        [Description("LoadingDoubleBounce")]
        DoubleBounce,
        [Description("LoadingFlipPlane")]
        FlipPlane,
        [Description("LoadingPulse")]
        Pulse,
        [Description("LoadingRing")]
        Ring,
        [Description("LoadingThreeDots")]
        ThreeDots
    }
}