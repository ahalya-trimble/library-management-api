using LibraryManagementAPI.Data;
using LibraryManagementAPI.DTOs;
using LibraryManagementAPI.Models;
using LibraryManagementAPI.Repositories;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace LibraryManagementAPI.Tests.Repositories;

public class MemberRepositoryTests
{
    private static LibraryDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<LibraryDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new LibraryDbContext(options);
    }

    [Fact]
    public async Task GetMemberByEmailAsync_ReturnsMember_WhenExists()
    {
        using var context = CreateContext();
        var repository = new MemberRepository(context);
        await repository.AddAsync(new Member { Name = "John", Email = "john@example.com", MembershipDate = DateTime.UtcNow });

        var result = await repository.GetMemberByEmailAsync("john@example.com");

        Assert.NotNull(result);
        Assert.Equal("John", result!.Name);
    }

    [Fact]
    public async Task EmailExistsAsync_ExcludesGivenId()
    {
        using var context = CreateContext();
        var repository = new MemberRepository(context);
        var member = new Member { Name = "John", Email = "john@example.com", MembershipDate = DateTime.UtcNow };
        await repository.AddAsync(member);

        var exists = await repository.EmailExistsAsync("john@example.com", member.Id);

        Assert.False(exists);
    }

    [Fact]
    public async Task GetMembersAsync_FiltersByName()
    {
        using var context = CreateContext();
        var repository = new MemberRepository(context);
        await repository.AddAsync(new Member { Name = "John Smith", Email = "john@example.com", MembershipDate = DateTime.UtcNow });
        await repository.AddAsync(new Member { Name = "Jane Doe", Email = "jane@example.com", MembershipDate = DateTime.UtcNow });

        var result = await repository.GetMembersAsync(new MemberQueryParameters { Name = "Smith" });

        Assert.Single(result.Items);
        Assert.Equal("John Smith", result.Items.First().Name);
    }

    [Fact]
    public async Task GetMembersAsync_SortsByEmailDescending()
    {
        using var context = CreateContext();
        var repository = new MemberRepository(context);
        await repository.AddAsync(new Member { Name = "A", Email = "aaa@example.com", MembershipDate = DateTime.UtcNow });
        await repository.AddAsync(new Member { Name = "B", Email = "zzz@example.com", MembershipDate = DateTime.UtcNow });

        var result = await repository.GetMembersAsync(new MemberQueryParameters { SortBy = "email", SortOrder = "desc" });

        Assert.Equal("zzz@example.com", result.Items.First().Email);
    }

    [Fact]
    public async Task GetMembersAsync_AppliesPaging()
    {
        using var context = CreateContext();
        var repository = new MemberRepository(context);
        for (var i = 1; i <= 5; i++)
        {
            await repository.AddAsync(new Member { Name = $"Member{i}", Email = $"member{i}@example.com", MembershipDate = DateTime.UtcNow });
        }

        var result = await repository.GetMembersAsync(new MemberQueryParameters { PageNumber = 2, PageSize = 2 });

        Assert.Equal(2, result.Items.Count());
        Assert.Equal(5, result.TotalCount);
    }
}
