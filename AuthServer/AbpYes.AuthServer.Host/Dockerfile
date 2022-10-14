FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["AuthServer/AbpYes.AuthServer.Host/AbpYes.AuthServer.Host.csproj", "AbpYes.AuthServer.Host/"]
RUN dotnet restore "AuthServer/AbpYes.AuthServer.Host/AbpYes.AuthServer.Host.csproj"
COPY . .
WORKDIR "/src/AbpYes.AuthServer.Host"
RUN dotnet build "AbpYes.AuthServer.Host.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "AbpYes.AuthServer.Host.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "AbpYes.AuthServer.Host.dll"]
