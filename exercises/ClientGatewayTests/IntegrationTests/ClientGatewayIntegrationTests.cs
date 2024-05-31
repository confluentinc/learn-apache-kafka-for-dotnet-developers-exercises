using System.Text;
using ClientGateway.Domain;
using Newtonsoft.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.VisualStudio.TestPlatform.TestHost; 

[TestFixture]
public class ClientGatewayControllerTests 
{
    private WebApplicationFactory<Program> _factory;
    private HttpClient _client;

    [SetUp]
    public void Setup()
    {
        // Set up the WebApplicationFactory and client
        _factory = new WebApplicationFactory<Program>();
        _client = _factory.CreateClient();
    }
 
    [Test]
    public async Task PostBioMetrics_ReturnsSuccessStatusCode()
    {
        // Arrange
        var bioMetrics = new BioMetrics(Guid.NewGuid(), new List<HeartRate>(), new List<StepCount>(), 0);
        var json = JsonConvert.SerializeObject(bioMetrics);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        // Act
        var response = await _client.PostAsync("/ClientGateway/biometrics", content);

        // Assert
        response.EnsureSuccessStatusCode();
    }

    [TearDown]
    public void TearDown()
    {
        // Dispose of the test server and client
        _factory.Dispose();
        _client = null;
    }
}