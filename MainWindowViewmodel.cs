using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using WPFbase;
namespace ProjectThreads1WPF
{
    public class MainWindowViewmodel : OnPropertyChangedHandler
    {
        public MainWindowViewmodel()
        {

        }
        public ObservableCollection<ProgressBar> ProgressBars { get; set; } = new ObservableCollection<ProgressBar>();
        public ChangingItem<int> MaxLength { get; set; } = new ChangingItem<int>();
        public int threadscol = 4;
        public List<Thread> threads = new List<Thread>();
        public ChangingItem<string> Letters { get; set; } = new ChangingItem<string>("");
        public List<string> AllVariants { get; private set; } = new List<string>();
        public bool IsAlive;
        int n => AllVariants.Count / threadscol;
        public class FinderObject
        {
            public int Skip { get; set; }
            public int Take { get; set; }
            public CancellationTokenSource CT { get; set; }
        }
        public void Finder(object o)
        {
            var obj = o as FinderObject;
            var mas = AllVariants.Skip(obj.Skip).Take(obj.Take).ToList();
            AllVariants.RemoveRange(obj.Skip, obj.Take);
            foreach (var item in mas)
            {
                if (!IsAlive)
                {
                    break;
                }
                
                if (CreateMD5(item) == Hash.Item)
                {
                    //Console.WriteLine(item);
                    MessageBox.Show("Слово - " + item);
                    CloseAllThreads();
                }
                mas.Remove(item);

            }

        }
        public void CloseAllThreads()
        {
            IsAlive = false;
            foreach (var item in threads)
            {
                item.Abort();
            }
        }
        void GetVariants(int pos, string text)
        {
            int length;
            for (int j = 1; j <= MaxLength.Item; j++)
            {
                length = j;
                for (int i = 0; i < Letters.Item.Length; i++)
                {
                    //Console.WriteLine(var);
                    if (pos < length)
                    {
                        var var = text + Letters.Item[i];
                        AllVariants.Add(var);
                        if (pos + 1 < length)
                        {
                            GetVariants(pos + 1, var);
                        }

                    }
                }
            }

        }
        public string CreateMD5(string input)
        {
            using (System.Security.Cryptography.MD5 md5 = System.Security.Cryptography.MD5.Create())
            {
                byte[] inputBytes = System.Text.Encoding.ASCII.GetBytes(input);
                byte[] hashBytes = md5.ComputeHash(inputBytes);
                StringBuilder sb = new StringBuilder();
                for (int i = 0; i < hashBytes.Length; i++)
                {
                    sb.Append(hashBytes[i].ToString("X2"));
                }
                return sb.ToString();
            }
        }
        public string Text
        {
            get => text;
            set
            {
                text = value;
                OnPropertyChanged(Text);
                Hash.Item = CreateMD5(text);
                if (text == "")
                {
                    Hash.Item = "";
                }
            }
        }
        string text { get; set; } = "";
        bool Valid 
        { 
            get
            {
                for (int i = 0; i < Text.Length; i++)
                {
                    if (!Letters.Item.ToList().Contains(Text[i]))
                    {
                        return false;
                    }
                }
                if (Letters.Item.Length != 0 && Text.Length != 0)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            } 
        }


        public ChangingItem<string> Hash { get; set; } = new ChangingItem<string>();
        public ICommand Start => new RelayCommand(o =>
        {
            IsAlive = true;
            AllVariants = new List<string>();
            GetVariants(0, "");
            threads = new List<Thread>();
            //Console.WriteLine($"All {AllVariants.Count} Thread {n}");
            for (int i = 0; i < threadscol; i++)
            {
                var a = new ParameterizedThreadStart(Finder);
                threads.Add(new Thread(a));
                var ct = new CancellationTokenSource();
                ProgressBars.Add(new ProgressBar()
                {
                    Height=250,
                    Width=400,
                    Minimum=0,
                    Maximum=n,
                    Value=0
                });
                OnPropertyChanged(nameof(ProgressBars));
                threads[i].Start(new FinderObject()
                {
                    Take = n,
                    Skip = i * n,
                    CT= ct
                });
                threads[i].Join();
            }
        }, o=>Valid);
    }
}