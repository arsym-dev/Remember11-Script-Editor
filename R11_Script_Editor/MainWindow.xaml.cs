using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
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
using System.IO;
using Microsoft.WindowsAPICodePack.Dialogs;

using R11_Script_Editor.Tokens;
using R11_Script_Editor.FileTypes;
using System.Configuration;

namespace R11_Script_Editor
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        ObservableCollection<ListViewItem> lvList = new ObservableCollection<ListViewItem>();
        string folder = "";
        string filename = "";
        Configuration config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);

        public MainWindow()
        {
            InitializeComponent();

            listviewFiles.ItemsSource = lvList;
            this.Closing += MainWindow_Closing;

            textbox_inputFolder.Text = GetConfig("input_folder");
            textbox_inputFolderJp.Text = GetConfig("input_folder_jp");
            textbox_listFile.Text = GetConfig("list_file");
            textbox_exportedAfs.Text = GetConfig("exported_afs");
            checkbox_SearchCaseSensitive.IsChecked = GetConfig("case_sensitive") == "1";
            checkbox_SearchAllFiles.IsChecked = GetConfig("search_all_files") == "1";
            textbox_search.Text = GetConfig("last_search");

            MenuViewFolder.IsChecked = GetConfig("view_folders", "1") == "1";
            if (!(bool)MenuViewFolder.IsChecked)
                GridTextboxes.Visibility = Visibility.Collapsed;
            MenuViewDescription.IsChecked = GetConfig("view_description", "1") == "1";
            MenuViewLabel.IsChecked = GetConfig("view_label", "1") == "1";

            textbox_inputFolder.TextChanged += (sender, ev) => { UpdateConfig("input_folder", textbox_inputFolder.Text); BrowseInputFolder(null, null); };
            textbox_inputFolderJp.TextChanged += (sender, ev) => UpdateConfig("input_folder_jp", textbox_inputFolderJp.Text);;
            textbox_listFile.TextChanged += (sender, ev) => UpdateConfig("list_file", textbox_listFile.Text);
            textbox_exportedAfs.TextChanged += (sender, ev) => UpdateConfig("exported_afs", textbox_exportedAfs.Text);
            checkbox_SearchCaseSensitive.Checked += (sender, ev) => UpdateConfig("case_sensitive", "1");
            checkbox_SearchCaseSensitive.Unchecked += (sender, ev) => UpdateConfig("case_sensitive", "0");
            checkbox_SearchAllFiles.Checked += (sender, ev) => UpdateConfig("search_all_files", "1");
            checkbox_SearchAllFiles.Unchecked += (sender, ev) => UpdateConfig("search_all_files", "0");
            textbox_search.TextChanged += (sender, ev) => UpdateConfig("last_search", textbox_search.Text);
            textbox_search.KeyDown += Textbox_search_KeyDown;

            TokenSelectorComboBox.Items.Add(new TokenSelectorComboBoxItem("EffectEx"));
            TokenSelectorComboBox.Items.Add(new TokenSelectorComboBoxItem("End"));
            TokenSelectorComboBox.Items.Add(new TokenSelectorComboBoxItem("ExternalGoto"));
            TokenSelectorComboBox.Items.Add(new TokenSelectorComboBoxItem("FadeExStart"));
            TokenSelectorComboBox.Items.Add(new TokenSelectorComboBoxItem("FadeExWait"));
            TokenSelectorComboBox.Items.Add(new TokenSelectorComboBoxItem("FileRead"));
            TokenSelectorComboBox.Items.Add(new TokenSelectorComboBoxItem("FileWait"));
            TokenSelectorComboBox.Items.Add(new TokenSelectorComboBoxItem("GraphDisp"));
            TokenSelectorComboBox.Items.Add(new TokenSelectorComboBoxItem("GraphDispEx"));
            TokenSelectorComboBox.Items.Add(new TokenSelectorComboBoxItem("If"));
            TokenSelectorComboBox.Items.Add(new TokenSelectorComboBoxItem("InternalGoto"));
            TokenSelectorComboBox.Items.Add(new TokenSelectorComboBoxItem("MsgDisp2"));
            TokenSelectorComboBox.Items.Add(new TokenSelectorComboBoxItem("MsgType"));
            TokenSelectorComboBox.Items.Add(new TokenSelectorComboBoxItem("MsgView"));
            TokenSelectorComboBox.Items.Add(new TokenSelectorComboBoxItem("QuickSave"));
            TokenSelectorComboBox.Items.Add(new TokenSelectorComboBoxItem("RegCalc"));
            TokenSelectorComboBox.Items.Add(new TokenSelectorComboBoxItem("SelectDisp2"));
            TokenSelectorComboBox.Items.Add(new TokenSelectorComboBoxItem("SystemMsg"));
            TokenSelectorComboBox.Items.Add(new TokenSelectorComboBoxItem("TimeWait"));


            BrowseInputFolder(null, null);

            AddHotKeys();
            Application.Current.DispatcherUnhandledException += Current_DispatcherUnhandledException;
            //AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
        }

        private void MainWindow_Closing(object sender, CancelEventArgs e)
        {
            bool success = CheckUnsavedChanges();
            e.Cancel = !success;
        }
        
        private void Current_DispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
        {
            var x = new ExceptionPopup(e.Exception);
            x.ShowDialog();
            e.Handled = true;
        }

        private void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            MessageBox.Show(e.ExceptionObject.ToString());
        }

        private bool CheckUnsavedChanges()
        {
            // Check for file changes, then prompt user to save
            if (Tokenizer.ChangedFile)
            {
                MessageBoxResult dialogResult = MessageBox.Show("File changed. Save?", "Unsaved changes", MessageBoxButton.YesNoCancel);

                if (dialogResult == MessageBoxResult.Yes)
                    Tokenizer.SaveFile(System.IO.Path.Combine(folder, filename));
                else if (dialogResult == MessageBoxResult.Cancel)
                    return false;

                Tokenizer.ChangedFile = false;
            }
            return true;
        }

        private void Textbox_search_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
                SearchNext(null, null);
        }

        private string GetConfig(string key, string initial = "")
        {
            if (config.AppSettings.Settings.AllKeys.Contains(key))
                return config.AppSettings.Settings[key].Value;

            return initial;
        }

        private void UpdateConfig(string key, string value)
        {
            
            config.AppSettings.Settings.Remove(key);
            config.AppSettings.Settings.Add(key, value);
            config.Save();
        }

        private void AddHotKeys()
        {
            RoutedCommand com;

            com = new RoutedCommand();
            com.InputGestures.Add(new KeyGesture(Key.S, ModifierKeys.Control));
            CommandBindings.Add(new CommandBinding(com, Menu_File_Save));

            com = new RoutedCommand();
            com.InputGestures.Add(new KeyGesture(Key.F, ModifierKeys.Control));
            CommandBindings.Add(new CommandBinding(com, SearchFocus));

            com = new RoutedCommand();
            com.InputGestures.Add(new KeyGesture(Key.E, ModifierKeys.Control));
            CommandBindings.Add(new CommandBinding(com, Menu_Export_Mac));

            com = new RoutedCommand();
            com.InputGestures.Add(new KeyGesture(Key.F3, ModifierKeys.None));
            CommandBindings.Add(new CommandBinding(com, SearchNext));

            com = new RoutedCommand();
            com.InputGestures.Add(new KeyGesture(Key.F4, ModifierKeys.None));
            CommandBindings.Add(new CommandBinding(com, SearchPrev));

            com = new RoutedCommand();
            com.InputGestures.Add(new KeyGesture(Key.F5, ModifierKeys.None));
            CommandBindings.Add(new CommandBinding(com, FocusTextNext));

            com = new RoutedCommand();
            com.InputGestures.Add(new KeyGesture(Key.F6, ModifierKeys.None));
            CommandBindings.Add(new CommandBinding(com, FocusTextPrev));
        }

        private void BrowseInputFolder(object sender, RoutedEventArgs e)
        {
            if (sender == null)
            {
                folder = textbox_inputFolder.Text;
                if (!Directory.Exists(folder))
                {
                    lvList.Clear();
                    return;
                }
            }

            else
            {
                CommonOpenFileDialog dialog = new CommonOpenFileDialog();
                dialog.IsFolderPicker = true;
                if (dialog.ShowDialog() == CommonFileDialogResult.Ok)
                    textbox_inputFolder.Text = dialog.FileName;
                return; // Return because changing the textbox will trigger this function again
            }
            
            lvList.Clear();

            // Populate the list
            
            string[] filenames = Directory.GetFiles(folder, "*.CMD", SearchOption.TopDirectoryOnly);
            foreach (var filename in filenames)
                lvList.Add(new ListViewItem() { Content = System.IO.Path.GetFileName(filename) });

            // Clicking the file name will load the file
            listviewFiles.SelectionChanged += ListViewFiles_SelectionChanged;
        }

        private void BrowseInputFolderJp(object sender, RoutedEventArgs e)
        {
            CommonOpenFileDialog dialog = new CommonOpenFileDialog();
            dialog.IsFolderPicker = true;
            if (dialog.ShowDialog() == CommonFileDialogResult.Ok)
                textbox_inputFolderJp.Text = dialog.FileName;
        }

        private void BrowseFilelist(object sender, RoutedEventArgs e) {
            CommonOpenFileDialog dialog = new CommonOpenFileDialog();
            if (dialog.ShowDialog() == CommonFileDialogResult.Ok)
                textbox_listFile.Text = dialog.FileName;
        }

        private void BrowseExportedAfs(object sender, RoutedEventArgs e) {
            CommonOpenFileDialog dialog = new CommonOpenFileDialog();
            if (dialog.ShowDialog() == CommonFileDialogResult.Ok)
                textbox_exportedAfs.Text = dialog.FileName;
        }

        private void SearchFocus(object sender, RoutedEventArgs e)
        {
            textbox_search.Focus();
        }

        private void SearchNext(object sender, RoutedEventArgs e)
        {
            Search(true);
        }

        private void SearchPrev(object sender, RoutedEventArgs e)
        {
            Search(false);
        }

        private void AddNode(object sender, RoutedEventArgs e)
        {
            // @TODO
            if (ListView1.SelectedIndex > -1)
            {
                if (TokenSelectorComboBox.SelectedIndex > -1)
                {
                    var token_combobox_item = (TokenSelectorComboBox.SelectedItem as TokenSelectorComboBoxItem);
                    //var token = (Token) Activator.CreateInstance(token_combobox_item.Value);
                    var token = Activator.CreateInstance(token_combobox_item.Value, new object[] { (bool)true }) as Token;

                    int idx = ListView1.SelectedIndex + 1;
                    Tokenizer.Tokens.Insert(idx, token);
                    CommandViewBox vb = DataContext as CommandViewBox;
                    vb.MyListItems.Insert(idx, token);

                    ListView1.SelectedIndex += 1;
                }
                
            }

        }

        private void DeleteNode(object sender, RoutedEventArgs e)
        {
            if (ListView1.SelectedIndex > -1)
            {
                int idx = ListView1.SelectedIndex;
                ListView1.SelectedIndex += 1;

                Tokenizer.Tokens.RemoveAt(idx);
                CommandViewBox vb = DataContext as CommandViewBox;
                vb.MyListItems.RemoveAt(idx);
            }
        }

        private void AddFadeToBlack(object sender, RoutedEventArgs e)
        {
            if (ListView1.SelectedIndex > -1)
            {
                int idx = ListView1.SelectedIndex + 1;

                var t1 = new TokenFadeExStart(true);
                t1.Transition = 144;
                t1.Duration = 30;
                t1.Unknown3 = 4096;
                var t2 = new TokenFadeExWait(true);
                var t3 = new TokenGraphDisp(true);
                t3.Entries[0].ImageNumber = 256;
                t3.Entries[0].Unknown2 = 7935;
                t3.Entries[0].FileDescription = 4096;
                var t4 = new TokenFadeExStart(true);
                t4.Transition = 127;
                t4.Duration = 1;
                t4.Unknown3 = 4096;
                var t5 = new TokenFadeExWait(true);

                // =================
                t1.UpdateData();
                t2.UpdateData();
                t3.UpdateData();
                t4.UpdateData();
                t5.UpdateData();

                Tokenizer.Tokens.Insert(idx, t1);
                Tokenizer.Tokens.Insert(idx+1, t2);
                Tokenizer.Tokens.Insert(idx+2, t3);
                Tokenizer.Tokens.Insert(idx+3, t4);
                Tokenizer.Tokens.Insert(idx+4, t5);

                CommandViewBox vb = DataContext as CommandViewBox;

                vb.MyListItems.Insert(idx, t1);
                vb.MyListItems.Insert(idx+1, t2);
                vb.MyListItems.Insert(idx+2, t3);
                vb.MyListItems.Insert(idx+3, t4);
                vb.MyListItems.Insert(idx+4, t5);

                ListView1.SelectedIndex += 5;
            }
        }

        private void AddFadeFromBlack(object sender, RoutedEventArgs e)
        {
            if (ListView1.SelectedIndex > -1)
            {
                int idx = ListView1.SelectedIndex + 1;

                var t1 = new TokenFadeExStart(true);
                t1.Transition = 128;
                t1.Duration = 1;
                t1.Unknown3 = 0;
                var t2 = new TokenFadeExWait(true);
                var t3 = new TokenGraphDisp(true);
                t3.Entries[0].ImageNumber = 256;
                t3.Entries[0].Unknown2 = 7935;
                t3.Entries[0].FileDescription = 4100;
                var t4 = new TokenFadeExStart(true);
                t4.Transition = 127;
                t4.Duration = 60;
                t4.Unknown3 = 4096;
                var t5 = new TokenFadeExWait(true);

                // =================
                t1.UpdateData();
                t2.UpdateData();
                t3.UpdateData();
                t4.UpdateData();
                t5.UpdateData();

                Tokenizer.Tokens.Insert(idx, t1);
                Tokenizer.Tokens.Insert(idx + 1, t2);
                Tokenizer.Tokens.Insert(idx + 2, t3);
                Tokenizer.Tokens.Insert(idx + 3, t4);
                Tokenizer.Tokens.Insert(idx + 4, t5);

                CommandViewBox vb = DataContext as CommandViewBox;

                vb.MyListItems.Insert(idx, t1);
                vb.MyListItems.Insert(idx + 1, t2);
                vb.MyListItems.Insert(idx + 2, t3);
                vb.MyListItems.Insert(idx + 3, t4);
                vb.MyListItems.Insert(idx + 4, t5);

                ListView1.SelectedIndex += 5;
            }
        }

        private void AddFlashWhite(object sender, RoutedEventArgs e)
        {
            if (ListView1.SelectedIndex > -1)
            {
                int idx = ListView1.SelectedIndex + 1;

                var t1 = new TokenFadeExStart(true);
                t1.Transition = 144;
                t1.Duration = 30;
                t1.Unknown3 = 4095;
                var t2 = new TokenFadeExWait(true);
                var t3 = new TokenGraphDisp(true);
                t3.Entries[0].ImageNumber = 256;
                t3.Entries[0].Unknown2 = 7935;
                t3.Entries[0].FileDescription = 4100;
                var t4 = new TokenFadeExStart(true);
                t4.Transition = 127;
                t4.Duration = 30;
                t4.Unknown3 = 4096;
                var t5 = new TokenFadeExWait(true);

                // =================
                t1.UpdateData();
                t2.UpdateData();
                t3.UpdateData();
                t4.UpdateData();
                t5.UpdateData();

                Tokenizer.Tokens.Insert(idx, t1);
                Tokenizer.Tokens.Insert(idx + 1, t2);
                Tokenizer.Tokens.Insert(idx + 2, t3);
                Tokenizer.Tokens.Insert(idx + 3, t4);
                Tokenizer.Tokens.Insert(idx + 4, t5);

                CommandViewBox vb = DataContext as CommandViewBox;

                vb.MyListItems.Insert(idx, t1);
                vb.MyListItems.Insert(idx + 1, t2);
                vb.MyListItems.Insert(idx + 2, t3);
                vb.MyListItems.Insert(idx + 3, t4);
                vb.MyListItems.Insert(idx + 4, t5);

                ListView1.SelectedIndex += 5;
            }
        }

        private void AddGotoScript(object sender, RoutedEventArgs e)
        {
            if (ListView1.SelectedIndex > -1)
            {
                int idx = ListView1.SelectedIndex + 1;

                var t1 = new TokenFileRead(true);
                var t2 = new TokenFileWait(true);
                var t3 = new TokenRegCalc(true);
                var t4 = new TokenExternalGoto(true);

                // =================
                t1.UpdateData();
                t2.UpdateData();
                t3.UpdateData();
                t4.UpdateData();

                Tokenizer.Tokens.Insert(idx, t1);
                Tokenizer.Tokens.Insert(idx + 1, t2);
                Tokenizer.Tokens.Insert(idx + 2, t3);
                Tokenizer.Tokens.Insert(idx + 3, t4);

                CommandViewBox vb = DataContext as CommandViewBox;

                vb.MyListItems.Insert(idx, t1);
                vb.MyListItems.Insert(idx + 1, t2);
                vb.MyListItems.Insert(idx + 2, t3);
                vb.MyListItems.Insert(idx + 3, t4);

                ListView1.SelectedIndex += 4;
            }
        }

        private void AddStopMusic(object sender, RoutedEventArgs e)
        {
            if (ListView1.SelectedIndex > -1)
            {
                int idx = ListView1.SelectedIndex + 1;

                var t1 = new TokenBgmSpeed(true);
                t1.Unknown1 = 128;
                t1.Unknown2 = 825;
                var t2 = new TokenBgmWait(true);
                var t3 = new TokenBgmDel(true);

                // =================
                t1.UpdateData();
                t2.UpdateData();
                t3.UpdateData();

                Tokenizer.Tokens.Insert(idx, t1);
                Tokenizer.Tokens.Insert(idx + 1, t2);
                Tokenizer.Tokens.Insert(idx + 2, t3);

                CommandViewBox vb = DataContext as CommandViewBox;

                vb.MyListItems.Insert(idx, t1);
                vb.MyListItems.Insert(idx + 1, t2);
                vb.MyListItems.Insert(idx + 2, t3);

                ListView1.SelectedIndex += 3;
            }
        }



        private void Search(bool next)
        {
            if (textbox_search.Text == "")
                return;

            int mod(int k, int n) { return ((k %= n) < 0) ? k + n : k; }

            if ((bool)checkbox_SearchAllFiles.IsChecked)
            {
                int startIdx = listviewFiles.SelectedIndex;
                if (startIdx == -1) startIdx = 0;

                int idx = startIdx;

                while (!Tokenizer.Search(textbox_search.Text, next, (bool)checkbox_SearchCaseSensitive.IsChecked))
                {
                    bool success = CheckUnsavedChanges();
                    if (!success)
                        return;

                    if (next)
                        idx = mod(idx + 1, listviewFiles.Items.Count);
                    else
                        idx = mod(idx - 1, listviewFiles.Items.Count);
                    
                    if (idx == startIdx)
                    {
                        MessageBox.Show("Searched all files");
                        break;
                    }

                    // Select and focus on the file
                    listviewFiles.SelectedIndex = idx;
                    listviewFiles.UpdateLayout();
                    listviewFiles.ScrollIntoView(listviewFiles.Items[idx]);
                }
            }
            else
            {
                if (!Tokenizer.Search(textbox_search.Text, next, (bool)checkbox_SearchCaseSensitive.IsChecked))
                    MessageBox.Show("End of file");
            }
            
        }

        private void FocusTextNext(object sender, RoutedEventArgs e)
        {
            Tokenizer.GoToNextText(true);
        }

        private void MenuViewFolders_Clicked(object sender, RoutedEventArgs e)
        {
            MenuViewFolder.IsChecked = !MenuViewFolder.IsChecked;
            UpdateConfig("view_folders", MenuViewFolder.IsChecked ? "1": "0");
            GridTextboxes.Visibility = MenuViewFolder.IsChecked ? Visibility.Visible : Visibility.Collapsed;
        }

        private void MenuViewDescription_Clicked(object sender, RoutedEventArgs e)
        {
            MenuViewDescription.IsChecked = !MenuViewDescription.IsChecked;
            UpdateConfig("view_description", MenuViewDescription.IsChecked ? "1" : "0");

            if (ListView1.SelectedItem != null)
                (ListView1.SelectedItem as Token).UpdateGui();
        }

        private void MenuViewLabel_Clicked(object sender, RoutedEventArgs e)
        {
            MenuViewLabel.IsChecked = !MenuViewLabel.IsChecked;
            UpdateConfig("view_label", MenuViewLabel.IsChecked ? "1" : "0");

            if (ListView1.SelectedItem != null)
                (ListView1.SelectedItem as Token).UpdateGui();
        }
        
        private void FocusTextPrev(object sender, RoutedEventArgs e)
        {
            Tokenizer.GoToNextText(false);
        }
        
        private void Menu_File_Save(object sender, RoutedEventArgs e)
        {
            string fname;
            try
            {
                fname = (string)(listviewFiles.SelectedItem as ListViewItem).Content;
            } catch { return;  }


            try
            {
                Tokenizer.SaveFile(System.IO.Path.Combine(folder, (string)(listviewFiles.SelectedItem as ListViewItem).Content));
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void Menu_Export_Mac(object sender, RoutedEventArgs e)
        {
            if (textbox_exportedAfs.Text == "")
                return;

            var stream = new FileStream(textbox_exportedAfs.Text, FileMode.Create, FileAccess.Write);
            try
            {
                byte[] data = AFS.Pack(textbox_listFile.Text, textbox_inputFolder.Text);
                stream.Write(data, 0, (int)data.Length);
                stream.Close();
                MessageBox.Show("Exported " + textbox_exportedAfs.Text);
            }
            catch (Exception ex)
            {
                stream.Close();
                MessageBox.Show(ex.Message, "Error exporting AFS");
            }
        }

        private void ListView1_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count == 0)
                return;
            var t = e.AddedItems[0];
            
            (t as Token).UpdateGui();
        }

        private void ListViewFiles_SelectionChanged(object sender, SelectionChangedEventArgs args)
        {
            if (args.AddedItems.Count == 0)
                return;

            var success = CheckUnsavedChanges();
            if (!success)
                return;

            filename = (string)((ListViewItem)args.AddedItems[0]).Content;
            string path_en = System.IO.Path.Combine(folder, filename);
            string path_jp = System.IO.Path.Combine(textbox_inputFolderJp.Text, filename);
            if (!File.Exists(path_jp))
                path_jp = null;

            Tokenizer.OpenFile(path_en, path_jp);
            ((MainWindow)Application.Current.MainWindow).Title = "R11 Script: " + filename;
            DataContext = new CommandViewBox();
        }

        private void ListView1_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            ContextMenu m = new ContextMenu();
            var token = (sender as ListViewItem).Content;
            if (token is TokenIf)
            {
                var label = (token as TokenIf).LabelJump;
                var v = new MenuItem() { Header = "Jump to " + label };
                v.Click += (sender2, arg) => { Tokenizer.JumpToLabel(label); };
                m.Items.Add(v);
            }
            else if (token is TokenInternalGoto)
            {
                var label = (token as TokenInternalGoto).LabelJump;
                var v = new MenuItem() { Header = "Jump to " + label };
                v.Click += (sender2, arg) => { Tokenizer.JumpToLabel(label); };
                m.Items.Add(v);
            }
            else if (token is TokenMsgDisp2)
            {
                var labels = (token as TokenMsgDisp2).IdenticalJpLabels;
                foreach (var label in labels)
                {
                    var v = new MenuItem() { Header = "Identical to " + label };
                    v.Click += (sender2, arg) => { Tokenizer.JumpToLabel(label); };
                    m.Items.Add(v);
                }
            }
            else if (token is TokenSelectDisp2)
            {
                var labels = (token as TokenSelectDisp2).IdenticalJpLabels;
                foreach (var label in labels)
                {
                    var v = new MenuItem() { Header = "Identical to " + label };
                    v.Click += (sender2, arg) => { Tokenizer.JumpToLabel(label); };
                    m.Items.Add(v);
                }
            }

            foreach (var reflabel in (token as Token).ReferingLabels)
            {
                var v = new MenuItem() { Header = "Jumped here from " + reflabel };
                v.Click += (sender2, arg) => { Tokenizer.JumpToLabel(reflabel); };
                m.Items.Add(v);
            }
            /*
            else if (token is TokenMsgDisp2)
            {
                var label = (token as TokenMsgDisp2).LabelJump;
                var v = new MenuItem() { Header = "Jump to " + label };
                v.Click += (sender2, arg) => { Tokenizer.JumpToLabel(label); };
                m.Items.Add(v);
            }
            */

            if (m.Items.Count > 0)
                ListView1.ContextMenu = m;
            else
                ListView1.ContextMenu = null;
        }
    }

    public class CommandViewBox : INotifyPropertyChanged
    {
        private ObservableCollection<Token> _myListItems;
        public ObservableCollection<Token> MyListItems
        {
            get => _myListItems;
            set
            {
                _myListItems = value;
                OnPropertyChanged(nameof(MyListItems));
            }
        }

        public CommandViewBox()
        {
            MyListItems = new ObservableCollection<Token>();

            foreach (var token in Tokenizer.Tokens)
            {
                MyListItems.Add(token);
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    public class TokenSelectorComboBoxItem
    {
        public string Name { get; set; }
        public Type Value { get; set; }
        public override string ToString() { return this.Name; }

        public TokenSelectorComboBoxItem(string text)
        {
            this.Name = text;
            this.Value = Type.GetType("R11_Script_Editor.Tokens.Token" + text);
        }
    }
}