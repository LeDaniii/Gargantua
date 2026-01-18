using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Hosting;

namespace Gargantua.Providers.Siemens.Internal;

public sealed class SiemensPlcConnectionService : BackgroundService, ISiemensPlcStateInfo
{
    private readonly SiemensTcpDriver _siemensTcpDriver;
    private readonly ILogger<SiemensPlcConnectionService> _logger;

    private readonly TimeSpan _idleDelay = TimeSpan.FromMilliseconds(100);
    private readonly TimeSpan _connectedHealthCheckDelay = TimeSpan.FromMilliseconds(250);
    private readonly TimeSpan _errorDelay = TimeSpan.FromSeconds(2);

    private volatile PlcConnectionState _currentState = PlcConnectionState.Idle;
    private volatile bool _isErrorManuallyReset;

    public SiemensPlcConnectionService(
        SiemensTcpDriver siemensTcpDriver,
        ILogger<SiemensPlcConnectionService> logger,
        string plcIdentifier)
    {
        _siemensTcpDriver = siemensTcpDriver;
        _logger = logger;
        PlcIdentifier = plcIdentifier;
    }

    public string PlcIdentifier { get; }

    public PlcConnectionState CurrentState => _currentState;

    public event Action<PlcConnectionState>? StateChanged;

    public void ResetError()
    {
        _isErrorManuallyReset = true;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        SetState(PlcConnectionState.Idle);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                PlcConnectionState currentStateSnapshot = _currentState;

                if (currentStateSnapshot == PlcConnectionState.Idle)
                {
                    SetState(PlcConnectionState.Connecting);
                    continue;
                }

                if (currentStateSnapshot == PlcConnectionState.Connecting)
                {
                    bool connected = await _siemensTcpDriver.ConnectAsync(stoppingToken);

                    if (connected)
                    {
                        SetState(PlcConnectionState.Connected);
                    }
                    else
                    {
                        SetState(PlcConnectionState.Error);
                    }

                    continue;
                }

                if (currentStateSnapshot == PlcConnectionState.Connected)
                {
                    await Task.Delay(_connectedHealthCheckDelay, stoppingToken);

                    bool stillConnected = await _siemensTcpDriver.IsConnectedAsync();
                    if (!stillConnected)
                    {
                        SetState(PlcConnectionState.Disconnected);
                    }

                    continue;
                }

                if (currentStateSnapshot == PlcConnectionState.Disconnected)
                {
                    await _siemensTcpDriver.DisconnectAsync();
                    await Task.Delay(_idleDelay, stoppingToken);
                    SetState(PlcConnectionState.Idle);
                    continue;
                }

                if (currentStateSnapshot == PlcConnectionState.Error)
                {
                    await _siemensTcpDriver.DisconnectAsync();

                    if (_isErrorManuallyReset)
                    {
                        _isErrorManuallyReset = false;
                        SetState(PlcConnectionState.Connecting);
                        continue;
                    }

                    await Task.Delay(_errorDelay, stoppingToken);
                    SetState(PlcConnectionState.Connecting);
                    continue;
                }

                await Task.Delay(_idleDelay, stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception exception)
            {
                _logger.LogError(exception, "Unhandled exception in SiemensPlcConnectionService loop.");
                SetState(PlcConnectionState.Error);
            }
        }

        await SafeShutdownAsync();
    }

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        await SafeShutdownAsync();
        await base.StopAsync(cancellationToken);
    }

    private async Task SafeShutdownAsync()
    {
        try
        {
            await _siemensTcpDriver.DisconnectAsync();
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "Error during SiemensPlcConnectionService shutdown disconnect.");
        }
    }

    private void SetState(PlcConnectionState newState)
    {
        PlcConnectionState oldState = _currentState;
        if (oldState == newState)
        {
            return;
        }

        _currentState = newState;

        _logger.LogInformation(
            "PLC state changed for {PlcIdentifier}: {OldState} -> {NewState}",
            PlcIdentifier,
            oldState,
            newState);

        StateChanged?.Invoke(newState);
    }
}
