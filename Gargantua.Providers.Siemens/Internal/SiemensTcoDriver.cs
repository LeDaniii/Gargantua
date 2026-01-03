using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using S7.Net;
namespace Gargantua.Providers.Siemens.Internal;

public interface ISiemensTcpDriver
{
    Task<bool> ConnectAsync(CancellationToken cancellationToken = default);
    Task DisconnectAsync();
    Task<bool> IsConnectedAsync();
    Task<short> ReadIntAsync(string address, CancellationToken cancellationToken = default);
    Task<int> ReadDintAsync(string address, CancellationToken cancellationToken = default);
    Task<bool> ReadBoolAsync(string address, CancellationToken cancellationToken = default);
    Task<float> ReadFloatAsync(string address, CancellationToken cancellationToken = default);
    Task<string> ReadStringAsync(string address, int length, CancellationToken cancellationToken = default);
    Task<DateTime> ReadDateTimeAsync(string address, int length, CancellationToken cancellationToken = default);
    Task<ushort> ReadUShortAsync(string address, CancellationToken cancellationToken = default);
    Task<bool> WriteIntAsync(string address, int value, CancellationToken cancellationToken = default);
    Task<bool> WriteDintAsync(string address, int value, CancellationToken cancellationToken = default);
    Task<bool> WriteBoolAsync(string address, bool value, CancellationToken cancellationToken = default);
    Task<bool> WriteFloatAsync(string address, float value, CancellationToken cancellationToken = default);
    Task<bool> WriteStringAsync(string address, string value, int maxLength, CancellationToken cancellationToken = default);
    Task<bool> WriteDateTime(string address, DateTime value, CancellationToken cancellationToken = default);
}

public class SiemensTcpDriver
{
    private readonly Plc _plc;
    private readonly string _ipAddress;
    private readonly CpuType _cpuType;
    private readonly short _rack;
    private readonly short _slot;
    private readonly int _reconnectDelayMilliseconds = 500;
    private readonly ILogger<SiemensTcpDriver> _logger;
    private bool _isConnected;

    public SiemensTcpDriver(string ipAddress, CpuType cpuType, ILogger<SiemensTcpDriver> logger, short rack = 0, short slot = 2)
    {
        _ipAddress = ipAddress;
        _cpuType = cpuType;
        _rack = rack;
        _slot = slot;
        _logger = logger;
        _plc = new Plc(cpuType, ipAddress, rack, slot);
    }

    public async Task<bool> ConnectAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            bool result = await Task.Run(() =>
            {
                try
                {
                    _plc.Open();
                    return _plc.IsConnected;
                }
                catch (Exception exception)
                {
                    _logger.LogError(exception, "Error while connecting to PLC.");
                    return false;
                }
            }, cancellationToken);

            _isConnected = result;

            if (_isConnected)
            {
                _logger.LogInformation("Connected to PLC.");
                _ = Task.Run(() => StartReconnectLoop(cancellationToken), cancellationToken);
            }
            else
            {
                _logger.LogWarning("Failed to connect to PLC.");
            }

