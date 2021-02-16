using System;
using System.Collections.Generic;
using System.Text;

namespace Trogsoft.CommandLine
{
    public class GuidResolver : TypeResolver<Guid>
    {
        public override ResolutionResult<Guid> Resolve(string value)
        {
            if (Guid.TryParse(value, out Guid result))
                return Success(result);
            else
                return InvalidValue();
        }
    }
}
