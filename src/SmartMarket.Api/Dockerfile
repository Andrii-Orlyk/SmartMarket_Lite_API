FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY SmartMarket.sln ./
COPY src/SmartMarket.Domain/SmartMarket.Domain.csproj src/SmartMarket.Domain/
COPY src/SmartMarket.Application/SmartMarket.Application.csproj src/SmartMarket.Application/
COPY src/SmartMarket.Infrastructure/SmartMarket.Infrastructure.csproj src/SmartMarket.Infrastructure/
COPY src/SmartMarket.Api/SmartMarket.Api.csproj src/SmartMarket.Api/

RUN dotnet restore src/SmartMarket.Api/SmartMarket.Api.csproj

COPY src/ src/
RUN dotnet publish src/SmartMarket.Api/SmartMarket.Api.csproj -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=build /app/publish .
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080
ENTRYPOINT ["dotnet", "SmartMarket.Api.dll"]
