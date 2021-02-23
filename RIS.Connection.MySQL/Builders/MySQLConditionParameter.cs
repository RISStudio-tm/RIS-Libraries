// Copyright (c) RISStudio, 2020. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE file in the project root for license information. 

using System;

namespace RIS.Connection.MySQL.Builders
{
    public sealed class MySQLConditionParameter
    {
        public string Name { get; private set; }
        public object Value { get; internal set; }

        internal MySQLConditionParameter(string name, object value)
        {
            Name = $"@{name}";
            Value = value;
        }
    }
}
