FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["AbpYes.BaseServer.HttpApi.Host/AbpYes.BaseServer.HttpApi.Host.csproj", "AbpYes.BaseServer.HttpApi.Host/"]
RUN dotnet restore "AbpYes.BaseServer.HttpApi.Host/AbpYes.BaseServer.HttpApi.Host.csproj"
COPY . .
WORKDIR "/src/AbpYes.BaseServer.HttpApi.Host"
RUN dotnet build "AbpYes.BaseServer.HttpApi.Host.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "AbpYes.BaseServer.HttpApi.Host.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "AbpYes.BaseServer.HttpApi.Host.dll"]
