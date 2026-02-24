using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace server.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Equipments",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityAlwaysColumn),
                    Name = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Equipments", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ofs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityAlwaysColumn),
                    of_ = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    produit = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    qte_produite = table.Column<int>(type: "integer", nullable: false),
                    qte_totale = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ofs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Roles",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityAlwaysColumn),
                    Name = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Roles", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Tags",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityAlwaysColumn),
                    tag_name = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Tags", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Lines",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityAlwaysColumn),
                    name = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    is_changement = table.Column<bool>(type: "boolean", nullable: false),
                    temps_changement = table.Column<int>(type: "integer", nullable: false),
                    equipment = table.Column<int>(type: "integer", nullable: false),
                    of_suivant = table.Column<int>(type: "integer", nullable: true),
                    of_en_cours = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Lines", x => x.Id);
                    table.ForeignKey(
                        name: "lines_equipment_fkey",
                        column: x => x.equipment,
                        principalTable: "Equipments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "lines_of_en_cours_fkey",
                        column: x => x.of_en_cours,
                        principalTable: "ofs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "lines_of_suivant_fkey",
                        column: x => x.of_suivant,
                        principalTable: "ofs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false),
                    identifiant = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    password = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    email = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    role = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.Id);
                    table.ForeignKey(
                        name: "users_role_fkey",
                        column: x => x.role,
                        principalTable: "Roles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Equipment_Tag",
                columns: table => new
                {
                    equipment = table.Column<int>(type: "integer", nullable: false),
                    tag_name = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Equipment_Tag", x => new { x.equipment, x.tag_name });
                    table.ForeignKey(
                        name: "equipment_tag_equipment_fkey",
                        column: x => x.equipment,
                        principalTable: "Equipments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "equipment_tag_tag_name_fkey",
                        column: x => x.tag_name,
                        principalTable: "Tags",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Historian",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityAlwaysColumn),
                    timestamp_ = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    value_ = table.Column<decimal>(type: "numeric(15,2)", precision: 15, scale: 2, nullable: false),
                    tag_name = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Historian", x => x.Id);
                    table.ForeignKey(
                        name: "historian_tag_name_fkey",
                        column: x => x.tag_name,
                        principalTable: "Tags",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Equipment_Tag_tag_name",
                table: "Equipment_Tag",
                column: "tag_name");

            migrationBuilder.CreateIndex(
                name: "IX_Historian_tag_name",
                table: "Historian",
                column: "tag_name");

            migrationBuilder.CreateIndex(
                name: "IX_Lines_of_en_cours",
                table: "Lines",
                column: "of_en_cours");

            migrationBuilder.CreateIndex(
                name: "IX_Lines_of_suivant",
                table: "Lines",
                column: "of_suivant");

            migrationBuilder.CreateIndex(
                name: "lines_equipment_key",
                table: "Lines",
                column: "equipment",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Users_role",
                table: "Users",
                column: "role");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Equipment_Tag");

            migrationBuilder.DropTable(
                name: "Historian");

            migrationBuilder.DropTable(
                name: "Lines");

            migrationBuilder.DropTable(
                name: "Users");

            migrationBuilder.DropTable(
                name: "Tags");

            migrationBuilder.DropTable(
                name: "Equipments");

            migrationBuilder.DropTable(
                name: "ofs");

            migrationBuilder.DropTable(
                name: "Roles");
        }
    }
}
