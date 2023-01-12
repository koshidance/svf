using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Pix
{
    public partial class Form2 : Form
    {
        public Form2()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            string[] s = textBox1.Text.Split("\n");
            string res = "{ ";
            List<string> sl = new List<string>();
            for(int i=0; i<s.Length; i++)
            {
                if (s[i] != "" && s[i].Length > 2) sl.Add(s[i]);
            }
            s = new string[sl.Count];
            int index = 0;
            foreach(string sa in sl)
            {
                s[index] = sa;
                index++;
            }
            for(int i=0; i<s.Length; i++)
            {
                res += $"\"#{s[i].Remove(s[i].Length-1)}\", ";
            }
            res = res.Remove(res.Length - 2);
            res += " }";
            textBox2.Text = res;
        }
    }
}
