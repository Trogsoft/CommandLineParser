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

        public int Run(string[] args)
        {

            // any args?
            if (!args.Any())
            {
                Error("No arguments passed.");
                return ERR_NO_ARGUMENTS;
            }

            var verbName = args.First();
            var operationName = args.Length > 1 ? args[1] : null;

            // find the right type
            var verbs = AppDomain.CurrentDomain.GetAssemblies().SelectMany(x => x.GetTypes().Where(y => typeof(Verb).IsAssignableFrom(y) && y.GetCustomAttribute<VerbAttribute>() != null && y.IsPublic && !y.IsInterface && !y.IsAbstract));
            var verb = verbs.SingleOrDefault(x => x.GetCustomAttribute<VerbAttribute>().Name.Equals(verbName, StringComparison.CurrentCultureIgnoreCase));

            if (verb == null)
            {
                Error($"{args.First()} is not a recognised operation.");
                return ERR_UNRECOGNISED_OPERATION;
            }

            var methods = verb.GetMethods().Where(x => x.GetCustomAttribute<OperationAttribute>() != null).Select(x => (method: x, op: x.GetCustomAttribute<OperationAttribute>())).ToList();

            // find the method
            var method =
                operationName == null
                    ? (methods.Any(x => x.op.IsDefault) ? methods.SingleOrDefault(x => x.op.IsDefault).method : null)
                    : methods.SingleOrDefault(x => x.method.Name.Equals(operationName, StringComparison.CurrentCultureIgnoreCase)).method;

            if (method == null)
                method = methods.SingleOrDefault(x => x.op.IsDefault).method;

            if (method == null)
            {
                Error("Method not found.");
                return ERR_METHOD_NOT_FOUND;
            }

            var vi = Activator.CreateInstance(verb);
            var para = getMethodParameters(method, args);
            var result = method.Invoke(vi, para);

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
                        paraConfig = new ParameterAttribute { ShortName = (char)para.Name.First() };
                    else
                        paraConfig = new ParameterAttribute { LongName = para.Name };

                var type = para.ParameterType;
                var value = getParameterValue(type, paraConfig, args);
                paras.Add(value);
            }

            if (paras.Any())
                return paras.ToArray();

            return null;
        }

        private object getParameterValue(Type type, ParameterAttribute paraConfig, string[] args)
        {

            var argList = args.ToList();

            var paraMarker = -1;
            if (paraConfig.ShortName != char.MinValue)
                if (args.Contains($"-{paraConfig.ShortName}"))
                    paraMarker = argList.IndexOf($"-{paraConfig.ShortName}");

            if (!string.IsNullOrWhiteSpace(paraConfig.LongName) && paraMarker == -1)
                if (args.Contains($"--{paraConfig.LongName}"))
                    paraMarker = argList.IndexOf($"--{paraConfig.LongName}");

            if (type == typeof(bool))
            {
                return paraMarker > -1;
            }

            bool isList = typeof(IEnumerable).IsAssignableFrom(type);

            if (args.Count() > paraMarker)
            {
                var value = args[paraMarker + 1];
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
                    Error("Missing parameter: " + paraConfig.LongName);
                    throw new Exception();
                }
                else
                {
                    return null;
                }
            }

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
