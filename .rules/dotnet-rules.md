# .NET Code Rules

- Use `nullable reference types` (`Nullable=enable`).
- Use `implicit usings` (`ImplicitUsings=enable`).
- Use `PascalCase` for classes, methods, and properties.
- Use `camelCase` in JSON serialization (via `JsonNamingPolicy.CamelCase`).
- Use `async/await` throughout. Never block on async calls.
- Use `IOptions<T>` for strongly typed configuration.
- Use `ILogger<T>` injected via DI. Never use static loggers.
- Prefer `sealed` classes unless inheritance is explicitly needed.
- Prefer `record` types for value objects and DTOs.
- Do not introduce abstractions without a concrete justification.
- Do not add patterns (Repository, CQRS, Mediator) unless the complexity demands them.
