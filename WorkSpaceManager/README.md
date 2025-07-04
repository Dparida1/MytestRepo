# AWS WorkSpace Manager - Web Application

A comprehensive ASP.NET Core web application for managing AWS WorkSpaces with role-based SSO authentication support. This application provides a user-friendly web interface to create, manage, and monitor AWS WorkSpaces with complete reporting capabilities.

## 🚀 Features

### Core Functionality
- **Bulk WorkSpace Creation**: Create multiple WorkSpaces with configurable parameters
- **WorkSpace Management**: Start, stop, reboot, terminate, and modify WorkSpaces
- **Real-time Dashboard**: Visual dashboard with key metrics and charts
- **Comprehensive Reporting**: Generate reports with data export capabilities
- **SQL Server Integration**: Complete data persistence and reporting

### Web Interface
- **Modern Bootstrap UI**: Responsive design with professional interface
- **Interactive Charts**: Real-time data visualization using Chart.js
- **Filtering & Pagination**: Advanced search and filtering capabilities
- **Batch Operations**: Perform operations on multiple WorkSpaces simultaneously
- **Real-time Updates**: Ajax-powered updates without page refreshes

### AWS Integration
- **Role-based SSO**: Full support for AWS SSO authentication
- **Multi-region Support**: Manage WorkSpaces across different AWS regions
- **Bundle & Directory Discovery**: Automatic discovery of available resources
- **Error Handling**: Comprehensive error handling with user-friendly messages

## 🏗️ Architecture

### Technology Stack
- **Framework**: ASP.NET Core 8.0
- **Database**: SQL Server with Entity Framework Core
- **Frontend**: Bootstrap 5, jQuery, Chart.js
- **AWS SDK**: AWS SDK for .NET with WorkSpaces support
- **Authentication**: AWS SSO integration

### Project Structure
```
WorkSpaceManager/
├── Controllers/           # MVC Controllers
│   ├── HomeController.cs     # Dashboard controller
│   ├── WorkSpaceController.cs # WorkSpace management
│   └── ReportsController.cs   # Reporting functionality
├── Models/               # Data models and view models
│   ├── WorkSpaceEntity.cs    # Database entities
│   └── ViewModels.cs         # View models and DTOs
├── Services/             # Business logic services
│   ├── IWorkSpaceService.cs  # WorkSpace service interface
│   ├── WorkSpaceService.cs   # WorkSpace service implementation
│   ├── IReportsService.cs    # Reports service interface
│   └── ReportsService.cs     # Reports service implementation
├── Data/                 # Entity Framework context
│   └── WorkSpaceContext.cs   # Database context
├── Views/                # Razor views
│   ├── Home/                 # Dashboard views
│   ├── WorkSpace/            # WorkSpace management views
│   ├── Reports/              # Reporting views
│   └── Shared/               # Shared layout and components
└── wwwroot/              # Static files (CSS, JS, images)
```

## 📋 Prerequisites

### Software Requirements
- .NET 8.0 SDK or later
- SQL Server (LocalDB, Express, or Full)
- Visual Studio 2022 or VS Code
- IIS Express (for development)

### AWS Requirements
- AWS Account with WorkSpaces service access
- IAM role with WorkSpaces permissions:
  - `workspaces:Describe*`
  - `workspaces:Create*`
  - `workspaces:Start*`
  - `workspaces:Stop*`
  - `workspaces:Reboot*`
  - `workspaces:Terminate*`
  - `workspaces:Modify*`
- AWS SSO configuration (if using SSO)
- At least one AWS Directory Service
- WorkSpaces bundles available in target regions

## 🛠️ Installation & Setup

### 1. Clone and Build
```bash
git clone <repository-url>
cd WorkSpaceManager
dotnet restore
dotnet build
```

### 2. Database Configuration
Update the connection string in `appsettings.json`:
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=WorkSpaceManagerDb;Trusted_Connection=true;MultipleActiveResultSets=true;TrustServerCertificate=true"
  }
}
```

For production, use a full SQL Server instance:
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=your-server;Database=WorkSpaceManagerDb;User Id=your-user;Password=your-password;TrustServerCertificate=true"
  }
}
```

