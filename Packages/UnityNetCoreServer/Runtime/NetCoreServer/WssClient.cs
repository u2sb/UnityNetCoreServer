using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
// ReSharper disable InconsistentlySynchronizedField

namespace NetCoreServer
{
  /// <summary>
  ///   WebSocket secure client
  /// </summary>
  /// <remarks>WebSocket secure client is used to communicate with secure WebSocket server. Thread-safe.</remarks>
  public class WssClient : HttpsClient, IWebSocket
  {
    internal readonly WebSocket WebSocket;

    // Sync connect flag
    private bool _syncConnect;

    /// <summary>
    ///   Initialize WebSocket client with a given IP address and port number
    /// </summary>
    /// <param name="context">SSL context</param>
    /// <param name="address">IP address</param>
    /// <param name="port">Port number</param>
    public WssClient(SslContext context, IPAddress address, int port) : base(context, address, port)
    {
      WebSocket = new WebSocket(this);
    }

    /// <summary>
    ///   Initialize WebSocket client with a given IP address and port number
    /// </summary>
    /// <param name="context">SSL context</param>
    /// <param name="address">IP address</param>
    /// <param name="port">Port number</param>
    public WssClient(SslContext context, string address, int port) : base(context, address, port)
    {
      WebSocket = new WebSocket(this);
    }

    /// <summary>
    ///   Initialize WebSocket client with a given DNS endpoint
    /// </summary>
    /// <param name="context">SSL context</param>
    /// <param name="endpoint">DNS endpoint</param>
    public WssClient(SslContext context, DnsEndPoint endpoint) : base(context, endpoint)
    {
      WebSocket = new WebSocket(this);
    }

    /// <summary>
    ///   Initialize WebSocket client with a given IP endpoint
    /// </summary>
    /// <param name="context">SSL context</param>
    /// <param name="endpoint">IP endpoint</param>
    public WssClient(SslContext context, IPEndPoint endpoint) : base(context, endpoint)
    {
      WebSocket = new WebSocket(this);
    }

    /// <summary>
    ///   WebSocket random nonce
    /// </summary>
    public byte[] WsNonce => WebSocket.WsNonce;

    #region WebSocket connection methods

    public override bool Connect()
    {
      _syncConnect = true;
      return base.Connect();
    }

    public override bool ConnectAsync()
    {
      _syncConnect = false;
      return base.ConnectAsync();
    }

    public virtual bool Close()
    {
      return Close(0, Span<byte>.Empty);
    }

    public virtual bool Close(int status)
    {
      return Close(status, Span<byte>.Empty);
    }

    public virtual bool Close(int status, string text)
    {
      return Close(status, Encoding.UTF8.GetBytes(text));
    }

    public virtual bool Close(int status, ReadOnlySpan<char> text)
    {
      return Close(status, Encoding.UTF8.GetBytes(text.ToArray()));
    }

    public virtual bool Close(int status, byte[] buffer)
    {
      return Close(status, buffer.AsSpan());
    }

    public virtual bool Close(int status, byte[] buffer, long offset, long size)
    {
      return Close(status, buffer.AsSpan((int)offset, (int)size));
    }

    public virtual bool Close(int status, ReadOnlySpan<byte> buffer)
    {
      SendClose(status, buffer);
      base.Disconnect();
      return true;
    }

    public virtual bool CloseAsync()
    {
      return CloseAsync(0, Span<byte>.Empty);
    }

    public virtual bool CloseAsync(int status)
    {
      return CloseAsync(status, Span<byte>.Empty);
    }

    public virtual bool CloseAsync(int status, string text)
    {
      return CloseAsync(status, Encoding.UTF8.GetBytes(text));
    }

    public virtual bool CloseAsync(int status, ReadOnlySpan<char> text)
    {
      return CloseAsync(status, Encoding.UTF8.GetBytes(text.ToArray()));
    }

