"""
AWS SSO Authentication Handler
Manages AWS SSO authentication and session handling.
"""

import boto3
import os
import time
import webbrowser
from typing import Optional
from botocore.exceptions import ClientError, BotoCoreError, NoCredentialsError
from config import Config


class SSOAuth:
    def __init__(self, config: Config):
        """Initialize SSO authentication"""
        self.config = config
        self._session = None
        self._credentials_cache = {}
    
    def login(self):
        """Perform SSO login"""
        if not self.config.is_sso_configured():
            raise Exception("SSO configuration is incomplete. Please run 'configure' first.")
        
        try:
            # Create SSO client
            sso_client = boto3.client('sso-oidc', region_name=self.config.sso_region)
            
            # Register client
            client_response = sso_client.register_client(
                clientName='AWS WorkSpace Manager',
                clientType='public'
            )
            
            client_id = client_response['clientId']
            client_secret = client_response['clientSecret']
            
            # Start device authorization
            device_response = sso_client.start_device_authorization(
                clientId=client_id,
                clientSecret=client_secret,
                startUrl=self.config.sso_start_url
            )
            
            # Display device code and open browser
            device_code = device_response['deviceCode']
            user_code = device_response['userCode']
            verification_uri = device_response['verificationUri']
            verification_uri_complete = device_response.get('verificationUriComplete')
            
            print(f"\nOpening browser to: {verification_uri}")
            print(f"User code: {user_code}")
            print("Please complete the authentication in your browser...")
            
            # Open browser to the verification URI
            if verification_uri_complete:
                webbrowser.open(verification_uri_complete)
            else:
                webbrowser.open(verification_uri)
            
            # Poll for completion
            expires_in = device_response['expiresIn']
            interval = device_response['interval']
            start_time = time.time()
            
            while time.time() - start_time < expires_in:
                try:
                    token_response = sso_client.create_token(
                        clientId=client_id,
                        clientSecret=client_secret,
                        grantType='urn:ietf:params:oauth:grant-type:device_code',
                        deviceCode=device_code
                    )
                    
                    # Store the token
                    access_token = token_response['accessToken']
                    self._store_sso_token(access_token)
                    
                    print("✓ Authentication successful!")
                    return
                    
                except ClientError as e:
                    error_code = e.response['Error']['Code']
                    if error_code == 'AuthorizationPendingException':
                        # Still waiting for user to authenticate
                        time.sleep(interval)
                        continue
                    elif error_code == 'SlowDownException':
                        # Polling too frequently
                        time.sleep(interval + 5)
                        continue
                    elif error_code == 'ExpiredTokenException':
                        raise Exception("Device code expired. Please try again.")
                    else:
                        raise Exception(f"Authentication failed: {e}")
            
            raise Exception("Authentication timed out. Please try again.")
            
        except Exception as e:
            raise Exception(f"SSO login failed: {e}")
    
    def get_session(self) -> boto3.Session:
        """Get an authenticated boto3 session"""
        if self._session:
            # Check if session is still valid
            try:
                # Test the session with a simple call
                sts = self._session.client('sts')
                sts.get_caller_identity()
                return self._session
            except (ClientError, NoCredentialsError):
                # Session expired, need to refresh
                self._session = None
        
        # Create new session
        self._session = self._create_session()
        return self._session
    
    def _create_session(self) -> boto3.Session:
        """Create a new authenticated session"""
        if not self.config.is_sso_configured():
            raise Exception("SSO configuration is incomplete. Please run 'configure' first.")
        
        try:
            # Try to use cached credentials first
            if self.config.profile:
                # Use AWS profile if specified
                session = boto3.Session(profile_name=self.config.profile)
                return session
            
            # Use SSO authentication
            access_token = self._get_sso_token()
            if not access_token:
                raise Exception("No valid SSO token found. Please run 'login' first.")
            
            # Get temporary credentials
            credentials = self._get_sso_credentials(access_token)
            
            # Create session with temporary credentials
            session = boto3.Session(
                aws_access_key_id=credentials['accessKeyId'],
                aws_secret_access_key=credentials['secretAccessKey'],
                aws_session_token=credentials['sessionToken'],
                region_name=self.config.region
            )
            
            return session
            
        except Exception as e:
            raise Exception(f"Failed to create authenticated session: {e}")
    
    def _get_sso_credentials(self, access_token: str) -> dict:
        """Get temporary credentials using SSO token"""
        try:
            sso_client = boto3.client('sso', region_name=self.config.sso_region)
            
            response = sso_client.get_role_credentials(
                roleName=self.config.role_name,
                accountId=self.config.account_id,
                accessToken=access_token
            )
            
            return response['roleCredentials']
            
        except ClientError as e:
            if e.response['Error']['Code'] == 'UnauthorizedException':
                raise Exception("SSO token expired or invalid. Please run 'login' again.")
            raise Exception(f"Failed to get SSO credentials: {e}")
    
    def _store_sso_token(self, access_token: str):
        """Store SSO token for later use"""
        # For simplicity, store in memory. In production, you might want to 
        # store this securely (e.g., AWS CLI cache)
        self._credentials_cache['access_token'] = access_token
        self._credentials_cache['timestamp'] = time.time()
    
    def _get_sso_token(self) -> Optional[str]:
        """Get stored SSO token if still valid"""
        token = self._credentials_cache.get('access_token')
        timestamp = self._credentials_cache.get('timestamp', 0)
        
        # Check if token is less than 1 hour old (tokens typically last 1 hour)
        if token and (time.time() - timestamp) < 3600:
            return token
        
        return None
    
    def logout(self):
        """Clear stored credentials"""
        self._credentials_cache.clear()
        self._session = None
        print("✓ Logged out successfully!")