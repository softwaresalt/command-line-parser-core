﻿
#region Using Directives

using Antlr4.Runtime;
using Antlr4.Runtime.Tree;
using System.Collections;
using System.Collections.Generic;
using System.CommandLine.Parser.Antlr;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

#endregion

namespace System.CommandLine.Parser
{
    /// <summary>
    /// Represents a parser, which is able to parse command line parameters and convert them to strongly-typed .NET data types.
    /// </summary>
    public static class Parser
    {
        #region Private Static Methods

        /// <summary>
        /// Gets the value of the specified parameter.
        /// </summary>
        /// <param name="parameter">The parameter whose value is to be retrieved.</param>
        /// <returns>Returns the value of the parameter. If the type of the parameter could not be determined then <c>null</c> is returned.</returns>
        private static object GetParameterValue(Parameter parameter)
        {
            // Determines the type of the parameter and returns its value accordingly
            BooleanParameter booleanParameter = parameter as BooleanParameter;
            if (booleanParameter != null)
                return booleanParameter.Value;
            NumberParameter numberParameter = parameter as NumberParameter;
            if (numberParameter != null)
                return numberParameter.Value;
            StringParameter stringParameter = parameter as StringParameter;
            if (stringParameter != null)
                return stringParameter.Value;
            DefaultParameter defaultParameter = parameter as DefaultParameter;
            if (defaultParameter != null)
                return defaultParameter.Value;
            ArrayParameter arrayParameter = parameter as ArrayParameter;
            if (arrayParameter != null)
                return arrayParameter.Value;

            // Since the parameter is of an unknown type, null is returned by default
            return null;
        }

        /// <summary>
        /// Gets the value of the specified parameter converted to the specified type.
        /// </summary>
        /// <param name="type">The type into which the value of the parameter should be casted.</param>
        /// <param name="parameter">The parameter whose value is to be retrieved.</param>
        /// <returns>Returns the value of the specified parameter casted to the specified type.</returns>
        private static object GetParameterValue(Type type, Parameter parameter)
        {
            // Gets the type of the command line parameter
            Type parameterType = Parser.GetParameterType(parameter);

            // Checks if the parameter can be assigned to the specified type
            if (type.IsAssignableFrom(parameterType))
                return Parser.GetParameterValue(parameter);
            
            // Checks if the type of the parameter is numeric and whether it can be converted to the specified type
            if (parameterType == typeof(double))
            {
                if (type == typeof(decimal))
                    return Convert.ToDecimal(Parser.GetParameterValue(parameter));
                if (type == typeof(double))
                    return Convert.ToDouble(Parser.GetParameterValue(parameter));
                if (type == typeof(float))
                    return Convert.ToSingle(Parser.GetParameterValue(parameter));
                if (type == typeof(long))
                    return Convert.ToInt64(Parser.GetParameterValue(parameter));
                if (type == typeof(int))
                    return Convert.ToInt32(Parser.GetParameterValue(parameter));
                if (type == typeof(short))
                    return Convert.ToInt16(Parser.GetParameterValue(parameter));
                if (type == typeof(byte))
                    return Convert.ToByte(Parser.GetParameterValue(parameter));
            }

            // Checks if the type of the parameter is an empty array, if so then it is tried to create the destination type if it is a collection type
            if (parameterType == typeof(IEnumerable<>))
            {
                // Determines whether the destination type is a collection type, if so then an empty instance of it is generated by using the default constructor
                Type baseType = type;
                while (baseType != null)
                {
                    if (baseType.GetInterfaces().Any(i => i == typeof(IEnumerable) || i == typeof(ICollection)))
                    {
                        ConstructorInfo defaultConstructorInfo = type.GetConstructors(BindingFlags.Public | BindingFlags.Instance).FirstOrDefault(constructorInfo => !constructorInfo.GetParameters().Any());
                        if (defaultConstructorInfo != null)
                            return defaultConstructorInfo.Invoke(new object[0]);
                    }
                }
            }

