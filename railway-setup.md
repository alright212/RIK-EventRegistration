# Railway PostgreSQL Setup

## Step 1: Add PostgreSQL to your Railway project

1. Go to your Railway dashboard
2. Click "Add Service" → "Database" → "PostgreSQL"
3. Railway will create a PostgreSQL database and provide connection details

## Step 2: Get your database connection string

In Railway dashboard, go to your PostgreSQL service and copy the connection string.
It will look like: `postgresql://username:password@host:port/database`

## Step 3: Set environment variable

In your Railway app service:
1. Go to "Variables" tab
2. Add new variable:
   - Name: `ConnectionStrings__DefaultConnection`
   - Value: `Your PostgreSQL connection string from Step 2`

## Step 4: Update your app to use PostgreSQL

✅ **Already done!** Your app now automatically detects PostgreSQL vs SQLite:

- If connection string contains "postgresql" → Uses PostgreSQL
- Otherwise → Uses SQLite (for local development)

## Step 5: Deploy

1. Commit and push your changes to GitHub
2. Railway will automatically redeploy with PostgreSQL
3. Your data will now persist between deployments!

## Alternative Solution: Volume Mount (if you prefer SQLite)

If you want to keep SQLite but make it persistent:

1. In Railway, go to your service → "Settings" → "Volumes"
2. Add volume: `/app/data` → This makes the database file persistent
3. No code changes needed - your current SQLite setup will work

## Testing the Fix

After deployment:
1. Add some test events and participants
2. Make a code change and redeploy
3. Your data should still be there! 🎉
