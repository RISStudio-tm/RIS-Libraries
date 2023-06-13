// Copyright (c) RISStudio, 2020. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE file in the project root for license information.

using System;
using System.Collections.Generic;

namespace RIS.Collections
{
    public interface IKeyedEnumerable
    {
        IEnumerator<KeyValuePair<object, object>> GetEnumerator();
    }

    public interface IKeyedEnumerable<TKey, TValue> : IKeyedEnumerable
    {
        new IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator();
    }
}
