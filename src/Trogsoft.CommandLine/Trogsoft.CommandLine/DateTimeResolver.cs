using System;
using System.Collections.Generic;
using System.Text;

namespace Trogsoft.CommandLine
{
    public class DateTimeResolver : ITypeResolver<DateTime>
    {
        public DateTime Resolve(string value)
        {
            if (DateTime.TryParse(value, out DateTime dt))
            {
                return dt;
            }
            else
            {
                throw new Exception();
            }
        }
    }
}
