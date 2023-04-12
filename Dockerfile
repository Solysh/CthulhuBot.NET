FROM mcr.microsoft.com/dotnet/runtime:6.0 AS base
WORKDIR /app

# Copy csproj and restore as distinct layers
COPY *.csproj ./
COPY NuGet.Config ./ 
RUN dotnet restore

# Copy everything else and build
COPY . ./
RUN dotnet publish -c Release -o out

# Build runtime image
FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /app
COPY --from=base /app/out .

COPY config.json ./
COPY emotes.json ./
ENTRYPOINT ["dotnet", "CthulhuBot.dll"]