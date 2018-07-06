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

// Edit with https://html5-editor.net/

namespace Jekyll_post_editor
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    ///     
    public partial class MainWindow : Window
    {
        private static string DATE_FORMAT = "yyyy-MM-dd";
        private string TEMPLATE_DIR = "template/default.template";
        private string FRONT_MATTER_UI_NAME = "tb_front_matter_element_";
        private int current_fm_row = 0;

        Dictionary<string, string> fmDictionary = new Dictionary<string, string>();
        Dictionary<int, string> fmDictionary_id = new Dictionary<int, string>();
        public MainWindow()
        {
            InitializeComponent();

            this.Title = "Jekyll post generator";
            dp_post_date.SelectedDate = DateTime.Today;
            tb_html_content.VerticalScrollBarVisibility = ScrollBarVisibility.Auto;
            Generate_Front_Matter_UI(TEMPLATE_DIR);
        }

        private void Window_MouseMove(object sender, MouseEventArgs e)
        {
            Title = e.GetPosition(this).ToString();
        }

        private void Button_Click_Exit(object sender, RoutedEventArgs e)
        {
            Close();
        }

        //************ SAVE FUNCTIONS ************

        // Save post files
        private void Button_Click_Save(object sender, RoutedEventArgs e)
        {
            // Check to make sure that user changed default content
            if (tb_title.Text == FindResource("strDefaultTitle").ToString())
            {
                if (MessageBox.Show("Are you sure you changed the title ?", "Default value detected !", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.No)
                {
                    return;
                }
            }
            if (tb_html_content.Text == FindResource("strDefaultHTMLContent").ToString())
            {
                if (MessageBox.Show("Are you sure you changed post html content ?", "Default value detected !", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.No)
                {
                    return;
                }
            }

            // Saving..
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.Filter = "Markdown files (*.md)|*.md|(*.markdown)|*.markdown|All files (*.*)|*.*";
            saveFileDialog.Title = "Save Post file";
            saveFileDialog.FileName = Jekyll_Name_Format(tb_title.Text, "title");
            saveFileDialog.ShowDialog();

            // Getting file with front matter
            string file_to_write = Generate_Front_Matter() + "\n" + tb_html_content.Text;
            if (saveFileDialog.FileName != "")
            {
                try
                {
                    File.WriteAllText(saveFileDialog.FileName, file_to_write);
                    Notify_Status("Save Successful", true);
                }
                catch (Exception ex)
                {
                    Notify_Status("Failed to save !!", false);
                    Console.WriteLine("Exception Caught !! {0}", ex);
                }

            }
        }

        //************ OPEN FUNCTIONS ************
        // Open post file
        private void Button_Click_Open(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "All files (*.*)|*.*|Markdown files (*.md)|*.md|(*.markdown)|*.markdown";
            if (openFileDialog.ShowDialog() == true)
            {
                string rawName = openFileDialog.SafeFileName;
                try
                {
                    if (rawName[10] == '-')
                    {
                        string front_matter = null;
                        string post_content = null;
                        tb_title.Text = Jekyll_Name_Format(rawName.Substring(11, rawName.Length - 11), "simple");
                        //tb_html_content.Text = File.ReadAllText(openFileDialog.FileName);
                        var raw_file = File.ReadAllText(openFileDialog.FileName);

                        Markdown_Breakdown(raw_file, out front_matter, out post_content);
                        tb_html_content.Text = post_content;
                        Load_Front_Matter(front_matter);

                        Notify_Status("Open file Successful", true);

                        if (DateTime.TryParseExact(rawName.Substring(0, 10), DATE_FORMAT, new CultureInfo("en-US"), DateTimeStyles.None, out DateTime postDate))
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

        //************ ULTILITIES FUNCTIONS ************

        // convert string to Jekyll post format
        // mode 1: title
        // mode 2: content
        private string Jekyll_Name_Format(string content, string mode)
        {
            string formatted_content = null;
            Regex pattern;
            switch (mode)
            {
                case "title":
                    // replace all space with '-'
                    pattern = new Regex(" ");
                    formatted_content = pattern.Replace(dp_post_date.SelectedDate.Value.ToString(DATE_FORMAT) + " " + content.Trim(), "-");
                    break;
                case "content":
                    break;
                case "simple":
                    int index = content.LastIndexOf('.');
                    pattern = new Regex("-");
                    formatted_content = pattern.Replace(content.Substring(0, index), " ");
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

        private void tb_html_content_TextChanged(object sender, TextChangedEventArgs e)
        {
            // Null check as HTML navigate to null
            if (string.IsNullOrEmpty(tb_html_content.Text))
            {
                tb_html_content.Text = " ";
            }
            wb_HTML_preview.NavigateToString(tb_html_content.Text);
        }

        private void Generate_Front_Matter_UI(string fm_file_path)
        {
            try
            {
                int i = 0;
                foreach (string line in File.ReadLines(fm_file_path))
                {
                    string[] keyvalue = line.Split(':');
                    if (keyvalue.Length == 2)
                    {
                        fmDictionary.Add(keyvalue[0], keyvalue[1]);
                        fmDictionary_id.Add(i, keyvalue[0]);

                        switch (keyvalue[1])
                        {
                            case "<custom>":
                                Add_Front_Matter_UI(keyvalue[0], keyvalue[1], i);
                                break;
                            case "<default-date>":
                                //TODO
                                //fmDictionary[fmDictionary_id[i]] = "123123";
                                break;
                        }
                        i++;
                    }
                }
            }
            catch (Exception ex)
            {
                Notify_Status("Failed to load Frontmatter", false);
                Console.WriteLine("Exception Caught !! {0}", ex);
            }
        }

        private void Add_Front_Matter_UI(string text, string textbox, int id)
        {
            TextBlock txt_temp = new TextBlock();
            TextBox tb_temp = new TextBox();

            // Label text
            txt_temp.Text = text;
            txt_temp.FontWeight = FontWeights.Bold;
            txt_temp.Margin = new Thickness(5, 0, 5, 0);
            txt_temp.SetValue(Grid.RowProperty, current_fm_row);
            txt_temp.SetValue(Grid.ColumnProperty, 0);

            // Text box
            tb_temp.Name = FRONT_MATTER_UI_NAME + id;
            tb_temp.SetValue(Grid.RowProperty, current_fm_row);
            tb_temp.SetValue(Grid.ColumnProperty, 1);

            // attach to UI            
            g_front_mater.Children.Add(txt_temp);
            g_front_mater.Children.Add(tb_temp);
            g_front_mater.RegisterName(tb_temp.Name, tb_temp);

            // Create new row
            g_front_mater.RowDefinitions.Add(new RowDefinition());
            current_fm_row++;
        }

        private string Generate_Front_Matter()
        {
            // Getting data from the box
            TextBox tb_temp;
            foreach (KeyValuePair<int, string> kvp in fmDictionary_id)
            {
                tb_temp = (TextBox)g_front_mater.FindName(FRONT_MATTER_UI_NAME + kvp.Key);
                if (tb_temp != null)
                {
                    //tb_temp = (TextBox)g_front_mater.FindName(FRONT_MATTER_UI_NAME + kvp.Key);
                    fmDictionary[fmDictionary_id[kvp.Key]] = tb_temp.Text;
                }

                // replace default date
                if (fmDictionary[fmDictionary_id[kvp.Key]] == "<default-date>")
                {
                    fmDictionary[fmDictionary_id[kvp.Key]] = dp_post_date.SelectedDate.Value.ToString(DATE_FORMAT);
                }

            }

            // Generating the front matter
            string temp = "---\n";
            foreach (KeyValuePair<string, string> kvp in fmDictionary)
            {
                // generate the string
                temp = temp + kvp.Key + ": " + kvp.Value + "\n";
            }
            temp = temp + "---\n";
            return temp;
        }

        private void Load_Front_Matter(string front_matter_string)
        {
            foreach (string line in front_matter_string.Split('\n'))
            {
                TextBox tb_temp;
                string[] keyvalue = line.Split(new char[] { ':' }, 2);
                if (keyvalue.Length == 2)
                {
                    // populate corresponding textbox with loaded value
                    var id = fmDictionary_id.First(x => x.Value.Contains(keyvalue[0])).Key;
                    tb_temp = (TextBox)g_front_mater.FindName(FRONT_MATTER_UI_NAME + id);
                    if (tb_temp != null)
                    {
                        tb_temp.Text = keyvalue[1].TrimStart();
                    }

                }
            }
        }

        private void Markdown_Breakdown(string rawfile, out string front_matter, out string post_content)
        {
            string[] breakdown_string = Regex.Split(rawfile, "---");

            front_matter = breakdown_string[1];
            post_content = breakdown_string[2].TrimStart();
        }

    }
}
