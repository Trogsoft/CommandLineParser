using System;
using System.Collections.Generic;
using System.Text;

namespace Trogsoft.CommandLine
{
    public class DateTimeResolver : TypeResolver<DateTime>
    {
        public override DateTime Resolve(string value)
        {
            if (DateTime.TryParse(value, out DateTime dt))
                return dt;
            else
                throw new ResolverException("Unable to parse datetime.", ParserErrorCodes.ERR_RESOLVER_ERROR);
        }
    }
}
