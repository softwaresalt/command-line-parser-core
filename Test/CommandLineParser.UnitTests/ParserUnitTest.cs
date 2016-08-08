﻿
#region Using Directives

using System.Collections.Generic;
using System.CommandLine.Parser.Parameters;
using System.Linq;
using Xunit;

#endregion

namespace System.CommandLine.Parser.UnitTests
{
    /// <summary>
    /// Represents a unit testing class, which tests the actual command line parser, which turns the command line parameters into typed .NET objects.
    /// </summary>
    public class ParserUnitTest
    {
        #region Private Methods

        /// <summary>
        /// Validates that the parse output against the specified expected output.
        /// </summary>
        /// <param name="parameters">The parameters, which are the result of the parsing process.</param>
        /// <param name="expectedParameters">The parameters, which are the expected output of the parsing process.</param>
        private void ValidateParseOutput(IDictionary<string, Parameter> parameters, IDictionary<string, Parameter> expectedParameters)
        {
            // Validates that the amount of parameters in the parameter bag and the expected parameter bag are the same
            Assert.Equal(expectedParameters.Count(), parameters.Count());
            if (parameters.Count() != expectedParameters.Count())
                return;

            // Validates that both parameter sets have the same parameter names
            Assert.True(parameters.Keys.All(key => expectedParameters.ContainsKey(key)));
            Assert.True(expectedParameters.Keys.All(key => parameters.ContainsKey(key)));

            // Cycles over all parameters in the parameter bag and validates them against the expected parameters
            foreach (string parameterName in parameters.Keys)
            {
                // Gets the two parameter at the current position
                Parameter parameter = parameters[parameterName];
                Parameter expectedParameter = expectedParameters[parameterName];

                // Validates that the parameter has the same type as the expected parameter
                Assert.IsType(expectedParameter.GetType(), parameter);
                if (parameter.GetType() != expectedParameter.GetType())
                    continue;
                
                // Checks if the parameter is a simple data type, if so its value is validated againts the expected parameter
                BooleanParameter booleanParameter = parameter as BooleanParameter;
                if (booleanParameter != null)
                    Assert.Equal((expectedParameter as BooleanParameter).Value, booleanParameter.Value);
                NumberParameter numberParameter = parameter as NumberParameter;
                if (numberParameter != null)
                    Assert.Equal((expectedParameter as NumberParameter).Value, numberParameter.Value);
                StringParameter stringParameter = parameter as StringParameter;
                if (stringParameter != null)
                    Assert.Equal((expectedParameter as StringParameter).Value, stringParameter.Value);

                // Checks if the parameter is of type array, if so then its contents are validated recursively
                ArrayParameter arrayParameter = parameter as ArrayParameter;
                if (arrayParameter != null)
                {
                    ArrayParameter expectedArrayParameter = expectedParameter as ArrayParameter;
                    this.ValidateParseOutput(
                        Enumerable.Range(0, arrayParameter.Value.Count()).ToDictionary(index => $"[{index}]", index => arrayParameter.Value.ElementAt(index)),
                        Enumerable.Range(0, expectedArrayParameter.Value.Count()).ToDictionary(index => $"[{index}]", index => expectedArrayParameter.Value.ElementAt(index)));
                }
            }
        }

        #endregion

        #region General Test Methods

        /// <summary>
        /// Tests how the parser handles empty command line parameters.
        /// </summary>
        [Fact]
        public void EmptyCommandLineParametersTest()
        {
            // Parses empty command line parameters
            CommandLineParser parser = new CommandLineParser();
            ParameterBag parameterBag = parser.Parse(string.Empty);

            // Validates that the parsed parameters are correct
            this.ValidateParseOutput(parameterBag.Parameters, new Dictionary<string, Parameter>());
        }

        #endregion

        #region Default Parameter Test Methods

        /// <summary>
        /// Tests how the parser handles a single default parameter.
        /// </summary>
        [Fact]
        public void SingleDefaultParameterTest()
        {
            // Parses a single default command line parameter
            CommandLineParser parser = new CommandLineParser();
            ParameterBag parameterBag = parser.Parse("abcXYZ");

            // Validates that the parsed parameters are correct
            Assert.Equal(1, parameterBag.DefaultParameters.Count());
            Assert.Equal("abcXYZ", parameterBag.DefaultParameters.OfType<DefaultParameter>().First().Value);
        }

