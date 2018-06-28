using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Jekyll_post_editor
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private static string DATE_FORMAT = "yyyy-MM-dd-";

        public MainWindow()
        {
            InitializeComponent();

            this.Title = "Jekyll post generator";
            dp_post_date.SelectedDate = DateTime.Today;
        }

        private void Window_MouseMove(object sender, MouseEventArgs e)
        {
            Title = e.GetPosition(this).ToString();
        }

        private void Button_Click_Cancel(object sender, RoutedEventArgs e)
        {
            Close();
        }

        // Save post files
        private void Button_Click_Save(object sender, RoutedEventArgs e)
        {

            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.Filter = "Markdown files (*.md)|*.md|(*.markdown)|*.markdown|All files (*.*)|*.*";
            saveFileDialog.Title = "Save Post file";
            saveFileDialog.FileName = Jekyll_Name_Format(tb_title.Text, "title");
            saveFileDialog.ShowDialog();

            if (saveFileDialog.FileName != "")
            {
                try
                {
                    File.WriteAllText(saveFileDialog.FileName, tb_html_content.Text);
                    Notify_Status("Save Successful", true);
                }
                catch (Exception ex)
                {
                    Notify_Status("Failed to save !!", false);
                    Console.WriteLine("Exception Caught !! {0}", ex);
                }

            }
        }

        // Open post file
        private void Button_Click_Open(object sender, RoutedEventArgs e)
        {
            //tb_title.Text = "Open";
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Markdown files (*.md)|*.md|(*.markdown)|*.markdown|All files (*.*)|*.*";
            if (openFileDialog.ShowDialog() == true)
            {
                string rawName = openFileDialog.SafeFileName;
                try
                {
                    if (rawName[10] == '-')
                    {
                        tb_title.Text = Jekyll_Name_Format(rawName.Substring(11, rawName.Length - 11), "simple");
                        tb_html_content.Text = File.ReadAllText(openFileDialog.FileName);

                        Notify_Status("Open file Successful", true);

                        if (DateTime.TryParseExact(rawName.Substring(0, 11), DATE_FORMAT, new CultureInfo("en-US"), DateTimeStyles.None, out DateTime postDate))
                        {
                            dp_post_date.SelectedDate = postDate;
                        }
                        else
                        {
                            Notify_Status("Failed to load Date", false);
                        }

                    }
                    else
                    {
                        Notify_Status("Not a correct Jekyll format files !!!", false);
                        return;
                    }
                }
                catch (IndexOutOfRangeException ex)
                {
                    Console.WriteLine(ex.ToString());
                    Notify_Status("Not a correct Jekyll format files !!!", false);
                    return;
                }
            }
        }

        // convert string to Jekyll post format
        // mode 1: title
        // mode 2: content
        private string Jekyll_Name_Format(string content, string mode)
        {
            string formatted_content = null;
            switch (mode)
            {
                case "title":
                    Regex pattern = new Regex(" ");
                    formatted_content = pattern.Replace(dp_post_date.SelectedDate.Value.ToString(DATE_FORMAT) + content, "-");
                    break;
                case "content":
                    break;
                case "simple":
                    int index = content.LastIndexOf('.');
                    formatted_content = content.Substring(0, index);
                    break;
            }
            return formatted_content;
        }

        private void Notify_Status(string message, bool isPassed)
        {
            if (!isPassed)
            {
                MessageBox.Show(message);
                tb_status.Foreground = Brushes.Red;
                tb_status.Text = (message);
                Reset_View();
            }
            else
            {
                tb_status.Foreground = Brushes.Green;
                tb_status.Text = (message);
            }

        }

        private void Reset_View()
        {
            tb_title.Text = FindResource("strDefaultTitle").ToString();
            tb_html_content.Text = FindResource("strDefaultHTMLContent").ToString();
            dp_post_date.SelectedDate = DateTime.Today;
        }
    }
}
