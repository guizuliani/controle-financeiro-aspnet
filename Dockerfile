# Estágio Base (Runtime do ASP.NET Core .NET 10)
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app
EXPOSE 8080
ENV ASPNETCORE_HTTP_PORTS=8080
ENV ConnectionStrings__DefaultConnection="Data Source=/app/data/controle_financeiro.db"

# Criação da pasta para o SQLite com permissões para o usuário padrão não-root (app: UID 1654)
RUN mkdir -p /app/data && chown -R app:app /app/data

# Estágio de Compilação (SDK do .NET 10)
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copia os csprojs e restaura as dependências aproveitando o cache de camadas do Docker
COPY ["src/ControleFinanceiro.Web/ControleFinanceiro.Web.csproj", "src/ControleFinanceiro.Web/"]
COPY ["src/ControleFinanceiro.Domain/ControleFinanceiro.Domain.csproj", "src/ControleFinanceiro.Domain/"]
COPY ["src/ControleFinanceiro.Application/ControleFinanceiro.Application.csproj", "src/ControleFinanceiro.Application/"]
COPY ["src/ControleFinanceiro.Infrastructure/ControleFinanceiro.Infrastructure.csproj", "src/ControleFinanceiro.Infrastructure/"]
RUN dotnet restore "src/ControleFinanceiro.Web/ControleFinanceiro.Web.csproj"

# Copia o restante dos arquivos do projeto
COPY . .
RUN dotnet build "src/ControleFinanceiro.Web/ControleFinanceiro.Web.csproj" -c Release -o /app/build --no-restore

# Estágio de Publicação da aplicação
FROM build AS publish
RUN dotnet publish "src/ControleFinanceiro.Web/ControleFinanceiro.Web.csproj" -c Release -o /app/publish /p:UseAppHost=false --no-restore

# Estágio Final (Runtime Enxuto)
FROM base AS final
WORKDIR /app
COPY --chown=app:app --from=publish /app/publish .

# Executa com usuário não-privilegiado
USER app

HEALTHCHECK --interval=30s --timeout=5s --start-period=10s --retries=3 \
  CMD bash -c "exec 3<>/dev/tcp/127.0.0.1/8080" || exit 1

ENTRYPOINT ["dotnet", "ControleFinanceiro.dll"]
