using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Trogsoft.CommandLine
{
    public class Parser
    {

        const int ERR_NO_ARGUMENTS = 1;
        const int ERR_UNRECOGNISED_OPERATION = 2;
        const int ERR_METHOD_NOT_FOUND = 3;
        const int ERR_PARAMETER_MISSING = 4;
        const int ERR_INVALID_PARAMETER = 5;
        const int ERR_MULTIPLE_DEFAULT_VERBS = 6;
        const int ERR_RESOLVER_ERROR = 7;

        private readonly bool debug;
        private List<VerbDefinition> verbDefinitions = new List<VerbDefinition>();
        private List<TypeResolverDefinition> typeConverters = new List<TypeResolverDefinition>();

        public Parser(bool debug = false)
        {

            this.debug = debug;

            // pre-cache the verbs
            verbDefinitions = AppDomain.CurrentDomain.GetAssemblies().SelectMany(x =>
                x.GetTypes()
                 .Where(y => typeof(Verb).IsAssignableFrom(y) && y.GetCustomAttribute<VerbAttribute>() != null && y.IsPublic && !y.IsAbstract && !y.IsInterface)
                 .Select(z => new VerbDefinition(z.GetCustomAttribute<VerbAttribute>(), z))
            ).ToList();

            // precache type converters
            var typeHandlers = AppDomain.CurrentDomain.GetAssemblies().SelectMany(x =>
                x.GetTypes()
                .Where(y => typeof(ITypeResolver).IsAssignableFrom(y) && y.IsPublic && !y.IsAbstract && !y.IsInterface)
            ).ToList();

            typeHandlers.ToList().ForEach(x =>
            {
                var tr = new TypeResolverDefinition();
                tr.Converter = x;
                foreach (var i in x.GetInterfaces())
                {
                    if (i.Name.StartsWith(nameof(ITypeResolver)) && i.IsGenericType)
                    {
                        tr.DestinationType = i.GetGenericArguments().First();
                    }
                }
                typeConverters.Add(tr);
            });

            WriteDebug($"Found {verbDefinitions.Count} verbs.");
            WriteDebug($"Found {typeConverters.Count} type resolvers.");
            typeConverters.ForEach(x => WriteDebug($"Type Resolver: {x.Converter}; converts to {x.DestinationType}"));

        }

        private void WriteDebug(string v)
        {
            if (debug)
            {
                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.WriteLine("debug: " + v);
                Console.ResetColor();
            }
        }

        private int ConfigurationCheck()
        {

            var defaultVerbCount = verbDefinitions.Count(x => x.Verb.IsDefault);
            if (defaultVerbCount > 1)
            {
                Error("Misconfiguration.  Multiple verbs are configured as the default.");
                return ERR_MULTIPLE_DEFAULT_VERBS;
            }

            foreach (var badResolver in typeConverters.Where(x => x.DestinationType == null))
            {
                Error($"Type Resolver {badResolver.Converter.FullName} does not have a valid destination type.");
                return ERR_RESOLVER_ERROR;
            }

            return 0;

        }

        public int Run(string[] args)
        {

            int usedParameters = 0;
            string verbName = null;
            string operationName = null;

            // a little sanity checking
            var sanitycheck = ConfigurationCheck();
            if (sanitycheck > 0)
                return sanitycheck;

            if (!args.Any() || args.FirstOrDefault().StartsWith("-"))
            {

                var defaultVerb = verbDefinitions.SingleOrDefault(x => x.Verb.IsDefault);
                if (defaultVerb != null)
                {
                    verbName = defaultVerb.Verb.Name;
                }
                else
                {
                    Error("No arguments passed.");
                    Help();
                    return ERR_NO_ARGUMENTS;
                }

            }
            else
            {

                usedParameters = 1;
                verbName = args.First();
                operationName = args.Length > 1 ? args[1] : null;

            }

            // find the right type
            var verb = verbDefinitions.SingleOrDefault(x => x.Verb.Name.Equals(verbName, StringComparison.CurrentCultureIgnoreCase));

            if (verb == null)
            {
                Error($"{args.First()} is not a recognised operation.");
                return ERR_UNRECOGNISED_OPERATION;
            }

            var methods = verb.Type.GetMethods().Where(x => x.GetCustomAttribute<OperationAttribute>() != null).Select(x => (method: x, op: x.GetCustomAttribute<OperationAttribute>())).ToList();

            // find the method
            MethodInfo method = null;
            if (operationName == null)
            {
                if (methods.Any(x => x.op.IsDefault))
                {
                    method = methods.SingleOrDefault(x => x.op.IsDefault).method;
                }
            }
            else
            {
                if (methods.Any(x => x.method.Name.Equals(operationName, StringComparison.CurrentCultureIgnoreCase)))
                {
                    usedParameters++;
                    method = methods.SingleOrDefault(x => x.method.Name.Equals(operationName, StringComparison.CurrentCultureIgnoreCase)).method;
                }
                else if (methods.Any(x => x.op.Name != null && x.op.Name.Equals(operationName, StringComparison.CurrentCultureIgnoreCase)))
                {
                    usedParameters++;
                    method = methods.SingleOrDefault(x => x.op.Name != null && x.op.Name.Equals(operationName, StringComparison.CurrentCultureIgnoreCase)).method;
                }
                else if (methods.Any(x => x.op.IsDefault))
                {
                    method = methods.SingleOrDefault(x => x.op.IsDefault).method;
                }
            }

            if (method == null)
            {
                Error("No such action.");
                Help();
                return ERR_METHOD_NOT_FOUND;
            }

            var vi = Activator.CreateInstance(verb.Type);
            object result = null;
            try
            {
                var para = getMethodParameters(method, args.Skip(usedParameters).ToArray());
                result = method.Invoke(vi, para);
            }
            catch (ParameterMissingException ex)
            {
                Error($"Missing parameter: {ex.ParameterInfo.LongName ?? ex.ParameterInfo.ShortName.ToString()}");
                Help(verbName);
                return ERR_PARAMETER_MISSING;
            }
            catch (InvalidParameterException ex)
            {
                Error($"Invalid value for parameter {ex.ParameterInfo.LongName ?? ex.ParameterInfo.ShortName.ToString()}.");
                Help(verbName);
                return ERR_INVALID_PARAMETER;
            }

            if (result is int)
                return (int)result;

            return 0;

        }

        private object[] getMethodParameters(MethodInfo method, string[] args)
        {
            List<object> paras = new List<object>();
            var paraConfigList = method.GetCustomAttributes<ParameterAttribute>();

            foreach (var para in method.GetParameters())
            {

                var paraConfig = paraConfigList.SingleOrDefault(x => x.ShortName.ToString().Equals(para.Name, StringComparison.CurrentCultureIgnoreCase));

                if (paraConfig == null)
                    paraConfig = paraConfigList.SingleOrDefault(x => !string.IsNullOrWhiteSpace(x.LongName) && x.LongName.Equals(para.Name, StringComparison.CurrentCultureIgnoreCase));

                if (paraConfig == null)
                    if (para.Name.Length == 1)
                        paraConfig = new ParameterAttribute { ShortName = (char)para.Name.First(), IsRequired = !para.HasDefaultValue, Default = (para.HasDefaultValue ? para.DefaultValue : null) };
                    else
                        paraConfig = new ParameterAttribute { LongName = para.Name, IsRequired = !para.HasDefaultValue, Default = (para.HasDefaultValue ? para.DefaultValue : null) };

                var isListOfSimpleThings = false;

                if (typeof(IEnumerable).IsAssignableFrom(para.ParameterType) && para.ParameterType != typeof(string))
                {
                    if (para.ParameterType.IsGenericType)
                    {
                        var firstGenericParameter = para.ParameterType.GetGenericArguments().First();
                        isListOfSimpleThings = (firstGenericParameter.IsPrimitive || firstGenericParameter.IsEnum || firstGenericParameter == typeof(string));
                    }
                }

                if (para.ParameterType.IsPrimitive || para.ParameterType.IsEnum || para.ParameterType == typeof(string) || isListOfSimpleThings)
                {

                    var type = para.ParameterType;
                    var value = getParameterValue(type, paraConfig, args);

                    if (value == null)
                        paras.Add(paraConfig.Default);
                    else if (value != null)
                        paras.Add(value);

                }
                else if (typeConverters.Any(x => x.DestinationType == para.ParameterType))
                {

                    var value = resolveValue(para.ParameterType, paraConfig, args);
                    if (value == null)
                        paras.Add(paraConfig.Default);
                    else
                        paras.Add(value);

                }
                else
                {

                    // not a primitive, enum, string or list thereof
                    paras.Add(buildModel(para.ParameterType, args));

                }

            }

            if (paras.Any())
                return paras.ToArray();

            return null;
        }

        private object resolveValue(Type parameterType, ParameterAttribute paraConfig, string[] args)
        {

            var resolver = typeConverters.FirstOrDefault(x => x.DestinationType == parameterType);
            if (resolver == null)
                return null;

            var pi = getParameterInfo(paraConfig, args);
            if (pi.Exists && pi.HasValue)
            {

                var resolverType = Activator.CreateInstance(resolver.Converter);
                var resolveMethod = resolverType.GetType().GetMethod("Resolve");

                var value = (ResolutionResult)resolveMethod.Invoke(resolverType, new object[] { pi.Value });
                if (value.Success)
                {
                    var resultProperty = value.GetType().GetProperty("Result");
                    var val = resultProperty.GetValue(value);
                    return val;
                }
                else
                {
                    throw new InvalidParameterException(paraConfig);
                }

            }
            else
            {
                if (paraConfig.IsRequired)
                {
                    if (!pi.Exists)
                        throw new ParameterMissingException(paraConfig);
                    else
                        throw new InvalidParameterException(paraConfig);
                }
                return null;

            }

        }

        private CommandLineParameterInfo getParameterInfo(ParameterAttribute paraConfig, string[] args)
        {

            var argList = args.ToList();

            if (paraConfig.Position > -1)
            {
                if (argList.Count > paraConfig.Position)
                {
                    return new CommandLineParameterInfo
                    {
                        Exists = true,
                        HasValue = true,
                        Position = paraConfig.Position,
                        Value = argList[paraConfig.Position]
                    };
                }
                else
                {
                    if (paraConfig.IsRequired)
                    {
                        throw new ParameterMissingException(paraConfig);
                    }
                    else
                    {
                        return new CommandLineParameterInfo
                        {
                            Exists = false,
                            HasValue = false,
                        };
                    }
                }
            }

            var paraMarker = -1;
            if (paraConfig.ShortName != char.MinValue)
                if (args.Contains($"-{paraConfig.ShortName}"))
                    paraMarker = argList.IndexOf($"-{paraConfig.ShortName}");

            if (!string.IsNullOrWhiteSpace(paraConfig.LongName) && paraMarker == -1)
                if (args.Contains($"--{paraConfig.LongName}", StringComparer.CurrentCultureIgnoreCase))
                    paraMarker = argList.FindIndex(x => x.Equals($"--{paraConfig.LongName}", StringComparison.CurrentCultureIgnoreCase));

            var result = new CommandLineParameterInfo
            {
                Position = paraMarker,
                Exists = paraMarker > -1,
                HasValue = argList.Count > (paraMarker + 1) && paraMarker >= 0,
            };

            if (result.Exists && result.HasValue)
                result.Value = args[paraMarker + 1];

            return result;

        }

        private object getParameterValue(Type type, ParameterAttribute paraConfig, string[] args)
        {

            var pi = getParameterInfo(paraConfig, args);
            if (type == typeof(bool))
            {
                return pi.Exists;
            }

            bool isList = typeof(IEnumerable).IsAssignableFrom(type) && type != typeof(string);

            if (pi.HasValue)
            {

                var value = pi.Value;

                if (type.IsEnum)
                {
                    if (Enum.TryParse(type, value, ignoreCase: true, out object res))
                    {
                        return res;
                    }
                    else
                    {
                        throw new InvalidParameterException(paraConfig);
                    }
                }

                if (isList)
                {
                    var separator = " ";
                    if (!string.IsNullOrWhiteSpace(paraConfig.ListSeparator))
                        separator = paraConfig.ListSeparator;

                    var splitValue = value.Split(new string[] { separator }, StringSplitOptions.RemoveEmptyEntries);

                    var underlyingType = type.GetGenericArguments().FirstOrDefault();
                    var constructedListType = typeof(List<>).MakeGenericType(underlyingType);
                    var constructedList = (IList)Activator.CreateInstance(constructedListType);

                    splitValue.ToList().ForEach(x => constructedList.Add(Convert.ChangeType(x, underlyingType)));

                    return constructedList;

                }
                else
                {
                    return Convert.ChangeType(value, type);
                }
            }
            else
            {
                if (paraConfig.IsRequired)
                {
                    throw new ParameterMissingException(paraConfig);
                }
                else
                {
                    return null;
                }
            }

        }


        private object buildModel(Type type, string[] args)
        {

            var model = Activator.CreateInstance(type);
            foreach (var prop in type.GetProperties())
            {
                if (prop.GetSetMethod() != null)
                {
                    bool isRequired = false;
                    var currentValue = prop.GetValue(model);

                    var paraConfig = prop.GetCustomAttribute<ParameterAttribute>();
                    if (paraConfig == null)
                    {

                        if (prop.PropertyType.IsValueType)
                            isRequired = currentValue.Equals(Activator.CreateInstance(prop.PropertyType));
                        else
                            isRequired = currentValue == null;

                        if (prop.Name.Length == 1)
                            paraConfig = new ParameterAttribute { ShortName = (char)prop.Name.First(), IsRequired = isRequired, Default = isRequired ? null : currentValue };
                        else
                            paraConfig = new ParameterAttribute { LongName = prop.Name, IsRequired = isRequired, Default = isRequired ? null : currentValue };
                    }

                    object propValue;
                    if (typeConverters.Any(x => x.DestinationType == prop.PropertyType))
                    {
                        propValue = resolveValue(prop.PropertyType, paraConfig, args);
                    }
                    else
                    {
                        propValue = getParameterValue(prop.PropertyType, paraConfig, args);
                    }

                    if (propValue == null && !isRequired)
                        propValue = currentValue;

                    prop.SetValue(model, propValue);

                }
            }

            return model;

        }

        private void Help(string verb = null, string action = null)
        {

            var verbs = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(x => x.GetTypes().Where(y => typeof(Verb).IsAssignableFrom(y) && y.IsPublic && !y.IsInterface && !y.IsAbstract && y.GetCustomAttribute<VerbAttribute>() != null)
                    .Select(y => (verbInfo: y.GetCustomAttribute<VerbAttribute>(), verbType: y)));

            if (!verbs.Any())
            {
                Error("This command contains no verbs.");
                return;
            }

            var processName = System.Diagnostics.Process.GetCurrentProcess().ProcessName.ToLower();
            Console.WriteLine();
            Console.WriteLine("Help");
            Console.WriteLine();
            Console.WriteLine("Usage: ");
            Console.WriteLine($"  {processName} verb [action] [parameters]");
            Console.WriteLine();

            if (verb == null)
            {
                Console.WriteLine("Possible verbs:");
                Console.WriteLine();
                var maxVerbWidth = verbs.Max(y => y.verbInfo.Name.Length) + 2;
                var helpWidth = 80 - maxVerbWidth - 2;

                foreach (var v in verbs.OrderBy(x => x.verbInfo.Name))
                {
                    Console.ForegroundColor = ConsoleColor.White;
                    Console.Write(v.verbInfo.Name.ToLower().PadLeft(maxVerbWidth) + "  ");
                    Console.ResetColor();
                    if (!string.IsNullOrWhiteSpace(v.verbInfo.HelpText))
                    {
                        var helpWords = v.verbInfo.HelpText.Split();
                        var wCount = 0;
                        var line = "";
                        List<string> lines = new List<string>();

                        foreach (var word in helpWords)
                        {
                            line += word + " ";
                            wCount += word.Length + 1;
                            if (wCount >= helpWidth)
                            {
                                wCount = 0;
                                lines.Add(line);
                                line = "";
                            }
                        }

                        if (!string.IsNullOrWhiteSpace(line))
                            lines.Add(line);

                        foreach (var l in lines)
                        {
                            Console.WriteLine(l.Trim());
                            Console.Write(new string(Enumerable.Range(0, maxVerbWidth + 2).Select(x => ' ').ToArray()));
                        }

                    }
                    Console.WriteLine();
                    Console.WriteLine();
                }
                Console.WriteLine($"For more specific help, try {processName} help verb");
            }

            Console.WriteLine();

        }

        private void Error(string message)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine(message);
            Console.ResetColor();
        }

        public ParseResult Parse(string[] args)
        {
            return new ParseResult();
        }

    }
}
