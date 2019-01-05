using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


/*

    basic types:
    int
    float
    string
    array
    complex

    native classes:
    static class Model
    abstract class Element
    class Resistance: Element
    class Capacitor: Element
    class Inductor: Element
    class Line: Element
    class VoltageSource: Element
    class CurrentSource: Element
    class Ground: Element


function list:
Resistance Model.resistance(string node1, string node2,float value)
Capacitor Model.capacitor(string node1, string node2)
Inductor Model.inductor(string node1, string node2)
Line Model.line(string node1,string node2)
void ground(string node)
VoltageSource Model.voltageSource(string node1,string node2,complex voltage,float frequency = 0)
VoltageSource Model.voltageSource(string node1,string node2,float amp,float phase,float frequency = 0)
CurrentSource Model.currentSource(string node1,string node2,complex current,float frequency = 0)
CurrentSource Model.currentSource(string node1,string node2,float amp,float phase,float frequency = 0)

void print(...)
float re(complex val)
float im(complex val)
float magn(complex val)
float phase(complex val)
float radians(float val)
float degrees(float val)
complex conj(complex val)

void Model.voltage(Element el)
void Model.voltage(string nodeLabel)
void Model.voltage(string node1,string node2)
void Model.current(Element el)
*/

namespace ElectricalPowerSystems.Parser
{
    delegate Variable FunctionExec(List<Variable> args);
        public class ArgumentSignature
        {
            public Variable defaultValue;
            public VariableType type;
        }
        public class FunctionSignature
        {
            public List<ArgumentSignature> Arguments { get; set; }
            public bool variableLength;
            public int defaultArgs;
        }
        public class AbstractMethod
        {
            public abstract Variable exec(List<Variable> args);
            public FunctionSignature Signature { get; set; }
        }
        public class NativeMethod
        {
            public abstract Variable exec(List<Variable> args);
            public FunctionSignature Signature { get; set; }
        }
        public class CustomMethod
        {

        }
        public abstract class AbstractFunction
        {
            public abstract Variable exec(List<Variable> args);
            public FunctionSignature Signature { get; set; }
            static public Variable compute(AbstractFunction f, List<Variable> variables)
            {
                List<Variable> args = new List<Variable>();
                if (variables.Count < f.Signature.Arguments.Count)
                {
                    if (f.Signature.defaultArgs >= f.Signature.Arguments.Count - variables.Count)
                    {
                        for (int i = 0; i < variables.Count; i++)
                        {
                            Variable var = convert(variables[i], max(f.Signature.Arguments[i].type, variables[i].Type));
                            args.Add(var);
                        }
                        for (int j = variables.Count; j < f.Signature.Arguments.Count; j++)
                        {
                            args.Add(f.Signature.Arguments[j].defaultValue);
                        }
                        return f.exec(args);
                    }
                    else
                        throw new Exception("Invalid number of arguments in function");
                }
                for (int i = 0; i < f.Signature.Arguments.Count; i++)
                {
                    Variable var = convert(variables[i], max(f.Signature.Arguments[i].defaultValue.Type, variables[i].Type));
                    args.Add(var);
                }
                if (variables.Count > f.Signature.Arguments.Count)
                {
                    if (f.Signature.variableLength)
                    {
                        for (int i = f.Signature.Arguments.Count; i < variables.Count; i++)
                            args.Add(variables[i]);
                    }
                    else
                        throw new Exception("Invalid number of arguments in function");
                }
                return f.exec(args);
            }
        public class NativeFunction:AbstractFunction
        {

        }
        public class CustomFunction : AbstractFunction
        {

        }
        public class FunctionDefinition
        {
            public FunctionExec Exec { get; set; }
            public FunctionSignature Signature { get; set; }
            static public Variable compute(FunctionDefinition f, List<Variable> variables)
            {
                List<Variable> args = new List<Variable>();
                if (variables.Count < f.Signature.Arguments.Count)
                {
                    if (f.Signature.defaultArgs >= f.Signature.Arguments.Count - variables.Count)
                    {
                        for (int i = 0; i < variables.Count; i++)
                        {
                            Variable var = convert(variables[i], max(f.Signature.Arguments[i].type, variables[i].Type));
                            args.Add(var);
                        }
                        for (int j = variables.Count; j < f.Signature.Arguments.Count; j++)
                        {
                            args.Add(f.Signature.Arguments[j].defaultValue);
                        }
                        return f.Exec(args);
                    }
                    else
                        throw new Exception("Invalid number of arguments in function");
                }
                for (int i = 0; i < f.Signature.Arguments.Count; i++)
                {
                    Variable var = convert(variables[i], max(f.Signature.Arguments[i].defaultValue.Type, variables[i].Type));
                    args.Add(var);
                }
                if (variables.Count > f.Signature.Arguments.Count)
                {
                    if (f.Signature.variableLength)
                    {
                        for (int i = f.Signature.Arguments.Count; i < variables.Count; i++)
                            args.Add(variables[i]);
                    }
                    else
                        throw new Exception("Invalid number of arguments in function");
                }
                return f.Exec(args);
            }
        }
        public static Dictionary<string, List<FunctionDefinition>> functionTable = new Dictionary<string, List<FunctionDefinition>>{
                {"key",new List<FunctionDefinition>
                { new FunctionDefinition{
                    Exec=,
                    Signature=new FunctionSignature{
                        Arguments=,
                        defaultArgs=0,
                        variableLength=false
                    }
                }
                }
                }
            };
    }
    static public class MetaStorage
    {
        static public List<Type> basicTypes = new List<Type>
        {
            new Type("Array"),
            new Type("Int"),
            new Type("Float"),
            new Type("Complex"),
            new Type("String")
        };
        static List<Type> customTypes = new List<Type>
        {
            new Class("Model",true)

        };
        static Dictionary<string,AbstractFunction> functionTable = new Dictionary<string,AbstractFunction>
        {
            {"print",new NativeFunction(){
                
            } },
            { },
            { },
            { },
            { },
        }
        static public void addNewClass(Class classType)
        {

        }
    }
    public class Type
    {
        public string TypeName { get; protected set; }
        public Type(string typeName)
        {
            this.TypeName = typeName;
        }
    }
    public class Class : Type
    {
        //methods;
        //constructors;
        //Dictionary<string,ObjectType> props;
        //static props
        //isStatic
        public bool isStatic;
        public Class(string typeName, bool isStatic) : base(typeName)
        {
            this.isStatic = isStatic;
            //if class is static all its methods and props should be static
        }
        public Dictionary<string, Type> Props { get; protected set; }
        public Dictionary<string, Type> StaticProps { get; protected set; }
        public Dictionary<string> Methods { get; protected set; }
        public Dictionary<string, AbstractFunction> StaticMethods { get; protected set; }
    }
    abstract class Object
    {
        protected bool isConst;
        protected Type type;
        public virtual Value getMember(string key)
        {
            throw new Exception("Property " + key + "doesn't available in object of class " + type.TypeName);
        }
        public Type getType()
        {
            return type;
        }
    }
    class CustomObject : Object
    {
        Dictionary<string, Object> props;
        Class classType;
        public override Value getMember(string key)
        {
            try
            {
                Object obj = props[key];
                if (isConst)
                    return new RValue(obj);
                else
                    return new LValueObject(this, key);
            }
            catch (Exception)
            {
                throw new Exception();
            }
        }
        public RValue getMemberValue(string key)
        {
            return new RValue(props[key]);
        }
        public void setMember(string key, Object obj)
        {
            //check types
        }
    }
    class Array : Object
    {
        List<Object> data;
        public Array()
        {
            this.type = MetaStorage.basicTypes[0];
        }
        public Value getElement(int index)
        {
            if (isConst)
                return new RValue(data[index]);
            return new LValueArray(this, index);
        }
        public void setElement(int index, Object value)
        {
            data[index] = value;
        }
        public RValue getElementValue(int index)
        {
            return new RValue(data[index]);
        }
    }
    class Float : Object
    {
        double val;
        public Float()
        {
            this.type = MetaStorage.basicTypes[2];
        }
    }

