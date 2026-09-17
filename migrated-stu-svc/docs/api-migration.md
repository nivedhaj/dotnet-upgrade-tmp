# API Migration Notes

## Route preservation
All implemented routes preserve their legacy path and HTTP verb exactly
(spec section 50, rule 7 and rule 22 — "do not silently remove an
endpoint"). The controller is mounted at the application root (`[Route("")]`)
so that e.g. `POST /create` resolves to `https://host/create`, matching the
legacy `StudentController` base path. If the legacy deployment actually
mounted the controller under a prefix (e.g. `/api/student`), add
`app.UsePathBase("/api/student")` in `Program.cs` — this was not specified
in the standalone spec and is left as a one-line change once confirmed.

## Response envelope
All responses use `ApiResponse<T>` (`{ success, data, error }`) per spec
section 52. If legacy clients expect a different shape (e.g. bare DTOs, or
`ProblemDetails` for errors), that needs to be confirmed against real client
code, which is outside what this standalone spec can determine.

## JSON casing
`Program.cs` configures camelCase JSON output. Legacy Newtonsoft.Json
defaults also produced camelCase for most controllers, but this should be
verified against the exact legacy `JsonSerializerSettings` if any custom
contract resolvers were in play.

## Implemented vs pending routes
See `docs/migration-matrix.md` for the authoritative, up-to-date list —
duplicating it here would drift out of sync.

## Known simplifications
- `PromoteStudentRequest`/`CreateStudentRequest` bind from JSON body; the
  legacy controller's exact parameter binding (query string vs. body vs.
  route) was not fully specified for every action, so query-string binding
  was used for simple scalar actions (enable/disable/setRollNo/etc.) and
  JSON body for multi-field payloads. Adjust if legacy clients send
  differently-shaped requests.
