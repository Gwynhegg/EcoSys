using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace EcoSys.Grids
{
    /// <summary>
    /// Логика взаимодействия для EquationHolder.xaml
    /// </summary>
    public partial class EquationHolder : UserControl
    {
        private readonly Block5 parent_block;
        public Entities.EquationsParser parser { get; } = new Entities.EquationsParser();
        private readonly List<string> tips = new List<string>();
        public EquationHolder(Block5 parent, string category, string equation, List<string> tips)
        {
            InitializeComponent();
            parent_block = parent;
            this.tips = tips;
            category_name.Text = category;
            createRatioAndValuesBundle(equation);
            createPlaceholders();
        }
        public byte getUsedCategory() => Byte.Parse(category_name.Text.Split('.')[0]);
        private void createRatioAndValuesBundle(string equation)
        {
            string[] result = equation.Split('=', '*', '+', '-');
            for (int i = 0; i < result.Length; i++)
            {
                if (Double.TryParse(result[i], out double number))
                {
                    if (i + 1 < result.Length)
                    {
                        if (result[i + 1].Contains("X"))
                            this.parser.monomial.Add((number, result[i + 1]));
                        else
                            this.parser.monomial.Add((number, String.Empty));
                    }
                    else
                        this.parser.monomial.Add((number, String.Empty));
                }
            }
            foreach (char c in equation)
            {
                if (c == '+' || c == '-')
                    this.parser.operands.Add(c);
            }

            if (this.parser.operands.Count != this.parser.monomial.Count)
                this.parser.operands.Insert(0, '+');

            this.parser.createValuesArray();
        }
        private void createPlaceholders()
        {
            for (int i = 0; i < this.parser.monomial.Count; i++)
            {
                TextBlock ratio = new TextBlock() { FontSize = 18 };
                if (this.parser.operands[i] == '-' && i == 0)
                    ratio.Text += '-';
                else if (i != 0)
                    ratio.Text += " " + this.parser.operands[i] + " ";

                ratio.Text += this.parser.monomial[i].Item1;
                if (this.parser.monomial[i].Item2 != String.Empty)
                {
                    ratio.Text += " * ";
                    equation_panel.Children.Add(ratio);
                    TextBox value = new TextBox() { FontSize = 18, MinWidth = 40, Opacity = 0.7, HorizontalAlignment = HorizontalAlignment.Center, Name = "Value" + i };
                    value.GotFocus += fieldGotFocus;
                    value.TextChanged += fieldTextChanged;
                    value.Text = this.parser.monomial[i].Item2;
                    value.ToolTip = new ToolTip() { Content = tips.Find(x => x.Contains(this.parser.monomial[i].Item2)) };
                    equation_panel.Children.Add(value);
                }
                else
                    equation_panel.Children.Add(ratio);
            }

            TextBlock finish = new TextBlock() { Text = " = Y", FontSize = 18, Name = "ResultText" };
            equation_panel.Children.Add(finish);
        }
        private void tryToParseEquation()
        {
            string[] values = equation_panel.Children.OfType<TextBox>().Select(t => t.Text).ToArray();
            for (int i = 0; i < values.Length; i++)
            {
                if (Double.TryParse(values[i], out double result))
                {
                    if (values.Length == this.parser.values.Length)
                        this.parser.values[i] = result;
                    else
                        this.parser.values[i + 1] = result;
                }
            }
        }
        public void clearValues()
        {
            this.parser.answer = null;
            this.parser.createValuesArray();
            equation_panel.Children.Clear();
            createPlaceholders();
        }
        private void fieldGotFocus(object sender, RoutedEventArgs e) => (sender as TextBox).Text = "";
        private void fieldTextChanged(object sender, RoutedEventArgs e)
        {
            if (Double.TryParse((sender as TextBox).Text, out double number))
            {
                (sender as TextBox).Opacity = 1;
                tryToParseEquation();
                if (this.parser.tryCalculateValue())
                {
                    (equation_panel.Children[equation_panel.Children.Count - 1] as TextBlock).Text = " = " + this.parser.getAnswerToString();
                    parent_block.checkFields();
                }
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
    }
}
