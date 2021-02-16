using System;
using System.Collections.Generic;
using System.Text;

namespace Trogsoft.CommandLine
{
    public class DateTimeResolver : TypeResolver<DateTime>
    {
        public override ResolutionResult<DateTime> Resolve(string value)
        {
            if (DateTime.TryParse(value, out DateTime dt))
                return Success(dt);
            else
                return InvalidValue();
        }
    }
}
