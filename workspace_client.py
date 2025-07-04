"""
AWS WorkSpaces Client Wrapper
Provides a high-level interface for AWS WorkSpaces operations.
"""

import boto3
from typing import List, Dict, Any, Optional
from botocore.exceptions import ClientError, BotoCoreError


class WorkSpaceClient:
    def __init__(self, session: boto3.Session, region: str = 'us-east-1'):
        """Initialize the WorkSpaces client"""
        self.session = session
        self.region = region
        self.client = session.client('workspaces', region_name=region)
    
    def list_workspaces(self, state: Optional[str] = None, user: Optional[str] = None, 
                       bundle_id: Optional[str] = None) -> List[Dict[str, Any]]:
        """List WorkSpaces with optional filtering"""
        try:
            request = {}
            
            # Build filter criteria
            if state or user or bundle_id:
                request['Filter'] = {}
                
                if state:
                    request['Filter']['State'] = state
                if user:
                    request['Filter']['UserName'] = user
                if bundle_id:
                    request['Filter']['BundleId'] = bundle_id
            
            # Handle pagination
            workspaces = []
            paginator = self.client.get_paginator('describe_workspaces')
            
            for page in paginator.paginate(**request):
                workspaces.extend(page.get('Workspaces', []))
            
            return workspaces
            
        except ClientError as e:
            raise Exception(f"AWS API error: {e}")
        except BotoCoreError as e:
            raise Exception(f"AWS connection error: {e}")
    
    def get_workspace_info(self, workspace_id: str) -> Optional[Dict[str, Any]]:
        """Get detailed information about a specific WorkSpace"""
        try:
            response = self.client.describe_workspaces(
                WorkspaceIds=[workspace_id]
            )
            
            workspaces = response.get('Workspaces', [])
            return workspaces[0] if workspaces else None
            
        except ClientError as e:
            if e.response['Error']['Code'] == 'ResourceNotFoundException':
                return None
            raise Exception(f"AWS API error: {e}")
        except BotoCoreError as e:
            raise Exception(f"AWS connection error: {e}")
    
    def start_workspaces(self, workspace_ids: List[str]) -> List[Dict[str, Any]]:
        """Start one or more WorkSpaces"""
        try:
            # Prepare the request
            requests = [{'WorkspaceId': ws_id} for ws_id in workspace_ids]
            
            response = self.client.start_workspaces(StartWorkspaceRequests=requests)
            return response.get('FailedRequests', []) + response.get('SuccessfulRequests', [])
            
        except ClientError as e:
            raise Exception(f"AWS API error: {e}")
        except BotoCoreError as e:
            raise Exception(f"AWS connection error: {e}")
    
    def stop_workspaces(self, workspace_ids: List[str]) -> List[Dict[str, Any]]:
        """Stop one or more WorkSpaces"""
        try:
            # Prepare the request
            requests = [{'WorkspaceId': ws_id} for ws_id in workspace_ids]
            
            response = self.client.stop_workspaces(StopWorkspaceRequests=requests)
            return response.get('FailedRequests', []) + response.get('SuccessfulRequests', [])
            
        except ClientError as e:
            raise Exception(f"AWS API error: {e}")
        except BotoCoreError as e:
            raise Exception(f"AWS connection error: {e}")
    
    def reboot_workspaces(self, workspace_ids: List[str]) -> List[Dict[str, Any]]:
        """Reboot one or more WorkSpaces"""
        try:
            # Prepare the request
            requests = [{'WorkspaceId': ws_id} for ws_id in workspace_ids]
            
            response = self.client.reboot_workspaces(RebootWorkspaceRequests=requests)
            return response.get('FailedRequests', []) + response.get('SuccessfulRequests', [])
            
        except ClientError as e:
            raise Exception(f"AWS API error: {e}")
        except BotoCoreError as e:
            raise Exception(f"AWS connection error: {e}")
    
    def terminate_workspaces(self, workspace_ids: List[str]) -> List[Dict[str, Any]]:
        """Terminate one or more WorkSpaces"""
        try:
            # Prepare the request
            requests = [{'WorkspaceId': ws_id} for ws_id in workspace_ids]
            
            response = self.client.terminate_workspaces(TerminateWorkspaceRequests=requests)
            return response.get('FailedRequests', []) + response.get('SuccessfulRequests', [])
            
        except ClientError as e:
            raise Exception(f"AWS API error: {e}")
        except BotoCoreError as e:
            raise Exception(f"AWS connection error: {e}")
    
    def list_bundles(self) -> List[Dict[str, Any]]:
        """List available WorkSpace bundles"""
        try:
            bundles = []
            paginator = self.client.get_paginator('describe_workspace_bundles')
            
            for page in paginator.paginate():
                bundles.extend(page.get('Bundles', []))
            
            return bundles
            
        except ClientError as e:
            raise Exception(f"AWS API error: {e}")
        except BotoCoreError as e:
            raise Exception(f"AWS connection error: {e}")
    
    def create_workspace(self, user_name: str, bundle_id: str, directory_id: str,
                        root_volume_encryption_enabled: bool = True,
                        user_volume_encryption_enabled: bool = True,
                        volume_encryption_key: Optional[str] = None,
                        tags: Optional[List[Dict[str, str]]] = None) -> Dict[str, Any]:
        """Create a new WorkSpace"""
        try:
            workspace_request = {
                'DirectoryId': directory_id,
                'UserName': user_name,
                'BundleId': bundle_id,
                'RootVolumeEncryptionEnabled': root_volume_encryption_enabled,
                'UserVolumeEncryptionEnabled': user_volume_encryption_enabled
            }
            
            if volume_encryption_key:
                workspace_request['VolumeEncryptionKey'] = volume_encryption_key
            
            if tags:
                workspace_request['Tags'] = tags
            
            response = self.client.create_workspaces(
                Workspaces=[workspace_request]
            )
            
            return response
            
        except ClientError as e:
            raise Exception(f"AWS API error: {e}")
        except BotoCoreError as e:
            raise Exception(f"AWS connection error: {e}")
    
    def list_directories(self) -> List[Dict[str, Any]]:
        """List WorkSpaces directories"""
        try:
            directories = []
            paginator = self.client.get_paginator('describe_workspace_directories')
            
            for page in paginator.paginate():
                directories.extend(page.get('Directories', []))
            
            return directories
            
        except ClientError as e:
            raise Exception(f"AWS API error: {e}")
        except BotoCoreError as e:
            raise Exception(f"AWS connection error: {e}")
    
    def modify_workspace_properties(self, workspace_id: str, 
                                  running_mode: Optional[str] = None,
                                  running_mode_auto_stop_timeout: Optional[int] = None,
                                  root_volume_size_gib: Optional[int] = None,
                                  user_volume_size_gib: Optional[int] = None,
                                  compute_type_name: Optional[str] = None) -> Dict[str, Any]:
        """Modify WorkSpace properties"""
        try:
            workspace_properties = {}
            
            if running_mode:
                workspace_properties['RunningMode'] = running_mode
            if running_mode_auto_stop_timeout:
                workspace_properties['RunningModeAutoStopTimeoutInMinutes'] = running_mode_auto_stop_timeout
            if root_volume_size_gib:
                workspace_properties['RootVolumeSizeGib'] = root_volume_size_gib
            if user_volume_size_gib:
                workspace_properties['UserVolumeSizeGib'] = user_volume_size_gib
            if compute_type_name:
                workspace_properties['ComputeTypeName'] = compute_type_name
            
            response = self.client.modify_workspace_properties(
                WorkspaceId=workspace_id,
                WorkspaceProperties=workspace_properties
            )
            
            return response
            
        except ClientError as e:
            raise Exception(f"AWS API error: {e}")
        except BotoCoreError as e:
            raise Exception(f"AWS connection error: {e}")