using Microsoft.VisualStudio.TestTools.UnitTesting;
using IncrementService.Controllers;
using IncrementService.Models;
using System;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Mvc;
using System.Net.Http;
using System.Collections.Generic;
using Microsoft.Extensions.Logging.Abstractions;


namespace IncrementService.Tests
{
    [TestClass]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Globalization", "CA1304:Specify CultureInfo", Justification = "<Pending>")]
    public class ControllerTests
    {
        private const string _RequestScheme = "http";
        private const string _RequestHostname = "localhost";
           
        [TestMethod]
        public void AddNewIncrementTest()
        {
            // Arrange
            string key = "addkeytest";
            IncrementController controller = CreateControllerInstance();

            // Act
            CreatedResult result = (CreatedResult)controller.Put(key);

            //Assert
            Assert.AreEqual(201, result.StatusCode);
            Assert.AreEqual($"{_RequestScheme}://{_RequestHostname}/Increment/{key}", result.Location);
        }

        [TestMethod]
        public void AddNewIncrementWithInvalidKeyTest()
        {
            // Arrange
            string key = "badkey!!";
            IncrementController controller = CreateControllerInstance();

            // Act
            BadRequestObjectResult result = (BadRequestObjectResult)controller.Put(key);

            //Assert
            Assert.AreEqual(400, result.StatusCode);
            Assert.AreEqual("Increment Key is not valid.", result.Value.ToString(), true); // true == ignore case
        }

        
        [TestMethod]
        public void AddNewIncrementWithInvalidInitialCountTest()
        {
            // Arrange
            string key = "validkey";
            long initialCount = -1;
            IncrementController controller = CreateControllerInstance();

            // Act
            BadRequestObjectResult result = (BadRequestObjectResult)controller.Put(key, initialCount);

            //Assert
            Assert.AreEqual(400, result.StatusCode);
            Assert.AreEqual("Invalid Initial Count.", result.Value.ToString(), true); // true == ignore case
        }

        [TestMethod]
        public void DeleteIncrementTest()
        {
            // Arrange
            string key = "validkey";
            IncrementController controller = CreateControllerInstance();

            // Act
            OkResult result = (OkResult)controller.Delete(key);

            //Assert
            Assert.AreEqual(200, result.StatusCode);
        }

        [TestMethod]
        public void DeleteIncrementWithInvalidKeyTest()
        {
            // Arrange
            string key = "badkey!!";
            IncrementController controller = CreateControllerInstance();

            // Act
            BadRequestObjectResult result = (BadRequestObjectResult)controller.Delete(key);

            //Assert
            Assert.AreEqual(400, result.StatusCode);
            Assert.AreEqual("Increment Key is not valid.", result.Value.ToString(), true); // true == ignore case
        }

        [TestMethod]
        public void IncrementTest()
        {
            // Arrange
            string key = "validkey";
            IncrementController controller = CreateControllerInstance();

            // Act
            OkObjectResult result = (OkObjectResult)controller.Post(key);

            //Assert
            Assert.AreEqual(200, result.StatusCode);
            Assert.AreEqual(42, (long)result.Value);
        }

        [TestMethod]
        public void IncrementWithInvalidKeyTest()
        {
            // Arrange
            string key = "badkey!!";
            IncrementController controller = CreateControllerInstance();

            // Act
            BadRequestObjectResult result = (BadRequestObjectResult)controller.Post(key);

            //Assert
            Assert.AreEqual(400, result.StatusCode);
            Assert.AreEqual("Increment Key is not valid.", result.Value.ToString(), true); // true == ignore case
        }

        [TestMethod]
        public void GetWithKeyTest()
        {
            // Arrange
            string key = "validkey";
            IncrementController controller = CreateControllerInstance();

            // Act
            ActionResult<IncrementDto> actionResult = controller.Get(key);
            var result = (OkObjectResult)actionResult.Result;
            var value = (IncrementDto)result.Value;

            //Assert
            Assert.AreEqual(200, result.StatusCode); 
            Assert.AreEqual(key, value.Key);
            Assert.AreEqual(IncrementModelMock.DefaultLastUsedTime, value.LastUsed);
            Assert.AreEqual(IncrementModelMock.DefaultNextValue, value.NextVaue);
        }

        [TestMethod]
        public void GetWithInvalidKeyTest()
        {
            // Arrange
            string key = "badkey!!";
            IncrementController controller = CreateControllerInstance();

            // Act
            ActionResult<IncrementDto> actionResult = controller.Get(key);
            BadRequestObjectResult result = (BadRequestObjectResult)actionResult.Result;

            //Assert
            Assert.AreEqual(400, result.StatusCode);
            Assert.AreEqual("Increment Key is not valid.", result.Value.ToString(), true); // true == ignore case
        }

        [TestMethod]
        public void GetAllTest()
        {
            // Arrange
            IncrementController controller = CreateControllerInstance();

            // Act
            ActionResult<IncrementDto> actionResult = controller.Get();
            var result = (OkObjectResult)actionResult.Result;
            var value = (List<IncrementDto>)result.Value;

            //Assert
            Assert.AreEqual(200, result.StatusCode);
            Assert.AreEqual(2, value.Count);
            Assert.AreEqual(IncrementModelMock.DefaultKeyOne, value[0].Key);
            Assert.AreEqual(IncrementModelMock.DefaultKeyTwo, value[1].Key);
        }



        // ----------------- END OF TESTS -------------------

        private IncrementController CreateControllerInstance()
        {
            var model = new IncrementModelMock();

            var httpContext = new DefaultHttpContext();
            httpContext.Request.Scheme = _RequestScheme;
            httpContext.Request.Host = new HostString(_RequestHostname);

            var controllerContext = new ControllerContext()
            {
                HttpContext = httpContext
            };

            IncrementController controller = new IncrementController(new NullLogger<IncrementController>(), model)
            {
                ControllerContext = controllerContext
            };

            return controller;
        }
    }
}