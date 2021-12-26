using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace winformasync
{
    public partial class Form1 : Form
    {

        private readonly TextBox textbox;
        private readonly Button blockingButton;
        private readonly Button oldThreadButton;
        private readonly Button asyncThreadButton;
        private readonly Button asyncIOButton;

        public Form1()
        {
            InitializeComponent();

            textbox = new TextBox()
            {
                Multiline = true,
                Dock = DockStyle.Fill,
            };

            blockingButton = new Button()
            {
                Text = "同期処理",
                Dock = DockStyle.Bottom,
            };

            oldThreadButton = new Button()
            {
                Text = "非同期スレッド処理（旧）",
                Dock = DockStyle.Bottom,
            };

            asyncThreadButton = new Button()
            {
                Text = "非同期スレッド処理（新）",
                Dock = DockStyle.Bottom,
            };

            asyncIOButton = new Button()
            {
                Text = "非同期IO処理",
                Dock = DockStyle.Bottom,
            };

            Button[] buttons = new[]
            {
                blockingButton,
                oldThreadButton,
                asyncThreadButton,
                asyncIOButton,
            };


            foreach (Button button in buttons)
            {
                button.Height = 50;
            }

            this.Controls.AddRange(
                new Control[] {
                    textbox,
                    blockingButton,
                    oldThreadButton,
                    asyncThreadButton,
                    asyncIOButton,
                    }
            );

            blockingButton.Click += blockingButton_OnClick;
            oldThreadButton.Click += oldThreadButton_OnClick;
            asyncThreadButton.Click += asyncThreadButton_OnClick;
            asyncIOButton.Click += asyncIOButton_OnClick;
        }

        private void blockingButton_OnClick(object? sender, EventArgs e)
        {
            throw new NotImplementedException();
        }


        private void oldThreadButton_OnClick(object? sender, EventArgs e)
        {
            throw new NotImplementedException();
        }
        private void asyncThreadButton_OnClick(object? sender, EventArgs e)
        {
            throw new NotImplementedException();
        }

        private void asyncIOButton_OnClick(object? sender, EventArgs e)
        {
            throw new NotImplementedException();
        }

    }
}
