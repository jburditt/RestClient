using Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NSubstitute;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;

namespace RestClient.Tests
{
    [TestClass]
    public class ServiceTests
    {
        private MemoryDatabaseLogger _databaseLogger;
        private ILogging _logging;
        
        [TestInitialize]
        public void TestInitialize()
        {
            _databaseLogger = new MemoryDatabaseLogger();
            _logging = new SimpleLogger(null, _databaseLogger);
        }

        [TestMethod]
        public void Trace_Request()
        {
            // mock rest client
            var baseClient = Substitute.For<IRestClient>();
            var response = Substitute.For<IRestResponse<ApiResult>>();
            response.StatusCode.Returns(HttpStatusCode.OK);
            baseClient.Execute<ApiResult>(Arg.Any<IRestRequest>()).Returns(response);

            // initialize promo service client
            var config = new TestServiceConfig { BaseUrl = "http://localhost/api/", Token = "SECRET" };
            var client = new TestClient(_logging, baseClient, config);
            var model = new ViewModelRequest();

            // act
            var result = client.Validate(model);
            var log = _databaseLogger.Logs.LastOrDefault();

            // assert
            Assert.AreEqual(Level.Trace, log.Level);
            Assert.AreEqual("Request http://localhost/api/todo/add sent with parameters: application/json={\"Id\":null,\"Description\":null}, Token=SECRET", log.Message);
        }
    }
}
