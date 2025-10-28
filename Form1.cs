using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Windows.Forms;
using System.Collections.Generic;
using System.Net.Http;

namespace QQLogin
{
    public partial class Form1 : Form
    {
        QQQuickLogin qQQuickLogin;
        private ImageList imageList1;
        private Dictionary<string, PictureBox> animatedPictures = new Dictionary<string, PictureBox>();

        public Form1()
        {
            InitializeComponent();
            // 设置ListView为大图标视图模式
            listView1.View = View.LargeIcon;
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            try
            {
                // 清空ListView
                listView1.Items.Clear();

                // 初始化ImageList用于显示头像
                imageList1 = new ImageList();
                imageList1.ImageSize = new Size(64, 64); // 增大头像尺寸
                listView1.LargeImageList = imageList1; // 使用大图标列表

                // 创建QQQuickLogin实例
                qQQuickLogin = new QQQuickLogin(QQAppType.Qun);

                // 获取所有QQ用户列表
                List<QQQuickLogin.uins> uinsList = qQQuickLogin.GetUins();

                // 遍历每个用户，获取头像并添加到ListView
                foreach (var uinInfo in uinsList)
                {
                    try
                    {
                        // 获取用户头像信息
                        QQQuickLogin.QQFaceInfo faceInfo = qQQuickLogin.GetFace(uinInfo);

                        // 创建ListView项，UIN作为文本标签（将显示在头像下方）
                        ListViewItem item = new ListViewItem(uinInfo.uin.ToString());

                        // 添加到ListView
                        listView1.Items.Add(item);

                        if (faceInfo.IsSuccess && !string.IsNullOrEmpty(faceInfo.FaceUrl))
                        {
                            // 同步加载头像图片
                            LoadAvatar(item, faceInfo.FaceUrl);
                        }
                    }
                    catch (Exception ex)
                    {
                        // 记录单个用户处理失败的错误，但继续处理其他用户
                        Console.WriteLine($"处理用户 {uinInfo.uin} 时出错: {ex.Message}");
                        // 添加错误信息到ListView
                        ListViewItem errorItem = new ListViewItem(uinInfo.uin.ToString());
                        listView1.Items.Add(errorItem);
                    }
                }
            }
            catch (Exception ex)
            {
                // 显示整体错误信息
                MessageBox.Show($"加载QQ用户信息失败: {ex.Message}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// 同步加载头像图片
        /// </summary>
        private void LoadAvatar(ListViewItem item, string imageUrl)
        {
            try
            {
                if (!string.IsNullOrEmpty(imageUrl))
                {
                    // 使用HttpRequestClass同步下载图片数据
                    byte[] imageData = null;

                    try
                    {
                        // 创建新的HttpRequestClass实例进行请求
                        var http = new HttpRequestClass();
                        http.Open(imageUrl);
                        http.Send();
                        HttpResponseData response = http.GetResponse();

                        if (response.StatusCode == 200 && response.rawResult != null)
                        {
                            imageData = response.rawResult;
                        }
                        else
                        {
                            throw new Exception($"下载失败，状态码: {response.StatusCode}");
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"HttpRequestClass下载图片失败: {ex.Message}");
                        throw; // 重新抛出异常以便上层处理
                    }

                    // 检查是否为GIF格式
                    bool isGif = false;
                    if (imageData.Length > 4)
                    {
                        isGif = imageData[0] == 0x47 && imageData[1] == 0x49 &&
                                imageData[2] == 0x46 && imageData[3] == 0x38;
                    }

                    if (isGif)
                    {
                        // 对于GIF图片，创建一个隐藏的PictureBox来显示动画
                        string key = item.Text;
                        PictureBox animatedBox = new PictureBox();
                        animatedBox.Size = new Size(64, 64);
                        animatedBox.SizeMode = PictureBoxSizeMode.StretchImage;
                        animatedBox.Visible = false; // 隐藏但仍然会播放动画

                        // 创建内存流的副本
                        MemoryStream ms = new MemoryStream(imageData);
                        animatedBox.Image = Image.FromStream(ms);

                        // 添加到字典以便后续访问
                        animatedPictures[key] = animatedBox;
                        this.Controls.Add(animatedBox); // 添加到控件集合以启用动画

                        // 使用第一帧作为静态图标
                        Image frame = animatedBox.Image.Clone() as Image;
                        frame.SelectActiveFrame(FrameDimension.Time, 0);

                        // 添加到ImageList
                        imageList1.Images.Add(frame);
                        item.ImageIndex = imageList1.Images.Count - 1;
                    }
                    else
                    {
                        // 对于非GIF图片，直接添加到ImageList
                        using (MemoryStream ms = new MemoryStream(imageData))
                        {
                            Image avatarImage = Image.FromStream(ms);
                            // 创建适合大图标模式的头像大小
                            Image largeAvatar = new Bitmap(avatarImage, new Size(64, 64));

                            // 添加到ImageList
                            imageList1.Images.Add(largeAvatar);
                            item.ImageIndex = imageList1.Images.Count - 1;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"加载头像失败 {imageUrl}: {ex.Message}");
            }
        }

        private void listView1_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            // 获取鼠标点击位置下的ListView项
            ListViewItem clickedItem = listView1.GetItemAt(e.X, e.Y);

            if (clickedItem != null)
            {
                // 获取选中的QQ号
                string qqNumber = clickedItem.Text;

                qQQuickLogin = new QQQuickLogin(QQAppType.Qun);
                foreach (var uinInfo in qQQuickLogin.GetUins())
                {
                    if (uinInfo.uin.ToString() == qqNumber)
                    {
                        textBox1.Text = qQQuickLogin.Login(uinInfo);
                        break;
                    }
                }
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            using (InputDialog inputDialog = new InputDialog("请输入skey"))
            {
                if (inputDialog.ShowDialog() == DialogResult.OK)
                {
                    int sign = common.Get_G_tk(inputDialog.InputText);
                    //复制
                    Clipboard.SetText(sign.ToString());
                }
            }
        }
    }
}
