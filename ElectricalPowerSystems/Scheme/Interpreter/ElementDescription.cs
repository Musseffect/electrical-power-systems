using System;
using System.Collections.Generic;

namespace ElectricalPowerSystems.Scheme.Interpreter
{
    public partial class Interpreter
    {
        public class ElementDescription
        {
            public enum NodeType
            {
                OnePhase,
                ThreePhase
            }
            public class ParameterTypeRecord
            {
                readonly Constant defaultValue;
                readonly IType type;
                public bool HasDefault { get { return defaultValue != null; } }
                public IType Type { get { return type; } }
                public Constant DefaultValue { get { return defaultValue; } }
                public ParameterTypeRecord(IType type, Constant defaultValue = null)
                {
                    this.type = type;
                    this.defaultValue = defaultValue;
                }
            }
            readonly Dictionary<string, NodeType> nodes;
            readonly Dictionary<string, ParameterTypeRecord> parameterTypes;
            ITransientElementModel transientModel;
            ISteadyStateElementModel steadyStateModel;
            public ElementDescription(Dictionary<string, NodeType> nodes, Dictionary<string, ParameterTypeRecord> parameterTypes,
                ITransientElementModel transientModel, ISteadyStateElementModel steadyStateModel)
            {
                this.nodes = nodes;
                this.parameterTypes = parameterTypes;
                this.transientModel = transientModel;
                this.steadyStateModel = steadyStateModel;
            }
            public Pin CreatePin(string name,int index)
            {
                var type = GetNodeType(name);
                switch (type)
                {
                    case ElementDescription.NodeType.OnePhase:
                        return new Pin1Phase(index);
                    case ElementDescription.NodeType.ThreePhase:
                        return new Pin3Phase(index);
                }
                throw new Exception("Неизвестный тип узла");
            }
            public ITransientElement CreateTransientElement(Object elementObject,
                Dictionary<string, Pin> elementNodes)
            {
                return transientModel?.CreateElement(elementObject, elementNodes);
            }
            public ISteadyStateElement CreateSteadyStateElement(Object elementObject,
                Dictionary<string, Pin> elementNodes)
            {
                return steadyStateModel?.CreateElement(elementObject, elementNodes);
            }
            public bool ContainsNode(string name)
            {
                return nodes.ContainsKey(name);
            }
            public NodeType GetNodeType(string name)
            {
                return nodes[name];
            }
            public Dictionary<string,NodeType>.KeyCollection GetNodes()
            {
                return nodes.Keys;
            }
            public int GetNodeCount()
            {
                return nodes.Count;
            }
            public bool Validate(ref Object elementObject, ref List<ErrorMessage> errors)
            {
                bool result = true;
                foreach (var parameter in parameterTypes)
                {
                    if (elementObject.ContainsKey(parameter.Key))
                    {
                        try
                        {
                            elementObject.SetValue(parameter.Key, parameter.Value.Type.Validate(elementObject.GetValue(parameter.Key)));
                        }
                        catch (TypeConversionError exc)
                        {
                            result = false;
                            errors.Add(new ErrorMessage($"Невохможно осуществить преобразование типов \"{exc.Src}\" в \"{exc.Dst}\" для параметра \"{parameter.Key}\" в элементе \"{elementObject.Name}\""));
                        }
                        catch (Exception exc)
                        {
                            result = false;
                            errors.Add(new ErrorMessage(exc.Message));
                        }
                    }
                    else if (parameter.Value.HasDefault)
                    {
                        try
                        {
                            elementObject.SetValue(parameter.Key, parameter.Value.Type.Validate(parameter.Value.DefaultValue));
                        }
                        catch (TypeConversionError exc)
                        {
                            result = false;
                            errors.Add(new ErrorMessage($"Невохможно осуществить преобразование типов \"{exc.Src}\" в \"{exc.Dst}\" для параметра \"{parameter.Key}\" в элементе \"{elementObject.Name}\""));
                        }
                        catch (Exception exc)
                        {
                            result = false;
                            errors.Add(new ErrorMessage(exc.Message));
                        }
                    } else
                    {
                        result = false;
                        errors.Add(new ErrorMessage($"Отсутствует параметр \"{parameter.Key}\" в элементе \"{elementObject.Name}\""));
                    }
                }
                return result;
            }
        }
    }
}
