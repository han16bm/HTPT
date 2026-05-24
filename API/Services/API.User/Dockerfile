FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["Services/API.User/API.User.csproj", "Services/API.User/"]
COPY ["netcore/netcore.Commons/netcore.Commons.csproj", "netcore/netcore.Commons/"]
COPY ["netcore/netcore.Entities/netcore.Entities.csproj", "netcore/netcore.Entities/"]
RUN dotnet restore "Services/API.User/API.User.csproj"
COPY . .
WORKDIR "/src/Services/API.User"
RUN dotnet build "API.User.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "API.User.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "API.User.dll"]
