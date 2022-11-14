using Xunit;
using Shouldly;
using KombitStsUtilities;
using System.IO;
using System;

namespace KombitStsUtilityTests;

public class KombitStsUtilityTests
{
    [Fact]
    public void RequestShouldBeCorrect()
    {
        var request = KombitSts.BuildRequest(new Uri("https://echo:8443/runtime/services/kombittrust/14/certificatemixed"), "http://organisation.serviceplatformen.dk/service/organisation/5");
        var expected = File.ReadAllText("Request.xml");
        request.ShouldBe(expected);
    }
}