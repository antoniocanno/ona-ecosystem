using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Ona.Commit.Application.Interfaces.Services;
using Ona.Core.Common.Enums;
using Ona.Core.Interfaces;
using Ona.ServiceDefaults.Attributes;

namespace Ona.Commit.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class WhatsappController : ControllerBase
    {
        private readonly ICurrentTenant _currentTenant;
        private readonly IWhatsAppAppService _appService;
        private readonly ILogger<WhatsappController> _logger;

        public WhatsappController(
            ICurrentTenant currentTenant,
            IWhatsAppAppService appService,
            ILogger<WhatsappController> logger)
        {
            _currentTenant = currentTenant;
            _appService = appService;
            _logger = logger;
        }

        /// <summary>
        /// Inicia a conexão do WhatsApp e retorna o QR Code
        /// </summary>
        [HttpPost("connect")]
        [AuthorizeRoles(Role.Operator)]
        public async Task<IActionResult> Connect()
        {
            try
            {
                var result = await _appService.ConnectAsync(_currentTenant.Id.Value);

                if (result.Status == "connected")
                {
                    return Ok(new
                    {
                        message = "WhatsApp já está conectado",
                        isConnected = true,
                        instanceName = result.InstanceName
                    });
                }

                return Ok(new
                {
                    qrCode = result.QrCodeBase64,
                    instanceName = result.InstanceName,
                    status = result.Status,
                    message = "Escaneie o QR Code com o WhatsApp para conectar"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao iniciar conexão WhatsApp");
                return StatusCode(500, new { message = "Erro ao conectar WhatsApp", error = ex.Message });
            }
        }

        /// <summary>
        /// Obtém um novo QR Code para conexão
        /// </summary>
        [HttpGet("qrcode")]
        [AuthorizeRoles(Role.Operator)]
        public async Task<IActionResult> GetQrCode()
        {
            try
            {
                var result = await _appService.GetQrCodeAsync(_currentTenant.Id.Value);

                return Ok(new
                {
                    qrCode = result.QrCodeBase64,
                    expiresIn = result.ExpiresIn,
                    status = result.Status
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao obter QR Code");
                return StatusCode(500, new { message = "Erro ao obter QR Code", error = ex.Message });
            }
        }

        /// <summary>
        /// Verifica o status da conexão do WhatsApp
        /// </summary>
        [HttpGet("status")]
        [AuthorizeRoles(Role.Operator)]
        public async Task<IActionResult> GetStatus()
        {
            try
            {
                var status = await _appService.GetStatusAsync(_currentTenant.Id.Value);

                return Ok(new
                {
                    isConnected = status.IsConnected,
                    state = status.State,
                    phoneNumber = status.PhoneNumber,
                    instanceName = status.InstanceName
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao verificar status");
                return StatusCode(500, new { message = "Erro ao verificar status", error = ex.Message });
            }
        }

        /// <summary>
        /// Desconecta e remove a instância do WhatsApp
        /// </summary>
        [HttpDelete("disconnect")]
        [AuthorizeRoles(Role.Operator)]
        public async Task<IActionResult> Disconnect()
        {
            try
            {
                await _appService.DisconnectAsync(_currentTenant.Id.Value);
                return Ok(new { message = "WhatsApp desconectado com sucesso" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao desconectar WhatsApp");
                return StatusCode(500, new { message = "Erro ao desconectar", error = ex.Message });
            }
        }

        /// <summary>
        /// Envia uma mensagem de teste (apenas para debug/validação)
        /// </summary>
        [HttpPost("send-test")]
        [AuthorizeRoles(Role.Operator)]
        public async Task<IActionResult> SendTestMessage([FromBody] SendTestMessageRequest request)
        {
            try
            {
                var messageId = await _appService.SendTestMessageAsync(
                    _currentTenant.Id.Value,
                    request.PhoneNumber,
                    request.Message
                );

                return Ok(new
                {
                    success = true,
                    messageId = messageId,
                    message = "Mensagem enviada com sucesso"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao enviar mensagem de teste");
                return StatusCode(500, new { message = "Erro ao enviar mensagem", error = ex.Message });
            }
        }
        /// <summary>
        /// Envia uma mensagem com botões de teste (apenas para debug/validação)
        /// </summary>
        [HttpPost("send-buttons-test")]
        [AuthorizeRoles(Role.Operator)]
        public async Task<IActionResult> SendTestButtonMessage([FromBody] SendTestButtonMessageRequest request)
        {
            try
            {
                var buttons = request.Buttons.Select(b => new Ona.Commit.Domain.Interfaces.Gateways.WhatsAppButton
                {
                    Type = b.Type,
                    DisplayText = b.DisplayText,
                    Id = b.Id,
                    CopyCode = b.CopyCode,
                    Url = b.Url,
                    PhoneNumber = b.PhoneNumber,
                    Currency = b.Currency,
                    Name = b.Name,
                    KeyType = b.KeyType,
                    Key = b.Key
                }).ToList();

                var messageId = await _appService.SendButtonsMessageAsync(
                    _currentTenant.Id.Value,
                    request.PhoneNumber,
                    request.Title,
                    request.Description,
                    request.Footer,
                    buttons
                );

                return Ok(new
                {
                    success = true,
                    messageId = messageId,
                    message = "Mensagem com botões enviada com sucesso"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao enviar mensagem com botões de teste");
                return StatusCode(500, new { message = "Erro ao enviar mensagem", error = ex.Message });
            }
        }
    }

    public class SendTestMessageRequest
    {
        public string PhoneNumber { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
    }

    public class SendTestButtonMessageRequest
    {
        public string PhoneNumber { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Footer { get; set; } = string.Empty;
        public List<TestButton> Buttons { get; set; } = new();
    }

    public class TestButton
    {
        public string Type { get; set; } = "reply";
        public string DisplayText { get; set; } = string.Empty;
        public string? Id { get; set; }
        public string? CopyCode { get; set; }
        public string? Url { get; set; }
        public string? PhoneNumber { get; set; }
        public string? Currency { get; set; }
        public string? Name { get; set; }
        public string? KeyType { get; set; }
        public string? Key { get; set; }
    }
}
