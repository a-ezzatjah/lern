using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Entities.Migrations;

[DbContext(typeof(ShopDbContext))]
[Migration("20261009120000_AddElasticCategories")]
public sealed class AddElasticCategories : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql("""
            IF NOT EXISTS (SELECT 1 FROM [categories] WHERE [Name] = N'کش')
                INSERT INTO [categories] ([Name], [Slug], [SortOrder]) VALUES (N'کش', N'elastic', 0);
            DECLARE @root int = (SELECT [Id] FROM [categories] WHERE [Name] = N'کش');
            UPDATE [categories] SET [ParentId] = NULL WHERE [Id] = @root;
            IF NOT EXISTS (SELECT 1 FROM [categories] WHERE [Name] = N'کش قیزان')
                INSERT INTO [categories] ([Name], [Slug], [ParentId], [SortOrder]) VALUES (N'کش قیزان', N'elastic-qizan', @root, 1);
            IF NOT EXISTS (SELECT 1 FROM [categories] WHERE [Name] = N'کش ایرانی')
                INSERT INTO [categories] ([Name], [Slug], [ParentId], [SortOrder]) VALUES (N'کش ایرانی', N'elastic-iranian', @root, 2);
            IF NOT EXISTS (SELECT 1 FROM [categories] WHERE [Name] = N'کش خارجی')
                INSERT INTO [categories] ([Name], [Slug], [ParentId], [SortOrder]) VALUES (N'کش خارجی', N'elastic-imported', @root, 3);
            UPDATE [categories] SET [ParentId] = @root WHERE [Name] IN (N'کش قیزان', N'کش ایرانی', N'کش خارجی');
            DECLARE @iranian int = (SELECT [Id] FROM [categories] WHERE [Name] = N'کش ایرانی');
            -- These are the existing elastic products the catalog owner asked to move.
            DELETE pc FROM [ProductCategories] pc JOIN [products] p ON p.[Id] = pc.[ProductId]
                WHERE p.[Name] IN (N'کش سه سانت', N'کش 1 سانت') AND pc.[CategoryId] <> @iranian;
            INSERT INTO [ProductCategories] ([ProductId], [CategoryId])
                SELECT p.[Id], @iranian FROM [products] p
                WHERE p.[Name] IN (N'کش سه سانت', N'کش 1 سانت')
                    AND NOT EXISTS (SELECT 1 FROM [ProductCategories] pc WHERE pc.[ProductId] = p.[Id] AND pc.[CategoryId] = @iranian);
            -- Remove the unused legacy category without discarding unrelated products or children.
            DELETE c FROM [categories] c WHERE c.[Slug] = N'buttons-and-hardware'
                AND c.[Name] IN (N'دکمه و یراق‌آلات', N'دکمه و یراق آلات')
                AND NOT EXISTS (SELECT 1 FROM [ProductCategories] pc WHERE pc.[CategoryId] = c.[Id])
                AND NOT EXISTS (SELECT 1 FROM [categories] child WHERE child.[ParentId] = c.[Id]);
            """);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        // Product/category assignments can have been edited since this migration.
        throw new NotSupportedException("Catalog data changes must be restored explicitly to preserve product assignments.");
    }
}
