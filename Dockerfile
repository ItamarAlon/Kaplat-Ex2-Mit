FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
WORKDIR /app
EXPOSE 4785

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["Kaplat_Ex4_Docker/Kaplat_Ex4_Docker.csproj", "Kaplat_Ex4_Docker/"]
RUN dotnet restore "./Kaplat_Ex4_Docker/Kaplat_Ex4_Docker.csproj"
COPY . .
WORKDIR "/src/Kaplat_Ex4_Docker"
RUN dotnet build "./Kaplat_Ex4_Docker.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./Kaplat_Ex4_Docker.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Kaplat_Ex4_Docker.dll"]
