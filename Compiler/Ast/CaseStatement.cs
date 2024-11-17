namespace DotNetComp.Compiler.Ast;


public sealed record CaseStatement(Range Range, INode Compared, IEnumerable<CaseStatement.Clause> Clauses, INode? Default) : AbstractNode, ICallableStatement
{
    public bool Equals(CaseStatement? stmt)
    {
        if (stmt is not null)
        {
            return 
                Range.Equals(stmt.Range) && 
                Type == stmt.Type && Clauses.SequenceEqual(stmt.Clauses) && 
                ((Default is null && stmt.Default is null) ||
                (Default is not null && stmt.Default is not null && Convert.ChangeType(stmt.Default, Default.GetType()).Equals(Default)));
        }
        return false;
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(Range, Type, Tools.HashSequence(Clauses), Default);
    }

    public sealed record Clause(INode Input, INode Result);
    public static (INode, bool) FromArgList(Range span, List<INode> args)
    {
        if (args.Count < 3)
        {
            return (new InvalidNode(span), false);
        }
        List<INode> clauseArgs;
        List<Clause> clauses = [];
        INode? defaultValue;
        if (args.Count % 2 == 0)
        {
            clauseArgs = args[1..^1];
            defaultValue = args[^1];
        }
        else
        {
            clauseArgs = args[1..];
            defaultValue = null;
        }
        for (int i = 0; i < clauseArgs.Count / 2; i++)
        {
            clauses.Add(new Clause(clauseArgs[2*i], clauseArgs[2*i+1]));
        }
        return (new CaseStatement(span, args[0], clauses, defaultValue), true); 
    }

    public int[] Arity() => [-1];

    public T Accept<T>(INodeVisitor<T> visitor)
    {
        return visitor.VisitCaseStatement(this);
    }
}
