Imports System.IO
Imports System.Runtime.InteropServices
Imports System.Text
Imports System.Drawing.Drawing2D
Imports System.Windows.Forms
Imports Explorer.ExtractLargeIconFromFile
Imports System.Math
Imports ProgressBarEx
Imports Microsoft.Toolkit.Forms.UI.XamlHost

Class Form1
    Dim IsMouseDown As Boolean
    Dim startPoint
    Dim expl = False
    Dim copyfilename As String = ""
    Dim copyfile As String = ""
    Dim tr = False
    Dim cut = False
    Dim down As Boolean = False
    Dim renfile As String = ""

    Private Sub AddIcon(ByVal fname As String)
        Dim size = CType(ShellEx.IconSizeEnum.LargeIcon48, ShellEx.IconSizeEnum)
        iIconList.Images.Add(fname, ShellEx.GetBitmapFromFilePath(fname, size))
    End Sub

    <DllImport("shell32", EntryPoint:="#261", CharSet:=CharSet.Unicode, PreserveSig:=False)>
    Public Shared Sub GetUserTilePath(username As String, whatever As UInt32, picpath As StringBuilder, maxLength As Integer)
    End Sub

    Public Function GetUserTilePath(username As String) As String
        Dim sb As StringBuilder
        sb = New StringBuilder(1000)
        GetUserTilePath(username, 2147483648, sb, sb.Capacity)
        Return sb.ToString()
    End Function

    Public Function GetUserTile(username As String) As Image
        Return Image.FromFile(GetUserTilePath(username))
    End Function
    <DllImport("user32.dll", EntryPoint:="DestroyIcon")>
    Private Shared Function DestroyIcon(ByVal hIcon As System.IntPtr) As <MarshalAs(UnmanagedType.Bool)> Boolean
    End Function

    Private Structure SHFILEINFO

        Public hIcon As IntPtr

        Public iIcon As Integer

        Public dwAttributes As Integer

        <MarshalAs(UnmanagedType.ByValTStr, SizeConst:=260)>
        Public szDisplayName As String

        <MarshalAs(UnmanagedType.ByValTStr, SizeConst:=80)>
        Public szTypeName As String

    End Structure

    Private Sub TitleBar_MouseUp(ByVal sender As Object, ByVal e As System.Windows.Forms.MouseEventArgs) Handles Titlebar.MouseUp
        IsMouseDown = False

    End Sub

    Private Sub TitleBar_MouseMove(ByVal sender As Object, ByVal e As System.Windows.Forms.MouseEventArgs) Handles Titlebar.MouseMove
        If IsMouseDown Then
            Dim p1 = New Point(e.X, e.Y)
            Dim p2 = PointToScreen(p1)
            Dim p3 = New Point(p2.X - startPoint.X, p2.Y - startPoint.Y)
            Location = p3
            Opacity = 0.95
        End If
    End Sub



    Private Sub TitleBar_MouseDown(ByVal sender As Object, ByVal e As MouseEventArgs) Handles Titlebar.MouseDown

        If e.Button = MouseButtons.Left Then
            Me.Titlebar.Capture = False
            Header.Capture = False
            Const WM_NCLBUTTONDOWN As Integer = &HA1S
            Const HTCAPTION As Integer = 2
            Dim msg As Message = Message.Create(Me.Handle, WM_NCLBUTTONDOWN, New IntPtr(HTCAPTION), IntPtr.Zero)
            Me.DefWndProc(msg)
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
    Private Declare Auto Function SHGetFileInfo Lib "shell32.dll" _
            (ByVal pszPath As String,
             ByVal dwFileAttributes As Integer,
             ByRef psfi As SHFILEINFO,
             ByVal cbFileInfo As Integer,
             ByVal uFlags As Integer) As IntPtr 'Retrieves information about an object in the file system, such as a file, folder, directory, or drive root

    Private Const SHGFI_ICON = &H100 'Icon
    Private Const SHGFI_SMALLICON = &H1 'Small Icon
    Private Const SHGFI_LARGEICON = &H0 ' Large icon
    Private Const FILE_ATTRIBUTE_DIRECTORY As Integer = &H10
    Private Const MAX_PATH = 512 'Path to Icon
    Private Const MAX_TYPE As Integer = 80


    Private Sub AddImages(ByVal strFileName As String)

        Try

            Dim shInfo As SHFILEINFO 'Create File Info Object

            shInfo = New SHFILEINFO() 'Instantiate File Info Object

            shInfo.szDisplayName = New String(vbNullChar, MAX_PATH) 'Get Display Name

            shInfo.szTypeName = New String(vbNullChar, 80) 'Get File Type

            Dim hIcon As IntPtr 'Get File Type Icon Based On File Association

            hIcon = SHGetFileInfo(strFileName, 0, shInfo, Marshal.SizeOf(shInfo), SHGFI_LARGEICON Or SHGFI_ICON)

            Dim MyIcon As Drawing.Bitmap 'Create icon

            MyIcon = Drawing.Icon.FromHandle(shInfo.hIcon).ToBitmap 'Set Icon
            Try
                iIconList.Images.Add(strFileName.ToString(), MyIcon) 'Add To ListView FileNames
            Catch ex As Exception
                GoSearch(FPath.Text)
            End Try

        Catch ex As Exception
            'Dialog.Show()
            'Dialog.Title.Text = "Something went wrong"
            'Dialog.Body.Text = ex.Message
            ' Dialog.TopMost = True
            '  Dialog.BringToFront()
        End Try

    End Sub
    Private Sub RoundPic(pic As PictureBox)
        Dim originalImage = pic.BackgroundImage
        pic.BackgroundImageLayout = ImageLayout.Zoom
        Dim croppedImage As New Bitmap(originalImage.Width, originalImage.Height)
        'Prepare to draw on the new image.
        Using g = Graphics.FromImage(croppedImage)
            Dim path As New GraphicsPath
            path.AddEllipse(0, 0, croppedImage.Width, croppedImage.Height)
            Dim reg As New Region(path)
            'Draw only within the specified ellipse.
            g.Clip = reg
            g.DrawImage(originalImage, Point.Empty)
        End Using
        'Display the new image.
        pic.BackgroundImage = croppedImage
    End Sub
    Private Sub Form1_Load(sender As Object, e As EventArgs) Handles MyBase.Load

        Try
            Button17.PerformClick()
            BtnGeneral.PerformClick()
        Catch ex As Exception
            GoSearch("C:\")
        End Try
        MaximumSize = New Size(Screen.PrimaryScreen.WorkingArea.Width + 13, Screen.PrimaryScreen.WorkingArea.Height + 13)
        FPath.DeselectAll()
        PictureBox1.BackgroundImage = GetUserTile(Security.Principal.WindowsIdentity.GetCurrent().Name)
        RoundPic(PictureBox1)
        UserBtn.Text = GetUserName().ToString
        lvFiles.Columns.Add("File Name", 150, HorizontalAlignment.Left)
        lvFiles.Columns.Add("File Type", 80, HorizontalAlignment.Left)
        lvFiles.Columns.Add("Date Modified", 150, HorizontalAlignment.Left)
    End Sub
    Private Sub lvFiles_MouseDoubleClick(sender As Object, e As MouseEventArgs) Handles lvFiles.MouseDoubleClick
        Dim lstvw As ListView = CType(sender, ListView)
        If lvFiles.FocusedItem.Text = "OneDrive" And FPath.Text = "" Then
            GoSearch("C:\Users\" & GetUserName() & "\OneDrive\")
        Else
            Try
                If lstvw.FocusedItem.Text.Contains(".") Then
                    Process.Start(FPath.Text & lstvw.FocusedItem.Text)
                Else
                    GoSearch(FPath.Text & lstvw.FocusedItem.Text)
                End If
            Catch ex As Exception

                GoSearch(FPath.Text & lstvw.FocusedItem.Text)

            End Try
        End If

    End Sub
    <DllImport("user32.dll", SetLastError:=True, CharSet:=CharSet.Auto)>
    Private Shared Function SendMessage(ByVal hWnd As IntPtr, ByVal Msg As UInteger, ByVal wParam As Integer, ByVal lParam As Integer) As IntPtr
    End Function
    Private Sub GoSearch(pathF As String)
        If CutBtn.Visible = False Then
            CutBtn.Visible = True
            CopyBtn.Visible = True
            PasteBtn.Visible = True
            Button20.Visible = True
            Button18.Visible = True
            Label1.Visible = False
            Label1.BringToFront()
            Button16.Text = ""
            SearchBox.Visible = False
            PanelBufferTop.BackColor = Color.Transparent
            Panel10.BackColor = Color.Transparent
            PanelBufferTop.Height = 15
        End If
        If pathF <> "" Then
            For Each a As Control In Panel7.Controls
                If a.Name.Contains("Pbar") Then
                    Panel7.Controls.Remove(a)
                    a.Dispose()
                    Panel7.Refresh()
                    lvFiles.BringToFront()
                    Panel11.BringToFront()
                    SortPanel.BringToFront()
                    TextBox1.BringToFront()
                End If
            Next
            lvFiles.ContextMenuStrip = ContextMenuStrip1
            Button20.Visible = True
            lvFiles.View = My.Settings.View
            lvFiles.LargeImageList = iIconList
            lvFiles.SmallImageList = iIconList
            expl = True
            startPoint = New Point(Location.X, Location.Y)
            If pathF.EndsWith("\") = False Then
                FPath.Text = pathF & "\"
            Else
                FPath.Text = pathF
            End If
            Dim FileExtension As String 'Stores File Extension

            Dim SubItemIndex As Integer 'Sub Item Counter

            Dim DateMod As String 'Stores Date Modified Of File

            lvFiles.Items.Clear() 'Clear Existing Items
            Try
                iIconList.Images.Clear()

                For Each i In My.Computer.FileSystem.GetDirectories(pathF)
                    'Environment.GetFolderPath(Environment.SpecialFolder.MyPictures)
                    Dim size = CType(ShellEx.IconSizeEnum.LargeIcon48, ShellEx.IconSizeEnum)
                    iIconList.Images.Add(i, ShellEx.GetBitmapFromFolderPath(i, size))
                    lvFiles.Items.Add(i.Substring(i.LastIndexOf("\") + 1), iIconList.Images.Count - 1)
                    lvFiles.Items(SubItemIndex).SubItems.Add("Folder")
                    SubItemIndex += 1
                Next
            Catch ex As Exception
            End Try
            Dim folder As String = CStr(pathF) 'Folder Name

            If Not folder Is Nothing AndAlso Directory.Exists(folder) Then

                Try

                    For Each file As String In Directory.GetFiles(folder) 'Get Files In Folder

                        FileExtension = Path.GetExtension(file) 'Get File Extension(s)

                        DateMod = System.IO.File.GetLastWriteTime(file).ToString() 'Get Date Modified For Each File
                        AddIcon(file)
                        lvFiles.Items.Add(file.Substring(file.LastIndexOf("\"c) + 1), file.ToString()) 'Add Files & File Properties To ListView
                        If My.Settings.KnownFormats.Contains(FileExtension.ToString) Then
                            Dim index As Integer = My.Settings.KnownFormats.IndexOf(FileExtension.ToString)
                            Dim desc As String = My.Settings.FormatDescriptions.Item(index).ToString
                            lvFiles.Items(SubItemIndex).SubItems.Add(desc)
                        Else
                            lvFiles.Items(SubItemIndex).SubItems.Add(FileExtension.ToString() & " File")
                        End If
                        lvFiles.Items(SubItemIndex).SubItems.Add(DateMod.ToString())

                        SubItemIndex += 1

                    Next

                Catch ex As Exception

                End Try

            End If
            expl = False

        Else
            lvFiles.ContextMenuStrip = Nothing
            Button20.Visible = False
            lvFiles.View = View.Tile
            lvFiles.Items.Clear()
            lvFiles.LargeImageList = ImageList1
            lvFiles.SmallImageList = ImageList1
            Dim allDrives As DriveInfo() = DriveInfo.GetDrives()
            Dim size = CType(ShellEx.IconSizeEnum.LargeIcon48, ShellEx.IconSizeEnum)
            Dim subitemindex As Integer = 0
            For Each d As DriveInfo In allDrives
                If d.IsReady = True Then
                    ImageList1.Images.Add(d.VolumeLabel, ShellEx.GetBitmapFromFolderPath(d.Name, size))
                    lvFiles.Items.Add(d.Name, ImageList1.Images.Count - 1)
                    lvFiles.Items(subitemindex).SubItems.Add(" ")
                    Dim percentFree As Double = (1 - (CDbl(d.TotalFreeSpace) / d.TotalSize))
                    lvFiles.Items(subitemindex).SubItems.Add(d.VolumeLabel.ToString & "  " & Math.Round(100 * percentFree) & "% Used")
                    Dim ab As ProgressBarEx = New ProgressBarEx With {
                        .Name = "Pbar " & subitemindex.ToString,
                        .RoundedCorners = False,
                        .Width = 150,
                        .Height = 12,
                        .Minimum = 0,
                        .Maximum = 100,
                        .Value = percentFree * 100,
                        .Location = New Point(lvFiles.Items(subitemindex).GetBounds(ItemBoundsPortion.Entire).X + 53, lvFiles.Items(subitemindex).GetBounds(ItemBoundsPortion.Entire).Bottom - 20),
                        .GradiantPosition = ProgressBarEx.GradiantArea.None,
                        .ProgressColor = Color.FromArgb(255, 38, 160, 218),
                        .BackgroundColor = Color.FromArgb(255, 230, 230, 230)
                        }
                    If percentFree * 100 > 80 Then
                        ab.ProgressColor = Color.DarkOrange
                    End If
                    Panel7.Controls.Add(ab)
                    ab.Visible = True
                    ab.BringToFront()
                    subitemindex += 1
                End If
            Next

            If My.Computer.FileSystem.DirectoryExists("C:\Users\" & GetUserName() & "\OneDrive\") Then
                ImageList1.Images.Add(Image.FromFile("C:\Users\Aditya\Downloads\simply_styled_icon_set___731_icons____free__by_dakirby309-d7rmhyo (1)\OneDrive.png"))

                lvFiles.Items.Add("OneDrive", ImageList1.Images.Count - 1)
                lvFiles.Items(subitemindex).SubItems.Add("Cloud Storage")
            End If
        End If

        lvFiles.Select()
        If lvFiles.Items.Count.ToString <> 0 Then
            Label2.Text = lvFiles.Items.Count.ToString & " Items"
        Else
            Label2.Text = "0 Items"
        End If
    End Sub


    Private Sub CloseButton_Click(sender As Object, e As EventArgs) Handles CloseButton.Click
        Close()
    End Sub

    Private Sub FPath_KeyUp(sender As Object, e As KeyEventArgs) Handles FPath.KeyUp
        If expl = False Then
            Try
                If e.KeyCode = Keys.Enter Then
                    If FPath.Text <> "" Then
                        GoSearch(FPath.Text)
                    End If
                End If
            Catch ex As Exception

            End Try
        End If

    End Sub

    Private Sub MinimizeButton_Click(sender As Object, e As EventArgs) Handles MinimizeButton.Click
        Me.WindowState = FormWindowState.Minimized
    End Sub

    Private Sub Button3_Click(sender As Object, e As EventArgs) Handles Button3.Click
        GoSearch(FPath.Text)
    End Sub

    Private Sub Button2_Click(sender As Object, e As EventArgs) Handles Button2.Click
        FPath.Text = ""
        GoSearch("")
    End Sub

    Private Sub Button1_Click(sender As Object, e As EventArgs) Handles Button1.Click
        If FPath.Text.Length <> 3 Then
            Dim a As String = FPath.Text
            a = a.Remove(a.Length - 1, 1)
            a = a.Remove(a.LastIndexOf("\"), a.Length - a.LastIndexOf("\")) + "\"
            GoSearch(a)
        Else
            FPath.Text = ""
            GoSearch("")
        End If
    End Sub

    Dim imagebitmap As Bitmap
    Dim graphicsvariable As Graphics

    Private Sub Button4_Click(sender As Object, e As EventArgs) Handles HeaderBtn.Click
        If Pane.Width = 217 Then
            Pane.Width = 38
            HeaderBtn.Text = "   Expand Pane"
        Else
            Pane.Width = 217
            HeaderBtn.Text = "   Collapse Pane"

        End If
    End Sub

    Private Sub BtnGeneral_Click(sender As Object, e As EventArgs) Handles BtnGeneral.Click
        FPath.Text = ""
        GoSearch("")
    End Sub
    Declare Function GetUserName Lib "advapi32.dll" Alias _
       "GetUserNameA" (ByVal lpBuffer As String,
       ByRef nSize As Integer) As Integer

    Public Function GetUserName() As String
        Dim iReturn As Integer
        Dim userName As String
        userName = New String(CChar(" "), 50)
        iReturn = GetUserName(userName, 50)
        GetUserName = userName.Substring(0, userName.IndexOf(Chr(0)))
    End Function

    Private Sub Button5_Click(sender As Object, e As EventArgs) Handles Button5.Click

        GoSearch("C:\Users\" & GetUserName() & "\Desktop\")
    End Sub

    Private Sub Button6_Click(sender As Object, e As EventArgs) Handles Button6.Click
        GoSearch("C:\Users\" & GetUserName() & "\Documents\")
    End Sub

    Private Sub Button8_Click(sender As Object, e As EventArgs) Handles Button8.Click
        GoSearch("C:\Users\" & GetUserName() & "\Pictures\")
    End Sub

    Private Sub Button7_Click(sender As Object, e As EventArgs) Handles Button7.Click
        GoSearch("C:\Users\" & GetUserName() & "\Videos\")
    End Sub

    Private Sub Button9_Click(sender As Object, e As EventArgs) Handles Button9.Click
        GoSearch("C:\Users\" & GetUserName() & "\Downloads\")
    End Sub

    Private Sub BtnReportBugs_Click(sender As Object, e As EventArgs) Handles BtnReportBugs.Click
        MsgBox("Mail me at adityavikram2016@gmail.com")
    End Sub

    Private Sub Panel6_Paint(sender As Object, e As PaintEventArgs) Handles Panel6.Paint

    End Sub

    Private Sub Panel1_Paint(sender As Object, e As PaintEventArgs) Handles Panel1.Paint

    End Sub

    Private Sub Button15_Click(sender As Object, e As EventArgs) Handles Button15.Click
        If FPath.Text <> "" Then
            If SearchBox.Visible = False Then
                SearchBox.Visible = True
                SearchBox.Focus()
                Button16.Text = ""
                Label1.Visible = True
                CutBtn.Visible = False
                CopyBtn.Visible = False
                PasteBtn.Visible = False
                Button18.Visible = False
                Button20.Visible = False
                Label1.SendToBack()
                Label1.Text = "   Search"
                PanelBufferTop.Height = 3
                PanelBufferTop.BackColor = SystemColors.Highlight
                Panel10.BackColor = SystemColors.Highlight
            Else
                If SearchBox.Text <> "" And SearchBox.Text <> "Type Here" Then
                    Button16.Text = ""
                    For Each item As ListViewItem In lvFiles.Items
                        If item.Text.Contains(SearchBox.Text) = False Then
                            item.Remove()
                        End If
                        If lvFiles.Items.Count = 0 Then
                            MessageBox.Show("No Results were found.", "Explorer", MessageBoxButtons.OK, MessageBoxIcon.Exclamation)
                        End If
                    Next
                    Label1.Text = "   Search Results for " & My.Resources.dblinv & SearchBox.Text & My.Resources.dblinv
                Else
                    CutBtn.Visible = True
                    CopyBtn.Visible = True
                    PasteBtn.Visible = True
                    Button20.Visible = True
                    Button18.Visible = True
                    Label1.Visible = False
                    Label1.BringToFront()
                    Button16.Text = ""
                    SearchBox.Visible = False
                    GoSearch(FPath.Text)
                    PanelBufferTop.BackColor = Color.Transparent
                    Panel10.BackColor = Color.Transparent
                    PanelBufferTop.Height = 15
                End If
            End If
        End If
    End Sub

    Private Sub Button14_Click(sender As Object, e As EventArgs) Handles Button14.Click
        If SortPanel.Visible = False Then
            SortPanel.Location = New Point((Button14.Location.X - SortPanel.Width) + 31, 0)
            SortPanel.Visible = True
        Else
            SortPanel.Visible = False
        End If

    End Sub



    Private Sub Button12_Click(sender As Object, e As EventArgs) Handles CopyBtn.Click
        copyfilename = FPath.Text + lvFiles.FocusedItem.Text.ToString
        copyfile = lvFiles.FocusedItem.Text.ToString
    End Sub

    Private Sub Button11_Click(sender As Object, e As EventArgs) Handles CutBtn.Click
        copyfilename = FPath.Text + lvFiles.FocusedItem.Text.ToString
        copyfile = lvFiles.FocusedItem.Text.ToString
        cut = True
    End Sub

    Private Sub Button13_Click(sender As Object, e As EventArgs) Handles PasteBtn.Click
        If cut = False Then
            Try
                My.Computer.FileSystem.MoveFile(copyfilename, FPath.Text + copyfile, FileIO.UIOption.AllDialogs)
            Catch
                My.Computer.FileSystem.MoveFile(copyfilename, FPath.Text + "Copy - " & copyfile, FileIO.UIOption.AllDialogs)
            End Try
        Else
            My.Computer.FileSystem.MoveFile(copyfilename, FPath.Text + copyfile, FileIO.UIOption.AllDialogs)
            My.Computer.FileSystem.DeleteFile(copyfilename)
            cut = False
        End If
        GoSearch(FPath.Text)
    End Sub

    Private Sub Button16_Click(sender As Object, e As EventArgs) Handles Button16.Click
        If SearchBox.Visible = False Then
            Dim path As String = FPath.Text + lvFiles.FocusedItem.Text.ToString
            Try
                My.Computer.FileSystem.DeleteFile(path, FileIO.UIOption.AllDialogs, FileIO.RecycleOption.SendToRecycleBin)
                GoSearch(FPath.Text)
            Catch
                Try
                    My.Computer.FileSystem.DeleteDirectory(path, FileIO.UIOption.AllDialogs, FileIO.RecycleOption.SendToRecycleBin)
                    GoSearch(FPath.Text)
                Catch ex As Exception

                End Try
            End Try
        Else
            PanelBufferTop.BackColor = Color.Transparent
            Panel10.BackColor = Color.Transparent
            PanelBufferTop.Height = 15
            Label1.Text = "   Search"
            CutBtn.Visible = True
            CopyBtn.Visible = True
            PasteBtn.Visible = True
            Button18.Visible = True
            Label1.Visible = False
            Label1.BringToFront()
            GoSearch(FPath.Text)
            SearchBox.Visible = False
            Button16.Text = ""
        End If
    End Sub

    Private Sub Button17_Click(sender As Object, e As EventArgs) Handles Button17.Click
        GoSearch("C:\Users\" & GetUserName() & "\OneDrive\")
    End Sub

    Private Sub MaximizeButton_Click(sender As Object, e As EventArgs) Handles MaximizeButton.Click
        If WindowState <> FormWindowState.Maximized Then
            WindowState = FormWindowState.Maximized
            MaximizeButton.Text = ""
        Else
            WindowState = FormWindowState.Normal
            MaximizeButton.Text = ""
        End If
    End Sub

    Private Sub Button18_Click(sender As Object, e As EventArgs) Handles UserBtn.Click

        If tr = False Then
            Userinfo.Show()
            Userinfo.Location = New Point(Location.X + 7 + UserBtn.Location.X - UserBtn.Width - 14, Location.Y + 7 + Titlebar.Height - Header.Height - 2)
            tr = True
        Else
            Userinfo.Hide()
            tr = False
        End If
    End Sub

    Private Sub PictureBox1_Click(sender As Object, e As EventArgs) Handles PictureBox1.Click

    End Sub

    Private Sub UserBtn_MouseHover(sender As Object, e As EventArgs) Handles UserBtn.MouseHover
        PictureBox1.BackColor = Color.FromArgb(255, 64, 64, 64)

    End Sub

    Private Sub UserBtn_MouseLeave(sender As Object, e As EventArgs) Handles UserBtn.MouseLeave
        PictureBox1.BackColor = Color.Transparent
    End Sub

    Private Sub PictureBox1_MouseHover(sender As Object, e As EventArgs) Handles PictureBox1.MouseHover
        PictureBox1.BackColor = Color.FromArgb(255, 64, 64, 64)
        UserBtn.BackColor = Color.FromArgb(255, 64, 64, 64)
    End Sub

    Private Sub PictureBox1_MouseLeave(sender As Object, e As EventArgs) Handles PictureBox1.MouseLeave
        PictureBox1.BackColor = Color.Transparent
        UserBtn.BackColor = Color.Transparent
    End Sub

    Private Sub PictureBox1_MouseEnter(sender As Object, e As EventArgs) Handles PictureBox1.MouseEnter
        PictureBox1.BackColor = Color.FromArgb(255, 64, 64, 64)
        UserBtn.BackColor = Color.FromArgb(255, 64, 64, 64)
    End Sub

    Private Sub UserBtn_MouseEnter(sender As Object, e As EventArgs) Handles UserBtn.MouseEnter
        PictureBox1.BackColor = Color.FromArgb(255, 64, 64, 64)
    End Sub

    Private Sub Button19_Click(sender As Object, e As EventArgs) Handles Button19.Click
        GoSearch("C:\Users\" & GetUserName() & "\Music\")
    End Sub

    Private Sub Button18_Click_1(sender As Object, e As EventArgs) Handles Button18.Click
        RenameToolStripMenuItem.PerformClick()
        GoSearch(FPath.Text)
    End Sub

    Private Sub Button4_Click_1(sender As Object, e As EventArgs) Handles Button4.Click
        Dim a As New Form1
        a.Show()
        a.BringToFront()
    End Sub



    Private Sub Button4_MouseEnter(sender As Object, e As EventArgs) Handles Button4.MouseEnter
        Button4.Text = " Open New Window"
    End Sub

    Private Sub Button4_MouseLeave(sender As Object, e As EventArgs) Handles Button4.MouseLeave
        Button4.Text = ""
    End Sub

    Private Sub HeaderBtn_MouseEnter(sender As Object, e As EventArgs) Handles HeaderBtn.MouseEnter
        If Pane.Width = 217 Then
            HeaderBtn.Text = "   Collapse Pane"
        Else
            HeaderBtn.Text = "   Expand Pane"
        End If
    End Sub

    Private Sub HeaderBtn_MouseLeave(sender As Object, e As EventArgs) Handles HeaderBtn.MouseLeave
        HeaderBtn.Text = "   File Explorer"
    End Sub

    Private Sub lvFiles_ItemMouseHover(sender As Object, e As ListViewItemMouseHoverEventArgs) Handles lvFiles.ItemMouseHover

    End Sub

    Private Sub lvFiles_Click(sender As Object, e As EventArgs) Handles lvFiles.Click

    End Sub

    Private Sub Button11_Click_1(sender As Object, e As EventArgs) Handles Button11.Click
        lvFiles.Sorting = SortOrder.Descending
        lvFiles.Sort()
        SortPanel.Visible = False
    End Sub

    Private Sub Button12_Click_1(sender As Object, e As EventArgs) Handles Button12.Click
        lvFiles.Sorting = SortOrder.Ascending
        lvFiles.Sort()
        SortPanel.Visible = False
    End Sub

    Private Sub Button13_Click_1(sender As Object, e As EventArgs) Handles Button13.Click
        lvFiles.Sorting = SortOrder.None
        lvFiles.Sort()
        SortPanel.Visible = False
        GoSearch(FPath.Text)
    End Sub

    Private Sub CreateShortcutToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles CreateShortcutToolStripMenuItem.Click

    End Sub

    Private Sub FolderToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles FolderToolStripMenuItem.Click '
        Dim foldername As String = "New Folder " + lvFiles.Items.Count.ToString
        renfile = FPath.Text + foldername
        Dim index As Integer = 0
        Try
            My.Computer.FileSystem.CreateDirectory(FPath.Text + foldername + "\")
        Catch ex As Exception
            MsgBox(ex)
        End Try
        GoSearch(FPath.Text)
        Dim pt As Point = New Point(0, 0)

        lvFiles.FindItemWithText(foldername).Selected = True
        pt = New Point(lvFiles.SelectedItems.Item(index).GetBounds(ItemBoundsPortion.Label).X, lvFiles.SelectedItems.Item(index).GetBounds(ItemBoundsPortion.Label).Y + 23)
        TextBox1.Location = pt
        TextBox1.Text = foldername.Remove(foldername.Length - 2)
        TextBox1.Visible = True
        TextBox1.Select()
        TextBox1.SelectAll()
    End Sub


    Private Sub TextBox1_KeyUp(sender As Object, e As KeyEventArgs) Handles TextBox1.KeyUp
        If e.KeyCode = Keys.Enter Then
            renamefile()
        Else

        End If
    End Sub

    Private Sub renamefile()
        Try
            If lvFiles.FocusedItem.Text.Contains(".") = True Then
                Try
                    My.Computer.FileSystem.RenameFile(FPath.Text + lvFiles.FocusedItem.Text, TextBox1.Text)
                Catch
                End Try
            ElseIf lvFiles.FocusedItem.Text.Contains(".") = False Then
                Try
                    My.Computer.FileSystem.RenameDirectory(FPath.Text + lvFiles.FocusedItem.Text + "\", TextBox1.Text)
                Catch
                End Try
            End If
        Catch ex As Exception

        End Try
        GoSearch(FPath.Text)
        TextBox1.Visible = False
    End Sub
    Private Sub TextBox1_LostFocus(sender As Object, e As EventArgs) Handles TextBox1.LostFocus
        If TextBox1.Visible = True Then
            renamefile()
            TextBox1.Visible = False
        End If
    End Sub

    Private Sub RenameToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles RenameToolStripMenuItem.Click
        Dim pt As Point
        Dim foldername = lvFiles.FocusedItem.Text.ToString
        If lvFiles.FocusedItem.Text <> "" Then
            If lvFiles.FocusedItem.Text.Contains(".") = False Then
                pt = New Point(lvFiles.FocusedItem.GetBounds(ItemBoundsPortion.Label).X, lvFiles.FocusedItem.GetBounds(ItemBoundsPortion.Label).Y + 24)
            Else
                pt = New Point(lvFiles.FocusedItem.GetBounds(ItemBoundsPortion.Label).X, lvFiles.FocusedItem.GetBounds(ItemBoundsPortion.Label).Y + 18)
            End If
            TextBox1.Location = pt
            TextBox1.Text = foldername
            TextBox1.Visible = True
            TextBox1.BringToFront()
            TextBox1.Select()
            TextBox1.Select(0, TextBox1.Text.LastIndexOf("."))
        End If
    End Sub

    Private Sub LvFiles_SelectedIndexChanged(sender As Object, e As EventArgs) Handles lvFiles.SelectedIndexChanged

    End Sub

    Private Sub lvFiles_MouseDown(sender As Object, e As MouseEventArgs) Handles lvFiles.MouseDown
        If e.Button = MouseButtons.Left Then
            Try
                If lvFiles.FocusedItem.Text.Contains(".png") Or lvFiles.FocusedItem.Text.Contains(".bmp") Or lvFiles.FocusedItem.Text.Contains(".jpg") Or lvFiles.FocusedItem.Text.Contains(".gif") Then
                    If TextBox1.Visible = False Then
                        ImgVw.Show()
                        ImgVw.Location = New Point(MousePosition.X + 10, MousePosition.Y + 10)
                    End If
                End If
            Catch
            End Try
            'ElseIf e.Button = MouseButtons.Right And FPath.Text = "" And lvFiles.FocusedItem.Text <> "" Then
            '       Dim drivetext As String = lvFiles.FocusedItem.Text
            '    Dim allDrives As DriveInfo() = DriveInfo.GetDrives()
            '   Dim size = CType(ShellEx.IconSizeEnum.LargeIcon48, ShellEx.IconSizeEnum)
            '    Dim d As New DriveInfo(lvFiles.FocusedItem.Text)
            '    If d.VolumeLabel = lvFiles.FocusedItem.Text.ToString Then
            'DrvInf.Show()
            'DrvInf.DiskLabel.Text = d.Name
            'DrvInf.Label2.Text = "Type:           " & d.DriveType.ToString
            ''DrvInf.Label3.Text = "File system:    " & d.DriveFormat.ToString
            'DrvInf.Label4.Text = "Used Space:     " & (d.TotalSize - d.TotalFreeSpace).ToString & " bytes"
            'DrvInf.Label5.Text = "Free Space:     " & d.TotalFreeSpace.ToString & " bytes"
            'DrvInf.Label6.Text = "Capacity:       " & d.TotalSize.ToString & " bytes"
            'DrvInf.Label7.Text = "Drive " & d.VolumeLabel.ToString
            'DrvInf.PictureBox1.Image = ImageList1.Images.Item(lvFiles.FocusedItem.Index)
            'Dim percentFree As Double = (1 - (CDbl(d.TotalFreeSpace) / d.TotalSize))
            'My.Settings.FocusPercentage = 100 * Round(percentFree)
            ' End If
        End If
    End Sub

    Private Sub lvFiles_MouseHover(sender As Object, e As EventArgs) Handles lvFiles.MouseHover

    End Sub

    Private Sub lvFiles_MouseUp(sender As Object, e As MouseEventArgs) Handles lvFiles.MouseUp
    End Sub

    Private Sub lvFiles_LocationChanged(sender As Object, e As EventArgs) Handles lvFiles.LocationChanged

    End Sub

    Private Sub Button20_Click(sender As Object, e As EventArgs) Handles Button20.Click
        If My.Settings.View = 1 Then
            My.Settings.View = 4
            iIconList.ImageSize = New Size(48, 48)
        ElseIf My.Settings.View = 4 Then
            My.Settings.View = 0
            iIconList.ImageSize = New Size(60, 60)
        ElseIf My.Settings.View = 0 Then
            My.Settings.View = 1
            iIconList.ImageSize = New Size(20, 20)
        End If
        GoSearch(FPath.Text)
    End Sub
    Private Sub MakeNewFile(fileformat As String, filename As String, filepath As String)
        Dim ab As New RichTextBox
        ab.Text = ""
        ab.SaveFile(filepath, filename + filepath)
        ab.Dispose()
    End Sub
End Class