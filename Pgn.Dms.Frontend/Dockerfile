FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY Pgn.Dms.Shared/Pgn.Dms.Shared.csproj Pgn.Dms.Shared/
COPY Pgn.Dms.Frontend/Pgn.Dms.Frontend.csproj Pgn.Dms.Frontend/
RUN dotnet restore Pgn.Dms.Frontend/Pgn.Dms.Frontend.csproj

COPY Pgn.Dms.Shared/ Pgn.Dms.Shared/
COPY Pgn.Dms.Frontend/ Pgn.Dms.Frontend/
RUN dotnet publish Pgn.Dms.Frontend/Pgn.Dms.Frontend.csproj -c Release -o /app --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
COPY --from=build /app .
ENTRYPOINT ["dotnet", "Pgn.Dms.Web.dll"]
