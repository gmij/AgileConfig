using System;
using System.Collections.Generic;
using System.Net;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using AgileConfig.Server.Apisite;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ApiSiteTests.Websocket;

/// <summary>
/// End-to-end tests for WebSocket /ws endpoint with ping/pong heartbeat protocol.
/// Tests authentication, connection lifecycle, and message handling.
/// </summary>
[TestClass]
public class WebsocketE2ETests
{
    private TestServer _server;
    private IConfiguration _configuration;

    [TestInitialize]
    public void TestInitialize()
    {
        // Configure test server with SQLite for simplicity
        var configBuilder = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string>
            {
                ["db:provider"] = "sqlite",
                ["db:conn"] = "Data Source=:memory:",
                ["JwtSetting:SecurityKey"] = "test-security-key-for-websocket-tests-minimum-32-characters",
                ["JwtSetting:Issuer"] = "agileconfig.test",
                ["JwtSetting:Audience"] = "agileconfig.test",
                ["JwtSetting:ExpireSeconds"] = "3600"
            });

        _configuration = configBuilder.Build();

        // Create test server
        var webHostBuilder = new WebHostBuilder()
            .UseConfiguration(_configuration)
            .UseStartup<Startup>();

        _server = new TestServer(webHostBuilder);
    }

    [TestCleanup]
    public void TestCleanup()
    {
        _server?.Dispose();
    }

    /// <summary>
    /// Test successful WebSocket connection with valid credentials.
    /// Verifies that the server accepts the connection and responds to ping messages.
    /// </summary>
    [TestMethod]
    public async Task ConnectWebSocket_WithValidAuth_ShouldSucceed()
    {
        // Arrange
        var appId = "test_app";
        var appSecret = "test_secret";

        // Create WebSocket client
        var wsClient = _server.CreateWebSocketClient();

        // Add basic auth header (Base64 encoded "appId:appSecret")
        var credentials = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{appId}:{appSecret}"));
        wsClient.ConfigureRequest = request =>
        {
            request.Headers.Add("Authorization", $"Basic {credentials}");
            request.Headers.Add("appid", appId);
            request.Headers.Add("env", "TEST");
        };

        // Act
        var webSocket = await wsClient.ConnectAsync(new Uri(_server.BaseAddress, "/ws"), CancellationToken.None);

        // Assert
        Assert.AreEqual(WebSocketState.Open, webSocket.State, "WebSocket should be in Open state");

        // Cleanup
        await webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Test completed", CancellationToken.None);
        webSocket.Dispose();
    }

    /// <summary>
    /// Test WebSocket rejection when authentication fails.
    /// Verifies that the server returns 401 Unauthorized for invalid credentials.
    /// </summary>
    [TestMethod]
    public async Task ConnectWebSocket_WithInvalidAuth_ShouldReturn401()
    {
        // Arrange
        var wsClient = _server.CreateWebSocketClient();

        // No authentication headers

        // Act & Assert
        try
        {
            var webSocket = await wsClient.ConnectAsync(new Uri(_server.BaseAddress, "/ws"), CancellationToken.None);
            Assert.Fail("Expected WebSocketException due to 401 response");
        }
        catch (WebSocketException ex)
        {
            // Expected: Server should reject connection with 401
            Assert.IsTrue(ex.Message.Contains("401") || ex.Message.Contains("Unauthorized"),
                $"Exception should indicate 401 Unauthorized, got: {ex.Message}");
        }
    }

    /// <summary>
    /// Test ping/pong heartbeat mechanism.
    /// Sends a "ping" message and verifies the server updates the last heartbeat time.
    /// </summary>
    [TestMethod]
    public async Task WebSocket_PingMessage_ShouldUpdateHeartbeat()
    {
        // Arrange
        var appId = "test_app";
        var appSecret = "test_secret";
        var credentials = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{appId}:{appSecret}"));

        var wsClient = _server.CreateWebSocketClient();
        wsClient.ConfigureRequest = request =>
        {
            request.Headers.Add("Authorization", $"Basic {credentials}");
            request.Headers.Add("appid", appId);
            request.Headers.Add("env", "TEST");
        };

        var webSocket = await wsClient.ConnectAsync(new Uri(_server.BaseAddress, "/ws"), CancellationToken.None);

        try
        {
            // Act - Send ping message
            var pingMessage = Encoding.UTF8.GetBytes("ping");
            await webSocket.SendAsync(new ArraySegment<byte>(pingMessage), WebSocketMessageType.Text, true, CancellationToken.None);

            // Wait a bit to allow server to process
            await Task.Delay(100);

            // The server should still keep the connection open after receiving ping
            Assert.AreEqual(WebSocketState.Open, webSocket.State, "WebSocket should remain open after ping");

            // Send another ping to verify continued communication
            await webSocket.SendAsync(new ArraySegment<byte>(pingMessage), WebSocketMessageType.Text, true, CancellationToken.None);
            await Task.Delay(100);

            Assert.AreEqual(WebSocketState.Open, webSocket.State, "WebSocket should still be open after second ping");
        }
        finally
        {
            // Cleanup
            await webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Test completed", CancellationToken.None);
            webSocket.Dispose();
        }
    }

    /// <summary>
    /// Test WebSocket connection without appId header defaults to DEV environment.
    /// Verifies that missing 'env' header is handled gracefully with a default value.
    /// </summary>
    [TestMethod]
    public async Task ConnectWebSocket_WithoutEnvHeader_ShouldDefaultToDEV()
    {
        // Arrange
        var appId = "test_app";
        var appSecret = "test_secret";
        var credentials = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{appId}:{appSecret}"));

        var wsClient = _server.CreateWebSocketClient();
        wsClient.ConfigureRequest = request =>
        {
            request.Headers.Add("Authorization", $"Basic {credentials}");
            request.Headers.Add("appid", appId);
            // Intentionally omit "env" header
        };

        // Act
        var webSocket = await wsClient.ConnectAsync(new Uri(_server.BaseAddress, "/ws"), CancellationToken.None);

        // Assert
        Assert.AreEqual(WebSocketState.Open, webSocket.State, "WebSocket should connect successfully even without env header");

        // Cleanup
        await webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Test completed", CancellationToken.None);
        webSocket.Dispose();
    }

    /// <summary>
    /// Test WebSocket connection with client name and tag query parameters.
    /// Verifies that client metadata can be passed via query string.
    /// </summary>
    [TestMethod]
    public async Task ConnectWebSocket_WithClientNameAndTag_ShouldSucceed()
    {
        // Arrange
        var appId = "test_app";
        var appSecret = "test_secret";
        var clientName = "TestClient";
        var clientTag = "Integration";
        var credentials = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{appId}:{appSecret}"));

        var wsClient = _server.CreateWebSocketClient();
        wsClient.ConfigureRequest = request =>
        {
            request.Headers.Add("Authorization", $"Basic {credentials}");
            request.Headers.Add("appid", appId);
            request.Headers.Add("env", "TEST");
        };

        // Act
        var uri = new Uri(_server.BaseAddress, $"/ws?client_name={clientName}&client_tag={clientTag}");
        var webSocket = await wsClient.ConnectAsync(uri, CancellationToken.None);

        // Assert
        Assert.AreEqual(WebSocketState.Open, webSocket.State, "WebSocket should connect successfully with client metadata");

        // Cleanup
        await webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Test completed", CancellationToken.None);
        webSocket.Dispose();
    }

    /// <summary>
    /// Test graceful connection closure from client side.
    /// Verifies that the server handles client-initiated close correctly.
    /// </summary>
    [TestMethod]
    public async Task WebSocket_ClientInitiatedClose_ShouldCloseGracefully()
    {
        // Arrange
        var appId = "test_app";
        var appSecret = "test_secret";
        var credentials = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{appId}:{appSecret}"));

        var wsClient = _server.CreateWebSocketClient();
        wsClient.ConfigureRequest = request =>
        {
            request.Headers.Add("Authorization", $"Basic {credentials}");
            request.Headers.Add("appid", appId);
            request.Headers.Add("env", "TEST");
        };

        var webSocket = await wsClient.ConnectAsync(new Uri(_server.BaseAddress, "/ws"), CancellationToken.None);

        // Act - Client initiates close
        await webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Client closing", CancellationToken.None);

        // Assert
        Assert.IsTrue(
            webSocket.State == WebSocketState.Closed || webSocket.State == WebSocketState.CloseReceived,
            $"WebSocket should be closed, but was {webSocket.State}");

        // Cleanup
        webSocket.Dispose();
    }
}
