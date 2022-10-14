using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.ChangeTracking.Internal;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Migrations.Internal;
using Microsoft.EntityFrameworkCore.Migrations.Operations;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Update;
using Microsoft.EntityFrameworkCore.Update.Internal;

namespace AbpYes.BaseServer.EntityFrameworkCore;

/*
 *  阻止EFCore生成主外键关系
 *  Tips:
 *      1）根据阿里开发手册规范，应该禁止相关外键的生成，具体原因可参考阿里手册。
 *      2）此类将在迁移时生效。
 */
public class AbpYesMigrationsModelDiffer : MigrationsModelDiffer
{
    public AbpYesMigrationsModelDiffer(
        [NotNull] IRelationalTypeMappingSource typeMappingSource,
        [NotNull] IMigrationsAnnotationProvider migrationsAnnotations, [NotNull] IChangeDetector changeDetector,
        [NotNull] IUpdateAdapterFactory updateAdapterFactory,
        [NotNull] CommandBatchPreparerDependencies commandBatchPreparerDependencies) : base(typeMappingSource,
        migrationsAnnotations, changeDetector, updateAdapterFactory, commandBatchPreparerDependencies
    )
    {
    }


    public override IReadOnlyList<MigrationOperation> GetDifferences(IRelationalModel source, IRelationalModel target)
    {
        var operations = base.GetDifferences(source, target).ToList();

        foreach (var operation in operations.OfType<CreateTableOperation>())
        {
            operation.ForeignKeys?.Clear();
        }

        return operations;
    }
}