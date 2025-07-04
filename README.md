# AWS WorkSpaces Manager

A comprehensive CLI tool for managing AWS WorkSpaces with SSO authentication support. This tool is designed for organizations that use role-based SSO authentication and need to manage WorkSpaces programmatically without direct access to the AWS console.

## Features

- **SSO Authentication**: Full support for AWS SSO with role-based authentication
- **Comprehensive WorkSpace Management**: List, start, stop, reboot, terminate, and create WorkSpaces
- **Rich CLI Interface**: Beautiful tables and progress indicators using Rich
- **Filtering & Search**: Filter WorkSpaces by state, user, or bundle
- **Configuration Management**: Save and load SSO and AWS configuration
- **Multiple Output Formats**: Table and JSON output support
- **Batch Operations**: Perform operations on multiple WorkSpaces simultaneously
- **Error Handling**: Comprehensive error handling with helpful messages

## Installation

### Option 1: Install with pip (recommended)

```bash
pip install -r requirements.txt
pip install -e .
```

### Option 2: Run directly

```bash
pip install -r requirements.txt
python aws_workspace_manager.py --help
```

## Quick Start

### 1. Configure the tool

```bash
aws-workspace-manager configure
```

You'll be prompted for:
- SSO Start URL (e.g., `https://your-org.awsapps.com/start`)
- SSO Region (e.g., `us-east-1`)
- Account ID (your AWS account ID)
- Role Name (the role to assume)
- Default Region (AWS region for WorkSpaces)

### 2. Login with SSO

```bash
aws-workspace-manager login
```

This will open your browser for SSO authentication.

### 3. List your WorkSpaces

```bash
aws-workspace-manager list
```

## Commands

### Authentication Commands

- `configure` - Configure SSO and AWS settings
- `login` - Authenticate with AWS SSO
- `logout` - Clear stored credentials

### WorkSpace Management Commands

- `list` - List all WorkSpaces with optional filtering
- `start <workspace-id> [workspace-id...]` - Start one or more WorkSpaces
- `stop <workspace-id> [workspace-id...]` - Stop one or more WorkSpaces
- `reboot <workspace-id> [workspace-id...]` - Reboot one or more WorkSpaces
- `terminate <workspace-id> [workspace-id...]` - Terminate one or more WorkSpaces
- `info <workspace-id>` - Get detailed information about a WorkSpace
- `create` - Create a new WorkSpace
- `modify <workspace-id>` - Modify WorkSpace properties

### Resource Discovery Commands

- `bundles` - List available WorkSpace bundles
- `directories` - List WorkSpaces directories

## Usage Examples

### List WorkSpaces with filtering

```bash
# List all WorkSpaces
aws-workspace-manager list

# List only running WorkSpaces
aws-workspace-manager list --state AVAILABLE

# List WorkSpaces for a specific user
aws-workspace-manager list --user john.doe

# Output as JSON
aws-workspace-manager list --format json
```

### Start/Stop WorkSpaces

```bash
# Start a single WorkSpace
aws-workspace-manager start ws-1234567890abcdef0

# Start multiple WorkSpaces
aws-workspace-manager start ws-1234567890abcdef0 ws-0987654321fedcba0

# Stop WorkSpaces
aws-workspace-manager stop ws-1234567890abcdef0
```

### Create a new WorkSpace

```bash
aws-workspace-manager create \
  --user john.doe \
  --bundle wsb-1234567890abcdef0 \
  --directory d-1234567890
```

### Modify WorkSpace properties

```bash
aws-workspace-manager modify ws-1234567890abcdef0 \
  --running-mode AUTO_STOP \
  --auto-stop-timeout 60 \
  --compute-type STANDARD
```

### Get detailed information

```bash
aws-workspace-manager info ws-1234567890abcdef0
```

## Configuration

The tool stores configuration in `~/.aws-workspace-manager.yaml`. Example configuration:

```yaml
sso_start_url: https://your-org.awsapps.com/start
sso_region: us-east-1
account_id: "123456789012"
role_name: WorkSpacesAdministrator
region: us-east-1
```

## SSO Authentication Flow