    public virtual bool CloseAsync(int status, byte[] buffer)
    {
      return CloseAsync(status, buffer.AsSpan());
    }

    public virtual bool CloseAsync(int status, byte[] buffer, long offset, long size)
    {
      return CloseAsync(status, buffer.AsSpan((int)offset, (int)size));
    }

    public virtual bool CloseAsync(int status, ReadOnlySpan<byte> buffer)
    {
      SendClose(status, buffer);
      base.DisconnectAsync();
      return true;
    }

    #endregion

    #region WebSocket send text methods

    public long SendText(string text)
    {
      return SendText(Encoding.UTF8.GetBytes(text));
    }

    public long SendText(ReadOnlySpan<char> text)
    {
      return SendText(Encoding.UTF8.GetBytes(text.ToArray()));
    }

    public long SendText(byte[] buffer)
    {
      return SendText(buffer.AsSpan());
    }

    public long SendText(byte[] buffer, long offset, long size)
    {
      return SendText(buffer.AsSpan((int)offset, (int)size));
    }

    public long SendText(ReadOnlySpan<byte> buffer)
    {
      lock (WebSocket.WsSendLock)
      {
        WebSocket.PrepareSendFrame(WebSocket.WS_FIN | WebSocket.WS_TEXT, true, buffer);
        return base.Send(WebSocket.WsSendBuffer.AsReadOnlySpan());
      }
    }

    public bool SendTextAsync(string text)
    {
      return SendTextAsync(Encoding.UTF8.GetBytes(text));
    }

    public bool SendTextAsync(ReadOnlySpan<char> text)
    {
      return SendTextAsync(Encoding.UTF8.GetBytes(text.ToArray()));
    }

    public bool SendTextAsync(byte[] buffer)
    {
      return SendTextAsync(buffer.AsSpan());
    }

    public bool SendTextAsync(byte[] buffer, long offset, long size)
    {
      return SendTextAsync(buffer.AsSpan((int)offset, (int)size));
    }

    public bool SendTextAsync(ReadOnlySpan<byte> buffer)
    {
      lock (WebSocket.WsSendLock)
      {
        WebSocket.PrepareSendFrame(WebSocket.WS_FIN | WebSocket.WS_TEXT, true, buffer);
        return base.SendAsync(WebSocket.WsSendBuffer.AsReadOnlySpan());
      }
    }

    #endregion

    #region WebSocket send binary methods

    public long SendBinary(string text)
    {
      return SendBinary(Encoding.UTF8.GetBytes(text));
    }

    public long SendBinary(ReadOnlySpan<char> text)
    {
      return SendBinary(Encoding.UTF8.GetBytes(text.ToArray()));
    }

    public long SendBinary(byte[] buffer)
    {
      return SendBinary(buffer.AsSpan());
    }

    public long SendBinary(byte[] buffer, long offset, long size)
    {
      return SendBinary(buffer.AsSpan((int)offset, (int)size));
    }

    public long SendBinary(ReadOnlySpan<byte> buffer)
    {
      lock (WebSocket.WsSendLock)
      {
        WebSocket.PrepareSendFrame(WebSocket.WS_FIN | WebSocket.WS_BINARY, true, buffer);
        return base.Send(WebSocket.WsSendBuffer.AsReadOnlySpan());
      }
    }

    public bool SendBinaryAsync(string text)
    {
      return SendBinaryAsync(Encoding.UTF8.GetBytes(text));
    }

    public bool SendBinaryAsync(ReadOnlySpan<char> text)
    {
      return SendBinaryAsync(Encoding.UTF8.GetBytes(text.ToArray()));
    }

    public bool SendBinaryAsync(byte[] buffer)
    {
      return SendBinaryAsync(buffer.AsSpan());
    }

    public bool SendBinaryAsync(byte[] buffer, long offset, long size)
    {
      return SendBinaryAsync(buffer.AsSpan((int)offset, (int)size));
    }

