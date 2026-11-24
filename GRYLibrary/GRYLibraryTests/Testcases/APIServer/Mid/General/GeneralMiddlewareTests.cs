using GRYLibrary.Core.APIServer.Mid.General;
using Microsoft.Extensions.Primitives;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Net;

namespace GRYLibrary.Tests.Testcases.APIServer.Mid.General
{
    [TestClass]
    public class GeneralMiddlewareTests
    {
        [TestMethod]
        [TestProperty(nameof(GRYLibrary.Core.Misc.TestKind), nameof(GRYLibrary.Core.Misc.TestKind.UnitTest))]
        public void GetClientIPAddressFromForwardedForHeaderReturnsSingleAddress()
        {
            //arrange
            StringValues headerValue = new StringValues("203.0.113.7");

            //act
            IPAddress result = GeneralMiddleware<object>.GetClientIPAddressFromForwardedForHeader(headerValue);

            //assert
            Assert.AreEqual(IPAddress.Parse("203.0.113.7"), result);
        }

        [TestMethod]
        [TestProperty(nameof(GRYLibrary.Core.Misc.TestKind), nameof(GRYLibrary.Core.Misc.TestKind.UnitTest))]
        public void GetClientIPAddressFromForwardedForHeaderReturnsLastAddressOfList()
        {
            //arrange
            StringValues headerValue = new StringValues("198.51.100.1, 203.0.113.7");

            //act
            IPAddress result = GeneralMiddleware<object>.GetClientIPAddressFromForwardedForHeader(headerValue);

            //assert
            Assert.AreEqual(IPAddress.Parse("203.0.113.7"), result);
        }

        [TestMethod]
        [TestProperty(nameof(GRYLibrary.Core.Misc.TestKind), nameof(GRYLibrary.Core.Misc.TestKind.UnitTest))]
        public void GetClientIPAddressFromForwardedForHeaderReturnsLastAddressOfLastHeader()
        {
            //arrange
            StringValues headerValue = new StringValues(new string[] { "198.51.100.1", "203.0.113.7" });

            //act
            IPAddress result = GeneralMiddleware<object>.GetClientIPAddressFromForwardedForHeader(headerValue);

            //assert
            Assert.AreEqual(IPAddress.Parse("203.0.113.7"), result);
        }

        [TestMethod]
        [TestProperty(nameof(GRYLibrary.Core.Misc.TestKind), nameof(GRYLibrary.Core.Misc.TestKind.UnitTest))]
        public void GetClientIPAddressFromForwardedForHeaderReturnsIPv6Address()
        {
            //arrange
            StringValues headerValue = new StringValues("2001:db8::1");

            //act
            IPAddress result = GeneralMiddleware<object>.GetClientIPAddressFromForwardedForHeader(headerValue);

            //assert
            Assert.AreEqual(IPAddress.Parse("2001:db8::1"), result);
        }

        [TestMethod]
        [TestProperty(nameof(GRYLibrary.Core.Misc.TestKind), nameof(GRYLibrary.Core.Misc.TestKind.UnitTest))]
        public void GetClientIPAddressFromForwardedForHeaderThrowsForInvalidLastAddress()
        {
            //arrange
            StringValues headerValue = new StringValues("203.0.113.7, not-an-ip-address");

            //act
            void action() => GeneralMiddleware<object>.GetClientIPAddressFromForwardedForHeader(headerValue);

            //assert
            Assert.ThrowsExactly<FormatException>(action);
        }
    }
}
