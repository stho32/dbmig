# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

dbmig is a powerful command-line tool for SQL Server database migrations and testing with focus on security, traceability, and testability. It provides enterprise-grade migration management, automated testing, and comprehensive rollback capabilities for SQL Server environments.

## Project Structure

```
/
├── CLAUDE.md                   # This file - AI development guidelines
├── Initialize-AI.md            # Quick AI initialization commands
├── Dokumentation/              # Comprehensive project documentation
│   ├── Anforderungen/          # Feature requirements (R00001-*.md format)
│   ├── Architektur/            # Architecture docs (CodeStyle, Programming Patterns)
│   ├── Commands/               # Standard AI prompts for reviews
│   ├── Promptlog/              # Development prompts history (P00001-*.md)
│   └── Technologien/           # Technology stack documentation
├── Scripts/                    # Production-ready PowerShell automation
│   ├── Build.ps1              # Comprehensive build system
│   ├── Database-Initialize.ps1 # Database initialization/clearing
│   ├── Database-Migrate.ps1   # Migration runner with rollback
│   ├── Run-Tests.ps1          # Unit & integration test runner
│   └── Run-UITests.ps1        # UI test execution (if needed)
├── Source/
│   ├── Create-Solution.ps1    # Automated .NET solution generator
│   ├── Code/                  # dbmig application code
│   └── DBMigrations/          # SQL migration scripts (00001-*.sql)
│       └── README.md          # Migration system documentation
├── Tests/
│   ├── Datenbank/             # Database-specific tests
│   │   └── README.md          # Database testing strategies
│   └── UI/                    # UI tests (if needed)
│       └── README.md          # UI testing documentation
└── README.md                  # Project overview and usage guide
```

## AI-Optimized Development

This project is specifically designed for AI collaboration with production-ready features:

### Quick AI Initialization
Use `Initialize-AI.md` for comprehensive context loading:
```bash
cat Initialize-AI.md  # Contains all necessary read commands and documentation
```

### Production Features for AI Development
- **Console Application**: Command-line tool for database migrations
- **Migration Management**: SQL-based migrations with versioning
- **Rollback Support**: Safe rollback capabilities with transaction support
- **Enterprise Testing**: Comprehensive database testing strategies
- **Cross-Platform Scripts**: PowerShell scripts work on Windows/Linux/macOS

### Standard Prompts Available
- `Dokumentation/Commands/Security-Review.md` - Security analysis
- `Dokumentation/Commands/Code-Quality.md` - Code quality review  
- `Dokumentation/Commands/Architektur-Pruefung.md` - Architecture review
- `Dokumentation/Commands/Rechtschreibpruefung.md` - German spell checking

### Documentation Standards
- Requirements: `R{00001}-{description}.md` format in Anforderungen/
- Prompts: `P{00001}-{kurzbeschreibung}.md` format in Promptlog/
- Architecture: CodeStyle.md and ProgrammierPatterns.md templates
- Technology: Individual .md files for each library/framework
- Migrations: `00001-kurze-inhaltsangabe.sql` format with documentation
- Testing: Comprehensive README files for each test strategy

## Development Workflow

1. **Initialize Context**: Use `cat Initialize-AI.md` for complete AI context
2. **Create Console App**: Use `Source/Create-Solution.ps1 -ProjectType Console`
3. **Build & Test**: Leverage `Scripts/*.ps1` for all development tasks
4. **Migration Development**: Create and test SQL migrations
5. **Quality Assurance**: Apply standard prompts for reviews
6. **Documentation-First**: Always document before implementing

## Key Features for dbmig

### 🔄 **Migration Management**
- Version-controlled SQL migrations
- Automatic rollback on errors
- Transaction safety for critical changes
- Detailed migration logging

### 🧪 **Database Testing**
- Schema validation tests
- Migration performance tests
- Data integrity checks
- Isolated test environments with containers

### 🔐 **Enterprise Features**
- Multi-environment support
- Encrypted connection strings
- Comprehensive error handling
- Audit trail for all operations

### 🤖 **AI-Enhanced Development**
- Structured documentation for optimal AI collaboration
- Standard prompts for consistent code reviews
- Quick initialization for AI assistants
- Prompt logging for development history

## dbmig-Specific Principles

- **Command-Line First**: Console application for automation
- **SQL-Native**: Direct SQL scripts, no ORM abstractions
- **Safety-Focused**: Rollback and validation at every step
- **Enterprise-Ready**: Production-grade error handling and logging
- **Testable**: Comprehensive test coverage for migrations
- **Cross-Platform**: Works on Windows, Linux, and macOS