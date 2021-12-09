using System;
using System.Collections.Generic;

namespace EcoSys.Auxiliary
{
    public static class EasyEquationParser
    {
        public static double getRatioFromString(string equation)        //Простой парсер для преобразования уравнения модели в матматическое выражение
        {
            string[] arg = equation.Split('=', '*');        //Пытаемся выделить коэффициент...
            if (Double.TryParse(arg[1], out double result))
                return result;     //Если он есть - возвращаем
            else
                    if (arg[1][0] == '-')
                return -1;
            else
                return 1;     //Если коэффициент отсутствует, то он равен единице
        }
    }
    public class EquationsParser        //Сложный парсер для выражений линейной зависимости
    {
        public List<(double, string)> monomial { get; set; } = new List<(double, string)>();        //Лист для хранения одночленов, участвующих в уравнении
        public List<char> operands { get; set; } = new List<char>();        //Лист для хранения мат. операций
        public double? answer { get; set; } = null;     //рассчитанный с помощью значений ответ
        public double?[] values { get; set; }       //Массив для хранения определенных значений определенных переменных
        public void createValuesArray()     //инициализация массива значений переменных
=> this.values = new double?[this.monomial.Count];

        public string getAnswerToString()       //Преобразование ответа в текстовое значение, для упрощения объединенный с округлением
        {
            double temp_answer = 0;
            if (this.answer != null)
                temp_answer = Math.Round((double)this.answer, 3);
            else
                return null;

            return temp_answer.ToString();
        }

        public bool tryCalculateValue()     //попытка вычисления ответа
        {
            for (int i = 0; i < this.monomial.Count; i++)
            {
                if (!this.monomial[i].Item2.Equals(String.Empty) && this.values[i] is null)
                    return false;     //Выражение вычисляется только тогда, когда все значения переменных заполнены, т.е. != null
            }

            calculateValues();      //Переходим к вычислению
            return true;        //Оповещаем об успешности операции
        }

        private void calculateValues()      //Метод вычисления ответа
        {
            this.answer = 0;
            for (int i = 0; i < this.monomial.Count; i++)
            {
                double temp;
                if (this.monomial[i].Item2 != String.Empty)
                    temp = this.monomial[i].Item1 * (double)this.values[i];
                else
                    temp = this.monomial[i].Item1;         //Здесь высчитывается определенный одночлен

                if (this.operands[i] == '+')
                    this.answer += temp;
                else
                    this.answer -= temp;        //Одночлен суммируется или минусуется к ответу в зависимости от операнды
            }
        }
    }
}
