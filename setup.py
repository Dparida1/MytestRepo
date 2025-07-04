from setuptools import setup, find_packages

with open("README.md", "r", encoding="utf-8") as fh:
    long_description = fh.read()

with open("requirements.txt", "r", encoding="utf-8") as fh:
    requirements = [line.strip() for line in fh if line.strip() and not line.startswith("#")]

setup(
    name="aws-workspace-manager",
    version="1.0.0",
    author="AWS WorkSpace Manager",
    description="A CLI tool for managing AWS WorkSpaces with SSO authentication support",
    long_description=long_description,
    long_description_content_type="text/markdown",
    packages=find_packages(),
    classifiers=[
        "Development Status :: 4 - Beta",
        "Intended Audience :: Developers",
        "Intended Audience :: System Administrators",
        "License :: OSI Approved :: MIT License",
        "Operating System :: OS Independent",
        "Programming Language :: Python :: 3",
        "Programming Language :: Python :: 3.8",
        "Programming Language :: Python :: 3.9",
        "Programming Language :: Python :: 3.10",
        "Programming Language :: Python :: 3.11",
        "Topic :: System :: Systems Administration",
        "Topic :: Utilities",
    ],
    python_requires=">=3.8",
    install_requires=requirements,
    entry_points={
        "console_scripts": [
            "aws-workspace-manager=aws_workspace_manager:cli",
            "awsm=aws_workspace_manager:cli",
        ],
    },
    keywords="aws workspaces sso cli management",
    project_urls={
        "Bug Reports": "https://github.com/your-org/aws-workspace-manager/issues",
        "Source": "https://github.com/your-org/aws-workspace-manager",
    },
)