GenpactProject: C# Playwright Automation Framework
Lightweight framework for UI and API testing on Wikipedia.
Setup

Install .NET SDK 8.0
Run dotnet restore
Run npx playwright install

Run Tests

dotnet test (Headless mode)
set HEADLESS=false && dotnet test (Headed mode)
Outputs TestReport.html

Tasks

Task 1: Extract "Debugging features" section (UI via POM, MediaWiki API), normalize, assert unique word counts.
Task 2: Validate all "Microsoft development tools" items are links.
Task 3: Change Color (beta) to Dark, verify via UI.
Bonus: HTML report (TestReport.html).
