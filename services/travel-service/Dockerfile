FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ./src/*.csproj ./
RUN dotnet restore
COPY ./src/. ./
RUN dotnet publish -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:8.0-alpine AS runtime
WORKDIR /app

# QuestPDF/Skia needs fontconfig + at least one decent TTF family to render
# PDFs that don't look "default Linux serif" amateurish. DejaVu Sans + Sans
# Mono ship the Latin glyph coverage we need; Liberation Sans gives us a
# Helvetica-alike for the editorial display tone.
RUN apk add --no-cache fontconfig ttf-dejavu ttf-liberation && fc-cache -f

COPY --from=build /app/publish ./
EXPOSE 8080
HEALTHCHECK --interval=15s --timeout=5s --retries=5 CMD wget --spider -q http://localhost:8080/health || exit 1
ENTRYPOINT ["dotnet", "TravelService.dll"]
