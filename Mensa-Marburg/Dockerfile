FROM mcr.microsoft.com/dotnet/runtime:6.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["Mensa-Marburg/Mensa-Marburg.csproj", "Mensa-Marburg/"]
RUN dotnet restore "Mensa-Marburg/Mensa-Marburg.csproj"
COPY . .
WORKDIR "/src/Mensa-Marburg"
RUN dotnet build "Mensa-Marburg.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Mensa-Marburg.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Mensa-Marburg.dll"]