    public bool SendBinaryAsync(ReadOnlySpan<byte> buffer)
    {
      lock (WebSocket.WsSendLock)
      {
        WebSocket.PrepareSendFrame(WebSocket.WS_FIN | WebSocket.WS_BINARY, true, buffer);
        return base.SendAsync(WebSocket.WsSendBuffer.AsReadOnlySpan());
      }
    }

    #endregion

    #region WebSocket send close methods

    public long SendClose(int status, string text)
    {
      return SendClose(status, Encoding.UTF8.GetBytes(text));
    }

    public long SendClose(int status, ReadOnlySpan<char> text)
    {
      return SendClose(status, Encoding.UTF8.GetBytes(text.ToArray()));
    }

    public long SendClose(int status, byte[] buffer)
    {
      return SendClose(status, buffer.AsSpan());
    }

    public long SendClose(int status, byte[] buffer, long offset, long size)
    {
      return SendClose(status, buffer.AsSpan((int)offset, (int)size));
    }

    public long SendClose(int status, ReadOnlySpan<byte> buffer)
    {
      lock (WebSocket.WsSendLock)
      {
        WebSocket.PrepareSendFrame(WebSocket.WS_FIN | WebSocket.WS_CLOSE, true, buffer, status);
        return base.Send(WebSocket.WsSendBuffer.AsReadOnlySpan());
      }
    }

    public bool SendCloseAsync(int status, string text)
    {
      return SendCloseAsync(status, Encoding.UTF8.GetBytes(text));
    }

    public bool SendCloseAsync(int status, ReadOnlySpan<char> text)
    {
      return SendCloseAsync(status, Encoding.UTF8.GetBytes(text.ToArray()));
    }

    public bool SendCloseAsync(int status, byte[] buffer)
    {
      return SendCloseAsync(status, buffer.AsSpan());
    }

    public bool SendCloseAsync(int status, byte[] buffer, long offset, long size)
    {
      return SendCloseAsync(status, buffer.AsSpan((int)offset, (int)size));
    }

    public bool SendCloseAsync(int status, ReadOnlySpan<byte> buffer)
    {
      lock (WebSocket.WsSendLock)
      {
        WebSocket.PrepareSendFrame(WebSocket.WS_FIN | WebSocket.WS_CLOSE, true, buffer, status);
        return base.SendAsync(WebSocket.WsSendBuffer.AsReadOnlySpan());
      }
    }

    #endregion

    #region WebSocket send ping methods

    public long SendPing(string text)
    {
      return SendPing(Encoding.UTF8.GetBytes(text));
    }

    public long SendPing(ReadOnlySpan<char> text)
    {
      return SendPing(Encoding.UTF8.GetBytes(text.ToArray()));
    }

    public long SendPing(byte[] buffer)
    {
      return SendPing(buffer.AsSpan());
    }

    public long SendPing(byte[] buffer, long offset, long size)
    {
      return SendPing(buffer.AsSpan((int)offset, (int)size));
    }

    public long SendPing(ReadOnlySpan<byte> buffer)
    {
      lock (WebSocket.WsSendLock)
      {
        WebSocket.PrepareSendFrame(WebSocket.WS_FIN | WebSocket.WS_PING, true, buffer);
        return base.Send(WebSocket.WsSendBuffer.AsReadOnlySpan());
      }
    }

    public bool SendPingAsync(string text)
    {
      return SendPingAsync(Encoding.UTF8.GetBytes(text));
    }

    public bool SendPingAsync(ReadOnlySpan<char> text)
    {
      return SendPingAsync(Encoding.UTF8.GetBytes(text.ToArray()));
    }

    public bool SendPingAsync(byte[] buffer)
    {
      return SendPingAsync(buffer.AsSpan());
    }

    public bool SendPingAsync(byte[] buffer, long offset, long size)
    {
      return SendPingAsync(buffer.AsSpan((int)offset, (int)size));
    }

