# Use the official .NET SDK image to build the app
FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build

# Set the working directory
WORKDIR /app

# Copy the csproj and restore any dependencies (via nuget)
COPY *.csproj ./
RUN dotnet restore

# Copy the rest of the files and publish the app
COPY . ./
RUN dotnet publish -c Release -o out

# Define the entrypoint
ENTRYPOINT ["dotnet", "out/SimpleAdminWebApp.dll"]
