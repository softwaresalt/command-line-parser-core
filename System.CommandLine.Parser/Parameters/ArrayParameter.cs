﻿
#region Using Directives

using System.Collections.Generic;

#endregion

namespace System.CommandLine.Parser.Parameters
{
    /// <summary>
    /// Represents an array command line parameter.
    /// </summary>
    public class ArrayParameter : Parameter
    {
        #region Public Properties

        /// <summary>
        /// Gets the kind of the parameter.
        /// </summary>
        public override ParameterKind Kind
        {
            get
            {
                return ParameterKind.Array;
            }
        }

        /// <summary>
        /// Gets the boolean value of the command line parameter.
        /// </summary>
        public IEnumerable<Parameter> Value { get; internal set; }

        #endregion
    }
}