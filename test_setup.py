#!/usr/bin/env python3
"""
Test Setup Script for AWS WorkSpaces Manager
Validates installation and basic functionality.
"""

import sys
import subprocess
import importlib
import os
from pathlib import Path

def check_python_version():
    """Check if Python version is 3.8+"""
    print("🐍 Checking Python version...")
    if sys.version_info < (3, 8):
        print(f"❌ Python 3.8+ required, found {sys.version}")
        return False
    print(f"✅ Python {sys.version_info.major}.{sys.version_info.minor}.{sys.version_info.micro}")
    return True

def check_dependencies():
    """Check if all required dependencies are installed"""
    print("\n📦 Checking dependencies...")
    
    dependencies = [
        'boto3',
        'click',
        'yaml',
        'tabulate',
        'colorama',
        'rich',
        'dateutil'
    ]
    
    missing = []
    for dep in dependencies:
        try:
            if dep == 'yaml':
                importlib.import_module('yaml')
            elif dep == 'dateutil':
                importlib.import_module('dateutil')
            else:
                importlib.import_module(dep)
            print(f"  ✅ {dep}")
        except ImportError:
            print(f"  ❌ {dep}")
            missing.append(dep)
    
    if missing:
        print(f"\n❌ Missing dependencies: {', '.join(missing)}")
        print("Run: pip install -r requirements.txt")
        return False
    
    return True

def check_files():
    """Check if all required files exist"""
    print("\n📁 Checking project files...")
    
    required_files = [
        'aws_workspace_manager.py',
        'workspace_client.py',
        'config.py',
        'auth.py',
        'requirements.txt',
        'setup.py',
        'README.md'
    ]
    
    missing = []
    for file in required_files:
        if os.path.exists(file):
            print(f"  ✅ {file}")
        else:
            print(f"  ❌ {file}")
            missing.append(file)
    
    if missing:
        print(f"\n❌ Missing files: {', '.join(missing)}")
        return False
    
    return True

def check_executables():
    """Check if scripts are executable"""
    print("\n🔧 Checking executable permissions...")
    
    executables = [
        'aws_workspace_manager.py',
        'install.sh',
        'examples.sh'
    ]
    
    for exe in executables:
        if os.path.exists(exe):
            if os.access(exe, os.X_OK):
                print(f"  ✅ {exe} (executable)")
            else:
                print(f"  ⚠️  {exe} (not executable)")
                print(f"     Run: chmod +x {exe}")
        else:
            print(f"  ❌ {exe} (missing)")

def test_imports():
    """Test importing the main modules"""
    print("\n🧪 Testing module imports...")
    
    modules = [
        ('config', 'Config'),
        ('auth', 'SSOAuth'),
        ('workspace_client', 'WorkSpaceClient')
    ]
    
    for module_name, class_name in modules:
        try:
            module = importlib.import_module(module_name)
            getattr(module, class_name)
            print(f"  ✅ {module_name}.{class_name}")
        except Exception as e:
            print(f"  ❌ {module_name}.{class_name}: {e}")
            return False
    
    return True

def test_cli_help():
    """Test CLI help command"""
    print("\n🖥️  Testing CLI help...")
    
    try:
        result = subprocess.run([
            sys.executable, 'aws_workspace_manager.py', '--help'
        ], capture_output=True, text=True, timeout=10)
        
        if result.returncode == 0 and 'AWS WorkSpaces Management Tool' in result.stdout:
            print("  ✅ CLI help works")
            return True
        else:
            print(f"  ❌ CLI help failed: {result.stderr}")
            return False
    except Exception as e:
        print(f"  ❌ CLI help error: {e}")
        return False

def test_config_creation():
    """Test configuration file creation"""
    print("\n⚙️  Testing configuration...")
    
    try:
        from config import Config
        
        # Create a test config in memory
        test_config = Config('/tmp/test-aws-workspace-manager.yaml')
        test_config.sso_start_url = 'https://test.awsapps.com/start'
        test_config.account_id = '123456789012'
        test_config.role_name = 'TestRole'
        
        # Test saving and loading
        test_config.save()
        
        # Load it back
        test_config2 = Config('/tmp/test-aws-workspace-manager.yaml')
        
        if (test_config2.sso_start_url == test_config.sso_start_url and
            test_config2.account_id == test_config.account_id and
            test_config2.role_name == test_config.role_name):
            print("  ✅ Configuration save/load works")
            
            # Clean up
            if os.path.exists('/tmp/test-aws-workspace-manager.yaml'):
                os.remove('/tmp/test-aws-workspace-manager.yaml')
            
            return True
        else:
            print("  ❌ Configuration save/load failed")
            return False
            
    except Exception as e:
        print(f"  ❌ Configuration test error: {e}")
        return False

def main():
    """Run all tests"""
    print("🔍 AWS WorkSpaces Manager - Setup Validation")
    print("=" * 50)
    
    tests = [
        check_python_version,
        check_dependencies,
        check_files,
        check_executables,
        test_imports,
        test_cli_help,
        test_config_creation
    ]
    
    passed = 0
    total = len(tests)
    
    for test in tests:
        if test():
            passed += 1
    
    print(f"\n📊 Test Results: {passed}/{total} passed")
    
    if passed == total:
        print("\n🎉 All tests passed! The tool is ready to use.")
        print("\nNext steps:")
        print("  1. Run: aws-workspace-manager configure")
        print("  2. Run: aws-workspace-manager login")
        print("  3. Run: aws-workspace-manager list")
        print("\nFor examples: ./examples.sh")
        return True
    else:
        print(f"\n❌ {total - passed} test(s) failed. Please fix the issues above.")
        return False

if __name__ == '__main__':
    success = main()
    sys.exit(0 if success else 1)