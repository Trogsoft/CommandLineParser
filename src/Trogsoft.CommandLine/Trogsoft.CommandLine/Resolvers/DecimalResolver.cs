using System;
using System.Collections.Generic;
using System.Text;

namespace Trogsoft.CommandLine.Resolvers
{
    public class DecimalResolver : TypeResolver<decimal>
    {
        public override decimal Resolve(string value)
        {
            if (decimal.TryParse(value, out decimal result))
                return result;
            else
                throw new ResolverException("Unable to parse decimal.", ParserErrorCodes.ERR_RESOLVER_ERROR);
        }
    }
}
