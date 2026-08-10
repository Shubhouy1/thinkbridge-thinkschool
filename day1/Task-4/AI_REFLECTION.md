# AI Reflection

Claude/Copilot got the main refactoring goal right by moving the shipping, tax, and discount rules out of `OrderService` into separate strategy classes. This made the business rules easier to change without modifying the main service. I also checked the generated changes instead of accepting them blindly. One issue I caught was that the first version manually created the strategy classes in the `OrderService` constructor. I pushed back and changed it to use dependency injection, which is cleaner and easier to test.

Copilot saved time when creating the validation tests. I only had to describe the behavior in comments, such as rejecting negative and zero quantities, and Copilot generated the test structure. I still reviewed the generated tests because it initially used `NullLogger.Instance`, which did not match the constructor cleanly. I changed it to `NullLogger<OrderService>.Instance`.

The tests gave me confidence that the refactor did not break the existing order behavior. After adding the new tests, all five tests passed.

At 2 AM IST while debugging production, I would reach for Copilot first for small, focused code changes or tests because it works directly with the codebase. For a larger design problem, I would use Claude to reason about the structure, but I would still review every change myself. The main lesson was that AI can speed up refactoring, but I still need to understand and verify what it changes.