    public bool SendPingAsync(ReadOnlySpan<byte> buffer)
    {
      lock (WebSocket.WsSendLock)
      {
        WebSocket.PrepareSendFrame(WebSocket.WS_FIN | WebSocket.WS_PING, true, buffer);
        return base.SendAsync(WebSocket.WsSendBuffer.AsReadOnlySpan());
      }
    }

    #endregion

    #region WebSocket send pong methods

    public long SendPong(string text)
    {
      return SendPong(Encoding.UTF8.GetBytes(text));
    }

    public long SendPong(ReadOnlySpan<char> text)
    {
      return SendPong(Encoding.UTF8.GetBytes(text.ToArray()));
    }

    public long SendPong(byte[] buffer)
    {
      return SendPong(buffer.AsSpan());
    }

    public long SendPong(byte[] buffer, long offset, long size)
    {
      return SendPong(buffer.AsSpan((int)offset, (int)size));
    }

    public long SendPong(ReadOnlySpan<byte> buffer)
    {
      lock (WebSocket.WsSendLock)
      {
        WebSocket.PrepareSendFrame(WebSocket.WS_FIN | WebSocket.WS_PONG, true, buffer);
        return base.Send(WebSocket.WsSendBuffer.AsReadOnlySpan());
      }
    }

    public bool SendPongAsync(string text)
    {
      return SendPongAsync(Encoding.UTF8.GetBytes(text));
    }

    public bool SendPongAsync(ReadOnlySpan<char> text)
    {
      return SendPongAsync(Encoding.UTF8.GetBytes(text.ToArray()));
    }

    public bool SendPongAsync(byte[] buffer)
    {
      return SendPongAsync(buffer.AsSpan());
    }

    public bool SendPongAsync(byte[] buffer, long offset, long size)
    {
      return SendPongAsync(buffer.AsSpan((int)offset, (int)size));
    }

    public bool SendPongAsync(ReadOnlySpan<byte> buffer)
    {
      lock (WebSocket.WsSendLock)
      {
        WebSocket.PrepareSendFrame(WebSocket.WS_FIN | WebSocket.WS_PONG, true, buffer);
        return base.SendAsync(WebSocket.WsSendBuffer.AsReadOnlySpan());
      }
    }

    #endregion

    #region WebSocket receive methods

    public string ReceiveText()
    {
      var result = new Buffer();

      if (!WebSocket.WsHandshaked)
        return result.ExtractString(0, result.Data.Length);

      var cache = new Buffer();

      // Receive WebSocket frame data
      while (!WebSocket.WsFinalReceived)
      {
        while (!WebSocket.WsFrameReceived)
        {
          var required = WebSocket.RequiredReceiveFrameSize();
          cache.Resize(required);
          long received = (int)base.Receive(cache.Data, 0, required);
          if (received != required)
            return result.ExtractString(0, result.Data.Length);
          WebSocket.PrepareReceiveFrame(cache.Data, 0, received);
        }

        if (!WebSocket.WsFinalReceived)
          WebSocket.PrepareReceiveFrame(null, 0, 0);
      }

      // Copy WebSocket frame data
      result.Append(WebSocket.WsReceiveFinalBuffer);
      WebSocket.PrepareReceiveFrame(null, 0, 0);
      return result.ExtractString(0, result.Data.Length);
    }

    public Buffer ReceiveBinary()
    {
      var result = new Buffer();

      if (!WebSocket.WsHandshaked)
        return result;

      var cache = new Buffer();

      // Receive WebSocket frame data
      while (!WebSocket.WsFinalReceived)
      {
        while (!WebSocket.WsFrameReceived)
        {
          var required = WebSocket.RequiredReceiveFrameSize();
          cache.Resize(required);
          long received = (int)base.Receive(cache.Data, 0, required);
          if (received != required)
            return result;
          WebSocket.PrepareReceiveFrame(cache.Data, 0, received);
        }

        if (!WebSocket.WsFinalReceived)
          WebSocket.PrepareReceiveFrame(null, 0, 0);
      }

      // Copy WebSocket frame data
      result.Append(WebSocket.WsReceiveFinalBuffer);
      WebSocket.PrepareReceiveFrame(null, 0, 0);
      return result;
    }

