# 📚 Console Game Documentation

Welcome to the Console Game documentation! This folder contains all guides, implementation notes, and technical documentation for the project.

## 📂 Documentation Structure

### 📖 [guides/](./guides/)

**User guides and tutorials** for using the game's systems and libraries.

| File | Description |
|------|-------------|
| [CONSOLEUI_GUIDE.md](./guides/CONSOLEUI_GUIDE.md) | Complete guide to using Spectre.Console UI components |
| [SETTINGS_GUIDE.md](./guides/SETTINGS_GUIDE.md) | How to use the Microsoft.Extensions.Configuration settings system |
| [SETTINGS_QUICK_START.cs](./guides/SETTINGS_QUICK_START.cs) | Quick reference code examples for settings |
| [ENV_FILE_GUIDE.md](./guides/ENV_FILE_GUIDE.md) | Using .env files for environment-specific configuration |
| [GAME_LOOP_GUIDE.md](./guides/GAME_LOOP_GUIDE.md) | Understanding the game loop and state machine architecture |
| [QUICK_START_LOOPS.md](./guides/QUICK_START_LOOPS.md) | Quick examples of implementing game loops |

### 🔧 [implementation/](./implementation/)

**Implementation summaries and technical decisions** made during development.

| File | Description |
|------|-------------|
| [FINALIZED.md](./implementation/FINALIZED.md) | Summary of GameEngine implementation and architecture |
| [CONSOLEUI_EXPANSION_SUMMARY.md](./implementation/CONSOLEUI_EXPANSION_SUMMARY.md) | ConsoleUI security fixes and expansion summary |
| [SETTINGS_IMPLEMENTATION.md](./implementation/SETTINGS_IMPLEMENTATION.md) | Settings system implementation details and packages |

### 🧪 [testing/](./testing/)

**Testing documentation** including coverage reports and test guidelines.

| File | Description |
|------|-------------|
| [TEST_COVERAGE_REPORT.md](./testing/TEST_COVERAGE_REPORT.md) | Comprehensive test coverage report (148 tests) |

## 🚀 Quick Start

### New to the Project?

Start here:

1. Read the main [README.md](../README.md) in the project root
2. Review [GAME_LOOP_GUIDE.md](./guides/GAME_LOOP_GUIDE.md) for architecture overview
3. Check [CONSOLEUI_GUIDE.md](./guides/CONSOLEUI_GUIDE.md) to understand the UI system
4. Read [SETTINGS_GUIDE.md](./guides/SETTINGS_GUIDE.md) for configuration management

### Adding Features?

Reference these guides:

- **UI Changes**: [CONSOLEUI_GUIDE.md](./guides/CONSOLEUI_GUIDE.md)
- **Configuration**: [SETTINGS_GUIDE.md](./guides/SETTINGS_GUIDE.md)
- **Game Logic**: [GAME_LOOP_GUIDE.md](./guides/GAME_LOOP_GUIDE.md)

### Writing Tests?

- See [TEST_COVERAGE_REPORT.md](./testing/TEST_COVERAGE_REPORT.md) for examples and current coverage

## 📋 Documentation Index by Topic

### Architecture & Design

- [GameEngine State Machine](./guides/GAME_LOOP_GUIDE.md)

- [Dependency Injection Setup](./guides/SETTINGS_GUIDE.md#dependency-injection)

### Configuration & Settings

- [Settings System Guide](./guides/SETTINGS_GUIDE.md)
- [Environment Variables](./guides/ENV_FILE_GUIDE.md)
- [appsettings.json Reference](./guides/SETTINGS_GUIDE.md#configuration-sections)
- [Settings Quick Examples](./guides/SETTINGS_QUICK_START.cs)

### User Interface

- [Spectre.Console Usage](./guides/CONSOLEUI_GUIDE.md)
- [Security Best Practices](./guides/CONSOLEUI_GUIDE.md#security)
- [Advanced UI Components](./implementation/CONSOLEUI_EXPANSION_SUMMARY.md)

### Testing

- [Test Coverage Report](./testing/TEST_COVERAGE_REPORT.md)
- [Writing Unit Tests](./testing/TEST_COVERAGE_REPORT.md#test-examples)
- [FluentValidation Testing](./testing/TEST_COVERAGE_REPORT.md#validator-tests)

## 🔄 Documentation Maintenance

### When to Update Documentation

| Trigger | Update This |
|---------|-------------|
| New feature added | Add guide in `guides/` |
| Architecture change | Update `GAME_LOOP_GUIDE.md` or `FINALIZED.md` |
| Test coverage changes | Update `TEST_COVERAGE_REPORT.md` |
| New configuration option | Update `SETTINGS_GUIDE.md` |
| UI component added | Update `CONSOLEUI_GUIDE.md` |

### Documentation Standards

- Use clear headings and table of contents
- Include code examples with syntax highlighting
- Add ✅/❌ for do's and don'ts
- Keep examples executable when possible
- Update README.md when structure changes

## 🎯 Project Links

- **Main Project**: [../README.md](../README.md)
- **GitHub Copilot Instructions**: [../.github/copilot-instructions.md](../.github/copilot-instructions.md)
- **Source Code**: [../Game/](../Game/)
- **Tests**: [../Game.Tests/](../Game.Tests/)

---

**Last Updated**: December 4, 2025  
**Documentation Version**: 1.0  
**Test Coverage**: 148 tests (100% of testable components)