            // Checks if the parameter is array, if so then it is tried to create the destination type if it is a list type
            if (parameterType.Name == typeof(IEnumerable<>).Name)
            {
                // Determines whether the destination type is a collection type, if so then an instance of it is generated by using the default constructor and
                // the contents of the array parameter are added to it
                Type baseType = type;
                while (baseType != null)
                {
                    // Checks if the destination type implements IList, if so then it is constructed and the contents of the array parameter are added to it
                    if (baseType.GetInterfaces().Any(i => i == typeof(IList)))
                    {
                        ConstructorInfo defaultConstructorInfo = type.GetConstructors(BindingFlags.Public | BindingFlags.Instance).FirstOrDefault(constructorInfo => !constructorInfo.GetParameters().Any());
                        if (defaultConstructorInfo != null)
                        {
                            IList list = defaultConstructorInfo.Invoke(new object[0]) as IList;
                            foreach (Parameter arrayContentParameter in (parameter as ArrayParameter).Value)
                                list.Add(Parser.GetParameterValue(typeof(object), arrayContentParameter));
                        }
                    }

                    // Checks if the destination type implements ICollection<>, if so then it is constructed and then the contents of the array parameter are added to it
                    if (baseType.GetInterfaces().Any(i => i.Name == typeof(ICollection<>).Name))
                    {
                        ConstructorInfo defaultConstructorInfo = type.GetConstructors(BindingFlags.Public | BindingFlags.Instance).FirstOrDefault(constructorInfo => !constructorInfo.GetParameters().Any());
                        if (defaultConstructorInfo != null)
                        {
                            Type collectionType = baseType.GetInterfaces().FirstOrDefault(i => i.Name == typeof(ICollection<>).Name);
                            Type genericCollectionParameterType = collectionType.GetGenericArguments().First();
                            MethodInfo addMethod = collectionType.GetMethod("Add");
                            object list = defaultConstructorInfo.Invoke(new object[0]);
                            foreach (Parameter arrayContentParameter in (parameter as ArrayParameter).Value)
                                addMethod.Invoke(list, new object[] { Parser.GetParameterValue(genericCollectionParameterType, arrayContentParameter) });
                        }
                    }
                }
            }

            // Since the parameter could not be converted into the specified type, an exception is thrown
            throw new InvalidOperationException();
        }

        /// <summary>
        /// Gets the type of the parameter.
        /// </summary>
        /// <param name="parameter">The parameter whose type is to be retrieved.</param>
        /// <returns>Returns the type of the parameter. If the type of the parameter can not be determined then <c>null</c> is returned.</returns>
        private static Type GetParameterType(Parameter parameter)
        {
            // Checks if the parameter is a simple data type, if so then its simple type is returned
            BooleanParameter booleanParameter = parameter as BooleanParameter;
            if (booleanParameter != null)
                return typeof(bool);
            NumberParameter numberParameter = parameter as NumberParameter;
            if (numberParameter != null)
                return typeof(double);
            StringParameter stringParameter = parameter as StringParameter;
            if (stringParameter != null)
                return typeof(string);
            DefaultParameter defaultParameter = parameter as DefaultParameter;
            if (defaultParameter != null)
                return typeof(string);

            // Checks if the parameter is of type array, if so then its type is determined recursively
            ArrayParameter arrayParameter = parameter as ArrayParameter;
            if (arrayParameter != null)
            {
                // Checks if the array parameter is empty, in that case the type of the array is IEnumerable<>
                if (!arrayParameter.Value.Any())
                    return typeof(IEnumerable<>);

                // Gets the types of the items of the array
                IEnumerable<Type> arrayContentTypes = arrayParameter.Value.Select(value => Parser.GetParameterType(value));

                // Checks if all types of the contents are the same, in that case the type is returned
                if (arrayContentTypes.GroupBy(type => type.FullName).Count() == 1)
                {
                    Type genericArrayType = typeof(IEnumerable<>);
                    return genericArrayType.MakeGenericType(new Type[] { arrayContentTypes.First() });
                }
                
                // Since not all elements of the array parameter have the same type it is determined whether the elements are value types or reference types
                Type valueTypeInfo = typeof(ValueType);
                if (arrayContentTypes.All(type => valueTypeInfo.IsAssignableFrom(type)))
                    return typeof(IEnumerable<ValueType>);
                return typeof(IEnumerable<object>);
            }

            // Since it is an unsupported type, null is returned
            return null;
        }

