namespace DotNetComp.Compiler.JsGeneration;



public class ContextVariable(string initialName)
{
    public class Scope : IDisposable
    {
        private readonly ContextVariable variable;
        private readonly string oldName;

        public Scope(ContextVariable var, string name)
        {
            variable = var;
            oldName = variable.CurrentVariableName;
            variable.CurrentVariableName = name;
        }

        public void Dispose()
        {
            variable.CurrentVariableName = oldName;
        }
    }

    public string CurrentVariableName { get; private set; } = initialName;

    public Scope ScopedName(string name)
    {
        return new Scope(this, name);
    }
}