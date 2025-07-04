#!/usr/bin/env python3
"""
AWS WorkSpaces Management Tool
A CLI tool for managing AWS WorkSpaces with SSO authentication support.
"""

import click
import sys
import json
from rich.console import Console
from rich.table import Table
from rich.panel import Panel
from rich import print as rprint
from datetime import datetime
from typing import Optional, Dict, Any

from workspace_client import WorkSpaceClient
from config import Config
from auth import SSOAuth

console = Console()


class WorkSpaceManager:
    def __init__(self):
        self.config = Config()
        self.client = None
        
    def ensure_authenticated(self):
        """Ensure we have a valid authenticated client"""
        if not self.client:
            auth = SSOAuth(self.config)
            session = auth.get_session()
            self.client = WorkSpaceClient(session, self.config.region)
        return self.client


@click.group()
@click.option('--profile', default=None, help='AWS profile to use')
@click.option('--region', default=None, help='AWS region to use')
@click.option('--verbose', '-v', is_flag=True, help='Enable verbose output')
@click.pass_context
def cli(ctx, profile, region, verbose):
    """AWS WorkSpaces Management Tool
    
    Manage your AWS WorkSpaces with SSO authentication support.
    """
    ctx.ensure_object(dict)
    ctx.obj['verbose'] = verbose
    
    # Initialize the manager
    manager = WorkSpaceManager()
    if profile:
        manager.config.profile = profile
    if region:
        manager.config.region = region
    
    ctx.obj['manager'] = manager


@cli.command()
@click.pass_context
def configure(ctx):
    """Configure the tool with your AWS SSO settings"""
    manager = ctx.obj['manager']
    config = manager.config
    
    console.print(Panel.fit("AWS WorkSpaces Manager Configuration", style="bold blue"))
    
    # Get SSO configuration
    sso_start_url = click.prompt("SSO Start URL", default=config.sso_start_url or "")
    sso_region = click.prompt("SSO Region", default=config.sso_region or "us-east-1")
    account_id = click.prompt("Account ID", default=config.account_id or "")
    role_name = click.prompt("Role Name", default=config.role_name or "")
    region = click.prompt("Default Region", default=config.region or "us-east-1")
    
    # Save configuration
    config.sso_start_url = sso_start_url
    config.sso_region = sso_region
    config.account_id = account_id
    config.role_name = role_name
    config.region = region
    config.save()
    
    console.print("[green]✓[/green] Configuration saved successfully!")


@cli.command()
@click.pass_context
def login(ctx):
    """Authenticate with AWS SSO"""
    manager = ctx.obj['manager']
    
    try:
        auth = SSOAuth(manager.config)
        auth.login()
        console.print("[green]✓[/green] Successfully authenticated with AWS SSO!")
    except Exception as e:
        console.print(f"[red]✗[/red] Authentication failed: {e}")
        sys.exit(1)


@cli.command()
@click.pass_context
def logout(ctx):
    """Clear stored credentials"""
    manager = ctx.obj['manager']
    
    try:
        auth = SSOAuth(manager.config)
        auth.logout()
    except Exception as e:
        console.print(f"[red]✗[/red] Logout failed: {e}")