        /// <summary>
        /// Tests how the parser handles multiple default parameters.
        /// </summary>
        [Fact]
        public void MutlipleDefaultParameterTest()
        {
            // Parses multiple default command line parameters
            CommandLineParser parser = new CommandLineParser();
            ParameterBag parameterBag = parser.Parse("abc \"123 456\" XYZ \"789 0\"");

            // Validates that the parsed parameters are correct
            Assert.Equal(4, parameterBag.DefaultParameters.Count());
            Assert.Equal("abc", parameterBag.DefaultParameters.OfType<DefaultParameter>().ElementAt(0).Value);
            Assert.Equal("123 456", parameterBag.DefaultParameters.OfType<DefaultParameter>().ElementAt(1).Value);
            Assert.Equal("XYZ", parameterBag.DefaultParameters.OfType<DefaultParameter>().ElementAt(2).Value);
            Assert.Equal("789 0", parameterBag.DefaultParameters.OfType<DefaultParameter>().ElementAt(3).Value);
        }

        #endregion

        #region Parameter Test Methods

        /// <summary>
        /// Tests how the parser handles Windows style switches.
        /// </summary>
        [Fact]
        public void WindowsStyleSwitchTest()
        {
            // Parses a Windows style switch
            CommandLineParser parser = new CommandLineParser();
            ParameterBag parameterBag = parser.Parse("/Switch");

            // Validates that the parsed parameters are correct
            this.ValidateParseOutput(parameterBag.Parameters, new Dictionary<string, Parameter>
            {
                ["Switch"] = new BooleanParameter { Value = true }
            });
        }

        /// <summary>
        /// Tests how the parser handles Windows style parameters.
        /// </summary>
        [Fact]
        public void WindowsStyleParameterTest()
        {
            // Parses a Windows style parameter
            CommandLineParser parser = new CommandLineParser();
            ParameterBag parameterBag = parser.Parse("/Parameter:123");

            // Validates that the parsed parameters are correct
            this.ValidateParseOutput(parameterBag.Parameters, new Dictionary<string, Parameter>
            {
                ["Parameter"] = new NumberParameter { Value = 123.0M }
            });
        }

        /// <summary>
        /// Tests how the parser handles UNIX style switches.
        /// </summary>
        [Fact]
        public void UnixStyleSwitchTest()
        {
            // Parses a UNIX style switch
            CommandLineParser parser = new CommandLineParser();
            ParameterBag parameterBag = parser.Parse("--Switch");

            // Validates that the parsed parameters are correct
            this.ValidateParseOutput(parameterBag.Parameters, new Dictionary<string, Parameter>
            {
                ["Switch"] = new BooleanParameter { Value = true }
            });
        }

        /// <summary>
        /// Tests how the parser handles UNIX style parameters.
        /// </summary>
        [Fact]
        public void UnixStyleParameterTest()
        {
            // Parses a UNIX style parameter
            CommandLineParser parser = new CommandLineParser();
            ParameterBag parameterBag = parser.Parse("--Parameter=\"abc XYZ\"");

            // Validates that the parsed parameters are correct
            this.ValidateParseOutput(parameterBag.Parameters, new Dictionary<string, Parameter>
            {
                ["Parameter"] = new StringParameter { Value = "abc XYZ" }
            });
        }

        /// <summary>
        /// Tests how the parser handles UNIX style flagged switches.
        /// </summary>
        [Fact]
        public void UnixStyleFlaggedSwitchesTest()
        {
            // Parses a UNIX style flagged switch
            CommandLineParser parser = new CommandLineParser();
            ParameterBag parameterBag = parser.Parse("-sUtZ");

            // Validates that the parsed parameters are correct
            this.ValidateParseOutput(parameterBag.Parameters, new Dictionary<string, Parameter>
            {
                ["s"] = new BooleanParameter { Value = true },
                ["U"] = new BooleanParameter { Value = true },
                ["t"] = new BooleanParameter { Value = true },
                ["Z"] = new BooleanParameter { Value = true }
            });
        }

        /// <summary>
        /// Tests how the parser handles UNIX style alias parameters.
        /// </summary>
        [Fact]
        public void UnixStyleAliasParameterTest()
        {
            // Parses a UNIX style alias parameters
            CommandLineParser parser = new CommandLineParser();
            ParameterBag parameterBag = parser.Parse("-s=123.456");

            // Validates that the parsed parameters are correct
            this.ValidateParseOutput(parameterBag.Parameters, new Dictionary<string, Parameter>
            {
                ["s"] = new NumberParameter { Value = 123.456M }
            });
        }

