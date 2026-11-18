# Infrastructure authentication tests

Run `dotnet restore`, `dotnet build`, and `dotnet test` from the solution root
using the .NET 10 SDK.

Moq controls the virtual Identity UserManager/SignInManager operations. This
lets the tests exercise the real AuthService branches for failed passwords,
invalid tokens, successful changes, user update failures, and missing/inactive
users, without constructing an Identity database or authentication session.
The tests verify flag transitions and subsequent persistence/session calls.
They do not test Identity's own password hashing or token generation.

GetMeAsync executes its actual EF query against a unique InMemory database per
test. Those cases cover service/query mapping, not SQL Server translation or
relational constraints. No production database, migrations, or email is used.

A successful Identity password operation clears MustChangePassword before the
separate user update. If that update fails, the current implementation throws
but leaves the in-memory flag cleared. These tests verify the exception and
absence of session refresh in that case; they do not claim transactional
rollback across Identity operations. Production code has not been changed.