    #endregion

    #region Session handlers

    protected override void OnHandshaked()
    {
      // Clear WebSocket send/receive buffers
      WebSocket.ClearWsBuffers();

      // Fill the WebSocket upgrade HTTP request
      OnWsConnecting(Request);

      // Send the WebSocket upgrade HTTP request
      if (_syncConnect)
        SendRequest(Request);
      else
        SendRequestAsync(Request);
    }

    protected override void OnDisconnecting()
    {
      if (WebSocket.WsHandshaked)
        OnWsDisconnecting();
    }

    protected override void OnDisconnected()
    {
      // Disconnect WebSocket
      if (WebSocket.WsHandshaked)
      {
        WebSocket.WsHandshaked = false;
        OnWsDisconnected();
      }

      // Reset WebSocket upgrade HTTP request and response
      Request.Clear();
      Response.Clear();

      // Clear WebSocket send/receive buffers
      WebSocket.ClearWsBuffers();

      // Initialize new WebSocket random nonce
      WebSocket.InitWsNonce();
    }

    protected override void OnReceived(byte[] buffer, long offset, long size)
    {
      // Check for WebSocket handshaked status
      if (WebSocket.WsHandshaked)
      {
        // Prepare receive frame
        WebSocket.PrepareReceiveFrame(buffer, offset, size);
        return;
      }

      base.OnReceived(buffer, offset, size);
    }

    protected override void OnReceivedResponseHeader(HttpResponse response)
    {
      // Check for WebSocket handshaked status
      if (WebSocket.WsHandshaked)
        return;

      // Try to perform WebSocket upgrade
      if (!WebSocket.PerformClientUpgrade(response, Id)) base.OnReceivedResponseHeader(response);
    }

    protected override void OnReceivedResponse(HttpResponse response)
    {
      // Check for WebSocket handshaked status
      if (WebSocket.WsHandshaked)
      {
        // Prepare receive frame from the remaining response body
        var body = Response.Body;
        var data = Encoding.UTF8.GetBytes(body);
        WebSocket.PrepareReceiveFrame(data, 0, data.Length);
        return;
      }

      base.OnReceivedResponse(response);
    }

    protected override void OnReceivedResponseError(HttpResponse response, string error)
    {
      // Check for WebSocket handshaked status
      if (WebSocket.WsHandshaked)
      {
        OnError(new SocketError());
        return;
      }

      base.OnReceivedResponseError(response, error);
    }

    #endregion

    #region Web socket handlers

    public virtual void OnWsConnecting(HttpRequest request)
    {
    }

    public virtual void OnWsConnected(HttpResponse response)
    {
    }

    public virtual bool OnWsConnecting(HttpRequest request, HttpResponse response)
    {
      return true;
    }

    public virtual void OnWsConnected(HttpRequest request)
    {
    }

    public virtual void OnWsDisconnecting()
    {
    }

    public virtual void OnWsDisconnected()
    {
    }

    public virtual void OnWsReceived(byte[] buffer, long offset, long size)
    {
    }

    public virtual void OnWsClose(byte[] buffer, long offset, long size, int status = 1000)
    {
      CloseAsync();
    }

    public virtual void OnWsPing(byte[] buffer, long offset, long size)
    {
      SendPongAsync(buffer, offset, size);
    }

    public virtual void OnWsPong(byte[] buffer, long offset, long size)
    {
    }

    public virtual void OnWsError(string error)
    {
      OnError(SocketError.SocketError);
    }

    public virtual void OnWsError(SocketError error)
    {
      OnError(error);
    }

    #endregion
  }
}