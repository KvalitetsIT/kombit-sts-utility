using Xunit;
using Shouldly;
using KombitStsUtility;
using System.IO;
using System.Threading.Tasks;
using System;

namespace KombitStsUtilityTests;

public class KombitStsUtilityTests
{
    [Fact]
    public async Task RequestShouldBeCorrect()
    {
        var request = new KombitStsRequest(audience: "http://organisation.serviceplatformen.dk/service/organisation/5", "TODO")
        {
            WsAddressingTo = "https://echo:8443/runtime/services/kombittrust/14/certificatemixed",
            Action = "http://docs.oasis-open.org/ws-sx/ws-trust/200512/RST/Issue",
        }
        .ToString();
        await File.WriteAllTextAsync("GeneratedRequest.xml", request);
        var expected = File.ReadAllText("Request.xml");
        request.ShouldBe(expected);
    }
}