        /// <summary>
        /// Checks if the value of the parameter is assignable to the specified type.
        /// </summary>
        /// <param name="parameterType">The type for which is to be checked whether the value of the parameter can be assigned.</param>
        /// <param name="parameter">The parameter for which is to be checked whether it is assignable to the specified type.</param>
        /// <returns>Returns <c>true</c> if the value of the parameter can be assigned to the specified type and <c>false</c> otherwise.</returns>
        private static bool IsParameterAssignable(Type parameterType, Parameter parameter)
        {
            // Gets the type of the specified parameter and checks if the type could be determined, if not then an excpetion is thrown
            Type actualParameterType = Parser.GetParameterType(parameter);
            if (actualParameterType == null)
                throw new InvalidOperationException();

            // Checks if the actual parameter type is a numeric type, in that case the parameter type must be some type of numeric type as well
            if (actualParameterType == typeof(double))
            {
                return parameterType == typeof(decimal) ||
                    parameterType == typeof(double) ||
                    parameterType == typeof(float) ||
                    parameterType == typeof(long) ||
                    parameterType == typeof(int) ||
                    parameterType == typeof(short) ||
                    parameterType == typeof(byte);
            }

            // Checks if the actual parameter type is an empty array, then the parameter type must have IEnumerable<> as its base-type
            if (actualParameterType == typeof(IEnumerable<>))
            {
                Type parameterBaseType = parameterType;
                while (parameterBaseType != null)
                {
                    if (parameterBaseType.GetInterfaces().Any(i => i.Name == typeof(IEnumerable<>).Name))
                        return true;
                    parameterBaseType = parameterBaseType.BaseType;
                }
            }

            // Checks if the actual parameter type is some other type of array, then the parameter must have IEnumerable<> as its base-type, which is assignable
            // from the actual parameter type
            if (actualParameterType.Name == typeof(IEnumerable<>).Name && actualParameterType.IsConstructedGenericType)
            {
                Type parameterBaseType = parameterType;
                while (parameterBaseType != null)
                {
                    if (parameterBaseType.IsAssignableFrom(actualParameterType))
                        return true;
                    parameterBaseType = parameterBaseType.BaseType;
                }
            }

            // Finally, if all of the above, could not determine the assignability, the algorithm defaults to the IsAssignableFrom method of the type
            return parameterType.IsAssignableFrom(actualParameterType);
        }

        #endregion

        #region Public Static Methods

        /// <summary>
        /// Parses the specified command line parameters.
        /// </summary>
        /// <param name="commandLineParameters">The command line parameters that are to be parsed.</param>
        /// <returns>Returns the parsed parameters.</returns>
        public static ParameterBag Parse(string commandLineParameters)
        {
            // Parses the command line parameters using the ANTRL4 generated parsers
            CommandLineLexer lexer = new CommandLineLexer(new AntlrInputStream(new StringReader(commandLineParameters)));
            CommandLineParser parser = new CommandLineParser(new CommonTokenStream(lexer)) { BuildParseTree = true };
            IParseTree parseTree = parser.commandLine();
            CommandLineVisitor commandLineVisitor = new CommandLineVisitor();
            commandLineVisitor.Visit(parseTree);

            // Returns the parsed parameters wrapped in a parameter bag
            return new ParameterBag
            {
                CommandLineParameters = commandLineParameters,
                Parameters = commandLineVisitor.Parameters,
                DefaultParameters = commandLineVisitor.DefaultParameters
            };
        }

