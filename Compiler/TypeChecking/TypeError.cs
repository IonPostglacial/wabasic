using DotNetComp.Compiler.Typing;

namespace DotNetComp.Compiler.TypeChecking;

public interface ITypeError { Range Range {get; } }
public record ExpectedTypeButGot(Range Range, TypeInstance Expected, TypeInstance Actual) : ITypeError;
public record ExpectedTypeInBranch(Range Range, TypeInstance Expected, TypeInstance Actual) : ITypeError;
public record UnknownTypeButExpectedError(Range Range, TypeInstance Expected) : ITypeError;
public record UnknownTypeError(Range Range) : ITypeError;
public record NotCallableTypeError(Range Range, TypeInstance? Actual) : ITypeError;
public record WrongArityError(Range Range, int Expected, int Actual) : ITypeError;
public record UnknownMethodError(Range Range, TypeInstance This, string Method) : ITypeError;