    class Int : Object
    {
        int val;
        public Int()
        {
            this.type = MetaStorage.basicTypes[1];
        }
    }

    class Complex : Object
    {
        Float re;
        Float im;
        public Complex()
        {
            this.type = MetaStorage.basicTypes[3];
        }
        public override Value getMember(string key)
        {
            if (key == "re")
            {
                return new RValue(re);
            }
            else if (key == "im")
            {
                return new RValue(im);
            }
            else return base.getMember(key);
        }
    }

    class String : Object
    {
        string val;
        public String()
        {
            this.type = MetaStorage.basicTypes[4];
        }
    }
    abstract class Value
    {
    }
    abstract class LValue : Value
    {
        public abstract void setValue(Object value);
        public abstract RValue toRValue();
    }
    class LValueObject : LValue
    {
        CustomObject parent;
        string key;
        public LValueObject(CustomObject obj, string key)
        {
            this.parent = obj;
            this.key = key;
        }
        public override void setValue(Object value)
        {
            parent.setMember(key, value);
        }
        public override RValue toRValue()
        {
            return parent.getMemberValue(key);
        }
    }
    class LValueArray : LValue
    {
        Array array;
        int index;
        public LValueArray(Array arr, int index)
        {
            this.array = arr;
            this.index = index;
        }
        public override void setValue(Object value)
        {
            array.setElement(index, value);
        }
        public override RValue toRValue()
        {
            return array.getElementValue(index);
        }
    }
    class RValue : Value
    {
        Object value;
        public RValue(Object value)
        {
            this.value = value;
        }
    }
}
