FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ./src/*.csproj ./
RUN dotnet restore
COPY ./src/. ./
RUN dotnet publish -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:8.0-alpine AS runtime
RUN addgroup -S app && adduser -S app -G app
WORKDIR /app
COPY --from=build /app/publish ./
RUN chown -R app:app /app
USER app
EXPOSE 8080
HEALTHCHECK --interval=15s --timeout=5s --retries=5 CMD wget --spider -q http://localhost:8080/health || exit 1
ENTRYPOINT ["dotnet", "ApiGateway.dll"]
