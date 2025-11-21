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
public class PutIncrementTests
{
    [TestMethod]
    public async Task SuccessTest()
    {
        // Arrange
        DateTimeOffset now = DateTimeOffset.Now;
        PutIncrementCommand command = new() { Key = "test" };

        Mock<ILogger<PutIncrementHandler>> logger = new();
        Mock<IIncrementRepository> repository = new();
        repository.Setup(x => x.UpsertIncrement(command))
            .ReturnsAsync(() => new IncrementKey
                { Key = command.Key, PreviousValue = command.PreviousValue ?? 0, LastUsed = now });

        PutIncrementHandler handler = new(repository.Object, logger.Object);

        // Act
        IncrementKey result = await handler.Handle(command, CancellationToken.None).ConfigureAwait(false);

        // Assert
        result.Key.Should().Be(command.Key);
        result.LastUsed.Should().Be(now);
        result.PreviousValue.Should().Be(0);
    }

    [TestMethod]
    public async Task SuccessWithPreviousValueSuppliedTest()
    {
        // Arrange
        DateTimeOffset now = DateTimeOffset.Now;
        PutIncrementCommand command = new() { Key = "test", PreviousValue = 1 };

        Mock<ILogger<PutIncrementHandler>> logger = new();
        Mock<IIncrementRepository> repository = new();
        repository.Setup(x => x.UpsertIncrement(command))
            .ReturnsAsync(() => new IncrementKey
                { Key = command.Key, PreviousValue = command.PreviousValue ?? 0, LastUsed = now });

        PutIncrementHandler handler = new(repository.Object, logger.Object);

        // Act
        IncrementKey result = await handler.Handle(command, CancellationToken.None).ConfigureAwait(false);

        // Assert
        result.Key.Should().Be("test");
        result.PreviousValue.Should().Be(1);
    }

    [TestMethod]
    public async Task EmptyKeyTest()
    {
        // Arrange
        Mock<IIncrementRepository> repository = new();
        Mock<ILogger<PutIncrementHandler>> logger = new();
        PutIncrementHandler handler = new(repository.Object, logger.Object);

        PutIncrementCommand command = new() { Key = string.Empty };

        // Act
        Func<Task> act = async () => await handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<BadRequestException>()
            .WithMessage("Validation Failed: Key must be between 3 and 50 characters.; Key must only contain letters, digits, or underscores.");
    }

    [TestMethod]
    public async Task TooShortKeyTest()
    {
        // Arrange
        Mock<IIncrementRepository> repository = new();
        Mock<ILogger<PutIncrementHandler>> logger = new();
        PutIncrementHandler handler = new(repository.Object, logger.Object);

        PutIncrementCommand command = new() { Key = "TS" };

        // Act
        Func<Task> act = async () => await handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<BadRequestException>()
            .WithMessage("Validation Failed: Key must be between 3 and 50 characters.");
    }

    [TestMethod]
    public async Task TooLongKeyTest()
    {
        // Arrange
        Mock<IIncrementRepository> repository = new();
        Mock<ILogger<PutIncrementHandler>> logger = new();
        PutIncrementHandler handler = new(repository.Object, logger.Object);

        PutIncrementCommand command = new() { Key = "123456789012345678901234567890123456789012345678901" }; // 51 characters

        // Act
        Func<Task> act = async () => await handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<BadRequestException>()
            .WithMessage("Validation Failed: Key must be between 3 and 50 characters.");
    }

    [TestMethod]
    public async Task KeyContainingInvalidCharactersTest()
    {
        // Arrange
        Mock<IIncrementRepository> repository = new();
        Mock<ILogger<PutIncrementHandler>> logger = new();
        PutIncrementHandler handler = new(repository.Object, logger.Object);

        PutIncrementCommand command = new()
        {
            Key = "Hello-Test" // If this string contains any invalid characters, the test should pass. If not, it should fail.
        }; 

        // Act
        Func<Task> act = async () => await handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<BadRequestException>()
            .WithMessage("Validation Failed: Key must only contain letters, digits, or underscores.");
    }
}
