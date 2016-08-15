using System;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using Quantify.Logging;

namespace Quantify.Graphite
{
    public class GraphiteConnection : IDisposable
    {
        private static readonly ILog Log = LogProvider.GetLogger(typeof(GraphiteConnection));

        private readonly string _hostname;
        private readonly int _port;
        private TcpClient _client = null;
        private NetworkStream _stream = null;
        private byte[] _buffer = new byte[256];

        public GraphiteConnection(string hostname, int port)
        {
            _hostname = hostname;
            _port = port;
        }

        private void CloseConnection()
        {
            _client?.GetStream()?.Dispose();
            _client?.Dispose();
        }

        private async Task OpenConnection()
        {
            _client = new TcpClient();
            await _client.ConnectAsync(_hostname, _port);
            _stream = _client.GetStream();
        }

        public async Task SendAsync(string reading)
        {
            try
            {
                if (_stream == null)
                    await OpenConnection();

                if (reading.Length + 1 > _buffer.Length)
                    _buffer = new byte[Math.Max(reading.Length, _buffer.Length * 2)];

                var bytesWritten = Encoding.ASCII.GetBytes(reading, 0, reading.Length, _buffer, 0);
                _buffer[bytesWritten] = (byte)'\n';

                await _stream.WriteAsync(_buffer, 0, bytesWritten + 1);
            }
            catch (Exception ex)
            {
                Log.Error($"Failed to send data to graphite endpoint {_hostname}:{_port}: {ex}");
                CloseConnection();
            }
        }

        public async Task FlushAsync()
        {
            if (_stream == null) return;
            await _stream.FlushAsync();
        }

        public void Dispose()
        {
            CloseConnection();
        }
    }
}