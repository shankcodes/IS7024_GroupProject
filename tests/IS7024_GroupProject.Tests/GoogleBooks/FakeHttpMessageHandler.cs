using System.Net;
using System.Text;

namespace IS7024_GroupProject.Tests;

// A pretend "internet" for unit tests. Instead of calling Google Books,
// HttpClient asks this handler, which returns whatever the test told it to.
// It also remembers every request so tests can check the URL or that no call was made.
public class FakeHttpMessageHandler : HttpMessageHandler
{
    private readonly HttpStatusCode _statusCode;
    private readonly string _responseJson;
    private readonly Exception? _exceptionToThrow;

    public List<HttpRequestMessage> Requests { get; } = new();

    // Returns the given JSON with the given status code (200 OK by default).
    public FakeHttpMessageHandler(string responseJson, HttpStatusCode statusCode = HttpStatusCode.OK)
    {
        _responseJson = responseJson;
        _statusCode = statusCode;
    }

    // Throws the given exception, e.g. to pretend there was a timeout.
    public FakeHttpMessageHandler(Exception exceptionToThrow)
    {
        _responseJson = string.Empty;
        _exceptionToThrow = exceptionToThrow;
    }

    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        Requests.Add(request);

        if (_exceptionToThrow != null)
        {
            throw _exceptionToThrow;
        }

        var response = new HttpResponseMessage(_statusCode)
        {
            Content = new StringContent(_responseJson, Encoding.UTF8, "application/json")
        };
        return Task.FromResult(response);
    }
}
