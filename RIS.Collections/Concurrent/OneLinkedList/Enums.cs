// Copyright (c) RISStudio, 2020. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE file in the project root for license information.

using System;

namespace RIS.Collections.Concurrent
{
    internal enum ConcurrentOneLinkedListNodeState : byte
    {
        Ins = 0,
        Rem = 1,
        Dat = 2,
        Inv = 3
    }
}
