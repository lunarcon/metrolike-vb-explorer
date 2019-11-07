Public Class Userinfo
    Private Sub Userinfo_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        Location = New Point(Form1.Location.X + Form1.UserBtn.Location.X - Form1.UserBtn.Width + 5, Form1.Location.Y + 7 + Form1.Titlebar.Height - Form1.Header.Height - 2)
        PictureBox1.BackgroundImage = Form1.PictureBox1.BackgroundImage
        Label1.Text = Security.Principal.WindowsIdentity.GetCurrent().Name.ToString
        Dim auth As String = Security.Principal.WindowsIdentity.GetCurrent().AuthenticationType.ToString
        If auth = "CloudAP" Then
            Label2.Text = "Microsoft Account"
        Else
            Label2.Text = "Local Account"
        End If
    End Sub

    Protected Overrides ReadOnly Property CreateParams As CreateParams
        Get
            Dim cp As CreateParams = MyBase.CreateParams
            cp.Style = (cp.Style Or 262144)
            'WS_SIZEBOX;
            Return cp
        End Get
    End Property
    Private Sub Form1_LostFocus(ByVal sender As Object, ByVal e As EventArgs) _
        Handles Me.LostFocus

        Me.Close()
    End Sub

    Private Sub Label3_Click(sender As Object, e As EventArgs) Handles Label3.Click
        Process.Start("https://accounts.microsoft.com/")

    End Sub

    Private Sub Label2_Click(sender As Object, e As EventArgs) Handles Label2.Click
        Label3.ForeColor = SystemColors.Highlight
    End Sub

    Private Sub Label3_MouseEnter(sender As Object, e As EventArgs) Handles Label3.MouseEnter
        Label3.ForeColor = SystemColors.Highlight
    End Sub

    Private Sub Label3_MouseLeave(sender As Object, e As EventArgs) Handles Label3.MouseLeave
        Label3.ForeColor = Color.SteelBlue
    End Sub
End Class