        /// <summary>
        /// Tests how the parser handles multiple parameters.
        /// </summary>
        [Fact]
        public void MultipleParameterTest()
        {
            // Parses a multiple parameter
            CommandLineParser parser = new CommandLineParser();
            ParameterBag parameterBag = parser.Parse("/on /key:value --auto --parameter=123 -aFl -h false");

            // Validates that the parsed parameters are correct
            this.ValidateParseOutput(parameterBag.Parameters, new Dictionary<string, Parameter>
            {
                ["on"] = new BooleanParameter { Value = true },
                ["key"] = new StringParameter { Value = "value" },
                ["auto"] = new BooleanParameter { Value = true },
                ["parameter"] = new NumberParameter { Value = 123.0M },
                ["a"] = new BooleanParameter { Value = true },
                ["F"] = new BooleanParameter { Value = true },
                ["l"] = new BooleanParameter { Value = true },
                ["h"] = new BooleanParameter { Value = false }
            });
        }

        #endregion

        #region Mixed Default Parameter & Parameter Test Methods

        /// <summary>
        /// Tests how the parser handles mixing of default parameters and parameters.
        /// </summary>
        [Fact]
        public void MixedDefaultParameterAndParameterTest()
        {
            // Parses multiple parameters
            CommandLineParser parser = new CommandLineParser();
            ParameterBag parameterBag = parser.Parse("\"C:\\Users\\name\\Downloads\" /key:value --auto");

            // Validates that the parsed parameters are correct
            Assert.Equal(1, parameterBag.DefaultParameters.Count());
            Assert.Equal("C:\\Users\\name\\Downloads", parameterBag.DefaultParameters.OfType<DefaultParameter>().First().Value);
            this.ValidateParseOutput(parameterBag.Parameters, new Dictionary<string, Parameter>
            {
                ["key"] = new StringParameter { Value = "value" },
                ["auto"] = new BooleanParameter { Value = true }
            });
        }

        #endregion

        #region Data Type Test Methods

        /// <summary>
        /// Tests how the parser handles boolean values.
        /// </summary>
        [Fact]
        public void BooleanDataTypeTest()
        {
            // Parses a boolean value
            CommandLineParser parser = new CommandLineParser();
            ParameterBag parameterBag = parser.Parse("/first:false --second=true");

            // Validates that the parsed parameters are correct
            this.ValidateParseOutput(parameterBag.Parameters, new Dictionary<string, Parameter>
            {
                ["first"] = new BooleanParameter { Value = false },
                ["second"] = new BooleanParameter { Value = true }
            });
        }

        /// <summary>
        /// Tests how the parser handles numbers.
        /// </summary>
        [Fact]
        public void NumberDataTypeTest()
        {
            // Parses a positive integer and validates that the parsed parameters are correct
            CommandLineParser parser = new CommandLineParser();
            ParameterBag parameterBag = parser.Parse("/parameter=123");
            this.ValidateParseOutput(parameterBag.Parameters, new Dictionary<string, Parameter>
            {
                ["parameter"] = new NumberParameter { Value = 123.0M }
            });

            // Parses a negative integer and validates that the parsed parameters are correct
            parameterBag = parser.Parse("--parameter:-123");
            this.ValidateParseOutput(parameterBag.Parameters, new Dictionary<string, Parameter>
            {
                ["parameter"] = new NumberParameter { Value = -123.0M }
            });

            // Parses a positive floating point number and validates that the parsed parameters are correct
            parameterBag = parser.Parse("/parameter 123.456");
            this.ValidateParseOutput(parameterBag.Parameters, new Dictionary<string, Parameter>
            {
                ["parameter"] = new NumberParameter { Value = 123.456M }
            });

            // Parses a negative floating point number and validates that the parsed parameters are correct
            parameterBag = parser.Parse("--parameter -123.456");
            this.ValidateParseOutput(parameterBag.Parameters, new Dictionary<string, Parameter>
            {
                ["parameter"] = new NumberParameter { Value = -123.456M }
            });

            // Parses a positive floating point number with no digits before the decimal point and validates that the parsed parameters are correct
            parameterBag = parser.Parse("/parameter .123");
            this.ValidateParseOutput(parameterBag.Parameters, new Dictionary<string, Parameter>
            {
                ["parameter"] = new NumberParameter { Value = 0.123M }
            });

            // Parses a negative floating point number with no digits before the decimal point and validates that the parsed parameters are correct
            parameterBag = parser.Parse("--parameter:-.123");
            this.ValidateParseOutput(parameterBag.Parameters, new Dictionary<string, Parameter>
            {
                ["parameter"] = new NumberParameter { Value = -0.123M }
            });

            // Parses a positive floating point number with no digits after the decimal point and validates that the parsed parameters are correct
            parameterBag = parser.Parse("/parameter=123.");
            this.ValidateParseOutput(parameterBag.Parameters, new Dictionary<string, Parameter>
            {
                ["parameter"] = new NumberParameter { Value = 123.0M }
            });

            // Parses a negative floating point number with no digits after the decimal point and validates that the parsed parameters are correct
            parameterBag = parser.Parse("--parameter=-123.");
            this.ValidateParseOutput(parameterBag.Parameters, new Dictionary<string, Parameter>
            {
                ["parameter"] = new NumberParameter { Value = -123.0M }
            });
        }

