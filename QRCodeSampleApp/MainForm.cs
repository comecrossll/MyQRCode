using System;
using System.ComponentModel;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Drawing.Imaging;
using System.Drawing.Printing;
using System.Threading;
using System.Globalization;
using ThoughtWorks.QRCode.Codec;
using ThoughtWorks.QRCode.Codec.Data;
using ThoughtWorks.QRCode.Codec.Util;
using System.IO;
using WinSearchFile;
using System.Text.RegularExpressions;
using System.Security.Cryptography;
using System.Diagnostics;


namespace QRCodeSample
{
    public partial class MainForm : Form
    {
        private ComponentResourceManager res = new ComponentResourceManager(typeof(MainForm));
        CspParameters cspPas = new CspParameters() { KeyContainerName = "MyRSA"};
        private string[] fixedKey = { "dddddddd", "dddddddddddddddd", "dddddddddddddddddddddddd" };
        private static string RSAKey = "";
        private static string OpenedFileName = "";
        private static bool useHybridEncryption;
        private static bool running = false;

        public MainForm()
        {
            InitializeComponent();
            this.skinEngine1.SkinFile = @"Skins\MSN.ssk";   // 选择皮肤
        }

        private void frmSample_Load(object sender, EventArgs e)
        {
            cboEncoding.SelectedIndex = 1;
            cboVersion.SelectedIndex = 0;
            cboCorrectionLevel.SelectedIndex = 1;
            cboEncryptAlgo.SelectedIndex = cboEncryptAlgo.Items.Count - 1;
            cboDecryptAlgo.SelectedIndex = cboEncryptAlgo.Items.Count - 1;
            cspPas.KeyContainerName = "MyRSA";
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void btnEncode_Click(object sender, EventArgs e)
        {
            if (txtEncodeData.Text.Trim() == String.Empty)
            {
                MessageBox.Show("Data must not be empty.");
                return;
            }
            try
            {
                useHybridEncryption = false;
                RSAKey = "";
                ShowStatusLabel(sender, e);

                string data = txtEncodeData.Text;
                string key = txtEncodeKey.Text;

                data = data.TrimEnd();

                #region 加密
                //此处可以对data进行加密
                int encryption = cboEncryptAlgo.SelectedIndex;
                switch (encryption)
                {
                    case 0:
                        //混合
                        string tempKey = new Random().Next(10000000, 99999999).ToString();
                        RSAKey = (RSA.Operate.Encrypt(tempKey, cspPas))[0];
                        useHybridEncryption = true;
                        data = DES.Operate.Encrypt(data, tempKey);
                        break;

                    case 1:
                        //des 
                        //密钥为8位
                        if (String.IsNullOrEmpty(key))
                        {
                            key = fixedKey[0];
                        }
                        data = DES.Operate.Encrypt(data, key);
                        break;

                    case 2:
                        //RSA
                        data = RSA.Operate.Encrypt(data, cspPas)[0];
                        break;

                    default:
                        break;
                }
                #endregion

                #region 二维码编码
                Operate.QRcodeHelper.QRCodeInput qrCodeInput = new Operate.QRcodeHelper.QRCodeInput()
                {
                    Source = data,
                    QRCodeEncodeMode = QRCodeEncoder.ENCODE_MODE.BYTE,
                    QRCodeScale = 4,
                    QRCodeErrorCorrect = QRCodeEncoder.ERROR_CORRECTION.M,
                    QRCodeVersion = 0,

                };

                int encoding = cboEncoding.SelectedIndex;
                switch (encoding)
                {
                    case 0:
                        qrCodeInput.QRCodeEncodeMode = QRCodeEncoder.ENCODE_MODE.ALPHA_NUMERIC;
                        break;

                    case 1:
                        qrCodeInput.QRCodeEncodeMode = QRCodeEncoder.ENCODE_MODE.BYTE;
                        break;

                    case 2:
                        qrCodeInput.QRCodeEncodeMode = QRCodeEncoder.ENCODE_MODE.NUMERIC;
                        break;

                    default:
                        qrCodeInput.QRCodeEncodeMode = QRCodeEncoder.ENCODE_MODE.BYTE;
                        break;
                }

                qrCodeInput.QRCodeScale = Convert.ToInt32(txtSize.Text);
                qrCodeInput.QRCodeVersion = Convert.ToInt32(cboVersion.Text);

                int errorCorrect = cboCorrectionLevel.SelectedIndex;
                switch (encoding)
                {
                    case 0:
                        qrCodeInput.QRCodeErrorCorrect = QRCodeEncoder.ERROR_CORRECTION.L;
                        break;

                    case 1:
                        qrCodeInput.QRCodeErrorCorrect = QRCodeEncoder.ERROR_CORRECTION.M;
                        break;

                    case 2:
                        qrCodeInput.QRCodeErrorCorrect = QRCodeEncoder.ERROR_CORRECTION.Q;
                        break;

                    case 3:
                        qrCodeInput.QRCodeErrorCorrect = QRCodeEncoder.ERROR_CORRECTION.H;
                        break;

                    default:
                        qrCodeInput.QRCodeErrorCorrect = QRCodeEncoder.ERROR_CORRECTION.M;
                        break;
                }
                #endregion

                txtEncodeData.Text = data;
                picEncode.Image = Operate.QRcodeHelper.Encode(qrCodeInput);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            if (picEncode.Image == null)
            {
                return;
            }
            if (picDecode.Image != null)//已修正
            {
                picDecode.Image.Dispose();
                picDecode.Image = null;
            }

            SaveFileDialog SVD = new SaveFileDialog();
            SVD.Filter = "JPeg Image|*.jpg|Bitmap Image|*.bmp|Gif Image|*.gif|PNG Image|*.png";
            SVD.Title = "Save";
            SVD.FileName = string.Empty;
            SVD.ShowDialog();

            // If the file name is not an empty string open it for saving.
            if (SVD.FileName != "")
            {
                // Saves the Image via a FileStream created by the OpenFile method.
                System.IO.FileStream fs = (System.IO.FileStream)SVD.OpenFile();
                // Saves the Image in the appropriate ImageFormat based upon the
                // File type selected in the dialog box.
                // NOTE that the FilterIndex property is one-based.
                switch (SVD.FilterIndex)
                {
                    case 1:
                        this.picEncode.Image.Save(fs,
                           System.Drawing.Imaging.ImageFormat.Jpeg);
                        break;

                    case 2:
                        this.picEncode.Image.Save(fs,
                           System.Drawing.Imaging.ImageFormat.Bmp);
                        break;

                    case 3:
                        this.picEncode.Image.Save(fs,
                           System.Drawing.Imaging.ImageFormat.Gif);
                        break;
                    case 4:
                        this.picEncode.Image.Save(fs,
                           System.Drawing.Imaging.ImageFormat.Png);
                        break;
                }
                fs.Close();

                if (useHybridEncryption && !String.IsNullOrEmpty(RSAKey))
                {
                    Operate.ReadWriteFile.String2File(RSAKey, SVD.FileName + ".key");
                }
            }
        }

        private void btnPrint_Click(object sender, EventArgs e)
        {
            if (picEncode.Image == null)
            {
                return;
            }

            PrintDialog PD = new PrintDialog();
            PD.UseEXDialog = true;
            PD.Document = new PrintDocument();
            PD.Document.PrintPage += new PrintPageEventHandler((object sender1, PrintPageEventArgs e1) =>
            {
                e1.Graphics.DrawImage(picEncode.Image, 0, 0);
            });

            if (PD.ShowDialog() == DialogResult.OK)
            {
                PD.Document.Print();
            }
        }

        private void btnOpen_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog1 = new OpenFileDialog();
            //openFileDialog1.InitialDirectory = "c:\\";
            openFileDialog1.Filter = "JPeg Image|*.jpg|Bitmap Image|*.bmp|Gif Image|*.gif|PNG Image|*.png|All files (*.*)|*.*";
            openFileDialog1.FilterIndex = 1;
            openFileDialog1.RestoreDirectory = true;
            openFileDialog1.FileName = string.Empty;

            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                if (picEncode.Image != null)//已修正
                {
                    picEncode.Image.Dispose();
                    picEncode.Image = null;
                }

                if (picDecode.Image != null)//已修正
                {
                    picDecode.Image.Dispose();
                    picDecode.Image = null;
                }

                string fileName = openFileDialog1.FileName;
                OpenedFileName = fileName;
                picDecode.Image = new Bitmap(fileName);//这里会占用文件，需要修正

            }
        }

