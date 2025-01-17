[![.NET Build and Test](https://github.com/ardalis/HttpClientTestExtensions/workflows/.NET%20Build%20and%20Test/badge.svg)](https://github.com/ardalis/HttpClientTestExtensions/actions?query=workflow%3A%22.NET+Build+and+Test%22)
[![Nuget](https://img.shields.io/nuget/v/Ardalis.HttpClientTestExtensions)](https://www.nuget.org/packages/Ardalis.HttpClientTestExtensions/)
[![Nuget](https://img.shields.io/nuget/dt/Ardalis.HttpClientTestExtensions)](https://www.nuget.org/packages/Ardalis.HttpClientTestExtensions/)

# HttpClient Test Extensions

Extensions for testing HTTP endpoints and deserializing the results. Currently works with XUnit.

## Installation

Add the NuGet package:

```powershell
dotnet add package Ardalis.HttpClientTestExtensions
```

In your tests add this namespace:

```csharp
using Ardalis.HttpClientTestExtensions;
```

## Usage

If you have existing test code that looks something like this:

```csharp
public class DoctorsList : IClassFixture<CustomWebApplicationFactory<Startup>>
{
  private readonly HttpClient _client;
  private readonly ITestOutputHelper _outputHelper;

  public DoctorsList(CustomWebApplicationFactory<Startup> factory,
    ITestOutputHelper outputHelper)
  {
    _client = factory.CreateClient();
    _outputHelper = outputHelper;
  }

  [Fact]
  public async Task Returns3Doctors()
  {
    var response = await _client.GetAsync("/api/doctors");
    response.EnsureSuccessStatusCode();
    var stringResponse = await response.Content.ReadAsStringAsync();
    _outputHelper.WriteLine(stringResponse);
    var result = JsonSerializer.Deserialize<ListDoctorResponse>(stringResponse,
      Constants.DefaultJsonOptions);

    Assert.Equal(3, result.Doctors.Count());
    Assert.Contains(result.Doctors, x => x.Name == "Dr. Smith");
  }
}
```

You can now update the test to eliminate all but one of the lines prior to the assertions:

```csharp
[Fact]
public async Task Returns3Doctors()
{
  var result = await _client.GetAndDeserialize<ListDoctorResponse>("/api/doctors", _outputHelper);

  Assert.Equal(3, result.Doctors.Count());
  Assert.Contains(result.Doctors, x => x.Name == "Dr. Smith");
}
```

If you need to verify an endpoint returns a 404, you can use this approach:

```csharp
[Fact]
public async Task ReturnsNotFoundGivenInvalidAuthorId()
{
  int invalidId = 9999;

  var response = await _client.GetAsync(Routes.Authors.Get(invalidId));

  response.EnsureNotFound();
}
```

## List of Included Helper Methods

All methods are extensions on `HttpClient`; the following samples assume `client` is an `HttpClient`. All methods take an optional `ITestOutputHelper`, which is an xUnit type.

```csharp
// GET and return an object T
AuthorDto result = await client.GetAndDeserializeAsync("/authors/1", _testOutputHelper);

// GET and assert a 404 is returned
await client.GetAndEnsureNotFoundAsync("/authors/-1");

// GET and return response as a string
string result = client.GetAndReturnStringAsync("/healthcheck");

// POST and assert a 404 is returned

var content = new StringContent(JsonSerializer.Serialize(dto), Encoding.UTF8, "application/json");
await client.PostAndEnsureNotFoundAsync("/wrongendpoint", content)
```

## Notes

- For now this is coupled with xUnit but if there is interest it could be split so the ITestOutputHelper dependency is removed/optional/swappable
- Additional helpers for other verbs are planned
- This is using System.Text.Json with default camelCase options that I've found most useful in my projects. This could be made extensible somehow as well.

