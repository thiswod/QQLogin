using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace QQLogin
{
    public class InputDialog : Form
    {
        private Label labelPrompt;
        private TextBox textBox;
        private Button button1;
        private Button button2;

        public string InputText => textBox.Text; // 公共属性用于获取输入的内容

        public string PromptText
        {
            get => labelPrompt.Text;
            set => labelPrompt.Text = value;
        }

        public InputDialog(string title="", string prompt = "请输入：")
        {
            // 窗口设置
            this.Text = title;
            this.StartPosition = FormStartPosition.CenterScreen;
            this.FormBorderStyle = FormBorderStyle.FixedDialog; // 固定对话框尺寸
            //最大化按钮和最小化按钮取消
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Size = new Size(400, 200);

            // 创建提示标签
            labelPrompt = new Label();
            labelPrompt.Text = prompt;
            labelPrompt.Location = new Point(50, 0);
            labelPrompt.AutoSize = true;
            this.Controls.Add(labelPrompt);

            // 创建输入框
            textBox = new TextBox();
            textBox.Location = new Point(50, 30);
            textBox.Size = new Size(300, 20);
            // 添加KeyDown事件处理，确保按回车键可以实现确定效果
            textBox.KeyDown += TextBox_KeyDown;
            this.Controls.Add(textBox);

            // 创建第一个按钮
            button1 = new Button();
            button1.Text = "确定";
            button1.Location = new Point(100, 80);
            button1.Size = new Size(80, 30);
            //button1.Click += (sender, e) => InputAuthCode();
            button1.DialogResult = DialogResult.OK; // 设置对话框结果
            this.Controls.Add(button1);

            // 创建第二个按钮
            button2 = new Button();
            button2.Text = "取消";
            button2.Location = new Point(220, 80);
            button2.Size = new Size(80, 30);
            //button2.Click += (sender, e) => this.Close(); ;
            button2.DialogResult = DialogResult.Cancel; // 设置对话框结果
            this.Controls.Add(button2);

            // 在创建完按钮后再设置AcceptButton和CancelButton，确保引用有效
            this.AcceptButton = button1; // 设置回车默认按钮
            this.CancelButton = button2; // 设置ESC默认按钮
        }

        /// <summary>
        /// 处理输入框的按键事件，确保按回车键可以触发确定按钮的点击事件
        /// </summary>
        private void TextBox_KeyDown(object sender, KeyEventArgs e)
        {
            // 如果按下的是回车键
            if (e.KeyCode == Keys.Enter)
            {
                // 模拟点击确定按钮
                button1.PerformClick();
                // 阻止默认行为
                e.SuppressKeyPress = true;
            }
        }
    }
}
