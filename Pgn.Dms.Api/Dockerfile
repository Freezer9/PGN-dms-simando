FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY Pgn.Dms.Shared/Pgn.Dms.Shared.csproj Pgn.Dms.Shared/
COPY Pgn.Dms.Api/Pgn.Dms.Api.csproj Pgn.Dms.Api/
RUN dotnet restore Pgn.Dms.Api/Pgn.Dms.Api.csproj

COPY Pgn.Dms.Shared/ Pgn.Dms.Shared/
COPY Pgn.Dms.Api/ Pgn.Dms.Api/
RUN dotnet publish Pgn.Dms.Api/Pgn.Dms.Api.csproj -c Release -o /app --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
COPY --from=build /app .
ENTRYPOINT ["dotnet", "Pgn.Dms.Api.dll"]
