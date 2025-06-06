using YamlDotNet.Core.Events;
using YamlDotNet.Serialization;

namespace InfraScribe.CLI.Resolvers;

public class AwsTagIgnoringResolver : INodeTypeResolver
{
    public bool Resolve(NodeEvent nodeEvent, ref Type currentType)
    {
        if (nodeEvent.Tag != null && !nodeEvent.Tag.IsEmpty && nodeEvent.Tag.Value.StartsWith("!"))
        {
            currentType = typeof(object);
            return true;
        }
        return false;
    }
}
