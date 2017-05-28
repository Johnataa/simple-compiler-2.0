﻿using Microsoft.Win32;
using SimpleCompilerService.Analisador;
using System;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Input;

namespace SimpleCompilerPresentation
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private string FilePath;
        public MainWindow()
        {
            InitializeComponent();
            FilePath = null;
        }

        #region 1. Interações com a Tela
        private void BtnCompilar_Click(object sender, RoutedEventArgs e)
        {
            var teste = new TextRange(CodigoFonte.Document.ContentStart, CodigoFonte.Document.ContentEnd).Text;
            Lexico.ScanText(teste);
            if (Lexico.ContemErroLexico)
            {
                var result = MessageBox.Show("A análise encontrou erros léxicos!\nDeseja ver os erros?", "Erro Léxico!", MessageBoxButton.YesNo, MessageBoxImage.Question, MessageBoxResult.Yes);
                if (result == MessageBoxResult.Yes)
                {
                    foreach (var item in Lexico.Tokens.Where(t => t.Tag == SimpleCompilerService.Suporte.Tag.ERRO_LEXICO))
                    {
                        MessageBox.Show(item.ToString(), "Token");
                    }
                }
            }
            else
            {
                MessageBox.Show("Sucesso na análise Léxica!", "Sucesso!");
            }
        }

        private void BtnAbrir_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFile = new OpenFileDialog();
            openFile.Filter = "Arquivo LALG |*.lalg";
            openFile.Title = "Abrir arquivo";

            if (openFile.ShowDialog() == true)
            {
                var op = openFile.OpenFile();
                FilePath = openFile.FileName;
                try
                {
                    using (StreamReader sr = new StreamReader(FilePath))
                    {
                        String texto = sr.ReadToEnd();
                        sr.Close();
                        LoadSourceCode(texto);
                        ModifyFilename();
                        ModifyTextIcon("checked");
                    }
                }
                catch (Exception)
                {
                    MessageBox.Show("Falha ao ler o arquivo, tente novamente.", "Erro!", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void BtnSalvar_Click(object sender, RoutedEventArgs e)
        {
            if (FilePath != null)
            {
                string texto = GetSourceCodeText();
                FileStream fs = new FileStream(FilePath, FileMode.Create);
                StreamWriter writer = new StreamWriter(fs);
                writer.Write(texto);
                writer.Close();
                ModifyFilename();
                ModifyTextIcon("checked");
            }
            else
            {
                BtnSalvarComo_Click(sender, e);
            }
            
        }

        private void BtnSalvarComo_Click(object sender, RoutedEventArgs e)
        {
            string texto = GetSourceCodeText();
            SaveFileDialog sfd = new SaveFileDialog();
            sfd.Filter = "Arquivo LALG |*.lalg";
            sfd.Title = "Salvar arquivo como";
            if (sfd.ShowDialog() == true)
            {
                FileStream fs = new FileStream(sfd.FileName, FileMode.Create);
                StreamWriter writer = new StreamWriter(fs);
                writer.Write(texto);
                writer.Close();
                FilePath = sfd.FileName;
                ModifyFilename();
                ModifyTextIcon("checked");
            }
        }

        private void CodigoFonte_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            ModifyTextIcon("blank");
        }
        #endregion

        #region 2. Interações com atalhos
        public void Executed_Open(object sender, ExecutedRoutedEventArgs e)
        {
            BtnAbrir_Click(sender, e);
        }

        public void Executed_Save(object sender, ExecutedRoutedEventArgs e)
        {
            BtnSalvar_Click(sender, e);
        }

        private void Executed_SaveAs(object sender, ExecutedRoutedEventArgs e)
        {
            BtnSalvarComo_Click(sender, e);
        }
        #endregion

        #region 3. Métodos privados
        private void LoadSourceCode(string texto)
        {
            CodigoFonte.Document.Blocks.Clear();
            Paragraph p;
            var lines = texto.Split('\n');
            if (texto.Contains("\r"))
            {
                lines = texto.Split(new[] { "\r\n" }, StringSplitOptions.None);
            }
            foreach (var item in lines)
            {
                int tab = 0;
                foreach (var c in item)
                {
                    if (c == '\t')
                    {
                        tab++;
                    }
                    else
                    {
                        break;
                    }
                }
                var text = item.Substring(tab);
                p = new Paragraph(new Run(text));
                p.TextIndent = 20 * tab;
                CodigoFonte.Document.Blocks.Add(p);
            }
        }
        private string GetSourceCodeText()
        {
            var block = CodigoFonte.Document.Blocks.FirstBlock;
            string texto = "";
            while (block != null)
            {
                var totalTabs = ((Paragraph)block).TextIndent / 20;
                for (int i = 0; i < totalTabs; i++)
                {
                    texto += "\t";
                }
                var line = new TextRange(block.ContentStart, block.ContentEnd).Text;
                texto += line + "\n";
                block = block.NextBlock;
            }
            return texto.Remove(texto.Length-1);
        }

        private void ModifyTextIcon(string icon)
        {
            if (icon.ToLower() == "blank")
            {
                CheckFile.Kind = MaterialDesignThemes.Wpf.PackIconKind.CheckboxMultipleBlankOutline;
            }
            else
            {
                CheckFile.Kind = MaterialDesignThemes.Wpf.PackIconKind.CheckboxMultipleMarkedOutline;
            }
            CheckFile.Visibility = Visibility.Visible;
        }

        private void ModifyFilename()
        {
            Filename.Visibility = Visibility.Visible;
            Filename.Text = Path.GetFileName(FilePath);
        }
        #endregion

    }
}