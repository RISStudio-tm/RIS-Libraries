// This file is used by Code Analysis to maintain SuppressMessage
// attributes that are applied to this project.
// Project-level suppressions either have no target or are given
// a specific target and scoped to a namespace, type, member, etc.

//[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Security", "CA2100:Review SQL queries for security vulnerabilities", Justification = "<Ожидание>", Scope = "type", Target = "~T:RIS.Connection.MySQL.Requests")]
using System.Diagnostics.CodeAnalysis;

[assembly: SuppressMessage("Security", "CA2100:Review SQL queries for security vulnerabilities")]
[assembly: SuppressMessage("AsyncUsage", "AsyncFixer02:Long running or blocking operations under an async method")]
