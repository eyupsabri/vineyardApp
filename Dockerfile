# Stage 1: build
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy everything
COPY . .

# Restore your solution
RUN dotnet restore VineyardApp/VineyardApp.sln

# Publish only the API project
WORKDIR /src/VineyardApp
RUN dotnet publish -c Release -o /app/publish

# Stage 2: runtime
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app

# Prepare volume mount for SQLite
RUN mkdir -p /app/Data

# Copy published output
COPY --from=build /app/publish .

# Bind Kestrel to port 80
ENV ASPNETCORE_URLS=http://+:80

# Expose only HTTP (Fly handles TLS)
EXPOSE 80

ENTRYPOINT ["dotnet", "VineyardApp.dll"]