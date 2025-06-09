# Deployment Guide for Event Registration System

## Hosting Options

### 1. Azure App Service (Recommended for .NET)

**Steps:**

1. Create an Azure account at https://azure.microsoft.com/
2. Install Azure CLI: `brew install azure-cli` (macOS)
3. Login to Azure: `az login`
4. Create a resource group:
   ```bash
   az group create --name EventRegistrationRG --location "East US"
   ```
5. Create an App Service plan:
   ```bash
   az appservice plan create --name EventRegistrationPlan --resource-group EventRegistrationRG --sku B1 --is-linux
   ```
6. Create the web app:
   ```bash
   az webapp create --resource-group EventRegistrationRG --plan EventRegistrationPlan --name your-unique-app-name --runtime "DOTNETCORE:8.0"
   ```
7. Deploy your application:
   ```bash
   cd WebApplication1
   dotnet publish -c Release
   cd bin/Release/net8.0/publish
   zip -r ../../../../../deploy.zip *
   cd ../../../../../
   az webapp deployment source config-zip --resource-group EventRegistrationRG --name your-unique-app-name --src deploy.zip
   ```

**Cost:** ~$13-55/month depending on plan

### 2. Railway (Easy deployment)

**Steps:**

1. Create account at https://railway.app/
2. Install Railway CLI: `npm install -g @railway/cli`
3. Login: `railway login`
4. Initialize project: `railway init`
5. Deploy: `railway up`

**Cost:** Free tier available, then $5/month

### 3. Render (Free tier available)

**Steps:**

1. Create account at https://render.com/
2. Connect your GitHub repository
3. Create a new Web Service
4. Use these settings:
   - Build Command: `dotnet publish WebApplication1/WebApplication1.csproj -c Release -o out`
   - Start Command: `dotnet out/WebApplication1.dll`

**Cost:** Free tier available, then $7/month

### 4. Heroku (Docker deployment)

**Steps:**

1. Create account at https://heroku.com/
2. Install Heroku CLI
3. Login: `heroku login`
4. Create app: `heroku create your-app-name`
5. Deploy with Docker:
   ```bash
   heroku container:login
   heroku container:push web --app your-app-name
   heroku container:release web --app your-app-name
   ```

**Cost:** $7/month (no free tier)

### 5. DigitalOcean App Platform

**Steps:**

1. Create account at https://digitalocean.com/
2. Go to App Platform in the control panel
3. Connect your GitHub repository
4. Configure build settings:
   - Build Command: `dotnet publish WebApplication1/WebApplication1.csproj -c Release -o out`
   - Run Command: `dotnet out/WebApplication1.dll`

**Cost:** $5/month

## Recommended Quick Start: Railway

For the easiest deployment, I recommend Railway:

1. Push your code to GitHub
2. Sign up at https://railway.app/
3. Connect your GitHub repository
4. Railway will automatically detect it's a .NET project
5. Your app will be live in minutes!

## Before Deploying

1. Test locally: `dotnet run --project WebApplication1`
2. Run tests: `dotnet test`
3. Commit all changes to Git
4. Push to GitHub/GitLab

## Production Considerations

- The SQLite database will reset on each deployment
- Consider upgrading to PostgreSQL for production
- Add proper logging and monitoring
- Configure HTTPS certificates
- Set up environment variables for secrets
