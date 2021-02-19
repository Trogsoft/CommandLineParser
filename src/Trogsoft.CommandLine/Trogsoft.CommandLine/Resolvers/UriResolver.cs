using System;
using System.Collections.Generic;
using System.Text;

namespace Trogsoft.CommandLine
{
    public class UriResolver : TypeResolver<Uri>
    {
        public override Uri Resolve(string value)
        {
            if (Uri.TryCreate(value, UriKind.Absolute, out Uri uri))
                return uri;
            else
                throw new ResolverException("Unable to parse URI.", ParserErrorCodes.ERR_RESOLVER_ERROR);
        }
    }
}
 