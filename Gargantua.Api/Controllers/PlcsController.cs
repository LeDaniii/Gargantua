using Gargantua.Core.Models;
using Gargantua.Providers.Abstractions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Gargantua.Api.Controllers;

[ApiController]
[Route("plcs")]
public sealed class PlcsController : ControllerBase
{
    private readonly IPlcProvider plcProvider;

    public PlcsController(IPlcProvider plcProvider)
    {
        this.plcProvider = plcProvider;
    }

    public sealed class PlcReadRequestDto
    {
        public required List<string> Addresses { get; init; }
    }

    [HttpPost("{plcIdentifier}/read")]
    public async Task<ActionResult<PlcReadResult>> ReadAsync(
        string plcIdentifier,
        [FromBody] PlcReadRequestDto plcReadRequestDto,
        CancellationToken cancellationToken)
    {
        if (!string.Equals(plcIdentifier, plcProvider.PlcIdentifier, StringComparison.OrdinalIgnoreCase))
        {
            return NotFound();
        }

        var plcAddresses = plcReadRequestDto.Addresses
            .Select(addressString => new PlcAddress
            {
                PlcIdentifier = plcIdentifier,
                VendorAddress = addressString,
                DataType = PlcDataType.String // vorerst als String, später typisiert
            })
            .ToList();

        var plcReadRequest = new PlcReadRequest
        {
            PlcIdentifier = plcIdentifier,
            Addresses = plcAddresses
        };

        PlcReadResult plcReadResult = await plcProvider.ReadAsync(plcReadRequest, cancellationToken);

        return Ok(plcReadResult);
    }
}
