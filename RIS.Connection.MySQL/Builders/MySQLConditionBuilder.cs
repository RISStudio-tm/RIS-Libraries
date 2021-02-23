// Copyright (c) RISStudio, 2020. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE file in the project root for license information. 

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;

namespace RIS.Connection.MySQL.Builders
{
    public sealed class MySQLConditionBuilder : ICloneable
    {
        private static readonly Dictionary<char, char> ParenthesesMap;

        public static readonly MySQLConditionBuilder Empty;

        private readonly StringBuilder _sql;
        private readonly List<MySQLConditionParameter> _parameters;

        private bool _locked;

        static MySQLConditionBuilder()
        {
            ParenthesesMap = new Dictionary<char, char>
            {
                { ')', '(' },
                { ']', '[' },
                { '}', '{' }
            };

            Empty = Where()
                .Lock();
        }

        private MySQLConditionBuilder()
        {
            _sql = new StringBuilder();
            _parameters = new List<MySQLConditionParameter>();
        }

        private MySQLConditionBuilder(string sql,
            IEnumerable<MySQLConditionParameter> parameters,
            bool locked = false)
        {
            _sql = new StringBuilder(sql.TrimEnd());
            _parameters = new List<MySQLConditionParameter>(parameters);

            if (locked)
                Lock();
        }



        private void ThrowIfLocked()
        {
            if (_locked)
            {
                var exception =
                    new FormatException($"The current MySQLConditionBuilder[{_sql}] instance is locked");
                Events.OnError(this,
                    new RErrorEventArgs(exception, exception.Message, exception.StackTrace));

                throw exception;
            }
        }

        private void ValidateParameterValue(MySQLConditionParameter parameter)
        {
            if (parameter == null)
                return;

            switch (parameter.Value)
            {
                case "NULL":
                case null:
                    parameter.Value = DBNull.Value;
                    break;
                case "'NULL'":
                    parameter.Value = "NULL";
                    break;
                default:
                    break;
            }
        }

        private bool IsValidParentheses()
        {
            if (_sql.Length == 0)
                return true;
            if (_sql.Length == 1)
                return false;

            var stack = new Stack<char>();

            for (var i = 0; i < _sql.Length; ++i)
            {
                var ch = _sql[i];

                if (ParenthesesMap.ContainsValue(ch))
                {
                    stack.Push(ch);
                }
                else if (ParenthesesMap.TryGetValue(ch, out var chMapped))
                {
                    if (stack.Count == 0 || stack.Pop() != chMapped)
                        return false;
                }
            }

            return stack.Count == 0;
        }

        private bool IsValid()
        {
            if (!IsValidParentheses())
                return false;

            return true;
        }



        private MySQLConditionBuilder Not()
        {
            ThrowIfLocked();

            if (_sql[_sql.Length - 1] != '('
                && _sql[_sql.Length - 1] != ' ')
            {
                _sql.Append(' ');
            }

            _sql.Append("NOT");

            return this;
        }



        public MySQLConditionBuilder Lock()
        {
            _locked = true;

            return this;
        }

        public bool IsEmpty()
        {
            var sql = _sql
                .ToString()
                .TrimEnd();

            if (sql.StartsWith(" WHERE"))
                return sql.Length <= 6;

            return sql.Length == 0;
        }

        public ReadOnlyCollection<MySQLConditionParameter> GetParameters()
        {
            return new ReadOnlyCollection<MySQLConditionParameter>(_parameters);
        }

        public (string Sql, ReadOnlyCollection<MySQLConditionParameter> Parameters) Build()
        {
            var sql = _sql
                .ToString()
                .TrimEnd();

            if (!IsValid())
            {
                var exception =
                    new FormatException($"An error occurred during the MySQL condition string build because the result string [{sql}] had an incorrect format or was invalid");
                Events.OnError(this,
                    new RErrorEventArgs(exception, exception.Message, exception.StackTrace));

                throw exception;
            }

            return (sql, GetParameters());
        }

        public MySQLConditionBuilder Copy()
        {
            return new MySQLConditionBuilder(_sql.ToString(), _parameters);
        }

        public override string ToString()
        {
            return Build().Sql;
        }

        public object Clone()
        {
            return Copy();
        }



        public MySQLConditionBuilder OpenBracket()
        {
            ThrowIfLocked();

            if (_sql[_sql.Length - 1] != '('
                && _sql[_sql.Length - 1] != ' ')
            {
                _sql.Append(' ');
            }

            _sql.Append('(');

            return this;
        }

        public MySQLConditionBuilder CloseBracket()
        {
            ThrowIfLocked();

            if (_sql[_sql.Length - 1] == ' ')
            {
                _sql.Remove(_sql.Length - 1, 1);
            }

            _sql.Append(')');

            return this;
        }

