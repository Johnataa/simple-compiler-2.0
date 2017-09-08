using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace SimpleCompilerPresentation
{
    /// <summary>
    /// Lógica interna para Instrucoes.xaml
    /// </summary>
    public partial class Instrucoes : Window
    {
        public Instrucoes(List<string> c)
        {
            InitializeComponent();
            foreach (var item in c.Select((value, i) => new { i, value }))
            {
                ListInstrucoes.Items.Add(new ListViewItem { Content = item.i + ". " + item.value });
            }
            
        }
    }
}
