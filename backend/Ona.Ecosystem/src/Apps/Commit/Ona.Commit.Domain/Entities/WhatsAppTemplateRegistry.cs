using Ona.Core.Entities;

namespace Ona.Commit.Domain.Entities
{
    public class WhatsAppTemplateRegistry : TenantEntity
    {
        public string LogicalName { get; set; } = string.Empty;
        public string MetaTemplateName { get; set; } = string.Empty;
        public string LanguageCode { get; set; } = "pt_BR";

        public WhatsAppTemplateRegistry()
        {
        }

        public WhatsAppTemplateRegistry(string logicalName, string metaTemplateName, string languageCode = "pt_BR")
        {
            LogicalName = logicalName;
            MetaTemplateName = metaTemplateName;
            LanguageCode = languageCode;
        }

        public void UpdateMetaTemplate(string metaTemplateName, string languageCode)
        {
            MetaTemplateName = metaTemplateName;
            LanguageCode = languageCode;
            Update();
        }
    }
}
