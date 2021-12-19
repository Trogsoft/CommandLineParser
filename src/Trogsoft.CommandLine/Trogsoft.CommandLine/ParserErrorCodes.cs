using System;
using System.Collections.Generic;
using System.Text;

namespace Trogsoft.CommandLine
{
    public class ParserErrorCodes
    {
        public static readonly int ERR_INVALID_VERB = 0x800;
        public static readonly int ERR_INVALID_OPERATION = 0x801;
        public static readonly int ERR_PARAMETER_MISSING = 0x805;
        public static readonly int ERR_INVALID_PARAMETER = 0x806;
        public static readonly int ERR_MULTIPLE_DEFAULT_VERBS = 0x807;
        public static readonly int ERR_RESOLVER_ERROR = 0x808;
        public static readonly int ERR_MULTIPLE_PARAMETER_TYPES = 0x810;
        public static readonly int ERR_MULTIPLE_MODEL_PARAMETERS = 0x812;
        public static readonly int ERR_MODEL_TYPE_HAS_PARAMETER_ATTRIBUTE = 0x814;
        public static readonly int ERR_DEFAULTED_TO_HELP = 0x816;
    }
}
