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
    }
}
