using System.Net;
using System.Text;
using System.Text.Json;
using MinimalApi.Domain.ModelViews;
using MinimalApi.DTOs;
using Test.Helpers;
using Test.Mocks;

namespace Test.Requests;

[TestClass]
public class AdminRequestsTest
{
    [ClassInitialize]
    public static void ClassInit(TestContext testContext)
    {
        Setup.ClassInit(testContext);
    }
    [TestMethod]
    public async Task TestGetSetProperties()
    {
        var loginDTO = new LoginDTO
        {
            Email = "adm@teste.com",
            Senha = "123456"
        };

        var content = new StringContent(JsonSerializer.Serialize(loginDTO), Encoding.UTF8, "Application/json");

        var response = await Setup.client.PostAsync("/admins/login", content);

        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);

        var result = await response.Content.ReadAsStringAsync();
        var admLogado = JsonSerializer.Deserialize<AdmLogado>(result, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        Assert.IsNotNull(admLogado?.Email ?? "");
        Assert.IsNotNull(admLogado?.Perfil ?? "Editor");
        Assert.IsNotNull(admLogado?.Token ?? "");
    }
}