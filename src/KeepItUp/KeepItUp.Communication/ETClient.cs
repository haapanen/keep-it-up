using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using KeepItUp.Common;

namespace KeepItUp.Communication
{
    public class ETClient
    {
        private readonly UdpClient _udpClient;
        private readonly byte[] _outOfBoundPrefix = {0xff, 0xff, 0xff, 0xff};
        private readonly byte[] _getStatusPacket;
            

        public ETClient()
        {
            _udpClient = new UdpClient();
            _getStatusPacket = _outOfBoundPrefix.Concat(Encoding.ASCII.GetBytes("getstatus")).ToArray();
        }

        public async Task<bool> IsRunningAsync(string address, short port, TimeSpan timeout)
        {
            try
            {
                await _udpClient.SendAsync(_getStatusPacket, _getStatusPacket.Length, address, port)
                    .WithTimeout(timeout);
                var result = _udpClient.ReceiveAsync().WithTimeout(timeout);
                return true;
            }
            catch (TimeoutException)
            {
                return false;
            }
        }
    }
}
