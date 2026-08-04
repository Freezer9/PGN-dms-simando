FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY Shared/Shared.csproj Shared/
COPY Api/Api.csproj Api/
RUN dotnet restore Api/Api.csproj

COPY Shared/ Shared/
COPY Api/ Api/
RUN dotnet publish Api/Api.csproj -c Release -o /app --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
COPY --from=build /app .
ENTRYPOINT ["dotnet", "Pgn.Dms.Api.dll"]
