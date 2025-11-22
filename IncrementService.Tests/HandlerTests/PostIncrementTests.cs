using System;
using System.Threading;
using System.Threading.Tasks;
using Common.Exceptions;
using IncrementService.Handlers;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Repository;
using Moq;
using Microsoft.Extensions.Logging;
using FluentAssertions;
using Infrastructure;

namespace IncrementService.Tests.HandlerTests;

[TestClass]
public class PostIncrementTests
{
    [TestMethod]
    public async Task SuccessTest()
    {
        // Arrange
        DateTimeOffset now = DateTimeOffset.Now;
        PostIncrementCommand command = new() { Key = "test" };

        Mock<ILogger<PostIncrementHandler>> logger = new();
        Mock<IIncrementRepository> repository = new();
        repository.Setup(x => x.PostIncrement(command.Key))
            .ReturnsAsync(() => new IncrementKey
                { Key = command.Key, PreviousValue = 42, LastUsed = now });

        PostIncrementHandler handler = new(repository.Object, logger.Object);

        // Act
        IncrementKey result = await handler.Handle(command, CancellationToken.None).ConfigureAwait(false);

        // Assert
        result.Key.Should().Be(command.Key);
        result.LastUsed.Should().Be(now);
        result.PreviousValue.Should().Be(42);
    }

    [TestMethod]
    public async Task EmptyKeyTest()
    {
        // Arrange
        Mock<IIncrementRepository> repository = new();
        Mock<ILogger<PostIncrementHandler>> logger = new();
        PostIncrementHandler handler = new(repository.Object, logger.Object);

        PostIncrementCommand command = new() { Key = string.Empty };

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
