using System;
using System.Collections.Generic;
using System.Text;

namespace Trogsoft.CommandLine.Resolvers
{
    public class DecimalResolver : TypeResolver<decimal>
    {
        public override ResolutionResult<decimal> Resolve(string value)
        {
            if (decimal.TryParse(value, out decimal result))
                return Success(result);
            else
                return InvalidValue();
        }
    }
}