        /// <summary>
        /// Tests how the parser handles un-quoted strings.
        /// </summary>
        [Fact]
        public void StringDataTypeTest()
        {
            // Parses an un-quoted string value
            CommandLineParser parser = new CommandLineParser();
            ParameterBag parameterBag = parser.Parse("/first:abc --second=XYZ");

            // Validates that the parsed parameters are correct
            this.ValidateParseOutput(parameterBag.Parameters, new Dictionary<string, Parameter>
            {
                ["first"] = new StringParameter { Value = "abc" },
                ["second"] = new StringParameter { Value = "XYZ" }
            });
        }

        /// <summary>
        /// Tests how the parsers handles quoted strings.
        /// </summary>
        [Fact]
        public void QuotedStringDataTypeTest()
        {
            // Parses a quoted string value
            CommandLineParser parser = new CommandLineParser();
            ParameterBag parameterBag = parser.Parse("/parameter \"abc XYZ 123 ! § $ % & / ( ) = ? \\\"");

            // Validates that the parsed parameters are correct
            this.ValidateParseOutput(parameterBag.Parameters, new Dictionary<string, Parameter>
            {
                ["parameter"] = new StringParameter { Value = "abc XYZ 123 ! § $ % & / ( ) = ? \\" }
            });
        }

        /// <summary>
        /// Tests how the parser handles arrays.
        /// </summary>
        [Fact]
        public void ArrayDataTypeTest()
        {
            // Parses an empty array and validates that the parsed parameters are correct
            CommandLineParser parser = new CommandLineParser();
            ParameterBag parameterBag = parser.Parse("--parameter=[]");
            this.ValidateParseOutput(parameterBag.Parameters, new Dictionary<string, Parameter>
            {
                ["parameter"] = new ArrayParameter { Value = new List<Parameter>() }
            });

            // Parses an array with a single element and validates that the parsed parameters are correct
            parameterBag = parser.Parse("/parameter [123]");
            this.ValidateParseOutput(parameterBag.Parameters, new Dictionary<string, Parameter>
            {
                ["parameter"] = new ArrayParameter
                {
                    Value = new List<Parameter> { new NumberParameter { Value = 123.0M } }
                }
            });

            // Parses an array with all different kinds of data types and validates that the parsed parameters are correct
            parameterBag = parser.Parse("--parameter:[false, 123.456, abcXYZ, \"abc XYZ 123\"]");
            this.ValidateParseOutput(parameterBag.Parameters, new Dictionary<string, Parameter>
            {
                ["parameter"] = new ArrayParameter
                {
                    Value = new List<Parameter>
                    {
                        new BooleanParameter { Value = false },
                        new NumberParameter { Value = 123.456M },
                        new StringParameter { Value = "abcXYZ" },
                        new StringParameter { Value = "abc XYZ 123" }
                    }
                }
            });

            // Parses a jagged array (array of arrays) and validates that the parsed parameters are correct
            parameterBag = parser.Parse("/parameter:[123, [abcXYZ, true]]");
            this.ValidateParseOutput(parameterBag.Parameters, new Dictionary<string, Parameter>
            {
                ["parameter"] = new ArrayParameter
                {
                    Value = new List<Parameter>
                    {
                        new NumberParameter { Value = 123.0M },
                        new ArrayParameter
                        {
                            Value = new List<Parameter>
                            {
                                new StringParameter { Value = "abcXYZ" },
                                new BooleanParameter { Value = true }
                            }
                        }
                    }
                }
            });
        }

