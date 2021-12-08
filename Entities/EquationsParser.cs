using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

namespace EcoSys.Entities
{
    public static class EasyEquationParser
    {
        public static double getRatioFromString(string equation)
        {
            double result;
            string[] arg = equation.Split('=', '*');
            if (Double.TryParse(arg[1], out result)) return result;
            else
            {
                if (arg[1][0] == '-') return -1; else return 1;
            }
        }
    }
    public class EquationsParser
    {
        public List<(double, string)> monomial { get; set; } = new List<(double, string)>();
        public List<char> operands { get; set; } = new List<char>();
        public double? answer { get; set; } = null;
        public double?[] values { get; set; }
        public void createValuesArray()
        {
            values = new double?[monomial.Count];
        }

        public string getAnswerToString()
        {
            var temp_answer = Math.Round((double)answer, 3);
            return temp_answer.ToString();
        }

        public bool tryCalculateValue()
        {
            for (int i = 0; i < monomial.Count; i++)
                if (!monomial[i].Item2.Equals(String.Empty) && values[i] is null) return false;
            calculateValues();
            return true;
        }

        private void calculateValues()
        {
            answer = 0;
            for (int i = 0; i < monomial.Count; i++)
            {
                double temp;
                if (monomial[i].Item2 != String.Empty) temp = monomial[i].Item1 * (double)values[i]; else temp = monomial[i].Item1;
                if (operands[i] == '+') answer += temp; else answer -= temp;
            }
        }
    }
}
