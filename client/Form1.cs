using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using winformasync.common;

namespace winformasync
{
    public partial class Form1 : Form
    {

        private readonly System.Timers.Timer timer;
        private readonly TextBox textbox;
        private readonly Button blockingButton;
        private readonly Button oldThreadButton;
        private readonly Button asyncThreadButton;
        private readonly Button asyncIOButton;

        private readonly HttpClient _httpClient = new HttpClient();

        public Form1()
        {
            InitializeComponent();

            var font = new Font("Yu Gothic UI", 16);
            var timerTextBox = new TextBox()
            {
                Font = font,
                Dock = DockStyle.Top,
            };
            timer = new System.Timers.Timer() { Interval = 1000 };
            timer.Start();
            var sw = Stopwatch.StartNew();
            timer.Elapsed += (obj, ev) => this.Invoke(new Action(() =>
                {
                    if (this.Disposing || this.IsDisposed) return;
                    timerTextBox.Text = $"{sw.ElapsedMilliseconds / 1000.0:0}";
                }));

            textbox = new TextBox()
            {
                Multiline = true,
                Font = font,
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
                    timerTextBox,
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


        #region "============同期処理==========="

        private void blockingButton_OnClick(object? sender, EventArgs e)
        {
            using HttpResponseMessage res = _httpClient.Send(CreateGetProductRequest());
            Product[] products = JsonToProducts(ToString(res));
            textbox.Text = ToText(products);
        }

        #endregion


        #region "============非同期 スレッド（旧）==========="

        delegate void TooBadThreadGetProduct();
        delegate void TooBadShowText(Product[] products);
        private void oldThreadButton_OnClick(object? sender, EventArgs e)
        {
            TooBadThreadGetProduct getMethod = RunTooBadThreadGetProduct;
            var thread = new Thread(new ThreadStart(getMethod));
            thread.Start();
        }

        private void RunTooBadThreadGetProduct()
        {
            using HttpResponseMessage res = _httpClient.Send(CreateGetProductRequest());
            Product[] products = JsonToProducts(ToString(res));

            if (false)
            {
                // サブスレッドからUIを更新するのでエラー
                textbox.Text = ToText(products);
            }
            else
            {
                // UIスレッドに戻して処理する必要がある
                TooBadShowText showMethod = WorstShowText;
                this.Invoke(showMethod, new Object[] { products });
            }
        }

        private void WorstShowText(Product[] products)
        {
            textbox.Text = ToText(products);
        }

        #endregion

        #region "============非同期 スレッド（新）==========="
        private async void asyncThreadButton_OnClick(object? sender, EventArgs e)
        {
            if (false)
            {
                // 
                Product[] products = await Task.Run(RunThreadGetProduct);
                textbox.Text = ToText(products);
            }
            else
            {
                // 上の処理はこれと同じ処理。（糖衣構文）。

                // こう記載すると new Thread(XXX).Start() とほぼ同じ感じなのが分かる。
                //   戻り値や引数の型がobject型にならないメリット。
                //   処理の状態・結果を管理するクラスが、Thread型から汎用化したTask型になっているだけ。
                Task<Product[]> task = Task.Factory.StartNew<Product[]>(RunThreadGetProduct);

                // そして、Task型は、taskが終わった次に実行する処理を指定できる。
                //   UIスレッドで実行するかどうかは選べる。
                //  （FromCurrentSynchronizationContext＝同期コンテキスト＝UIスレッド）
                task.ContinueWith((Task<Product[]> t) =>
                    {
                        textbox.Text = ToText(t.Result); // ※
                    }, TaskScheduler.FromCurrentSynchronizationContext());
                // ※
                //     この書き方は`task`の処理がサブスレッドで実行されてる場合に限り問題ない。
                //     処理がUIスレッドで実行されていたらデッドロックする。
                //     難しいけど正確な解説はここ
                //     https://ufcpp.net/study/csharp/sp5_awaitable.html
            }
        }
        private Product[] RunThreadGetProduct()
        {
            using HttpResponseMessage res = _httpClient.Send(CreateGetProductRequest());
            return JsonToProducts(ToString(res));
        }
        #endregion


        #region "============非同期IO==========="
        private async void asyncIOButton_OnClick(object? sender, EventArgs e)
        {
            if (true)
            {
                using HttpResponseMessage res = await _httpClient.SendAsync(CreateGetProductRequest());
                Product[] products = JsonToProducts(await res.Content.ReadAsStringAsync());
                textbox.Text = ToText(products);

            }
            else
            {
                // 非同期IO（シングルスレッド）はawaitなしには記述できない…
                // （ことはないのかもしれないが恐らくとんでもなく難しい）
            }
        }

        #endregion

        #region "============共通処理==========='
        private HttpRequestMessage CreateGetProductRequest()
        {
            return new HttpRequestMessage(HttpMethod.Get, @"http://localhost:5000/api/product?timesec=5");
        }

        private Product[] JsonToProducts(string? productJson)
        {
            if (productJson is null)
            {
                return Array.Empty<Product>();
            }
            var serializeOptions = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                WriteIndented = true
            };

            Product[]? res = JsonSerializer.Deserialize<Product[]>(productJson, serializeOptions);
            return res ?? Array.Empty<Product>();
        }


        private string ToText(Product[] products)
        {
            return string.Join(Environment.NewLine, products.Select(x => x.ToString()));
        }


        private string ToString(HttpResponseMessage res)
        {
            using var reader = new StreamReader(res.Content.ReadAsStream());
            return reader.ReadToEnd();
        }
        #endregion

    }
}
