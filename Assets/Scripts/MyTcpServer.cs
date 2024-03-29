using System;
using System.Text;
using Cysharp.Threading.Tasks;
using NetCoreServer;
using UnityEngine;
using UnityEngine.UI;

public class MyTcpServer : MonoBehaviour
{
  [SerializeField] private Text text;

  private SbTcpServer _tcpServer;

  private void Start()
  {
    _tcpServer = new SbTcpServer("127.0.0.1", 3567);
    _tcpServer.OnReceivedData += (guid, buffer, offset, size) =>
    {
      var s = Encoding.UTF8.GetString(buffer, (int)offset, (int)size);
      UniTask.Post(() => { text.text = $"Received data from {guid} \r\n {s}"; });
    };

    _tcpServer.Start();
  }

  private void OnDestroy()
  {
    _tcpServer?.DisconnectAll();
    _tcpServer?.Stop();
    _tcpServer?.Dispose();
  }
}


public class SbTcpServer : TcpServer
{
  public Action<Guid, byte[], long, long> OnReceivedData;

  public SbTcpServer(string address, int port) : base(address, port)
  {
  }

  protected override TcpSession CreateSession()
  {
    return new SbTcpSession(this);
  }
}


public class SbTcpSession : TcpSession
{
  private readonly SbTcpServer _sbServer;

  public SbTcpSession(TcpServer server) : base(server)
  {
    _sbServer = server as SbTcpServer;
  }

  protected override void OnConnected()
  {
    Debug.Log($"welcome {Id}");
  }

  protected override void OnDisconnected()
  {
    Debug.Log($"good bye {Id}");
  }

  protected override void OnReceived(byte[] buffer, long offset, long size)
  {
    _sbServer.OnReceivedData?.Invoke(Id, buffer, offset, size);
    Debug.Log(Encoding.UTF8.GetString(buffer, (int)offset, (int)size));
  }
}