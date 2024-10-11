using NUnit.Framework;
using SupportSystem.WebAPI.Controllers;
using SupportSystem.WebAPI.Model.MetaData;
using Microsoft.AspNetCore.Mvc;

namespace SupportSystem.WebAPI.Tests
{
    
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

      




    }
}