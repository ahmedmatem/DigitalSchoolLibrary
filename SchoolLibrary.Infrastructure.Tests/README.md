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

After a successful Identity password operation, AuthService clears
MustChangePassword and saves the user. If this save returns a failed
IdentityResult or throws, the previous flag is restored on the tracked user
and the error is propagated without refreshing the session. Regression cases
cover both password operations and both initial flag values.

This restores in-memory flag consistency; it does not roll back the preceding
Identity password write or provide a transaction across both writes. A caller
may receive an error even though the password has already changed.

ForgotPassword cases cover missing/inactive accounts, one reset email for an
active account, whitespace normalization, frontend base paths/trailing slashes,
and literal expected URLs with reserved characters in the email/token. The
email sender is a strict mock; tests verify cancellation-token forwarding and
error propagation without sending real email or retrying delivery.
