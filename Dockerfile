FROM mcr.microsoft.com/dotnet/sdk:10.0.401 AS build

WORKDIR /source

COPY . .

RUN dotnet restore src/WalletTransfer.Api/WalletTransfer.Api.csproj

RUN dotnet publish src/WalletTransfer.Api/WalletTransfer.Api.csproj \
    --configuration Release \
    --no-restore \
    --output /app/publish \
    /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:10.0.12 AS final

WORKDIR /app

ENV ASPNETCORE_URLS=http://+:8080

EXPOSE 8080

COPY --from=build /app/publish .

USER $APP_UID

ENTRYPOINT ["dotnet", "WalletTransfer.Api.dll"]