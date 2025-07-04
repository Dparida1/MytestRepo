#!/bin/bash
# AWS WorkSpaces Manager - Usage Examples

echo "📚 AWS WorkSpaces Manager - Usage Examples"
echo "==========================================="
echo ""

echo "🔧 Initial Setup:"
echo "  aws-workspace-manager configure"
echo "  aws-workspace-manager login"
echo ""

echo "📋 List Operations:"
echo "  # List all workspaces"
echo "  aws-workspace-manager list"
echo ""
echo "  # List only running workspaces"
echo "  aws-workspace-manager list --state AVAILABLE"
echo ""
echo "  # List workspaces for specific user"
echo "  aws-workspace-manager list --user john.doe"
echo ""
echo "  # Output as JSON"
echo "  aws-workspace-manager list --format json"
echo ""

echo "⚡ Power Management:"
echo "  # Start a workspace"
echo "  aws-workspace-manager start ws-1234567890abcdef0"
echo ""
echo "  # Start multiple workspaces"
echo "  aws-workspace-manager start ws-1234567890abcdef0 ws-0987654321fedcba0"
echo ""
echo "  # Stop a workspace"
echo "  aws-workspace-manager stop ws-1234567890abcdef0"
echo ""
echo "  # Reboot a workspace"
echo "  aws-workspace-manager reboot ws-1234567890abcdef0"
echo ""

echo "🔍 Information Commands:"
echo "  # Get detailed workspace info"
echo "  aws-workspace-manager info ws-1234567890abcdef0"
echo ""
echo "  # List available bundles"
echo "  aws-workspace-manager bundles"
echo ""
echo "  # List directories"
echo "  aws-workspace-manager directories"
echo ""

echo "🏗️ Create and Modify:"
echo "  # Create a new workspace"
echo "  aws-workspace-manager create \\"
echo "    --user john.doe \\"
echo "    --bundle wsb-1234567890abcdef0 \\"
echo "    --directory d-1234567890"
echo ""
echo "  # Modify workspace properties"
echo "  aws-workspace-manager modify ws-1234567890abcdef0 \\"
echo "    --running-mode AUTO_STOP \\"
echo "    --auto-stop-timeout 60 \\"
echo "    --compute-type STANDARD"
echo ""

echo "🗑️ Cleanup:"
echo "  # Terminate workspace (with confirmation)"
echo "  aws-workspace-manager terminate ws-1234567890abcdef0"
echo ""
echo "  # Force terminate without confirmation"
echo "  aws-workspace-manager terminate ws-1234567890abcdef0 --force"
echo ""

echo "🔐 Authentication:"
echo "  # Login again if token expires"
echo "  aws-workspace-manager login"
echo ""
echo "  # Clear stored credentials"
echo "  aws-workspace-manager logout"
echo ""

echo "🛠️ Advanced Usage:"
echo "  # Use specific AWS profile"
echo "  aws-workspace-manager --profile my-profile list"
echo ""
echo "  # Use different region"
echo "  aws-workspace-manager --region us-west-2 list"
echo ""
echo "  # Verbose output"
echo "  aws-workspace-manager --verbose list"
echo ""

echo "📝 Batch Operations Example Script:"
echo ""
cat << 'EOF'
#!/bin/bash
# Example: Start all stopped workspaces for a user

USER="john.doe"
WORKSPACES=$(aws-workspace-manager list --user "$USER" --state STOPPED --format json | jq -r '.[].WorkspaceId')

if [ -n "$WORKSPACES" ]; then
    echo "Starting workspaces for user $USER..."
    aws-workspace-manager start $WORKSPACES
else
    echo "No stopped workspaces found for user $USER"
fi
EOF