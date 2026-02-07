using HrSystem.Domain.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using System.Linq.Expressions;
using System.Reflection;

namespace HrSystem.Infrustructure.Extensions;

public static class SoftDeleteQueryExtension
{
    public static void GetOnlyNotDeletedEntities(this ModelBuilder builder)
    {
        foreach (var entityType in builder.Model.GetEntityTypes())
        {
            if (typeof(BaseAuditableEntity).IsAssignableFrom(entityType.ClrType) ||
                typeof(BaseAuditableMasterEntity).IsAssignableFrom(entityType.ClrType))
            {
                entityType.AddSoftDeleteQueryFilter();
            }
        }
    }
    public static void AddSoftDeleteQueryFilter3(this IMutableEntityType entityType)
    {
        // Guard: only apply if the type derives from BaseAuditableEntity
        if (!typeof(BaseAuditableEntity).IsAssignableFrom(entityType.ClrType) &&
            !typeof(BaseAuditableMasterEntity).IsAssignableFrom(entityType.ClrType))
            return;

        // Build lambda expression e => e.IsDeleted for the concrete type
        var parameter = Expression.Parameter(entityType.ClrType, "e");
        var prop = Expression.Property(parameter, nameof(BaseAuditableEntity.IsDeleted));
        var body = Expression.Equal(prop, Expression.Constant(true)); // only deleted
        var lambda = Expression.Lambda(body, parameter);

        entityType.SetQueryFilter(lambda);
    }
    private static void AddSoftDeleteQueryFilter(this IMutableEntityType entityData)
    {
        var parameter = Expression.Parameter(entityData.ClrType, "entity");
        var propertyInfo = entityData.ClrType.GetProperty(nameof(BaseAuditableEntity.IsDeleted));

        if (propertyInfo == null)
        {
            throw new InvalidOperationException($"IsDeleted property is missing on {entityData.ClrType.Name}.");
        }

        var memberAccess = Expression.Property(parameter, propertyInfo);
        var body = Expression.Equal(memberAccess, Expression.Constant(false));
        var lambda = Expression.Lambda(body, parameter);

        entityData.SetQueryFilter(lambda);

        var isDeletedProperty = entityData.FindProperty(nameof(BaseAuditableEntity.IsDeleted))
                                ?? throw new InvalidOperationException();
        entityData.AddIndex(isDeletedProperty);
    }
}
