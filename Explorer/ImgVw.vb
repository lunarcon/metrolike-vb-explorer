Public Class ImgVw
    Protected Overrides ReadOnly Property CreateParams As CreateParams
        Get
            Dim cp As CreateParams = MyBase.CreateParams
            cp.Style = (cp.Style Or 262144)
            'WS_SIZEBOX;
            Return cp
        End Get
    End Property

    Private Sub ImgVw_Load(sender As Object, e As EventArgs) Handles MyBase.Load

        Location = New Point(MousePosition.X + 10, MousePosition.Y + 10)

        PictureBox1.Image = Image.FromFile(Form1.FPath.Text.ToString & Form1.lvFiles.FocusedItem.Text.ToString)
    End Sub

    Private Sub ImgVw_LostFocus(sender As Object, e As EventArgs) Handles Me.LostFocus
        PictureBox1.Image.Dispose()
        Close()
    End Sub

    Private Sub Timer1_Tick(sender As Object, e As EventArgs)
    End Sub
End Class