        private void btnDecode_Click(object sender, EventArgs e)
        {
            try
            {
                string data = Operate.QRcodeHelper.Decode(picDecode.Image);

                //此处可以对decodedString进行解密
                #region 解密
                int encryption = cboDecryptAlgo.SelectedIndex;
                string key = txtDecodeKey.Text;

                switch (encryption)
                {
                    case 0:
                        //混合
                        string tempRSAKey = Operate.ReadWriteFile.File2String(OpenedFileName + ".key");
                        string tempKey = RSA.Operate.Decrypt(tempRSAKey, cspPas);
                        data = DES.Operate.Decrypt(data, tempKey);
                        break; 

                    case 1:
                        //des
                        //密钥为8位                        
                        if (String.IsNullOrEmpty(key))
                        {
                            key = fixedKey[0];
                        }
                        data = DES.Operate.Decrypt(data, key);
                        data = data.TrimEnd();
                        break;

                    case 2:
                        //RSA
                        data = RSA.Operate.Decrypt(data, cspPas);
                        break;

                    default:
                        break;
                }
                #endregion

                txtDecodedData.Text = data;
                ShowStatusLabel(sender, e);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void ShowStatusLabel(object sender, EventArgs e)
        {
            if (tabMain.SelectedIndex == 0)
            {
                toolStripStatusLabel1.Text = "内容长度：";
                toolStripStatusLabel2.Text = txtEncodeData.Text.Length.ToString();
            }
            else if (tabMain.SelectedIndex == 1)
            {
                toolStripStatusLabel1.Text = "内容长度：";
                toolStripStatusLabel2.Text = txtDecodedData.Text.Length.ToString();
            }
            else if (tabMain.SelectedIndex == 2)
            {
                toolStripStatusLabel1.Text = "当前状态：";
                toolStripStatusLabel2.Text = "";
            }
            else if (tabMain.SelectedIndex == 3)
            {
                toolStripStatusLabel1.Text = "当前状态：";
                toolStripStatusLabel2.Text = "";
            }
        }

        private void ChangeLanguage_Click(object sender, EventArgs e)
        {
            ToolStripMenuItem CurTSMI = (ToolStripMenuItem)sender;
            int SelectIndex = 0;
            for (int i = 0; i < languageToolStripMenuItem.DropDownItems.Count; i++)
            {
                languageToolStripMenuItem.DropDownItems[i].Text =
                    languageToolStripMenuItem.DropDownItems[i].Text.Replace("√", "");
                languageToolStripMenuItem.DropDownItems[i].Text =
                    languageToolStripMenuItem.DropDownItems[i].Text.Trim();
                if (languageToolStripMenuItem.DropDownItems[i].Equals(CurTSMI))
                {
                    SelectIndex = i;
                }
            }

            switch (SelectIndex)
            {
                case 0:
                    Thread.CurrentThread.CurrentUICulture = CultureInfo.GetCultureInfo("en");
                    break;

                case 1:
                    //更改当前线程的 CultureInfo
                    //zh-CN 为中文，更多的关于 Culture 的字符串请查 MSDN
                    Thread.CurrentThread.CurrentUICulture = CultureInfo.GetCultureInfo("zh-CN");
                    break;

                default:
                    Thread.CurrentThread.CurrentUICulture = CultureInfo.GetCultureInfo("en");
                    break;
            }

            //对当前窗体应用更改后的资源
            ApplyResource();
            ((ToolStripMenuItem)sender).Text += "  √";
        }

        /// <summary>
        /// 应用资源
        /// ApplyResources 的第一个参数为要设置的控件
        /// 第二个参数为在资源文件中的ID，默认为控件的名称
        /// </summary>
        private void ApplyResource()
        {
            try
            {
                //控件
                ApplyResourceForControl(this);

                //菜单
                foreach (ToolStripMenuItem item in this.menuStrip1.Items)
                {
                    ApplyResourceForMenuStrip(item);
                }

                //状态栏
                foreach (ToolStripStatusLabel item in this.statusStrip1.Items)
                {
                    res.ApplyResources(item, item.Name);
                }

                //标题
                res.ApplyResources(this, "$this");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

        }

        private void ApplyResourceForControl(Control ctl)
        {
            if (ctl.HasChildren == false)
            {
                res.ApplyResources(ctl, ctl.Name);
                return;
            }
            else
            {
                res.ApplyResources(ctl, ctl.Name);
                foreach (Control subCtl in ctl.Controls)
                {
                    ApplyResourceForControl(subCtl);
                }
            }
        }

        private void ApplyResourceForMenuStrip(ToolStripMenuItem item)
        {
            if (item.HasDropDownItems == false)
            {
                res.ApplyResources(item, item.Name);
                return;
            }
            else
            {
                res.ApplyResources(item, item.Name);
                foreach (ToolStripMenuItem subItem in item.DropDownItems)
                {
                    ApplyResourceForMenuStrip(subItem);
                }
            }
        }

        private void btnFormat_Click(object sender, EventArgs e)
        {
            try
            {
                //txtEncodeData.Text = txtEncodeData.Text.Replace("##", "");
                txtEncodeData.Text = txtEncodeData.Text.Replace("\r\n", "##");
                txtEncodeData.Text = txtEncodeData.Text.Replace(" ", "##");
                txtEncodeData.Text = txtEncodeData.Text.Replace("\t", "##");
                txtEncodeData.Text = txtEncodeData.Text.Replace("######", "##");
                txtEncodeData.Text = txtEncodeData.Text.Replace("####", "##");
                //txtEncodeData.Text = "##" + txtEncodeData.Text;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnIdentify_Click(object sender, EventArgs e)
        {
            string source = txtDecodedData.Text;
            if (String.IsNullOrEmpty(source))
            {
                return;
            }

            Regex IdentifyDelim = new Regex("##");
            string[] subStr = IdentifyDelim.Split(source);

            try
            {
                txtComCode.Text = subStr[0].Trim();
                txtComName.Text = subStr[1].Trim();
                txtComManager.Text = subStr[2].Trim();
                txtBusinessScope.Text = subStr[3].Trim();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

        }

        private void btnOpen2_Click(object sender, EventArgs e)
        {
            OpenFileDialog OFD = new OpenFileDialog();
            OFD.Filter = "JPeg Image|*.jpg|Bitmap Image|*.bmp|Gif Image|*.gif|PNG Image|*.png";
            OFD.FilterIndex = 1;
            OFD.RestoreDirectory = true;
            OFD.FileName = string.Empty;

            if (OFD.ShowDialog() != DialogResult.OK)
            {
                return;
            }
            txtQRCodePath.Text = OFD.FileName;

            try
            {

                string txt = Operate.QRcodeHelper.Decode(txtQRCodePath.Text);
                txt = txt.Replace("\r\n", " ");
                if (txt.Length > 50)
                {
                    txt = txt.Substring(0, 50);
                    txt += "...";
                }
                txtSearch.Text = txt;

            }
            catch (Exception ex)
            {
                txtSearch.Text = "";
                MessageBox.Show(ex.Message, "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnSelectPath_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog FBD = new FolderBrowserDialog();
            FBD.RootFolder = Environment.SpecialFolder.Desktop;

            if (FBD.ShowDialog() != DialogResult.OK)
            {
                return;
            }
            txtSelectPath.Text = FBD.SelectedPath;
        }

        private void btnSearch_Click(object sender, EventArgs e)
        {
            if (String.IsNullOrEmpty(txtSearch.Text) && String.IsNullOrEmpty(txtQRCodePath.Text))
            {
                return;
            }

            try
            {
                running = true;
                tabMain.AllowSelect = false;
                this.btnSearch.Enabled = false;
                this.btnRemove.Enabled = false;
                AutoResetEvent areFinish = new AutoResetEvent(false);
                Thread waitTime = new Thread(() =>
                {
                    DateTime dtStart = DateTime.Now;
                    this.Invoke(new MethodInvoker(() =>
                    {
                        this.toolStripStatusLabel2.Text = "正在搜索文件和解码二维码";
                    }));
                    areFinish.WaitOne();
                    DateTime dtFinish = DateTime.Now;
                    this.Invoke(new MethodInvoker(() =>
                    {
                        this.toolStripStatusLabel2.Text = "搜索完成，所用时间："
                            + ((int)(dtFinish - dtStart).TotalSeconds).ToString("d") + "秒";

                        this.btnSearch.Enabled = true;
                        this.btnRemove.Enabled = true;
                        tabMain.AllowSelect = true;
                        running = false;
                    }));
                });
                waitTime.IsBackground = true;
                waitTime.Name = "计时线程";
                waitTime.Start();

                string source = String.Empty;
                if (String.IsNullOrEmpty(txtQRCodePath.Text))
                {
                    source = txtSearch.Text;
                }
                else
                {
                    source = Operate.QRcodeHelper.Decode(txtQRCodePath.Text);
                }
                listFileFounded.Items.Clear();
                string Dir = txtSelectPath.Text;
                string SearchPattern = "*.jpg,*.png,*gif,*.bmp";
                FileSearch fs = new FileSearch(Dir, SearchPattern, source, this);
                Thread searchFileThread = new Thread(() =>
                {
                    fs.Start();
                    areFinish.Set();

                });
                searchFileThread.IsBackground = true;
                searchFileThread.Name = "搜索主线程";
                searchFileThread.Start();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void listFileFounded_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (listFileFounded.SelectedItems.Count <= 0)
            {
                return;
            }

            try
            {
                Thread showThread = new Thread(() =>
                {
                    try
                    {
                        this.Invoke(new MethodInvoker(() =>
                        {
                            label17.Text = "解码中";
                        }));

                        string path = "";
                        this.Invoke(new MethodInvoker(() =>
                        {
                            path = listFileFounded.SelectedItems[0].SubItems[1].Text;
                        }));
                        string txt = Operate.QRcodeHelper.Decode(path);
                        txt = txt.Replace("\r\n", " ");
                        if (txt.Length > 50)
                        {
                            txt = txt.Substring(0, 50);
                            txt += "...";
                        }
                        this.Invoke(new MethodInvoker(() =>
                        {
                            label17.Text = txt;
                        }));
                    }
                    catch
                    {
                        this.Invoke(new MethodInvoker(() =>
                        {
                            label17.Text = "解码失败";
                        }));
                    }
                    finally
                    {
                    }
                });
                showThread.Start();
            }
            catch
            {
                label17.Text = "解码失败";
            }
        }

        private void listFileFounded_DoubleClick(object sender, EventArgs e)
        {
            if (listFileFounded.SelectedItems.Count <= 0)
            {
                return;
            }
            try
            {
                string path = listFileFounded.SelectedItems[0].SubItems[1].Text;
                Process.Start("Explorer", "/select," + path);
            }
            catch { }
        }

        private void btnOpen3_Click(object sender, EventArgs e)
        {
            OpenFileDialog OFD = new OpenFileDialog();
            OFD.Filter = "文本文档|*.txt";
            OFD.FilterIndex = 1;
            OFD.RestoreDirectory = false;
            OFD.FileName = string.Empty;

            if (OFD.ShowDialog() != DialogResult.OK)
            {
                return;
            }
            txtFilePath.Text = OFD.FileName;
        }

        private void btnCreate_Click(object sender, EventArgs e)
        {
            if (String.IsNullOrEmpty(txtFilePath.Text))
            {
                return;
            }
            try
            {
                running = true;
                tabMain.AllowSelect = false;
                this.btnCreate.Enabled = false;
                AutoResetEvent finish = new AutoResetEvent(false);
                Thread waitTime = new Thread(() =>
                {
                    DateTime dtStart = DateTime.Now;
                    this.Invoke(new MethodInvoker(() =>
                    {
                        this.toolStripStatusLabel2.Text = "正在读取文件和生成二维码";
                    }));
                    finish.WaitOne();
                    DateTime dtFinish = DateTime.Now;
                    this.Invoke(new MethodInvoker(() =>
                    {
                        this.toolStripStatusLabel2.Text = "生成完成，所用时间："
                            + ((int)(dtFinish - dtStart).TotalSeconds).ToString("d") + "秒";
                        this.btnCreate.Enabled = true;
                        tabMain.AllowSelect = true;
                        running = false;
                    }));
                });
                waitTime.IsBackground = true;
                waitTime.Name = "计时线程";
                waitTime.Start();

                string filePath = txtFilePath.Text;
                listFileReaded.Items.Clear();
                BatchEncode be = new BatchEncode(filePath, this);
                Thread createThread = new Thread(() =>
                {
                    be.Start();
                    finish.Set();
                });
                createThread.IsBackground = true;
                createThread.Name = "生成主线程";
                createThread.Start();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnRemove_Click(object sender, EventArgs e)
        {
            if (listFileFounded.Items.Count <= 0)
            {
                return;
            }

            listFileFounded.BeginUpdate();
            for (int i = 0; i < listFileFounded.Items.Count; i++)
            {
                if (listFileFounded.Items[i].SubItems[2].Text != "符合")
                {
                    listFileFounded.Items.RemoveAt(i);
                    i--;
                }
            }
            for (int i = 0; i < listFileFounded.Items.Count; i++)
            {
                listFileFounded.Items[i].SubItems[0].Text = (i + 1).ToString();
            }
            listFileFounded.EndUpdate();
        }
    }
}