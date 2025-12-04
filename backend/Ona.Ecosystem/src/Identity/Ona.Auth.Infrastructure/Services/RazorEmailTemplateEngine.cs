using Ona.Auth.Application.Interfaces.Common;
using RazorLight;
using System.Reflection;

namespace Ona.Auth.Infrastructure.Services
{
    public class RazorEmailTemplateEngine : IEmailTemplateEngine
    {
        private readonly RazorLightEngine _engine;

        public RazorEmailTemplateEngine()
        {
            var assembly = Assembly.GetExecutingAssembly();
            var assemblyName = assembly.GetName().Name;

            _engine = new RazorLightEngineBuilder()
                .UseEmbeddedResourcesProject(assembly, $"{assemblyName}.Templates.Emails")
                .UseMemoryCachingProvider()
                .SetOperatingAssembly(assembly)
                .EnableDebugMode()
                .Build();
        }

        public async Task<string> RenderTemplateAsync<T>(string templateName, T model)
        {
            try
            {
                var templateKey = $"{templateName}.cshtml";
                string result = await _engine.CompileRenderAsync(templateKey, model);
                return result;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException(
                    $"Erro ao renderizar template '{templateName}.cshtml'. " +
                    $"Certifique-se de que o template está configurado como Embedded Resource. " +
                    $"Erro original: {ex.Message}", ex);
            }
        }
    }
}
