using System;
using System.IO;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Diagnostics;
using System.Runtime.InteropServices; 
using Microsoft.Win32.SafeHandles;


namespace Lab2_3
{
    public partial class WriteToZero : Form
    {
        public WriteToZero()
        {
            InitializeComponent();
                //поиск внешних накопителей
            
        }
        public static byte[] buffer0sector;
        const string alf = "qwertyuiopasdfghjklzxcvbnm0123456789";
        const string fileway = "password.txt";

        private string res;
        private int k, x, z;

        [DllImport("Kernel32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        static extern SafeFileHandle CreateFile(
        string fileName,
        [MarshalAs(UnmanagedType.U4)] FileAccess fileAccess,
        [MarshalAs(UnmanagedType.U4)] FileShare fileShare,
        int securityAttributes,
        [MarshalAs(UnmanagedType.U4)] FileMode creationDisposition,
        [MarshalAs(UnmanagedType.U4)] FileAttributes fileAttributes,
        IntPtr template);

        private void Form1_Load(object sender, EventArgs e)
        {
            
            string mydrive;
            DriveInfo[] allDrives = DriveInfo.GetDrives();
            foreach (DriveInfo d in allDrives)
            {
                if (d.IsReady && (d.DriveType == DriveType.Removable))
                {
                    mydrive = d.Name;
                    comboBoxdisk.Items.Add(mydrive);
                }
            }
            comboBoxdisk.SelectedIndex = 0;
            if (comboBoxdisk.Items.Count == 0)
            {
                comboBoxdisk.Items.Add("Внешние носители отсутствуют");
            }
        }
        private void ReadZeroSector()
        {

            //настройка функции из кернел.длл
            string path = "\\\\.\\" + comboBoxdisk.Text.Remove(2);
            SafeFileHandle driveHandle = CreateFile(path, FileAccess.Read, FileShare.ReadWrite, 0, FileMode.Open, FileAttributes.Normal, IntPtr.Zero);
            using (FileStream disk = new FileStream(driveHandle, FileAccess.Read))
            {
                buffer0sector = new byte[512];
                disk.Read(buffer0sector, 0, 512);      
                //выводим нулевой сектор в таблицу
                for (int i = 0; i < 64; i++)
                {
                    dataGridView1.Rows.Add((char)buffer0sector[i * 8 + 0], (char)buffer0sector[i * 8 + 1], (char)buffer0sector[i * 8 + 2], (char)buffer0sector[i * 8 + 3],
                        (char)buffer0sector[i * 8 + 4], (char)buffer0sector[i * 8 + 5], (char)buffer0sector[i * 8 + 6], (char)buffer0sector[i * 8 + 7]);
                }
            }
        }
        private void button2_Click(object sender, EventArgs e)
        {
            dataGridView1.Rows.Clear();
            ReadZeroSector();
            //найдём сумму байт в секторах с 384 по 416 чтобы узнать есть ли там пароль
            int summa = 0;
            for (int i = 384; i < 416; i++)
                summa += buffer0sector[i];
            //если сумма отличается от нуля, значит пароль в нулевом секторе есть
            if (summa == 0)
            {
                MessageBox.Show("Пароля в нулевом секторе нет! Задайте пароль!");
            }
            //закрасим место с паролем
            for (int i = 48; i < 52; i++)
                for (int j = 0; j < 8; j++)
                    dataGridView1[j, i].Style.BackColor = Color.Green;
        }


        /*private string CreateKey()
        {
            string key = "";
            var rnd = new Random();
            var s = new StringBuilder();
            for (int i = 0; i < 3; i++)
                s.Append((char)rnd.Next('a', 'z'));
            key = s.ToString();
            return key;
        }*/
        private string Encryption(string pass, string key) //функция шифрования
        {

            res = string.Empty;
            while (key.Length < pass.Length)
            {
                key += key;
                if (key.Length > pass.Length) key = key.Remove(pass.Length);
            }
            for (int i = 0; i < pass.Length; i++)
            {
                for (int id = 0; id < alf.Length; id++)
                {
                    if (key[i] == alf[id]) k = id;
                    if (pass[i] == alf[id]) x = id;
                    z = (x + k) % alf.Length;
                }
                res += alf[z];
            }
            return res;
        }
        private void button1_Click(object sender, EventArgs e)
        {
            //@"\\.\G:",
            string path = "\\\\.\\" + comboBoxdisk.Text.Remove(2);
            SafeFileHandle driveHandle = CreateFile(path, FileAccess.Read, FileShare.ReadWrite, 0, FileMode.Open, FileAttributes.Normal, IntPtr.Zero);
            using (FileStream disk = new FileStream(driveHandle, FileAccess.Read))
            {
                buffer0sector = new byte[512];
                disk.Read(buffer0sector, 0, 512);
            }
            //найдём сумму байт в секторах с 384 по 416 чтобы узнать есть ли там пароль
            int summa = 0;
            for (int i = 384; i < 416; i++)
                summa += buffer0sector[i];
            //если сумма отличается от нуля, значит пароль в нулевом секторе есть
            if (summa > 0)
            {
                //пароль есть и его нужно проверить и изменить
                //создаем новое окно
                this.Hide();
                ChangePassword f = new ChangePassword(path); //Вывод на экран окна успешного ввода
                f.Show();
            }
            else
            {
                SafeFileHandle driveHandleRead = CreateFile(path, FileAccess.Write, FileShare.ReadWrite, 0, FileMode.Open, FileAttributes.Normal, IntPtr.Zero);
                {
                    using (FileStream disk = new FileStream(driveHandleRead, FileAccess.Write))
                    {
                        String hashpassword = Encryption(textBox1.Text, "abc");
                        for (int i = 0; i < hashpassword.Length; i++)
                            buffer0sector[384 + i] = (byte)hashpassword[i];

                        disk.Write(buffer0sector, 0, 512);
                        MessageBox.Show("Пароль записан в нулевой сектор!");
                    }
                }
            }
        }
    }
      
}
