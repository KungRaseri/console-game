# Coverage Report Scripts

## generate-coverage-report.ps1

Generates a timestamped HTML coverage report from the most recent test run.

### Usage

**From Command Line:**
```powershell
.\scripts\generate-coverage-report.ps1
```

**From VS Code Task:**
- Press `Ctrl+Shift+P`
- Run task: `generate-coverage-report`

Or run the combined task:
- Press `Ctrl+Shift+P`
- Run task: `test-coverage` (runs tests + generates report)

### Features

- ✅ **Timestamped Reports**: Each report is saved in `Game.Tests/TestResults/CoverageReport/{timestamp}/`
- ✅ **Latest Symlink**: Always creates/updates `latest/` folder pointing to most recent report
- ✅ **Multiple Formats**: Generates HTML report + SVG badges + text summary
- ✅ **Interactive**: Prompts to open report in browser
- ✅ **Summary Display**: Shows coverage percentages in terminal

### Output Structure

```
Game.Tests/TestResults/
├── CoverageReport/
│   ├── 20251213_211433/          # Timestamped report
│   │   ├── index.html            # Main report
│   │   ├── Summary.txt           # Text summary
│   │   ├── badge_*.svg           # Coverage badges
│   │   └── Game_*.html           # Per-class reports
│   ├── 20251213_153022/          # Previous report
│   └── latest/                   # Copy of most recent
└── {guid}/
    └── coverage.cobertura.xml    # Raw coverage data
```

### Coverage Badges

The script generates SVG badges for:
- **Line Coverage**
- **Branch Coverage**
- **Method Coverage**
- **Full Method Coverage**

Badges use shields.io style with color coding:
- 🟢 Green: 90-100%
- 🟡 Yellow: 70-89%
- 🟠 Orange: 50-69%
- 🔴 Red: 0-49%

### Prerequisites

- .NET SDK installed
- `dotnet test --collect:"XPlat Code Coverage"` must be run first
- `reportgenerator` global tool installed

Install reportgenerator:
```powershell
dotnet tool install -g dotnet-reportgenerator-globaltool
```

### Troubleshooting

**No coverage file found:**
```
Error: No coverage file found. Run 'dotnet test --collect:XPlat Code Coverage' first.
```
Solution: Run `dotnet test --collect:"XPlat Code Coverage"` or use the `test-with-coverage` task.

**reportgenerator not found:**
```
reportgenerator : The term 'reportgenerator' is not recognized...
```
Solution: Install the tool with `dotnet tool install -g dotnet-reportgenerator-globaltool`
