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

# Copia o csproj e restaura as dependências aproveitando o cache de camadas do Docker
COPY ["ControleFinanceiro.csproj", "./"]
RUN dotnet restore "ControleFinanceiro.csproj"

# Copia o restante dos arquivos do projeto
COPY . .
RUN dotnet build "ControleFinanceiro.csproj" -c Release -o /app/build --no-restore

# Estágio de Publicação da aplicação
FROM build AS publish
RUN dotnet publish "ControleFinanceiro.csproj" -c Release -o /app/publish /p:UseAppHost=false --no-restore

# Estágio Final (Runtime Enxuto)
FROM base AS final
WORKDIR /app
COPY --chown=app:app --from=publish /app/publish .

# Executa com usuário não-privilegiado
USER app

ENTRYPOINT ["dotnet", "ControleFinanceiro.dll"]
