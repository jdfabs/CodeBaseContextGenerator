using System.Collections.Immutable;
using Antlr4.Runtime;
using CodeBaseContextGenerator.Core.Interfaces.Representations;
using CodeBaseContextGenerator.Core.Interfaces.Representations.Properties;
using CodeBaseContextGenerator.Core.Models.Representations;
using CodeBaseContextGenerator.Core.Shared;
using CodeBaseContextGenerator.Languages.JavaAntlr4.Base;

namespace CodeBaseContextGenerator.Languages.JavaAntlr4.Visitors__;

public class JavaRepresentationAssembler(CommonTokenStream tokenStream)
{
   private readonly JavaExtractor extractor = new ();
   public TypeRepresentationBase CreateRepresentation(ParserRuleContext node, CommonTokenStream tokenStream)
   {
       var (path, linestart, lineend, colstart,colend) = extractor.ExtractSourceAnchor(node);
       string code = extractor.ExtractCode(node);
       return new TypeRepresentationBase()
       {
           Name = extractor.ExtractName(node),
           FullQualifiedName = extractor.ExtractFullQualifiedName(node),
           Modifiers = extractor.ExtractModifiers(node),
           Docs = extractor.ExtractDocs(node, tokenStream),
           FilePath = path,
           Code = code,
           ReferenceTypes = extractor.ExtractReferences(node),
           StartLine = linestart,
           EndLine = lineend,
           StartColumn = colstart ,
           EndColumn = colend,
           Type = extractor.ExtractType(node),
           Summary = SummaryGenerator.Summarize(code),
   
           BaseTypes = extractor.ExtractInheritedTypes(node),
           Fields = extractor.ExtractorFields(node),
           Methods = extractor.ExtractMethods(node),
           Constructors = extractor.ExtractConstructor(node),
           NestedTypes = extractor.ExtractNestedType(node),
       };
   } 
}