@cli.command()
@click.option('--state', type=click.Choice(['AVAILABLE', 'PENDING', 'REBOOTING', 'STARTING', 'STOPPED', 'STOPPING', 'SUSPENDED', 'TERMINATING', 'TERMINATED', 'UNHEALTHY']), help='Filter by workspace state')
@click.option('--user', help='Filter by username')
@click.option('--bundle', help='Filter by bundle ID')
@click.option('--format', type=click.Choice(['table', 'json']), default='table', help='Output format')
@click.pass_context
def list(ctx, state, user, bundle, format):
    """List all WorkSpaces"""
    manager = ctx.obj['manager']
    
    try:
        client = manager.ensure_authenticated()
        workspaces = client.list_workspaces(state=state, user=user, bundle_id=bundle)
        
        if format == 'json':
            click.echo(json.dumps(workspaces, indent=2, default=str))
            return
        
        if not workspaces:
            console.print("[yellow]No WorkSpaces found.[/yellow]")
            return
        
        table = Table(title="AWS WorkSpaces")
        table.add_column("Workspace ID", style="cyan")
        table.add_column("User", style="green")
        table.add_column("State", style="bold")
        table.add_column("Bundle ID", style="blue")
        table.add_column("IP Address", style="magenta")
        table.add_column("Computer Name")
        
        for ws in workspaces:
            state_style = "green" if ws['State'] == 'AVAILABLE' else "red" if ws['State'] in ['STOPPED', 'TERMINATED'] else "yellow"
            table.add_row(
                ws['WorkspaceId'],
                ws['UserName'],
                f"[{state_style}]{ws['State']}[/{state_style}]",
                ws.get('BundleId', 'N/A'),
                ws.get('IpAddress', 'N/A'),
                ws.get('ComputerName', 'N/A')
            )
        
        console.print(table)
        
    except Exception as e:
        console.print(f"[red]✗[/red] Failed to list WorkSpaces: {e}")
        sys.exit(1)


@cli.command()
@click.argument('workspace_ids', nargs=-1, required=True)
@click.pass_context
def start(ctx, workspace_ids):
    """Start one or more WorkSpaces"""
    manager = ctx.obj['manager']
    
    try:
        client = manager.ensure_authenticated()
        
        with console.status("Starting WorkSpaces..."):
            results = client.start_workspaces(list(workspace_ids))
        
        _display_workspace_operation_results(results, "Start")
        
    except Exception as e:
        console.print(f"[red]✗[/red] Failed to start WorkSpaces: {e}")
        sys.exit(1)


@cli.command()
@click.argument('workspace_ids', nargs=-1, required=True)
@click.pass_context
def stop(ctx, workspace_ids):
    """Stop one or more WorkSpaces"""
    manager = ctx.obj['manager']
    
    try:
        client = manager.ensure_authenticated()
        
        with console.status("Stopping WorkSpaces..."):
            results = client.stop_workspaces(list(workspace_ids))
        
        _display_workspace_operation_results(results, "Stop")
        
    except Exception as e:
        console.print(f"[red]✗[/red] Failed to stop WorkSpaces: {e}")
        sys.exit(1)


@cli.command()
@click.argument('workspace_ids', nargs=-1, required=True)
@click.pass_context
def reboot(ctx, workspace_ids):
    """Reboot one or more WorkSpaces"""
    manager = ctx.obj['manager']
    
    try:
        client = manager.ensure_authenticated()
        
        with console.status("Rebooting WorkSpaces..."):
            results = client.reboot_workspaces(list(workspace_ids))
        
        _display_workspace_operation_results(results, "Reboot")
        
    except Exception as e:
        console.print(f"[red]✗[/red] Failed to reboot WorkSpaces: {e}")
        sys.exit(1)


@cli.command()
@click.argument('workspace_ids', nargs=-1, required=True)
@click.option('--force', is_flag=True, help='Skip confirmation prompt')
@click.pass_context
def terminate(ctx, workspace_ids, force):
    """Terminate one or more WorkSpaces"""
    manager = ctx.obj['manager']
    
    if not force:
        console.print(f"[red]Warning:[/red] This will permanently terminate {len(workspace_ids)} WorkSpace(s):")
        for ws_id in workspace_ids:
            console.print(f"  - {ws_id}")
        
        if not click.confirm("Are you sure you want to continue?"):
            console.print("Operation cancelled.")
            return
    
    try:
        client = manager.ensure_authenticated()
        
        with console.status("Terminating WorkSpaces..."):
            results = client.terminate_workspaces(list(workspace_ids))
        
        _display_workspace_operation_results(results, "Terminate")
        
    except Exception as e:
        console.print(f"[red]✗[/red] Failed to terminate WorkSpaces: {e}")
        sys.exit(1)