        /// <summary>
        /// Parses the command line parameters that have been passed to the program.
        /// </summary>
        /// <returns>Returns the parsed parameters.</returns>
        public static ParameterBag Parse() => Parser.Parse(Environment.CommandLine);

        /// <summary>
        /// Parses the specified command line parameters asynchronously.
        /// </summary>
        /// <param name="commandLineParameters">The command line parameters that are to be parsed.</param>
        /// <returns>Returns the parsed parameters.</returns>
        public static Task<ParameterBag> ParseAsync(string commandLineParameters) => Task.Run(() => Parser.Parse(commandLineParameters));

        /// <summary>
        /// Parses the command line parameters that have been passed to the program asynchronously.
        /// </summary>
        /// <returns>Returns the parsed parameters.</returns>
        public static Task<ParameterBag> ParseAsync() => Task.Run(() => Parser.Parse());

        /// <summary>
        /// Parses the specified command line parameters and converts them into the specified type.
        /// </summary>
        /// <param name="commandLineParameters">The command line parameters that are to be parsed.</param>
        /// <typeparam name="T">The type that is to be instantiated and injected with the parameters from the command line.</typeparam>
        /// <returns>Returns an instance of the specified type injected with the parameters from the command line.</returns>
        public static T Parse<T>(string commandLineParameters) where T : class
        {
            // Parses the command line parameters
            ParameterBag parameterBag = Parser.Parse(commandLineParameters);

            // Gets the type information about the type that is to be instantiated
            Type returnType = typeof(T);

            // Determines the constructor, which is to be used for instantiating the specified type, the algorithm is greedy and uses the constructor with the
            // most constructor arguments that can be matched from the parsed command line arguments
            ConstructorInfo chosenConstructorInfo = null;
            Dictionary<string, Type> chosenConstructorParameterInfos = null;
            foreach (ConstructorInfo constructorInfo in returnType.GetConstructors()
                .Where(constructor => constructor.IsPublic && !constructor.IsStatic)
                .OrderByDescending(constructor => constructor.GetParameters().Count()))
            {
                // Creates a dictionary which can hold information about the parameters of the constructor and their types
                chosenConstructorParameterInfos = new Dictionary<string, Type>();

                // Cycles over all the parameters of the constructor and gather information about them
                foreach (ParameterInfo constructorParameterInfo in constructorInfo.GetParameters())
                {
                    // Gets the name of the command line parameter with which the constructor parameter is to be matched (which is either retrieved from the
                    // parameter name attribute or the name of the constructor parameter
                    string parameterName;
                    ParameterNameAttribute parameterNameAttribute = constructorParameterInfo.GetCustomAttribute<ParameterNameAttribute>();
                    if (parameterNameAttribute != null)
                        parameterName = parameterNameAttribute.ParameterName;
                    else
                        parameterName = constructorParameterInfo.Name;

                    // Adds the constructor parameter information to the parameter infos
                    chosenConstructorParameterInfos.Add(parameterName, constructorParameterInfo.ParameterType);
                }

                // Checks if the constructor can be used, which is when all constructor parameters can be matched with command line parameters
                bool canConstructorBeUsed = true;
                foreach (string parameterName in chosenConstructorParameterInfos.Keys)
                {
                    // Gets the type of the constructor parameter, which is to be used to match it to command line parameters
                    Type parameterType = chosenConstructorParameterInfos[parameterName];

                    // Gets the command line parameter by the name, if no parameter could be found, then the constructor can not be used
                    Parameter parameter = parameterBag.Parameters[parameterName];
                    if (parameter == null)
                    {
                        canConstructorBeUsed = false;
                        break;
                    }

                    // Checks if the type of the commmand line parameter assignable to the constructor argument
                    if (!Parser.IsParameterAssignable(parameterType, parameter))
                    {
                        canConstructorBeUsed = false;
                        break;
                    }
                }

                // Checks if the constructor can be used, if so then the other constructors do not need to be checked
                if (canConstructorBeUsed)
                {
                    chosenConstructorInfo = constructorInfo;
                    break;
                }
            }

            // Checks if a constructor could be found, if not then an exception is thrown
            if (chosenConstructorInfo == null)
                throw new InvalidOperationException();

            // Prepares the constructor arguments
            object[] constructorParameters = new object[chosenConstructorParameterInfos.Count];
            for (int i = 0; i < chosenConstructorParameterInfos.Count; i++)
            {
                // Gets the matched command line parameter
                KeyValuePair<string, Type> keyValuePair = chosenConstructorParameterInfos.ElementAt(i);
                string parameterName = keyValuePair.Key;
                Type parameterType = keyValuePair.Value;
                Parameter parameter = parameterBag.Parameters[parameterName];

                // Sets the constructor parameter
                constructorParameters[i] = Parser.GetParameterValue(parameterType, parameter);
            }

            // Craetes a new instance of the specified type and validates whether it could be instantiated, if not then an exception is thrown
            T instance = chosenConstructorInfo.Invoke(constructorParameters) as T;
            if (instance == null)
                throw new InvalidOperationException();

            // Matches the public properties of the instance and injects all possible command line parameters into it
            foreach (PropertyInfo propertyInfo in returnType.GetProperties(BindingFlags.Public | BindingFlags.Instance).Where(property => property.CanWrite))
            {
                // Gets the name of the command line parameter with which the property is to be matched (which is either retrieved from the parameter name
                // attribute or the name of the constructor parameter
                string propertyName;
                ParameterNameAttribute parameterNameAttribute = propertyInfo.GetCustomAttribute<ParameterNameAttribute>();
                if (parameterNameAttribute != null)
                    propertyName = parameterNameAttribute.ParameterName;
                else
                    propertyName = propertyInfo.Name;

                // Gets the command line parameter by the name, if no parameter could be found then the property can not be assigned
                if (!parameterBag.Parameters.ContainsKey(propertyName))
                    continue;
                Parameter parameter = parameterBag.Parameters[propertyName];
                if (parameter == null)
                    continue;

                // Checks if the type of the commmand line parameter assignable to the property
                if (!Parser.IsParameterAssignable(propertyInfo.PropertyType, parameter))
                    continue;

                // Assigns the command line parameter value to the property
                try
                {
                    propertyInfo.SetMethod.Invoke(instance, new object[] { Parser.GetParameterValue(propertyInfo.PropertyType, parameter) });
                }
                catch (InvalidOperationException) { }
            }

            // Returns the created instance of the specified type
            return instance;
        }

