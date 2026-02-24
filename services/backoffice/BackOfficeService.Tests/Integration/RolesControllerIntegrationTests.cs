using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using server.Data;
using server.DTOs.Role;
using server.Models;

/// <summary>
/// Tests d'intégration pour RolesController.
/// Chaque test est indépendant : il génère ses propres données avec un nom unique (Guid).
///
/// Stack testée : HTTP → Controller → Service → Repository → EF Core InMemory
/// Auth : JWT généré par JwtTestHelper avec les mêmes paramètres que la factory.
/// </summary>
public class RolesControllerIntegrationTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly CustomWebApplicationFactory _factory;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public RolesControllerIntegrationTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
    }

    // ── Helpers ──────────────────────────────────────────────────────────────

    /// <summary>Crée un HttpClient avec un token Admin dans le header Authorization.</summary>
    private HttpClient CreateAdminClient()
    {
        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", JwtTestHelper.GenerateToken(role: "Admin"));
        return client;
    }

    /// <summary>Crée un HttpClient avec un token d'un rôle non-Admin.</summary>
    private HttpClient CreateOperatorClient()
    {
        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", JwtTestHelper.GenerateToken(role: "Operator"));
        return client;
    }

    /// <summary>
    /// Insère directement un rôle en base via le DbContext (sans passer par l'API).
    /// Utile pour préparer l'état initial d'un test.
    /// </summary>
    private async Task<Role> SeedRoleAsync(string name)
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        var role = new Role { Name = name };
        db.Roles.Add(role);
        await db.SaveChangesAsync();
        return role;
    }

    // ── Tests d'authentification ──────────────────────────────────────────────

    [Fact]
    public async Task GetRoles_SansToken_Retourne401()
    {
        var client = _factory.CreateClient();

        var response = await client.GetAsync("/api/v1/roles");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetRoles_AvecTokenNonAdmin_Retourne403()
    {
        var client = CreateOperatorClient();

        var response = await client.GetAsync("/api/v1/roles");

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task GetRoles_AvecTokenAdmin_Retourne200()
    {
        var client = CreateAdminClient();

        var response = await client.GetAsync("/api/v1/roles");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    // ── GET /api/v1/roles ─────────────────────────────────────────────────────

    [Fact]
    public async Task GetRoles_QuandDeuxRolesExistent_RetourneListe()
    {
        // Arrange : insérer directement en base sans passer par l'API
        var suffix = Guid.NewGuid().ToString("N")[..8];
        await SeedRoleAsync($"Admin_{suffix}");
        await SeedRoleAsync($"Operator_{suffix}");

        var client = CreateAdminClient();

        // Act
        var response = await client.GetAsync("/api/v1/roles");
        var body = await response.Content.ReadAsStringAsync();
        var roles = JsonSerializer.Deserialize<List<RoleDto>>(body, JsonOptions);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        // La DB est partagée entre les tests de la classe : on vérifie au moins 2 rôles
        roles.Should().NotBeNull();
        roles!.Count.Should().BeGreaterThanOrEqualTo(2);
    }

    // ── POST /api/v1/roles ────────────────────────────────────────────────────

    [Fact]
    public async Task CreateRole_DonnéesValides_Retourne201AvecRole()
    {
        // Arrange
        var roleName = "TestRole_" + Guid.NewGuid().ToString("N")[..8];
        var client = CreateAdminClient();

        // Act
        var response = await client.PostAsJsonAsync("/api/v1/roles", new { name = roleName });
        var body = await response.Content.ReadAsStringAsync();
        var role = JsonSerializer.Deserialize<RoleDto>(body, JsonOptions);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        role.Should().NotBeNull();
        role!.Id.Should().BeGreaterThan(0);
        role.Name.Should().Be(roleName);
        role.UserCount.Should().Be(0);

        // Vérifie le header Location pointant vers GET /api/v1/roles/{id}
        response.Headers.Location.Should().NotBeNull();
        response.Headers.Location!.ToString().Should().Contain($"/api/v1/roles/{role.Id}");
    }

    [Fact]
    public async Task CreateRole_NomVide_Retourne400AvecErreurDeValidation()
    {
        var client = CreateAdminClient();

        var response = await client.PostAsJsonAsync("/api/v1/roles", new { name = "" });
        var body = await response.Content.ReadAsStringAsync();

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        body.Should().Contain("Name");
    }

    [Fact]
    public async Task CreateRole_NomDéjàExistant_Retourne400()
    {
        // Arrange : créer un rôle une première fois
        var roleName = "Duplicate_" + Guid.NewGuid().ToString("N")[..8];
        await SeedRoleAsync(roleName);
        var client = CreateAdminClient();

        // Act : tenter de recréer le même nom
        var response = await client.PostAsJsonAsync("/api/v1/roles", new { name = roleName });

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    // ── GET /api/v1/roles/{id} ────────────────────────────────────────────────

    [Fact]
    public async Task GetRoleById_QuandExiste_Retourne200AvecRole()
    {
        // Arrange
        var roleName = "GetById_" + Guid.NewGuid().ToString("N")[..8];
        var seeded = await SeedRoleAsync(roleName);
        var client = CreateAdminClient();

        // Act
        var response = await client.GetAsync($"/api/v1/roles/{seeded.Id}");
        var body = await response.Content.ReadAsStringAsync();
        var role = JsonSerializer.Deserialize<RoleDto>(body, JsonOptions);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        role.Should().NotBeNull();
        role!.Id.Should().Be(seeded.Id);
        role.Name.Should().Be(roleName);
    }

    [Fact]
    public async Task GetRoleById_QuandNExistePas_Retourne404()
    {
        var client = CreateAdminClient();

        var response = await client.GetAsync("/api/v1/roles/99999");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    // ── PUT /api/v1/roles/{id} ────────────────────────────────────────────────

    [Fact]
    public async Task UpdateRole_DonnéesValides_Retourne200AvecRoleMisÀJour()
    {
        // Arrange
        var originalName = "Original_" + Guid.NewGuid().ToString("N")[..8];
        var updatedName = "Updated_" + Guid.NewGuid().ToString("N")[..8];
        var seeded = await SeedRoleAsync(originalName);
        var client = CreateAdminClient();

        // Act
        var response = await client.PutAsJsonAsync($"/api/v1/roles/{seeded.Id}", new { name = updatedName });
        var body = await response.Content.ReadAsStringAsync();
        var role = JsonSerializer.Deserialize<RoleDto>(body, JsonOptions);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        role.Should().NotBeNull();
        role!.Name.Should().Be(updatedName);
    }

    [Fact]
    public async Task UpdateRole_QuandNExistePas_Retourne404()
    {
        var client = CreateAdminClient();

        var response = await client.PutAsJsonAsync("/api/v1/roles/99999", new { name = "SomeName" });

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task UpdateRole_AvecNomDéjàPris_Retourne400()
    {
        // Arrange : deux rôles distincts
        var nameA = "RoleA_" + Guid.NewGuid().ToString("N")[..8];
        var nameB = "RoleB_" + Guid.NewGuid().ToString("N")[..8];
        await SeedRoleAsync(nameA);
        var roleB = await SeedRoleAsync(nameB);
        var client = CreateAdminClient();

        // Act : tenter de renommer B avec le nom de A
        var response = await client.PutAsJsonAsync($"/api/v1/roles/{roleB.Id}", new { name = nameA });

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    // ── DELETE /api/v1/roles/{id} ─────────────────────────────────────────────

    [Fact]
    public async Task DeleteRole_QuandExiste_Retourne204()
    {
        // Arrange
        var roleName = "ToDelete_" + Guid.NewGuid().ToString("N")[..8];
        var seeded = await SeedRoleAsync(roleName);
        var client = CreateAdminClient();

        // Act
        var response = await client.DeleteAsync($"/api/v1/roles/{seeded.Id}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);

        // Vérifier que le rôle n'existe plus
        var getResponse = await client.GetAsync($"/api/v1/roles/{seeded.Id}");
        getResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task DeleteRole_QuandNExistePas_Retourne404()
    {
        var client = CreateAdminClient();

        var response = await client.DeleteAsync("/api/v1/roles/99999");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}
