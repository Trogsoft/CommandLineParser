using System;
using System.Collections.Generic;
using System.Text;

namespace Trogsoft.CommandLine
{
    public class UriResolver : TypeResolver<Uri>
    {
        public override ResolutionResult<Uri> Resolve(string value)
        {
            if (Uri.TryCreate(value, UriKind.Absolute, out Uri uri))
                return Success(uri);
            else
                return InvalidValue();
        }
    }
}
 