@cli.command()
@click.argument('workspace_id')
@click.pass_context
def info(ctx, workspace_id):
    """Get detailed information about a WorkSpace"""
    manager = ctx.obj['manager']
    
    try:
        client = manager.ensure_authenticated()
        workspace = client.get_workspace_info(workspace_id)
        
        if not workspace:
            console.print(f"[red]WorkSpace {workspace_id} not found.[/red]")
            return
        
        # Display detailed information
        panel_content = []
        panel_content.append(f"[bold]Workspace ID:[/bold] {workspace['WorkspaceId']}")
        panel_content.append(f"[bold]User:[/bold] {workspace['UserName']}")
        panel_content.append(f"[bold]State:[/bold] {workspace['State']}")
        panel_content.append(f"[bold]Bundle ID:[/bold] {workspace.get('BundleId', 'N/A')}")
        panel_content.append(f"[bold]Directory ID:[/bold] {workspace.get('DirectoryId', 'N/A')}")
        panel_content.append(f"[bold]IP Address:[/bold] {workspace.get('IpAddress', 'N/A')}")
        panel_content.append(f"[bold]Computer Name:[/bold] {workspace.get('ComputerName', 'N/A')}")
        panel_content.append(f"[bold]Subnet ID:[/bold] {workspace.get('SubnetId', 'N/A')}")
        
        if workspace.get('ErrorMessage'):
            panel_content.append(f"[bold red]Error:[/bold red] {workspace['ErrorMessage']}")
        
        console.print(Panel("\n".join(panel_content), title=f"WorkSpace Details: {workspace_id}"))
        
    except Exception as e:
        console.print(f"[red]✗[/red] Failed to get WorkSpace info: {e}")
        sys.exit(1)


@cli.command()
@click.pass_context
def bundles(ctx):
    """List available WorkSpace bundles"""
    manager = ctx.obj['manager']
    
    try:
        client = manager.ensure_authenticated()
        bundles = client.list_bundles()
        
        if not bundles:
            console.print("[yellow]No bundles found.[/yellow]")
            return
        
        table = Table(title="Available WorkSpace Bundles")
        table.add_column("Bundle ID", style="cyan")
        table.add_column("Name", style="green")
        table.add_column("Description")
        table.add_column("Compute Type", style="blue")
        table.add_column("Storage", style="magenta")
        
        for bundle in bundles:
            compute_type = bundle.get('ComputeType', {})
            root_storage = bundle.get('RootStorage', {})
            user_storage = bundle.get('UserStorage', {})
            
            storage_info = f"Root: {root_storage.get('Capacity', 'N/A')}GB, User: {user_storage.get('Capacity', 'N/A')}GB"
            
            table.add_row(
                bundle['BundleId'],
                bundle.get('Name', 'N/A'),
                bundle.get('Description', 'N/A'),
                compute_type.get('Name', 'N/A'),
                storage_info
            )
        
        console.print(table)
        
    except Exception as e:
        console.print(f"[red]✗[/red] Failed to list bundles: {e}")
        sys.exit(1)


@cli.command()
@click.pass_context
def directories(ctx):
    """List WorkSpaces directories"""
    manager = ctx.obj['manager']
    
    try:
        client = manager.ensure_authenticated()
        directories = client.list_directories()
        
        if not directories:
            console.print("[yellow]No directories found.[/yellow]")
            return
        
        table = Table(title="WorkSpaces Directories")
        table.add_column("Directory ID", style="cyan")
        table.add_column("Name", style="green")
        table.add_column("Type", style="blue")
        table.add_column("State", style="bold")
        table.add_column("Registration Code", style="magenta")
        
        for directory in directories:
            state_style = "green" if directory['State'] == 'REGISTERED' else "red"
            table.add_row(
                directory['DirectoryId'],
                directory.get('DirectoryName', 'N/A'),
                directory.get('DirectoryType', 'N/A'),
                f"[{state_style}]{directory['State']}[/{state_style}]",
                directory.get('RegistrationCode', 'N/A')
            )
        
        console.print(table)
        
    except Exception as e:
        console.print(f"[red]✗[/red] Failed to list directories: {e}")
        sys.exit(1)


