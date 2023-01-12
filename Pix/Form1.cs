using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using FFMpegCore.Extend;
using FFMpegCore.Pipes;
using FFMpegCore;

namespace Pix
{
    public partial class Form1 : Form
    {
        int globalProgressValue = 0;
        int globalProgressValue2 = 0;
        int globalProgressValue3 = 0;
        bool globalSmooth = false;
        int globalComboBox = 1;
        double globalVideoFps = 0;
        ulong globalVideoDuration = 0;
        string globalVideoPath = "";
        string globalStartVideoPath = "";
        byte[][,,] globalVideoFrames;
        Bitmap[] globalVideoFramesBitmap;
        bool globalEndFlag = true;
        bool globalConsole = false;
        bool globalDeveloper = false;
        Bitmap globalPreviewBMP;

        Random globalRnd;

        int tempIter = 0;


        Bitmap startBMP;
        Mutex setupM = new Mutex();
        Mutex m = new Mutex();
        Mutex m1 = new Mutex();
        Mutex m2 = new Mutex();
        Mutex mutexAsyncSaveBitmap = new Mutex();
        Mutex mutexAsyncSaveBitmap2 = new Mutex();
        Mutex mPB3 = new Mutex();


        double fPS = -1;

        public Form1()
        {
            InitializeComponent();
            globalRnd = new Random((int)DateTime.Now.Ticks);
            comboBox1.SelectedIndex = 0;
            this.MinimumSize = this.MaximumSize = new Size(693, 685);
            this.Size = new Size(693, 685);

            if (!globalDeveloper)
            {
                button1.Visible = false;
                trackBar1.Visible = false;
                button5.Visible = false;
                button2.Visible = false;
                button3.Visible = false;
                button6.Visible = false;
                button9.Visible = false;
                label1.Visible = false;
            }

            tryToDeleteTempFiles();
        }
        private void button1_Click(object sender, EventArgs e)
        {
            int UNSCALE = 25;

            var ofd = new OpenFileDialog();

            if (ofd.ShowDialog(this) == DialogResult.OK)
            {
                startBMP = bmpScrath((Bitmap)Image.FromFile(ofd.FileName), pictureBox1.Width, pictureBox1.Height);
                pictureBox1.Image = startBMP;
            }

        }
        private void button2_Click(object sender, EventArgs e)
        {
            byte[,,] data = BitmapToByteRgbQ(startBMP);
            data = Pixelion(data, searchNeight(trackBar1.Value, pictureBox1.Size.Width));
            pictureBox1.Image = byteToBMP(data);
        }
        private void button3_Click(object sender, EventArgs e)
        {
            byte[,,] data = BitmapToByteRgbQ((Bitmap)pictureBox1.Image);
            data = Minecraftion(data, getGlobalPallit(globalComboBox));
            pictureBox1.Image = byteToBMP(data);
        }
        private void button4_Click(object sender, EventArgs e)
        {
            var ofd = new OpenFileDialog();

            if (ofd.ShowDialog(this) == DialogResult.OK)
            {
                textBox1.Text = ofd.FileName;
                globalStartVideoPath = ofd.FileName;
                string newfolder = sha256_hash($"{DateTime.Now}");

                Directory.CreateDirectory(newfolder);

                ulong dur = getFrameRate(ofd.FileName);

                Thread.Sleep(100);

                Thread SaveAsync = new Thread(() => saveRawImage(newfolder));
                SaveAsync.Start();

                SaveAsync.Join();

                globalVideoPath = newfolder;

                globalVideoDuration = dur;
            }
        }
        private void button5_Click(object sender, EventArgs e)
        {
            byte[,,] data = BitmapToByteRgbQ(startBMP);
            data = Pixelion_Native(data, searchNeight(trackBar1.Value, pictureBox1.Size.Width));
            pictureBox1.Image = byteToBMP(data);
        }
        private void button6_Click(object sender, EventArgs e)
        {
            byte[,,] data = BitmapToByteRgbQ((Bitmap)pictureBox1.Image);
            data = Minecraftion(data, getGlobalPallit(globalComboBox), searchNeight(trackBar1.Value, pictureBox1.Size.Width));
            pictureBox1.Image = byteToBMP(data);
        }
        private void button7_Click(object sender, EventArgs e)
        {

            byte[,,] data = BitmapToByteRgbQ(globalPreviewBMP);

            data = !globalSmooth ? Pixelion_Native(data, searchNeight(trackBar2.Value, data.GetLength(2), data.GetLength(1))) : Pixelion(data, searchNeight(trackBar2.Value, data.GetLength(2), data.GetLength(1)));

            data = Minecraftion(data, getGlobalPallit(globalComboBox), searchNeight(trackBar2.Value, data.GetLength(2), data.GetLength(1)));

            pictureBox1.Image = byteToBMP(data);
        }
        private void button8_Click(object sender, EventArgs e)
        {
            int pixLevel = trackBar2.Value;
            new Thread(() => asyncFunc(pixLevel)).Start();

        }
        private void button9_Click(object sender, EventArgs e)
        {
            //IEnumerable<BitmapVideoFrameWrapper> ss = CreateFramesSD(100);
            pictureBox1.Image = globalVideoFramesBitmap[tempIter];
            tempIter++;
        }
        private void button9_Click_1(object sender, EventArgs e)
        {
            Form2 f2 = new Form2();
            f2.ShowDialog();
        }
        private void button10_Click(object sender, EventArgs e)
        {

        }
        private void button11_Click(object sender, EventArgs e)
        {
            string[] Dir = Directory.GetFiles($"{globalVideoPath}");

            string randFrame = Dir[globalRnd.Next(0, Dir.Length - 1)];

            Bitmap bmp = (Bitmap)Bitmap.FromFile(randFrame);

            bmp = bmpScrath(bmp, pictureBox1.Width, pictureBox1.Height);

            pictureBox1.Image = bmp;
            globalPreviewBMP = bmp;
        }
        private void button12_Click(object sender, EventArgs e)
        {
            globalConsole = !globalConsole;
            if (globalConsole)
            {
                button12.Text = "<";
                this.MinimumSize = this.MaximumSize = new Size(1008, 685);
                this.Size = new Size(1008, 685);
            }
            else
            {
                button12.Text = ">";
                this.MinimumSize = this.MaximumSize = new Size(693, 685);
                this.Size = new Size(693, 685);
            }
        }
        private void trackBar1_Scroll(object sender, EventArgs e)
        {
            label1.Text = trackBar1.Value.ToString();
        }
        private void trackBar2_Scroll(object sender, EventArgs e)
        {
            label3.Text = trackBar2.Value.ToString();
            
        }
        private void trackBar2_KeyUp(object sender, KeyEventArgs e)
        {

        }
        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            globalSmooth = checkBox1.Checked;
        }
        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            globalComboBox = comboBox1.SelectedIndex;
        }
        private void textBox2_TextChanged(object sender, EventArgs e)
        {
            textBox2.SelectionStart = textBox2.Text.Length;
            textBox2.ScrollToCaret();
            textBox2.Refresh();
        }
        private void Form1_FormClosed(object sender, FormClosedEventArgs e)
        {
            tryToDeleteTempFiles();
        }
        private void label4_Click(object sender, EventArgs e)
        {
            System.Diagnostics.Process.Start("explorer","https://github.com/koshidance");
        }

        private void saveRawImage(string newfolder)
        {
            var success2 = FFMpegArguments
                    .FromFileInput(globalStartVideoPath)
                    .OutputToFile(@$"{newfolder}\img%d.png", overwrite: true, options => options.WithVideoCodec("png").WithFrameOutputCount(-1))
                    .ProcessSynchronously();
        }
        private void thProgress(int counter)
        {
            int oldPV = 0,
                oldIter = 0;
            while (true)
            {
                m1.WaitOne();
                int localProgressValue = globalProgressValue;
                m1.ReleaseMutex();
                double proc = Math.Round((double)localProgressValue / counter * 100.0, 2);


                var updateProgBar = new Action(() => progressBar1.Value = Convert.ToInt16(proc));
                progressBar1.Invoke(updateProgBar);

                //Console.WriteLine($"{localProgressValue}/{counter}\t{proc}%");
                consoleWriteLine($"{localProgressValue}/{counter}\t{proc}%");

                if (oldPV == localProgressValue) oldIter++;
                else oldIter = 0;

                if (counter == localProgressValue || oldIter == 20) break;

                oldPV = localProgressValue;

                Thread.Sleep(625);

            }
            m1.Dispose();
        }
        private void thProgress2(int counter)
        {
            int oldPV = 0,
                oldIter = 0;
            while (true)
            {
                mutexAsyncSaveBitmap2.WaitOne();
                int localProgressValue = globalProgressValue2;
                mutexAsyncSaveBitmap2.ReleaseMutex();

                double proc = Math.Round((double)localProgressValue / counter * 100.0, 2);

                var updateProgBar = new Action(() => progressBar2.Value = Convert.ToInt16(proc));
                progressBar1.Invoke(updateProgBar);

                consoleWriteLine($"{localProgressValue}/{counter}\t{proc}%");


                if (oldPV == localProgressValue) oldIter++;
                else oldIter = 0;

                if (counter == localProgressValue || oldIter == 3) break;

                oldPV = localProgressValue;

                Thread.Sleep(625);

            }

            mutexAsyncSaveBitmap2.Dispose();
        }
        private void thProgress3(int counter)
        {
            int oldPV = 0,
                oldIter = 0;
            while (true)
            {
                mPB3.WaitOne();
                int localProgressValue = globalProgressValue3;
                mPB3.ReleaseMutex();

                double proc = Math.Round((double)localProgressValue / counter * 100.0, 2);

                var updateProgBar = new Action(() => progressBar3.Value = Convert.ToInt16(proc));
                progressBar1.Invoke(updateProgBar);

                consoleWriteLine($"{localProgressValue}/{counter}\t{proc}%");

                if (oldPV == localProgressValue) oldIter++;
                else oldIter = 0;

                if (counter == localProgressValue || oldIter == 3) break;

                oldPV = localProgressValue;

                Thread.Sleep(625);

            }

            mPB3.Dispose();
        }
        private void threading(string newfolder, int id, int start, int end, int trackBar)
        {
            setupM.WaitOne();
            int localTrackBar = -1;
            bool Minecraft = checkBox3.Checked;
            bool Pixelization = checkBox2.Checked;
            bool localSmooth = globalSmooth;
            int[,] pallit = getGlobalPallit(globalComboBox);
            setupM.ReleaseMutex();

            for (int i = start; i < end; i++)
            {
                m.WaitOne();
                byte[,,] data = globalVideoFrames[i];
                if (localTrackBar == -1) localTrackBar = searchNeight(trackBar, data.GetLength(2), data.GetLength(1));
                m.ReleaseMutex();

                if (Pixelization) data = localSmooth ? Pixelion(data, localTrackBar) : Pixelion_Native(data, localTrackBar);
                if (Minecraft) data = Minecraftion(data, pallit, localTrackBar);

                m1.WaitOne();
                globalProgressValue++;
                globalVideoFrames[i] = data;
                m1.ReleaseMutex();
            }
        }
        private void asyncFunc(int pixLevel)
        {
            if (File.Exists($"temp.mp3"))
            {
                File.Delete($"temp.mp3");
            }

            takeMusic();

            if (!Directory.Exists("out"))
            {
                Directory.CreateDirectory("out");
            }

            int counter = new DirectoryInfo(globalVideoPath).GetFiles().Length;
            globalVideoFps = Math.Round((double)counter / globalVideoDuration, 5);

            globalVideoFrames = new byte[counter][,,];

            string[] img = Directory.GetFiles(globalVideoPath);



            img = sortFilePath(img);

            for (int i = 0; i < img.Length; i++)
            {
                globalVideoFrames[i] = BitmapToByteRgbQ((Bitmap)Bitmap.FromFile(img[i]));
            }

            Thread[] threads = new Thread[searchNeight(14, counter)];

            for (int i = 0; i < threads.Length; i++)

            {
                int start = (i * (counter / threads.Length));
                int end = (i * (counter / threads.Length) + (counter / threads.Length));

                threads[i] = new Thread(() => threading($"{globalVideoPath}", i, start, end, pixLevel));
                threads[i].Start();

                consoleWriteLine($"th {i} start {start}/{end}");
                Thread.Sleep(50);
            }

            new Thread(() => thProgress(counter)).Start();

            for (int i = 0; i < threads.Length; i++)
            {
                threads[i].Join();
                consoleWriteLine($"th {i} closed");
            }
            Thread.Sleep(5000);
            consoleWriteLine("\n\n\n");

            m.Dispose();
            setupM.Dispose();

            globalVideoFramesBitmap = new Bitmap[counter];

            for (int i = 0; i < threads.Length; i++)
            {
                int start = (i * (counter / threads.Length));
                int end = (i * (counter / threads.Length) + (counter / threads.Length));

                threads[i] = new Thread(() => asyncSaveBitmap(start, end));
                threads[i].Start();
                consoleWriteLine($"th {i} start");
                Thread.Sleep(50);
            }

            new Thread(() => thProgress2(counter)).Start();

            for (int i = 0; i < threads.Length; i++)
            {
                threads[i].Join();
                consoleWriteLine($"th {i} closed");
            }
            Thread.Sleep(5000);
            consoleWriteLine("\n\n\n");
            Array.Clear(globalVideoFrames);

            new Thread(() => thProgress3(counter)).Start();

            var frames = CreateFramesSD(counter);
            var vfs = new RawVideoPipeSource(frames) { FrameRate = globalVideoFps };
            var success = FFMpegArguments
                .FromPipeInput(vfs)
                .OutputToFile($"out\\{globalVideoPath}.mp4", overwrite: true, options => options.WithVideoCodec("libvpx-vp9"))
                .ProcessSynchronously();

            while (globalEndFlag)
            {
                Thread.Sleep(1000);
            }

            setMusic();

            consoleWriteLine("\n\n\n");

            File.Delete($"out\\{globalVideoPath}.mp4");
            File.Delete($"temp.mp3");
            try
            {
                Directory.Delete($"{globalVideoPath}", true);
            }
            catch { }

            consoleWriteLine("success");

            MessageBox.Show("OKEY");


        }
        private void takeMusic()
        {
            FFMpeg.ExtractAudio(globalStartVideoPath, "temp.mp3");
        }
        private void setMusic()
        {
            FFMpeg.ReplaceAudio($"out\\{globalVideoPath}.mp4", "temp.mp3", $"out\\{globalVideoPath}l.mp4");
        }
        private void asyncSaveBitmap(int start, int end)
        {
            for (int i = start; i < end; i++)
            {
                mutexAsyncSaveBitmap.WaitOne();
                byte[,,] bytes = globalVideoFrames[i];
                mutexAsyncSaveBitmap.ReleaseMutex();

                Bitmap bmp = byteToBMP(bytes);

                mutexAsyncSaveBitmap2.WaitOne();
                globalVideoFramesBitmap[i] = bmp;
                globalProgressValue2++;
                mutexAsyncSaveBitmap2.ReleaseMutex();
            }
        }
        private void colorToMinecraftWool(int[,] pallit, int R, int G, int B, out byte NR, out byte NG, out byte NB)
        {
            double maxDist = 1000;
            int maxDistID = -1;

            for (int i = 0; i < pallit.GetLength(0); i++)
            {
                double thisDist = getDistanceFromColor(R, G, B, pallit[i, 0], pallit[i, 1], pallit[i, 2]);

                if (maxDist > thisDist)
                {
                    maxDist = thisDist;
                    maxDistID = i;
                }

            }
            NR = (byte)pallit[maxDistID, 0];
            NG = (byte)pallit[maxDistID, 1];
            NB = (byte)pallit[maxDistID, 2];
        }
        private void consoleWriteLine(string msg)
        {
            if (globalConsole)
            {
                msg = msg.Replace("\n", "\r\n");
                if (InvokeRequired)
                {
                    var updateTB = new Action(() => textBox2.Text += $"{msg}\r\n");
                    textBox2.Invoke(updateTB);
                }
            }
        }
        private void tryToDeleteTempFiles()
        {
            string[] tempDirs = Directory.GetDirectories(".");
            string[] tempFiles = Directory.GetFiles(".");
            for(int i=0; i<tempDirs.Length; i++)
            {
                try
                {
                    if (tempDirs[i].IndexOf("out") == -1)
                        Directory.Delete(tempDirs[i], true);
                }
                catch
                {

                }
            }
            for(int i=0; i<tempFiles.Length; i++)
            {
                if (tempFiles[i].IndexOf("temp.mp3") != -1)
                {
                    File.Delete(tempFiles[i]);
                }
            }
        }

        private int[,] getGlobalPallit(int index)
        {
            string[][] pallit = new string[5][];
            pallit[0] = new[] { "#DDDDDD", "#DB7D3E", "#B350BC", "#6B8AC9", "#B1A627", "#41AE38", "#D08499", "#404040", "#9AA1A1", "#2E6E89", "#7E3DB5", "#2E388D", "#4F321F", "#35461B", "#963430", "#191616" };
            pallit[1] = new[] { "#E9ECEC", "#F07613", "#BD44B3", "#3AAFD9", "#F8C627", "#70B919", "#ED8DAC", "#3E4447", "#8E8E86", "#158991", "#792AAC", "#35399D", "#724728", "#546D1B", "#A12722", "#141519" };
            pallit[2] = new[] { "#0000db", "#00b6db", "#00db6d", "#ffb600", "#ff926d", "#db0000", "#dbdbdb", "#000000" };
            pallit[3] = new[] { "#000000", "#181818", "#282828", "#383838", "#474747", "#565656", "#646464", "#717171", "#7E7E7E", "#8C8C8C", "#9B9B9B", "#ABABAB", "#BDBDBD", "#D1D1D1", "#E7E7E7", "#FFFFFF" };
            pallit[4] = new[] { "#1a1c2c", "#5d275d", "#b13e53", "#ef7d57", "#ffcd75", "#a7f070", "#38b764", "#257179", "#29366f", "#3b5dc9", "#41a6f6", "#73eff7", "#f4f4f4", "#94b0c2", "#566c86", "#333c57" };

            return colorsToINTArr(pallit[index]);
        }
        private int[,] colorsToINTArr(string[] str)
        {
            int[,] rolfan = new int[str.GetLength(0), 3];
            int i = 0;
            foreach (string strs in str)
            {
                Color cls = ColorTranslator.FromHtml(strs);
                rolfan[i, 0] = cls.R;
                rolfan[i, 1] = cls.G;
                rolfan[i, 2] = cls.B;
                i++;
            }
            return rolfan;
        }
        private int[,] quickSort(int[,] array, int leftIndex, int rightIndex)
        {
            int i = leftIndex;
            int j = rightIndex;
            int pivot = array[leftIndex, 1];

            while (i <= j)
            {
                while (array[i, 1] < pivot)
                {
                    i++;
                }

                while (array[j, 1] > pivot)
                {
                    j--;
                }
                if (i <= j)
                {
                    int temp0 = array[i, 0];
                    int temp = array[i, 1];

                    array[i, 0] = array[j, 0];
                    array[i, 1] = array[j, 1];

                    array[j, 0] = temp0;
                    array[j, 1] = temp;
                    i++;
                    j--;
                }
            }

            if (leftIndex < j)
                quickSort(array, leftIndex, j);
            if (i < rightIndex)
                quickSort(array, i, rightIndex);

            return array;
        }
        private int searchNeight(int a, int w, int h = -1)
        {
            if (h == -1)
            {
                if (w % a == 0) return a;
                int n = 1;
                int last = -1;
                while (n < a + 3)
                {
                    if (w % n == 0)
                    {
                        last = n;
                    }

                    n++;
                }
                return last;
            }
            else
            {
                if (w % a == 0 && h % a == 0) return a;
                int n = 1;
                int last = -1;
                while (n < a + 3)
                {
                    if (w % n == 0 && h % n == 0)
                    {
                        last = n;
                    }

                    n++;
                }
                return last;
            }
        }

        private Bitmap byteToBMP(byte[,,] data)
        {
            Bitmap bmp = new Bitmap(data.GetLength(2), data.GetLength(1));

            for (int h = 0; h < bmp.Height; h++)
                for (int w = 0; w < bmp.Width; w++)
                    bmp.SetPixel(w, h, Color.FromArgb(255, data[0, h, w], data[1, h, w], data[2, h, w]));

            return bmp;
        }
        private Bitmap bmpScrath(Bitmap bitmap, int w, int h)
        {
            Bitmap newI = new Bitmap(w, h);
            using (Graphics gr = Graphics.FromImage(newI))
            {
                gr.SmoothingMode = SmoothingMode.HighSpeed;
                gr.CompositingQuality = CompositingQuality.HighSpeed;
                gr.InterpolationMode = InterpolationMode.NearestNeighbor;
                gr.DrawImage(bitmap, new Rectangle(0, 0, w, h)); 
            }
            return newI;
        }

        private byte[,,] Pixelion(byte[,,] data, int pix)
        {
            int ww = data.GetLength(2),
                hh = data.GetLength(1);

            byte[,,] datan = new byte[3, hh, ww];

            for (int h = 0; h < hh; h+=pix)
            {
                for (int w = 0; w < ww; w+=pix)
                {
                    int AR = 0,
                        AG = 0,
                        AB = 0;
                    for(int j=0; j<pix; j++)
                    {
                        for(int i=0; i<pix; i++)
                        {
                            AR += data[0, h + j, w + i];
                            AG += data[1, h + j, w + i];
                            AB += data[2, h + j, w + i];
                        }
                    }
                    AR /= (int)Math.Pow(pix, 2);
                    AG /= (int)Math.Pow(pix, 2);
                    AB /= (int)Math.Pow(pix, 2);
                    for (int j = 0; j < pix; j++)
                    {
                        for (int i = 0; i < pix; i++)
                        {
                            datan[0, h + j, w + i] = (byte)AR;
                            datan[1, h + j, w + i] = (byte)AG;
                            datan[2, h + j, w + i] = (byte)AB;
                        }
                    }
                }
            }
            return datan;
        }
        private byte[,,] Pixelion_Native(byte[,,] data, int pix)
        {
            int ww = data.GetLength(2),
                hh = data.GetLength(1);

            byte[,,] datan = new byte[3, hh, ww];

            for (int h = 0; h < hh; h += pix)
            {
                for (int w = 0; w < ww; w += pix)
                {
                    int AR = -1,
                        AG = -1,
                        AB = -1;
                    for (int j = 0; j < pix; j++)
                    {
                        for (int i = 0; i < pix; i++)
                        {
                            if(AR == 0)
                            {
                                AR = data[0, h + j, w + i];
                                AG = data[1, h + j, w + i];
                                AB = data[2, h + j, w + i];
                            }
                            else
                            {
                                AR = (AR + data[0, h + j, w + i]) / 2;
                                AG = (AG + data[1, h + j, w + i]) / 2;
                                AB = (AB + data[2, h + j, w + i]) / 2;
                            }

                        }
                    }
                    for (int j = 0; j < pix; j++)
                    {
                        for (int i = 0; i < pix; i++)
                        {
                            datan[0, h + j, w + i] = (byte)AR;
                            datan[1, h + j, w + i] = (byte)AG;
                            datan[2, h + j, w + i] = (byte)AB;
                        }
                    }
                }
            }
            return datan;
        }
        private byte[,,] Minecraftion(byte[,,] data, int[,] pallit, int pix = -1)
        {
            if (pix == -1)
            {
                for (int h = 0; h < data.GetLength(1); h++)
                {
                    for (int w = 0; w < data.GetLength(2); w++)
                    {
                        byte r = data[0, h, w];
                        byte g = data[1, h, w];
                        byte b = data[2, h, w];

                        colorToMinecraftWool(pallit, r, g, b, out data[0, h, w], out data[1, h, w], out data[2, h, w]);

                    }
                }
                return data;
            }
            else
            {
                for (int h = 0; h < data.GetLength(1); h += pix)
                {
                    for (int w = 0; w < data.GetLength(2); w += pix)
                    {
                        byte r = data[0, h, w];
                        byte g = data[1, h, w];
                        byte b = data[2, h, w];

                        colorToMinecraftWool(pallit, r, g, b, out r, out g, out b);

                        for (int j = 0; j < pix; j++)
                        {
                            for (int i = 0; i < pix; i++)
                            {
                                data[0, h + j, w + i] = r;
                                data[1, h + j, w + i] = g;
                                data[2, h + j, w + i] = b;
                            }
                        }
                    }
                }
                return data;
            }
        }
        private unsafe byte[,,] BitmapToByteRgbQ(Bitmap bmp)
        {
            int width = bmp.Width,
                height = bmp.Height;
            byte[,,] res = new byte[3, height, width];
            BitmapData bd = bmp.LockBits(new Rectangle(0, 0, width, height), ImageLockMode.ReadOnly,
                System.Drawing.Imaging.PixelFormat.Format24bppRgb);
            try
            {
                byte* curpos;
                fixed (byte* _res = res)
                {
                    byte* _r = _res, _g = _res + width * height, _b = _res + 2 * width * height;
                    for (int h = 0; h < height; h++)
                    {
                        curpos = ((byte*)bd.Scan0) + h * bd.Stride;
                        for (int w = 0; w < width; w++)
                        {
                            *_b = *(curpos++); ++_b;
                            *_g = *(curpos++); ++_g;
                            *_r = *(curpos++); ++_r;
                        }
                    }
                }
            }
            finally
            {
                bmp.UnlockBits(bd);
            }
            return res;
        }

        private double getDistanceFromColor(int r1, int g1, int b1, int r2, int g2, int b2)
        {
            return Math.Sqrt(Math.Pow(r2 - r1, 2) + Math.Pow(g2 - g1, 2) + Math.Pow(b2 - b1, 2));
        }

        private string sha256_hash(String value)
        {
            StringBuilder Sb = new StringBuilder();

            using (SHA256 hash = SHA256Managed.Create())
            {
                Encoding enc = Encoding.UTF8;
                Byte[] result = hash.ComputeHash(enc.GetBytes(value));

                foreach (Byte b in result)
                    Sb.Append(b.ToString("x2"));
            }

            string sw = Sb.ToString();
            sw = sw.Remove(sw.Length / 4);
            return sw;
        }

        private ulong getFrameRate(string path)
        {
            string dir = Path.GetDirectoryName(path);
            string file = Path.GetFileName(path);


            Type shellAppType = Type.GetTypeFromProgID("Shell.Application");
            dynamic shell = Activator.CreateInstance(shellAppType);
            dynamic folder = shell.NameSpace(dir);
            dynamic folderItem = folder.ParseName(file);
            string value = folder.GetDetailsOf(folderItem, 27).ToString();

            string[] dur = value.Split(":");

            int h = Convert.ToInt16(dur[0]);
            int m = Convert.ToInt16(dur[1]);
            int s = Convert.ToInt16(dur[2]);

            return (ulong)(h * 3600 + m * 60 + s);

        }

        private string[] sortFilePath(string[] paths)
        {
            Regex rgx = new Regex(@"(.*?)\\img([0-9]*).jpg");

            int[,] arr = new int[paths.Length, 2];

            for (int i = 0; i < paths.Length; i++)
            {
                arr[i, 0] = i;
                arr[i, 1] = Convert.ToInt32(Regex.Match(paths[i], @".*?\\img([0-9]*).png").Groups[1].Value);
            }

            arr = quickSort(arr, 0, arr.GetLength(0) - 1);

            int s = 0;

            string[] newpath = new string[paths.Length];
            for (int i = 0; i < paths.Length; i++)
            {
                newpath[i] = paths[arr[i, 0]];
            }

            return newpath;
        }

        IEnumerable<BitmapVideoFrameWrapper> CreateFramesSD(int count)
        {
            for(int i=0; i<count; i++)
            {
               mPB3.WaitOne();
                globalProgressValue3 = i;
               mPB3.ReleaseMutex();
                using BitmapVideoFrameWrapper wrapBit = new(globalVideoFramesBitmap[i]);
                yield return wrapBit;
            }
            globalEndFlag = false;
        }
    }
}