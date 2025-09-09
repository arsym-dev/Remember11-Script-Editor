using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Web.Script.Serialization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using Xceed.Wpf.Toolkit;

namespace R11_Script_Editor.Tokens
{
    public class Token : INotifyPropertyChanged
    {
        public const TokenType Type = 0;
        private SolidColorBrush BrushForeground = new SolidColorBrush(Color.FromRgb(255,255,255));
        private SolidColorBrush BrushBackground = new SolidColorBrush(Color.FromRgb(51, 51, 51));

        protected string _command;
        public string Command
        {
            get => _command;
        }

        protected string _description;
        public string Description
        {
            get => _description;
        }

        protected UInt16 _offset;
        public UInt16 Offset
        {
            get => _offset;
            set => _offset = value;
        }

        protected int _length;
        public int Length
        {
            get { return _length; }
        }

        private string _label;
        public string Label
        {
            get => _label;
            set
            {
                _label = value;
                OnPropertyChanged(nameof(Label));
            }
        }

        public string Land
        {
            get {
                if (this.ReferingLabels.Count > 0)
                    return ">";
                return "";
            }
        }


        private List<string> _refering_labels = new List<string>();
        public List<string> ReferingLabels
        {
            get => _refering_labels;
            set
            {
                _refering_labels = value;
                OnPropertyChanged(nameof(ReferingLabels));
            }
        }

        private string _data;
        public string Data
        {
            get => _data;
            set
            {
                _data = value;
                OnPropertyChanged(nameof(Data));
            }
        }

