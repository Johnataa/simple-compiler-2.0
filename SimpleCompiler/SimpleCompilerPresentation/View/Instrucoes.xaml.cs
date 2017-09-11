using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace SimpleCompiler
{
    /// <summary>
    /// Lógica interna para Instrucoes.xaml
    /// </summary>
    public partial class Instrucoes : Window
    {
        public Instrucoes()
        {
            InitializeComponent();
            var c = MaquinaHipotetica.GetInstance().C;
            foreach (var item in c.Select((value, i) => new { i, value }))
            {
                ListInstrucoes.Items.Add(new ListViewItem { Content = item.i + ". " + item.value });
            }
            
        }
    }
}
