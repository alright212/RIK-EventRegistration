#!/bin/bash

# Simple deployment script for Event Registration System

echo "🚀 Deploying Event Registration System..."

# Check if we're in the right directory
if [ ! -f "EventRegistration.sln" ]; then
    echo "❌ Error: Please run this script from the EventRegistration root directory"
    exit 1
fi

# Clean and restore
echo "🧹 Cleaning and restoring packages..."
dotnet clean
dotnet restore

# Run tests
echo "🧪 Running tests..."
dotnet test
if [ $? -ne 0 ]; then
    echo "❌ Tests failed! Deployment aborted."
    exit 1
fi

# Build for production
echo "🔨 Building for production..."
dotnet publish WebApplication1/WebApplication1.csproj -c Release -o ./publish

echo "✅ Build complete! Files are in ./publish directory"
echo ""
echo "🌐 Next steps for deployment:"
echo "1. For Railway: Push to GitHub and connect repository at https://railway.app/"
echo "2. For Render: Connect GitHub repository at https://render.com/"
echo "3. For Azure: Use 'az webapp deployment source config-zip' command"
echo "4. For Docker: Run 'docker build -t event-registration .'"
echo ""
echo "📖 See DEPLOYMENT.md for detailed instructions"
