using System;
using System.Collections.Generic;
using System.Text;

namespace EcoSys.Entities
{
    public class EquationsParser
    {
        public List<(double, string)> monomial { get; set; } = new List<(double, string)>();
        public List<char> operands { get; set; } = new List<char>();
        public double answer { get; set; }
        public double?[] values { get; set; }
        public void createValuesArray()
        {
            values = new double?[monomial.Count];
            for (int i = 0; i < monomial.Count; i++)
                values[i] = null;
        }

        public string getAnswer()
        {
            var temp_answer = Math.Round(answer, 3);
            return temp_answer.ToString();
        }

        public bool tryCalculateValue()
        {
            for (int i = 0; i < monomial.Count; i++)
                if (values[i] == null) return false;
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
