using System.Net;
using System.Text.Json;
using InventoryService.Contracts;
using Microsoft.AspNetCore.WebUtilities;

namespace OrderService;

public class InventoryApiClient
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<InventoryApiClient> _logger;
    
    private static readonly (string name, string value)[] NoHeaders = Array.Empty<(string name, string value)>();
    private static readonly (string name, string? value)[] NoParameters = Array.Empty<(string name, string? value)>();
    private static readonly HttpContent EmptyHttpContent = new StringContent(string.Empty);
  
    public InventoryApiClient(HttpClient httpClient, ILogger<InventoryApiClient> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task<bool> ReserveItems(string itemName, int numberOfItems, Guid orderId, CancellationToken cancellationToken)
    {
        var requestContent = new ReserveItemsRequest
        {
            ItemName = itemName,
            NumberOfItems = numberOfItems,
            OrderId = orderId
        };

        var (status, responseContent) = await SendInternal<ReserveItemsRequest, ReserveItemsResponse>(
            HttpMethod.Post, "reserve-items", NoParameters, NoHeaders, requestContent, cancellationToken);

        if (IsSuccessStatusCode(status))
        {
            _logger.LogInformation("Call to InventoryService->ReserveItems was successfull: {StatusCode}", status);
            return true;
        }
        else
        {
            _logger.LogWarning("Call to InventoryService->ReserveItems failed: {StatusCode} {RequestContent} {ResponseContent}", status, requestContent, responseContent);
            return false;
        }
    }

    private async Task<(HttpStatusCode responseStatus, TY responseContent)> SendInternal<T, TY>(
        HttpMethod httpMethod, string path, (string name, string? value)[] parameters, (string name, string value)[] headers, 
        T requestContent, CancellationToken cancellationToken, bool checkResponseStatus = true)
        where TY: new()
    {
        var requestMessageContent = JsonContent.Create(requestContent);
        
        var (responseStatus, responseContent) = await SendInternal(httpMethod, path, parameters, headers, requestMessageContent, cancellationToken, checkResponseStatus);

        if (string.IsNullOrWhiteSpace(responseContent))
        {
            return (responseStatus, IDefineEmpty<TY>.Empty);
        }

        var deserializedResponse = JsonSerializer.Deserialize<TY>(responseContent);
        if (deserializedResponse != null)
        {
            return (responseStatus, deserializedResponse);
        }
        else
        {
            _logger.LogError("Could not deserialize response: {responseContent}", responseContent);
            throw new JsonException();
        }
    }
    
    private async Task<(HttpStatusCode responseStatus, string responseContent)> SendInternal(HttpMethod httpMethod, string path, (string name, string? value)[] parameters, (string name, string value)[] headers, HttpContent requestContent, CancellationToken cancellationToken, bool checkResponseStatus = true)
    {
        var uri = CreateUriWithPathAndParameters(_httpClient.BaseAddress!.ToString(), path, parameters);

        using (var requestMessage = new HttpRequestMessage(httpMethod, uri))
        {
            AddHeadersToRequest(requestMessage, headers);
            AddContentToRequest(requestMessage, requestContent);

            var httpResponseMessage = await _httpClient.SendAsync(requestMessage, cancellationToken).ConfigureAwait(false);
            var responseContent = await httpResponseMessage.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);

            if (!checkResponseStatus || checkResponseStatus && httpResponseMessage.IsSuccessStatusCode)
            {
                return (responseStatus: httpResponseMessage.StatusCode, responseContent);
            }
            else
            {
                _logger.LogWarning("{HttpVerb} request to {RequestUrl} returns statusCode:{ResponseStatusCode} content:{ResponseContent}", requestMessage.Method,
                    requestMessage.RequestUri, httpResponseMessage.StatusCode, responseContent);
                return (responseStatus: httpResponseMessage.StatusCode, responseContent: responseContent);
            }
        }
    }
    
    private string CreateUriWithPathAndParameters(string settingsHubSpotApiBaseUrl, string path, (string name, string? value)[] parameters)
    {
        var uriBuilder = new UriBuilder(settingsHubSpotApiBaseUrl);
        uriBuilder.Path += path;

        var query = new Dictionary<string, string?>();
        foreach (var (name, value) in parameters)
        {
            query[name] = value;
        }

        var uriWithQuery = QueryHelpers.AddQueryString(uriBuilder.Uri.AbsoluteUri, query);
        return uriWithQuery;
    }
    
    private static void AddContentToRequest(HttpRequestMessage requestMessage, HttpContent requestContent)
    {
        if (requestContent != EmptyHttpContent)
            requestMessage.Content = requestContent;
    }

    private static void AddHeadersToRequest(HttpRequestMessage requestMessage, (string name, string value)[] headers)
    {
        foreach (var (name, value) in headers)
        {
            requestMessage.Headers.Add(name, value);
        }
    }
    
    private bool IsSuccessStatusCode(HttpStatusCode statusCode) => (int)statusCode is >= 200 and < 300;
}