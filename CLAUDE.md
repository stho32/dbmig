# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

GoldenCageDotNet is a production-ready .NET project template (Golden Cage) designed for AI-optimized development. It provides a comprehensive foundation with automated PowerShell scripts, extensive documentation, standardized prompts, test architectures, and clear separation of concerns for enterprise-grade development.

## Project Structure

```
/
├── CLAUDE.md                   # This file - AI development guidelines
├── Initialize-AI.md            # Quick AI initialization commands
├── Dokumentation/              # Comprehensive project documentation
│   ├── Anforderungen/          # Application requirements (R00001-*.md format)
│   ├── Architektur/            # Architecture docs (CodeStyle, Programming Patterns)
│   ├── Commands/               # Standard AI prompts for reviews
│   └── Technologien/           # Technology stack documentation
├── Scripts/                    # Production-ready PowerShell automation
│   ├── Build.ps1              # Comprehensive build system
│   ├── Database-Initialize.ps1 # Database initialization/clearing
│   ├── Database-Migrate.ps1   # Controlled migration with rollback
│   ├── Run-Tests.ps1          # Unit & integration test runner
│   └── Run-UITests.ps1        # Playwright UI test execution
├── Source/
│   ├── Create-Solution.ps1    # Automated .NET solution generator
│   ├── Code/                  # Main application code
│   └── DBMigrations/          # SQL-based migrations (00001-*.sql)
│       └── README.md          # Migration system documentation
├── Tests/
│   ├── Datenbank/             # Database tests with Testcontainers
│   │   └── README.md          # Database testing strategies
│   └── UI/                    # Playwright UI tests with Page Object Model
│       └── README.md          # UI testing documentation
└── README.md                  # Project overview and quick start
```

## AI-Optimized Development

This template is specifically designed for AI collaboration with production-ready features:

### Quick AI Initialization
Use `Initialize-AI.md` for comprehensive context loading:
```bash
cat Initialize-AI.md  # Contains all necessary read commands and documentation
```

### Production Features for AI Development
- **Automated Solution Generation**: `Source/Create-Solution.ps1` creates complete .NET solutions
- **Comprehensive Build System**: `Scripts/Build.ps1` with multi-framework support
- **Database Migration System**: SQL-based migrations with rollback support
- **Enterprise Testing**: Testcontainers, Playwright, Page Object Model
- **Cross-Platform Scripts**: PowerShell scripts work on Windows/Linux/macOS

### Standard Prompts Available
- `Dokumentation/Commands/Security-Review.md` - Security analysis
- `Dokumentation/Commands/Code-Quality.md` - Code quality review  
- `Dokumentation/Commands/Architektur-Pruefung.md` - Architecture review
- `Dokumentation/Commands/Rechtschreibpruefung.md` - German spell checking

### Documentation Standards
- Requirements: `R{00001}-{description}.md` format in Anforderungen/
- Architecture: CodeStyle.md and ProgrammierPatterns.md templates
- Technology: Individual .md files for each library/framework
- Migrations: `00001-kurze-inhaltsangabe.sql` format with documentation
- Testing: Comprehensive README files for each test strategy

## Development Workflow

1. **Initialize Context**: Use `cat Initialize-AI.md` for complete AI context
2. **Generate Solutions**: Use `Source/Create-Solution.ps1` for new projects
3. **Build & Test**: Leverage `Scripts/*.ps1` for all development tasks
4. **Database Management**: Use migration scripts for schema changes
5. **Quality Assurance**: Apply standard prompts for reviews
6. **Documentation-First**: Always document before implementing

## Key Production Features

### 🚀 **Automation-First**
- Complete PowerShell script ecosystem
- Automated solution generation with templates
- Cross-platform development support

### 🧪 **Enterprise Testing**
- Multi-browser UI testing with Playwright
- Database testing with Testcontainers
- Performance and integration test strategies

### 🗄️ **Professional Database Management**
- SQL-based migration system
- Rollback capabilities
- Migration documentation and versioning

### 🤖 **AI-Enhanced Development**
- Structured documentation for optimal AI collaboration
- Standard prompts for consistent code reviews
- Quick initialization for AI assistants

## Template Usage Principles

- **Production-Ready**: All scripts and patterns are enterprise-grade
- **AI-Collaborative**: Optimized for AI-assisted development workflows  
- **Documentation-Centric**: Comprehensive docs for all systems and processes
- **Standardized**: Consistent naming and organizational patterns
- **Scalable**: Works from prototype to enterprise applications
- **Cross-Platform**: PowerShell and .NET for universal compatibility