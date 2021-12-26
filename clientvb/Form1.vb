Imports System
Imports System.Data
Imports System.Diagnostics
Imports System.IO
Imports System.Linq
Imports System.Net.Http
Imports System.Text.Json
Imports System.Threading
Imports System.Threading.Tasks
Imports System.Windows.Forms
Imports winformasync.common

Partial Public Class Form1
    Inherits Form

    Private ReadOnly timer As Timers.Timer
    Private ReadOnly textbox As TextBox
    Private ReadOnly blockingButton As Button
    Private ReadOnly oldThreadButton As Button
    Private ReadOnly asyncThreadButton As Button
    Private ReadOnly asyncIOButton As Button
    Private ReadOnly _httpClient As HttpClient = New HttpClient()

    Public Sub New()
        Dim font = New Font("Yu Gothic UI", 16)
        Dim timerTextBox = New TextBox() With {
            .Font = font,
            .Dock = DockStyle.Top
        }
        timer = New Timers.Timer() With {
            .Interval = 1000
        }
        timer.Start()
        Dim sw = Stopwatch.StartNew()
        AddHandler timer.Elapsed,
            Sub(obj, ev)
                Me.Invoke(New Action(Sub()
                                         If Me.Disposing OrElse Me.IsDisposed Then Return
                                         timerTextBox.Text = $"{sw.ElapsedMilliseconds / 1000.0:0}"
                                     End Sub))
            End Sub

        textbox = New TextBox() With {
            .Multiline = True,
            .Font = font,
            .Dock = DockStyle.Fill
        }
        blockingButton = New Button() With {
            .Text = "同期処理",
            .Dock = DockStyle.Bottom
        }
        oldThreadButton = New Button() With {
            .Text = "非同期スレッド処理（旧）",
            .Dock = DockStyle.Bottom
        }
        asyncThreadButton = New Button() With {
            .Text = "非同期スレッド処理（新）",
            .Dock = DockStyle.Bottom
        }
        asyncIOButton = New Button() With {
            .Text = "非同期IO処理",
            .Dock = DockStyle.Bottom
        }
        Dim buttons As Button() = {blockingButton, oldThreadButton, asyncThreadButton, asyncIOButton}

        For Each button As Button In buttons
            button.Height = 50
        Next

        Me.Controls.AddRange(New Control() {textbox, timerTextBox, blockingButton, oldThreadButton, asyncThreadButton, asyncIOButton})
        AddHandler Me.blockingButton.Click, AddressOf blockingButton_OnClick
        AddHandler Me.oldThreadButton.Click, AddressOf oldThreadButton_OnClick
        AddHandler Me.asyncThreadButton.Click, AddressOf asyncThreadButton_OnClick
        AddHandler Me.asyncIOButton.Click, AddressOf asyncIOButton_OnClick
    End Sub


#Region "============同期処理==========="

    Private Sub blockingButton_OnClick(ByVal sender As Object, ByVal e As EventArgs)
        Dim res As HttpResponseMessage = _httpClient.Send(CreateGetProductRequest())
        Dim products = Me.JsonToProducts(HttpResToString(res))
        textbox.Text = ToText(products)
    End Sub

#End Region


#Region "============非同期 スレッド（旧）==========="

    Friend Delegate Sub TooBadThreadGetProduct()
    Friend Delegate Sub TooBadShowText(ByVal products As Product())

    Private Sub oldThreadButton_OnClick(ByVal sender As Object, ByVal e As EventArgs)
        Dim thread = New Thread(New ThreadStart(AddressOf RunTooBadThreadGetProduct))
        thread.Start()
    End Sub

    Private Sub RunTooBadThreadGetProduct()
        Dim res As HttpResponseMessage = _httpClient.Send(CreateGetProductRequest())
        Dim products = Me.JsonToProducts(HttpResToString(res))

        If False Then
            ' サブスレッドからUIを更新するのでエラー
            textbox.Text = ToText(products)
        Else
            ' UIスレッドに戻して処理する必要がある
            Dim showMethod As TooBadShowText = AddressOf WorstShowText
            Me.Invoke(showMethod, New Object() {products})
        End If
    End Sub

    Private Sub WorstShowText(ByVal products As Product())
        textbox.Text = ToText(products)
    End Sub

#End Region

#Region "============非同期 スレッド（新）==========="
    Private Async Sub asyncThreadButton_OnClick(ByVal sender As Object, ByVal e As EventArgs)
        If False Then
            ' 
            Dim products As Product() = Await Task.Run(AddressOf RunThreadGetProduct)
            textbox.Text = ToText(products)
        Else
            ' 上の処理はこれと同じ処理。（糖衣構文）。

            ' こう記載すると new Thread(XXX).Start() とほぼ同じ感じなのが分かる。
            '   戻り値や引数の型がobject型にならないメリット。
            '   処理の状態・結果を管理するクラスが、Thread型から汎用化したTask型になっているだけ。
            Dim task As Task(Of Product()) = Tasks.Task.Factory.StartNew(AddressOf RunThreadGetProduct)

            ' そして、Task型は、taskが終わった次に実行する処理を指定できる。
            '   UIスレッドで実行するかどうかは選べる。
            '  （FromCurrentSynchronizationContext＝同期コンテキスト＝UIスレッド）
            task.ContinueWith(
                Sub(ByVal t As Task(Of Product())) textbox.Text = ToText(t.Result),
                TaskScheduler.FromCurrentSynchronizationContext())
            ' ※
            '     この書き方は`task`の処理がサブスレッドで実行されてる場合に限り問題ない。
            '     処理がUIスレッドで実行されていたらデッドロックする。
            '     難しいけど正確な解説はここ
            '     https://ufcpp.net/study/csharp/sp5_awaitable.html
        End If
    End Sub

    Private Function RunThreadGetProduct() As Product()
        Dim res As HttpResponseMessage = _httpClient.Send(CreateGetProductRequest())
        Return Me.JsonToProducts(HttpResToString(res))
    End Function
#End Region


#Region "============非同期IO==========="
    Private Async Sub asyncIOButton_OnClick(ByVal sender As Object, ByVal e As EventArgs)
        If True Then
            Dim res As HttpResponseMessage = Await _httpClient.SendAsync(CreateGetProductRequest())
            Dim products As Product() = Me.JsonToProducts(Await res.Content.ReadAsStringAsync())
            textbox.Text = ToText(products)
        Else
            ' 非同期IO（シングルスレッド）はawaitなしには記述できない…
            ' （ことはないのかもしれないが恐らくとんでもなく難しい）
        End If
    End Sub

#End Region

#Region "============共通処理==========='"
    Private Function CreateGetProductRequest() As HttpRequestMessage
        Return New HttpRequestMessage(HttpMethod.Get, "http://localhost:5000/api/product?timesec=5")
    End Function

    Private Function JsonToProducts(ByVal productJson As String) As Product()
        If productJson Is Nothing Then
            Return Array.Empty(Of Product)()
        End If

        Dim serializeOptions = New JsonSerializerOptions With {
            .PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            .WriteIndented = True
        }
        Dim res As Product() = JsonSerializer.Deserialize(Of Product())(productJson, serializeOptions)
        Return If(res, Array.Empty(Of Product)())
    End Function

    Private Function ToText(ByVal products As Product()) As String
        Return String.Join(Environment.NewLine, products.Select(Function(x) x.ToString()))
    End Function

    Private Function HttpResToString(ByVal res As HttpResponseMessage) As String
        Dim reader = New StreamReader(res.Content.ReadAsStream())
        Return reader.ReadToEnd()
    End Function
#End Region

End Class
