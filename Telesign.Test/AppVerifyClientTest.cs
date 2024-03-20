using NUnit.Framework;
using System.Collections.Generic;
using MockHttpServer;
using System.Net;
using System.Linq;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;

namespace Telesign.Test
{
    [TestFixture]
    [ExcludeFromCodeCoverage]
    public class AppVerifyClientTest : IDisposable
    {
        private string customerId;
        private string apiKey;

        private MockServer mockServer;

        private List<HttpListenerRequest> requests;
        private List<string> requestBodies;
        private List<Dictionary<string, string>> requestHeaders;
        private Func<HttpListenerRequest, HttpListenerResponse, Dictionary<string, string>, string> handlerLambda;

        bool disposed = false;

        [SetUp]
        public void SetUp()
        {
            this.customerId = "FFFFFFFF-EEEE-DDDD-1234-AB1234567890";
            this.apiKey = "EXAMPLETE8sTgg45yusumoN6BYsBVkh+yRJ5czgsnCehZaOYldPJdmFh6NeX8kunZ2zU1YWaUw/0wV6xfw==";

            this.requests = new List<HttpListenerRequest>();
            this.requestBodies = new List<string>();
            this.requestHeaders = new List<Dictionary<string, string>>();
            
            this.handlerLambda = (req, rsp, prm) =>
            {
                requests.Add(req);
                requestBodies.Add(req.Content());

                Dictionary<string, string> headers = new Dictionary<string, string>
                {
                    {"Content-Type", req.Headers["Content-Type"]},
                    {"x-ts-auth-method", req.Headers["x-ts-auth-method"]},
                    {"x-ts-nonce", req.Headers["x-ts-nonce"]},
                    {"Date", req.Headers["Date"]},
                    {"Authorization", req.Headers["Authorization"]}
                };
                requestHeaders.Add(headers);

                return "{}";
            };
            
            this.mockServer = new MockServer(0, "/v1/phoneid/15555555555", (req, rsp, prm) =>
            {
                requests.Add(req);
                requestBodies.Add(req.Content());

                Dictionary<string, string> headers = new Dictionary<string, string>
                {
                    {"Content-Type", req.Headers["Content-Type"]},
                    {"x-ts-auth-method", req.Headers["x-ts-auth-method"]},
                    {"x-ts-nonce", req.Headers["x-ts-nonce"]},
                    {"Date", req.Headers["Date"]},
                    {"Authorization", req.Headers["Authorization"]}
                };
                requestHeaders.Add(headers);

                return "{}";
            });
        }

        [TearDown]
        public void TearDown()
        {
            this.Dispose();
        }

        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (this.disposed)
                return;

            this.mockServer.Dispose();
            this.disposed = true;
        }

        [Test]
        public void TestAppVerifyClientConstructors()
        {
            var appVerifyClient = new AppVerifyClient(this.customerId, this.apiKey);
        }

        [Test]
        public void TestAppVerifyClientStatus()
        {

            var client = new AppVerifyClient(this.customerId,
                this.apiKey,
                string.Format("http://localhost:{0}", this.mockServer.Port));

            
            var externalId = new Guid().ToString();
            
            this.mockServer.AddRequestHandler(new MockHttpHandler($"/v1/mobile/verification/status/{externalId}", "GET", handlerLambda));

            client.Status(externalId);

            Assert.That(this.requests.Last().HttpMethod, Is.EqualTo("GET"), "method is not as expected");
            Assert.That(this.requests.Last().RawUrl, Is.EqualTo($"/v1/mobile/verification/status/{externalId}"), "path is not as expected");
            Assert.That(this.requestHeaders.Last()["Content-Type"], Is.Null);
            Assert.That(this.requestHeaders.Last()["x-ts-auth-method"], Is.EqualTo("HMAC-SHA256"),
                "x-ts-auth-method header is not as expected");

            Guid dummyGuid;
            Assert.That(Guid.TryParse(this.requestHeaders.Last()["x-ts-nonce"], out dummyGuid), Is.True,
                "x-ts-nonce header is not a valid UUID");

            DateTime dummyDateTime;
            Assert.That(DateTime.TryParse(this.requestHeaders.Last()["Date"], out dummyDateTime), Is.True,
                "Date header is not valid rfc2616 format");

            Assert.That(this.requestHeaders.Last()["Authorization"], Is.Not.Null);
        }
        
        [Test]
        public async Task TestAppVerifyClientStatusAsync()
        {

            var client = new AppVerifyClient(this.customerId,
                this.apiKey,
                string.Format("http://localhost:{0}", this.mockServer.Port));

            var externalId = new Guid().ToString();
            
            this.mockServer.AddRequestHandler(new MockHttpHandler($"/v1/mobile/verification/status/{externalId}", "GET", handlerLambda));

            await client.StatusAsync(externalId);

            Assert.That(this.requests.Last().HttpMethod, Is.EqualTo("GET"), "method is not as expected");
            Assert.That(this.requests.Last().RawUrl, Is.EqualTo($"/v1/mobile/verification/status/{externalId}"), "path is not as expected");
            Assert.That(this.requestHeaders.Last()["Content-Type"], Is.Null);
            Assert.That(this.requestHeaders.Last()["x-ts-auth-method"], Is.EqualTo("HMAC-SHA256"),
                "x-ts-auth-method header is not as expected");

            Guid dummyGuid;
            Assert.That(Guid.TryParse(this.requestHeaders.Last()["x-ts-nonce"], out dummyGuid), Is.True,
                "x-ts-nonce header is not a valid UUID");

            DateTime dummyDateTime;
            Assert.That(DateTime.TryParse(this.requestHeaders.Last()["Date"], out dummyDateTime), Is.True,
                "Date header is not valid rfc2616 format");

            Assert.That(this.requestHeaders.Last()["Authorization"], Is.Not.Null);
        }
    }
}