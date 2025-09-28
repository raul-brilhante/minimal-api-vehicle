using MinimalApi.Domain.Entities;

namespace Test.Domain.Entities;

[TestClass]
public class AdminTest
{
    [TestMethod]
    public void TestGetSetProperties()
    {
        var adm = new Admin();

        adm.Id = 1;
        adm.Email = "teste@teste.com";
        adm.Senha = "testesenha";
        adm.Perfil = "Adm";

        Assert.AreEqual(1, adm.Id);
        Assert.AreEqual("teste@teste.com", adm.Email);
        Assert.AreEqual("testesenha", adm.Senha);
        Assert.AreEqual("Adm", adm.Perfil);
    }
}