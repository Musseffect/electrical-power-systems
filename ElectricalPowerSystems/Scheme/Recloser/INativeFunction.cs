using System;

namespace ElectricalPowerSystems.Scheme.Recloser
{
    public interface INativeFunction
    {
        void Execute(Stack stack);
    }
    public class NativeFPow : INativeFunction
    {
        void INativeFunction.Execute(Stack stack)
        {
            Float a2 = stack.Pop() as Float;
            Float a1 = stack.Pop() as Float;
            Float result = new Float(Math.Pow(a1.Value, a2.Value));
            stack.Push(result);
        }
    }
    public class NativeFMin : INativeFunction
    {
        void INativeFunction.Execute(Stack stack)
        {
            Float a2 = stack.Pop() as Float;
            Float a1 = stack.Pop() as Float;
            Float result = new Float(Math.Min(a1.Value, a2.Value));
            stack.Push(result);
        }
    }
    public class NativeFMax : INativeFunction
    {
        void INativeFunction.Execute(Stack stack)
        {
            Float a2 = stack.Pop() as Float;
            Float a1 = stack.Pop() as Float;
            Float result = new Float(Math.Max(a1.Value, a2.Value));
            stack.Push(result);
        }
    }
    public class NativeIMin : INativeFunction
    {
        void INativeFunction.Execute(Stack stack)
        {
            Int a2 = stack.Pop() as Int;
            Int a1 = stack.Pop() as Int;
            Int result = new Int(Math.Min(a1.Value, a2.Value));
            stack.Push(result);
        }
    }
    public class NativeIMax : INativeFunction
    {
        void INativeFunction.Execute(Stack stack)
        {
            Int a2 = stack.Pop() as Int;
            Int a1 = stack.Pop() as Int;
            Int result = new Int(Math.Max(a1.Value, a2.Value));
            stack.Push(result);
        }
    }
    public class NativeFStep : INativeFunction
    {
        void INativeFunction.Execute(Stack stack)
        {
            Float arg = stack.Pop() as Float;
            Float result = new Float(arg.Value>=0?1:0);
            stack.Push(result);
        }
    }
    public class NativeIAbs : INativeFunction
    {
        void INativeFunction.Execute(Stack stack)
        {
            Int arg = stack.Pop() as Int;
            Int result = new Int(Math.Abs(arg.Value));
            stack.Push(result);
        }
    }
    public class NativeFAbs : INativeFunction
    {
        void INativeFunction.Execute(Stack stack)
        {
            Float arg = stack.Pop() as Float;
            Float result = new Float(Math.Abs(arg.Value));
            stack.Push(result);
        }
    }
    public class NativeFSqrt : INativeFunction
    {
        void INativeFunction.Execute(Stack stack)
        {
            Float arg = stack.Pop() as Float;
            Float result = new Float(Math.Sqrt(arg.Value));
            stack.Push(result);
        }
    }
    public class NativeFE : INativeFunction
    {
        void INativeFunction.Execute(Stack stack)
        {
            Float result = new Float(Math.E);
            stack.Push(result);
        }
    }
    public class NativeFPI : INativeFunction
    {
        void INativeFunction.Execute(Stack stack)
        {
            Float result = new Float(Math.PI);
            stack.Push(result);
        }
    }
    public class NativeFExp : INativeFunction
    {
        void INativeFunction.Execute(Stack stack)
        {
            Float arg = stack.Pop() as Float;
            Float result = new Float(Math.Exp(arg.Value));
            stack.Push(result);
        }
    }
    public class NativeFSin : INativeFunction
    {
        void INativeFunction.Execute(Stack stack)
        {
            Float arg = stack.Pop() as Float;
            Float result = new Float(Math.Sin(arg.Value));
            stack.Push(result);
        }
    }
    public class NativeFCos : INativeFunction
    {
        void INativeFunction.Execute(Stack stack)
        {
            Float arg = stack.Pop() as Float;
            Float result = new Float(Math.Cos(arg.Value));
            stack.Push(result);
        }
    }
}
