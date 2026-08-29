using System.Linq.Expressions;
using NSubstitute;
using Shouldly;
using Simando.Application.Common;
using Simando.Domain.Common;

namespace Simando.Application.Tests.Common;

public class TestAuditableEntity : AuditableEntity
{
    public string Name { get; set; } = string.Empty;
}

public class EntityServiceTests
{
    private readonly IRepository<TestAuditableEntity> _repository;
    private readonly EntityService<TestAuditableEntity> _service;

    public EntityServiceTests()
    {
        _repository = Substitute.For<IRepository<TestAuditableEntity>>();
        _service = new EntityService<TestAuditableEntity>(_repository);
    }

    [Fact]
    public async Task GetByIdAsync_ForwardsToRepository()
    {
        var id = Guid.NewGuid();
        var entity = new TestAuditableEntity { Id = id, Name = "Test" };
        _repository.GetByIdAsync(id, false, Arg.Any<CancellationToken>())
            .Returns(entity);

        var result = await _service.GetByIdAsync(id);

        result.ShouldBe(entity);
        await _repository.Received(1).GetByIdAsync(id, false, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task GetAllAsync_ForwardsToRepository()
    {
        var entities = new List<TestAuditableEntity>
        {
            new() { Id = Guid.NewGuid(), Name = "A" },
            new() { Id = Guid.NewGuid(), Name = "B" }
        };
        _repository.GetAllAsync(null, false, Arg.Any<CancellationToken>())
            .Returns(entities);

        var result = await _service.GetAllAsync();

        result.ShouldBe(entities);
        await _repository.Received(1).GetAllAsync(null, false, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task GetPagedAsync_ForwardsToRepository()
    {
        var pagedResult = new PagedResult<TestAuditableEntity>(
            new List<TestAuditableEntity> { new() { Id = Guid.NewGuid(), Name = "A" } },
            TotalCount: 1,
            Page: 1,
            PageSize: 10);

        Expression<Func<TestAuditableEntity, object>> orderBy = e => e.Name;
        _repository.GetPagedAsync(1, 10, orderBy, null, false, Arg.Any<CancellationToken>())
            .Returns(pagedResult);

        var result = await _service.GetPagedAsync(1, 10, orderBy);

        result.ShouldBe(pagedResult);
        await _repository.Received(1).GetPagedAsync(1, 10, orderBy, null, false, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExistsAsync_ForwardsToRepository()
    {
        Expression<Func<TestAuditableEntity, bool>> predicate = e => e.Name == "Target";
        _repository.ExistsAsync(predicate, false, Arg.Any<CancellationToken>())
            .Returns(true);

        var result = await _service.ExistsAsync(predicate);

        result.ShouldBeTrue();
        await _repository.Received(1).ExistsAsync(predicate, false, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task AddAsync_ForwardsToRepository()
    {
        var entity = new TestAuditableEntity { Id = Guid.NewGuid(), Name = "New" };
        _repository.AddAsync(entity, Arg.Any<CancellationToken>())
            .Returns(entity);

        var result = await _service.AddAsync(entity);

        result.ShouldBe(entity);
        await _repository.Received(1).AddAsync(entity, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task UpdateAsync_ForwardsToRepository()
    {
        var id = Guid.NewGuid();
        Action<TestAuditableEntity> mutate = e => e.Name = "Updated";
        _repository.UpdateAsync(id, mutate, Arg.Any<CancellationToken>())
            .Returns(true);

        var result = await _service.UpdateAsync(id, mutate);

        result.ShouldBeTrue();
        await _repository.Received(1).UpdateAsync(id, mutate, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task SoftDeleteAsync_ForwardsToRepository()
    {
        var id = Guid.NewGuid();
        _repository.SoftDeleteAsync(id, Arg.Any<CancellationToken>())
            .Returns(true);

        var result = await _service.SoftDeleteAsync(id);

        result.ShouldBeTrue();
        await _repository.Received(1).SoftDeleteAsync(id, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task DeleteAsync_ForwardsToRepository()
    {
        var id = Guid.NewGuid();
        _repository.DeleteAsync(id, Arg.Any<CancellationToken>())
            .Returns(true);

        var result = await _service.DeleteAsync(id);

        result.ShouldBeTrue();
        await _repository.Received(1).DeleteAsync(id, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task RestoreAsync_ForwardsToRepository()
    {
        var id = Guid.NewGuid();
        _repository.RestoreAsync(id, Arg.Any<CancellationToken>())
            .Returns(true);

        var result = await _service.RestoreAsync(id);

        result.ShouldBeTrue();
        await _repository.Received(1).RestoreAsync(id, Arg.Any<CancellationToken>());
    }
}
