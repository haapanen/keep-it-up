using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using KeepItUp.Communication;
using KeepItUp.Core;
using KeepItUp.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace KeepItUp.Service
{
    public class KeepItUpService : IHostedService, IDisposable
    {
        private readonly KeepItUpContext _keepItUpContext;
        private object _timerLock = new object();
        private bool _isChecking;
        private readonly ILogger<KeepItUpService> _logger;
        private readonly ETClient _etClient;
        private readonly IConfigurationRoot _configuration;
        private Timer _timer;

        public KeepItUpService(KeepItUpContext keepItUpContext, ETClient etClient, IConfigurationRoot configuration, ILogger<KeepItUpService> logger)
        {
            _keepItUpContext = keepItUpContext;
            _etClient = etClient;
            _configuration = configuration;
            _logger = logger;
        }

        private async void TimerOnElapsed(object state)
        {
            lock (_timerLock)
            {
                if (_isChecking)
                {
                    return;
                }
                _isChecking = true;
            }

            try
            {
                await CheckAsync();
            }
            catch (Exception exception)
            {
                _logger.LogError("Uncaught exception while polling", exception);
            }
            finally
            {
                lock (_timerLock)
                {
                    _isChecking = false;
                }
            }
        }

        private async Task CheckAsync()
        {
            foreach (var server in _keepItUpContext.Servers)
            {
                if (server.Enabled)
                {
                    await EnsureRunningAsync(server);
                }
                else
                {
                    await EnsureNotRunningAsync(server);
                }
            }
        }

        private async Task EnsureRunningAsync(Server server)
        {
            if (server.ProcessId == Server.NoPid)
            {
                await StartServerAsync(server);
            }
            else
            {
                if (!await _etClient.IsRunningAsync("localhost", server.Port, TimeSpan.FromSeconds(1)))
                {
                    await StartServerAsync(server);
                }
            }
        }

        private async Task StartServerAsync(Server server)
        {
            var process = new Process();
            process.StartInfo.FileName = _configuration["screen"];
            process.StartInfo.Arguments =
                $"-dms {server.Name} {_configuration["etded"]} +set fs_game etjump +set fs_basepath {server.BasePath} +set fs_homepath {server.HomePath} +exec server.cfg +map oasis";
            process.StartInfo.CreateNoWindow = false;
            process.StartInfo.UseShellExecute = false;
            process.Start();
            server.ProcessId = process.Id;
            await _keepItUpContext.SaveChangesAsync();
        }

        private async Task EnsureNotRunningAsync(Server server)
        {
            if (server.ProcessId != Server.NoPid)
            {
                var process = Process.GetProcessById(server.ProcessId);
                if (!process.HasExited)
                {
                    process.Kill();
                    server.ProcessId = Server.NoPid;
                    await _keepItUpContext.SaveChangesAsync();
                }
            }
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            await _keepItUpContext.Database.EnsureCreatedAsync();
            await _keepItUpContext.Database.MigrateAsync();

            _timer = new Timer(TimerOnElapsed, null, TimeSpan.Zero, TimeSpan.FromSeconds(5));
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }

        public void Dispose()
        {
            _timer?.Dispose();
        }
    }
}
