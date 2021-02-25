// Copyright (c) RISStudio, 2020. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE file in the project root for license information. 

using System;
using System.Text;
using MySqlConnector;

namespace RIS.Connection.MySQL
{
    internal static class RequestEngineHelper
    {
        internal static void ReplaceDBNullParameterValue(string value, ref MySqlCommand command,
            string parameterName)
        {
            if (command == null)
                return;

            if (string.Equals(value, "NULL", StringComparison.OrdinalIgnoreCase) || value == null)
                command.Parameters[parameterName].Value = DBNull.Value;
            else if (string.Equals(value, "'NULL'", StringComparison.OrdinalIgnoreCase))
                command.Parameters[parameterName].Value = value.Substring(1, value.Length - 2);
        }
        internal static void ReplaceDBNullParameterValue(string value, ref MySqlDataAdapter adapter,
            string parameterName)
        {
            if (adapter?.SelectCommand == null)
                return;

            if (string.Equals(value, "NULL", StringComparison.OrdinalIgnoreCase) || value == null)
                adapter.SelectCommand.Parameters[parameterName].Value = DBNull.Value;
            else if (string.Equals(value, "'NULL'", StringComparison.OrdinalIgnoreCase))
                adapter.SelectCommand.Parameters[parameterName].Value = value.Substring(1, value.Length - 2);
        }

        internal static void ReplaceFunctionParameterValue(string value, ref MySqlCommand command,
            string parameterName, ref string sql)
        {
            if (command == null)
                return;

            if (string.Equals(value, "CURRENT_TIMESTAMP", StringComparison.OrdinalIgnoreCase))
                sql = sql.Replace(parameterName, "CURRENT_TIMESTAMP");
            else if (string.Equals(value, "'CURRENT_TIMESTAMP'", StringComparison.OrdinalIgnoreCase))
                command.Parameters[parameterName].Value = value.Substring(1, value.Length - 2);
        }
        internal static void ReplaceFunctionParameterValue(string value, ref MySqlCommand command,
            string parameterName, ref StringBuilder sqlBuilder)
        {
            if (command == null)
                return;

            if (string.Equals(value, "CURRENT_TIMESTAMP", StringComparison.OrdinalIgnoreCase))
                sqlBuilder = sqlBuilder.Replace(parameterName, "CURRENT_TIMESTAMP");
            else if (string.Equals(value, "'CURRENT_TIMESTAMP'", StringComparison.OrdinalIgnoreCase))
                command.Parameters[parameterName].Value = value.Substring(1, value.Length - 2);
        }
        internal static void ReplaceFunctionParameterValue(string value, ref MySqlDataAdapter adapter,
            string parameterName, ref string sql)
        {
            if (adapter?.SelectCommand == null)
                return;

            if (string.Equals(value, "CURRENT_TIMESTAMP", StringComparison.OrdinalIgnoreCase))
                sql = sql.Replace(parameterName, "CURRENT_TIMESTAMP");
            else if (string.Equals(value, "'CURRENT_TIMESTAMP'", StringComparison.OrdinalIgnoreCase))
                adapter.SelectCommand.Parameters[parameterName].Value = value.Substring(1, value.Length - 2);
        }
        internal static void ReplaceFunctionParameterValue(string value, ref MySqlDataAdapter adapter,
            string parameterName, ref StringBuilder sqlBuilder)
        {
            if (adapter?.SelectCommand == null)
                return;

            if (string.Equals(value, "CURRENT_TIMESTAMP", StringComparison.OrdinalIgnoreCase))
                sqlBuilder = sqlBuilder.Replace(parameterName, "CURRENT_TIMESTAMP");
            else if (string.Equals(value, "'CURRENT_TIMESTAMP'", StringComparison.OrdinalIgnoreCase))
                adapter.SelectCommand.Parameters[parameterName].Value = value.Substring(1, value.Length - 2);
        }
    }
}
