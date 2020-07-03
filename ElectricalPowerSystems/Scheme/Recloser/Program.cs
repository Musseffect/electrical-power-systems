using System;
using System.Collections.Generic;
using static ElectricalPowerSystems.Scheme.Recloser.Compiler;

namespace ElectricalPowerSystems.Scheme.Recloser
{
    public class Program
    {
        public class Line
        {
            public int LineNumber;
            public int Count;
        }
        public ByteArray Bytecode { get { return code; } }
        //int[] lines;
        ByteArray code;
        Line[] lines;
        public Line[] Lines { get { return lines; } }
        public List<FunctionType> Functions { get { return functions; } }
        Dictionary<string, Compiler.FunctionType> functionTable;
        List<Compiler.FunctionType> functions;
        List<INativeFunction> nativeFunctions;
        List<Compiler.Variable> globals;
        public List<Compiler.Variable> Globals { get { return globals; } }
        public Program(ByteArray bytecode, List<Compiler.Variable> globals, 
            List<Compiler.FunctionType> functions,Dictionary<string,Compiler.FunctionType> functionTable,
            List<INativeFunction> nativeFunctions, Line[] lines)
        {
            code = bytecode;
            this.functionTable = functionTable;
            this.functions = functions;
            this.globals = globals;
            this.nativeFunctions = nativeFunctions;
            this.lines = lines;
        }
        public void RegisterFunctions(List<Compiler.FunctionType> functions)
        {
            foreach (var function in functions)
            {
                functionTable.Add(function.Name, function);
            }
            this.functions = functions;
        }
        public Compiler.FunctionType GetFunctionType(string functionName)
        {
            return functionTable[functionName];
        }
        public int GetFunctionIndex(string function)
        {
            return functionTable[function].Index;
        }
        public Compiler.FunctionType GetFunctionType(int index)
        {
            return functions[index];
        }
        public void CallNative(Stack stack,int index)
        {
#if DEBUG||TEST
            if (index >= nativeFunctions.Count||index<0)
                throw new Exception("Incorrect native function index " + index);
#endif
            nativeFunctions[index].Execute(stack);
        }
        public bool HasFunction(string functionName, Compiler.Type[] argumentsTypes, Compiler.BasicType returnType)
        {
            if (!functionTable.ContainsKey(functionName))
                return false;
            FunctionType type = functionTable[functionName];
            if (!type.Signature.ReturnType.Equals(returnType))
                return false;
            if (argumentsTypes.Length != type.Signature.ArgumentTypes.Count)
                return false;
            for (int i=0;i<argumentsTypes.Length;i++)
            {
                if (!argumentsTypes[i].Equals(type.Signature.ArgumentTypes[i].Type) || type.Signature.ArgumentTypes[i].IsLocal == false)
                    return false;
            }
            return true;
        }
    }
}
