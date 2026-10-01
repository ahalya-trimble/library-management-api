using LibraryManagementAPI.Controllers;
using LibraryManagementAPI.DTOs;
using LibraryManagementAPI.Models;
using LibraryManagementAPI.Repositories;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;

namespace LibraryManagementAPI.Tests.Controllers;

public class MembersControllerTests
{
    private readonly Mock<IMemberRepository> _repositoryMock = new();
    private readonly MembersController _controller;

    public MembersControllerTests()
    {
        _controller = new MembersController(_repositoryMock.Object)
        {
            ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext()
            }
        };
    }

    [Fact]
    public async Task GetMembers_ReturnsOkWithItems()
    {
        var pagedResult = new PagedResult<Member>
        {
            Items = new List<Member> { new() { Id = 1, Name = "John", Email = "john@example.com" } },
            TotalCount = 1,
            PageNumber = 1,
            PageSize = 10
        };
        _repositoryMock.Setup(r => r.GetMembersAsync(It.IsAny<MemberQueryParameters>())).ReturnsAsync(pagedResult);

        var result = await _controller.GetMembers(new MemberQueryParameters());

        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var members = Assert.IsAssignableFrom<IEnumerable<Member>>(okResult.Value);
        Assert.Single(members);
    }

    [Fact]
    public async Task GetMemberById_ReturnsNotFound_WhenMissing()
    {
        _repositoryMock.Setup(r => r.GetByIdAsync(It.IsAny<int>())).ReturnsAsync((Member?)null);

        var result = await _controller.GetMemberById(1);

        Assert.IsType<NotFoundResult>(result.Result);
    }

    [Fact]
    public async Task GetMemberByEmail_ReturnsOk_WhenFound()
    {
        _repositoryMock.Setup(r => r.GetMemberByEmailAsync("john@example.com"))
            .ReturnsAsync(new Member { Id = 1, Name = "John", Email = "john@example.com" });

        var result = await _controller.GetMemberByEmail("john@example.com");

        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var member = Assert.IsType<Member>(okResult.Value);
        Assert.Equal("john@example.com", member.Email);
    }

    [Fact]
    public async Task CreateMember_ReturnsConflict_WhenEmailExists()
    {
        _repositoryMock.Setup(r => r.EmailExistsAsync(It.IsAny<string>(), null)).ReturnsAsync(true);

        var result = await _controller.CreateMember(new CreateMemberDto { Name = "John", Email = "john@example.com", MembershipDate = DateTime.UtcNow });

        Assert.IsType<ConflictObjectResult>(result.Result);
        _repositoryMock.Verify(r => r.AddAsync(It.IsAny<Member>()), Times.Never);
    }

    [Fact]
    public async Task CreateMember_ReturnsCreatedAtAction_WhenEmailIsUnique()
    {
        _repositoryMock.Setup(r => r.EmailExistsAsync(It.IsAny<string>(), null)).ReturnsAsync(false);

        var result = await _controller.CreateMember(new CreateMemberDto { Name = "John", Email = "john@example.com", MembershipDate = DateTime.UtcNow });

        Assert.IsType<CreatedAtActionResult>(result.Result);
        _repositoryMock.Verify(r => r.AddAsync(It.Is<Member>(m => m.Email == "john@example.com")), Times.Once);
    }

    [Fact]
    public async Task UpdateMember_ReturnsNotFound_WhenMissing()
    {
        _repositoryMock.Setup(r => r.GetByIdAsync(It.IsAny<int>())).ReturnsAsync((Member?)null);

        var result = await _controller.UpdateMember(1, new UpdateMemberDto { Name = "John", Email = "john@example.com", MembershipDate = DateTime.UtcNow });

        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task UpdateMember_ReturnsConflict_WhenEmailExistsOnAnotherMember()
    {
        _repositoryMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(new Member { Id = 1, Name = "Old", Email = "old@example.com" });
        _repositoryMock.Setup(r => r.EmailExistsAsync("new@example.com", 1)).ReturnsAsync(true);

        var result = await _controller.UpdateMember(1, new UpdateMemberDto { Name = "New", Email = "new@example.com", MembershipDate = DateTime.UtcNow });

        Assert.IsType<ConflictObjectResult>(result);
    }

    [Fact]
    public async Task DeleteMember_ReturnsNoContent_WhenDeleted()
    {
        var member = new Member { Id = 1, Name = "John", Email = "john@example.com" };
        _repositoryMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(member);

        var result = await _controller.DeleteMember(1);

        Assert.IsType<NoContentResult>(result);
        _repositoryMock.Verify(r => r.DeleteAsync(member), Times.Once);
    }
}
