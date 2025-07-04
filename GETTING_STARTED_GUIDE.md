# AWS WorkSpace Manager - Complete Beginner's Guide

This guide will help you get started with the AWS WorkSpace Manager web application from scratch. No prior experience with ASP.NET Core required!

## 📋 What You'll Need

### Software Prerequisites
1. **Windows 10/11** (recommended) or **macOS/Linux**
2. **.NET 8.0 SDK** - [Download here](https://dotnet.microsoft.com/download/dotnet/8.0)
3. **Visual Studio Community 2022** (free) - [Download here](https://visualstudio.microsoft.com/vs/community/)
   - Alternative: **Visual Studio Code** (free) - [Download here](https://code.visualstudio.com/)
4. **SQL Server LocalDB** (comes with Visual Studio) or **SQL Server Express** (free)

### AWS Prerequisites
1. **AWS Account** with WorkSpaces access
2. **AWS CLI** installed - [Download here](https://aws.amazon.com/cli/)
3. **AWS SSO configured** (if using SSO) or **AWS credentials**
4. **WorkSpaces permissions** in your AWS account

## 🛠️ Step 1: Install Required Software

### Install .NET 8.0 SDK
1. Go to https://dotnet.microsoft.com/download/dotnet/8.0
2. Download ".NET 8.0 SDK" for your operating system
3. Run the installer and follow the prompts
4. Verify installation by opening Command Prompt/Terminal and typing:
   ```bash
   dotnet --version
   ```
   You should see something like `8.0.100`

### Install Visual Studio Community 2022
1. Download from https://visualstudio.microsoft.com/vs/community/
2. During installation, make sure to select:
   - **ASP.NET and web development** workload
   - **.NET desktop development** workload
   - **SQL Server LocalDB** (under Individual components)

### Install AWS CLI (Optional but Recommended)
1. Download from https://aws.amazon.com/cli/
2. Install and configure with your credentials:
   ```bash
   aws configure
   ```

## 🚀 Step 2: Get the Code Running

### Download the Solution
If you have the solution files, extract them to a folder like `C:\WorkSpaceManager\`

### Open in Visual Studio
1. **Launch Visual Studio 2022**
2. Click **"Open a project or solution"**
3. Navigate to your folder and select `WorkSpaceManager.sln`
4. Visual Studio will load the project

### Restore NuGet Packages
1. In Visual Studio, right-click on the solution in **Solution Explorer**
2. Select **"Restore NuGet Packages"**
3. Wait for packages to download (this may take a few minutes)

## 🗄️ Step 3: Set Up the Database

### Option A: Use SQL Server LocalDB (Easiest)
The application is already configured to use LocalDB. No additional setup needed!

### Option B: Use SQL Server Express
1. Download SQL Server Express from Microsoft
2. Install with default settings
3. Update the connection string in `appsettings.json`:
   ```json
   {
     "ConnectionStrings": {
       "DefaultConnection": "Server=.\\SQLEXPRESS;Database=WorkSpaceManagerDb;Trusted_Connection=true;MultipleActiveResultSets=true;TrustServerCertificate=true"
     }
   }
   ```

## ⚙️ Step 4: Configure AWS Settings

### Method 1: Using AWS Profile (Recommended)
1. Open `appsettings.json` file in Visual Studio
2. Update the AWS section:
   ```json
   {
     "AWS": {
       "Profile": "your-aws-profile-name",
       "Region": "us-east-1"
     }
   }
   ```

### Method 2: Using Environment Variables
Set these environment variables on your computer:
- `AWS_ACCESS_KEY_ID=your-access-key`
- `AWS_SECRET_ACCESS_KEY=your-secret-key`
- `AWS_DEFAULT_REGION=us-east-1`

### Method 3: Using AWS SSO
If your organization uses AWS SSO:
1. Configure AWS CLI with SSO:
   ```bash
   aws configure sso
   ```
2. Follow the prompts to authenticate
3. Update `appsettings.json`:
   ```json
   {
     "AWS": {
       "Profile": "your-sso-profile-name",
       "Region": "us-east-1"
     }
   }
   ```

## ▶️ Step 5: Run the Application

### From Visual Studio
1. Make sure **WorkSpaceManager** is selected as the startup project
2. Press **F5** or click the green **"Start"** button
3. The application will build and launch in your web browser
4. You'll see the dashboard at `https://localhost:5001`

### From Command Line
1. Open Command Prompt/Terminal
2. Navigate to the WorkSpaceManager folder
3. Run:
   ```bash
   dotnet run
   ```
4. Open your browser and go to `https://localhost:5001`

## 🎯 Step 6: Understanding the Application

### Dashboard (Home Page)
When you first open the application, you'll see:
- **Metrics Cards**: Total WorkSpaces, Running, Stopped, etc.
- **Charts**: Visual representation of your WorkSpaces
- **Recent Activity**: Latest operations
- **Quick Actions**: Buttons to perform common tasks

### Navigation Menu
- **Dashboard**: Main overview page
- **WorkSpaces**: Dropdown with options to view all or create new
- **Reports**: Access to detailed reports and data export

## 📝 Step 7: Your First WorkSpace Creation

### Sync Data First (Important!)
1. On the dashboard, click **"Sync from AWS"** in the Quick Actions panel
2. Enter your AWS region (e.g., `us-east-1`)
3. Wait for the sync to complete - this loads your bundles and directories

### Create WorkSpaces
1. Click **WorkSpaces** → **Create New** in the navigation
2. Fill out the form:

   **Basic Information:**
   - **Request Name**: Give it a name like "Test WorkSpaces"
   - **Number of WorkSpaces**: Start with 1 for testing

   **AWS Configuration:**
   - **Region**: Select your AWS region
   - **Bundle**: Choose from available bundles (loaded from sync)
   - **Directory**: Select your directory service

   **User Configuration:**
   - **Username Prefix**: Enter something like "test-user"
   - You'll see a preview of usernames that will be created

   **Security:**
   - Leave encryption settings as default (enabled)

3. Click **"Create WorkSpaces"**
4. The system will submit your request to AWS

### Monitor Progress
1. Go back to the dashboard
2. Check the **Recent Activity** section
3. Go to **WorkSpaces** → **View All** to see your WorkSpaces

## 📊 Step 8: Using Reports

### View Dashboard Reports
- The dashboard shows real-time metrics
- Charts update automatically
- Click **"Refresh"** to get latest data

### Export Data
1. Go to **Reports**
2. Set date filters if needed
3. Click **"Export WorkSpaces"** to download CSV data
4. Open in Excel for further analysis

## 🔧 Common Tasks

### Start/Stop WorkSpaces
1. Go to **WorkSpaces** → **View All**
2. Find your WorkSpace in the list
3. Use the action buttons to Start, Stop, or Reboot
4. For multiple WorkSpaces, use checkboxes and **Bulk Operations**

### View WorkSpace Details
1. In the WorkSpaces list, click on a WorkSpace ID
2. You'll see detailed information about that WorkSpace

### Modify WorkSpace Properties
1. In WorkSpace details, use the **Modify** options
2. Change running mode, auto-stop timeout, etc.

## 🐛 Troubleshooting Common Issues

### "Cannot connect to database"
**Solution:**
1. Make sure SQL Server LocalDB is installed
2. Check if the connection string in `appsettings.json` is correct
3. Try running Visual Studio as Administrator

### "AWS credentials not found"
**Solution:**
1. Verify your AWS credentials are configured
2. Test with: `aws sts get-caller-identity`
3. Make sure the profile name in `appsettings.json` matches your AWS profile

### "No bundles found"
**Solution:**
1. Make sure you have WorkSpaces enabled in your AWS region
2. Check your IAM permissions
3. Use the **"Sync from AWS"** feature to reload data

### "Failed to create WorkSpaces"
**Solution:**
1. Check your WorkSpaces quota in AWS console
2. Verify your directory service is active
3. Ensure you have proper IAM permissions

### Application won't start
**Solution:**
1. Check the **Output** window in Visual Studio for error messages
2. Make sure .NET 8.0 SDK is installed
3. Try cleaning and rebuilding the solution: **Build** → **Clean Solution**, then **Build** → **Rebuild Solution**

## 🔐 Required AWS Permissions

Your AWS user/role needs these permissions:
```json
{
    "Version": "2012-10-17",
    "Statement": [
        {
            "Effect": "Allow",
            "Action": [
                "workspaces:Describe*",
                "workspaces:Create*",
                "workspaces:Start*",
                "workspaces:Stop*",
                "workspaces:Reboot*",
                "workspaces:Terminate*",
                "workspaces:Modify*",
                "ds:Describe*"
            ],
            "Resource": "*"
        }
    ]
}
```

## 📚 Learning Resources

### ASP.NET Core Basics
- [Microsoft's ASP.NET Core Tutorial](https://docs.microsoft.com/en-us/aspnet/core/tutorials/)
- [C# Programming Guide](https://docs.microsoft.com/en-us/dotnet/csharp/)

### AWS WorkSpaces
- [AWS WorkSpaces Documentation](https://docs.aws.amazon.com/workspaces/)
- [AWS WorkSpaces API Reference](https://docs.aws.amazon.com/workspaces/latest/api/)

## 🆘 Getting Help

### Application Logs
1. Check the **Output** window in Visual Studio
2. Look at the **Console** output when running the application
3. Check Windows Event Viewer for detailed error logs

### Debug Mode
To see more detailed information:
1. In Visual Studio, make sure you're running in **Debug** mode (not Release)
2. Set breakpoints in the code to inspect variables
3. Use the **Debug** → **Windows** → **Output** to see detailed logs

## 📋 Next Steps Checklist

- [ ] Install .NET 8.0 SDK
- [ ] Install Visual Studio 2022
- [ ] Configure AWS credentials
- [ ] Open the solution in Visual Studio
- [ ] Update `appsettings.json` with your AWS settings
- [ ] Run the application (F5)
- [ ] Sync data from AWS
- [ ] Create your first WorkSpace
- [ ] Explore the dashboard and reports

## 💡 Tips for Success

1. **Start Small**: Create 1-2 WorkSpaces first to test everything works
2. **Use LocalDB**: For development, LocalDB is the easiest database option
3. **Check Permissions**: Most issues are related to AWS permissions
4. **Monitor Costs**: WorkSpaces incur charges, so monitor your AWS billing
5. **Regular Sync**: Use the sync feature regularly to keep data up-to-date

## 🎓 Understanding the Code Structure

### Key Files to Know
- **`Program.cs`**: Application startup and configuration
- **`appsettings.json`**: Configuration settings
- **`Controllers/`**: Handle web requests (like web page actions)
- **`Models/`**: Data structures and database entities
- **`Services/`**: Business logic for AWS operations
- **`Views/`**: HTML templates for web pages

### How It Works
1. **Web Request**: User clicks button or submits form
2. **Controller**: Receives request and calls appropriate service
3. **Service**: Communicates with AWS API or database
4. **Response**: Returns data to controller
5. **View**: Displays results to user

This architecture separates concerns and makes the application maintainable and testable.

---

**Remember**: Take your time with each step. If you encounter issues, check the troubleshooting section or refer to the error messages for guidance. The application is designed to be user-friendly, but AWS WorkSpaces is a complex service, so some learning is expected!