        /// <summary>
        /// Parses the command line parameters that have been passed to the program and converts them into the specified type.
        /// </summary>
        /// <typeparam name="T">The type that is to be instantiated and injected with the parameters from the command line.</typeparam>
        /// <returns>Returns an instance of the specified type injected with the parameters from the command line.</returns>
        public static T Parse<T>() where T : class => Parser.Parse<T>(Environment.CommandLine);

        /// <summary>
        /// Parses the specified command line parameters and converts them into the specified type asynchronously.
        /// </summary>
        /// <param name="commandLineParameters">The command line parameters that are to be parsed.</param>
        /// <typeparam name="T">The type that is to be instantiated and injected with the parameters from the command line.</typeparam>
        /// <returns>Returns an instance of the specified type injected with the parameters from the command line.</returns>
        public static Task<T> ParseAsync<T>(string commandLineParameters) where T : class => Task.Run(() => Parser.Parse<T>(commandLineParameters));

        /// <summary>
        /// Parses the command line parameters that have been passed to the program and converts them into the specified type asynchronously.
        /// </summary>
        /// <typeparam name="T">The type that is to be instantiated and injected with the parameters from the command line.</typeparam>
        /// <returns>Returns an instance of the specified type injected with the parameters from the command line.</returns>
        public static Task<T> ParseAsync<T>() where T : class => Task.Run(() => Parser.Parse<T>());

        #endregion
    }
}