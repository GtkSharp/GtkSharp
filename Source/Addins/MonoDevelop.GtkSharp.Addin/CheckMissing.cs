using Mono.Addins;

namespace MonoDevelop.GtkSharp.Addin
{
    public class CheckMissing : ConditionType
    {
        public override bool Evaluate(NodeElement conditionNode)
        {
            return AddinManager.GetExtensionNode("/MonoDevelop/Ide/ProjectTemplateCategories/crossplat/app") == null;
        }
    }
}
