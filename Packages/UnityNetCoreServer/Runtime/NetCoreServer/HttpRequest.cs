using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace NetCoreServer
{
  /// <summary>
  ///   HTTP request is used to create or process parameters of HTTP protocol request(method, URL, headers, etc).
  /// </summary>
  /// <remarks>Not thread-safe.</remarks>
  public class HttpRequest
  {
    // HTTP request cookies
    private readonly List<(string, string)> _cookies = new();

    // HTTP request headers
    private readonly List<(string, string)> _headers = new();

    // HTTP request body
    private int _bodyIndex;
    private int _bodyLength;
    private bool _bodyLengthProvided;
    private int _bodySize;

    // HTTP request cache

    private int _cacheSize;

    // HTTP request method

    // HTTP request protocol

    // HTTP request URL

    /// <summary>
    ///   Initialize an empty HTTP request
    /// </summary>
    public HttpRequest()
    {
      Clear();
    }

    /// <summary>
    ///   Initialize a new HTTP request with a given method, URL and protocol
    /// </summary>
    /// <param name="method">HTTP method</param>
    /// <param name="url">Requested URL</param>
    /// <param name="protocol">Protocol version (default is "HTTP/1.1")</param>
    public HttpRequest(string method, string url, string protocol = "HTTP/1.1")
    {
      SetBegin(method, url, protocol);
    }

    /// <summary>
    ///   Is the HTTP request empty?
    /// </summary>
    public bool IsEmpty => Cache.Size == 0;

    /// <summary>
    ///   Is the HTTP request error flag set?
    /// </summary>
    public bool IsErrorSet { get; private set; }

    /// <summary>
    ///   Get the HTTP request method
    /// </summary>
    public string Method { get; private set; } = string.Empty;

    /// <summary>
    ///   Get the HTTP request URL
    /// </summary>
    public string Url { get; private set; } = string.Empty;

    /// <summary>
    ///   Get the HTTP request protocol version
    /// </summary>
    public string Protocol { get; private set; } = string.Empty;

    /// <summary>
    ///   Get the HTTP request headers count
    /// </summary>
    public long Headers => _headers.Count;

    /// <summary>
    ///   Get the HTTP request cookies count
    /// </summary>
    public long Cookies => _cookies.Count;

    /// <summary>
    ///   Get the HTTP request body as string
    /// </summary>
    public string Body => Cache.ExtractString(_bodyIndex, _bodySize);

    /// <summary>
    ///   Get the HTTP request body as byte array
    /// </summary>
    public byte[] BodyBytes => Cache.Data[_bodyIndex..(_bodyIndex + _bodySize)];

    /// <summary>
    ///   Get the HTTP request body as byte span
    /// </summary>
    public Span<byte> BodySpan => new(Cache.Data, _bodyIndex, _bodySize);

    /// <summary>
    ///   Get the HTTP request body length
    /// </summary>
    public long BodyLength => _bodyLength;

    /// <summary>
    ///   Get the HTTP request cache content
    /// </summary>
    public Buffer Cache { get; } = new();

    /// <summary>
    ///   Get the HTTP request header by index
    /// </summary>
    public (string, string) Header(int i)
    {
      Debug.Assert(i < _headers.Count, "Index out of bounds!");
      if (i >= _headers.Count)
        return ("", "");

      return _headers[i];
    }

    /// <summary>
    ///   Get the HTTP request cookie by index
    /// </summary>
    public (string, string) Cookie(int i)
    {
      Debug.Assert(i < _cookies.Count, "Index out of bounds!");
      if (i >= _cookies.Count)
        return ("", "");

      return _cookies[i];
    }

    /// <summary>
    ///   Get string from the current HTTP request
    /// </summary>
    public override string ToString()
    {
      var sb = new StringBuilder();
      sb.AppendLine($"Request method: {Method}");
      sb.AppendLine($"Request URL: {Url}");
      sb.AppendLine($"Request protocol: {Protocol}");
      sb.AppendLine($"Request headers: {Headers}");
      for (var i = 0; i < Headers; i++)
      {
        var header = Header(i);
        sb.AppendLine($"{header.Item1} : {header.Item2}");
      }

      sb.AppendLine($"Request body: {BodyLength}");
      sb.AppendLine(Body);
      return sb.ToString();
    }

    /// <summary>
    ///   Clear the HTTP request cache
    /// </summary>
    public HttpRequest Clear()
    {
      IsErrorSet = false;
      Method = "";
      Url = "";
      Protocol = "";
      _headers.Clear();
      _cookies.Clear();
      _bodyIndex = 0;
      _bodySize = 0;
      _bodyLength = 0;
      _bodyLengthProvided = false;

      Cache.Clear();
      _cacheSize = 0;
      return this;
    }

    /// <summary>
    ///   Set the HTTP request begin with a given method, URL and protocol
    /// </summary>
    /// <param name="method">HTTP method</param>
    /// <param name="url">Requested URL</param>
    /// <param name="protocol">Protocol version (default is "HTTP/1.1")</param>
    public HttpRequest SetBegin(string method, string url, string protocol = "HTTP/1.1")
    {
      // Clear the HTTP request cache
      Clear();

      // Append the HTTP request method
      Cache.Append(method);
      Method = method;

      Cache.Append(" ");

      // Append the HTTP request URL
      Cache.Append(url);
      Url = url;

      Cache.Append(" ");

      // Append the HTTP request protocol version
      Cache.Append(protocol);
      Protocol = protocol;

      Cache.Append("\r\n");
      return this;
    }

    /// <summary>
    ///   Set the HTTP request header
    /// </summary>
    /// <param name="key">Header key</param>
    /// <param name="value">Header value</param>
    public HttpRequest SetHeader(string key, string value)
    {
      // Append the HTTP request header's key
      Cache.Append(key);

      Cache.Append(": ");

      // Append the HTTP request header's value
      Cache.Append(value);

      Cache.Append("\r\n");

      // Add the header to the corresponding collection
      _headers.Add((key, value));
      return this;
    }

    /// <summary>
    ///   Set the HTTP request cookie
    /// </summary>
    /// <param name="name">Cookie name</param>
    /// <param name="value">Cookie value</param>
    public HttpRequest SetCookie(string name, string value)
    {
      const string key = "Cookie";
      var cookie = name + "=" + value;

      // Append the HTTP request header's key
      Cache.Append(key);

      Cache.Append(": ");

      // Append Cookie
      Cache.Append(cookie);

      Cache.Append("\r\n");

      // Add the header to the corresponding collection
      _headers.Add((key, cookie));
      // Add the cookie to the corresponding collection
      _cookies.Add((name, value));
      return this;
    }

    /// <summary>
    ///   Add the HTTP request cookie
    /// </summary>
    /// <param name="name">Cookie name</param>
    /// <param name="value">Cookie value</param>
    public HttpRequest AddCookie(string name, string value)
    {
      // Append Cookie
      Cache.Append("; ");
      Cache.Append(name);
      Cache.Append("=");
      Cache.Append(value);

      // Add the cookie to the corresponding collection
      _cookies.Add((name, value));
      return this;
    }

    /// <summary>
    ///   Set the HTTP request body
    /// </summary>
    /// <param name="body">Body string content (default is "")</param>
    public HttpRequest SetBody(string body = "")
    {
      return SetBody(body.AsSpan());
    }

    /// <summary>
    ///   Set the HTTP request body
    /// </summary>
    /// <param name="body">Body string content as a span of characters</param>
    public HttpRequest SetBody(ReadOnlySpan<char> body)
    {
      var length = body.IsEmpty ? 0 : Encoding.UTF8.GetByteCount(body);

      // Append content length header
      SetHeader("Content-Length", length.ToString());

      Cache.Append("\r\n");

      var index = (int)Cache.Size;

      // Append the HTTP request body
      Cache.Append(body);
      _bodyIndex = index;
      _bodySize = length;
      _bodyLength = length;
      _bodyLengthProvided = true;
      return this;
    }

    /// <summary>
    ///   Set the HTTP request body
    /// </summary>
    /// <param name="body">Body binary content</param>
    public HttpRequest SetBody(byte[] body)
    {
      return SetBody(body.AsSpan());
    }

    /// <summary>
    ///   Set the HTTP request body
    /// </summary>
    /// <param name="body">Body binary content as a span of bytes</param>
    public HttpRequest SetBody(ReadOnlySpan<byte> body)
    {
      // Append content length header
      SetHeader("Content-Length", body.Length.ToString());

      Cache.Append("\r\n");

      var index = (int)Cache.Size;

      // Append the HTTP request body
      Cache.Append(body);
      _bodyIndex = index;
      _bodySize = body.Length;
      _bodyLength = body.Length;
      _bodyLengthProvided = true;
      return this;
    }

    /// <summary>
    ///   Set the HTTP request body length
    /// </summary>
    /// <param name="length">Body length</param>
    public HttpRequest SetBodyLength(int length)
    {
      // Append content length header
      SetHeader("Content-Length", length.ToString());

      Cache.Append("\r\n");

      var index = (int)Cache.Size;

      // Clear the HTTP request body
      _bodyIndex = index;
      _bodySize = 0;
      _bodyLength = length;
      _bodyLengthProvided = true;
      return this;
    }

    /// <summary>
    ///   Make HEAD request
    /// </summary>
    /// <param name="url">URL to request</param>
    public HttpRequest MakeHeadRequest(string url)
    {
      Clear();
      SetBegin("HEAD", url);
      SetBody();
      return this;
    }

    /// <summary>
    ///   Make GET request
    /// </summary>
    /// <param name="url">URL to request</param>
    public HttpRequest MakeGetRequest(string url)
    {
      Clear();
      SetBegin("GET", url);
      SetBody();
      return this;
    }

    /// <summary>
    ///   Make POST request
    /// </summary>
    /// <param name="url">URL to request</param>
    /// <param name="content">String content</param>
    /// <param name="contentType">Content type (default is "text/plain; charset=UTF-8")</param>
    public HttpRequest MakePostRequest(string url, string content, string contentType = "text/plain; charset=UTF-8")
    {
      return MakePostRequest(url, content.AsSpan(), contentType);
    }

    /// <summary>
    ///   Make POST request
    /// </summary>
    /// <param name="url">URL to request</param>
    /// <param name="content">String content as a span of characters</param>
    /// <param name="contentType">Content type (default is "text/plain; charset=UTF-8")</param>
    public HttpRequest MakePostRequest(string url, ReadOnlySpan<char> content,
      string contentType = "text/plain; charset=UTF-8")
    {
      Clear();
      SetBegin("POST", url);
      if (!string.IsNullOrEmpty(contentType))
        SetHeader("Content-Type", contentType);
      SetBody(content);
      return this;
    }

    /// <summary>
    ///   Make POST request
    /// </summary>
    /// <param name="url">URL to request</param>
    /// <param name="content">Binary content</param>
    /// <param name="contentType">Content type (default is "")</param>
    public HttpRequest MakePostRequest(string url, byte[] content, string contentType = "")
    {
      return MakePostRequest(url, content.AsSpan(), contentType);
    }

    /// <summary>
    ///   Make POST request
    /// </summary>
    /// <param name="url">URL to request</param>
    /// <param name="content">Binary content as a span of bytes</param>
    /// <param name="contentType">Content type (default is "")</param>
    public HttpRequest MakePostRequest(string url, ReadOnlySpan<byte> content, string contentType = "")
    {
      Clear();
      SetBegin("POST", url);
      if (!string.IsNullOrEmpty(contentType))
        SetHeader("Content-Type", contentType);
      SetBody(content);
      return this;
    }

    /// <summary>
    ///   Make PUT request
    /// </summary>
    /// <param name="url">URL to request</param>
    /// <param name="content">String content</param>
    /// <param name="contentType">Content type (default is "text/plain; charset=UTF-8")</param>
    public HttpRequest MakePutRequest(string url, string content, string contentType = "text/plain; charset=UTF-8")
    {
      return MakePutRequest(url, content.AsSpan(), contentType);
    }

    /// <summary>
    ///   Make PUT request
    /// </summary>
    /// <param name="url">URL to request</param>
    /// <param name="content">String content as a span of characters</param>
    /// <param name="contentType">Content type (default is "text/plain; charset=UTF-8")</param>
    public HttpRequest MakePutRequest(string url, ReadOnlySpan<char> content,
      string contentType = "text/plain; charset=UTF-8")
    {
      Clear();
      SetBegin("PUT", url);
      if (!string.IsNullOrEmpty(contentType))
        SetHeader("Content-Type", contentType);
      SetBody(content);
      return this;
    }

    /// <summary>
    ///   Make PUT request
    /// </summary>
    /// <param name="url">URL to request</param>
    /// <param name="content">Binary content</param>
    /// <param name="contentType">Content type (default is "")</param>
    public HttpRequest MakePutRequest(string url, byte[] content, string contentType = "")
    {
      return MakePutRequest(url, content.AsSpan(), contentType);
    }

    /// <summary>
    ///   Make PUT request
    /// </summary>
    /// <param name="url">URL to request</param>
    /// <param name="content">Binary content as a span of bytes</param>
    /// <param name="contentType">Content type (default is "")</param>
    public HttpRequest MakePutRequest(string url, ReadOnlySpan<byte> content, string contentType = "")
    {
      Clear();
      SetBegin("PUT", url);
      if (!string.IsNullOrEmpty(contentType))
        SetHeader("Content-Type", contentType);
      SetBody(content);
      return this;
    }

    /// <summary>
    ///   Make DELETE request
    /// </summary>
    /// <param name="url">URL to request</param>
    public HttpRequest MakeDeleteRequest(string url)
    {
      Clear();
      SetBegin("DELETE", url);
      SetBody();
      return this;
    }

    /// <summary>
    ///   Make OPTIONS request
    /// </summary>
    /// <param name="url">URL to request</param>
    public HttpRequest MakeOptionsRequest(string url)
    {
      Clear();
      SetBegin("OPTIONS", url);
      SetBody();
      return this;
    }

    /// <summary>
    ///   Make TRACE request
    /// </summary>
    /// <param name="url">URL to request</param>
    public HttpRequest MakeTraceRequest(string url)
    {
      Clear();
      SetBegin("TRACE", url);
      SetBody();
      return this;
    }

    // Is pending parts of HTTP request
    internal bool IsPendingHeader()
    {
      return !IsErrorSet && _bodyIndex == 0;
    }

    internal bool IsPendingBody()
    {
      return !IsErrorSet && _bodyIndex > 0 && _bodySize > 0;
    }

    internal bool ReceiveHeader(byte[] buffer, int offset, int size)
    {
      // Update the request cache
      Cache.Append(buffer, offset, size);

      // Try to seek for HTTP header separator
      for (var i = _cacheSize; i < (int)Cache.Size; i++)
      {
        // Check for the request cache out of bounds
        if (i + 3 >= (int)Cache.Size)
          break;

        // Check for the header separator
        if (Cache[i + 0] == '\r' && Cache[i + 1] == '\n' && Cache[i + 2] == '\r' && Cache[i + 3] == '\n')
        {
          var index = 0;

          // Set the error flag for a while...
          IsErrorSet = true;

          // Parse method
          var methodIndex = index;
          var methodSize = 0;
          while (Cache[index] != ' ')
          {
            methodSize++;
            index++;
            if (index >= (int)Cache.Size)
              return false;
          }

          index++;
          if (index >= (int)Cache.Size)
            return false;
          Method = Cache.ExtractString(methodIndex, methodSize);

          // Parse URL
          var urlIndex = index;
          var urlSize = 0;
          while (Cache[index] != ' ')
          {
            urlSize++;
            index++;
            if (index >= (int)Cache.Size)
              return false;
          }

          index++;
          if (index >= (int)Cache.Size)
            return false;
          Url = Cache.ExtractString(urlIndex, urlSize);

          // Parse protocol version
          var protocolIndex = index;
          var protocolSize = 0;
          while (Cache[index] != '\r')
          {
            protocolSize++;
            index++;
            if (index >= (int)Cache.Size)
              return false;
          }

          index++;
          if (index >= (int)Cache.Size || Cache[index] != '\n')
            return false;
          index++;
          if (index >= (int)Cache.Size)
            return false;
          Protocol = Cache.ExtractString(protocolIndex, protocolSize);

          // Parse headers
          while (index < (int)Cache.Size && index < i)
          {
            // Parse header name
            var headerNameIndex = index;
            var headerNameSize = 0;
            while (Cache[index] != ':')
            {
              headerNameSize++;
              index++;
              if (index >= i)
                break;
              if (index >= (int)Cache.Size)
                return false;
            }

            index++;
            if (index >= i)
              break;
            if (index >= (int)Cache.Size)
              return false;

            // Skip all prefix space characters
            while (char.IsWhiteSpace((char)Cache[index]))
            {
              index++;
              if (index >= i)
                break;
              if (index >= (int)Cache.Size)
                return false;
            }

            // Parse header value
            var headerValueIndex = index;
            var headerValueSize = 0;
            while (Cache[index] != '\r')
            {
              headerValueSize++;
              index++;
              if (index >= i)
                break;
              if (index >= (int)Cache.Size)
                return false;
            }

            index++;
            if (index >= (int)Cache.Size || Cache[index] != '\n')
              return false;
            index++;
            if (index >= (int)Cache.Size)
              return false;

            // Validate header name and value (sometimes value can be empty)
            if (headerNameSize == 0)
              return false;

            // Add a new header
            var headerName = Cache.ExtractString(headerNameIndex, headerNameSize);
            var headerValue = Cache.ExtractString(headerValueIndex, headerValueSize);
            _headers.Add((headerName, headerValue));

            // Try to find the body content length
            if (string.Compare(headerName, "Content-Length", StringComparison.OrdinalIgnoreCase) == 0)
            {
              _bodyLength = 0;
              for (var j = headerValueIndex; j < headerValueIndex + headerValueSize; j++)
              {
                if (Cache[j] < '0' || Cache[j] > '9')
                  return false;
                _bodyLength *= 10;
                _bodyLength += Cache[j] - '0';
                _bodyLengthProvided = true;
              }
            }

            // Try to find Cookies
            if (string.Compare(headerName, "Cookie", StringComparison.OrdinalIgnoreCase) == 0)
            {
              var name = true;
              var token = false;
              var current = headerValueIndex;
              var nameIndex = index;
              var nameSize = 0;
              var cookieIndex = index;
              var cookieSize = 0;
              for (var j = headerValueIndex; j < headerValueIndex + headerValueSize; j++)
              {
                if (Cache[j] == ' ')
                {
                  if (token)
                  {
                    if (name)
                    {
                      nameIndex = current;
                      nameSize = j - current;
                    }
                    else
                    {
                      cookieIndex = current;
                      cookieSize = j - current;
                    }
                  }

                  token = false;
                  continue;
                }

                if (Cache[j] == '=')
                {
                  if (token)
                  {
                    if (name)
                    {
                      nameIndex = current;
                      nameSize = j - current;
                    }
                    else
                    {
                      cookieIndex = current;
                      cookieSize = j - current;
                    }
                  }

                  token = false;
                  name = false;
                  continue;
                }

                if (Cache[j] == ';')
                {
                  if (token)
                  {
                    if (name)
                    {
                      nameIndex = current;
                      nameSize = j - current;
                    }
                    else
                    {
                      cookieIndex = current;
                      cookieSize = j - current;
                    }

                    // Validate the cookie
                    if (nameSize > 0 && cookieSize > 0)
                    {
                      // Add the cookie to the corresponding collection
                      _cookies.Add((Cache.ExtractString(nameIndex, nameSize),
                        Cache.ExtractString(cookieIndex, cookieSize)));

                      // Resset the current cookie values
                      nameIndex = j;
                      nameSize = 0;
                      cookieIndex = j;
                      cookieSize = 0;
                    }
                  }

                  token = false;
                  name = true;
                  continue;
                }

                if (!token)
                {
                  current = j;
                  token = true;
                }
              }

              // Process the last cookie
              if (token)
              {
                if (name)
                {
                  nameIndex = current;
                  nameSize = headerValueIndex + headerValueSize - current;
                }
                else
                {
                  cookieIndex = current;
                  cookieSize = headerValueIndex + headerValueSize - current;
                }

                // Validate the cookie
                if (nameSize > 0 && cookieSize > 0)
                  // Add the cookie to the corresponding collection
                  _cookies.Add((Cache.ExtractString(nameIndex, nameSize),
                    Cache.ExtractString(cookieIndex, cookieSize)));
              }
            }
          }

          // Reset the error flag
          IsErrorSet = false;

          // Update the body index and size
          _bodyIndex = i + 4;
          _bodySize = (int)Cache.Size - i - 4;

          // Update the parsed cache size
          _cacheSize = (int)Cache.Size;

          return true;
        }
      }

      // Update the parsed cache size
      _cacheSize = (int)Cache.Size >= 3 ? (int)Cache.Size - 3 : 0;

      return false;
    }

    internal bool ReceiveBody(byte[] buffer, int offset, int size)
    {
      // Update the request cache
      Cache.Append(buffer, offset, size);

      // Update the parsed cache size
      _cacheSize = (int)Cache.Size;

      // Update body size
      _bodySize += size;

      // Check if the body length was provided
      if (_bodyLengthProvided)
      {
        // Was the body fully received?
        if (_bodySize >= _bodyLength)
        {
          _bodySize = _bodyLength;
          return true;
        }
      }
      else
      {
        // HEAD/GET/DELETE/OPTIONS/TRACE request might have no body
        if (Method == "HEAD" || Method == "GET" || Method == "DELETE" || Method == "OPTIONS" || Method == "TRACE")
        {
          _bodyLength = 0;
          _bodySize = 0;
          return true;
        }

        // Check the body content to find the request body end
        if (_bodySize >= 4)
        {
          var index = _bodyIndex + _bodySize - 4;

          // Was the body fully received?
          if (Cache[index + 0] == '\r' && Cache[index + 1] == '\n' && Cache[index + 2] == '\r' &&
              Cache[index + 3] == '\n')
          {
            _bodyLength = _bodySize;
            return true;
          }
        }
      }

      // Body was received partially...
      return false;
    }
  }
}