        #endregion

        #region Parser Errors Test Methods

        /// <summary>
        /// Tests how the lexer handles too many dashes in front of a parameter name.
        /// </summary>
        [Fact]
        public void ToManyDashesTest() => Assert.Throws<CommandLineParserException>(() => new CommandLineParser().Parse("---parameter"));

        /// <summary>
        /// Tests how the lexer handles too many slashes in front of a parameter name.
        /// </summary>
        [Fact]
        public void ToManySlashesTest() => Assert.Throws<CommandLineParserException>(() => new CommandLineParser().Parse("//parameter"));

        /// <summary>
        /// Tests how the lexer handles strings that are missing a closing quote.
        /// </summary>
        [Fact]
        public void ToManyInvalidStringTest() => Assert.Throws<CommandLineParserException>(() => new CommandLineParser().Parse("\"value"));

        /// <summary>
        /// Tests how the parser handles a missing parameter value.
        /// </summary>
        [Fact]
        public void MissingParameterValueTest() => Assert.Throws<CommandLineParserException>(() => new CommandLineParser().Parse("--parameter1= /parameter2"));
        
        /// <summary>
        /// Tests how the parser handles a missing parameter name in front of a parameter value.
        /// </summary>
        [Fact]
        public void MissingParameterNameTest() => Assert.Throws<CommandLineParserException>(() => new CommandLineParser().Parse("--parameter=123 \"value\""));

        /// <summary>
        /// Tests how the parser handles invalid default parameter formats.
        /// </summary>
        [Fact]
        public void InvalidDefaultParameter() => Assert.Throws<CommandLineParserException>(() => new CommandLineParser().Parse("abc 123 \"xyz\""));

        #endregion

        #region Command Line Parameter Injection Test Methods

        /// <summary>
        /// Tests how the parser handles the command line parameter injection into an empty class.
        /// </summary>
        [Fact]
        public void EmptyParameterContainerInjectionTest()
        {
            // Parses an empty command line and validates that the object was properly created
            CommandLineParser parser = new CommandLineParser();
            EmptyParameterContainer emptyParameterContainer = parser.Bind<EmptyParameterContainer>(string.Empty);
            Assert.NotNull(emptyParameterContainer);

            // Parses several command line parameters and validates that the object was properly created
            emptyParameterContainer = parser.Bind<EmptyParameterContainer>("/on /key:value --auto --parameter=123 -aFl");
            Assert.NotNull(emptyParameterContainer);
        }

        /// <summary>
        /// Tests how the parser handles the command line parameter injection into a class that has a single non-default constructor.
        /// </summary>
        [Fact]
        public void SingleConstructorParameterContainerInjectionTest()
        {
            // Parses command line parameters and validates that the object was properly created
            CommandLineParser parser = new CommandLineParser();
            SingleConstructorParameterContainer singleConstructorParameterContainer = parser.Bind<SingleConstructorParameterContainer>("/first \"abc XYZ\" --second:-123.456");
            Assert.NotNull(singleConstructorParameterContainer);
            Assert.Equal("abc XYZ", singleConstructorParameterContainer.First);
            Assert.Equal(-123, singleConstructorParameterContainer.Second);
        }

        /// <summary>
        /// Tests how the parser handles the command line parameter injection into a class that has a multiple non-default constructor.
        /// </summary>
        [Fact]
        public void MultipleConstructorParameterContainerInjectionTest()
        {
            // Parses a single command line parameter and validates that the correct constructor was called
            CommandLineParser parser = new CommandLineParser();
            MultipleConstructorsParameterContainer multipleConstructorsParameterContainer = parser.Bind<MultipleConstructorsParameterContainer>("/first true");
            Assert.Equal(1, multipleConstructorsParameterContainer.ConstructorCalled);

            // Parses two command line parameters and validates that the correct constructor was called
            multipleConstructorsParameterContainer = parser.Bind<MultipleConstructorsParameterContainer>("/first true --second=abc");
            Assert.Equal(2, multipleConstructorsParameterContainer.ConstructorCalled);

            // Parses three command line parameters and validates that the correct constructor was called
            multipleConstructorsParameterContainer = parser.Bind<MultipleConstructorsParameterContainer>("/first true --second=abc /third:123");
            Assert.Equal(3, multipleConstructorsParameterContainer.ConstructorCalled);
        }