### 3. AWS Configuration
Configure AWS credentials using one of these methods:

#### Option A: AWS Profile
```json
{
  "AWS": {
    "Profile": "your-profile-name",
    "Region": "us-east-1"
  }
}
```

#### Option B: Environment Variables
```bash
export AWS_ACCESS_KEY_ID=your-access-key
export AWS_SECRET_ACCESS_KEY=your-secret-key
export AWS_DEFAULT_REGION=us-east-1
```

#### Option C: IAM Role (for EC2 deployment)
The application will automatically use the attached IAM role.

### 4. Database Migration
```bash
dotnet ef database update
```

Or let the application create the database automatically on first run.

### 5. Run the Application
```bash
dotnet run
```

The application will be available at `https://localhost:5001` (HTTPS) or `http://localhost:5000` (HTTP).

## 🎯 Usage Guide

### Initial Setup
1. **Access the Application**: Navigate to the application URL
2. **Configure AWS**: Ensure AWS credentials are properly configured
3. **Sync Data**: Use the "Sync from AWS" feature to load existing resources
4. **Verify Connectivity**: Check the system status on the dashboard

### Creating WorkSpaces

#### Bulk Creation Process
1. **Navigate to Create**: Click "WorkSpaces" → "Create New"
2. **Basic Information**:
   - Enter a descriptive request name
   - Specify the number of WorkSpaces (1-100)
3. **AWS Configuration**:
   - Select the target AWS region
   - Choose a WorkSpace bundle
   - Select the directory service
4. **User Configuration**:
   - Set the username prefix
   - Preview the generated usernames
5. **Security Settings**:
   - Configure volume encryption
   - Optionally specify a KMS key
6. **Submit Request**: Review and submit the creation request

#### Parameters Explanation
- **Request Name**: Descriptive name for tracking purposes
- **Number of WorkSpaces**: How many WorkSpaces to create (1-100 per request)
- **Region**: AWS region where WorkSpaces will be created
- **Bundle**: WorkSpace configuration (OS, compute, storage)
- **Directory**: Active Directory service for user authentication
- **Username Prefix**: Base name for users (e.g., "dev-user" → "dev-user001", "dev-user002")
- **Encryption**: Volume-level encryption settings
- **Notes**: Additional comments for documentation

### Managing WorkSpaces

#### Individual Operations
- **Start**: Power on a stopped WorkSpace
- **Stop**: Power off a running WorkSpace
- **Reboot**: Restart a WorkSpace
- **Terminate**: Permanently delete a WorkSpace
- **Modify**: Change WorkSpace properties

#### Bulk Operations
1. **Select WorkSpaces**: Use checkboxes to select multiple WorkSpaces
2. **Choose Operation**: Select start, stop, reboot, or terminate
3. **Confirm Action**: Review and confirm the bulk operation
4. **Monitor Progress**: Check results and any errors

### Viewing Reports

#### Dashboard Reports
- **Key Metrics**: Total, running, stopped WorkSpaces
- **Visual Charts**: Distribution by region, state, and bundle
- **Recent Activity**: Latest WorkSpace operations
- **System Status**: Health checks and connectivity

#### Detailed Reports
1. **Navigate to Reports**: Click "Reports" in the navigation
2. **Filter Data**: Set date ranges, regions, or status filters
3. **View Charts**: Interactive charts with drill-down capabilities
4. **Export Data**: Download reports as CSV files

### Data Export
- **WorkSpaces Export**: Complete WorkSpace inventory with details
- **Requests Export**: Creation request history and status
- **Custom Reports**: Filtered data based on specific criteria

## ⚙️ Configuration Options

### Application Settings
```json
{
  "Application": {
    "Name": "AWS WorkSpace Manager",
    "Version": "1.0.0",
    "SupportEmail": "support@company.com"
  },
  "Features": {
    "EnableBulkOperations": true,
    "EnableReports": true,
    "EnableExport": true,
    "MaxWorkSpacesPerRequest": 100,
    "DefaultPageSize": 20
  }
}
```

### Logging Configuration
```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning",
      "Microsoft.EntityFrameworkCore": "Warning"
    }
  }
}
```

## 🔧 Deployment

### Development Deployment
```bash
dotnet run --environment Development
```

