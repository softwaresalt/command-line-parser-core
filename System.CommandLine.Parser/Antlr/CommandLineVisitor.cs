//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     ANTLR Version: 4.5
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

// Generated from CommandLine.g4 by ANTLR 4.5

// Unreachable code detected
#pragma warning disable 0162
// The variable '...' is assigned but its value is never used
#pragma warning disable 0219
// Missing XML comment for publicly visible type or member '...'
#pragma warning disable 1591

namespace System.CommandLine.Parser.Antlr {
using Antlr4.Runtime.Misc;
using Antlr4.Runtime.Tree;
using IToken = Antlr4.Runtime.IToken;

/// <summary>
/// This interface defines a complete generic visitor for a parse tree produced
/// by <see cref="CommandLineParser"/>.
/// </summary>
/// <typeparam name="Result">The return type of the visit operation.</typeparam>
[System.CodeDom.Compiler.GeneratedCode("ANTLR", "4.5")]
[System.CLSCompliant(false)]
internal interface ICommandLineVisitor<Result> : IParseTreeVisitor<Result> {
	/// <summary>
	/// Visit a parse tree produced by <see cref="CommandLineParser.commandLine"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitCommandLine([NotNull] CommandLineParser.CommandLineContext context);
	/// <summary>
	/// Visit a parse tree produced by the <c>DefaultParameterString</c>
	/// labeled alternative in <see cref="CommandLineParser.defaultParameter"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitDefaultParameterString([NotNull] CommandLineParser.DefaultParameterStringContext context);
	/// <summary>
	/// Visit a parse tree produced by the <c>WindowsStyleSwitch</c>
	/// labeled alternative in <see cref="CommandLineParser.parameter"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitWindowsStyleSwitch([NotNull] CommandLineParser.WindowsStyleSwitchContext context);
	/// <summary>
	/// Visit a parse tree produced by the <c>WindowsStyleParameter</c>
	/// labeled alternative in <see cref="CommandLineParser.parameter"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitWindowsStyleParameter([NotNull] CommandLineParser.WindowsStyleParameterContext context);
	/// <summary>
	/// Visit a parse tree produced by the <c>UnixStyleSwitch</c>
	/// labeled alternative in <see cref="CommandLineParser.parameter"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitUnixStyleSwitch([NotNull] CommandLineParser.UnixStyleSwitchContext context);
	/// <summary>
	/// Visit a parse tree produced by the <c>UnixStyleParameter</c>
	/// labeled alternative in <see cref="CommandLineParser.parameter"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitUnixStyleParameter([NotNull] CommandLineParser.UnixStyleParameterContext context);
	/// <summary>
	/// Visit a parse tree produced by the <c>UnixStyleAliasParameter</c>
	/// labeled alternative in <see cref="CommandLineParser.parameter"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitUnixStyleAliasParameter([NotNull] CommandLineParser.UnixStyleAliasParameterContext context);
	/// <summary>
	/// Visit a parse tree produced by the <c>UnixStyleFlaggedSwitch</c>
	/// labeled alternative in <see cref="CommandLineParser.parameter"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitUnixStyleFlaggedSwitch([NotNull] CommandLineParser.UnixStyleFlaggedSwitchContext context);
	/// <summary>
	/// Visit a parse tree produced by the <c>String</c>
	/// labeled alternative in <see cref="CommandLineParser.value"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitString([NotNull] CommandLineParser.StringContext context);
	/// <summary>
	/// Visit a parse tree produced by the <c>Number</c>
	/// labeled alternative in <see cref="CommandLineParser.value"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitNumber([NotNull] CommandLineParser.NumberContext context);
	/// <summary>
	/// Visit a parse tree produced by the <c>Array</c>
	/// labeled alternative in <see cref="CommandLineParser.value"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitArray([NotNull] CommandLineParser.ArrayContext context);
	/// <summary>
	/// Visit a parse tree produced by the <c>Boolean</c>
	/// labeled alternative in <see cref="CommandLineParser.value"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitBoolean([NotNull] CommandLineParser.BooleanContext context);
}
} // namespace System.CommandLine.Parser.Antlr
