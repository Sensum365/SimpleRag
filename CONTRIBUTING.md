# Contributing to SimpleRag

Thank you for your interest in contributing to **SimpleRag**.  
SimpleRag is a **C# library** for **Retrieval Augmented Generation (RAG)**, published as a NuGet package.

We welcome contributions that improve the project, including code, documentation, and examples.

---

## How to Contribute

- **Report Issues**: Use [GitHub Issues](https://github.com/Sensum365/SimpleRag/issues) to report bugs, request features, or suggest improvements.  
- **Contribute Code**: Extend the library with new features or improve existing functionality.  

---

## Development Setup

1. Clone the repository:
   ```sh
   git clone https://github.com/Sensum365/SimpleRag.git
   cd SimpleRag
   ```

2. Open the solution in **Visual Studio**, **Rider**, or **VS Code** with the C# extension.

3. Build the project:
   ```sh
   dotnet build
   ```

---

## Coding Guidelines

- **Language**: C# (.NET).  
- **Style**:
  - Follow [Microsoft C# Coding Conventions](https://learn.microsoft.com/dotnet/csharp/fundamentals/coding-style/coding-conventions).  
  - Always use braces `{ }`, even for single-line statements.  
  - Prefer `async/await` for asynchronous operations.  
- **Naming**:
  - Use `PascalCase` for classes, methods, and public properties.  
  - Use `camelCase` for private fields and local variables.  
- **Public API**:
  - Keep APIs small and focused.  
  - Add XML documentation to public types and members.

---

## Branching Model

- **main**: Default branch. All work is merged via Pull Requests.  
- **feature/**: New features (e.g., `feature/vector-store-support`).  
- **fix/**: Bug fixes (e.g., `fix/config-handling`).  

---

## Pull Request Process

1. Create a new branch from `main`.  
2. Make your changes.  
3. Ensure the solution builds without errors.  
4. Open a Pull Request targeting `main` and include:
   - A clear title and description.  
   - Links to related issues where applicable (`Fixes #123`).  
