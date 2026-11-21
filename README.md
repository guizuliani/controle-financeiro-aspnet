# 💰 Controle Financeiro - ASP.NET Core MVC (.NET 10)

![.NET 10](https://img.shields.io/badge/.NET-10.0-512BD4?style=for-the-badge&logo=dotnet)
![C#](https://img.shields.io/badge/C%23-239120?style=for-the-badge&logo=c-sharp&logoColor=white)
![ASP.NET Core](https://img.shields.io/badge/ASP.NET%20Core-MVC-512BD4?style=for-the-badge&logo=dotnet)
![Tailwind CSS](https://img.shields.io/badge/Tailwind_CSS-38B2AC?style=for-the-badge&logo=tailwind-css&logoColor=white)
![Flowbite](https://img.shields.io/badge/Flowbite-1A56DB?style=for-the-badge&logo=flowbite&logoColor=white)
![Docker](https://img.shields.io/badge/Docker-2496ED?style=for-the-badge&logo=docker&logoColor=white)
![SQLite](https://img.shields.io/badge/SQLite-003B57?style=for-the-badge&logo=sqlite&logoColor=white)

Aplicação Web moderna para **gestão financeira pessoal/empresarial**, desenvolvida em **ASP.NET Core MVC com .NET 10**, com front-end moderno em **Tailwind CSS + Flowbite**, suporte a **Cartões de Crédito**, **Controle de Faturas**, **Compras Parceladas** e **Sistema Configurável de Pontos e Milhas**.

---

## ✨ Funcionalidades Principais

* **📊 Dashboard Financeiro Unificado (Visual Fintech)**:
  * Visão consolidada de **Receitas em Conta**, **Despesas em Conta**, **Saldo em Conta**, **Faturas do Mês** e **Pontos/Milhas Acumulados**.
  * Barras de progresso elegantes com **Uso de Limite** de cada cartão cadastrado.
  * Gráfico interativo via **Chart.js** mapeando os gastos por categoria.
  * Tabela de lançamentos recentes e filtros práticos de **Mês e Ano**.

* **💳 Gestão de Cartões de Crédito & Faturas**:
  * Cadastro de cartões com bandeira, cor visual, limite total, **dia de fechamento** e **dia de vencimento**.
  * Cartões físicos virtuais com design moderno em gradiente.
  * Cálculo dinâmico do "Melhor dia de compra" (virada de fatura automática conforme o dia de fechamento).
  * Visualização detalhada da **Fatura do Mês** (compras, parcelas 1/N, total da fatura e total de pontos gerados).
  * Botão de **Pagar Fatura**: quita os lançamentos da fatura e gera a despesa consolidada no extrato da sua conta bancária.

* **⭐ Sistema Configurável de Pontos e Milhas**:
  * Definição de regras de pontuação por cartão:
    * **Por Dólar Gasto (USD)** (ex: *2.5 pontos por US$ 1*, com cotação de referência).
    * **Por Real Gasto (BRL)** (ex: *1 ponto por R$ 1 gasto*).
    * **Cashback (%)** (ex: *1% ou 1.5% de volta*).
  * **Cálculo automático de pontos** gerados em cada compra ou parcela.
  * **Painel de Pontos**: Saldo líquido por programa (*Livelo, Esfera, C6 Átomos, Smiles, etc.*), extrato de acúmulo e registro de **Resgate/Transferência** de pontos.

* **💸 Gestão de Transações & Parcelamento**:
  * Registro de transações em **Conta Corrente** ou **Cartão de Crédito**.
  * Suporte a compras parceladas (1x até 48x) com alocação automática nas faturas dos meses subsequentes.
  * Filtros avançados por busca textual, mês, ano, tipo, forma de pagamento e cartão.

* **🏷️ Gestão de Categorias**:
  * Cadastro de categorias personalizadas com identificação de cores em hexadecimal e ícones.
  * Validação que impede exclusão acidental de categorias com transações vinculadas.

* **💾 Persistência com Entity Framework Core 10**:
  * Banco de dados local portátil **SQLite** (`controle_financeiro.db`).
  * Pacote `SQLitePCLRaw.bundle_e_sqlite3` `2.1.14` auditado sem vulnerabilidades.
  * Inicialização e *seeding* automático de categorias e cartões exemplo na primeira execução.

---

## 💻 Tecnologias Utilizadas

* **Framework**: .NET 10.0 (ASP.NET Core MVC)
* **ORM**: Entity Framework Core 10 (SQLite Provider)
* **Front-end & UI**:
  * HTML5 / CSS3 Moderno
  * **Tailwind CSS** (Utility-first framework)
  * **Flowbite** (Componentes de UI: Modais, Tabelas, Alertas, Formulários)
  * Google Fonts (Inter)
  * Chart.js (Gráficos estatísticos)
* **Controle de Versão**: Git / GitHub

---

## 🚀 Como Executar o Projeto Localmente

### Pré-requisitos

* [.NET 10.0 SDK](https://dotnet.microsoft.com/download/dotnet/10.0) instalado no seu computador.

### Passo a passo

1. **Clonar o repositório**:
   ```bash
   git clone https://github.com/guizuliani/controle-financeiro-aspnet.git
   cd controle-financeiro-aspnet
   ```

2. **Executar a aplicação**:
   ```bash
   dotnet run
   ```

3. **Acessar no navegador**:
   * HTTPS: `https://localhost:7050`
   * HTTP: `http://localhost:5050`

---

## 🐳 Executando com Docker e Docker Compose

A aplicação já conta com `Dockerfile` multi-stage otimizado para o **.NET 10** e `docker-compose.yml` com persistência de dados do SQLite através de volumes.

### Opção 1: Via Docker Compose (Recomendado)

Inicie a aplicação e o volume de dados com um único comando:

```bash
docker compose up -d
```

Acesse em: **[http://localhost:8080](http://localhost:8080)**

Para parar os containers:
```bash
docker compose down
```

### Opção 2: Via Docker CLI Direto

1. **Construir a imagem**:
   ```bash
   docker build -t controle-financeiro:latest .
   ```

2. **Executar o container com persistência de dados**:
   ```bash
   docker run -d --name controle-financeiro-app -p 8080:8080 -v sqlite-data:/app/data controle-financeiro:latest
   ```

---

## 📂 Estrutura do Repositório (Arquitetura Hexagonal)

```text
├── src/
│   ├── ControleFinanceiro.Domain/           # Núcleo puro: Entidades ricas, Enums e Driven Ports (Interfaces)
│   ├── ControleFinanceiro.Application/      # Casos de Uso, DTOs e Driving Ports (Serviços)
│   └── ControleFinanceiro.Infrastructure/   # Driven Adapters: EF Core (AppDbContext) e Repositórios SQLite
├── Controllers/                             # Driving Adapters (ASP.NET Core MVC)
│   ├── HomeController.cs
│   ├── CartoesController.cs
│   ├── PontosController.cs
│   ├── TransacoesController.cs
│   └── CategoriasController.cs
├── Models/                                  # ViewModels Web
├── Views/                                   # Views Razor modernas com Tailwind CSS + Flowbite
├── wwwroot/                                 # Ativos estáticos (CSS, JS, Favicon)
├── tests/ControleFinanceiro.Tests/          # Suíte de testes unitários (Domínio) e integração (xUnit)
├── appsettings.json                         # Configuração de conexão do SQLite
├── Dockerfile & docker-compose.yml          # Conteinerização multi-stage com persistência
└── Program.cs                               # Composition Root (Injeção de Dependência)
```

---

## 📄 Licença

Este projeto está livre para uso, estudos e modificações.
