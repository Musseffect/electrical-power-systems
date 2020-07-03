using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ElectricalPowerSystems.Scheme.Interpreter
{
    public partial class Interpreter
    {
        public interface IType
        {
            Constant Validate(Constant constant);
        }
        public class FloatType : IType
        {
            Constant IType.Validate(Constant constant)
            {
                return Convert(constant, Constant.Type.Float);
            }
        }
        public class ComplexType : IType
        {
            Constant IType.Validate(Constant constant)
            {
                return Convert(constant, Constant.Type.Complex);
            }
        }
        public class IntType : IType
        {
            Constant IType.Validate(Constant constant)
            {
                return Convert(constant, Constant.Type.Int);
            }
        }
        public class CompelxType : IType
        {
            Constant IType.Validate(Constant constant)
            {
                return Convert(constant, Constant.Type.Complex);
            }
        }
        public class BoolType : IType
        {
            Constant IType.Validate(Constant constant)
            {
                return Convert(constant, Constant.Type.Bool);
            }
        }
        public class StringType : IType
        {
            Constant IType.Validate(Constant constant)
            {
                return Convert(constant, Constant.Type.String);
            }
        }
        public class ObjectType : IType
        {
            Dictionary<string, IType> memberTypes;
            public ObjectType(Dictionary<string, IType> memberTypes)
            {
                this.memberTypes = memberTypes;
            }
            Constant IType.Validate(Constant constant)
            {
                //return Convert(constant, Constant.Type.Object) as Object;
                Object obj = Convert(constant, Constant.Type.Object) as Object;
                Object result = new Object();
                result.Name = obj.Name;
                foreach (var value in obj.Values)
                {
                    if (memberTypes.ContainsKey(value.Key))
                    {
                        IType type = memberTypes[value.Key];
                        result.Values.Add(value.Key, type.Validate(value.Value));
                    }
                    else
                    {
                        throw new Exception($"Неизвестное поле \"{value.Key}\" в объекте \"{obj.Name}\"");
                    }
                }
                foreach (var member in memberTypes)
                {
                    if (!result.ContainsKey(member.Key))
                    {
                        throw new Exception($"Отсутствует поле \"{member}\" в объекте \"{obj.Name}\"");
                    }
                }
                return result;
            }
        }
        public class ArrayType : IType
        {
            IType arrayType;
            public ArrayType(IType arrayType)
            {
                this.arrayType = arrayType;
            }
            Constant IType.Validate(Constant constant)
            {
                Array array = Convert(constant, Constant.Type.Array) as Array;
                for (int i = 0; i < array.Values.Length; i++)
                {
                    array.Values[i] = arrayType.Validate(array.Values[i]);
                }
                return array;
            }
        }
    }
}
