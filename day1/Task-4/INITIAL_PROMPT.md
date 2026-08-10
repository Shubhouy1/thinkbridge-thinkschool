# Initial AI Prompt

Create a deliberately bad legacy-style OrderController.cs for an ASP.NET Core 10 application.

This is for a refactoring exercise, so intentionally include realistic code smells.

Requirements:

- Approximately 300 lines.
- One giant POST /api/orders action.
- Mix business logic, validation, EF Core database access, and HTTP response shaping directly inside the action.
- Use four separate empty catch { } blocks that swallow exceptions.
- Include synchronous EF Core calls such as ToList(), FirstOrDefault(), SaveChanges(), etc. inside an async action.
- Return object instead of strongly typed responses.
- Include no tests.
- Include at least two subtle bugs:
  1. an off-by-one error
  2. a possible null reference exception.
- Use poor naming and duplicated logic where appropriate.
- Make the code look like realistic two-year-old production code rather than obviously artificial bad code.
- Keep the code concentrated in the controller.
- The endpoint should be POST /api/orders.

Do NOT refactor the code.
Do NOT explain the smells.
Do NOT improve the architecture.

Save the generated code as OrderController.cs.