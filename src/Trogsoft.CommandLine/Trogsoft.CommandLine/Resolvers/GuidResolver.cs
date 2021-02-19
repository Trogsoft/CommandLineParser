using System;
using System.Collections.Generic;
using System.Text;

namespace Trogsoft.CommandLine
{
    public class GuidResolver : TypeResolver<Guid>
    {
        public override Guid Resolve(string value)
        {
            if (Guid.TryParse(value, out Guid result))
                return result;
            else
                throw new ResolverException("Unable to parse GUID.", ParserErrorCodes.ERR_RESOLVER_ERROR);
        }
    }
}
