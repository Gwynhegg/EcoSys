using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

namespace EcoSys.Entities
{
    public static class EasyEquationParser
    {
        public static double getRatioFromString(string equation)        //Простой парсер для преобразования уравнения модели в матматическое выражение
        {
            double result;
            string[] arg = equation.Split('=', '*');        //Пытаемся выделить коэффициент...
                if (Double.TryParse(arg[1], out result)) return result;     //Если он есть - возвращаем
                else
                    if (arg[1][0] == '-') return -1; else return 1;     //Если коэффициент отсутствует, то он равен единице
        }
    }
    public class EquationsParser        //Сложный парсер для выражений линейной зависимости
    {
        public List<(double, string)> monomial { get; set; } = new List<(double, string)>();        //Лист для хранения одночленов, участвующих в уравнении
        public List<char> operands { get; set; } = new List<char>();        //Лист для хранения мат. операций
        public double? answer { get; set; } = null;     //рассчитанный с помощью значений ответ
        public double?[] values { get; set; }       //Массив для хранения определенных значений определенных переменных
        public void createValuesArray()     //инициализация массива значений переменных
        {
            values = new double?[monomial.Count];
        }

        public string getAnswerToString()       //Преобразование ответа в текстовое значение, для упрощения объединенный с округлением
        {
            double temp_answer = 0;
            if (answer != null) temp_answer = Math.Round((double)answer, 3); else return null;
            return temp_answer.ToString();
        }

        public bool tryCalculateValue()     //попытка вычисления ответа
        {
            for (int i = 0; i < monomial.Count; i++)
                if (!monomial[i].Item2.Equals(String.Empty) && values[i] is null) return false;     //Выражение вычисляется только тогда, когда все значения переменных заполнены, т.е. != null
            calculateValues();      //Переходим к вычислению
            return true;        //Оповещаем об успешности операции
        }

        private void calculateValues()      //Метод вычисления ответа
        {
            answer = 0;
            for (int i = 0; i < monomial.Count; i++)
            {
                double temp;
                if (monomial[i].Item2 != String.Empty) temp = monomial[i].Item1 * (double)values[i]; else temp = monomial[i].Item1;         //Здесь высчитывается определенный одночлен
                if (operands[i] == '+') answer += temp; else answer -= temp;        //Одночлен суммируется или минусуется к ответу в зависимости от операнды
            }
        }
    }
}