        private string _data2;
        public string Data2
        {
            get => _data2;
            set
            {
                _data2 = value;
                OnPropertyChanged(nameof(Data2));
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        public virtual byte[] GetBytes() { throw new NotImplementedException(); }

        public virtual string GetMessages() { return null; }

        public virtual byte[] GetMessagesBytes() { return null; }

        public virtual int SetMessagePointer(int offset) { return offset; }

        public virtual void UpdateData() { }

        public virtual void UpdateGui()
        {
            UpdateGui(true);
        }

        public virtual void AddItem(int index) { }
        public virtual void RemoveItem(int index) { }

        public virtual void UpdateGui(bool clear_list)
        {
            Tokenizer.Grid.Children.Clear();
            Tokenizer.Grid.RowDefinitions.Clear();
            Tokenizer.Grid.Height = 0;
            if (clear_list)
                Tokenizer.EntriesList.ItemsSource = null;

            var rows = Tokenizer.Grid.RowDefinitions;

            AddDescriptionBox();
            if (!Tokenizer.MenuViewDescription.IsChecked)
            {
                rows[rows.Count - 1].Height = new GridLength(0, GridUnitType.Pixel);
                Tokenizer.Grid.Height -= 60;
            }
                

            AddTextbox("Label", "Label");
            if (!Tokenizer.MenuViewLabel.IsChecked)
            {
                rows[rows.Count - 1].Height = new GridLength(0, GridUnitType.Pixel);
                Tokenizer.Grid.Height -= 24;
            }

            AddSpacer();
            if (!Tokenizer.MenuViewLabel.IsChecked && !Tokenizer.MenuViewDescription.IsChecked)
            {
                rows[rows.Count - 1].Height = new GridLength(0, GridUnitType.Pixel);
                Tokenizer.Grid.Height -= 24;
            }
        }

        protected void AddCombobox<T>(string label, string var_name)
        {
            var x = Tokenizer.Grid;
            x.Height += 24;
            var row = new RowDefinition();
            row.Height = new GridLength(24, GridUnitType.Pixel);
            x.RowDefinitions.Add(row);

            TextBlock txtBlock1 = new TextBlock();
            txtBlock1.Text = label;
            txtBlock1.FontSize = 14;
            txtBlock1.FontWeight = FontWeights.Bold;
            Grid.SetRow(txtBlock1, x.RowDefinitions.Count-1);
            Grid.SetColumn(txtBlock1, 0);
            x.Children.Add(txtBlock1);

            ComboBox cb = new ComboBox();
            foreach (var e in Enum.GetValues(typeof(T)))
                cb.Items.Add(e);

            cb.SelectedItem = this.GetType().GetProperty(var_name).GetValue(this);
            Grid.SetRow(cb, x.RowDefinitions.Count - 1);
            Grid.SetColumn(cb, 1);
            cb.SelectionChanged += (sender, args) =>
            {
                this.GetType().GetProperty(var_name).SetValue(this, (T)args.AddedItems[0], null);
                UpdateData();
            };
            x.Children.Add(cb);
        }

        protected void AddSpacer()
        {
            var x = Tokenizer.Grid;
            x.Height += 24;
            var row = new RowDefinition();
            row.Height = new GridLength(24, GridUnitType.Pixel);
            x.RowDefinitions.Add(row);
        }

        protected void AddRichTextbox(string label, string var_name, bool enabled=true)
        {
            var x = Tokenizer.Grid;
            x.Height += 60;
            var row = new RowDefinition();
            row.Height = new GridLength(60, GridUnitType.Pixel);
            x.RowDefinitions.Add(row);

            TextBlock txtBlock1 = new TextBlock();
            txtBlock1.Text = label;
            txtBlock1.FontSize = 14;
            txtBlock1.FontWeight = FontWeights.Bold;
            Grid.SetRow(txtBlock1, x.RowDefinitions.Count - 1);
            Grid.SetColumn(txtBlock1, 0);
            x.Children.Add(txtBlock1);


            TextBox tb = new TextBox();
            // disable drag and drop
            DataObject.AddCopyingHandler(tb, (sender, e) => { if (e.IsDragDrop) e.CancelCommand(); });
            Grid.SetRow(tb, x.RowDefinitions.Count - 1);
            Grid.SetColumn(tb, 1);
            tb.TextWrapping = TextWrapping.Wrap;
            tb.VerticalScrollBarVisibility = ScrollBarVisibility.Auto;
            //tb.IsEnabled = enabled;
            tb.IsReadOnly = !enabled;
            tb.Text = (string)this.GetType().GetProperty(var_name).GetValue(this);
            tb.TextChanged += (sender, args) =>
            {
                Tokenizer.ChangedFile = true;
                this.GetType().GetProperty(var_name).SetValue(this, tb.Text, null);
                UpdateData();
            };
            x.Children.Add(tb);
        }

        protected void AddDescriptionBox()
        {
            var x = Tokenizer.Grid;
            x.Height += 60;
            var row = new RowDefinition();
            row.Height = new GridLength(60, GridUnitType.Pixel);
            x.RowDefinitions.Add(row);

            TextBlock txtBlock1 = new TextBlock();
            txtBlock1.Text = "Description";
            txtBlock1.FontSize = 14;
            txtBlock1.FontWeight = FontWeights.Bold;
            Grid.SetRow(txtBlock1, x.RowDefinitions.Count - 1);
            Grid.SetColumn(txtBlock1, 0);
            x.Children.Add(txtBlock1);


            TextBox tb = new TextBox();
            // disable drag and drop
            DataObject.AddCopyingHandler(tb, (sender, e) => { if (e.IsDragDrop) e.CancelCommand(); });
            Grid.SetRow(tb, x.RowDefinitions.Count - 1);
            Grid.SetColumn(tb, 1);
            tb.TextWrapping = TextWrapping.Wrap;
            tb.VerticalScrollBarVisibility = ScrollBarVisibility.Auto;
            tb.IsReadOnly = true;
            tb.Text = (string)this.GetType().GetProperty("Description").GetValue(this);
            x.Children.Add(tb);
        }

        protected void AddTextbox(string label, string var_name, object obj= null)
        {
            if (obj == null)
                obj = this;

            var x = Tokenizer.Grid;
            x.Height += 24;
            var row = new RowDefinition();
            row.Height = new GridLength(24, GridUnitType.Pixel);
            x.RowDefinitions.Add(row);

            TextBlock txtBlock1 = new TextBlock();
            txtBlock1.Text = label;
            txtBlock1.FontSize = 14;
            txtBlock1.FontWeight = FontWeights.Bold;
            Grid.SetRow(txtBlock1, x.RowDefinitions.Count - 1);
            Grid.SetColumn(txtBlock1, 0);
            x.Children.Add(txtBlock1);

            TextBox tb = new TextBox();
            // disable drag and drop
            DataObject.AddCopyingHandler(tb, (sender, e) => { if (e.IsDragDrop) e.CancelCommand(); });
            Grid.SetRow(tb, x.RowDefinitions.Count - 1);
            Grid.SetColumn(tb, 1);
            tb.Text = (string)obj.GetType().GetProperty(var_name).GetValue(obj);
            tb.TextChanged += (sender, args) =>
            {
                Tokenizer.ChangedFile = true;
                obj.GetType().GetProperty(var_name).SetValue(obj, tb.Text, null);
                UpdateData();
            };
            x.Children.Add(tb);
        }

        protected void AddTranslationButton(string label, string var_name, bool enabled = true)
        {
            var x = Tokenizer.Grid;

            var row1 = new RowDefinition();
            row1.Height = new GridLength(24, GridUnitType.Pixel);
            x.Height += 24;
            x.RowDefinitions.Add(row1);

            TextBlock txtBlock1 = new TextBlock();
            txtBlock1.Text = label;
            txtBlock1.FontSize = 14;
            txtBlock1.FontWeight = FontWeights.Bold;
            Grid.SetRow(txtBlock1, x.RowDefinitions.Count - 1);
            Grid.SetColumn(txtBlock1, 0);
            x.Children.Add(txtBlock1);

            Button button_google = new Button();
            Grid.SetRow(button_google, x.RowDefinitions.Count - 1);
            Grid.SetColumn(button_google, 1);
            button_google.Content = "Google TL";
            x.Children.Add(button_google);

            var row3 = new RowDefinition();
            row3.Height = new GridLength(24, GridUnitType.Pixel);
            x.Height += 24;
            x.RowDefinitions.Add(row3);

            Button button_bing = new Button();
            Grid.SetRow(button_bing, x.RowDefinitions.Count - 1);
            Grid.SetColumn(button_bing, 1);
            button_bing.Content = "Bing TL";
            x.Children.Add(button_bing);

            var row4 = new RowDefinition();
            row4.Height = new GridLength(24, GridUnitType.Pixel);
            x.Height += 24;
            x.RowDefinitions.Add(row4);

            Button button_deepl = new Button();
            Grid.SetRow(button_deepl, x.RowDefinitions.Count - 1);
            Grid.SetColumn(button_deepl, 1);
            button_deepl.Content = "DeepL TL";
            x.Children.Add(button_deepl);


            var row2 = new RowDefinition();
            row2.Height = new GridLength(120, GridUnitType.Pixel);
            x.Height += 120;
            x.RowDefinitions.Add(row2);

            TextBox tb = new TextBox();
            // disable drag and drop
            DataObject.AddCopyingHandler(tb, (sender, e) => { if (e.IsDragDrop) e.CancelCommand(); });
            Grid.SetRow(tb, x.RowDefinitions.Count - 1);
            Grid.SetColumn(tb, 1);
            tb.TextWrapping = TextWrapping.Wrap;
            tb.VerticalScrollBarVisibility = ScrollBarVisibility.Auto;
            tb.IsReadOnly = !enabled;
            x.Children.Add(tb);


            button_google.Click += (sender, args) =>
            {
                string input = (string)this.GetType().GetProperty(var_name).GetValue(this);

                string url = String.Format("https://translate.googleapis.com/translate_a/single?client=gtx&sl={0}&tl={1}&dt=t&q={2}", "ja", "en", Uri.EscapeUriString(input));
                WebClient webClient = new WebClient();
                webClient.Encoding = System.Text.Encoding.UTF8;
                webClient.Headers.Add("user-agent", "Mozilla/4.0 (compatible; MSIE 6.0; " +
                                  "Windows NT 5.2; .NET CLR 1.0.3705;)");
                string result = webClient.DownloadString(url);

                // Get all json data
                var jsonData = new JavaScriptSerializer().Deserialize<List<dynamic>>(result);
                var translationItems = jsonData[0];
                string translation = "";

                // Loop through the collection extracting the translated objects
                foreach (object item in translationItems)
                {
                    IEnumerable translationLineObject = item as IEnumerable;
                    IEnumerator translationLineString = translationLineObject.GetEnumerator();
                    translationLineString.MoveNext();
                    translation += string.Format(" {0}", Convert.ToString(translationLineString.Current));
                }

                // Remove first blank character
                if (translation.Length > 1) { translation = translation.Substring(1); };

                // Return translation
                tb.Text = translation;
            };

            button_bing.Click += (sender, args) =>
            {
                string input = (string)this.GetType().GetProperty(var_name).GetValue(this);

                string url = "https://www.bing.com/ttranslatev3?isVertical=1&&IG=89A617AD83C84B9383CD52F1D13A4EB6&IID=translator.5026.3";
                WebClient webClient = new WebClient();
                webClient.Encoding = System.Text.Encoding.UTF8;
                webClient.Headers[HttpRequestHeader.ContentType] = "application/x-www-form-urlencoded";
                string myParameters = String.Format("&fromLang={0}&to={1}&text={2}", "ja", "en", Uri.EscapeUriString(input));
                string result = webClient.UploadString(url, myParameters);

                // Get all json data
                var jsonData = new JavaScriptSerializer().Deserialize<List<dynamic>>(result);
                var translation = jsonData[0]["translations"][0]["text"];

                tb.Text = translation;
            };

            button_deepl.Click += (sender, args) =>
            {
                string input = (string)this.GetType().GetProperty(var_name).GetValue(this);

                string url = "https://www2.deepl.com/jsonrpc";

                var httpWebRequest = (HttpWebRequest)WebRequest.Create(url);
                httpWebRequest.ContentType = "application/json";
                httpWebRequest.Method = "POST";

                using (var streamWriter = new StreamWriter(httpWebRequest.GetRequestStream()))
                {
                    string json =
                        "{ \"jsonrpc\":\"2.0\"," +
                        "\"method\": \"LMT_handle_jobs\"," +
                        "\"params\":{ " +
                            "\"jobs\":[" +
                                "{ \"kind\":\"default\"," +
                                "\"raw_en_sentence\":\"" + input + "\"," +
                                "\"raw_en_context_before\":[]," +
                                "\"raw_en_context_after\":[]," +
                                "\"preferred_num_beams\":4}" +
                                "]," +
                            "\"lang\":{" +
                                "\"user_preferred_langs\":[\"EN\",\"DE\",\"JA\"]," +
                                "\"source_lang_user_selected\":\"JA\"," +
                                "\"target_lang\":\"EN\"" +
                                "}," +
                            "\"priority\":1," +
                            "\"commonJobParams\":{}," +
                            "\"timestamp\": " + (new DateTimeOffset(DateTime.UtcNow).ToUnixTimeMilliseconds()).ToString() +
                        "}," +
                        "\"id\":98910006" +
                        "}";

                    streamWriter.Write(json);
                }

                string result;
                var httpResponse = (HttpWebResponse)httpWebRequest.GetResponse();
                using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
                {
                    result = streamReader.ReadToEnd();
                }
                //string result = "{\"id\":98910006,\"jsonrpc\":\"2.0\",\"result\":{\"date\":\"20200510\",\"source_lang\":\"JA\",\"source_lang_is_confident\":1,\"target_lang\":\"EN\",\"timestamp\":1589154589,\"translations\":[{\"beams\":[{\"num_symbols\":18,\"postprocessed_sentence\":\"Where is this place, did Yuni survive, and who is this woman?\",\"score\":-4991.57,\"totalLogProb\":16.3437},{\"num_symbols\":18,\"postprocessed_sentence\":\"Where is this place, did Yuni survive, and who was this woman?\",\"score\":-4991.58,\"totalLogProb\":16.3246},{\"num_symbols\":18,\"postprocessed_sentence\":\"Where is this place, did Yuni survive, and who was that woman?\",\"score\":-4991.77,\"totalLogProb\":15.9421},{\"num_symbols\":19,\"postprocessed_sentence\":\"Where is this place, did Yuni survive, and who was this woman really?\",\"score\":-4992.6,\"totalLogProb\":14.3136}],\"quality\":\"normal\"}]}}\r\n";

                // Process JSON result
                result = result.Replace("\n", "").Replace("\r", "");
                var jsonData = new JavaScriptSerializer().Deserialize<dynamic>(result);
                var v = jsonData["result"];

                var final = "";
                foreach (var beam in jsonData["result"]["translations"][0]["beams"])
                {
                    if (final != "")
                        final += "\n\n";
                    final += beam["postprocessed_sentence"];
                }

                tb.Text = final;
            };
        }



        protected void AddUint8(string label, string var_name, object obj=null)
        {
            if (obj == null)
                obj = this;

            var x = Tokenizer.Grid;
            x.Height += 24;
            var row = new RowDefinition();
            row.Height = new GridLength(24, GridUnitType.Pixel);
            x.RowDefinitions.Add(row);

            TextBlock txtBlock1 = new TextBlock();
            txtBlock1.Text = label;
            txtBlock1.FontSize = 14;
            txtBlock1.FontWeight = FontWeights.Bold;
            Grid.SetRow(txtBlock1, x.RowDefinitions.Count - 1);
            Grid.SetColumn(txtBlock1, 0);
            x.Children.Add(txtBlock1);

            ByteUpDown ud = new ByteUpDown();
            // disable drag and drop
            DataObject.AddCopyingHandler(ud, (sender, e) => { if (e.IsDragDrop) e.CancelCommand(); });
            var style = Application.Current.Resources[typeof(Button)];
            ud.Background = BrushBackground;
            ud.Foreground = BrushForeground;
            Grid.SetRow(ud, x.RowDefinitions.Count - 1);
            Grid.SetColumn(ud, 1);
            ud.Width = 60;
            ud.HorizontalAlignment = HorizontalAlignment.Left;
            ud.Value = (byte)obj.GetType().GetProperty(var_name).GetValue(obj);
            ud.ValueChanged += (sender, args) =>
            {
                Tokenizer.ChangedFile = true;
                obj.GetType().GetProperty(var_name).SetValue(obj, (byte)args.NewValue, null);
                UpdateData();
            };
            x.Children.Add(ud);
        }

        protected void AddUint16(string label, string var_name, object obj = null)
        {
            if (obj == null)
                obj = this;

            var x = Tokenizer.Grid;
            x.Height += 24;
            var row = new RowDefinition();
            row.Height = new GridLength(24, GridUnitType.Pixel);
            x.RowDefinitions.Add(row);

            TextBlock txtBlock1 = new TextBlock();
            txtBlock1.Text = label;
            txtBlock1.FontSize = 14;
            txtBlock1.FontWeight = FontWeights.Bold;
            Grid.SetRow(txtBlock1, x.RowDefinitions.Count - 1);
            Grid.SetColumn(txtBlock1, 0);
            x.Children.Add(txtBlock1);

            IntegerUpDown ud = new IntegerUpDown();
            // disable drag and drop
            DataObject.AddCopyingHandler(ud, (sender, e) => { if (e.IsDragDrop) e.CancelCommand(); });
            ud.Background = BrushBackground;
            ud.Foreground = BrushForeground;
            Grid.SetRow(ud, x.RowDefinitions.Count - 1);
            Grid.SetColumn(ud, 1);
            ud.Width = 80;
            ud.HorizontalAlignment = HorizontalAlignment.Left;
            ud.Value = (UInt16)obj.GetType().GetProperty(var_name).GetValue(obj);
            ud.ValueChanged += (sender, args) =>
            {
                if (args.NewValue == null)
                    return;

                int number = (int)args.NewValue;
                
                if(number > UInt16.MaxValue) ud.Value = UInt16.MaxValue;
                else if (number < 0) ud.Value = 0;
                else
                {
                    Tokenizer.ChangedFile = true;
                    obj.GetType().GetProperty(var_name).SetValue(obj, (UInt16)number, null);
                    UpdateData();
                }
            };
            x.Children.Add(ud);
        }

        protected void PopulateEntryList<T>(List<T> entries, SelectionChangedEventHandler ev_handler)
        {
            Tokenizer.EntriesList.ItemsSource = entries;
            Tokenizer.EntriesList.SelectionChanged += ev_handler;
            if (entries.Count > 0)
                Tokenizer.EntriesList.SelectedIndex = 0;
        }
    }

    public enum TokenType
    {
        nop = 0x00,
        end = 0x01,
        ifx = 0x02,
        int_goto = 0x03,
        int_call = 0x04,
        int_return = 0x05,
        ext_goto = 0x06,
        ext_call = 0x07,
        ext_return = 0x08,
        reg_calc = 0x09,
        count_clear = 0x0A,
        count_wait = 0x0B,
        time_wait = 0x0C,
        pad_wait = 0x0D,
        pad_get = 0x0E,
        file_read = 0x0F,
        file_wait = 0x10,
        msg_wind = 0x11,
        msg_view = 0x12,
        msg_mode = 0x13,
        msg_pos = 0x14,
        msg_size = 0x15,
        msg_type = 0x16,
        msg_cursor = 0x17,
        msg_set = 0x18,
        msg_wait = 0x19,
        msg_clear = 0x1A,
        msg_line = 0x1B,
        msg_speed = 0x1C,
        msg_color = 0x1D,
        msg_anim = 0x1E,
        msg_disp = 0x1F,
        sel_set = 0x20,
        sel_entry = 0x21,
        sel_view = 0x22,
        sel_wait = 0x23,
        sel_style = 0x24,
        sel_disp = 0x25,
        fade_start = 0x26,
        fade_wait = 0x27,
        graph_set = 0x28,
        graph_del = 0x29,
        graph_copy = 0x2A,
        graph_view = 0x2B,
        graph_pos = 0x2C,
        graph_move = 0x2D,
        graph_prio = 0x2E,
        graph_anim = 0x2F,
        graph_pal = 0x30,
        graph_lay = 0x31,
        graph_wait = 0x32,
        graph_disp = 0x33,
        effect_start = 0x34,
        effect_end = 0x35,
        effect_wait = 0x36,
        bgm_set = 0x37,
        bgm_del = 0x38,
        bgm_req = 0x39,
        bgm_wait = 0x3A,
        bgm_speed = 0x3B,
        bgm_vol = 0x3C,
        se_set = 0x3D,
        se_del = 0x3E,
        se_req = 0x3F,
        se_wait = 0x40,
        se_speed = 0x41,
        se_vol = 0x42,
        voice_set = 0x43,
        voice_del = 0x44,
        voice_req = 0x45,
        voice_wait = 0x46,
        voice_speed = 0x47,
        voice_vol = 0x48,
        menu_lock = 0x49,
        save_lock = 0x4A,
        save_check = 0x4B,
        save_disp = 0x4C,
        disk_change = 0x4D,
        skip_start = 0x4E,
        skip_end = 0x4F,
        task_entry = 0x50,
        task_del = 0x51,
        cal_disp = 0x52,
        title_disp = 0x53,
        vib_start = 0x54,
        vib_end = 0x55,
        vib_wait = 0x56,
        map_view = 0x57,
        map_entry = 0x58,
        map_disp = 0x59,
        edit_view = 0x5A,
        chat_send = 0x5B,
        chat_msg = 0x5C,
        chat_entry = 0x5D,
        chat_exit = 0x5E,
        nop2 = 0x5F,
        movie_play = 0x60,
        graph_pos_auto = 0x61,
        graph_pos_save = 0x62,
        graph_uv_auto = 0x63,
        graph_uv_save = 0x64,
        effect_ex = 0x65,
        fade_ex = 0x66,
        vib_ex = 0x67,
        clock_disp = 0x68,
        graph_disp_ex = 0x69,
        map_init_ex = 0x6A,
        map_point_ex = 0x6B,
        map_route_ex = 0x6C,
        quick_save = 0x6D,
        trace_spc = 0x6E,
        sys_msg = 0x6F,
        skip_lock = 0x70,
        key_lock = 0x71,
        graph_disp2 = 0x72,
        msg_disp2 = 0x73,
        sel_disp2 = 0x74,
        date_disp = 0x75,
        vr_disp = 0x76,
        vr_select = 0x77,
        vr_reg_calc = 0x78,
        vr_msg_disp = 0x79,
        map_select = 0x7A,
        ecg_set = 0x7B,
        ev_init = 0x7C,
        ev_disp = 0x7D,
        ev_anim = 0x7E,
        eye_lock = 0x7F,
        msg_lock = 0x80,
        graph_scale_auto = 0x81,
        movie_start = 0x82,
        movie_end = 0x83,
        fade_ex_start = 0x84,
        fade_ex_wait = 0x85,
        breath_lock = 0x86,
    }
}
