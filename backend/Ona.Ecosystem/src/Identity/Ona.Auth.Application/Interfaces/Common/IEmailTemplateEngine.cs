namespace Ona.Auth.Application.Interfaces.Common
{
    public interface IEmailTemplateEngine
    {
        Task<string> RenderTemplateAsync<T>(string templateName, T model);
    }
}
