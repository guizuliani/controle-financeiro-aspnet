using ControleFinanceiro.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container (Driving & Driven Adapters).
builder.Services.AddControllersWithViews();
builder.Services.AddHexagonalArchitecture(builder.Configuration.GetConnectionString("DefaultConnection"));

var app = builder.Build();

// Garantir a criação do banco de dados e dados iniciais
DependencyInjection.GarantirBancoDadosCriado(app.Services);

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
