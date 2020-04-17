﻿using MathNet.Numerics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ElectricalPowerSystems.PowerModel.OldModel.PowerGraph
{
    public abstract class GraphOutput
    {
        public abstract string generate(PowerGraphManager.PowerGraphModel model, PowerGraphManager.PowerGraphSolveResult result);
    }
    public enum OutputMode
    {
        A = 0,
        B = 1,
        C = 2,
        FULL = 3
    }
    public class PowerOutput : GraphOutput
    {
        OutputMode mode;
        int element;
        public PowerOutput(OutputMode mode, int element)
        {
            this.mode = mode;
            this.element = element;
        }
        public override string generate(PowerGraphManager.PowerGraphModel model, PowerGraphManager.PowerGraphSolveResult result)
        {
            Complex32 value = result.powers[element];
            return $"Power[element={element}] = {value}";
        }
    }
    public class VoltageOutput : GraphOutput {
        OutputMode mode;
        string node;
        public VoltageOutput(OutputMode mode, string node)
        {
            this.mode = mode;
            this.node = node;
        }
        public override string generate(PowerGraphManager.PowerGraphModel model, PowerGraphManager.PowerGraphSolveResult result)
        {
            int nodeId = model.getNodeId(node);
            PowerGraphManager.ABCValue abcValue = result.nodeVoltages[nodeId];
            if (mode == OutputMode.FULL)
            {
                return $"Voltage[{node}] = A: {abcValue.A}, B: {abcValue.B}, C: {abcValue.C} ";
            }
            return $"Voltage[{node}] = {mode.ToString()}: {abcValue.get(Convert.ToInt32(mode))}";
        }
    }
    public class VoltageDiffOutput : GraphOutput
    {
        OutputMode mode1;
        string node1;
        OutputMode mode2;
        string node2;
        public VoltageDiffOutput(OutputMode mode1, string node1, OutputMode mode2, string node2)
        {
            this.mode1 = mode1;
            this.node1 = node1;
            this.mode2 = mode2;
            this.node2 = node2;
        }
        public override string generate(PowerGraphManager.PowerGraphModel model, PowerGraphManager.PowerGraphSolveResult result)
        {
            int nodeId1 = model.getNodeId(node1);
            int nodeId2 = model.getNodeId(node2);
            PowerGraphManager.ABCValue abcValue1 = result.nodeVoltages[nodeId1];
            PowerGraphManager.ABCValue abcValue2 = result.nodeVoltages[nodeId2];
            if (mode1 == OutputMode.FULL)
            {
                if(mode2 == OutputMode.FULL)
                    return $"Voltage difference[{node2} - {node1}] = A: {abcValue2.A-abcValue1.A}, B: {abcValue2.B-abcValue1.B}, C: {abcValue2.C-abcValue1.C} ";
                Complex32 value2 = abcValue2.get(Convert.ToInt32(mode2));
                return $"Voltage difference[{node2} - {node1}] =  {mode2.ToString()} - A: {value2 - abcValue1.A}, " +
                    $" {mode2.ToString()} - A: {value2 - abcValue1.B},  {mode2.ToString()} - B: {value2 - abcValue1.C} ";
            }
            else
            {
                Complex32 value1 = abcValue1.get(Convert.ToInt32(mode1));
                if (mode2 == OutputMode.FULL)
                {
                    return $"Voltage difference[{node2} - {node1}] = A - {mode1.ToString()}: {abcValue2.A - value1}, B: {abcValue2.B - value1}, C: {abcValue2.C - value1} ";
                }
                Complex32 value2 = abcValue2.get(Convert.ToInt32(mode1));
                return $"Voltage difference[{node2} - {node1}] = {mode2.ToString()} - {mode1.ToString()}: {value2 - value1}";
            }
        }
    }
}