FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["Gateway/FishShop.Gateway/FishShop.Gateway.csproj", "Gateway/FishShop.Gateway/"]
COPY ["netcore/netcore.Commons/netcore.Commons.csproj", "netcore/netcore.Commons/"]
COPY ["netcore/netcore.Entities/netcore.Entities.csproj", "netcore/netcore.Entities/"]
RUN dotnet restore "Gateway/FishShop.Gateway/FishShop.Gateway.csproj"
COPY . .
WORKDIR "/src/Gateway/FishShop.Gateway"
RUN dotnet build "FishShop.Gateway.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "FishShop.Gateway.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "FishShop.Gateway.dll"]
