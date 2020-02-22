using ElectricalPowerSystems.Interpreter.Equations.Nonlinear;
using MathNet.Numerics;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ElectricalPowerSystems.ACGraph
{

    namespace ElementsAC
    {
        public class EquationBlock
        {
            public string Equation;
        }
        //left side of f(x)=I
        //for source voltage it is Ie=Ielement
        //for resistance it is Y(v1-v2)=Ielement
        //each current equation block creates two equation
        public class CurrentFlowBlock : EquationBlock
        {
            public string Node1;
            public string Node2;
        }
        public enum ElementTypeEnum
        {
            Resistor,
            ResistorWithCurrent,
            Capacitor,
            Inductor,
            VoltageSource,
            CurrentSource,
            Transformer2w,
            Transformer3w,
            Autotransformer,
            Line,
            Ground,
            Switch,
            Impedance
        }
        public abstract class Element
        {
            public ElementTypeEnum ElementType { get; protected set; }
            public int[] nodes;
            public int elementIndex;
            public Element()
            {
                nodes = null;
            }
            protected string currentRe()
            {
                return $"I_{elementIndex}_re";
            }
            protected string currentIm()
            {
                return $"I_{elementIndex}_im";
            }
            protected string nodeVoltageRe(int i)
            {
                return $"v_{nodes[i]}_re";
            }
            protected string nodeVoltageIm(int i)
            {
                return $"v_{nodes[i]}_im";
            }
            public abstract List<EquationBlock> GetParametersAC();
            public abstract List<EquationBlock> GenerateEquationsAC();
            public abstract List<EquationBlock> GenerateEquationsTransient();
            public abstract List<EquationBlock> GetParametersTransient();
            public abstract Complex32 GetCurrent(NonlinearSystemSolution acSolution, float frequency);
            public virtual Complex32 GetVoltageDrop(NonlinearSystemSolution acSolution)
            {
                return new Complex32(0.0f,0.0f);
            }
        }
        public abstract class Element2N : Element
        {
            public Element2N(int node1, int node2,int index)
            {
                nodes = new int[] { node1, node2 };
                this.elementIndex = index;
            }
            public override Complex32 GetVoltageDrop(NonlinearSystemSolution acSolution)
            {
                var re = acSolution.getValue(nodeVoltageRe(0)) - acSolution.getValue(nodeVoltageRe(1));
                var im = acSolution.getValue(nodeVoltageIm(0)) - acSolution.getValue(nodeVoltageIm(1));
                return new Complex32((float)re,(float)im);
            }
        }
        public abstract class Element3N : Element
        {
            public Element3N(int node1, int node2, int node3, int index)
            {
                nodes = new int[] { node1, node2, node3 };
                this.elementIndex = index;
            }
        }
        public abstract class Element4N : Element
        {
            public Element4N(int node1, int node2, int node3, int node4, int index)
            {
                nodes = new int[] { node1, node2, node3, node4 };
                this.elementIndex = index;
            }
        }
        public abstract class Element6N : Element
        {
            public Element6N(int node1, int node2, int node3, int node4,int node5,int node6, int index)
            {
                nodes = new int[] { node1, node2, node3, node4, node5, node6 };
                this.elementIndex = index;
            }
        }
        public abstract class Element1N : Element
        {
            public Element1N(int node, int index)
            {
                nodes = new int[] { node };
                this.elementIndex = index;
            }
        }
        public class ResistorWithCurrent : Element2N
        {
            public float resistance;
            public ResistorWithCurrent(int node1, int node2, int index, float resistance) : base(node1, node2, index)
            {
                this.resistance = resistance;
                this.ElementType = ElementTypeEnum.ResistorWithCurrent;
            }
            public override List<EquationBlock> GetParametersAC()
            {
                List<EquationBlock> equations = new List<EquationBlock>();
                equations.Add(new EquationBlock
                {
                    Equation = $"set R_{elementIndex} = {resistance.ToString(new CultureInfo("en-US"))};"
                });
                return equations;
            }
            public override List<EquationBlock> GenerateEquationsAC()
            {
                List<EquationBlock> equations = new List<EquationBlock>();
                string R = $"R_{elementIndex}";
                string Ire = $"I_{elementIndex}_re";
                string Iim = $"I_{elementIndex}_im";
                string v1re = $"v_{nodes[0]}_re";
                string v1im = $"v_{nodes[0]}_im";
                string v2re = $"v_{nodes[1]}_re";
                string v2im = $"v_{nodes[1]}_im";

                equations.Add(new CurrentFlowBlock
                {
                    Equation = Ire,
                    Node1 = v1re,
                    Node2 = v2re
                });
                equations.Add(new CurrentFlowBlock
                {
                    Equation = Iim,
                    Node1 = v1im,
                    Node2 = v2im
                });
                equations.Add(new EquationBlock
                {
                    Equation = $"{v1re} - {v2re} - {Ire} * {R} = 0;"
                });
                equations.Add(new EquationBlock
                {
                    Equation = $"{v1im} - {v2im} - {Iim} * {R} = 0;"
                });
                return equations;
            }
            public override string ToString()
            {
                return $"Resistor{{n1 = {nodes[0]}, n2 = {nodes[1]}, resistance = {resistance} }}";
            }
            public override Complex32 GetCurrent(NonlinearSystemSolution acSolution, float frequency)
            {
                string Ire = $"I_{elementIndex}_re";
                string Iim = $"I_{elementIndex}_im";
                return new Complex32((float)acSolution.getValue(Ire),(float)acSolution.getValue(Iim));
            }

            public override List<EquationBlock> GenerateEquationsTransient()
            {
                List<EquationBlock> equations = new List<EquationBlock>();
                string R = $"R_{elementIndex}";
                string I = $"I_{elementIndex}";
                string v1 = $"v_{nodes[0]}";
                string v2 = $"v_{nodes[1]}";

                equations.Add(new CurrentFlowBlock
                {
                    Equation = I,
                    Node1 = v1,
                    Node2 = v2
                });
                equations.Add(new EquationBlock
                {
                    Equation = $"{v1} - {v2} - {I} * {R} = 0;"
                });
                return equations;
            }

            public override List<EquationBlock> GetParametersTransient()
            {
                return this.GetParametersAC();
            }
        }
        public class ImpedanceWithCurrent : Element2N
        {
            public Complex32 impedance;
            public ImpedanceWithCurrent(int node1, int node2, int index, Complex32 impedance) : base(node1, node2, index)
            {
                this.impedance = impedance;
                this.ElementType = ElementTypeEnum.Impedance;
            }
            public override List<EquationBlock> GetParametersAC()
            {
                List<EquationBlock> equations = new List<EquationBlock>();
                equations.Add(new EquationBlock
                {
                    Equation = $"set R_{elementIndex} = {impedance.Real.ToString(new CultureInfo("en-US"))};"
                });
                equations.Add(new EquationBlock
                {
                    Equation = $"set X_{elementIndex} = {impedance.Imaginary.ToString(new CultureInfo("en-US"))};"
                });
                return equations;
            }
            public override List<EquationBlock> GenerateEquationsAC()
            {
                List<EquationBlock> equations = new List<EquationBlock>();
                string R = $"R_{elementIndex}";
                string X = $"X_{elementIndex}";
                string Ire = $"I_{elementIndex}_re";
                string Iim = $"I_{elementIndex}_im";
                string v1re = $"v_{nodes[0]}_re";
                string v1im = $"v_{nodes[0]}_im";
                string v2re = $"v_{nodes[1]}_re";
                string v2im = $"v_{nodes[1]}_im";

                equations.Add(new CurrentFlowBlock
                {
                    Equation = Ire,
                    Node1 = v1re,
                    Node2 = v2re
                });
                equations.Add(new CurrentFlowBlock
                {
                    Equation = Iim,
                    Node1 = v1im,
                    Node2 = v2im
                });
                equations.Add(new EquationBlock
                {
                    Equation = $"{v1re} - {v2re} - ({Ire} * {R} - {Iim} * {X}) = 0;"
                });
                equations.Add(new EquationBlock
                {
                    Equation = $"{v1im} - {v2im} - ({Ire} * {X} + {Iim} * {R}) = 0;"
                });
                return equations;
            }
            public override string ToString()
            {
                return $"Complex resistor{{n1 = {nodes[0]}, n2 = {nodes[1]}, impedance = {impedance.ToString()} }}";
            }
            public override Complex32 GetCurrent(NonlinearSystemSolution acSolution, float frequency)
            {
                string Ire = $"I_{elementIndex}_re";
                string Iim = $"I_{elementIndex}_im";
                return new Complex32((float)acSolution.getValue(Ire), (float)acSolution.getValue(Iim));
            }
            public override List<EquationBlock> GenerateEquationsTransient()
            {
                List<EquationBlock> equations = new List<EquationBlock>();
                string R = $"R_{elementIndex}";
                string X = $"X_{elementIndex}";
                string I = $"I_{elementIndex}";
                string v1 = $"v_{nodes[0]}";
                string v2 = $"v_{nodes[1]}";

                equations.Add(new CurrentFlowBlock
                {
                    Equation = I,
                    Node1 = v1,
                    Node2 = v2
                });
                equations.Add(new EquationBlock
                {
                    Equation = $"{v1} - {v2} - ({I} * {R} - der({I}) * {X}/frequency) = 0;"
                });
                return equations;
            }
            public override List<EquationBlock> GetParametersTransient()
            {
                List<EquationBlock> equations = new List<EquationBlock>();
                equations.Add(new EquationBlock
                {
                    Equation = $"set R_{elementIndex} = {impedance.Real.ToString(new CultureInfo("en-US"))};"
                });
                equations.Add(new EquationBlock
                {
                    Equation = $"set X_{elementIndex} = {impedance.Imaginary.ToString(new CultureInfo("en-US"))};"
                });
                return equations;
            }
        }
        public class Impedance : Element2N
        {
            public Complex32 impedance;
            public Impedance(int node1, int node2, int index, Complex32 impedance) : base(node1, node2, index)
            {
                this.impedance = impedance;
                this.ElementType = ElementTypeEnum.Impedance;
            }
            public override List<EquationBlock> GetParametersAC()
            {
                List<EquationBlock> equations = new List<EquationBlock>();
                Complex32 Y = new Complex32(1.0f,0.0f) / impedance;
                equations.Add(new EquationBlock
                {
                    Equation = $"set G_{elementIndex} = {Y.Real.ToString(new CultureInfo("en-US"))};"
                });
                equations.Add(new EquationBlock
                {
                    Equation = $"set B_{elementIndex} = {Y.Imaginary.ToString(new CultureInfo("en-US"))};"
                });
                return equations;
            }
            public override List<EquationBlock> GenerateEquationsAC()
            {
                List<EquationBlock> equations = new List<EquationBlock>();
                string G = $"G_{elementIndex}";
                string B = $"B_{elementIndex}";
                equations.Add(new CurrentFlowBlock
                {
                    Equation = $"(v_{nodes[0]}_re * {G} - v_{nodes[0]}_im * {B}) - (v_{nodes[1]}_re * {G} - v_{nodes[1]}_im * {B})",
                    Node1 = $"v_{nodes[0]}_re",
                    Node2 = $"v_{nodes[1]}_re"
                });
                equations.Add(new CurrentFlowBlock
                {
                    Equation = $"(v_{nodes[0]}_im * {G} + v_{nodes[0]}_re * {B}) - (v_{nodes[1]}_im * {G} + v_{nodes[1]}_re * {B})",
                    Node1 = $"v_{nodes[0]}_im",
                    Node2 = $"v_{nodes[1]}_im"
                });
                return equations;
            }
            public override string ToString()
            {
                return $"Complex resistor{{n1 = {nodes[0]}, n2 = {nodes[1]}, impedance = {impedance.ToString()} }}";
            }
            public override Complex32 GetCurrent(NonlinearSystemSolution acSolution, float frequency)
            {
                double dvRe = acSolution.getValue($"v_{nodes[0]}_re") - acSolution.getValue($"v_{nodes[1]}_re");
                double dvIm = acSolution.getValue($"v_{nodes[0]}_im") - acSolution.getValue($"v_{nodes[1]}_im");
                return new Complex32((float)dvRe,(float)dvIm)/ impedance;
            }
            public override List<EquationBlock> GenerateEquationsTransient()
            {
                List<EquationBlock> equations = new List<EquationBlock>();
                string R = $"R_{elementIndex}";
                string X = $"X_{elementIndex}";
                string I = $"I_{elementIndex}";
                string v1 = $"v_{nodes[0]}";
                string v2 = $"v_{nodes[1]}";

                equations.Add(new CurrentFlowBlock
                {
                    Equation = I,
                    Node1 = v1,
                    Node2 = v2
                });
                equations.Add(new EquationBlock
                {
                    Equation = $"{v1} - {v2} - ({I} * {R} - der({I}) * {X}/frequency) = 0;"
                });
                return equations;
            }
            public override List<EquationBlock> GetParametersTransient()
            {
                List<EquationBlock> equations = new List<EquationBlock>();
                equations.Add(new EquationBlock
                {
                    Equation = $"set R_{elementIndex} = {impedance.Real.ToString(new CultureInfo("en-US"))};"
                });
                equations.Add(new EquationBlock
                {
                    Equation = $"set X_{elementIndex} = {impedance.Imaginary.ToString(new CultureInfo("en-US"))};"
                });
                return equations;
            }
        }
        public class Resistor : Element2N
        {
            public float resistance;
            public Resistor(int node1, int node2,int index, float resistance) : base(node1, node2,index)
            {
                this.resistance = resistance;
                this.ElementType = ElementTypeEnum.Resistor;
            }
            public override List<EquationBlock> GetParametersAC()
            {
                List<EquationBlock> equations = new List<EquationBlock>();
                equations.Add(new EquationBlock
                {
                    Equation = $"set G_{elementIndex} = {(1.0/resistance).ToString(new CultureInfo("en-US"))};"
                });
                return equations;
            }
            public override List<EquationBlock> GenerateEquationsAC()
            {
                List<EquationBlock> equations = new List<EquationBlock>();
                string G = $"G_{elementIndex}";
                equations.Add(new CurrentFlowBlock
                {
                    Equation = $"(v_{nodes[0]}_re * {G} - v_{nodes[1]}_re * {G})",
                    Node1 = $"v_{nodes[0]}_re",
                    Node2 = $"v_{nodes[1]}_re"
                });
                equations.Add(new CurrentFlowBlock
                {
                    Equation = $"(v_{nodes[0]}_im * {G} - v_{nodes[1]}_im * {G})",
                    Node1 = $"v_{nodes[0]}_im",
                    Node2 = $"v_{nodes[1]}_im"
                });
                return equations;
            }
            public override List<EquationBlock> GetParametersTransient()
            {
                List<EquationBlock> equations = new List<EquationBlock>();
                equations.Add(new EquationBlock
                {
                    Equation = $"set G_{elementIndex} = {(1.0 / resistance).ToString(new CultureInfo("en-US"))};"
                });
                return equations;
            }
            public override List<EquationBlock> GenerateEquationsTransient()
            {
                List<EquationBlock> equations = new List<EquationBlock>();
                string G = $"G_{elementIndex}";
                equations.Add(new CurrentFlowBlock
                {
                    Equation = $"(v_{nodes[0]} * {G} - v_{nodes[1]} * {G})",
                    Node1 = $"v_{nodes[0]}",
                    Node2 = $"v_{nodes[1]}"
                });
                return equations;
            }
            public override string ToString()
            {
                return $"Resistor{{n1 = {nodes[0]}, n2 = {nodes[1]}, resistance = {resistance} }}";
            }
            public override Complex32 GetCurrent(NonlinearSystemSolution acSolution, float frequency)
            {
                double dvRe = acSolution.getValue($"v_{nodes[0]}_re") - acSolution.getValue($"v_{nodes[1]}_re");
                double dvIm = acSolution.getValue($"v_{nodes[0]}_im") - acSolution.getValue($"v_{nodes[1]}_im");
                return new Complex32((float)dvRe, (float)dvIm) / resistance;
            }
        }
        /*public class TransformerDeltaDelta:Element
        {
            public float k;
            public TransformerDeltaDelta(int a1, int b1, int c1, int a2, int b2, int c2,int index, float k)
            {
                this.k = k;
                nodes = new int[] { a1,b1,c1,a2,b2,c2 };
                this.elementIndex = index;
            }
            public override List<EquationBlock> GetParametersAC()
            {
                List<EquationBlock> equations = new List<EquationBlock>();
                equations.Add(new EquationBlock
                {
                    Equation = $"set G_{elementIndex} = {(1.0 / resistance).ToString(new CultureInfo("en-US"))};"
                });
                return equations;
            }
            public override List<EquationBlock> GenerateEquationsAC()
            {
                List<EquationBlock> equations = new List<EquationBlock>();
                string G = $"G_{elementIndex}";
                equations.Add(new CurrentFlowBlock
                {
                    Equation = $"(v_{nodes[0]}_re * {G} - v_{nodes[1]}_re * {G})",
                    Node1 = $"v_{nodes[0]}_re",
                    Node2 = $"v_{nodes[1]}_re"
                });
                equations.Add(new CurrentFlowBlock
                {
                    Equation = $"(v_{nodes[0]}_im * {G} - v_{nodes[1]}_im * {G})",
                    Node1 = $"v_{nodes[0]}_im",
                    Node2 = $"v_{nodes[1]}_im"
                });
                return equations;
            }
            public override string ToString()
            {
                return $"TransformerDeltaDelta{{a1 = {nodes[0]}, b1 = {nodes[1]}, c1 = {nodes[2]}, a2 = {nodes[3]}, b2 = {nodes[4]}, c2 = {nodes[5]} k = {k} }}";
            }
            public override Complex32 GetCurrent(NonlinearSystemSolution acSolution, float frequency)
            {
                return new Complex32();
            }
        }*/
        public class Transformer3w : Element6N
        {
            public float b1;
            public float b2;
            public Transformer3w(int node1, int node2, int node3, int node4,int node5,int node6, 
                int index, float b1, float b2) : base(node1, node2, node3, node4, node5, node6, index)
            {
                this.b1 = b1;
                this.b2 = b2;
                this.ElementType = ElementTypeEnum.Transformer3w;
            }
            public override List<EquationBlock> GenerateEquationsAC()
            {
                List<EquationBlock> equations = new List<EquationBlock>();
                string I1re = $"I_{elementIndex}_1_re";
                string I1im = $"I_{elementIndex}_1_im";
                string I2re = $"I_{elementIndex}_2_re";
                string I2im = $"I_{elementIndex}_2_im";
                string I3re = $"I_{elementIndex}_3_re";
                string I3im = $"I_{elementIndex}_3_im";
                string v1re = $"v_{nodes[0]}_re";
                string v1im = $"v_{nodes[0]}_im";
                string v2re = $"v_{nodes[1]}_re";
                string v2im = $"v_{nodes[1]}_im";
                string v3re = $"v_{nodes[2]}_re";
                string v3im = $"v_{nodes[2]}_im";
                string v4re = $"v_{nodes[3]}_re";
                string v4im = $"v_{nodes[3]}_im";
                string v5re = $"v_{nodes[4]}_re";
                string v5im = $"v_{nodes[4]}_im";
                string v6re = $"v_{nodes[5]}_re";
                string v6im = $"v_{nodes[5]}_im";
                string k1 = $"k_{elementIndex}_1";
                string k2 = $"k_{elementIndex}_2";
                equations.Add(new CurrentFlowBlock
                {
                    Equation = $"({I2re} * {k1} + {I3re} * {k2})",
                    Node1 = v1re,
                    Node2 = v2re
                });
                equations.Add(new CurrentFlowBlock
                {
                    Equation = $"({I2im} * {k1} + {I3im} * {k2})",
                    Node1 = v1im,
                    Node2 = v2im
                });
                equations.Add(new CurrentFlowBlock
                {
                    Equation = $"{I2re}",
                    Node1 = v3re,
                    Node2 = v4re
                });
                equations.Add(new CurrentFlowBlock
                {
                    Equation = $"{I2im}",
                    Node1 = v3im,
                    Node2 = v4im
                });
                equations.Add(new CurrentFlowBlock
                {
                    Equation = $"{I3re}",
                    Node1 = v5re,
                    Node2 = v6re
                });
                equations.Add(new CurrentFlowBlock
                {
                    Equation = $"{I3im}",
                    Node1 = v5im,
                    Node2 = v6im
                });
                equations.Add(new EquationBlock
                {
                    Equation = $"{k1} * ({v1re} - {v2re}) = ({v3re} - {v4re});"
                });
                equations.Add(new EquationBlock
                {
                    Equation = $"{k1} * ({v1im} - {v2im}) = ({v3im} - {v4im});"
                });
                equations.Add(new EquationBlock
                {
                    Equation = $"{k2} * ({v1re} - {v2re}) = ({v5re} - {v6re});"
                });
                equations.Add(new EquationBlock
                {
                    Equation = $"{k2} * ({v1im} - {v2im}) = ({v5im} - {v6im});"
                });
                return equations;
            }
            public override List<EquationBlock> GetParametersAC()
            {
                List<EquationBlock> equations = new List<EquationBlock>();
                equations.Add(new EquationBlock
                {
                    Equation = $"set k_{elementIndex}_1 = {b1.ToString(new CultureInfo("en-US"))};"
                });
                equations.Add(new EquationBlock
                {
                    Equation = $"set k_{elementIndex}_1 = {b2.ToString(new CultureInfo("en-US"))};"
                });
                return equations;
            }

            public override List<EquationBlock> GenerateEquationsTransient()
            {
                List<EquationBlock> equations = new List<EquationBlock>();
                string I1 = $"I_{elementIndex}_1";
                string I2 = $"I_{elementIndex}_2";
                string I3 = $"I_{elementIndex}_3";
                string v1 = $"v_{nodes[0]}";
                string v2 = $"v_{nodes[1]}";
                string v3 = $"v_{nodes[2]}";
                string v4 = $"v_{nodes[3]}";
                string v5 = $"v_{nodes[4]}";
                string v6 = $"v_{nodes[5]}";
                string k1 = $"k_{elementIndex}_1";
                string k2 = $"k_{elementIndex}_2";
                equations.Add(new CurrentFlowBlock
                {
                    Equation = $"({I2} * {k1} + {I3} * {k2})",
                    Node1 = v1,
                    Node2 = v2
                });
                equations.Add(new CurrentFlowBlock
                {
                    Equation = $"{I2}",
                    Node1 = v3,
                    Node2 = v4
                });
                equations.Add(new CurrentFlowBlock
                {
                    Equation = $"{I3}",
                    Node1 = v5,
                    Node2 = v6
                });
                equations.Add(new EquationBlock
                {
                    Equation = $"{k1} * ({v1} - {v2}) = ({v3} - {v4});"
                });
                equations.Add(new EquationBlock
                {
                    Equation = $"{k2} * ({v1} - {v2}) = ({v5} - {v6});"
                });
                return equations;
            }
            public override string ToString()
            {
                return $"Transformer3w{{n1 = {nodes[0]}, n2 = {nodes[1]}, n3 = {nodes[2]}, n4 = {nodes[3]}, n5 = {nodes[4]}, n6 = {nodes[5]} b1 = {b1} , b2 = {b2} }}";
            }
            public override Complex32 GetCurrent(NonlinearSystemSolution acSolution, float frequency)
            {
                return new Complex32();
            }

            public override List<EquationBlock> GetParametersTransient()
            {
                return this.GetParametersAC();
            }
        }
        public class Transformer2w : Element4N
        {
            public float b;
            public Transformer2w(int node1, int node2, int node3, int node4,int index, float b) : base(node1, node2, node3, node4,index)
            {
                this.b = b;
                this.ElementType = ElementTypeEnum.Transformer2w;
            }
            public override List<EquationBlock> GenerateEquationsAC()
            {
                List<EquationBlock> equations = new List<EquationBlock>();
                string I1re = $"I_{elementIndex}_1_re";
                string I1im = $"I_{elementIndex}_1_im";
                string I2re = $"I_{elementIndex}_2_re";
                string I2im = $"I_{elementIndex}_2_im";
                string v1re = $"v_{nodes[0]}_re";
                string v1im = $"v_{nodes[0]}_im";
                string v2re = $"v_{nodes[1]}_re";
                string v2im = $"v_{nodes[1]}_im";
                string v3re = $"v_{nodes[2]}_re";
                string v3im = $"v_{nodes[2]}_im";
                string v4re = $"v_{nodes[3]}_re";
                string v4im = $"v_{nodes[3]}_im";
                string k = $"k_{elementIndex}";
                equations.Add(new CurrentFlowBlock
                {
                    Equation = $"{I1re}",
                    Node1 = v1re,
                    Node2 = v2re
                });
                equations.Add(new CurrentFlowBlock
                {
                    Equation = $"{I1im}",
                    Node1 = v1im,
                    Node2 = v2im
                });
                equations.Add(new CurrentFlowBlock
                {
                    Equation = $"{I2re}",
                    Node1 = v3re,
                    Node2 = v4re
                });
                equations.Add(new CurrentFlowBlock
                {
                    Equation = $"{I2im}",
                    Node1 = v3im,
                    Node2 = v4im
                });
                equations.Add(new EquationBlock
                {
                    Equation = $"{I1re} = {k} * {I2re};"
                });
                equations.Add(new EquationBlock
                {
                    Equation = $"{I1im} = {k} * {I2im};"
                });
                equations.Add(new EquationBlock
                {
                    Equation = $"{k} * ({v1re} - {v2re}) = {v3re} - {v4re};"
                });
                equations.Add(new EquationBlock
                {
                    Equation = $"{k} * ({v1im} - {v2im}) = {v3im} - {v4im};"
                });
                return equations;
            }
            public override List<EquationBlock> GenerateEquationsTransient()
            {
                List<EquationBlock> equations = new List<EquationBlock>();
                string I1 = $"I_{elementIndex}_1";
                string I2 = $"I_{elementIndex}_2";
                string v1 = $"v_{nodes[0]}";
                string v2 = $"v_{nodes[1]}";
                string v3 = $"v_{nodes[2]}";
                string v4 = $"v_{nodes[3]}";
                string k = $"k_{elementIndex}";
                equations.Add(new CurrentFlowBlock
                {
                    Equation = $"{I1}",
                    Node1 = v1,
                    Node2 = v2
                });
                equations.Add(new CurrentFlowBlock
                {
                    Equation = $"{I1}",
                    Node1 = v1,
                    Node2 = v2
                });
                equations.Add(new CurrentFlowBlock
                {
                    Equation = $"{I2}",
                    Node1 = v3,
                    Node2 = v4
                });
                equations.Add(new CurrentFlowBlock
                {
                    Equation = $"{I2}",
                    Node1 = v3,
                    Node2 = v4
                });
                equations.Add(new EquationBlock
                {
                    Equation = $"{I1} = {k} * {I2};"
                });
                equations.Add(new EquationBlock
                {
                    Equation = $"{k} * ({v1} - {v2}) = {v3} - {v4};"
                });
                return equations;
            }
            public override List<EquationBlock> GetParametersAC()
            {
                List<EquationBlock> equations = new List<EquationBlock>();
                equations.Add(new EquationBlock
                {
                    Equation = $"set k_{elementIndex} = {b.ToString(new CultureInfo("en-US"))};"
                });
                return equations;
            }
            public override string ToString()
            {
                return $"Transformer{{n1 = {nodes[0]}, n2 = {nodes[1]}, n3 = {nodes[2]}, n4 = {nodes[3]}, b = {b} }}";
            }
            public override Complex32 GetCurrent(NonlinearSystemSolution acSolution, float frequency)
            {
                return new Complex32();
            }
            public override List<EquationBlock> GetParametersTransient()
            {
                return this.GetParametersAC();
            }
        }
        public class Autotransformer : Element3N
        {
            public float b;
            public Autotransformer(int node1, int node2, int node3, int index, float b) : base(node1, node2, node3, index)
            {
                this.b = b;
                this.ElementType = ElementTypeEnum.Autotransformer;
            }
            public override List<EquationBlock> GenerateEquationsAC()
            {
                List<EquationBlock> equations = new List<EquationBlock>();
                string I1re = $"I_{elementIndex}_1_re";
                string I1im = $"I_{elementIndex}_1_im";
                string I2re = $"I_{elementIndex}_2_re";
                string I2im = $"I_{elementIndex}_2_im";
                string v1re = $"v_{nodes[0]}_re";
                string v1im = $"v_{nodes[0]}_im";
                string v2re = $"v_{nodes[1]}_re";
                string v2im = $"v_{nodes[1]}_im";
                string v3re = $"v_{nodes[2]}_re";
                string v3im = $"v_{nodes[2]}_im";
                string k = $"k_{elementIndex}";
                equations.Add(new CurrentFlowBlock
                {
                    Equation = $"{I1re}",
                    Node1 = v1re,
                    Node2 = v2re
                });
                equations.Add(new CurrentFlowBlock
                {
                    Equation = $"{I1im}",
                    Node1 = v1im,
                    Node2 = v2im
                });
                equations.Add(new CurrentFlowBlock
                {
                    Equation = $"{I2re}",
                    Node1 = v3re,
                    Node2 = v2re
                });
                equations.Add(new CurrentFlowBlock
                {
                    Equation = $"{I2im}",
                    Node1 = v3im,
                    Node2 = v2im
                });
                equations.Add(new EquationBlock
                {
                    Equation = $"{I1re} = {k} * {I1re} + {k} * {I2re};"
                });
                equations.Add(new EquationBlock
                {
                    Equation = $"{I1im} = {k} * {I1im} + {k} * {I2im};"
                });
                equations.Add(new EquationBlock
                {
                    Equation = $"{k} * ({v1re} - {v3re}) = {v2re} - {v3re};"
                });
                equations.Add(new EquationBlock
                {
                    Equation = $"{k} * ({v1im} - {v3im}) = {v2im} - {v3im};"
                });
                return equations;
            }
            public override List<EquationBlock> GetParametersAC()
            {
                List<EquationBlock> equations = new List<EquationBlock>();
                equations.Add(new EquationBlock
                {
                    Equation = $"set k_{elementIndex} = {b.ToString(new CultureInfo("en-US"))};"
                });
                return equations;
            }
            public override string ToString()
            {
                return $"Autotransformer{{n1 = {nodes[0]}, n2 = {nodes[1]}, n3 = {nodes[2]}, b = {b} }}";
            }
            public override Complex32 GetCurrent(NonlinearSystemSolution acSolution, float frequency)
            {
                return new Complex32();
            }

            public override List<EquationBlock> GenerateEquationsTransient()
            {
                List<EquationBlock> equations = new List<EquationBlock>();
                string I1 = $"I_{elementIndex}_1";
                string I2 = $"I_{elementIndex}_2";
                string v1 = $"v_{nodes[0]}";
                string v2 = $"v_{nodes[1]}";
                string v3 = $"v_{nodes[2]}";
                string k = $"k_{elementIndex}";
                equations.Add(new CurrentFlowBlock
                {
                    Equation = $"{I1}",
                    Node1 = v1,
                    Node2 = v2
                });
                equations.Add(new CurrentFlowBlock
                {
                    Equation = $"{I2}",
                    Node1 = v3,
                    Node2 = v2
                });
                equations.Add(new EquationBlock
                {
                    Equation = $"{I1} = {k} * {I1} + {k} * {I2};"
                });
                equations.Add(new EquationBlock
                {
                    Equation = $"{k} * ({v1} - {v3}) = {v2} - {v3};"
                });
                return equations;
            }

            public override List<EquationBlock> GetParametersTransient()
            {
                return this.GetParametersAC();
            }
        }
        public class Capacitor : Element2N
        {
            public float capacity;
            public Capacitor(int node1, int node2, int index, float capacity) : base(node1, node2,index)
            {
                this.capacity = capacity;
                this.ElementType = ElementTypeEnum.Capacitor;
            }
            public override List<EquationBlock> GenerateEquationsAC()
            {
                string v1re = $"v_{nodes[0]}_re";
                string v1im = $"v_{nodes[0]}_im";
                string v2re = $"v_{nodes[1]}_re";
                string v2im = $"v_{nodes[1]}_im";
                string C = $"C_{ elementIndex}";
                List<EquationBlock> equations = new List<EquationBlock>();
                equations.Add(new CurrentFlowBlock
                {
                    Equation = $" {C} * frequency * (- {v1im} + {v2im})",
                    Node1 = v1re,
                    Node2 = v2re
                });
                equations.Add(new CurrentFlowBlock
                {
                    Equation = $" {C} * frequency * ({v1re} - {v2re})",
                    Node1 = v1im,
                    Node2 = v2im
                });
                return equations;
            }
            public override List<EquationBlock> GenerateEquationsTransient()
            {
                string v1re = $"v_{nodes[0]}";
                string v2re = $"v_{nodes[1]}";
                string C = $"C_{ elementIndex}";
                List<EquationBlock> equations = new List<EquationBlock>();
                equations.Add(new CurrentFlowBlock
                {
                    Equation = $" {C} * (der({v1re}) - der({v2re}))",
                    Node1 = v1re,
                    Node2 = v2re
                });
                return equations;
            }
            public override List<EquationBlock> GetParametersAC()
            {
                List<EquationBlock> equations = new List<EquationBlock>();
                equations.Add(new EquationBlock
                {
                    Equation = $"set C_{elementIndex} = {capacity.ToString(new CultureInfo("en-US"))};"
                });
                return equations;
            }
            public override string ToString()
            {
                return $"Capacitor{{n1 = {nodes[0]}, n2 = {nodes[1]}, capacity = {capacity} }}";
            }
            public override Complex32 GetCurrent(NonlinearSystemSolution acSolution,float frequency)
            {
                double dvRe = acSolution.getValue($"v_{nodes[0]}_re") - acSolution.getValue($"v_{nodes[1]}_re");
                double dvIm = acSolution.getValue($"v_{nodes[0]}_im") - acSolution.getValue($"v_{nodes[1]}_im");
                return new Complex32((float)dvRe,(float)dvIm)*new Complex32(0,this.capacity * frequency);
            }

            public override List<EquationBlock> GetParametersTransient()
            {
                return this.GetParametersAC();
            }
        }
        public class Inductor : Element2N
        {
            public float inductivity;
            public Inductor(int node1, int node2, int index, float inductivity) : base(node1, node2,index)
            {
                this.inductivity = inductivity;
                this.ElementType = ElementTypeEnum.Inductor;
            }
            public override List<EquationBlock> GenerateEquationsTransient()
            {
                string v1 = $"v_{nodes[0]}";
                string v2 = $"v_{nodes[1]}";
                string L = $"L_{ elementIndex}";
                string I = $"I_{elementIndex}";
                List<EquationBlock> equations = new List<EquationBlock>();
                equations.Add(new CurrentFlowBlock
                {
                    Equation = I,
                    Node1 = v1,
                    Node2 = v2
                });
                equations.Add(new EquationBlock
                {
                    Equation = $"{v1} - {v2} = {L} * der({I});"
                });
                return equations;
            }
            public override List<EquationBlock> GenerateEquationsAC()
            {
                string v1re = $"v_{nodes[0]}_re";
                string v1im = $"v_{nodes[0]}_im";
                string v2re = $"v_{nodes[1]}_re";
                string v2im = $"v_{nodes[1]}_im";
                string L = $"L_{ elementIndex}";
                string Ire = $"I_{elementIndex}_re";
                string Iim = $"I_{elementIndex}_im";
                List<EquationBlock> equations = new List<EquationBlock>();
                equations.Add(new CurrentFlowBlock
                {
                    Equation = Ire,
                    Node1 = v1re,
                    Node2 = v2re
                });
                equations.Add(new CurrentFlowBlock
                {
                    Equation = Iim,
                    Node1 = v1im,
                    Node2 = v2im
                });
                equations.Add(new EquationBlock
                {
                    Equation = $"{v1re} - {v2re} = - frequency * {L} * {Iim};"
                });
                equations.Add(new EquationBlock
                {
                    Equation = $"{v1im} - {v2im} = frequency * {L} * {Ire};"
                });
                return equations;
            }
            public override List<EquationBlock> GetParametersAC()
            {
                List<EquationBlock> equations = new List<EquationBlock>();
                equations.Add(new EquationBlock
                {
                    Equation = $"set L_{elementIndex} = {inductivity.ToString(new CultureInfo("en-US"))};"
                });
                return equations;
            }
            public override string ToString()
            {
                return $"Inductor{{n1 = {nodes[0]}, n2 = {nodes[1]}, inductivity = {inductivity} }}";
            }
            public override Complex32 GetCurrent(NonlinearSystemSolution acSolution, float frequency)
            {
                return new Complex32((float)acSolution.getValue($"I_{elementIndex}_re"), 
                    (float)acSolution.getValue($"I_{elementIndex}_im"));
            }
            public override List<EquationBlock> GetParametersTransient()
            {
                return this.GetParametersAC();
            }
        }
        public class Ground : Element1N
        {
            public Ground(int node,int index) : base(node,index)
            {
                this.ElementType = ElementTypeEnum.Ground;
            }
            public override string ToString()
            {
                return $"Ground{{n = {nodes[0]} }}";
            }
            public override List<EquationBlock> GenerateEquationsAC()
            {
                List<EquationBlock> equations = new List<EquationBlock>();
                equations.Add(new CurrentFlowBlock
                {
                    Equation = $"I_{elementIndex}_re",
                    Node1 = $"v_{nodes[0]}_re",
                    Node2 = null
                });
                equations.Add(new CurrentFlowBlock
                {
                    Equation = $"I_{elementIndex}_im",
                    Node1 = $"v_{nodes[0]}_im",
                    Node2 = null
                });
                equations.Add(new EquationBlock
                {
                    Equation = $"v_{nodes[0]}_re = 0;"
                });
                equations.Add(new EquationBlock
                {
                    Equation = $"v_{nodes[0]}_im = 0;"
                });
                return equations;
            }
            public override List<EquationBlock> GetParametersAC()
            {
                return new List<EquationBlock>();
            }
            public override Complex32 GetCurrent(NonlinearSystemSolution acSolution, float frequency)
            {
                return new Complex32((float)acSolution.getValue($"I_{elementIndex}_re"),
                    (float)acSolution.getValue($"I_{elementIndex}_im"));
            }
            public override List<EquationBlock> GenerateEquationsTransient()
            {
                List<EquationBlock> equations = new List<EquationBlock>();
                equations.Add(new CurrentFlowBlock
                {
                    Equation = $"I_{elementIndex}",
                    Node1 = $"v_{nodes[0]}",
                    Node2 = null
                });
                equations.Add(new EquationBlock
                {
                    Equation = $"v_{nodes[0]} = 0;"
                });
                return equations;
            }
            public override List<EquationBlock> GetParametersTransient()
            {
                return this.GetParametersTransient();
            }
        }
        public class Line : Element2N
        {
            public Line(int node1, int node2,int index) : base(node1, node2, index)
            {
                this.ElementType = ElementTypeEnum.Line;
            }
            public override string ToString()
            {
                return $"Line{{n1 = {nodes[0]}, n2 = {nodes[1]} }}";
            }
            public override List<EquationBlock> GenerateEquationsAC()
            {
                List<EquationBlock> equations = new List<EquationBlock>();
                equations.Add(new CurrentFlowBlock
                {
                    Equation = $"I_{elementIndex}_re",
                    Node1 = $"v_{nodes[0]}_re",
                    Node2 = $"v_{nodes[1]}_re"
                });
                equations.Add(new CurrentFlowBlock
                {
                    Equation = $"I_{elementIndex}_im",
                    Node1 = $"v_{nodes[0]}_im",
                    Node2 = $"v_{nodes[1]}_im"
                });
                equations.Add(new EquationBlock
                {
                    Equation = $"v_{nodes[0]}_re - v_{nodes[1]}_re = 0;",
                });
                equations.Add(new EquationBlock
                {
                    Equation = $"v_{nodes[0]}_im - v_{nodes[1]}_im = 0;",
                });
                return equations;
            }
            public override List<EquationBlock> GetParametersAC()
            {
                return new List<EquationBlock>();
            }
            public override Complex32 GetCurrent(NonlinearSystemSolution acSolution, float frequency)
            {
                return new Complex32((float)acSolution.getValue($"I_{elementIndex}_re"),
                    (float)acSolution.getValue($"I_{elementIndex}_im"));
            }

            public override List<EquationBlock> GenerateEquationsTransient()
            {
                List<EquationBlock> equations = new List<EquationBlock>();
                equations.Add(new CurrentFlowBlock
                {
                    Equation = $"I_{elementIndex}",
                    Node1 = $"v_{nodes[0]}",
                    Node2 = $"v_{nodes[1]}"
                });
                equations.Add(new EquationBlock
                {
                    Equation = $"v_{nodes[0]} - v_{nodes[1]} = 0;",
                });
                return equations;
            }

            public override List<EquationBlock> GetParametersTransient()
            {
                return this.GetParametersAC();
            }
        }
        public class VoltageSource : Element2N
        {
            public float voltage;
            public float phase;
            public float frequency;
            public VoltageSource(int node1, int node2, int index, float voltage, float phase, float frequency) : base(node1, node2,index)
            {
                this.voltage = voltage;
                this.frequency = frequency;
                this.phase = phase;
                this.ElementType = ElementTypeEnum.VoltageSource;
            }
            public override string ToString()
            {
                return $"Voltage Source{{n1 = {nodes[0]}, n2 = {nodes[1]}, voltage = {voltage}@{Utils.degrees(phase)}, frequency = {frequency} Hz}}";
            }
            public override List<EquationBlock> GenerateEquationsAC()
            {
                List<EquationBlock> equations = new List<EquationBlock>();
                equations.Add(new CurrentFlowBlock
                {
                    Equation = $"I_{elementIndex}_re",
                    Node1 = $"v_{nodes[1]}_re",
                    Node2 = $"v_{nodes[0]}_re"
                });
                equations.Add(new CurrentFlowBlock
                {
                    Equation = $"I_{elementIndex}_im",
                    Node1 = $"v_{nodes[1]}_im",
                    Node2 = $"v_{nodes[0]}_im"
                });
                equations.Add(new EquationBlock
                {
                    Equation = $"v_{nodes[1]}_re - v_{nodes[0]}_re = E_{elementIndex}_re;",
                });
                equations.Add(new EquationBlock
                {
                    Equation = $"v_{nodes[1]}_im - v_{nodes[0]}_im = E_{elementIndex}_im;",
                });
                return equations;
            }
            public override List<EquationBlock> GetParametersAC()
            {
                List<EquationBlock> equations = new List<EquationBlock>();
                equations.Add(new EquationBlock
                {
                    Equation = $"set E_{elementIndex}_re = {(voltage * Math.Cos(phase)).ToString(new CultureInfo("en-US"))};"
                });
                equations.Add(new EquationBlock
                {
                    Equation = $"set E_{elementIndex}_im = {(voltage * Math.Sin(phase)).ToString(new CultureInfo("en-US"))};"
                });
                return equations;
            }
            public override Complex32 GetCurrent(NonlinearSystemSolution acSolution, float frequency)
            {
                return new Complex32((float)acSolution.getValue($"I_{elementIndex}_re"),
                    (float)acSolution.getValue($"I_{elementIndex}_im"));
            }

            public override List<EquationBlock> GenerateEquationsTransient()
            {
                List<EquationBlock> equations = new List<EquationBlock>();
                equations.Add(new CurrentFlowBlock
                {
                    Equation = $"I_{elementIndex}",
                    Node1 = $"v_{nodes[1]}",
                    Node2 = $"v_{nodes[0]}"
                });
                equations.Add(new EquationBlock
                {
                    Equation = $"v_{nodes[1]} - v_{nodes[0]} = E_{elementIndex} * cos(w_{elementIndex} * t + ph_{elementIndex});",
                });
                return equations;
            }

            public override List<EquationBlock> GetParametersTransient()
            {
                List<EquationBlock> equations = new List<EquationBlock>();
                equations.Add(new EquationBlock
                {
                    Equation = $"set E_{elementIndex} = {(voltage).ToString(new CultureInfo("en-US"))};"
                });
                equations.Add(new EquationBlock
                {
                    Equation = $"set w_{elementIndex} = {frequency.ToString(new CultureInfo("en-US"))};"
                });
                equations.Add(new EquationBlock
                {
                    Equation = $"set ph_{elementIndex} = {phase.ToString(new CultureInfo("en-US"))};"
                });
                return equations;
            }
        }
        public class Switch : Element2N
        {
            public bool state;//true - on, false - off
            public Switch(int node1, int node2,int index, bool state) : base(node1, node2,index)
            {
                this.state = state;
                this.ElementType = ElementTypeEnum.Switch;
            }
            public override string ToString()
            {
                return $"Switch{{n1 = {nodes[0]}, n2 = {nodes[1]}, state = {state}}}";
            }
            public override List<EquationBlock> GenerateEquationsAC()
            {
                List<EquationBlock> equations = new List<EquationBlock>();
                string p = $"switch_state_{elementIndex}";
                equations.Add(new CurrentFlowBlock
                {
                    Equation = $"I_{elementIndex}_re",
                    Node1 = $"v_{nodes[0]}_re",
                    Node2 = $"v_{nodes[1]}_re"
                });
                equations.Add(new CurrentFlowBlock
                {
                    Equation = $"I_{elementIndex}_im",
                    Node1 = $"v_{nodes[0]}_im",
                    Node2 = $"v_{nodes[1]}_im"
                });
                equations.Add(new EquationBlock
                {
                    Equation = $"{p} * (v_{nodes[0]}_re - v_{nodes[1]}_re) + (1 - {p}) * i_{elementIndex}_re = {0.0};",
                });
                equations.Add(new EquationBlock
                {
                    Equation = $"{p} * (v_{nodes[0]}_im - v_{nodes[1]}_im) + (1 - {p}) * i_{elementIndex}_im  = {0.0};",
                });
                return equations;
            }
            public override List<EquationBlock> GetParametersAC()
            {
                List<EquationBlock> equations = new List<EquationBlock>();
                equations.Add(new EquationBlock
                {
                    Equation = $"set switch_state_{elementIndex} = {(state ? 1.0 : 0.0).ToString(new CultureInfo("en-US"))};"
                });
                return equations;
            }

            public override Complex32 GetCurrent(NonlinearSystemSolution acSolution, float frequency)
            {
                string currentRe = $"I_{elementIndex}_re";
                string currentIm = $"I_{elementIndex}_re";
                return new Complex32((float)acSolution.getValue(currentRe),(float)acSolution.getValue(currentIm));
            }

            public override List<EquationBlock> GenerateEquationsTransient()
            {
                throw new NotImplementedException();
            }

            public override List<EquationBlock> GetParametersTransient()
            {
                return this.GetParametersAC();
            }
        }
        public class CurrentSource : Element2N
        {
            public float current;
            public float phase;
            public float frequency;
            public CurrentSource(int node1, int node2, int index, float current, float phase, float frequency) : base(node1, node2, index)
            {
                this.current = current;
                this.frequency = frequency;
                this.phase = phase;
                this.ElementType = ElementTypeEnum.CurrentSource;
            }
            public override string ToString()
            {
                return $"Current Source{{n1 = {nodes[0]}, n2 = {nodes[1]}, current = {current}@{Utils.degrees(phase)}, frequency = {frequency} Hz}}";
            }
            public override List<EquationBlock> GenerateEquationsAC()
            {
                List<EquationBlock> equations = new List<EquationBlock>();
                equations.Add(new CurrentFlowBlock
                {
                    Equation = $"J_{elementIndex}_re",
                    Node1 = $"v_{nodes[0]}_re",
                    Node2 = $"v_{nodes[1]}_re"
                });
                equations.Add(new CurrentFlowBlock
                {
                    Equation = $"J_{elementIndex}_im",
                    Node1 = $"v_{nodes[0]}_im",
                    Node2 = $"v_{nodes[1]}_im"
                });
                return equations;
            }
            public override List<EquationBlock> GenerateEquationsTransient()
            {
                List<EquationBlock> equations = new List<EquationBlock>();
                equations.Add(new CurrentFlowBlock
                {
                    Equation = $"J_{elementIndex} * cos(w_{elementIndex} * t + ph_{elementIndex})",
                    Node1 = $"v_{nodes[0]}",
                    Node2 = $"v_{nodes[1]}"
                });
                return equations;
            }
            public override List<EquationBlock> GetParametersAC()
            {
                List<EquationBlock> equations = new List<EquationBlock>();
                equations.Add(new EquationBlock
                {
                    Equation = $"set J_{elementIndex}_re = {(current*Math.Cos(phase)).ToString(new CultureInfo("en-US"))};"
                });
                equations.Add(new EquationBlock
                {
                    Equation = $"set J_{elementIndex}_im = {(current * Math.Sin(phase)).ToString(new CultureInfo("en-US"))};"
                });
                return equations;
            }
            public override Complex32 GetCurrent(NonlinearSystemSolution acSolution, float frequency)
            {
                return new Complex32((float)(current*Math.Cos(phase)), (float)(current *Math.Sin(phase)));
            }

            public override List<EquationBlock> GetParametersTransient()
            {
                List<EquationBlock> equations = new List<EquationBlock>();
                equations.Add(new EquationBlock
                {
                    Equation = $"set J_{elementIndex} = {current.ToString(new CultureInfo("en-US"))};"
                });
                equations.Add(new EquationBlock
                {
                    Equation = $"set w_{elementIndex} = {frequency.ToString(new CultureInfo("en-US"))};"
                });
                equations.Add(new EquationBlock
                {
                    Equation = $"set ph_{elementIndex} = {phase.ToString(new CultureInfo("en-US"))};"
                });
                return equations;
            }
        }
    }
}