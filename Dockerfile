# 1. Stage: Build
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /app

# Copy csproj and restore dependencies
COPY src/*.csproj ./src/
RUN dotnet restore ./src/src.csproj

# Copy remaining source code and publish
COPY . .
RUN dotnet publish ./src/src.csproj -c Release -o out

# 2. Stage: Runtime
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime
WORKDIR /app
COPY --from=build /app/out .

EXPOSE 5000
ENTRYPOINT ["dotnet", "src.dll"]
