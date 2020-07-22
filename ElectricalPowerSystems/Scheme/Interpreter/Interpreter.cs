﻿
using ElectricalPowerSystems.MathUtils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Antlr4.Runtime;
using ElectricalPowerSystems.Scheme.Grammar;
using ElectricalPowerSystems.Scheme.SteadyState;

namespace ElectricalPowerSystems.Scheme.Interpreter
{
    public partial class Interpreter
    {
        static Interpreter instance;
        Dictionary<string, ElementDescription> elementsMap;
        Dictionary<string, Constant> variableTable;
        Dictionary<string, Constant> GetParameters(List<KeyValueNode> parameters)
        {
            Dictionary<string, Constant> result = new Dictionary<string, Constant>() ;
            foreach (var parameter in parameters)
            {
                result.Add(parameter.Key,Eval(parameter.Value).GetRValue());
            }
            return result;
        }
        private Transient.TransientModel GetTransientModel(
            List<ElementNode> elements,
            List<ConnectionNode> rootConnections,
            Object modelParameters)
        {
            Dictionary<string, ElementEntry> elementEntries = new Dictionary<string, ElementEntry>();
            int nodeIndicies = 0;
            Transient.TransientModel model = new Transient.TransientModel();
            foreach (var element in elements)
            {
                //get Object from element.Definition
                Object obj = BuildObject(element.Definition);

                if (!elementsMap.ContainsKey(obj.Name))
                {
                    errors.Add(new ErrorMessage($"Не существует элемента {obj.Name}", element.Line, element.Position));
                    continue;
                }
                //get Element description( nodes and parameters)
                ElementDescription description = elementsMap[obj.Name];

                Dictionary<string, Pin> elementPins = new Dictionary<string, Pin>();
                var keys = description.GetNodes();
                for (int i = 0; i < keys.Count; i++)
                {
                    string key = keys.ElementAt(i);
                    Pin pin = description.CreatePin(key, nodeIndicies);
                    elementPins.Add(key, pin);
                    model.AddPin(pin);
                    nodeIndicies++;
                }
                //create element entry
                if (elementEntries.ContainsKey(element.Id))
                {
                    errors.Add(new ErrorMessage($"Повторное использование идентификатора {element.Id}", element.Line, element.Position));
                    continue;
                }
                elementEntries.Add(element.Id, new ElementEntry(description, elementPins));
                bool valid = description.Validate(ref obj, ref errors);
                if (valid)
                {
                    try
                    {
                        if (description.Validate(ref obj, ref errors))
                        {
                            ITransientElement modelElement = description.CreateTransientElement(obj, elementPins);
                            if (modelElement is null)
                            {
                                errors.Add(new ErrorMessage($"Элемент {obj.Name} не может использоваться для расчёта переходных процессов", element.Line, element.Position));
                            }
                            else
                            {
                                model.AddElement(modelElement);
                            }
                        }
                    }
                    catch (MissingValueException exc)
                    {
                        errors.Add(new ErrorMessage($"Отсутствует параметр {exc.Key} в элементе {obj.Name}", element.Line, element.Position));
                    }
                    catch (Exception)
                    {
                        errors.Add(new ErrorMessage($"Ошибка при создании элемента {obj.Name}", element.Line, element.Position));
                    }
                }
            }
            //resolve connections between elements
            List<Connection> connections = new List<Connection>();
            foreach (var connection in rootConnections)
            {
                try
                {
                    if (!elementEntries.ContainsKey(connection.Element1))
                    {
                        errors.Add(new ErrorMessage($"Неуществующий элемент \"{connection.Element1}\" в соединении", connection.Line, connection.Position));
                        continue;
                    }
                    if (!elementEntries.ContainsKey(connection.Element2))
                    {
                        errors.Add(new ErrorMessage($"Неуществующий элемент \"{connection.Element2}\" в соединении", connection.Line, connection.Position));
                        continue;
                    }
                    ElementEntry entry1 = elementEntries[connection.Element1];
                    ElementEntry entry2 = elementEntries[connection.Element2];
                    ElementDescription element1 = entry1.GetDescription();
                    ElementDescription element2 = entry2.GetDescription();
                    if (!element1.ContainsNode(connection.Node1))
                    {
                        errors.Add(new ErrorMessage($"Несуществующий узел \"{connection.Element1}.{connection.Node1}\" в соединении", connection.Line, connection.Position));
                        continue;
                    }
                    if (!element2.ContainsNode(connection.Node2))
                    {
                        errors.Add(new ErrorMessage($"Несуществующий узел \"{connection.Element2}.{connection.Node2}\" в соединении", connection.Line, connection.Position));
                        continue;
                    }
                    ElementDescription.NodeType node1 = element1.GetNodeType(connection.Node1);
                    ElementDescription.NodeType node2 = element2.GetNodeType(connection.Node2);
                    if (node1 != node2)
                    {
                        errors.Add(new ErrorMessage($"Узлы в соединении \"connect({connection.Element1}.{connection.Node1},{connection.Element2}.{connection.Node2})\" имеют различные типы", connection.Line, connection.Position));
                        continue;
                    }
                    connections.Add(new Connection(entry1.GetPin(connection.Node1), entry2.GetPin(connection.Node2)));
                }
                catch (Exception exc)
                {
                    throw exc;
                }
            }
            foreach (Connection connection in connections)
            {
                model.AddElement(connection.CreateTransientElement());
            }
            try
            {
                Object solver = (Object)Convert(modelParameters.GetValue("solver"), Constant.Type.Object);
                switch (solver.Name)
                {
                    case "radauIIA5":
                        {
                            FloatValue fAbsTol = Convert(solver.GetValue("fAbsTol"), Constant.Type.Float) as FloatValue;
                            IntValue iterations = Convert(solver.GetValue("iterations"), Constant.Type.Int) as IntValue;
                            FloatValue alpha = Convert(solver.GetValue("alpha"), Constant.Type.Float) as FloatValue;
                            FloatValue step = Convert(solver.GetValue("step"), Constant.Type.Float) as FloatValue;
                            model.SetSolver(new Equations.DAE.Implicit.RADAUIIA5(fAbsTol.Value, iterations.Value, alpha.Value, step.Value));
                        }
                        break;
                    case "radauIIA3":
                        {
                            FloatValue fAbsTol = Convert(solver.GetValue("fAbsTol"), Constant.Type.Float) as FloatValue;
                            IntValue iterations = Convert(solver.GetValue("iterations"), Constant.Type.Int) as IntValue;
                            FloatValue alpha = Convert(solver.GetValue("alpha"), Constant.Type.Float) as FloatValue;
                            FloatValue step = Convert(solver.GetValue("step"), Constant.Type.Float) as FloatValue;
                            model.SetSolver(new Equations.DAE.Implicit.RADAUIIA3(fAbsTol.Value, iterations.Value, alpha.Value, step.Value));
                        }
                        break;
                    case "bdf1":
                        { 
                            FloatValue fAbsTol = Convert(solver.GetValue("fAbsTol"), Constant.Type.Float) as FloatValue;
                            IntValue iterations = Convert(solver.GetValue("iterations"), Constant.Type.Int) as IntValue;
                            FloatValue alpha = Convert(solver.GetValue("alpha"), Constant.Type.Float) as FloatValue;
                            FloatValue step = Convert(solver.GetValue("step"), Constant.Type.Float) as FloatValue;
                            model.SetSolver(new Equations.DAE.Implicit.BDF1(fAbsTol.Value, iterations.Value, alpha.Value, step.Value));
                        }
                        break;
                    case "bdf2":
                        {
                            FloatValue fAbsTol = Convert(solver.GetValue("fAbsTol"), Constant.Type.Float) as FloatValue;
                            IntValue iterations = Convert(solver.GetValue("iterations"), Constant.Type.Int) as IntValue;
                            FloatValue alpha = Convert(solver.GetValue("alpha"), Constant.Type.Float) as FloatValue;
                            FloatValue step = Convert(solver.GetValue("step"), Constant.Type.Float) as FloatValue;
                            model.SetSolver(new Equations.DAE.Implicit.BDF2(fAbsTol.Value, iterations.Value, alpha.Value, step.Value));
                            break;
                        }
                    default:
                        errors.Add(new ErrorMessage("Неизвестный решатель для данного режима"));
                        return null;
                }
                FloatValue t0 = Convert(modelParameters.GetValue("t0"), Constant.Type.Float) as FloatValue;
                FloatValue t1 = Convert(modelParameters.GetValue("t1"), Constant.Type.Float) as FloatValue;

                FloatValue baseFrequency = (FloatValue)Convert(modelParameters.GetValue("baseFrequency"), Constant.Type.Float);
                model.SetBaseFrequency(baseFrequency.Value);
                /*if (modelParameters.ContainsKey("exportTo"))
                {
                    StringValue fileName = Convert(modelParameters.GetValue("exportTo"), Constant.Type.String) as StringValue;
                    model.setExportTo(fileName);
                }*/
                if (modelParameters.ContainsKey("useAdaptiveStep"))
                {
                    BoolValue useAdaptiveStep = Convert(modelParameters.GetValue("useAdaptiveStep"), Constant.Type.Bool) as BoolValue;
                    model.SetIsAdaptive(useAdaptiveStep.Value);
                    if (useAdaptiveStep.Value)
                    {
                        FloatValue minStep = Convert(modelParameters.GetValue("minStep"), Constant.Type.Float) as FloatValue;
                        model.SetMinStep(minStep.Value);
                    }
                }
                /*if (modelParameters.ContainsKey("epsilon"))
                {
                    FloatValue epsilon = Convert(modelParameters.GetValue("epsilon"), Constant.Type.Float) as FloatValue;
                    model.setEpsilon(epsilon);
                }*/
                model.SetT0(t0.Value);
                model.SetT1(t1.Value);
            }
            catch (MissingValueException exc)
            {
                errors.Add(new ErrorMessage($"Отсутствует параметр \"{exc.Key}\" в определении модели."));
                return null;
            }
            catch (Exception exc)
            {
                errors.Add(new ErrorMessage($"Необработанное исключение: {exc.Message}"));
                return null;
            }
            return model;
        }
        private SteadyStateModel GetSteadyStateModel(
            List<ElementNode> elements,
            List<ConnectionNode> rootConnections,
            Object modelParameters)
        {
            Dictionary<string, ElementEntry> elementEntries = new Dictionary<string, ElementEntry>();
            int nodeIndicies = 0;
            SteadyStateModel model = new SteadyStateModel();
            foreach (var element in elements)
            {
                //get Object from element.Definition
                Object obj = BuildObject(element.Definition);

                //get Element description( nodes and parameters)
                if (!elementsMap.ContainsKey(obj.Name))
                {
                    errors.Add(new ErrorMessage($"Не существует элемента {obj.Name}",element.Line,element.Position));
                    continue;
                }
                ElementDescription description = elementsMap[obj.Name];
                
                Dictionary<string, Pin> elementPins = new Dictionary<string, Pin>();
                var keys = description.GetNodes();
                for (int i = 0; i < keys.Count; i++)
                {
                    string key = keys.ElementAt(i);
                    Pin pin = description.CreatePin(key, nodeIndicies);
                    elementPins.Add(key, pin);
                    model.AddPin(pin);
                    nodeIndicies++;
                }
                //create element entry
                if (elementEntries.ContainsKey(element.Id))
                {
                    errors.Add(new ErrorMessage($"Повторное использование идентификатора {element.Id}", element.Line, element.Position));
                    continue;
                }
                elementEntries.Add(element.Id, new ElementEntry(description, elementPins));
                bool valid = description.Validate(ref obj, ref errors);
                if (valid)
                {
                    try
                    {
                        if (description.Validate(ref obj,ref errors))
                        {
                            ISteadyStateElement modelElement = description.CreateSteadyStateElement(obj, elementPins);
                            if (modelElement is null)
                            {
                                errors.Add(new ErrorMessage($"Элемент \"{obj.Name}\" не может использоваться для установившегося режима", element.Line, element.Position));
                            }
                            else
                            {
                                model.AddElement(modelElement);
                            }
                        }
                    }
                    catch (MissingValueException exc)
                    {
                        errors.Add(new ErrorMessage($"Отсутствует необходимый параметр \"{exc.Key}\" в элементе \"{obj.Name}\"", element.Line, element.Position));
                    }
                    catch (Exception exc)
                    {
                        errors.Add(new ErrorMessage($"Ошибка при создании элемента {obj.Name}: {exc.Message}", element.Line, element.Position));
                    }
                }
            }
            //resolve connections between elements
            List<Connection> connections = new List<Connection>();
            foreach (var connection in rootConnections)
            {
                try
                {
                    if (!elementEntries.ContainsKey(connection.Element1))
                    {
                        errors.Add(new ErrorMessage($"Неуществующий элемент \"{connection.Element1}\" в соединении", connection.Line, connection.Position));
                        continue;
                    }
                    if (!elementEntries.ContainsKey(connection.Element2))
                    {
                        errors.Add(new ErrorMessage($"Неуществующий элемент \"{connection.Element2}\" в соединении", connection.Line, connection.Position));
                        continue;
                    }
                    ElementEntry entry1 = elementEntries[connection.Element1];
                    ElementEntry entry2 = elementEntries[connection.Element2];
                    ElementDescription element1 = entry1.GetDescription();
                    ElementDescription element2 = entry2.GetDescription();
                    if (!element1.ContainsNode(connection.Node1))
                    {
                        errors.Add(new ErrorMessage($"Несуществующий узел \"{connection.Element1}.{connection.Node1}\" в соединении", connection.Line, connection.Position));
                        continue;
                    }
                    if (!element2.ContainsNode(connection.Node2))
                    {
                        errors.Add(new ErrorMessage($"Несуществующий узел \"{connection.Element2}.{connection.Node2}\" в соединении", connection.Line, connection.Position));
                        continue;
                    }
                    ElementDescription.NodeType node1 = element1.GetNodeType(connection.Node1);
                    ElementDescription.NodeType node2 = element2.GetNodeType(connection.Node2);
                    if (node1 != node2)
                    {
                        errors.Add(new ErrorMessage($"Узлы в соединении \"connect({connection.Element1}.{connection.Node1},{connection.Element2}.{connection.Node2})\" имеют различные типы", connection.Line, connection.Position));
                        continue;
                    }
                    connections.Add(new Connection(entry1.GetPin(connection.Node1), entry2.GetPin(connection.Node2)));
                }
                catch (Exception exc)
                {
                    throw exc;
                }
            }
            foreach (Connection connection in connections)
            {
                model.AddElement(connection.CreateSteadyStateElement());
            }
            try
            {
                Object solver = (Object)Convert(modelParameters.GetValue("solver"), Constant.Type.Object);
                switch (solver.Name)
                {
                    case "newton":
                        FloatValue fAbsTol = Convert(solver.GetValue("fAbsTol"), Constant.Type.Float) as FloatValue;
                        IntValue iterations = Convert(solver.GetValue("iterations"), Constant.Type.Int) as IntValue;
                        FloatValue alpha = Convert(solver.GetValue("alpha"), Constant.Type.Float) as FloatValue;
                        model.SetSolver(new SteadyStateNewtonSolver(fAbsTol.Value, iterations.Value, alpha.Value));
                        break;
                    default:
                        errors.Add(new ErrorMessage("Неизвестный решатель для данного режима"));
                        return null;
                }
                FloatValue baseFrequency = (FloatValue)Convert(modelParameters.GetValue("baseFrequency"), Constant.Type.Float);
                model.SetBaseFrequency(baseFrequency.Value);
            }
            catch (MissingValueException exc)
            {
                errors.Add(new ErrorMessage($"Отсутствует параметр \"{exc.Key}\" в определении модели."));
                return null;
            }
            catch (Exception exc)
            {
                errors.Add(new ErrorMessage($"Exception: {exc.Message}"));
                return null;
            }
            return model;
        }
        List<ErrorMessage> errors;
        List<string> output;
        private void InitElements()
        {
            variableTable = new Dictionary<string, Constant>();
            variableTable.Add("Dd0",new IntValue(0));
            variableTable.Add("Dd2", new IntValue(1));
            variableTable.Add("Dd4", new IntValue(2));
            variableTable.Add("Dd6", new IntValue(3));
            variableTable.Add("Dd8", new IntValue(4));
            variableTable.Add("Dd10", new IntValue(5));

            variableTable.Add("Yy0", new IntValue(0));
            variableTable.Add("Yy2", new IntValue(1));
            variableTable.Add("Yy4", new IntValue(2));
            variableTable.Add("Yy6", new IntValue(3));
            variableTable.Add("Yy8", new IntValue(4));
            variableTable.Add("Yy10", new IntValue(5));

            variableTable.Add("Yd1", new IntValue(0));
            variableTable.Add("Yd3", new IntValue(1));
            variableTable.Add("Yd5", new IntValue(2));
            variableTable.Add("Yd7", new IntValue(3));
            variableTable.Add("Yd9", new IntValue(4));
            variableTable.Add("Yd11", new IntValue(5));

            variableTable.Add("Dy1", new IntValue(0));
            variableTable.Add("Dy3", new IntValue(1));
            variableTable.Add("Dy5", new IntValue(2));
            variableTable.Add("Dy7", new IntValue(3));
            variableTable.Add("Dy9", new IntValue(4));
            variableTable.Add("Dy11", new IntValue(5));

            variableTable.Add("PI",new FloatValue(Math.PI));
            variableTable.Add("E", new FloatValue(Math.E));

            elementsMap = new Dictionary<string, ElementDescription>();
            elementsMap.Add("Scope1p", new ElementDescription(
                new Dictionary<string, ElementDescription.NodeType>
                {
                    { "in", ElementDescription.NodeType.OnePhase},
                    { "out", ElementDescription.NodeType.OnePhase}
                },
                new Dictionary<string, ElementDescription.ParameterTypeRecord>()
                {
                    { "Label", new ElementDescription.ParameterTypeRecord(new StringType()) },
                    { "ShowCurrent", new ElementDescription.ParameterTypeRecord(new BoolType(), new BoolValue(true)) },
                    { "ShowVoltage", new ElementDescription.ParameterTypeRecord(new BoolType(), new BoolValue(true)) },
                    { "ShowPower", new ElementDescription.ParameterTypeRecord(new BoolType(), new BoolValue(true)) }
                }, new Elements.TransienteScope1PModel(), new Elements.SteadyStateScope1PModel())
                );
            elementsMap.Add("Resistor", new ElementDescription(
                new Dictionary<string, ElementDescription.NodeType>{
                    { "in", ElementDescription.NodeType.OnePhase},
                    { "out", ElementDescription.NodeType.OnePhase}
                },
                new Dictionary<string, ElementDescription.ParameterTypeRecord>{
                        { "R", new ElementDescription.ParameterTypeRecord(new FloatType())}
                }, new Elements.TransientResistorModel(), new Elements.SteadyStateResistorModel())
            );
            elementsMap.Add("Ground", new ElementDescription(
                new Dictionary<string, ElementDescription.NodeType>{
                    { "in", ElementDescription.NodeType.OnePhase}
                },
                new Dictionary<string, ElementDescription.ParameterTypeRecord>()
                , new Elements.TransientGroundModel(), new Elements.SteadyStateGroundModel())
            );
            elementsMap.Add("Capacitor", new ElementDescription(
                new Dictionary<string, ElementDescription.NodeType>{
                    { "in", ElementDescription.NodeType.OnePhase},
                    { "out", ElementDescription.NodeType.OnePhase}
                },
                new Dictionary<string, ElementDescription.ParameterTypeRecord>{
                        { "C", new ElementDescription.ParameterTypeRecord(new FloatType())}
                }, new Elements.TransientCapacitorModel(), new Elements.SteadyStateCapacitorModel())
            );
            elementsMap.Add("Inductor", new ElementDescription(
                new Dictionary<string, ElementDescription.NodeType>{
                    { "in", ElementDescription.NodeType.OnePhase},
                    { "out", ElementDescription.NodeType.OnePhase}
                },
                new Dictionary<string, ElementDescription.ParameterTypeRecord>{
                        { "L", new ElementDescription.ParameterTypeRecord(new FloatType())}
                }, new Elements.TransientInductorModel(), new Elements.SteadyStateInductorModel())
            );
            elementsMap.Add("Break1p", new ElementDescription(
                new Dictionary<string, ElementDescription.NodeType>{
                    { "in", ElementDescription.NodeType.OnePhase},
                    { "out", ElementDescription.NodeType.OnePhase}
                },
                new Dictionary<string, ElementDescription.ParameterTypeRecord>{
                        { "InitialState", new ElementDescription.ParameterTypeRecord(new BoolType())},
                        { "SwitchTime", new ElementDescription.ParameterTypeRecord(new FloatType())}
                }, new Elements.TransientBreak1PModel(), null)
            );
            elementsMap.Add("Switch2Way1p", new ElementDescription(
                new Dictionary<string, ElementDescription.NodeType>{
                    { "in", ElementDescription.NodeType.OnePhase},
                    { "out1", ElementDescription.NodeType.OnePhase},
                    { "out2", ElementDescription.NodeType.OnePhase}
                },
                new Dictionary<string, ElementDescription.ParameterTypeRecord>{
                        { "State", new ElementDescription.ParameterTypeRecord(new FloatType())}
                }, new Elements.TransientSwitch2Way1PModel(), new Elements.SteadyStateSwitch2Way1PModel())
            );
            elementsMap.Add("Transformer", new ElementDescription(
                new Dictionary<string, ElementDescription.NodeType>
                {
                    { "in_p", ElementDescription.NodeType.OnePhase},
                    { "out_p", ElementDescription.NodeType.OnePhase},
                    { "in_s", ElementDescription.NodeType.OnePhase},
                    { "out_s", ElementDescription.NodeType.OnePhase}
                },
                new Dictionary<string, ElementDescription.ParameterTypeRecord>()
                {
                    { "K", new ElementDescription.ParameterTypeRecord(new FloatType()) }
                }, new Elements.TransientTransformerModel(), new Elements.SteadyStateTransformerModel())
            );
            elementsMap.Add("RealTransformer", new ElementDescription(
                new Dictionary<string, ElementDescription.NodeType>
                {
                    { "in_p", ElementDescription.NodeType.OnePhase},
                    { "out_p", ElementDescription.NodeType.OnePhase},
                    { "in_s", ElementDescription.NodeType.OnePhase},
                    { "out_s", ElementDescription.NodeType.OnePhase}
                },
                new Dictionary<string, ElementDescription.ParameterTypeRecord>()
                {
                    { "Zp", new ElementDescription.ParameterTypeRecord(new ComplexType()) },
                    { "Zs", new ElementDescription.ParameterTypeRecord(new ComplexType()) },
                    { "Xm", new ElementDescription.ParameterTypeRecord(new FloatType()) },
                    { "Rc", new ElementDescription.ParameterTypeRecord(new FloatType()) },
                    { "K", new ElementDescription.ParameterTypeRecord(new FloatType()) }
                }, new Elements.TransientRealTransformerModel(), new Elements.SteadyStateRealTransformerModel())
            );
            elementsMap.Add("VoltageSource", new ElementDescription(
                new Dictionary<string, ElementDescription.NodeType>{
                    { "in", ElementDescription.NodeType.OnePhase },
                    { "out", ElementDescription.NodeType.OnePhase}
                },
                new Dictionary<string, ElementDescription.ParameterTypeRecord>{
                    { "Peak", new ElementDescription.ParameterTypeRecord(new FloatType())},
                    { "Phase", new ElementDescription.ParameterTypeRecord(new FloatType())},
                    { "Frequency", new ElementDescription.ParameterTypeRecord(new FloatType())}
                }, new Elements.TransientVoltageSourceModel(), new Elements.SteadyStateVoltageSourceModel())
            );
            elementsMap.Add("TwoPort", new ElementDescription(
                new Dictionary<string, ElementDescription.NodeType>{
                    { "in1", ElementDescription.NodeType.OnePhase },
                    { "in2", ElementDescription.NodeType.OnePhase },
                    { "out1", ElementDescription.NodeType.OnePhase},
                    { "out2", ElementDescription.NodeType.OnePhase}
                },
                new Dictionary<string, ElementDescription.ParameterTypeRecord>{
                    { "b11", new ElementDescription.ParameterTypeRecord(new FloatType())},
                    { "b12", new ElementDescription.ParameterTypeRecord(new FloatType())},
                    { "b21", new ElementDescription.ParameterTypeRecord(new FloatType())},
                    { "b22", new ElementDescription.ParameterTypeRecord(new FloatType())}
                }, new Elements.TransientTwoPortModel(), new Elements.SteadyStateTwoPortModel())
            );
            elementsMap.Add("Scope3p", new ElementDescription(
                new Dictionary<string, ElementDescription.NodeType>
                {
                    { "in", ElementDescription.NodeType.ThreePhase},
                    { "out", ElementDescription.NodeType.ThreePhase}
                },
                new Dictionary<string, ElementDescription.ParameterTypeRecord>()
                {
                    { "Label", new ElementDescription.ParameterTypeRecord(new StringType()) },
                    { "ShowCurrent", new ElementDescription.ParameterTypeRecord(new BoolType(), new BoolValue(true)) },
                    { "ShowVoltage", new ElementDescription.ParameterTypeRecord(new BoolType(), new BoolValue(true)) },
                    { "ShowPower", new ElementDescription.ParameterTypeRecord(new BoolType(), new BoolValue(true)) }
                }, new Elements.TransienteScope3PModel(), new Elements.SteadyStateScope3PModel())
            );
            elementsMap.Add("TransformerDd", new ElementDescription(
                new Dictionary<string, ElementDescription.NodeType>
                {
                    { "in", ElementDescription.NodeType.ThreePhase},
                    { "out", ElementDescription.NodeType.ThreePhase}
                },
                new Dictionary<string, ElementDescription.ParameterTypeRecord>()
                {
                    { "Zp", new ElementDescription.ParameterTypeRecord(new ComplexType()) },
                    { "Zs", new ElementDescription.ParameterTypeRecord(new ComplexType()) },
                    { "Xm", new ElementDescription.ParameterTypeRecord(new FloatType()) },
                    { "Rc", new ElementDescription.ParameterTypeRecord(new FloatType()) },
                    { "K", new ElementDescription.ParameterTypeRecord(new FloatType()) },
                    { "Group", new ElementDescription.ParameterTypeRecord(new IntType()) }
                }, new Elements.TransientTransformDdModel(), new Elements.SteadyStateTransformerDdModel())
            );
            elementsMap.Add("TransformerYd", new ElementDescription(
                new Dictionary<string, ElementDescription.NodeType>
                {
                    { "in", ElementDescription.NodeType.ThreePhase},
                    { "in_n", ElementDescription.NodeType.OnePhase},
                    { "out", ElementDescription.NodeType.ThreePhase}
                },
                new Dictionary<string, ElementDescription.ParameterTypeRecord>()
                {
                    { "Zp", new ElementDescription.ParameterTypeRecord(new ComplexType()) },
                    { "Zs", new ElementDescription.ParameterTypeRecord(new ComplexType()) },
                    { "Xm", new ElementDescription.ParameterTypeRecord(new FloatType()) },
                    { "Rc", new ElementDescription.ParameterTypeRecord(new FloatType()) },
                    { "K", new ElementDescription.ParameterTypeRecord(new FloatType()) },
                    { "Group", new ElementDescription.ParameterTypeRecord(new IntType()) }
                }, new Elements.TransientTransformYdModel(), new Elements.SteadyStateTransformerYdModel())
            );
            elementsMap.Add("TransformerDy", new ElementDescription(
                new Dictionary<string, ElementDescription.NodeType>
                {
                    { "in", ElementDescription.NodeType.ThreePhase},
                    { "out", ElementDescription.NodeType.ThreePhase},
                    { "out_n", ElementDescription.NodeType.OnePhase}
                },
                new Dictionary<string, ElementDescription.ParameterTypeRecord>()
                {
                    { "Zp", new ElementDescription.ParameterTypeRecord(new ComplexType()) },
                    { "Zs", new ElementDescription.ParameterTypeRecord(new ComplexType()) },
                    { "Xm", new ElementDescription.ParameterTypeRecord(new FloatType()) },
                    { "Rc", new ElementDescription.ParameterTypeRecord(new FloatType()) },
                    { "K", new ElementDescription.ParameterTypeRecord(new FloatType()) },
                    { "Group", new ElementDescription.ParameterTypeRecord(new IntType()) }
                }, new Elements.TransientTransformDyModel(), new Elements.SteadyStateTransformerDyModel())
            );
            elementsMap.Add("TransformerYy", new ElementDescription(
                new Dictionary<string, ElementDescription.NodeType>
                {
                    { "in", ElementDescription.NodeType.ThreePhase},
                    { "in_n", ElementDescription.NodeType.OnePhase},
                    { "out", ElementDescription.NodeType.ThreePhase},
                    { "out_n", ElementDescription.NodeType.OnePhase}
                },
                new Dictionary<string, ElementDescription.ParameterTypeRecord>()
                {
                    { "Zp", new ElementDescription.ParameterTypeRecord(new ComplexType()) },
                    { "Zs", new ElementDescription.ParameterTypeRecord(new ComplexType()) },
                    { "Xm", new ElementDescription.ParameterTypeRecord(new FloatType()) },
                    { "Rc", new ElementDescription.ParameterTypeRecord(new FloatType()) },
                    { "K", new ElementDescription.ParameterTypeRecord(new FloatType()) },
                    { "Group", new ElementDescription.ParameterTypeRecord(new IntType()) }
                }, new Elements.TransientTransformYyModel(), new Elements.SteadyStateTransformerYyModel())
            );
            elementsMap.Add("LoadY", new ElementDescription(
                new Dictionary<string, ElementDescription.NodeType>
                {
                    { "in", ElementDescription.NodeType.ThreePhase},
                    { "n", ElementDescription.NodeType.OnePhase}
                },
                new Dictionary<string, ElementDescription.ParameterTypeRecord> ()
                {
                    { "ZA", new ElementDescription.ParameterTypeRecord(new ComplexType()) },
                    { "ZB", new ElementDescription.ParameterTypeRecord(new ComplexType()) },
                    { "ZC", new ElementDescription.ParameterTypeRecord(new ComplexType()) }
                }, new Elements.TransientLoadYModel(), new Elements.SteadyStateLoadYModel())
            );
            elementsMap.Add("LoadD", new ElementDescription(
                new Dictionary<string, ElementDescription.NodeType>
                {
                    { "in", ElementDescription.NodeType.ThreePhase}
                },
                new Dictionary<string, ElementDescription.ParameterTypeRecord>()
                {
                    { "ZAB", new ElementDescription.ParameterTypeRecord(new ComplexType()) },
                    { "ZBC", new ElementDescription.ParameterTypeRecord(new ComplexType()) },
                    { "ZCA", new ElementDescription.ParameterTypeRecord(new ComplexType()) }
                }, new Elements.TransientLoadDModel(), new Elements.SteadyStateLoadDModel())
            );
            elementsMap.Add("GeneratorY", new ElementDescription(
                new Dictionary<string, ElementDescription.NodeType>
                {
                    { "out", ElementDescription.NodeType.ThreePhase},
                    { "n", ElementDescription.NodeType.OnePhase}
                },
                new Dictionary<string, ElementDescription.ParameterTypeRecord>()
                {
                    { "Peak", new ElementDescription.ParameterTypeRecord(new FloatType()) },
                    { "Phase", new ElementDescription.ParameterTypeRecord(new FloatType()) },
                    { "Frequency", new ElementDescription.ParameterTypeRecord(new FloatType()) },
                    { "Z", new ElementDescription.ParameterTypeRecord(new ComplexType()) }
                }, new Elements.TransientGeneratorYModel(), new Elements.SteadyStateGeneratorYModel())
            );
            elementsMap.Add("GeneratorD", new ElementDescription(
                new Dictionary<string, ElementDescription.NodeType>
                {
                    { "out", ElementDescription.NodeType.ThreePhase}
                },
                new Dictionary<string, ElementDescription.ParameterTypeRecord>()
                {
                    { "Peak", new ElementDescription.ParameterTypeRecord(new FloatType()) },
                    { "Phase", new ElementDescription.ParameterTypeRecord(new FloatType()) },
                    { "Frequency", new ElementDescription.ParameterTypeRecord(new FloatType()) },
                    { "Z", new ElementDescription.ParameterTypeRecord(new ComplexType()) }
                }, new Elements.TransientGeneratorDModel(), new Elements.SteadyStateGeneratorDModel())
            );
            elementsMap.Add("Switch3P", new ElementDescription(
                new Dictionary<string, ElementDescription.NodeType>
                {
                    { "in", ElementDescription.NodeType.ThreePhase},
                    { "out", ElementDescription.NodeType.ThreePhase}
                },
                new Dictionary<string, ElementDescription.ParameterTypeRecord>()
                {
                    { "State", new ElementDescription.ParameterTypeRecord(new BoolType()) }
                }, new Elements.TransientSwitch3PModel(), new Elements.SteadyStateSwitch3PModel())
            );
            elementsMap.Add("Connection3p1p", new ElementDescription(
                new Dictionary<string, ElementDescription.NodeType>
                {
                    { "abc", ElementDescription.NodeType.ThreePhase},
                    { "a", ElementDescription.NodeType.OnePhase},
                    { "b", ElementDescription.NodeType.OnePhase},
                    { "c", ElementDescription.NodeType.OnePhase}
                },
                new Dictionary<string, ElementDescription.ParameterTypeRecord>()
                {
                }, new Elements.TransienteConnection3P1PModel(), new Elements.SteadyStateSwitch3PModel())
            );
            elementsMap.Add("LinePiSection", new ElementDescription(
                new Dictionary<string, ElementDescription.NodeType>
                {
                    { "in", ElementDescription.NodeType.ThreePhase},
                    { "out", ElementDescription.NodeType.ThreePhase}
                },
                new Dictionary<string, ElementDescription.ParameterTypeRecord>()
                {
                    { "R", new ElementDescription.ParameterTypeRecord(new FloatType()) },
                    { "L", new ElementDescription.ParameterTypeRecord(new FloatType()) },
                    { "B", new ElementDescription.ParameterTypeRecord(new FloatType()) },
                    { "G", new ElementDescription.ParameterTypeRecord(new FloatType()) },
                    { "Bp", new ElementDescription.ParameterTypeRecord(new FloatType()) }
                }, new Elements.TransientLinePIModel(), new Elements.SteadyStateLinePiModel())
            );
            elementsMap.Add("LineRL", new ElementDescription(
                new Dictionary<string, ElementDescription.NodeType>
                {
                    { "in", ElementDescription.NodeType.ThreePhase},
                    { "out", ElementDescription.NodeType.ThreePhase}
                },
                new Dictionary<string, ElementDescription.ParameterTypeRecord>()
                {
                    { "R", new ElementDescription.ParameterTypeRecord(new FloatType()) },
                    { "L", new ElementDescription.ParameterTypeRecord(new FloatType()) }
                }, new Elements.TransientLineRLModel(), new Elements.SteadyStateLineRLModel())
            );
            elementsMap.Add("RecloserNative", new ElementDescription(
                new Dictionary<string, ElementDescription.NodeType>
                {
                    { "in", ElementDescription.NodeType.ThreePhase},
                    { "out", ElementDescription.NodeType.ThreePhase}
                },
                new Dictionary<string, ElementDescription.ParameterTypeRecord>()
                {
                    { "Frequency", new ElementDescription.ParameterTypeRecord(new FloatType()) },
                    { "InitialState", new ElementDescription.ParameterTypeRecord(new BoolType()) },
                    { "T0", new ElementDescription.ParameterTypeRecord(new FloatType()) },
                    { "CurrentPeakMax", new ElementDescription.ParameterTypeRecord(new FloatType()) },
                    { "PowerPeakMax", new ElementDescription.ParameterTypeRecord(new FloatType()) },
                    { "Tries", new ElementDescription.ParameterTypeRecord(new IntType()) },
                    { "WaitTime", new ElementDescription.ParameterTypeRecord(new FloatType()) },
                    { "RestoreTriesTime", new ElementDescription.ParameterTypeRecord(new FloatType()) }
                }, new Elements.TransientRecloserNative3PModel(), null)
            );
        }
        //Done
        private void ResolveStatements(List<ExpressionNode> statements)
        {
            foreach (var expression in statements)
            {
                Eval(expression);
            }
        }
        //Done
        public IModel Generate(Grammar.ModelNode root, ref List<ErrorMessage> errorList, ref List<string> output)
        {
            this.errors = errorList;
            this.output = output;
            ResolveStatements(root.Statements);
            Object modelParameters = BuildObject(root.ModelParameters);
            switch (modelParameters.Name)
            {
                case "steadystate":
                    return GetSteadyStateModel(root.Elements, root.Connections, modelParameters);
                case "transient":
                    return GetTransientModel(root.Elements, root.Connections, modelParameters);
                default:
                    errorList.Add(new ErrorMessage("Некорректный режим", root.Line, root.Position));
                    return null;
            }
        }
        //Done
        public void AddOutput(string line)
        {
            output.Add(line);
        }
        private Interpreter()
        {
            InitElements();
            InitFunctionDictionary();
        }
        //Done
        static public Interpreter GetInstanse()
        {
            if (instance == null)
                instance = new Interpreter();
            return instance;
        }

