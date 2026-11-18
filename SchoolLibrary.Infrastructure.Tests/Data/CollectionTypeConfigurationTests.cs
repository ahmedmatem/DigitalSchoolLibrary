using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using SchoolLibrary.Domain.Entities;
using SchoolLibrary.Domain.Enums;
using SchoolLibrary.Infrastructure.Data;
using Xunit;

namespace SchoolLibrary.Infrastructure.Tests.Data;

public class CollectionTypeConfigurationTests
{
    [Fact]
    public void SqlServerModel_CollectionDefaults_DoNotRaiseSentinelWarnings()
    {
        using var db = CreateContext();

        // Model validation runs without opening a SQL Server connection.
        Assert.NotNull(db.Model.FindEntityType(typeof(Category)));
        Assert.NotNull(db.Model.FindEntityType(typeof(Resource)));
    }

    [Theory]
    [InlineData(false, ResourceCollectionType.EducationalResources)]
    [InlineData(false, ResourceCollectionType.ELibrary)]
    [InlineData(true, ResourceCollectionType.EducationalResources)]
    [InlineData(true, ResourceCollectionType.ELibrary)]
    public void Track_ExplicitCollectionType_PreservesValue(bool resource, ResourceCollectionType collectionType)
    {
        using var db = CreateContext();
        object entity = resource
            ? new Resource { CollectionType = collectionType }
            : new Category { CollectionType = collectionType };

        var entry = db.Add(entity);
        var property = entry.Property(nameof(Category.CollectionType));

        Assert.Equal(collectionType, property.CurrentValue);
        Assert.False(property.IsTemporary);
        Assert.Equal((ResourceCollectionType)0, property.Metadata.Sentinel);
        Assert.Equal(ResourceCollectionType.EducationalResources, property.Metadata.GetDefaultValue());
    }

    private static ApplicationDbContext CreateContext() => new(
        new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseSqlServer("Server=unused;Database=unused")
            .ConfigureWarnings(warnings => warnings.Throw(RelationalEventId.BoolWithDefaultWarning))
            .Options);
}