        public MySQLConditionBuilder And()
        {
            ThrowIfLocked();

            if (_sql[_sql.Length - 1] != '('
                && _sql[_sql.Length - 1] != ' ')
            {
                _sql.Append(' ');
            }

            _sql.Append("AND");

            return this;
        }

        public MySQLConditionBuilder Or()
        {
            ThrowIfLocked();

            if (_sql[_sql.Length - 1] != '('
                && _sql[_sql.Length - 1] != ' ')
            {
                _sql.Append(' ');
            }

            _sql.Append("OR");

            return this;
        }

        public MySQLConditionBuilder IsTrue(string name, string value,
            ComparisonModeType comparisonMode = ComparisonModeType.Equal)
        {
            ThrowIfLocked();

            if (string.IsNullOrEmpty(name))
                return this;

            if (_sql[_sql.Length - 1] != '('
                && _sql[_sql.Length - 1] != ' ')
            {
                _sql.Append(' ');
            }

            if (comparisonMode == ComparisonModeType.NotEqualNullSafe
                && !(value == "NULL" || value == null))
            {
                Not()
                    .OpenBracket();
            }

            _sql.Append(name);

            if (value == "NULL" || value == null)
            {
#pragma warning disable SS018 // Add cases for missing enum member.
                switch (comparisonMode)
                {
                    case ComparisonModeType.NotEqual:
                    case ComparisonModeType.NotEqualNullSafe:
                        _sql.Append(" IS NOT NULL");
                        break;
                    default:
                        _sql.Append(" IS NULL");
                        break;
                }
#pragma warning restore SS018 // Add cases for missing enum member.

                return this;
            }

            var parameter = new MySQLConditionParameter(
                $"param_condition{_parameters.Count}",
                value);

            ValidateParameterValue(parameter);

            switch (comparisonMode)
            {
                case ComparisonModeType.Equal:
                    _sql.Append(" =");
                    break;
                case ComparisonModeType.EqualNullSafe:
                    _sql.Append(" <=>");
                    break;
                case ComparisonModeType.NotEqual:
                    _sql.Append(" <>");
                    break;
                case ComparisonModeType.NotEqualNullSafe:
                    _sql.Append(" <=>");
                    break;
                case ComparisonModeType.GreaterThan:
                    _sql.Append(" >");
                    break;
                case ComparisonModeType.GreaterThanOrEqual:
                    _sql.Append(" >=");
                    break;
                case ComparisonModeType.LessThan:
                    _sql.Append(" <");
                    break;
                case ComparisonModeType.LessThanOrEqual:
                    _sql.Append(" <=");
                    break;
                default:
                    break;
            }

            _sql.Append(' ')
                .Append(parameter.Name);

            if (comparisonMode == ComparisonModeType.NotEqualNullSafe)
            {
                CloseBracket();
            }

            _parameters.Add(parameter);

            return this;
        }

        public MySQLConditionBuilder IsFalse(string name, string value,
            ComparisonModeType comparisonMode = ComparisonModeType.Equal)
        {
            return Not()
                .OpenBracket()
                .IsTrue(name, value, comparisonMode)
                .CloseBracket();
        }

        public MySQLConditionBuilder IsNull(string name)
        {
            return IsTrue(name, null, ComparisonModeType.Equal);
        }

        public MySQLConditionBuilder IsNotNull(string name)
        {
            return IsTrue(name, null, ComparisonModeType.NotEqual);
        }

        public MySQLConditionBuilder Like(string name, string value,
            char escapeCharacter = '\\')
        {
            ThrowIfLocked();

            if (string.IsNullOrEmpty(name))
                return this;

            if (_sql[_sql.Length - 1] != '('
                && _sql[_sql.Length - 1] != ' ')
            {
                _sql.Append(' ');
            }

            _sql.Append(name);

            if (value == "NULL" || value == null)
            {
                _sql.Append(" IS NULL");

                return this;
            }

            var parameter = new MySQLConditionParameter(
                $"param_condition{_parameters.Count}",
                value);

            ValidateParameterValue(parameter);

            _sql.Append(" LIKE ")
                .Append(parameter.Name)
                .Append(" ESCAPE '")
                .Append(escapeCharacter);

            if (escapeCharacter == '\\')
                _sql.Append(escapeCharacter);

            _sql.Append('\'');

            _parameters.Add(parameter);

            return this;
        }

        public MySQLConditionBuilder NotLike(string name, string value,
            char escapeCharacter = '\\')
        {
            return Not()
                .OpenBracket()
                .Like(name, value, escapeCharacter)
                .CloseBracket();
        }



        public static MySQLConditionBuilder Where()
        {
            var builder = new MySQLConditionBuilder();

            builder._sql.Append(" WHERE");

            return builder;
        }
    }
}
