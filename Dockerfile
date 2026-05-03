FROM mcr.microsoft.com/dotnet/sdk:10.0-preview AS build
WORKDIR /src

# Copy project files and restore
COPY ["Backend/Api/Api.csproj", "Backend/Api/"]
COPY ["Backend/Application/Application.csproj", "Backend/Application/"]
COPY ["Backend/Domain/Domain.csproj", "Backend/Domain/"]
COPY ["Backend/Infrastructure/Infrastructure.csproj", "Backend/Infrastructure/"]

RUN dotnet restore "Backend/Api/Api.csproj"

# Copy the rest of the source code
COPY . .

# Build the application
WORKDIR "/src/Backend/Api"
RUN dotnet build "Api.csproj" -c Release -o /app/build

# Publish the application
FROM build AS publish
RUN dotnet publish "Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Run the application
FROM mcr.microsoft.com/dotnet/aspnet:10.0-preview AS final
WORKDIR /Backend
COPY --from=publish /app/publish .

# Copy Frontend files to wwwroot so the API can serve them
COPY ./Frontend ./wwwroot

ENTRYPOINT ["dotnet", "Api.dll"]
