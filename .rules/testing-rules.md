# Testing Rules

- Use xUnit as the test framework.
- Use Moq for mocking dependencies.
- Use `Microsoft.AspNetCore.Mvc.Testing` for integration tests.
- Test behaviour, not implementation details.
- Do not test getters, setters, or trivial code.
- Each test must have a single, clear assertion of intent.
- Test names must follow: `Should_[ExpectedBehavior]_When_[Condition]`.
- Unit tests must not use real Playwright or Chromium. Mock `IPdfGeneratorService`.
- Integration tests should replace `IPdfGeneratorService` with a mock via `WithWebHostBuilder`.
- Keep tests fast. Avoid unnecessary setup or teardown overhead.
