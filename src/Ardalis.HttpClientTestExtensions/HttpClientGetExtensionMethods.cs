﻿using System;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using Xunit.Abstractions;

namespace Ardalis.HttpClientTestExtensions;

public static partial class HttpClientGetExtensionMethods
{
  public static async Task<T> GetAndDeserializeAsync<T>(this HttpClient client, string requestUri, ITestOutputHelper output = null)
  {
    output?.WriteLine($"Requesting with GET {requestUri}");
    var response = await client.GetAsync(requestUri);
    response.EnsureSuccessStatusCode();
    var stringResponse = await response.Content.ReadAsStringAsync();
    output?.WriteLine($"Response: {stringResponse}");
    var result = JsonSerializer.Deserialize<T>(stringResponse,
      Constants.DefaultJsonOptions);

    return result;
  }

  public static async Task<HttpResponseMessage> GetAndEnsureNotFoundAsync(this HttpClient client, string requestUri, ITestOutputHelper output = null)
  {
    output?.WriteLine($"Requesting with GET {requestUri}");
    var response = await client.GetAsync(requestUri);
    response.EnsureNotFound();
    return response;
  }

  public static async Task<string> GetAndReturnStringAsync(this HttpClient client, string requestUri, ITestOutputHelper output = null)
  {
    output?.WriteLine($"Requesting with GET {requestUri}");
    var response = await client.GetAsync(requestUri);
    return await response.Content.ReadAsStringAsync();
  }

  public static async Task<string> GetAndEnsureSubstringAsync(this HttpClient client, string requestUri, string substring, ITestOutputHelper output = null)
  {
    var responseString = await client.GetAndReturnStringAsync(requestUri, output);
    if (!responseString.Contains(substring))
    {
      output?.WriteLine($"Throwing exception because expected substring \"{substring}\" not found in response \"{responseString}\"");
      throw new HttpRequestException($"Expected substring \"{substring}\" not found in response \"{responseString}\"");
    };

    return responseString;
  }
}