        /// <summary>
        /// Tests how the parser handles the command line parameter injection into a class with some simple-type properties.
        /// </summary>
        [Fact]
        public void SimplePropertyParameterContainerInjectionTest()
        {
            // Parses command line parameters and validates that the object was properly created
            CommandLineParser parser = new CommandLineParser();
            SimplePropertyParameterContainer simplePropertyParameterContainer = parser.Bind<SimplePropertyParameterContainer>("/string \"abc XYZ\" --number:123.456 --boolean=true /enum:Monday");
            Assert.NotNull(simplePropertyParameterContainer);
            Assert.Equal("abc XYZ", simplePropertyParameterContainer.String);
            Assert.Equal(123.456d, simplePropertyParameterContainer.Number);
            Assert.Equal(true, simplePropertyParameterContainer.Boolean);
            Assert.Equal(DayOfWeek.Monday, simplePropertyParameterContainer.Enumeration);
        }

        /// <summary>
        /// Tests how the parser handles the command line parameter injection into a class with array-type properties.
        /// </summary>
        [Fact]
        public void ArrayPropertyParameterContainerInjectionTest()
        {
            // Parses command line parameters that contain an array of boolean values and validates that the object was properly created
            CommandLineParser parser = new CommandLineParser();
            ArrayPropertyParameterContainer arrayPropertyParameterContainer = parser.Bind<ArrayPropertyParameterContainer>("/BooleanCollection [true, 1, false, 0]");
            Assert.NotNull(arrayPropertyParameterContainer.BooleanCollection);
            Assert.True(arrayPropertyParameterContainer.BooleanCollection.ElementAt(0));
            Assert.True(arrayPropertyParameterContainer.BooleanCollection.ElementAt(1));
            Assert.False(arrayPropertyParameterContainer.BooleanCollection.ElementAt(2));
            Assert.False(arrayPropertyParameterContainer.BooleanCollection.ElementAt(3));

            // Parses command line parameters that contain an array of string values and validates that the object was properly created
            arrayPropertyParameterContainer = parser.Bind<ArrayPropertyParameterContainer>("/StringCollection [abc, 123, \"abc XYZ\", 123.456, true]");
            Assert.NotNull(arrayPropertyParameterContainer.StringCollection);
            Assert.Equal("abc", arrayPropertyParameterContainer.StringCollection[0]);
            Assert.Equal("123", arrayPropertyParameterContainer.StringCollection[1]);
            Assert.Equal("abc XYZ", arrayPropertyParameterContainer.StringCollection[2]);
            Assert.Equal("123.456", arrayPropertyParameterContainer.StringCollection[3]);
            Assert.Equal("True", arrayPropertyParameterContainer.StringCollection[4]);

            // Parses command line parameters that contain an array of number values and validates that the object was properly created
            arrayPropertyParameterContainer = parser.Bind<ArrayPropertyParameterContainer>("--NumberCollection:[123.456, 123, true, \"456\"]");
            Assert.NotNull(arrayPropertyParameterContainer.NumberCollection);
            Assert.Equal(123.456d, arrayPropertyParameterContainer.NumberCollection.ElementAt(0));
            Assert.Equal(123.0d, arrayPropertyParameterContainer.NumberCollection.ElementAt(1));
            Assert.Equal(1.0d, arrayPropertyParameterContainer.NumberCollection.ElementAt(2));
            Assert.Equal(456.0d, arrayPropertyParameterContainer.NumberCollection.ElementAt(3));

            // Parses command line parameters that contain an array of enumeration values and validates that the object was properly created
            arrayPropertyParameterContainer = parser.Bind<ArrayPropertyParameterContainer>("/EnumerationCollection [Monday, \"Tuesday\", Wednesday, \"Thursday\", Friday, \"Saturday\", Sunday]");
            Assert.NotNull(arrayPropertyParameterContainer.EnumerationCollection);
            Assert.Equal(DayOfWeek.Monday, arrayPropertyParameterContainer.EnumerationCollection[0]);
            Assert.Equal(DayOfWeek.Tuesday, arrayPropertyParameterContainer.EnumerationCollection[1]);
            Assert.Equal(DayOfWeek.Wednesday, arrayPropertyParameterContainer.EnumerationCollection[2]);
            Assert.Equal(DayOfWeek.Thursday, arrayPropertyParameterContainer.EnumerationCollection[3]);
            Assert.Equal(DayOfWeek.Friday, arrayPropertyParameterContainer.EnumerationCollection[4]);
            Assert.Equal(DayOfWeek.Saturday, arrayPropertyParameterContainer.EnumerationCollection[5]);
            Assert.Equal(DayOfWeek.Sunday, arrayPropertyParameterContainer.EnumerationCollection[6]);
        }

