FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src

# 👇 Copy only the project folder
COPY SimpleAdminWebApp/SimpleAdminWebApp/ .

RUN dotnet restore SimpleAdminWebApp.csproj

RUN dotnet publish -c Release -o /app

FROM base AS final
WORKDIR /app
COPY --from=build /app .
ENTRYPOINT ["dotnet", "SimpleAdminWebApp.dll"]
