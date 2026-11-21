# 💰 Controle Financeiro - ASP.NET Core MVC (.NET 10)

![.NET 10](https://img.shields.io/badge/.NET-10.0-512BD4?style=for-the-badge&logo=dotnet)
![C#](https://img.shields.io/badge/C%23-239120?style=for-the-badge&logo=c-sharp&logoColor=white)
![ASP.NET Core](https://img.shields.io/badge/ASP.NET%20Core-MVC-512BD4?style=for-the-badge&logo=dotnet)
![Hexagonal Architecture](https://img.shields.io/badge/Architecture-Hexagonal-0F172A?style=for-the-badge)
![Tailwind CSS](https://img.shields.io/badge/Tailwind_CSS-38B2AC?style=for-the-badge&logo=tailwind-css&logoColor=white)
![Flowbite](https://img.shields.io/badge/Flowbite-1A56DB?style=for-the-badge&logo=flowbite&logoColor=white)
![Docker](https://img.shields.io/badge/Docker-2496ED?style=for-the-badge&logo=docker&logoColor=white)
![SQLite](https://img.shields.io/badge/SQLite-003B57?style=for-the-badge&logo=sqlite&logoColor=white)

Aplicação Web moderna para **gestão financeira pessoal/empresarial**, desenvolvida em **ASP.NET Core MVC com .NET 10** sobre **Arquitetura Hexagonal (Ports & Adapters)**, com front-end compilado em **Tailwind CSS + Flowbite**, suporte a **Múltiplas Contas**, **Cartões de Crédito & Faturas**, **Orçamentos/Metas**, **Lançamentos Recorrentes**, **Pontos & Milhas**, **Importação CSV/OFX** e **Autenticação Multi-usuário**.

---

## ✨ Funcionalidades Principais

* **🔐 Autenticação & Multi-usuário**:
  * Cadastro de usuários e login com **Cookie Authentication** e hashing seguro **PBKDF2-SHA256**.
  * Isolamento completo de contas, cartões, categorias, metas e transações por usuário (`UsuarioId`).
  * Usuário inicial padrão: `admin@financecontrol.local` / `Admin@123`.

* **📊 Dashboard Financeiro Unificado (Visual Fintech)**:
  * Visão consolidada de **Receitas em Conta**, **Despesas em Conta**, **Saldo em Conta**, **Faturas do Mês** e **Pontos/Milhas Acumulados**.
  * Resumo de **Contas & Carteiras** e **Metas do Mês** com alertas visuais (Normal, Atenção ≥ 80%, Excedido ≥ 100%).
  * Barras de progresso com **Uso de Limite** de cada cartão cadastrado.
  * Gráfico interativo via **Chart.js** mapeando os gastos por categoria e filtros de **Mês e Ano**.

* **🏦 Múltiplas Contas Bancárias & Carteiras**:
  * Cadastro de contas **Corrente**, **Poupança** e **Investimento** com saldo inicial e cálculo automático do saldo atual.

* **🎯 Orçamentos & Metas por Categoria**:
  * Definição de tetos mensais de gastos por categoria com acompanhamento percentual em tempo real.

* **🔁 Lançamentos Recorrentes**:
  * Cadastro de receitas e despesas fixas mensais (salário, aluguel, assinaturas) com geração em 1 clique para o mês de competência.

* **📥 Importação (CSV / OFX) & Exportação (CSV)**:
  * Importação de extratos bancários e faturas em **CSV** ou **OFX** diretamente para uma conta ou cartão.
  * Exportação do extrato em **CSV** (UTF-8 com BOM para abertura direta no Excel).

* **💳 Gestão de Cartões de Crédito & Faturas**:
  * Cadastro de cartões com bandeira, cor visual, limite total, **dia de fechamento** e **dia de vencimento**.
  * Cálculo dinâmico do "Melhor dia de compra" (virada de fatura automática conforme o dia de fechamento).
  * Visualização detalhada da **Fatura do Mês** e **Pagar Fatura** (quita os lançamentos e gera a despesa consolidada na conta).

* **⭐ Sistema Configurável de Pontos e Milhas**:
  * Regras **Por Dólar Gasto (USD)**, **Por Real Gasto (BRL)** ou **Cashback (%)**, com cálculo automático por parcela e registro de **Resgate/Transferência**.

* **💾 Persistência com EF Core Migrations & Observabilidade**:
  * Banco **SQLite** gerenciado com **EF Core Migrations** (`Database.Migrate()`), `App_Data/controle_financeiro.db` localmente e `/app/data/controle_financeiro.db` em container.
  * Endpoint **`/health`** validando aplicação e banco SQLite + `HEALTHCHECK` configurado no Docker.
  * Chaves do `DataProtection` persistidas em disco para manter sessões válidas entre reinicializações.

---

## 💻 Tecnologias & Qualidade

* **Backend**: .NET 10.0, ASP.NET Core MVC, Arquitetura Hexagonal (`Domain`, `Application`, `Infrastructure`, `Web`)
* **ORM**: Entity Framework Core 10 (SQLite + Migrations)
* **Front-end**: Tailwind CSS compilado e minificado (`wwwroot/css/app.css`), Flowbite, Bootstrap Icons, Chart.js, Fonte Inter
* **Testes Automatizados (`xUnit` + `NetArchTest.Rules`)**:
  * **26 testes** cobrindo Regras de Domínio, Casos de Uso (`Application`), Integração e **Testes de Arquitetura** garantindo o isolamento das camadas hexagonais.
* **CI/CD (GitHub Actions)**:
  * Restore, auditoria de vulnerabilidades (`dotnet list package --vulnerable --include-transitive`), build `Release`, testes com coleta de cobertura (`coverlet`), validação do `Dockerfile` e publicação automática da imagem Docker no **GitHub Container Registry (`ghcr.io`)**.

---

## 🚀 Como Executar o Projeto Localmente

### Pré-requisitos
* [.NET 10.0 SDK](https://dotnet.microsoft.com/download/dotnet/10.0) instalado.

### Passo a passo
1. **Clonar o repositório**:
   ```bash
   git clone https://github.com/guizuliani/controle-financeiro-aspnet.git
   cd controle-financeiro-aspnet
   ```

2. **Executar a aplicação**:
   ```bash
   dotnet run --project src/ControleFinanceiro.Web
   ```

3. **Executar os testes**:
   ```bash
   dotnet test ControleFinanceiro.slnx
   ```

4. **Acessar no navegador**:
   * HTTPS: `https://localhost:7050`
   * HTTP: `http://localhost:5050`
   * Health Check: `http://localhost:5050/health`

---

## 🐳 Executando com Docker e Docker Compose

A aplicação conta com `Dockerfile` multi-stage otimizado (usuário não-root `app`, `HEALTHCHECK`) e `docker-compose.yml` com persistência em volume nomeado (`sqlite-data:/app/data`).

### Via Docker Compose (Recomendado)
```bash
# Construir e iniciar
docker compose up -d --build

# Parar
docker compose down
```

Acesse em: **[http://localhost:8080](http://localhost:8080)**  
Health Check: **[http://localhost:8080/health](http://localhost:8080/health)**

### Via Docker CLI Direto
```bash
docker build -t controle-financeiro:latest .
docker run -d --name controle-financeiro-app -p 8080:8080 -v sqlite-data:/app/data controle-financeiro:latest
```

---

## 📂 Estrutura do Repositório (Arquitetura Hexagonal)

```text
├── .github/workflows/ci.yml                 # Pipeline CI/CD (Build, Testes, Cobertura, Auditoria, Docker & GHCR)
├── src/
│   ├── ControleFinanceiro.Domain/           # Núcleo puro: Entidades ricas, Enums e Driven Ports (Interfaces)
│   ├── ControleFinanceiro.Application/      # Casos de Uso, DTOs e Driving Ports (Serviços)
│   ├── ControleFinanceiro.Infrastructure/   # Driven Adapters: EF Core (AppDbContext, Migrations) e Repositórios SQLite
│   └── ControleFinanceiro.Web/              # Driving Adapter: ASP.NET Core MVC (Controllers, Views, HealthChecks, wwwroot)
├── tests/ControleFinanceiro.Tests/          # Testes de Arquitetura (NetArchTest), Domínio, Aplicação e Integração (xUnit)
├── Dockerfile & docker-compose.yml          # Conteinerização multi-stage com persistência e HEALTHCHECK
└── ControleFinanceiro.slnx                  # Solution unificada
```

---

## 📄 Licença

Este projeto está livre para uso, estudos e modificações.
