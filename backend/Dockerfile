# Build
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY global.json Directory.Build.props Directory.Packages.props ./
COPY src/Portfolio.Domain/Portfolio.Domain.csproj src/Portfolio.Domain/
COPY src/Portfolio.Application/Portfolio.Application.csproj src/Portfolio.Application/
COPY src/Portfolio.Infrastructure/Portfolio.Infrastructure.csproj src/Portfolio.Infrastructure/
COPY src/Portfolio.Api/Portfolio.Api.csproj src/Portfolio.Api/
RUN dotnet restore src/Portfolio.Api/Portfolio.Api.csproj -r linux-x64
COPY src/ src/
RUN dotnet publish src/Portfolio.Api/Portfolio.Api.csproj -c Release -r linux-x64 --no-restore --self-contained false -o /app

# Runtime (chiseled: sem shell, sem root, imagem mínima)
FROM mcr.microsoft.com/dotnet/aspnet:10.0-noble-chiseled
WORKDIR /app
COPY --from=build /app .
ENV ASPNETCORE_HTTP_PORTS=8080
EXPOSE 8080
ENTRYPOINT ["dotnet", "Portfolio.Api.dll"]
