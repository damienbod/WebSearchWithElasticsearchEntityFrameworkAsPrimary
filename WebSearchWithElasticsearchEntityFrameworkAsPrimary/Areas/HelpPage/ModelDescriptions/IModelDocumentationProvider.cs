using System;
using System.Reflection;

namespace WebSearchWithElasticsearchEntityFrameworkAsPrimary.Areas.HelpPage.ModelDescriptions
{
    public interface IModelDocumentationProvider
    {
        string GetDocumentation(MemberInfo member);

        string GetDocumentation(Type type);
    }
}