        /// <summary>
        /// Tests how the parser handles the command line parameter injection into a class that has properties with parameter name aliases.
        /// </summary>
        [Fact]
        public void AliasParameterContainerInjectionTest()
        {
            // Parses command line parameters and validates that the object was properly created
            CommandLineParser parser = new CommandLineParser();
            AliasParameterContainer aliasParameterContainer = parser.Bind<AliasParameterContainer>("/s \"abc XYZ\" -n:123.456");
            Assert.NotNull(aliasParameterContainer);
            Assert.Equal(123.456M, aliasParameterContainer.Number);
            Assert.Equal("abc XYZ", aliasParameterContainer.String);
        }

        #endregion

        #region Default Parameter Injection Test Methods

        /// <summary>
        /// Tests how the parser handles the default parameter injection into the constructor.
        /// </summary>
        [Fact]
        public void DefaultParameterContructorContainerInjectionTest()
        {
            // Parses default parameters and validates that the object was properly created
            CommandLineParser parser = new CommandLineParser();
            DefaultParameterContructorContainer defaultParameterContructorContainer = parser.Bind<DefaultParameterContructorContainer>("abc \"XYZ\"");
            Assert.NotNull(defaultParameterContructorContainer);
            Assert.Equal("abc XYZ", defaultParameterContructorContainer.DefaultParameters);
        }

        /// <summary>
        /// Tests how the parser handles the default parameter injection into properties of a class.
        /// </summary>
        [Fact]
        public void DeafultParameterPropertyContainerInjectionTest()
        {
            // Parses default parameters and validates that the object was properly created
            CommandLineParser parser = new CommandLineParser();
            DeafultParameterPropertyContainer deafultParameterPropertyContainer = parser.Bind<DeafultParameterPropertyContainer>("abc \"XYZ\"");
            Assert.NotNull(deafultParameterPropertyContainer);
            Assert.Equal("abc XYZ", deafultParameterPropertyContainer.First);
            Assert.Equal("abc", deafultParameterPropertyContainer.Second.ElementAt(0));
            Assert.Equal("XYZ", deafultParameterPropertyContainer.Second.ElementAt(1));
        }

        #endregion

        #region Nested Types

        /// <summary>
        /// Represents an empty parameter container, which is used to test injection into an empty class.
        /// </summary>
        private class EmptyParameterContainer { }
        
        /// <summary>
        /// Represents a parameter container, which is used to test single constructors.
        /// </summary>
        private class SingleConstructorParameterContainer
        {
            #region Constructors

            /// <summary>
            /// Initializes a new <see cref="SingleConstructorParameterContainer"/> instance.
            /// </summary>
            /// <param name="first">The first command line parameter.</param>
            /// <param name="second">The second command line parameter.</param>
            public SingleConstructorParameterContainer(string first, int second)
            {
                this.First = first;
                this.Second = second;
            }

            #endregion

            #region Public Properties

            /// <summary>
            /// Gets or sets the "first" command line parameter.
            /// </summary>
            public string First { get; set; }

            /// <summary>
            /// Gets or sets the "second" command line parameter.
            /// </summary>
            public int Second { get; set; }

            #endregion
        }

        /// <summary>
        /// Represents a parameter container, which is used to test multiple constructors.
        /// </summary>
        private class MultipleConstructorsParameterContainer
        {
            #region Constructors

            /// <summary>
            /// Initializes a new <see cref="MultipleConstructorsParameterContainer"/> instance.
            /// </summary>
            /// <param name="first">The first command line parameter.</param>
            public MultipleConstructorsParameterContainer(bool first)
            {
                this.ConstructorCalled = 1;
            }

            /// <summary>
            /// Initializes a new <see cref="MultipleConstructorsParameterContainer"/> instance.
            /// </summary>
            /// <param name="first">The first command line parameter.</param>
            /// <param name="second">The second command line parameter.</param>
            public MultipleConstructorsParameterContainer(bool first, string second)
            {
                this.ConstructorCalled = 2;
            }

