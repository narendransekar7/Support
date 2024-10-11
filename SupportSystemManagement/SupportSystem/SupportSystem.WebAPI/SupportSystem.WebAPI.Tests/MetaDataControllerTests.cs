namespace SupportSystem.WebAPI.Tests;
{
    
using System;
using NUnit.Framework;
using SupportSystem.WebAPI.Controllers;
using SupportSystem.WebAPI.Model.MetaData;
using Microsoft.AspNetCore.Mvc;

    [TestFixture]
    public class MetaDataControllerTests
    {
        private MetaDataController _controller;

        [SetUp]
        public void Setup()
        {
            _controller = new MetaDataController();
        }

        [Test]
        public void CreateMetaData_SQLServer_ReturnsOk()
        {
            // Arrange
            var serverDetail = new ServerDetail
            {
                Type = DatabaseType.SQLServer,
                Server = "learningdatanaren.database.windows.net",
                User_Id = "Narendran",
                Password = "Naren",
                Database = "Newdatabase"
            };

            // Act
            var result = _controller.CreateMetaData(serverDetail) as OkObjectResult;

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(200, result.StatusCode);
            Assert.IsNotNull(result.Value);
            // Additional assertions based on the expected return value
        }

        [Test]
        public void CreateMetaData_InvalidDatabaseType_ThrowsArgumentException()
        {
            // Arrange
            var serverDetail = new ServerDetail
            {
                Type = (DatabaseType)999, // Invalid type
                Server = "myServerAddress",
                User_Id = "myUsername",
                Password = "myPassword",
                Database = "myDatabase"
            };

            // Act & Assert
            var ex = Assert.Throws<ArgumentException>(() => _controller.CreateMetaData(serverDetail));
            Assert.AreEqual("Invalid database type specified.", ex.Message);
        }




    }
}
