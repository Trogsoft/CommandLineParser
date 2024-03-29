﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Trogsoft.CommandLine
{
    public class Parser
    {

        private readonly bool debug;
        private List<VerbDefinition> verbDefinitions = new List<VerbDefinition>();

        public Parser(bool debug = false)
        {

            this.debug = debug;

            loadVerbs();
            loadTypeConverters();

        }

        private void loadVerbs()
        {
            verbDefinitions = AppDomain.CurrentDomain.GetAssemblies().SelectMany(x =>
                x.GetTypes()
                 .Where(y => typeof(Verb).IsAssignableFrom(y) && y.GetCustomAttribute<VerbAttribute>() != null && y.IsPublic && !y.IsAbstract && !y.IsInterface)
                 .Select(z => new VerbDefinition(z.GetCustomAttribute<VerbAttribute>(), z))
            ).ToList();

            writeDebug($"Found {verbDefinitions.Count} verbs.");
        }

        private void loadTypeConverters()
        {
            writeDebug($"Found {TypeResolver.Count()} type resolvers.");
        }

        private void writeDebug(string v)
        {
            if (debug)
            {
                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.WriteLine("debug: " + v);
                Console.ResetColor();
            }
        }

        private int configurationCheck()
        {

            var defaultVerbCount = verbDefinitions.Count(x => x.Verb.IsDefault);
            if (defaultVerbCount > 1)
            {
                Error("Misconfiguration.  Multiple verbs are configured as the default.");
                return ParserErrorCodes.ERR_MULTIPLE_DEFAULT_VERBS;
            }

            return 0;

        }

        public int Run(string[] args)
        {

            int usedParameters = 0;
            string verbName = null;
            string operationName = null;
            VerbDefinition verb;
            MethodInfo operation;

            // a little sanity checking
            var sanitycheck = configurationCheck();
            if (sanitycheck > 0)
                return sanitycheck;

            // help
            if (helpCalled(args))
                return 0;

            try
            {
                verb = getVerb(args, out int usedArgs);
                verbName = verb.Verb.Name;
                usedParameters += usedArgs;
            }
            catch (UnspecifiedVerbException ex)
            {
                helpCalled(args, true);
                return ex.HResult;
            }
            catch (Exception ex)
            {
                Error(ex.Message);
                return ex.HResult;
            }

            try
            {
                operation = getOperation(verb, args.Skip(usedParameters).ToArray(), out int usedArgs);
                if (operation == null)
                {
                    helpCalled(args, true);
                    return ParserErrorCodes.ERR_DEFAULTED_TO_HELP;
                }
                usedParameters += usedArgs;
                operationName = operation.Name;
            }
            catch (Exception ex)
            {
                Error(ex.Message);
                return ex.HResult;
            }

            var activeVerb = Activator.CreateInstance(verb.Type);
            object result = null;
            try
            {
                var parameterList = getOperationParameterValues(operation, args.Skip(usedParameters).ToArray());
                result = operation.Invoke(activeVerb, parameterList.ToArray());
            }
            catch (TargetInvocationException ex)
            {
                if (ex.InnerException != null)
                {
                    Error(ex.InnerException.Message);
                    Help(verbName, operationName);
                    return ex.InnerException.HResult;
                }
                else
                {
                    Error(ex.Message);
                    Help(verbName, operationName);
                    return ex.HResult;
                }
            }
            catch (Exception ex)
            {
                Error(ex.Message);
                Help(verbName, operationName);
                return ex.HResult;
            }

            if (result is int)
                return (int)result;

            return 0;

        }

        private MethodInfo getOperation(VerbDefinition verb, string[] args, out int usedArgs)
        {

            usedArgs = 0;
            var methods = verb.Type.GetMethods().Where(x => x.GetCustomAttribute<OperationAttribute>() != null).Select(x => new { method = x, op = x.GetCustomAttribute<OperationAttribute>() }).ToList();

            // if there are no arguments or the first argument starts with a hyphen, we assume a default operation.
            var defaultOperation = methods.SingleOrDefault(x => x.op.IsDefault);
            if (!args.Any() || args.First().StartsWith("-"))
            {
                if (defaultOperation != null)
                    return defaultOperation.method;
                else
                {
                    return null;
                }
            }

            var firstArg = args.First();
            var op = methods.SingleOrDefault(x => string.Equals(x.op.Name, firstArg, StringComparison.CurrentCultureIgnoreCase) || x.method.Name.Equals(firstArg, StringComparison.CurrentCultureIgnoreCase));
            if (op == null)
            {
                if (defaultOperation != null)
                {
                    // no operation with the same name as the first argument.  See if the default operation on this verb has a positional parameter.
                    var methodParameters = getMethodParameterInfo(defaultOperation.method);
                    if (methodParameters.Any(x => x.Position == 0))
                    {
                        return defaultOperation.method;
                    }
                    throw new UnspecifiedOperationException(verb.Type.Name, firstArg);
                }
                else
                {
                    throw new UnspecifiedOperationException(verb.Type.Name, firstArg);
                }
            }

            usedArgs = 1;
            return op.method;

        }

        private VerbDefinition getVerb(string[] args, out int usedArgs)
        {

            usedArgs = 0;

            // no args, see if there's a default verb
            if (!args.Any() || args.First().StartsWith("-"))
            {
                var defaultVerb = verbDefinitions.SingleOrDefault(x => x.Verb.IsDefault);
                if (defaultVerb != null)
                    return defaultVerb;
                else
                    throw new UnspecifiedVerbException("@default");
            }

            // see if the first arg is a valid verb
            var firstArg = args.First();
            var verb = verbDefinitions.SingleOrDefault(x => x.Verb.Name.Equals(firstArg, StringComparison.CurrentCultureIgnoreCase));
            if (verb == null)
                throw new UnspecifiedVerbException(firstArg);

            usedArgs = 1;
            return verb;

        }

        private bool helpCalled(string[] args, bool force = false)
        {
            if (force || args.Any() && args.FirstOrDefault().Equals("--help", StringComparison.CurrentCultureIgnoreCase))
            {

                // If force is enabled, and none of the parameters is --help, we assume we defaulted into this method
                // and as such, the parameter list check needs to be shorter.
                //var modifier = (force && !args.Contains("--help")) ? 1 : 0;

                var argsWithoutHelp = args.Except(new[] { "--help" }, StringComparer.CurrentCultureIgnoreCase).ToArray();

                if (argsWithoutHelp.Length == 0)
                    Help();
                else if (argsWithoutHelp.Length == 1)
                    Help(argsWithoutHelp[0]);
                else if (argsWithoutHelp.Length >= 2)
                    Help(argsWithoutHelp[0], argsWithoutHelp[1]);

                return true;

            }
            return false;
        }

        private List<Parameter> getOperationParameterRequirements(MethodInfo method)
        {

            var pr = new List<Parameter>();
            var paraAttrList = method.GetCustomAttributes<ParameterAttribute>();

            foreach (var para in method.GetParameters())
            {
                var paraConfig = paraAttrList.SingleOrDefault(x => x.ShortName.ToString().Equals(para.Name, StringComparison.CurrentCultureIgnoreCase) || string.Equals(x.LongName, para.Name, StringComparison.CurrentCultureIgnoreCase));
                Parameter p = Parameter.Create(para, paraConfig);
                pr.Add(p);
            }

            int simpleParameters = pr.Count(x => x is SimpleParameter || x is ResolvedParameter);
            int modelParameters = pr.Count(x => x is ModelParameter);

            if (simpleParameters > 0 && modelParameters > 0)
                throw new MultipleIncompatibleParameterTypesException(method);

            if (modelParameters > 1)
                throw new MultipleModelParametersException(method);

            return pr;

        }

        private List<object> getOperationParameterValues(MethodInfo method, string[] args)
        {

            List<object> parameterValues = new List<object>();
            var parameterRequirements = getOperationParameterRequirements(method);
            foreach (var para in parameterRequirements)
            {
                if (para is SimpleParameter si)
                {
                    CommandLineParameterInfo pi = getParameterValue(si, args);
                    object typedValue = getTypedParameterValue(si.ParameterInfo, pi);
                    if (typedValue == null)
                        typedValue = si.ParameterInfo.Default;
                    parameterValues.Add(typedValue);
                }
                else if (para is ModelParameter mp)
                {
                    parameterValues.Add(buildModel(mp, args));
                    //parameterValues.Add(buildModel(mp.ModelType, args));
                }
            }

            return parameterValues;

        }

        private object getTypedParameterValue(ParameterAttribute para, CommandLineParameterInfo pi)
        {
            if (para.Type == typeof(bool))
                return pi.Exists;

            bool isList = typeof(IEnumerable).IsAssignableFrom(para.Type) && para.Type != typeof(string);

            if (pi.HasValue)
            {
                var value = pi.Value;

                if (para.Type.IsEnum)
                {
                    if (Enum.TryParse(para.Type, value, ignoreCase: true, out object res))
                        return res;
                    else
                        throw new InvalidParameterException(para);
                }

                if (TypeResolver.IsResolvableType(para.Type))
                {
                    return TypeResolver.Resolve(value, para.Type);
                }

                if (isList)
                {

                    var separator = " ";
                    if (!string.IsNullOrWhiteSpace(para.ListSeparator))
                        separator = para.ListSeparator;

                    var splitValue = value.Split(new string[] { separator }, StringSplitOptions.RemoveEmptyEntries);

                    var underlyingType = para.Type.GetGenericArguments().FirstOrDefault();
                    var constructedListType = typeof(List<>).MakeGenericType(underlyingType);
                    var constructedList = (IList)Activator.CreateInstance(constructedListType);

                    try
                    {
                        if (Parameter.IsSimpleType(underlyingType))
                        {
                            splitValue.ToList().ForEach(x => constructedList.Add(Convert.ChangeType(x, underlyingType)));
                        }
                        else if (TypeResolver.IsResolvableType(underlyingType))
                        {
                            splitValue.ToList().ForEach(x => constructedList.Add(TypeResolver.Resolve(x, underlyingType)));
                        }
                    }
                    catch (FormatException)
                    {
                        throw new InvalidParameterException(para);
                    }

                    return constructedList;

                }
                else
                {
                    return Convert.ChangeType(value, para.Type);
                }

            }
            else
            {
                if (para.IsRequired)
                {
                    throw new ParameterMissingException(para);
                }
                else
                {
                    return null;
                }
            }

        }

        private List<ParameterAttribute> getMethodParameterInfo(MethodInfo method, bool enumerateModelProperties = false)
        {

            var paraConfigList = method.GetCustomAttributes<ParameterAttribute>();

            List<ParameterAttribute> pi = new List<ParameterAttribute>();
            foreach (var para in method.GetParameters())
            {
                if (Parameter.IsSimpleType(para.ParameterType) || TypeResolver.IsResolvableType(para.ParameterType))
                {
                    var paraConfig = paraConfigList.SingleOrDefault(x => x.ShortName.ToString().Equals(para.Name, StringComparison.CurrentCultureIgnoreCase) || string.Equals(x.LongName, para.Name, StringComparison.CurrentCultureIgnoreCase));
                    if (paraConfig == null)
                        if (para.Name.Length == 1)
                            paraConfig = new ParameterAttribute { ShortName = (char)para.Name.First() };
                        else
                            paraConfig = new ParameterAttribute { LongName = para.Name };

                    if (para.HasDefaultValue)
                    {
                        paraConfig.IsRequired = false;
                        paraConfig.Default = para.DefaultValue;
                    }

                    paraConfig.isSimpleType = Parameter.IsSimpleType(para.ParameterType);
                    paraConfig.Type = para.ParameterType;

                    pi.Add(paraConfig);
                }
                else
                {
                    if (enumerateModelProperties)
                    {
                        var model = Activator.CreateInstance(para.ParameterType);
                        foreach (var prop in para.ParameterType.GetProperties().Where(x => x.GetSetMethod() != null))
                        {
                            bool isRequired = false;
                            var currentValue = prop.GetValue(model);

                            var paraConfig = prop.GetCustomAttribute<ParameterAttribute>();

                            if (prop.PropertyType.IsValueType)
                                isRequired = currentValue.Equals(Activator.CreateInstance(prop.PropertyType));
                            else
                                isRequired = currentValue == null;

                            if (paraConfig == null)
                            {
                                if (prop.Name.Length == 1)
                                    paraConfig = new ParameterAttribute { ShortName = (char)prop.Name.First() };
                                else
                                    paraConfig = new ParameterAttribute { LongName = prop.Name };
                            }

                            paraConfig.isSimpleType = false;
                            paraConfig.Type = para.ParameterType;

                            paraConfig.IsRequired = isRequired;
                            if (!paraConfig.IsRequired)
                                paraConfig.Default = currentValue;

                            pi.Add(paraConfig);
                        }
                    }
                    else
                    {
                        var paraConfig = new ParameterAttribute { isSimpleType = false, Type = para.ParameterType };
                        pi.Add(paraConfig);
                    }
                }
            }
            return pi;

        }

        internal CommandLineParameterInfo getParameterValue(SimpleParameter si, string[] args)
        {

            var argList = args.ToList();

            // Check for positional parameters
            if (si.ParameterInfo.Position > -1)
            {
                if (argList.Count > si.ParameterInfo.Position)
                {
                    return new CommandLineParameterInfo
                    {
                        Exists = true,
                        HasValue = true,
                        Position = si.ParameterInfo.Position,
                        Value = argList[si.ParameterInfo.Position]
                    };
                }
                else
                {
                    if (si.ParameterInfo.IsRequired)
                    {
                        throw new ParameterMissingException(si.ParameterInfo);
                    }
                    else
                    {
                        return new CommandLineParameterInfo
                        {
                            Exists = false,
                            HasValue = false
                        };
                    }
                }
            }

            // Normal parameters
            var paraMarker = -1;
            if (si.ParameterInfo.ShortName != char.MinValue)
                if (args.Contains($"-{si.ParameterInfo.ShortName}"))
                    paraMarker = argList.IndexOf($"-{si.ParameterInfo.ShortName}");

            if (!string.IsNullOrWhiteSpace(si.ParameterInfo.LongName) && paraMarker == -1)
                if (args.Contains($"--{si.ParameterInfo.LongName}", StringComparer.CurrentCultureIgnoreCase))
                    paraMarker = argList.FindIndex(x => x.Equals($"--{si.ParameterInfo.LongName}", StringComparison.CurrentCultureIgnoreCase));

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

        private object buildModel(ModelParameter mp, string[] args)
        {

            var modelType = mp.ParameterType;
            var model = Activator.CreateInstance(modelType);

            foreach (var prop in modelType.GetProperties().Where(x => x.GetSetMethod() != null))
            {

                var para = prop.GetCustomAttribute<ParameterAttribute>();
                if (para == null)
                {
                    para = mp.Parameters.SingleOrDefault(x => (!string.IsNullOrWhiteSpace(x.LongName) && x.LongName.Equals(prop.Name, StringComparison.CurrentCultureIgnoreCase)) || (x.ShortName > 0 && x.ShortName.ToString().Equals(prop.Name, StringComparison.CurrentCultureIgnoreCase)));
                    if (para == null)
                        throw new Exception("Internal error.");
                }
                else
                {
                    para.Type = prop.PropertyType;
                }

                object propValue;
                var value = getParameterInfo(para, args);
                if (value.Exists && value.HasValue)
                {
                    propValue = getTypedParameterValue(para, value);
                }
                else
                {
                    var currentValue = prop.GetValue(model);
                    if (para.IsRequired && object.Equals(para.Default, currentValue))
                    {
                        throw new ParameterMissingException(para);
                    }
                    propValue = para.Default;
                }

                prop.SetValue(model, propValue);

            }

            return model;

        }

        private string getCommandTemplate(string verb = null, string action = null)
        {
            var processName = System.Diagnostics.Process.GetCurrentProcess().ProcessName.ToLower();

            StringBuilder sb = new StringBuilder();
            var hasDefaultVerb = verbDefinitions.Any(x => x.Verb.IsDefault);
            var defaultVerb = verbDefinitions.SingleOrDefault(x => x.Verb.IsDefault);
            sb.Append(processName);
            sb.Append(" ");
            if (hasDefaultVerb)
                sb.Append(verb == null ? "[verb]" : verb);
            else
                sb.Append("verb");

            sb.Append(" ");
            if (hasDefaultVerb)
            {
                var defaultVerbOperations = defaultVerb.Type.GetMethods().Where(x => x.GetCustomAttribute<OperationAttribute>() != null).Select(x => x.GetCustomAttribute<OperationAttribute>());
                var defaultVerbHasDefaultOperation = defaultVerbOperations.Any(x => x.IsDefault);

                if (defaultVerbHasDefaultOperation)
                    sb.Append(action == null ? "[operation]" : action);
                else
                    sb.Append("operation");
            }
            else
            {
                sb.Append("[operation]");
            }

            sb.Append(" [parameters]");

            return sb.ToString();

        }

        private void Help(string verb = null, string action = null)
        {

            var processName = System.Diagnostics.Process.GetCurrentProcess().ProcessName.ToLower();

            if (!verbDefinitions.Any())
            {
                Error("This command contains no verbs.");
                return;
            }

            Console.WriteLine();
            Console.WriteLine("Help");
            Console.WriteLine();
            Console.WriteLine("Usage: ");
            Console.WriteLine($"  {getCommandTemplate(verb, action)}");
            Console.WriteLine();

            if (verb == null) // show all verbs unless one is specified
            {
                Console.WriteLine("Possible verbs:");
                Console.WriteLine();
                var maxVerbWidth = verbDefinitions.Max(y => y.Verb.Name.Length) + 2;
                var helpWidth = 80 - maxVerbWidth - 2;

                foreach (var v in verbDefinitions.OrderBy(x => x.Verb.Name))
                {
                    Console.ForegroundColor = ConsoleColor.White;
                    Console.Write(v.Verb.Name.ToLower().PadLeft(maxVerbWidth) + "  ");
                    Console.ResetColor();

                    renderHelpText(v.Verb.HelpText, helpWidth, maxVerbWidth);

                    Console.WriteLine();
                }
                Console.WriteLine();
                Console.WriteLine($"For more specific help, try: {processName} --help verb");
            }
            else if (verb != null && action == null)
            {

                var selectedVerb = verbDefinitions.SingleOrDefault(x => x.Verb.Name.Equals(verb, StringComparison.CurrentCultureIgnoreCase));
                if (selectedVerb == null)
                {
                    Error($"No such verb: {verb}.");
                    return;
                }

                var vTitle = selectedVerb.Verb.Name ?? verb;
                Console.Write($"{vTitle}");
                if (!string.IsNullOrWhiteSpace(selectedVerb.Verb.HelpText)) Console.WriteLine($": {selectedVerb.Verb.HelpText}");
                Console.WriteLine();
                Console.WriteLine($"{vTitle} Operations:");
                Console.WriteLine();

                var operations = selectedVerb.Type.GetMethods().Where(x => x.GetCustomAttribute<OperationAttribute>() != null).Select(x => new { Method = x, Info = x.GetCustomAttribute<OperationAttribute>() });
                var maxOpWidth = operations.Max(x => x.Info.Name?.Length ?? x.Method.Name.Length) + 2;
                var maxWidth = 80 - maxOpWidth - 2;

                foreach (var operation in operations)
                {

                    var name = operation.Info.Name ?? operation.Method.Name;

                    Console.ForegroundColor = ConsoleColor.White;
                    Console.Write(name.ToLower().PadLeft(maxOpWidth) + "  ");
                    Console.ResetColor();

                    renderHelpText(operation.Info.HelpText, maxWidth, maxOpWidth);

                    Console.WriteLine();

                }

                Console.WriteLine();
                Console.WriteLine($"For more specific help, try: {processName} --help " + verb + " operation");

            }
            else
            {

                var selectedVerb = verbDefinitions.SingleOrDefault(x => x.Verb.Name.Equals(verb, StringComparison.CurrentCultureIgnoreCase));
                if (selectedVerb == null)
                {
                    Error($"No such verb: {verb}.");
                    return;
                }

                var operations = selectedVerb.Type.GetMethods().Where(x => x.GetCustomAttribute<OperationAttribute>() != null).Select(x => new { Method = x, Info = x.GetCustomAttribute<OperationAttribute>() });
                var operation = operations.SingleOrDefault(x => (x.Info.Name != null && x.Info.Name.Equals(action, StringComparison.CurrentCultureIgnoreCase)) || x.Method.Name.Equals(action, StringComparison.CurrentCultureIgnoreCase));
                if (operation == null)
                {
                    Error($"No such operation: {action}.");
                    return;
                }

                Console.WriteLine(operation.Info.Name ?? operation.Method.Name);

                if (!string.IsNullOrWhiteSpace(operation.Info.HelpText))
                    Console.WriteLine(operation.Info.HelpText);

                if (operation.Method.GetParameters().Any())
                {

                    Console.WriteLine("Parameters:");
                    Console.WriteLine();

                    var parameters = getMethodParameterInfo(operation.Method);
                    var maxParaWidth = parameters.Max(x => string.IsNullOrWhiteSpace(x.LongName) ? 1 : x.LongName.Length) + 10;
                    var maxWidth = 80 - maxParaWidth - 2;

                    foreach (var para in parameters)
                    {
                        Console.ForegroundColor = ConsoleColor.White;
                        var p = "  ";

                        if (para.ShortName > 0)
                            p += ($"-{para.ShortName} ");

                        if (!string.IsNullOrWhiteSpace(para.LongName))
                            p += ($"--{para.LongName} ");

                        p += ("  ");
                        Console.Write(p.PadLeft(maxParaWidth));
                        Console.ResetColor();

                        if (string.IsNullOrWhiteSpace(para.HelpText))
                            para.HelpText = "";

                        List<string> bits = new List<string>();
                        bits.Add(para.IsRequired ? "Required" : "Optional");
                        if (para.Position > -1) bits.Add("Position: " + para.Position);
                        if (!string.IsNullOrWhiteSpace(para.ListSeparator)) bits.Add("Separator: " + (para.ListSeparator == " " ? "(space)" : para.ListSeparator));
                        if (para.Default != null) bits.Add("Default Value: " + para.Default);

                        if (!string.IsNullOrWhiteSpace(para.HelpText))
                            bits.Add(para.HelpText);

                        renderHelpText(string.Join("; ", bits), maxWidth, maxParaWidth);

                        Console.WriteLine();

                    }


                }
                else
                {

                    Console.WriteLine("This operation has no parameters.");

                }

            }

            Console.WriteLine();

        }

        private void renderHelpText(string helpText, int helpWidth, int indentWidth)
        {
            if (!string.IsNullOrWhiteSpace(helpText))
            {
                var helpWords = helpText.Split();
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
                    if (l != lines.Last())
                        Console.Write(new string(Enumerable.Range(0, indentWidth + 2).Select(x => ' ').ToArray()));
                }

            }
        }

        private void Error(string message)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine(message);
            Console.ResetColor();
        }

    }
}
