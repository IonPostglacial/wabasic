using DotNetComp.Compiler.Ast;

namespace DotNetComp.Compiler.Typing;

public record MethodDefinition(string Name, IEnumerable<MetaAttribute> Attributes, IReadOnlyList<ITypeParameter> TypeParameters);
