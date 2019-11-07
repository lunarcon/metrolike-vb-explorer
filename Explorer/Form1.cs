private void lvFiles_MouseDoubleClick(object sender, MouseEventArgs e)
{
    ListView lstvw = (ListView)sender;
    if (lvFiles.FocusedItem.Text == "OneDrive" & FPath.Text == "")
        GoSearch(@"C:\Users\" + GetUserName() + @"\OneDrive\");
    else
        try
        {
            if (lstvw.FocusedItem.Text.Contains("."))
                Process.Start(FPath.Text + lstvw.FocusedItem.Text);
            else
                GoSearch(FPath.Text + lstvw.FocusedItem.Text);
        }
        catch (Exception ex)
        {
            GoSearch(FPath.Text + lstvw.FocusedItem.Text);
        }
}