        static public void RunModel(string inputText, ref List<ErrorMessage> errorList, ref List<string> output)
        {
            AntlrInputStream inputStream = new AntlrInputStream(inputText);
            Grammar.ModelGrammarLexer modelLexer = new Grammar.ModelGrammarLexer(inputStream);
            ErrorListener<int> lexerErrorListener = new ErrorListener<int>();
            modelLexer.RemoveErrorListeners();
            modelLexer.AddErrorListener(lexerErrorListener);
            CommonTokenStream commonTokenStream = new CommonTokenStream(modelLexer);
            Grammar.ModelGrammarParser modelParser = new Grammar.ModelGrammarParser(commonTokenStream);
            ErrorListener<IToken> parserErrorListener = new ErrorListener<IToken>();
            modelParser.RemoveErrorListeners();
            modelParser.AddErrorListener(parserErrorListener);

            Grammar.ModelGrammarParser.ModelContext modelContext = modelParser.model();
            errorList.AddRange(lexerErrorListener.GetErrors());
            errorList.AddRange(parserErrorListener.GetErrors());
            if (errorList.Count > 0)
                return;
            Grammar.Visitor visitor = new Grammar.Visitor();
            Grammar.Node root = visitor.VisitModel(modelContext);
            var model = Interpreter.GetInstanse().Generate((Grammar.ModelNode)root, ref errorList, ref output);
            if (errorList.Count > 0)
                return;
            string solverOutput = model.Solve();
            //string solverOutput = model.GetEquations();
            output.Add(solverOutput);
        }
        static public void EquationGeneration(string inputText, ref List<ErrorMessage> errorList, ref List<string> output)
        {
            AntlrInputStream inputStream = new AntlrInputStream(inputText);
            Grammar.ModelGrammarLexer modelLexer = new Grammar.ModelGrammarLexer(inputStream);
            ErrorListener<int> lexerErrorListener = new ErrorListener<int>();
            modelLexer.RemoveErrorListeners();
            modelLexer.AddErrorListener(lexerErrorListener);
            CommonTokenStream commonTokenStream = new CommonTokenStream(modelLexer);
            Grammar.ModelGrammarParser modelParser = new Grammar.ModelGrammarParser(commonTokenStream);
            ErrorListener<IToken> parserErrorListener = new ErrorListener<IToken>();
            modelParser.RemoveErrorListeners();
            modelParser.AddErrorListener(parserErrorListener);

            Grammar.ModelGrammarParser.ModelContext modelContext = modelParser.model();
            errorList.AddRange(lexerErrorListener.GetErrors());
            errorList.AddRange(parserErrorListener.GetErrors());
            if (errorList.Count > 0)
                return;
            Grammar.Visitor visitor = new Grammar.Visitor();
            Grammar.Node root = visitor.VisitModel(modelContext);
            var model = Interpreter.GetInstanse().Generate((Grammar.ModelNode)root, ref errorList, ref output);
            if (errorList.Count > 0)
                return;
            string solverOutput = model.GetEquations();
            output.Add(solverOutput);
        }
    }
    public interface IModel
    {
        string Solve();
        string GetEquations();
    }
    public interface ITransientElementModel
    {
        ITransientElement CreateElement(Interpreter.Object elementObject, Dictionary<string, Pin> elementNodes);
    }
    public interface ISteadyStateElementModel
    {
        ISteadyStateElement CreateElement(Interpreter.Object elementObject, Dictionary<string, Pin> elementNodes);
    }
}