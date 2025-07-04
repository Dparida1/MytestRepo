"""
Configuration Management
Handles storing and loading configuration settings for the AWS workspace tool.
"""

import os
import yaml
from pathlib import Path
from typing import Optional, Dict, Any


class Config:
    def __init__(self, config_file: Optional[str] = None):
        """Initialize configuration"""
        self.config_file = config_file or os.path.expanduser('~/.aws-workspace-manager.yaml')
        self.config_dir = Path(self.config_file).parent
        
        # Default values
        self.sso_start_url: Optional[str] = None
        self.sso_region: str = 'us-east-1'
        self.account_id: Optional[str] = None
        self.role_name: Optional[str] = None
        self.region: str = 'us-east-1'
        self.profile: Optional[str] = None
        
        # Load existing configuration
        self.load()
    
    def load(self):
        """Load configuration from file"""
        if not os.path.exists(self.config_file):
            return
        
        try:
            with open(self.config_file, 'r') as f:
                config_data = yaml.safe_load(f) or {}
            
            # Update instance variables with loaded values
            self.sso_start_url = config_data.get('sso_start_url')
            self.sso_region = config_data.get('sso_region', 'us-east-1')
            self.account_id = config_data.get('account_id')
            self.role_name = config_data.get('role_name')
            self.region = config_data.get('region', 'us-east-1')
            self.profile = config_data.get('profile')
            
        except Exception as e:
            print(f"Warning: Could not load configuration from {self.config_file}: {e}")
    
    def save(self):
        """Save configuration to file"""
        try:
            # Ensure the config directory exists
            self.config_dir.mkdir(parents=True, exist_ok=True)
            
            config_data = {
                'sso_start_url': self.sso_start_url,
                'sso_region': self.sso_region,
                'account_id': self.account_id,
                'role_name': self.role_name,
                'region': self.region,
                'profile': self.profile
            }
            
            # Remove None values
            config_data = {k: v for k, v in config_data.items() if v is not None}
            
            with open(self.config_file, 'w') as f:
                yaml.dump(config_data, f, default_flow_style=False)
            
        except Exception as e:
            raise Exception(f"Could not save configuration to {self.config_file}: {e}")
    
    def is_sso_configured(self) -> bool:
        """Check if SSO is properly configured"""
        return all([
            self.sso_start_url,
            self.sso_region,
            self.account_id,
            self.role_name
        ])
    
    def get_sso_session_name(self) -> str:
        """Generate a session name for SSO"""
        if self.account_id and self.role_name:
            return f"aws-workspace-manager-{self.account_id}-{self.role_name}"
        return "aws-workspace-manager-session"
    
    def to_dict(self) -> Dict[str, Any]:
        """Convert configuration to dictionary"""
        return {
            'sso_start_url': self.sso_start_url,
            'sso_region': self.sso_region,
            'account_id': self.account_id,
            'role_name': self.role_name,
            'region': self.region,
            'profile': self.profile
        }
    
    def __str__(self) -> str:
        """String representation of configuration"""
        config_dict = {k: v for k, v in self.to_dict().items() if v is not None}
        return yaml.dump(config_dict, default_flow_style=False)