
using Microsoft.CodeAnalysis;

namespace Tools.SourceGenerators
{
   /// <summary>
   /// Roslyn Source generator to generate a internal class holding the Assembly Version Information.
   /// Implements the <see cref="ISourceGenerator" />
   /// </summary>
   /// <seealso cref="ISourceGenerator" />
   [Generator]
   public class AssemblyVersionGenerator : ISourceGenerator
   {
      /// <summary>
      /// Called to perform source generation.
      /// </summary>
      /// <param name="context">The <see cref="GeneratorExecutionContext" /> to add source to.</param>
      public void Execute(GeneratorExecutionContext context)
      {
         context.AnalyzerConfigOptions.GlobalOptions.TryGetValue("build_property.RootNamespace", out var rootNamespace);
         context.AddSource("AssemblyVersion.g.cs", $@"
namespace {rootNamespace}.Generated
{{
   internal static class AssemblyVersion
   {{
      public const string Version = ""{context.Compilation.Assembly.Identity.Version}"";
      public const string Name = ""{context.Compilation.Assembly.Name}"";
   }}
}}");
      }

      /// <summary>
      /// Called before generation occurs.
      /// </summary>
      /// <param name="context">The <see cref="GeneratorInitializationContext"/> to register callbacks on.</param>
      public void Initialize(GeneratorInitializationContext context)
      {
         //EMPTY_BODY - no initialization needed
      }
   }
}