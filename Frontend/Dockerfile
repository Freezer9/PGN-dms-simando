FROM node:22-slim AS css-build
WORKDIR /src/Frontend
COPY Frontend/package.json Frontend/package-lock.json ./
RUN npm ci
COPY Frontend/Styles Styles
COPY Frontend/Components Components
RUN npm run build:css

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY Shared/Shared.csproj Shared/
COPY Frontend/Frontend.csproj Frontend/
RUN dotnet restore Frontend/Frontend.csproj

COPY Shared/ Shared/
COPY Frontend/ Frontend/
COPY --from=css-build /src/Frontend/wwwroot/css/app.generated.css Frontend/wwwroot/css/app.generated.css
RUN dotnet publish Frontend/Frontend.csproj -c Release -o /app

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
COPY --from=build /app .
ENTRYPOINT ["dotnet", "Pgn.Dms.Web.dll"]
