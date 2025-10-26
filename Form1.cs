namespace QQLogin
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            QQQuickLogin qQQuickLogin = new QQQuickLogin();
            List<QQQuickLogin.uins> uins = qQQuickLogin.GetUins();
            QQQuickLogin.QQFaceInfo qQFaceInfo = qQQuickLogin.GetFace(uins[0]);
            pictureBox1.ImageLocation = qQFaceInfo.FaceUrl;
            label1.Text = qQFaceInfo.Uin.ToString();
        }
    }
}