            /// <summary>
            /// Initializes a new <see cref="MultipleConstructorsParameterContainer"/> instance.
            /// </summary>
            /// <param name="first">The first command line parameter.</param>
            /// <param name="second">The second command line parameter.</param>
            /// <param name="third">The third command line parameter.</param>
            public MultipleConstructorsParameterContainer(bool first, string second, int third)
            {
                this.ConstructorCalled = 3;
            }

            #endregion

            #region Public Properties

            /// <summary>
            /// Gets or sets the number of the constructor that has been called, which is used to validate that the correct constructor has been called.
            /// </summary>
            public int ConstructorCalled { get; set; }

            #endregion
        }

        /// <summary>
        /// Represents a parameter container, which is used to test the injection of simple data types.
        /// </summary>
        private class SimplePropertyParameterContainer
        {
            #region Public Properties

            /// <summary>
            /// Gets or sets the "number" command line parameter.
            /// </summary>
            [ParameterName("number")]
            public double Number { get; set; }

            /// <summary>
            /// Gets or sets the "boolean" command line parameter.
            /// </summary>
            [ParameterName("boolean")]
            public bool Boolean { get; set; }

            /// <summary>
            /// Gets or sets the "string" command line parameter.
            /// </summary>
            [ParameterName("string")]
            public string String { get; set; }

            /// <summary>
            /// Gets or sets the "enum" command line parameter.
            /// </summary>
            [ParameterName("enum")]
            public DayOfWeek Enumeration { get; set; }

            #endregion
        }

        /// <summary>
        /// Represents a parameter container, which is used to test the injection of array data types.
        /// </summary>
        private class ArrayPropertyParameterContainer
        {
            #region Public Properties

            /// <summary>
            /// Gets or sets the "BooleanCollection" command line parameter.
            /// </summary>
            public IEnumerable<bool> BooleanCollection { get; set; }

            /// <summary>
            /// Gets or sets the "StringCollection" command line parameter.
            /// </summary>
            public List<string> StringCollection { get; set; }

            /// <summary>
            /// Gets or sets the "NumberCollection" command line parameter.
            /// </summary>
            public HashSet<double> NumberCollection { get; set; }

            /// <summary>
            /// Gets or sets the "EnumerationCollection" command line parameter.
            /// </summary>
            public IList<DayOfWeek> EnumerationCollection { get; set; }

            #endregion
        }

        /// <summary>
        /// Represents a parameter container, which is used to test the injection of alias parameters.
        /// </summary>
        private class AliasParameterContainer
        {
            #region Public Properties

            /// <summary>
            /// Gets or sets the command line parameter with the name "number" and the alias "n".
            /// </summary>
            [ParameterName("number", "n")]
            public decimal Number { get; set; }

            /// <summary>
            /// Gets or sets the command line parameter with the name "string" and the alias "s".
            /// </summary>
            [ParameterName("string", "s")]
            public string String { get; set; }

            #endregion
        }

        /// <summary>
        /// Represents a parameter container, which is used to test the injection of default parameter into the constructor of a class.
        /// </summary>
        private class DefaultParameterContructorContainer
        {
            #region Constructors

            /// <summary>
            /// Initializes a new <see cref="DefaultParameterContructorContainer"/> instance.
            /// </summary>
            /// <param name="defaultParameters">The default parameters that are being injected.</param>
            public DefaultParameterContructorContainer([DefaultParameter] string defaultParameters)
            {
                this.DefaultParameters = defaultParameters;
            }

            #endregion

            #region Public Properties

            /// <summary>
            /// Gets or sets the default parametetrs that have been injected into the constructor.
            /// </summary>
            public string DefaultParameters { get; set; }

            #endregion
        }

        /// <summary>
        /// Represents a parameter container, which is used to test the injection of default parameter into the properties of a class.
        /// </summary>
        private class DeafultParameterPropertyContainer
        {
            #region Public Properties

            /// <summary>
            /// Gets or sets the default parametetrs that have been injected into the constructor as a string.
            /// </summary>
            [DefaultParameter]
            public string First { get; set; }

            /// <summary>
            /// Gets or sets the default parametetrs that have been injected into the constructor as a collection of strings.
            /// </summary>
            [DefaultParameter]
            public IEnumerable<string> Second { get; set; }

            #endregion
        }

        #endregion
    }
}