#!/bin/bash

# Format all C# files in the project using CSharpier
echo "Formatting all C# files with CSharpier..."
dotnet csharpier format .

echo "Done! All C# files have been formatted."
