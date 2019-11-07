Imports System.IO
Imports System.Runtime.InteropServices
Imports System.Text
Imports System.Drawing.Drawing2D
Imports System.Windows.Forms
Imports Explorer.ExtractLargeIconFromFile
Imports System.Math
Public Class DrvInf
    Private Sub DrvInf_Load(sender As Object, e As EventArgs) Handles MyBase.Load
    End Sub

    Private Sub DrawProgress(g As Graphics, rect As Rectangle, percentage As Single, progresswidth As Single, forecolor As Color, backcolor As Color, textcolor As Color)
        'work out the angles for each arc
        Dim progressAngle = CSng(360 / 100 * percentage)
        Dim remainderAngle = 360 - progressAngle

        'create pens to use for the arcs
        Using progressPen As New Pen(forecolor, progresswidth), remainderPen As New Pen(backcolor, progresswidth)
            'set the smoothing to high quality for better output
            g.SmoothingMode = Drawing2D.SmoothingMode.AntiAlias
            'draw the blue and white arcs
            g.DrawArc(progressPen, rect, -90, progressAngle)
            g.DrawArc(remainderPen, rect, progressAngle - 90, remainderAngle)
        End Using

        'draw the text in the center by working out how big it is and adjusting the co-ordinates accordingly
        Using fnt As New Font(Me.Font.FontFamily, 14)
            Dim text As String = percentage.ToString + "%"
            Dim textSize = g.MeasureString(text, fnt)
            Dim textPoint As New Point(CInt(rect.Left + (rect.Width / 2) - (textSize.Width / 2)), CInt(rect.Top + (rect.Height / 2) - (textSize.Height / 2)))
            'now we have all the values draw the text
            Dim b = New SolidBrush(textcolor)
            g.DrawString(text, fnt, b, textPoint)
        End Using
    End Sub

    Private Sub Panel1_Paint(sender As Object, e As PaintEventArgs) Handles Panel1.Paint
        DrawProgress(e.Graphics, New Rectangle(10, 10, 100, 100), Val(My.Settings.FocusPercentage), 8, SystemColors.Highlight, SystemColors.ControlDarkDark, SystemColors.ControlLight)
    End Sub
End Class