            return _isConnected;
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "Error while connecting to PLC.");
            return false;
        }
    }

    public Task DisconnectAsync()
    {
        if (_plc.IsConnected)
        {
            _plc.Close();
            _isConnected = false;
            _logger.LogInformation("Disconnected from PLC.");
        }

        return Task.CompletedTask;
    }

    public Task<bool> IsConnectedAsync()
    {
        return Task.FromResult(_isConnected);
    }

    private async Task StartReconnectLoop(CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            await Task.Delay(_reconnectDelayMilliseconds, cancellationToken);

            if (!_plc.IsConnected)
            {
                _logger.LogWarning("Connection lost. Reconnecting...");
                await ConnectAsync(cancellationToken);
            }
        }
    }

    public async Task<short> ReadIntAsync(string address, CancellationToken cancellationToken = default)
    {
        try
        {
            return (short)(ushort)await _plc.ReadAsync(address, cancellationToken);
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "Error reading Int from {Address}", address);
            return -1;
        }
    }

    public async Task<int> ReadDintAsync(string address, CancellationToken cancellationToken = default)
    {
        try
        {
            return (int)(uint)await _plc.ReadAsync(address, cancellationToken);
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "Error reading DInt from {Address}", address);
            return 0;
        }
    }

    public async Task<bool> ReadBoolAsync(string address, CancellationToken cancellationToken = default)
    {
        try
        {
            return (bool)await _plc.ReadAsync(address, cancellationToken);
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "Error reading Bool from {Address}", address);
            return false;
        }
    }

    public async Task<float> ReadFloatAsync(string address, CancellationToken cancellationToken = default)
    {
        try
        {
            object raw = await _plc.ReadAsync(address, cancellationToken);
            int bits = Convert.ToInt32(raw);
            return BitConverter.ToSingle(BitConverter.GetBytes(bits), 0);
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "Error reading REAL from {Address}", address);
            return -1f;
        }
    }

    public async Task<ushort> ReadUShortAsync(string address, CancellationToken cancellationToken = default)
    {
        try
        {
            ushort result = Convert.ToUInt16(await _plc.ReadAsync(address, cancellationToken));
            return result;
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "Error reading UShort from {Address}", address);
            return 0;
        }
    }

    public async Task<bool> WriteIntAsync(string address, int value, CancellationToken cancellationToken = default)
    {
        try
        {
            await _plc.WriteAsync(address, (ushort)value, cancellationToken);
            return true;
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "Error writing Int to {Address}", address);
            return false;
        }
    }

    public async Task<bool> WriteDintAsync(string address, int value, CancellationToken cancellationToken = default)
    {
        try
        {
            await _plc.WriteAsync(address, value, cancellationToken);
            return true;
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "Error writing DInt to {Address}", address);
            return false;
        }
    }

    public async Task<bool> WriteBoolAsync(string address, bool value, CancellationToken cancellationToken = default)
    {
        try
        {
            await _plc.WriteAsync(address, value, cancellationToken);
            return true;
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "Error writing Bool to {Address}", address);
            return false;
        }
    }

    public async Task<bool> WriteFloatAsync(string address, float value, CancellationToken cancellationToken = default)
    {
        try
        {
            await _plc.WriteAsync(address, value, cancellationToken);
            return true;
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "Error writing Float to {Address}", address);
            return false;
        }
    }

    public async Task<string> ReadStringAsync(string address, int length, CancellationToken cancellationToken = default)
    {
        try
        {
            Regex regex = new Regex(@"DB(\d+)\.DBB(\d+)");
            Match match = regex.Match(address);

            if (!match.Success)
            {
                throw new ArgumentException("Invalid string address format.", nameof(address));
            }

            int dataBlockNumber = int.Parse(match.Groups[1].Value);
            int byteOffset = int.Parse(match.Groups[2].Value);

            byte[] byteArray = await _plc.ReadBytesAsync(DataType.DataBlock, dataBlockNumber, byteOffset, length + 2, cancellationToken);
            string result = S7.Net.Types.S7String.FromByteArray(byteArray);

            _logger.LogDebug("Read String: {Value} from {Address}", result, address);
            return result;
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "Error reading String from {Address}", address);
            return string.Empty;
        }
    }

    public async Task<bool> WriteStringAsync(string address, string value, int maxLength, CancellationToken cancellationToken = default)
    {
        try
        {
            Regex regex = new Regex(@"DB(\d+)\.DBB(\d+)");
            Match match = regex.Match(address);

            if (!match.Success)
            {
                throw new ArgumentException("Invalid string address format.", nameof(address));
            }

            int dataBlockNumber = int.Parse(match.Groups[1].Value);
            int byteOffset = int.Parse(match.Groups[2].Value);

            byte[] byteArray = S7.Net.Types.S7String.ToByteArray(value, maxLength);
            List<byte> values = new List<byte>();
            values.AddRange(byteArray);

            await _plc.WriteBytesAsync(DataType.DataBlock, dataBlockNumber, byteOffset, values.ToArray(), cancellationToken);

            _logger.LogDebug("Wrote String: {Value} to {Address}", value, address);
            return true;
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "Error writing String to {Address}", address);
            return false;
        }
    }

    public async Task<DateTime> ReadDateTimeAsync(string address, int length, CancellationToken cancellationToken = default)
    {
        try
        {
            Regex regex = new Regex(@"DB(\d+)\.DBB(\d+)");
            Match match = regex.Match(address);

            if (!match.Success)
            {
                throw new ArgumentException("Invalid DateTime address format.", nameof(address));
            }

            int dataBlockNumber = int.Parse(match.Groups[1].Value);
            int byteOffset = int.Parse(match.Groups[2].Value);

            byte[] byteArray = await _plc.ReadBytesAsync(DataType.DataBlock, dataBlockNumber, byteOffset, length, cancellationToken);
            DateTime result = S7.Net.Types.DateTime.FromByteArray(byteArray);

            _logger.LogDebug("Read DateTime: {Value} from {Address}", result, address);
            return result;
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "Error reading DateTime from {Address}", address);
            return default;
        }
    }

    public async Task<bool> WriteDateTime(string address, DateTime value, CancellationToken cancellationToken = default)
    {
        try
        {
            Regex regex = new Regex(@"DB(\d+)\.DBB(\d+)");
            Match match = regex.Match(address);

            if (!match.Success)
            {
                throw new ArgumentException("Invalid DateTime address format.", nameof(address));
            }

            int dataBlockNumber = int.Parse(match.Groups[1].Value);
            int byteOffset = int.Parse(match.Groups[2].Value);

            byte[] byteArray = S7.Net.Types.DateTime.ToByteArray(value);
            List<byte> values = new List<byte>();
            values.AddRange(byteArray);

            await _plc.WriteBytesAsync(DataType.DataBlock, dataBlockNumber, byteOffset, values.ToArray(), cancellationToken);

            _logger.LogDebug("Wrote DateTime: {Value} to {Address}", value, address);
            return true;
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "Error writing DateTime to {Address}", address);
            return false;
        }
    }
}