1. The tool uses AWS SSO OIDC to initiate device authorization
2. Opens your browser to the SSO login page
3. You authenticate with your organization credentials
4. The tool receives temporary credentials and creates an authenticated session
5. Credentials are cached for the session (typically 1 hour)

## Command-line Options

### Global Options

- `--profile <profile>` - Use a specific AWS profile instead of SSO
- `--region <region>` - Override the default AWS region
- `--verbose, -v` - Enable verbose output

### List Command Options

- `--state <state>` - Filter by WorkSpace state (AVAILABLE, STOPPED, etc.)
- `--user <username>` - Filter by username
- `--bundle <bundle-id>` - Filter by bundle ID
- `--format <format>` - Output format (table or json)

### Create Command Options

- `--user <username>` - Username for the WorkSpace (required)
- `--bundle <bundle-id>` - Bundle ID for the WorkSpace (required)
- `--directory <directory-id>` - Directory ID for the WorkSpace (required)
- `--encrypt-root` - Enable root volume encryption (default: true)
- `--encrypt-user` - Enable user volume encryption (default: true)
- `--encryption-key <key>` - KMS key for encryption

### Modify Command Options

- `--running-mode <mode>` - Running mode (AUTO_STOP or ALWAYS_ON)
- `--auto-stop-timeout <minutes>` - Auto-stop timeout in minutes
- `--root-volume-size <gb>` - Root volume size in GB
- `--user-volume-size <gb>` - User volume size in GB
- `--compute-type <type>` - Compute type (VALUE, STANDARD, PERFORMANCE, etc.)

## WorkSpace States

- `AVAILABLE` - WorkSpace is running and available
- `PENDING` - WorkSpace is being created
- `REBOOTING` - WorkSpace is rebooting
- `STARTING` - WorkSpace is starting up
- `STOPPED` - WorkSpace is stopped
- `STOPPING` - WorkSpace is stopping
- `SUSPENDED` - WorkSpace is suspended
- `TERMINATING` - WorkSpace is being terminated
- `TERMINATED` - WorkSpace has been terminated
- `UNHEALTHY` - WorkSpace is in an unhealthy state

## Error Handling

The tool provides comprehensive error handling:

- **Authentication errors**: Clear messages about SSO configuration and login issues
- **AWS API errors**: Formatted error messages from AWS services
- **Network errors**: Connection and timeout issues
- **Permission errors**: IAM permission issues with helpful suggestions

## Troubleshooting

### Authentication Issues

1. **SSO Configuration**: Ensure all SSO parameters are correct in the configuration
2. **Browser Issues**: If the browser doesn't open automatically, copy the URL manually
3. **Token Expiration**: Run `aws-workspace-manager login` again if you get authentication errors

### Permission Issues

Ensure your SSO role has the following permissions:
- `workspaces:Describe*`
- `workspaces:Start*`
- `workspaces:Stop*`
- `workspaces:Reboot*`
- `workspaces:Terminate*`
- `workspaces:Create*`
- `workspaces:Modify*`

### Common Issues

1. **"No WorkSpaces found"**: Check your region and permissions
2. **"SSO configuration incomplete"**: Run `aws-workspace-manager configure`
3. **"Authentication failed"**: Run `aws-workspace-manager login`

## Development

### Project Structure

```
.
├── aws_workspace_manager.py  # Main CLI application
├── workspace_client.py       # AWS WorkSpaces client wrapper
├── config.py                 # Configuration management
├── auth.py                   # SSO authentication handler
├── requirements.txt          # Python dependencies
├── setup.py                  # Package setup
└── README.md                 # This file
```

### Adding New Features

1. Add new commands to `aws_workspace_manager.py`
2. Implement client methods in `workspace_client.py`
3. Update configuration in `config.py` if needed
4. Add authentication logic in `auth.py` if required

## License

This project is licensed under the MIT License.

## Contributing

1. Fork the repository
2. Create a feature branch
3. Make your changes
4. Add tests if applicable
5. Submit a pull request

## Support

For issues and questions:
1. Check the troubleshooting section
2. Review AWS WorkSpaces documentation
3. Open an issue in the project repository
