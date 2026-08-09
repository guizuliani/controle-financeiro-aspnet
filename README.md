# 💰 Controle Financeiro - ASP.NET Core MVC (.NET 10)

![.NET 10](https://img.shields.io/badge/.NET-10.0-512BD4?style=for-the-badge&logo=dotnet)
![C#](https://img.shields.io/badge/C%23-239120?style=for-the-badge&logo=c-sharp&logoColor=white)
![ASP.NET Core](https://img.shields.io/badge/ASP.NET%20Core-MVC-512BD4?style=for-the-badge&logo=dotnet)
![SQLite](https://img.shields.io/badge/SQLite-003B57?style=for-the-badge&logo=sqlite&logoColor=white)
![Bootstrap 5](https://img.shields.io/badge/Bootstrap-5.3-7952B3?style=for-the-badge&logo=bootstrap&logoColor=white)

Aplicação Web moderna para **gestão e controle de finanças pessoais/empresariais**, desenvolvida em **ASP.NET Core MVC com .NET 10**.

---

## ✨ Funcionalidades Principais

* **📊 Dashboard Financeiro**:
  * Resumo dinâmico de **Total de Receitas**, **Total de Despesas** e **Saldo Final** do mês.
  * Gráfico de rosca (*doughnut*) interativo via **Chart.js** mapeando os gastos por categoria.
  * Tabela de lançamentos recentes e filtros práticos de **Mês e Ano**.

* **💸 Gestão de Transações**:
  * Registro completo de **Entradas (Receitas)** e **Saídas (Despesas)**.
  * Filtros avançados por busca textual (descrição), mês, ano, tipo e categoria.
  * Operações completas de Criação, Edição e Exclusão (CRUD).

* **🏷️ Gestão de Categorias**:
  * Cadastro de categorias personalizadas com identificação de cores em hexadecimal e ícones (Bootstrap Icons).
  * Validação para prevenção de exclusão acidental de categorias com transações vinculadas.

* **💾 Persistência com Entity Framework Core 10**:
  * Banco de dados local portátil **SQLite** (`controle_financeiro.db`).
  * Inicialização e *seeding* automático das categorias padrão na primeira execução.

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
   Abra o navegador no endereço indicado no terminal (ex: `https://localhost:7050` ou `http://localhost:5050`).

---

## 📂 Estrutura do Repositório

```text
├── Controllers/
│   ├── HomeController.cs        # Dashboard e estatísticas financeiras
│   ├── TransacoesController.cs  # CRUD e filtros de receitas/despesas
│   └── CategoriasController.cs  # CRUD e validações de categorias
├── Models/
│   ├── Categoria.cs             # Modelo de dados de Categorias
│   ├── Transacao.cs             # Modelo de dados de Lançamentos
│   └── ViewModels/              # ViewModels para transporte de dados da Dashboard
├── Data/
│   └── AppDbContext.cs          # Mapeamento do Entity Framework e Seed Data
├── Views/                       # Páginas Razor modernas com Bootstrap 5
├── wwwroot/                     # Ativos estáticos (CSS, JS, Favicon)
├── appsettings.json             # Configuração de conexão do SQLite
└── Program.cs                   # Configuração de serviços e pipeline HTTP
```

---

## 📄 Licença

Este projeto foi desenvolvido como demonstração prática e está livre para uso, estudos e modificações.
