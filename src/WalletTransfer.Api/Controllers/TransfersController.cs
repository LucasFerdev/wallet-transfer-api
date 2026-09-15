using Microsoft.AspNetCore.Mvc;

using WalletTransfer.Api.Contracts.Transfers;
using WalletTransfer.Application.Transfers;
using WalletTransfer.Application.Transfers.Models;

namespace WalletTransfer.Api.Controllers;

[ApiController]
[Route("transfer")]
public sealed class TransfersController(
    ITransferService transferService)
    : ControllerBase
{
    [HttpPost]
    [ProducesResponseType<TransferResult>(
        StatusCodes.Status201Created)]
    public async Task<ActionResult<TransferResult>> CreateAsync(
        [FromBody] CreateTransferRequest request,
        CancellationToken cancellationToken)
    {
        var command = new CreateTransferCommand(
            request.Value,
            request.Payer,
            request.Payee);

        var result = await transferService.ExecuteAsync(
            command,
            cancellationToken);

        return StatusCode(
            StatusCodes.Status201Created,
            result);
    }
}