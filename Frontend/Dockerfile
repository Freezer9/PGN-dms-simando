FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY Shared/Shared.csproj Shared/
COPY Frontend/Frontend.csproj Frontend/
RUN dotnet restore Frontend/Frontend.csproj

COPY Shared/ Shared/
COPY Frontend/ Frontend/
RUN dotnet publish Frontend/Frontend.csproj -c Release -o /app --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
COPY --from=build /app .
ENTRYPOINT ["dotnet", "Pgn.Dms.Web.dll"]
