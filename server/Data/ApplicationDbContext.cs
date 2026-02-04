using Microsoft.EntityFrameworkCore;
using server.Models;

namespace server.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<Role> Roles { get; set; }
    public DbSet<Equipment> Equipments { get; set; }
    public DbSet<Tag> Tags { get; set; }
    public DbSet<Of> Ofs { get; set; }
    public DbSet<Historian> Historian { get; set; }
    public DbSet<Line> Lines { get; set; }
    public DbSet<User> Users { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configure Role
        modelBuilder.Entity<Role>(entity =>
        {
            entity.ToTable("Roles");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id)
                .UseIdentityAlwaysColumn()
                .HasColumnName("role_id");
            entity.Property(e => e.Name)
                .HasMaxLength(50)
                .HasColumnName("name");
        });

        // Configure Equipment
        modelBuilder.Entity<Equipment>(entity =>
        {
            entity.ToTable("Equipments");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id)
                .UseIdentityAlwaysColumn()
                .HasColumnName("equipment_id");
            entity.Property(e => e.Name)
                .IsRequired()
                .HasMaxLength(50)
                .HasColumnName("name");
        });

        // Configure Tag
        modelBuilder.Entity<Tag>(entity =>
        {
            entity.ToTable("Tags");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id)
                .UseIdentityAlwaysColumn()
                .HasColumnName("tag_id");
            entity.Property(e => e.TagName)
                .IsRequired()
                .HasMaxLength(50)
                .HasColumnName("tag_name");
        });

        // Configure Of
        modelBuilder.Entity<Of>(entity =>
        {
            entity.ToTable("ofs");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id)
                .UseIdentityAlwaysColumn()
                .HasColumnName("of_id");
            entity.Property(e => e.Of_)
                .IsRequired()
                .HasMaxLength(50)
                .HasColumnName("of_name");
            entity.Property(e => e.Produit)
                .IsRequired()
                .HasMaxLength(50)
                .HasColumnName("produit");
            entity.Property(e => e.QteProduite)
                .IsRequired()
                .HasColumnName("qte_produite");
            entity.Property(e => e.QteTotale)
                .IsRequired()
                .HasColumnName("qte_totale");
        });

        // Configure Historian
        modelBuilder.Entity<Historian>(entity =>
        {
            entity.ToTable("Historian");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id)
                .UseIdentityAlwaysColumn()
                .HasColumnName("historian_id");
            entity.Property(e => e.Timestamp)
                .IsRequired()
                .HasColumnName("timestamp");
            entity.Property(e => e.Value)
                .IsRequired()
                .HasPrecision(15, 2)
                .HasColumnName("value");
            entity.Property(e => e.TagNameId)
                .IsRequired()
                .HasColumnName("tag_name");

            entity.HasOne(h => h.TagNameNavigation)
                .WithMany(t => t.Historians)
                .HasForeignKey(h => h.TagNameId)
                .HasConstraintName("historian_tag_name_fkey")
                .OnDelete(DeleteBehavior.Restrict);
        });

        // Configure Line
        modelBuilder.Entity<Line>(entity =>
        {
            entity.ToTable("Lines");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id)
                .UseIdentityAlwaysColumn()
                .HasColumnName("line_id");
            entity.Property(e => e.Name)
                .IsRequired()
                .HasMaxLength(50)
                .HasColumnName("name");
            entity.Property(e => e.IsChangement)
                .IsRequired()
                .HasColumnName("is_changement");
            entity.Property(e => e.TempsChangement)
                .IsRequired()
                .HasColumnName("temps_changement");
            entity.Property(e => e.EquipmentId)
                .IsRequired()
                .HasColumnName("equipment");
            entity.Property(e => e.OfSuivantId)
                .HasColumnName("of_suivant");
            entity.Property(e => e.OfEnCoursId)
                .HasColumnName("of_en_cours");

            entity.HasOne(l => l.Equipment)
                .WithOne(e => e.Line)
                .HasForeignKey<Line>(l => l.EquipmentId)
                .HasConstraintName("lines_equipment_fkey")
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(l => l.OfSuivant)
                .WithMany(o => o.LinesSuivant)
                .HasForeignKey(l => l.OfSuivantId)
                .HasConstraintName("lines_of_suivant_fkey")
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(l => l.OfEnCours)
                .WithMany(o => o.LinesEnCours)
                .HasForeignKey(l => l.OfEnCoursId)
                .HasConstraintName("lines_of_en_cours_fkey")
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(e => e.EquipmentId)
                .IsUnique()
                .HasDatabaseName("lines_equipment_key");
        });

        // Configure User
        modelBuilder.Entity<User>(entity =>
        {
            entity.ToTable("Users");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id)
                .UseIdentityAlwaysColumn()
                .HasColumnName("user_id");
            entity.Property(e => e.Identifiant)
                .IsRequired()
                .HasMaxLength(50)
                .HasColumnName("identifiant");
            entity.Property(e => e.Password)
                .IsRequired()
                .HasMaxLength(255)
                .HasColumnName("password");
            entity.Property(e => e.Email)
                .IsRequired()
                .HasMaxLength(100)
                .HasColumnName("email");
            entity.Property(e => e.CreatedAt)
                .IsRequired()
                .HasColumnName("created_at");
            entity.Property(e => e.UpdatedAt)
                .IsRequired()
                .HasColumnName("updated_at");
            entity.Property(e => e.RoleId)
                .HasColumnName("role");

            entity.HasOne(u => u.Role)
                .WithMany(r => r.Users)
                .HasForeignKey(u => u.RoleId)
                .HasConstraintName("users_role_fkey")
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(e => e.Identifiant)
                .IsUnique()
                .HasDatabaseName("users_identifiant_key");
        });

        // Configure Equipment_Tag many-to-many relationship
        modelBuilder.Entity<Equipment>()
            .HasMany(e => e.Tags)
            .WithMany(t => t.Equipments)
            .UsingEntity<Dictionary<string, object>>(
                "Equipment_Tag",
                j => j
                    .HasOne<Tag>()
                    .WithMany()
                    .HasForeignKey("tag_name")
                    .HasConstraintName("equipment_tag_tag_name_fkey")
                    .OnDelete(DeleteBehavior.Restrict),
                j => j
                    .HasOne<Equipment>()
                    .WithMany()
                    .HasForeignKey("equipment")
                    .HasConstraintName("equipment_tag_equipment_fkey")
                    .OnDelete(DeleteBehavior.Restrict),
                j =>
                {
                    j.HasKey("equipment", "tag_name");
                    j.ToTable("Equipment_Tag");
                });
    }
}
