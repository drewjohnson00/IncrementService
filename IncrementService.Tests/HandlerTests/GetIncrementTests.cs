using System;
using System.Threading;
using System.Threading.Tasks;
using Common.Exceptions;
using FluentAssertions;
using IncrementService.Handlers;
using Infrastructure;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Repository;

namespace IncrementService.Tests.HandlerTests;

[TestClass]
public class GetIncrementTests
{
    [TestMethod]
    public async Task SuccessTest()
    {
        // Arrange
        DateTimeOffset now = DateTime.Now;
        string key = "test";
        Mock<ILogger<GetIncrementHandler>> logger = new();
        Mock<IIncrementRepository> repository = new();
        repository.Setup(x => x
            .GetIncrement(key))
            .ReturnsAsync(new IncrementKey {Key = key, PreviousValue = 42, LastUsed = now});
        GetIncrementHandler handler = new(repository.Object, logger.Object);

        GetIncrementQuery query = new() { Key = "test" };

        // Act
        IncrementKey incrementKey = await handler.Handle(query, CancellationToken.None).ConfigureAwait(false);

        // Assert
        incrementKey.Should().NotBeNull();
        incrementKey.Key.Should().Be(key);
        incrementKey.PreviousValue.Should().Be(42);
        incrementKey.LastUsed.Should().Be(now);
    }

    [TestMethod]
    public async Task EmptyKeyTest()
    {
        // Arrange
        Mock<IIncrementRepository> repository = new();
        Mock<ILogger<GetIncrementHandler>> logger = new();
        GetIncrementHandler handler = new(repository.Object, logger.Object);

        GetIncrementQuery command = new() { Key = string.Empty };

        // Act
        Func<Task> act = async () => await handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should()
            .ThrowAsync<BadRequestException>()
            .WithMessage("Validation Failed")
            .Where(e => e.ValidationErrors.Count == 1)
            .Where(e => e.ValidationErrors[0] == "Key must be provided.");
    }

}
