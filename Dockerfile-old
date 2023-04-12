FROM mcr.microsoft.com/dotnet/runtime:6.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["CthulhuBot/CthulhuBot.csproj", "CthulhuBot/"]
RUN dotnet restore "CthulhuBot/CthulhuBot.csproj"
COPY . .
WORKDIR "/src/CthulhuBot"
RUN dotnet build "CthulhuBot.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "CthulhuBot.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "CthulhuBot.dll"]
