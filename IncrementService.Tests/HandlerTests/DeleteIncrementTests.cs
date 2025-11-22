using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Common.Exceptions;
using IncrementService.Handlers;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Repository;
using Moq;
using Microsoft.Extensions.Logging;
using FluentAssertions;

namespace IncrementService.Tests.HandlerTests;

[TestClass]
public class DeleteIncrementTests
{
    [TestMethod]
    public async Task SuccessTest()
    {
        // Arrange
        Mock<IIncrementRepository> repository = new();
        Mock<ILogger<DeleteIncrementHandler>> logger = new();
        DeleteIncrementHandler handler = new(repository.Object, logger.Object);

        DeleteIncrementCommand command = new() { Key = "test" };

        // Act
        await handler.Handle(command, CancellationToken.None).ConfigureAwait(false);

        // Assert

    }

    [TestMethod]
    public async Task EmptyKeyTest()
    {
        // Arrange
        Mock<IIncrementRepository> repository = new();
        Mock<ILogger<DeleteIncrementHandler>> logger = new();
        DeleteIncrementHandler handler = new(repository.Object, logger.Object);

        DeleteIncrementCommand command = new() { Key = string.Empty };

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
