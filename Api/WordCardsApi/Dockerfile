FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY . .
RUN dotnet publish "WordCardsApi.csproj" -c Release -o /api/publish /p:UseAppHost=false


FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /api
COPY --from=build /api/publish .
ENTRYPOINT [ "dotnet", "WordCardsApi.dll" ]