### Production Deployment

#### Option 1: IIS Deployment
1. Publish the application:
```bash
dotnet publish -c Release -o ./publish
```

2. Configure IIS:
   - Create application pool (.NET Core)
   - Create website pointing to publish folder
   - Configure proper permissions

#### Option 2: Docker Deployment
Create `Dockerfile`:
```dockerfile
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["WorkSpaceManager.csproj", "."]
RUN dotnet restore
COPY . .
RUN dotnet build -c Release -o /app/build

FROM build AS publish
RUN dotnet publish -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "WorkSpaceManager.dll"]
```

Build and run:
```bash
docker build -t workspace-manager .
docker run -p 8080:80 workspace-manager
```

#### Option 3: Cloud Deployment
- **Azure App Service**: Deploy directly from Visual Studio or GitHub
- **AWS Elastic Beanstalk**: Use .NET Core platform
- **AWS ECS**: Deploy using Docker containers

### Production Considerations
1. **Database**: Use Azure SQL, AWS RDS, or dedicated SQL Server
2. **Security**: Configure HTTPS, authentication, and authorization
3. **Monitoring**: Set up application insights and logging
4. **Backup**: Regular database backups and disaster recovery
5. **Scaling**: Configure load balancing for high availability

## 🔒 Security Best Practices

### AWS Security
- Use IAM roles with minimal required permissions
- Enable AWS CloudTrail for audit logging
- Use VPC and security groups appropriately
- Implement cross-account role assumptions if needed

### Application Security
- Always use HTTPS in production
- Implement proper input validation
- Use parameterized queries (Entity Framework handles this)
- Configure CORS appropriately
- Implement rate limiting for API endpoints

### Database Security
- Use encrypted connections
- Implement proper backup encryption
- Use read-only connections for reporting where possible
- Regular security updates

## 🐛 Troubleshooting

### Common Issues

#### AWS Connectivity
**Problem**: "Failed to sync data from AWS"
**Solution**:
- Verify AWS credentials are correct
- Check IAM permissions
- Ensure network connectivity to AWS
- Verify region availability

#### Database Issues
**Problem**: "Cannot connect to database"
**Solution**:
- Check connection string
- Verify SQL Server is running
- Check database permissions
- Ensure database exists

#### WorkSpace Creation Failures
**Problem**: "Failed to create WorkSpaces"
**Solution**:
- Verify WorkSpaces quota limits
- Check directory service status
- Ensure proper VPC configuration
- Verify bundle availability in region

### Debug Mode
Enable detailed logging in development:
```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Debug",
      "WorkSpaceManager": "Debug"
    }
  }
}
```

### Health Checks
The application includes health check endpoints:
- `/health` - Overall application health
- Check database connectivity
- Verify AWS API access

## 📊 Performance Optimization

### Database Optimization
- Use proper indexing (automatically configured)
- Implement connection pooling
- Use read replicas for reporting
- Regular maintenance and statistics updates

### Application Optimization
- Enable response caching where appropriate
- Use asynchronous operations
- Implement proper pagination
- Optimize queries with Entity Framework

### AWS API Optimization
- Implement exponential backoff
- Use pagination for large datasets
- Cache frequently accessed data
- Monitor API rate limits

## 🤝 Contributing

### Development Guidelines
1. Follow ASP.NET Core best practices
2. Use async/await patterns consistently
3. Implement proper error handling
4. Write unit tests for business logic
5. Use dependency injection appropriately

### Code Style
- Follow Microsoft C# coding conventions
- Use meaningful names for variables and methods
- Comment complex business logic
- Keep methods focused and small

## 📄 License

This project is licensed under the MIT License. See the LICENSE file for details.

## 🆘 Support

For support and questions:
1. Check the troubleshooting section
2. Review AWS WorkSpaces documentation
3. Check application logs
4. Contact your system administrator

## 🔄 Version History

### Version 1.0.0
- Initial release
- Core WorkSpace management functionality
- Dashboard and reporting
- SQL Server integration
- AWS SSO support

---

**Note**: This application is designed for organizations using AWS WorkSpaces with role-based SSO authentication. Ensure you have proper AWS permissions and network connectivity before deployment.