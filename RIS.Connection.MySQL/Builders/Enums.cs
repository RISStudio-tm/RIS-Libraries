// Copyright (c) RISStudio, 2020. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE file in the project root for license information. 

using System;

namespace RIS.Connection.MySQL.Builders
{
    /// <summary>
    /// Представляет режим сравнения значений, используемый для логических выражений.
    /// </summary>
    public enum MySQLComparisonModeType : byte
    {
        /// <summary>
        ///     =
        /// </summary>
        Equal = 1,
        /// <summary>
        ///     &lt;=&gt;
        /// </summary>
        EqualNullSafe = 2,
        /// <summary>
        /// &lt;&gt;
        /// </summary>
        NotEqual = 3,
        /// <summary>
        /// NOT (&lt;=&gt;)
        /// </summary>
        NotEqualNullSafe = 4,
        /// <summary>
        /// &gt;
        /// </summary>
        GreaterThan = 5,
        /// <summary>
        /// &gt;=
        /// </summary>
        GreaterThanOrEqual = 6,
        /// <summary>
        /// &lt;
        /// </summary>
        LessThan = 7,
        /// <summary>
        /// &lt;=
        /// </summary>
        LessThanOrEqual = 8
    }
}
