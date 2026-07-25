# Build Stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY ["ScoutingTool.Api.csproj", "./"]
RUN dotnet restore "ScoutingTool.Api.csproj"

COPY . .
RUN dotnet publish "ScoutingTool.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Runtime Stage (Change 10.0 to 8.0 here!)
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app

# Invariant globalization setting to prevent Linux locale crashes
ENV DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=1

COPY --from=build /app/publish .

EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080

ENTRYPOINT ["dotnet", "ScoutingTool.Api.dll"]