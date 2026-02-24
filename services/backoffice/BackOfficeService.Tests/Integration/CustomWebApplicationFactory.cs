using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;

/// <summary>
/// Factory qui démarre l'application en mémoire pour les tests d'intégration.
///
/// Pourquoi UseEnvironment("Testing") suffit :
///   - Program.cs charge automatiquement appsettings.Testing.json → JWT config correcte
///   - Program.cs utilise EF Core InMemory (au lieu de Npgsql) pour l'environnement Testing
///   - Le nom de DB InMemory est unique par instance de factory (généré dans Program.cs)
///
/// Chaque IClassFixture obtient sa propre factory → sa propre DB InMemory → isolation parfaite.
/// </summary>
public class CustomWebApplicationFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        // Active le mode Testing dans Program.cs :
        //   → charge appsettings.Testing.json (JWT test config)
        //   → enregistre InMemory au lieu de Npgsql
        builder.UseEnvironment("Testing");
    }
}