@cli.command()
@click.option('--user', required=True, help='Username for the WorkSpace')
@click.option('--bundle', required=True, help='Bundle ID for the WorkSpace')
@click.option('--directory', required=True, help='Directory ID for the WorkSpace')
@click.option('--encrypt-root', is_flag=True, default=True, help='Enable root volume encryption')
@click.option('--encrypt-user', is_flag=True, default=True, help='Enable user volume encryption')
@click.option('--encryption-key', help='KMS key for encryption')
@click.pass_context
def create(ctx, user, bundle, directory, encrypt_root, encrypt_user, encryption_key):
    """Create a new WorkSpace"""
    manager = ctx.obj['manager']
    
    try:
        client = manager.ensure_authenticated()
        
        console.print(f"Creating WorkSpace for user [green]{user}[/green]...")
        
        with console.status("Creating WorkSpace..."):
            result = client.create_workspace(
                user_name=user,
                bundle_id=bundle,
                directory_id=directory,
                root_volume_encryption_enabled=encrypt_root,
                user_volume_encryption_enabled=encrypt_user,
                volume_encryption_key=encryption_key
            )
        
        # Display results
        failed_requests = result.get('FailedRequests', [])
        pending_requests = result.get('PendingRequests', [])
        
        if failed_requests:
            console.print("[red]✗[/red] Failed to create WorkSpace:")
            for req in failed_requests:
                console.print(f"  Error: {req.get('ErrorMessage', 'Unknown error')}")
        
        if pending_requests:
            console.print("[green]✓[/green] WorkSpace creation initiated:")
            for req in pending_requests:
                console.print(f"  WorkSpace ID: [cyan]{req['WorkspaceId']}[/cyan]")
                console.print(f"  User: [green]{req['UserName']}[/green]")
                console.print(f"  State: [yellow]{req['State']}[/yellow]")
        
    except Exception as e:
        console.print(f"[red]✗[/red] Failed to create WorkSpace: {e}")
        sys.exit(1)


@cli.command()
@click.argument('workspace_id')
@click.option('--running-mode', type=click.Choice(['AUTO_STOP', 'ALWAYS_ON']), help='Running mode')
@click.option('--auto-stop-timeout', type=int, help='Auto-stop timeout in minutes')
@click.option('--root-volume-size', type=int, help='Root volume size in GB')
@click.option('--user-volume-size', type=int, help='User volume size in GB')
@click.option('--compute-type', type=click.Choice(['VALUE', 'STANDARD', 'PERFORMANCE', 'POWER', 'GRAPHICS', 'POWERPRO', 'GRAPHICSPRO']), help='Compute type')
@click.pass_context
def modify(ctx, workspace_id, running_mode, auto_stop_timeout, root_volume_size, user_volume_size, compute_type):
    """Modify WorkSpace properties"""
    manager = ctx.obj['manager']
    
    try:
        client = manager.ensure_authenticated()
        
        console.print(f"Modifying WorkSpace [cyan]{workspace_id}[/cyan]...")
        
        with console.status("Modifying WorkSpace properties..."):
            result = client.modify_workspace_properties(
                workspace_id=workspace_id,
                running_mode=running_mode,
                running_mode_auto_stop_timeout=auto_stop_timeout,
                root_volume_size_gib=root_volume_size,
                user_volume_size_gib=user_volume_size,
                compute_type_name=compute_type
            )
        
        console.print("[green]✓[/green] WorkSpace properties modified successfully!")
        
    except Exception as e:
        console.print(f"[red]✗[/red] Failed to modify WorkSpace: {e}")
        sys.exit(1)


def _display_workspace_operation_results(results, operation):
    """Display the results of a workspace operation"""
    if not results:
        console.print(f"[yellow]No results returned for {operation.lower()} operation.[/yellow]")
        return
    
    table = Table(title=f"{operation} WorkSpaces Results")
    table.add_column("Workspace ID", style="cyan")
    table.add_column("Status", style="bold")
    table.add_column("Error Code", style="red")
    table.add_column("Error Message", style="red")
    
    for result in results:
        error_code = result.get('ErrorCode', '')
        error_message = result.get('ErrorMessage', '')
        
        if error_code:
            status = "[red]Failed[/red]"
        else:
            status = "[green]Success[/green]"
        
        table.add_row(
            result.get('WorkspaceId', 'N/A'),
            status,
            error_code,
            error_message
        )
    
    console.print(table)


if __name__ == '__main__':
    cli()