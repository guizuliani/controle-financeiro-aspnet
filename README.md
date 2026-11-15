# 💰 Controle Financeiro - ASP.NET Core MVC (.NET 10)

![.NET 10](https://img.shields.io/badge/.NET-10.0-512BD4?style=for-the-badge&logo=dotnet)
![C#](https://img.shields.io/badge/C%23-239120?style=for-the-badge&logo=c-sharp&logoColor=white)
![ASP.NET Core](https://img.shields.io/badge/ASP.NET%20Core-MVC-512BD4?style=for-the-badge&logo=dotnet)
![SQLite](https://img.shields.io/badge/SQLite-003B57?style=for-the-badge&logo=sqlite&logoColor=white)
![Bootstrap 5](https://img.shields.io/badge/Bootstrap-5.3-7952B3?style=for-the-badge&logo=bootstrap&logoColor=white)

Aplicação Web moderna para **gestão financeira pessoal/empresarial**, desenvolvida em **ASP.NET Core MVC com .NET 10**, agora com suporte completo a **Cartões de Crédito**, **Controle de Faturas**, **Compras Parceladas** e **Sistema Configurável de Pontos e Milhas**.

---

## ✨ Funcionalidades Principais

* **📊 Dashboard Financeiro Unificado**:
  * Visão consolidada de **Receitas em Conta**, **Despesas em Conta**, **Saldo em Conta**, **Faturas do Mês** e **Pontos/Milhas Acumulados**.
  * Barras de progresso com **Uso de Limite** de cada cartão cadastrado.
  * Gráfico interativo via **Chart.js** mapeando os gastos por categoria.
  * Tabela de lançamentos recentes e filtros práticos de **Mês e Ano**.

* **💳 Gestão de Cartões de Crédito & Faturas**:
  * Cadastro de cartões com bandeira, cor visual, limite total, **dia de fechamento** e **dia de vencimento**.
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
  * Cadastro de categorias personalizadas com identificação de cores em hexadecimal e ícones (Bootstrap Icons).
  * Validação que impede exclusão acidental de categorias com transações vinculadas.

* **💾 Persistência com Entity Framework Core 10**:
  * Banco de dados local portátil **SQLite** (`controle_financeiro.db`).
  * Inicialização e *seeding* automático de categorias e cartões exemplo na primeira execução.

---

## 💻 Tecnologias Utilizadas

* **Framework**: .NET 10.0 (ASP.NET Core MVC)
* **ORM**: Entity Framework Core 10 (SQLite Provider)
* **Front-end & UI**:
  * HTML5 / CSS3 (Design responsivo e limpo)
  * Bootstrap 5.3 + Bootstrap Icons
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

## 📂 Estrutura do Repositório

```text
├── Controllers/
│   ├── HomeController.cs        # Dashboard unificado e resumo de cartões/pontos
│   ├── CartoesController.cs     # CRUD de cartões, faturas e liquidação
│   ├── PontosController.cs      # Extrato de pontos, milhas e resgates
│   ├── TransacoesController.cs  # CRUD, parcelamento e cálculo de pontuação
│   └── CategoriasController.cs  # CRUD e validações de categorias
├── Models/
│   ├── CartaoCredito.cs         # Modelo de cartão com regras de pontuação
│   ├── ResgatePontos.cs         # Histórico de transferências/resgates de pontos
│   ├── Transacao.cs             # Transações com suporte a cartão, parcelas e pontos
│   ├── Categoria.cs             # Categorias de gastos/receitas
│   ├── Enums/                   # TipoTransacao, FormaPagamento, TipoRegraPontos
│   └── ViewModels/              # DashboardViewModel, FaturaViewModel, PontosViewModel
├── Data/
│   └── AppDbContext.cs          # Mapeamento do EF Core e Seed Data
├── Views/                       # Views Razor modernas (Cartões, Pontos, Transações)
├── wwwroot/                     # Ativos estáticos (CSS, JS, Favicon)
├── appsettings.json             # Configuração de conexão do SQLite
└── Program.cs                   # Inicialização de serviços e pipeline HTTP
```

---

## 📄 Licença

Este projeto está livre para uso, estudos e modificações.
