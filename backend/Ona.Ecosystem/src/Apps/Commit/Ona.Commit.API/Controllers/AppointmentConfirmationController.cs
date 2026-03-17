using Microsoft.AspNetCore.Mvc;
using Ona.Commit.Application.Interfaces.Services;
using Ona.Commit.Domain.Enums;
using System.Globalization;

namespace Ona.Commit.API.Controllers
{
    [ApiController]
    [Route("api/confirmation")]
    public class AppointmentConfirmationController : ControllerBase
    {
        private readonly IAppointmentAppService _appointmentService;

        public AppointmentConfirmationController(IAppointmentAppService appointmentService)
        {
            _appointmentService = appointmentService;
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetConfirmationPage(Guid id)
        {
            try
            {
                var appointment = await _appointmentService.GetByIdAsync(id);
                if (appointment == null)
                    return NotFound("Agendamento não encontrado.");

                bool isConfirmed = appointment.Status == AppointmentStatus.Confirmed;
                bool isCanceled = appointment.Status == AppointmentStatus.Canceled;

                string statusText = "";
                if (isConfirmed) statusText = "Agendamento Confirmado";
                if (isCanceled) statusText = "Agendamento Cancelado";

                var html = GenerateHtml(appointment.Id, appointment.Customer?.Name ?? "Cliente", appointment.StartDate, statusText, isConfirmed, isCanceled);
                return Content(html, "text/html");
            }
            catch (Exception)
            {
                return Content("<h1>Erro ao carregar agendamento</h1>", "text/html");
            }
        }

        [HttpPost("{id}/confirm")]
        public async Task<IActionResult> Confirm(Guid id)
        {
            try
            {
                await _appointmentService.ConfirmAsync(id);
                return Ok(new { message = "Confirmado com sucesso" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPost("{id}/cancel")]
        public async Task<IActionResult> Cancel(Guid id)
        {
            try
            {
                await _appointmentService.CancelAsync(id);
                return Ok(new { message = "Cancelado com sucesso" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        private string GenerateHtml(Guid id, string customerName, DateTimeOffset date, string statusText, bool isConfirmed, bool isCanceled)
        {
            var dateStr = date.ToString("dd/MM/yyyy", new CultureInfo("pt-BR"));
            var timeStr = date.ToString("HH:mm", new CultureInfo("pt-BR"));

            var css = @"
                <style>
                    :root {
                        --primary: #2563eb;
                        --primary-hover: #1d4ed8;
                        --success: #10b981;
                        --danger: #ef4444;
                        --bg: #0f172a;
                        --card-bg: #1e293b;
                        --text: #f8fafc;
                        --text-muted: #94a3b8;
                    }
                    body {
                        font-family: 'Inter', system-ui, -apple-system, sans-serif;
                        background-color: var(--bg);
                        color: var(--text);
                        display: flex;
                        justify-content: center;
                        align-items: center;
                        min-height: 100vh;
                        margin: 0;
                        padding: 1rem;
                    }
                    .card {
                        background-color: var(--card-bg);
                        border-radius: 1rem;
                        padding: 2rem;
                        max-width: 400px;
                        width: 100%;
                        box-shadow: 0 10px 25px -5px rgba(0, 0, 0, 0.5);
                        text-align: center;
                        border: 1px solid rgba(255,255,255,0.05);
                    }
                    h1 { margin-bottom: 0.5rem; font-size: 1.5rem; }
                    p { color: var(--text-muted); margin-bottom: 1.5rem; line-height: 1.5; }
                    .details {
                        background: rgba(255,255,255,0.05);
                        padding: 1rem;
                        border-radius: 0.5rem;
                        margin-bottom: 2rem;
                    }
                    .btn-group {
                        display: flex;
                        gap: 2rem;
                        flex-direction: column;
                    }
                    button {
                        border: none;
                        padding: 1rem 1.5rem;
                        border-radius: 0.75rem;
                        font-weight: 600;
                        font-size: 1.1rem;
                        cursor: pointer;
                        transition: all 0.2s;
                        width: 100%;
                    }
                    .btn-confirm {
                        background: linear-gradient(135deg, var(--success), #059669);
                        color: white;
                        box-shadow: 0 4px 6px -1px rgba(0, 0, 0, 0.1), 0 2px 4px -1px rgba(0, 0, 0, 0.06);
                    }
                    .btn-confirm:active { transform: scale(0.98); }
                    .btn-cancel {
                        background: transparent;
                        color: var(--text-muted);
                        font-size: 0.9rem;
                        padding: 0.5rem;
                        font-weight: normal;
                        text-decoration: underline;
                        opacity: 0.8;
                    }
                    .btn-cancel:hover { opacity: 1; color: var(--danger); }
                    
                    .status-badge {
                        display: inline-block;
                        padding: 0.5rem 1rem;
                        border-radius: 2rem;
                        font-size: 0.875rem;
                        font-weight: 600;
                        margin-bottom: 1rem;
                    }
                    .status-confirmed { background: rgba(16, 185, 129, 0.2); color: var(--success); }
                    .status-canceled { background: rgba(239, 68, 68, 0.2); color: var(--danger); }

                    .loader {
                        border: 3px solid rgba(255,255,255,0.1);
                        border-radius: 50%;
                        border-top: 3px solid var(--primary);
                        width: 20px;
                        height: 20px;
                        animation: spin 1s linear infinite;
                        margin: 0 auto;
                        display: none;
                    }
                    @keyframes spin { 0% { transform: rotate(0deg); } 100% { transform: rotate(360deg); } }
                </style>
            ";

            string buttonsHtml = "";
            if (!isCanceled && !isConfirmed)
            {
                buttonsHtml = $@"
                <div class='btn-group' id='actions'>
                    <button onclick='confirmApp()' class='btn-confirm'>Confirmar Presença</button>
                    <button onclick='cancelApp()' class='btn-cancel'>Cancelar Agendamento</button>
                </div>
                ";
            }
            else
            {
                string badgeClass = isConfirmed ? "status-confirmed" : "status-canceled";
                buttonsHtml = $"<div class='status-badge {badgeClass}'>{statusText}</div>";

                if (isConfirmed)
                {
                    buttonsHtml += @"
                    <div class='btn-group' id='actions' style='margin-top:1rem;'>
                        <button onclick='cancelApp()' class='btn-cancel'>Cancelar Agendamento</button>
                    </div>";
                }
            }

            var html = $@"
            <!DOCTYPE html>
            <html lang='pt-BR'>
            <head>
                <meta charset='UTF-8'>
                <meta name='viewport' content='width=device-width, initial-scale=1.0'>
                <title>Confirmação de Agendamento</title>
                {css}
            </head>
            <body>
                <div class='card'>
                    <h1>Olá, {customerName}</h1>
                    <p>Você possui um agendamento marcado.</p>
                    
                    <div class='details'>
                        <div style='font-size: 1.25rem; font-weight: bold; margin-bottom: 0.25rem;'>{timeStr}</div>
                        <div style='color: var(--text-muted);'>{dateStr}</div>
                    </div>

                    {buttonsHtml}
                    <div id='loader' class='loader'></div>
                    <p id='msg' style='margin-top: 1rem; font-size: 0.875rem;'></p>
                </div>

                <script>
                    const id = '{id}';
                    const actionsDiv = document.getElementById('actions');
                    const loader = document.getElementById('loader');
                    const msg = document.getElementById('msg');

                    async function callApi(action) {{
                        if(!confirm('Tem certeza?')) return;
                        
                        if(actionsDiv) actionsDiv.style.opacity = '0.5';
                        loader.style.display = 'block';
                        msg.innerText = '';

                        try {{
                            const res = await fetch(`/api/confirmation/${{id}}/${{action}}`, {{ method: 'POST' }});
                            const data = await res.json();
                            
                            if(res.ok) {{
                                msg.style.color = 'var(--success)';
                                msg.innerText = data.message;
                                setTimeout(() => window.location.reload(), 1500);
                            }} else {{
                                msg.style.color = 'var(--danger)';
                                msg.innerText = data.message || 'Erro ao processar';
                                if(actionsDiv) actionsDiv.style.opacity = '1';
                            }}
                        }} catch (e) {{
                            msg.style.color = 'var(--danger)';
                            msg.innerText = 'Erro de conexão';
                            if(actionsDiv) actionsDiv.style.opacity = '1';
                        }} finally {{
                            loader.style.display = 'none';
                        }}
                    }}

                    function confirmApp() {{ callApi('confirm'); }}
                    function cancelApp() {{ callApi('cancel'); }}
                </script>
            </body>
            </html>
            ";

            return html;
        }
    }
}
