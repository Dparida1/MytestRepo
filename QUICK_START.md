# Quick Start Guide

For experienced developers who just want to get up and running quickly.

## Prerequisites
- .NET Framework 4.8
- Visual Studio 2022 with ASP.NET workload
- SQL Server LocalDB (comes with Visual Studio) or SQL Server Express
- AWS credentials configured

## 1. Setup (Windows only - Web Forms requires Windows/IIS)
```cmd
setup.cmd
```

## 2. Configure AWS
Edit `web.config`:
```xml
<appSettings>
  <add key="AWS_Region" value="us-east-1" />
  <add key="AWS_Profile" value="your-aws-profile" />
</appSettings>
```

## 3. Run
- Open `WorkSpaceManager.sln` in Visual Studio
- Press `F5` to run
- Or use IIS Express from Visual Studio

## 5. First Use
1. Click "Sync from AWS" on dashboard
2. Go to WorkSpaces → Create New
3. Fill form and create workspaces

## Project Structure
```
├── Controllers/           # MVC Controllers
├── Models/               # Data models and ViewModels
├── Services/             # Business logic
├── Views/                # Razor templates
├── Data/                 # Entity Framework context
├── wwwroot/              # Static files
└── appsettings.json      # Configuration
```

## Key Features
- **Bulk WorkSpace Creation**: Create 1-100 WorkSpaces at once
- **Dashboard**: Real-time metrics and charts
- **Reports**: Export data to CSV, SQL Server storage
- **Multi-region**: Support for all AWS regions
- **SSO Support**: Works with AWS SSO authentication

## AWS Permissions Required
```json
{
  "Version": "2012-10-17",
  "Statement": [
    {
      "Effect": "Allow",
      "Action": [
        "workspaces:*",
        "ds:Describe*"
      ],
      "Resource": "*"
    }
  ]
}
```

## Using Visual Studio Code
1. Install C# extension
2. Open folder in VS Code
3. Press `F5` to run with debugging
4. Use `Ctrl+Shift+P` → "Tasks: Run Task" for build tasks

## Database
- Uses SQL Server LocalDB by default
- Entity Framework Code First
- Database auto-created on first run
- Connection string in `appsettings.json`

## Development
- Hot reload: `dotnet watch run`
- Build: `dotnet build`
- Test: `dotnet test` (if tests added)
- Publish: `dotnet publish -c Release`

For detailed instructions, see `GETTING_STARTED_GUIDE.md`.