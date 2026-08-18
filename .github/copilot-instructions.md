# AI Agent Workspace Instructions

## 1. Core Philosophy & Clean Code
- **SOLID Principles:** Enforce Single Responsibility, Open/Closed, Liskov Substitution, Interface Segregation, and Dependency Inversion.
- **Readability Over Cleverness:** Write expressive, self-documenting code with intention-revealing names. Avoid single-letter variables except in localized counters/loops.
- **Minimal Changes & YAGNI:** Prefer minimal changes and avoid over-engineering. Do not introduce abstractions (e.g., generic repository wrappers over ORMs) unless explicitly requested or justified by complex domain logic.
- **Explicit Failure & Error Handling:** Never suppress or catch-and-ignore errors without explicit logging or rethrowing. Prefer functional `Result<T, Error>` types for expected domain validation/failures, and exceptions exclusively for exceptional, unrecoverable system states.

## 2. Architecture & Layering Rules
Maintain strict **Clean Architecture / Onion Architecture** layer boundaries. Inner layers must have zero awareness or dependencies on outer layers.

[ Domain Layer ]         <-- Core entities, value objects, domain events, interface definitions  
       ^  
[ Application Layer ]    <-- Use cases, CQRS commands/queries, domain interfaces  
       ^  
[ Infrastructure Layer ] <-- DB access, external APIs, file systems, framework adapters  
       ^  
[ Presentation / API ]  <-- Controllers, RPC endpoints, Blazor UI, API contracts  

- **Contract Separation:** Keep API/UI contracts strictly separate from domain and service models. Never use API contract DTOs as internal service or domain inputs/outputs.
- **Descriptive Naming:** Name API contracts to reflect endpoint intent (e.g., `UpdateCustomerAddressRequest` instead of `CustomerInfoRequest`).
- **Blazor Separation:** Separate feature-specific UI state and presentation logic from `.razor` markup using partial code-behind files (`.razor.cs`) or dedicated component view models.
- **User Experience:** Focus user-facing page content on semantic user tasks rather than technical implementation details.

## 3. Preferred Design Patterns & C# Standards
- **Standard Patterns:** Prefer standard patterns (Factory, Strategy, CQRS/MediatR) where complexity warrants them. Avoid custom abstractions when language or framework features suffice.
- **Dependency Injection:** Program to interfaces. Register dependencies with appropriate lifetimes (`Transient`, `Scoped`, `Singleton`).
- **Central Package Management:** Manage all NuGet package versions centrally using `Directory.Packages.props`. Do not declare `Version` attributes inside individual `.csproj` files.
- **Modern C# Idioms:** Favor primary constructors, file-scoped namespaces, pattern matching, and `init`-only or record properties for immutable contract models.

## 4. Infrastructure & Docker
- **Docker Compose Syntax:** Use standard variable interpolation `${VAR}` syntax instead of `${VAR:?message}`.
- **Explicit Configuration:** Environment variables must be explicitly defined in `.env` files for production/staging environments. Standard defaults may be provided only for local development setups.
- **Linux Compatibility:** Build projects for Linux runtime compatibility; prefer Linux-ready dependencies

## 5. Code Generation Protocols
When generating or refactoring code:
1. **Provide Full Implementations:** Produce complete, runnable code blocks without placeholder comments like `// TODO: implement later` or `// ... rest of code ...`.
2. **Defensive Guards:** Place preconditions, null checks (using standard guards like `ArgumentNullException.ThrowIfNull`), and state validation at function entry points.
3. **Async Standard:** Use explicit asynchronous patterns (`async`/`await`) for all I/O, network, and database calls. Always accept and forward a `CancellationToken` through long-running or async call chains.
4. **Testability:** Structure code to be easily unit-tested using the `Arrange / Act / Assert` pattern, mocking outer infrastructure boundaries.