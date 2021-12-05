using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Linq;
using System.Diagnostics;

namespace EcoSys.Grids
{
    /// <summary>
    /// Логика взаимодействия для EquationHolder.xaml
    /// </summary>
    public partial class EquationHolder : UserControl
    {

        Entities.EquationsParser parser = new Entities.EquationsParser();
        List<string> tips = new List<string>();
        public EquationHolder(string category, string equation, List<string> tips)
        {
            InitializeComponent();
            this.tips = tips;
            category_name.Text = category;
            createRatioAndValuesBundle(equation);
            createPlaceholders();
        }

        public double getAnswer()
        {
            return parser.answer;
        }

        private void createRatioAndValuesBundle(string equation)
        {
            string[] result = equation.Split('=','*','+','-');
            for (int i = 0; i < result.Length; i++)
            {
                double number;
                if (Double.TryParse(result[i], out number))
                    if (i + 1 < result.Length)
                    {
                        if (result[i + 1].Contains("X"))
                            parser.monomial.Add((number, result[i + 1]));
                        else parser.monomial.Add((number, String.Empty));
                    }
                    else parser.monomial.Add((number, String.Empty));
            }
            foreach (char c in equation)
                if (c == '+' || c == '-') parser.operands.Add(c);
            if (parser.operands.Count != parser.monomial.Count) parser.operands.Insert(0, '+');
            parser.createValuesArray();
        }

        private void createPlaceholders()
        {
            for (int i = 0; i < parser.monomial.Count; i++)
            {
                TextBlock ratio = new TextBlock() { FontSize = 18};
                if (parser.operands[i] == '-' && i == 0) ratio.Text += '-';
                else if (i != 0) ratio.Text += " " + parser.operands[i] + " ";
                ratio.Text += parser.monomial[i].Item1;
                if (parser.monomial[i].Item2 != String.Empty) {
                    ratio.Text += " * ";
                    equation_panel.Children.Add(ratio);
                    TextBox value = new TextBox() { FontSize = 18, MinWidth = 40, Opacity = 0.7, HorizontalAlignment = HorizontalAlignment.Center, Name = "Value" + i};
                    value.GotFocus += fieldGotFocus;
                    value.TextChanged += fieldTextChanged;
                    value.Text = parser.monomial[i].Item2;
                    value.ToolTip = new ToolTip() { Content = tips.Find(x => x.Contains(parser.monomial[i].Item2))};
                    equation_panel.Children.Add(value);
                } else
                    equation_panel.Children.Add(ratio);
            }

            TextBlock finish = new TextBlock() { Text = " = Y" , FontSize = 18, Name = "ResultText"};
            equation_panel.Children.Add(finish);
        }

        private void fieldGotFocus(object sender, RoutedEventArgs e)
        {
            (sender as TextBox).Text = "";
        }

        private void fieldTextChanged(object sender, RoutedEventArgs e)
        {
            double number;
            if (Double.TryParse((sender as TextBox).Text, out number))
            {
                (sender as TextBox).Opacity = 1;
                tryToParseEquation();
                if (parser.tryCalculateValue()) (this.equation_panel.Children[equation_panel.Children.Count - 1] as TextBlock).Text = " = " + parser.getAnswer();
            }
            else
            {
                if ((sender as TextBox).Text != String.Empty && !(sender as TextBox).Text.Contains("X")) 
                {
                    MessageBox.Show("Введено некорректное значение");
                    (sender as TextBox).Text = string.Empty;
                    (sender as TextBox).Opacity = 0.7;
                }

            }
        }

        private void tryToParseEquation()
        {
            string[] values = this.equation_panel.Children.OfType<TextBox>().Select(t => t.Text).ToArray();
            for (int i = 0; i < values.Length; i++)
            {
                double result;
                if (Double.TryParse(values[i], out result)) parser.values[i] = result;
            }
        }
    }
}
