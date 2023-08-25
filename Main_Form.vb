﻿Imports System.Windows.Forms.VisualStyles.VisualStyleElement
Imports System.IO
Imports System.Threading
Public Class Main_Form
    Public mydata_Dataset As New DataSet
    Public Sub initialize_data()
        Dim Column_Select As New DataGridViewCheckBoxColumn
        Column_Select.HeaderText = "Select"
        DataGridView1.Columns.Insert(0, Column_Select)
        DataGridView1.AllowUserToAddRows = False

        Dim ref_table As New System.Data.DataTable
        ref_table.TableName = "Refs Table"
        Dim Column_ID As New System.Data.DataColumn("ID", System.Type.GetType("System.Int32"))
        Dim Column_Name As New System.Data.DataColumn("Name")
        Dim Column_Filter As New System.Data.DataColumn("Count")
        Dim Column_RF As New System.Data.DataColumn("Length")
        Dim Column_Reads As New System.Data.DataColumn("Depth")
        Dim Column_Assemble As New System.Data.DataColumn("Assemble")
        Dim Column_Align As New System.Data.DataColumn("Ass. Len.")
        Dim Column_State As New System.Data.DataColumn("State")
        ref_table.Columns.Add(Column_ID)
        ref_table.Columns.Add(Column_Name)
        ref_table.Columns.Add(Column_Filter)
        ref_table.Columns.Add(Column_RF)
        ref_table.Columns.Add(Column_Reads)
        ref_table.Columns.Add(Column_Assemble)
        ref_table.Columns.Add(Column_Align)
        ref_table.Columns.Add(Column_State)
        mydata_Dataset.Tables.Add(ref_table)

        refsView = mydata_Dataset.Tables("Refs Table").DefaultView
        refsView.AllowNew = False
        refsView.AllowDelete = False
        refsView.AllowEdit = False

        Dim Column_Select1 As New DataGridViewCheckBoxColumn
        Column_Select1.HeaderText = "Select"
        DataGridView2.Columns.Insert(0, Column_Select1)
        DataGridView2.AllowUserToAddRows = False

        Dim data_table As New System.Data.DataTable
        data_table.TableName = "Data Table"
        Dim Column_ID1 As New System.Data.DataColumn("ID", System.Type.GetType("System.Int32"))
        Dim Column_1 As New System.Data.DataColumn("Data 1")
        Dim Column_2 As New System.Data.DataColumn("Data 2")
        data_table.Columns.Add(Column_ID1)
        data_table.Columns.Add(Column_1)
        data_table.Columns.Add(Column_2)

        mydata_Dataset.Tables.Add(data_table)

        seqsView = mydata_Dataset.Tables("Data Table").DefaultView
        seqsView.AllowNew = False
        seqsView.AllowDelete = False
        seqsView.AllowEdit = False
    End Sub
    Public Sub do_filter(ByVal refresh As Boolean)

        Dim SI_filter As New ProcessStartInfo()
        Dim filePath As String = out_dir + "\ref_reads_count_dict.txt"
        If refresh Then
            If File.Exists(filePath) Then
                ref_filter_result(filePath)
            Else
                MsgBox("Run failed, you should do filter first!")
                Exit Sub
            End If
            SI_filter.FileName = currentDirectory + "analysis\win_refilter.exe" ' 替换为实际的命令行程序路径
            SI_filter.WorkingDirectory = currentDirectory + "temp\" ' 替换为实际的运行文件夹路径
            'SI_filter.CreateNoWindow = True
            SI_filter.Arguments = "-r " + """" + ref_dir + """"
            SI_filter.Arguments += " -q1" + q1 + " -q2" + q2
            SI_filter.Arguments += " -o " + """" + out_dir + """"
            SI_filter.Arguments += " -kf " + k1
            SI_filter.Arguments += " -s " + NumericUpDown2.Value.ToString
            SI_filter.Arguments += " -gr " + CheckBox2.Checked.ToString
            SI_filter.Arguments += " -lkd kmer_dict_k" + NumericUpDown1.Value.ToString + ".dict"
            SI_filter.Arguments += " -rl " + reads_length.ToString
            SI_filter.Arguments += " -max_depth " + NumericUpDown4.Value.ToString
            SI_filter.Arguments += " -max_size " + NumericUpDown9.Value.ToString
        Else
            SI_filter.FileName = currentDirectory + "analysis\win_filter.exe" ' 替换为实际的命令行程序路径
            SI_filter.WorkingDirectory = currentDirectory + "temp\" ' 替换为实际的运行文件夹路径
            'SI_filter.CreateNoWindow = True
            SI_filter.Arguments = "-r " + """" + ref_dir + """"
            SI_filter.Arguments += " -q1" + q1 + " -q2" + q2
            SI_filter.Arguments += " -o " + """" + out_dir + """"
            SI_filter.Arguments += " -kf " + NumericUpDown1.Value.ToString
            SI_filter.Arguments += " -s " + NumericUpDown2.Value.ToString
            SI_filter.Arguments += " -gr " + CheckBox2.Checked.ToString
            SI_filter.Arguments += " -lkd kmer_dict_k" + NumericUpDown1.Value.ToString + ".dict"
            If CheckBox3.Checked Then
                SI_filter.Arguments += " -m_reads " + NumericUpDown3.Value.ToString
            End If
        End If

        Dim process_filter As Process = Process.Start(SI_filter)
        process_filter.WaitForExit()
        process_filter.Close()


        If File.Exists(filePath) Then
            ref_filter_result(filePath)
        Else
            MsgBox("Run failed, please check the logs!")
        End If


    End Sub
    Public Sub ref_filter_result(ByVal filePath As String)
        Dim count_dict As New Dictionary(Of String, Integer)

        ' 读取文件内容并将内容存入字典
        Using sr As New StreamReader(filePath)
            While Not sr.EndOfStream
                Dim line As String = sr.ReadLine()
                Dim parts As String() = line.Split(","c)

                If parts.Length >= 2 Then
                    Dim key As String = parts(0)
                    Dim value As Integer

                    If Integer.TryParse(parts(1), value) Then
                        If count_dict.ContainsKey(key) Then
                            count_dict(key) = value
                        Else
                            count_dict.Add(key, value)
                        End If

                    End If
                End If
            End While
        End Using
        For i As Integer = 1 To refsView.Count
            If DataGridView1.Rows(i - 1).Cells(0).FormattedValue.ToString = "True" Then
                If count_dict.ContainsKey(DataGridView1.Rows(i - 1).Cells(2).Value.ToString) Then
                    If reads_length = 0 Then
                        Dim sr As New StreamReader(out_dir + "\filtered\" + DataGridView1.Rows(i - 1).Cells(2).Value.ToString + data_type)
                        sr.ReadLine()
                        reads_length = sr.ReadLine().Length
                        sr.Close()
                    End If
                    DataGridView1.Rows(i - 1).Cells(5).Value = CInt(count_dict(DataGridView1.Rows(i - 1).Cells(2).Value.ToString) / CInt(DataGridView1.Rows(i - 1).Cells(4).Value) * reads_length)
                Else
                    DataGridView1.Rows(i - 1).Cells(5).Value = 0
                End If
            End If
        Next
    End Sub
    Public Sub do_assemble()

        Dim SI_assembler As New ProcessStartInfo()
        SI_assembler.FileName = currentDirectory + "analysis\win_assembler.exe" ' 替换为实际的命令行程序路径
        SI_assembler.WorkingDirectory = currentDirectory + "temp\" ' 替换为实际的运行文件夹路径
        'SI_assembler.CreateNoWindow = True
        SI_assembler.Arguments = "-r " + """" + ref_dir + """"
        SI_assembler.Arguments += " -q1" + q1 + " -q2" + q2
        SI_assembler.Arguments += " -o " + """" + out_dir + """"
        SI_assembler.Arguments += " -kf " + k1
        SI_assembler.Arguments += " -s " + NumericUpDown2.Value.ToString
        SI_assembler.Arguments += " -gr " + CheckBox2.Checked.ToString
        SI_assembler.Arguments += " -lkd kmer_dict_k" + NumericUpDown1.Value.ToString + ".dict"
        SI_assembler.Arguments += " -gr " + CheckBox2.Checked.ToString
        If CheckBox1.Checked Then
            SI_assembler.Arguments += " -ka 0"

        Else
            SI_assembler.Arguments += " -ka " + k2
        End If
        SI_assembler.Arguments += " -k_min " + NumericUpDown6.Value.ToString
        SI_assembler.Arguments += " -k_max " + NumericUpDown7.Value.ToString
        SI_assembler.Arguments += " -limit_count " + NumericUpDown8.Value.ToString
        Dim process_filter As Process = Process.Start(SI_assembler)
        process_filter.WaitForExit()
        process_filter.Close()

        Dim filePath As String = out_dir + "\result_dict.txt"
        If File.Exists(filePath) Then
            ref_assemble_result(filePath)
        Else
            MsgBox("Run failed, please check the logs!")
        End If


    End Sub

    Public Sub ref_assemble_result(ByVal filePath As String)
        Dim result_dict As New Dictionary(Of String, String)

        ' 读取文件内容并将内容存入字典
        Using sr As New StreamReader(filePath)
            While Not sr.EndOfStream
                Dim line As String = sr.ReadLine()
                Dim parts As String() = line.Split(","c)

                If parts.Length >= 2 Then
                    Dim key As String = parts(0)
                    If result_dict.ContainsKey(key) Then
                        result_dict(key) = parts(1)
                    Else
                        result_dict.Add(key, parts(1))
                    End If
                End If
            End While
        End Using
        For i As Integer = 1 To refsView.Count
            If DataGridView1.Rows(i - 1).Cells(0).FormattedValue.ToString = "True" Then
                If result_dict.ContainsKey(DataGridView1.Rows(i - 1).Cells(2).Value.ToString) Then
                    DataGridView1.Rows(i - 1).Cells(6).Value = result_dict(DataGridView1.Rows(i - 1).Cells(2).Value.ToString)
                    If DataGridView1.Rows(i - 1).Cells(6).Value <> "success" Then
                        DataGridView1.Rows(i - 1).Cells(6).Value = "failed"
                        DataGridView1.Rows(i - 1).Cells(7).Value = 0
                        DataGridView1.Rows(i - 1).Cells(8).Value = "failed"
                    End If
                    If File.Exists(out_dir + "\contigs_best\" + DataGridView1.Rows(i - 1).Cells(2).Value.ToString + ".fasta") Then
                        Try
                            Dim sr As New StreamReader(out_dir + "\contigs_best\" + DataGridView1.Rows(i - 1).Cells(2).Value.ToString + ".fasta")
                            sr.ReadLine()
                            DataGridView1.Rows(i - 1).Cells(7).Value = sr.ReadLine().Length
                            sr.Close()
                            If DataGridView1.Rows(i - 1).Cells(7).Value / DataGridView1.Rows(i - 1).Cells(4).Value > 0.75 And DataGridView1.Rows(i - 1).Cells(7).Value / DataGridView1.Rows(i - 1).Cells(4).Value < 1.5 Then
                                DataGridView1.Rows(i - 1).Cells(8).Value = "passed"
                            ElseIf DataGridView1.Rows(i - 1).Cells(7).Value / DataGridView1.Rows(i - 1).Cells(4).Value < 0.75 Then
                                DataGridView1.Rows(i - 1).Cells(8).Value = "short"
                            Else
                                DataGridView1.Rows(i - 1).Cells(8).Value = "long"
                            End If
                        Catch ex As Exception
                            File.Delete(out_dir + "\contigs_best\" + DataGridView1.Rows(i - 1).Cells(2).Value.ToString + ".fasta")
                            DataGridView1.Rows(i - 1).Cells(6).Value = "failed"
                            DataGridView1.Rows(i - 1).Cells(7).Value = 0
                            DataGridView1.Rows(i - 1).Cells(8).Value = "failed"
                        End Try

                    End If
                End If
            End If
        Next
    End Sub
    Private Sub Main_Form_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        CheckForIllegalCrossThreadCalls = False
        currentDirectory = Application.StartupPath
        initialize_data()
        TextBox1.Text = currentDirectory + "results"
    End Sub

    Private Sub Main_Form_FormClosing(sender As Object, e As FormClosingEventArgs) Handles MyBase.FormClosing
        End
    End Sub



    Private Sub Timer1_Tick(sender As Object, e As EventArgs) Handles Timer1.Tick
        Select Case timer_id
            Case 0
            Case 1

            Case 2
                Timer1.Enabled = False
                DataGridView1.DataSource = refsView
                refsView.AllowNew = False
                refsView.AllowEdit = True

                DataGridView1.Columns(1).ReadOnly = True
                DataGridView1.Columns(2).ReadOnly = True
                DataGridView1.Columns(3).ReadOnly = True
                DataGridView1.Columns(4).ReadOnly = True
                DataGridView1.Columns(5).ReadOnly = True
                DataGridView1.Columns(6).ReadOnly = True
                DataGridView1.Columns(7).ReadOnly = True
                DataGridView1.Columns(8).ReadOnly = True

                DataGridView1.Columns(0).SortMode = DataGridViewColumnSortMode.NotSortable
                DataGridView1.Columns(1).SortMode = DataGridViewColumnSortMode.NotSortable
                DataGridView1.Columns(2).SortMode = DataGridViewColumnSortMode.NotSortable
                DataGridView1.Columns(3).SortMode = DataGridViewColumnSortMode.NotSortable
                DataGridView1.Columns(4).SortMode = DataGridViewColumnSortMode.NotSortable
                DataGridView1.Columns(5).SortMode = DataGridViewColumnSortMode.NotSortable
                DataGridView1.Columns(6).SortMode = DataGridViewColumnSortMode.NotSortable
                DataGridView1.Columns(7).SortMode = DataGridViewColumnSortMode.NotSortable
                DataGridView1.Columns(8).SortMode = DataGridViewColumnSortMode.NotSortable
                DataGridView1.Columns(0).Width = 50
                DataGridView1.Columns(1).Width = 80
                DataGridView1.Columns(2).Width = 80
                DataGridView1.Columns(3).Width = 80
                DataGridView1.Columns(4).Width = 80
                DataGridView1.Columns(5).Width = 80
                DataGridView1.Columns(6).Width = 160
                DataGridView1.Columns(7).Width = 80
                DataGridView1.Columns(8).Width = 80
                Timer1.Enabled = True
                DataGridView1.RefreshEdit()
                GC.Collect()
                timer_id = 0
                data_loaded = True
            Case 3
                Timer1.Enabled = False
                DataGridView2.DataSource = seqsView
                refsView.AllowNew = False
                refsView.AllowEdit = True

                DataGridView2.Columns(1).ReadOnly = True
                DataGridView2.Columns(2).ReadOnly = True
                DataGridView2.Columns(3).ReadOnly = True

                DataGridView2.Columns(0).SortMode = DataGridViewColumnSortMode.NotSortable
                DataGridView2.Columns(1).SortMode = DataGridViewColumnSortMode.NotSortable
                DataGridView2.Columns(2).SortMode = DataGridViewColumnSortMode.NotSortable
                DataGridView2.Columns(3).SortMode = DataGridViewColumnSortMode.NotSortable
                DataGridView2.Columns(0).Width = 50
                DataGridView2.Columns(1).Width = 80
                DataGridView2.Columns(2).Width = 400
                DataGridView2.Columns(3).Width = 400
                Timer1.Enabled = True
                DataGridView2.RefreshEdit()
                GC.Collect()
                timer_id = 0
                data_loaded = True
        End Select
    End Sub

    Private Sub 测序文件ToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles 测序文件ToolStripMenuItem.Click
        Dim opendialog As New OpenFileDialog
        opendialog.Filter = "FastaQ File(*.fq;*.fq.gz)|*.fq;*.fastaq;*.FQ;*.fq.gz;*.gz"
        opendialog.FileName = ""
        opendialog.Multiselect = True
        opendialog.DefaultExt = ".fas"
        opendialog.CheckFileExists = True
        opendialog.CheckPathExists = True
        Dim resultdialog As DialogResult = opendialog.ShowDialog()
        If resultdialog = DialogResult.OK Then
            If opendialog.FileName.ToLower.EndsWith(".gz") Or opendialog.FileName.ToLower.EndsWith(".fq") Then
                data_type = ".fq"
            Else
                data_type = ".fasta"
            End If
            If opendialog.FileNames.Length = 2 Then
                'mydata_Dataset.Tables("Data Table").Clear()
                data_loaded = False
                Dim newrow(2) As String
                seqsView.AllowNew = True
                seqsView.AddNew()
                newrow(0) = seqsView.Count
                newrow(1) = opendialog.FileNames(0)
                newrow(2) = opendialog.FileNames(1)
                seqsView.Item(seqsView.Count - 1).Row.ItemArray = newrow
                seqsView.AllowNew = False
                seqsView.AllowEdit = True
                timer_id = 3
            ElseIf opendialog.FileNames.Length = 1 Then
                'mydata_Dataset.Tables("Data Table").Clear()
                data_loaded = False
                Dim newrow(2) As String
                seqsView.AllowNew = True
                seqsView.AddNew()
                newrow(0) = seqsView.Count
                newrow(1) = opendialog.FileNames(0)
                newrow(2) = ""
                seqsView.Item(seqsView.Count - 1).Row.ItemArray = newrow
                seqsView.AllowNew = False
                seqsView.AllowEdit = True
                timer_id = 3
            ElseIf opendialog.FileNames.Length Mod 2 = 0 Then
                data_loaded = False
                Dim sortedFileNames As String() = opendialog.FileNames.OrderBy(Function(path) path).ToArray()
                For i As Integer = 1 To opendialog.FileNames.Length / 2
                    Dim newrow(2) As String
                    seqsView.AllowNew = True
                    seqsView.AddNew()
                    newrow(0) = seqsView.Count
                    newrow(1) = sortedFileNames((i - 1) * 2)
                    newrow(2) = sortedFileNames((i - 1) * 2 + 1)
                    seqsView.Item(seqsView.Count - 1).Row.ItemArray = newrow
                Next
                seqsView.AllowNew = False
                seqsView.AllowEdit = True
                timer_id = 3
            Else
                MsgBox("Only a single file or pairs of sequencing files can be selected.")
            End If
        End If
    End Sub

    Private Sub DataGridView1_CellContentClick(sender As Object, e As DataGridViewCellEventArgs) Handles DataGridView1.CellContentClick

    End Sub

    Private Sub DataGridView1_DataBindingComplete(sender As Object, e As DataGridViewBindingCompleteEventArgs) Handles DataGridView1.DataBindingComplete
        If data_loaded = False And refsView.Count > 0 Then
            For i As Integer = 1 To refsView.Count
                DataGridView1.Rows(i - 1).Cells(0).Value = True
            Next
        End If
    End Sub

    Private Sub DataGridView2_CellContentClick(sender As Object, e As DataGridViewCellEventArgs) Handles DataGridView2.CellContentClick

    End Sub

    Private Sub DataGridView2_DataBindingComplete(sender As Object, e As DataGridViewBindingCompleteEventArgs) Handles DataGridView2.DataBindingComplete
        If data_loaded = False And seqsView.Count > 0 Then
            For i As Integer = 1 To seqsView.Count
                DataGridView2.Rows(i - 1).Cells(0).Value = True
            Next
        End If
    End Sub

    Private Sub Button1_Click(sender As Object, e As EventArgs) Handles Button1.Click
        Dim opendialog As New FolderBrowserDialog
        Dim resultdialog As DialogResult = opendialog.ShowDialog()
        If resultdialog = DialogResult.OK Then
            TextBox1.Text = opendialog.SelectedPath
        End If
    End Sub

    Private Sub CheckBox1_CheckedChanged(sender As Object, e As EventArgs) Handles CheckBox1.CheckedChanged
        NumericUpDown6.Enabled = CheckBox1.Checked Xor False
        NumericUpDown7.Enabled = CheckBox1.Checked Xor False
        NumericUpDown5.Enabled = CheckBox1.Checked Xor True
    End Sub


    Private Sub 拼接ToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles 拼接ToolStripMenuItem.Click
        If TextBox1.Text <> "" Then
            DataGridView1.EndEdit()
            Dim refs_count As Integer = 0
            Dim seqs_count As Integer = 0
            Dim has_assemble As Boolean = False
            reads_length = 0
            ref_dir = (currentDirectory + "temp\temp_refs\").Replace("\", "/")
            out_dir = TextBox1.Text.Replace("\", "/")
            q1 = ""
            q2 = ""
            k1 = NumericUpDown1.Value.ToString
            k2 = NumericUpDown5.Value.ToString
            DeleteDir(ref_dir)
            My.Computer.FileSystem.CreateDirectory(ref_dir)


            For i As Integer = 1 To refsView.Count
                If DataGridView1.Rows(i - 1).Cells(0).FormattedValue.ToString = "True" Then
                    refs_count += 1
                    File.Copy(currentDirectory + "temp\org_seq\" + DataGridView1.Rows(i - 1).Cells(2).Value.ToString + ".fasta", ref_dir + DataGridView1.Rows(i - 1).Cells(2).Value.ToString + ".fasta")
                    If File.Exists(out_dir + "\contigs_best\" + DataGridView1.Rows(i - 1).Cells(2).Value.ToString + ".fasta") Then
                        has_assemble = True
                    End If
                End If
            Next
            If has_assemble Then
                Dim result As DialogResult = MessageBox.Show("Reassemble the successfully processed entries?", "Confirm Operation", MessageBoxButtons.YesNo, MessageBoxIcon.Question)
                ' 根据用户的选择执行相应的操作
                If result = DialogResult.Yes Then
                    For i As Integer = 1 To refsView.Count
                        If DataGridView1.Rows(i - 1).Cells(0).FormattedValue.ToString = "True" Then
                            If File.Exists(out_dir + "\contigs_best\" + DataGridView1.Rows(i - 1).Cells(2).Value.ToString + ".fasta") Then
                                File.Delete(out_dir + "\contigs_best\" + DataGridView1.Rows(i - 1).Cells(2).Value.ToString + ".fasta")
                            End If
                        End If
                    Next
                End If
            End If

            For i As Integer = 1 To seqsView.Count
                If DataGridView2.Rows(i - 1).Cells(0).FormattedValue.ToString = "True" Then
                    seqs_count += 1
                    q1 += " " + """" + DataGridView2.Rows(i - 1).Cells(2).Value.ToString.Replace("\", "/") + """"
                    If DataGridView2.Rows(i - 1).Cells(3).FormattedValue.ToString = "" Then
                        q2 += " " + """" + DataGridView2.Rows(i - 1).Cells(2).Value.ToString.Replace("\", "/") + """"
                    Else
                        q2 += " " + """" + DataGridView2.Rows(i - 1).Cells(3).Value.ToString.Replace("\", "/") + """"
                    End If
                End If
            Next

            If seqs_count >= 1 And refs_count >= 1 Then
                Dim th1 As New Thread(AddressOf do_assemble)
                th1.Start()
            Else
                MsgBox("Please select at least one reference and one sequence data!")
            End If
        Else
            MsgBox("Please select an output folder!")
        End If

    End Sub

    Private Sub 全选ToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles 全选ToolStripMenuItem.Click
        For i As Integer = 1 To refsView.Count
            DataGridView1.Rows(i - 1).Cells(0).Value = True
        Next
        DataGridView1.RefreshEdit()
    End Sub

    Private Sub 清空ToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles 清空ToolStripMenuItem.Click
        For i As Integer = 1 To refsView.Count
            DataGridView1.Rows(i - 1).Cells(0).Value = False
        Next
        DataGridView1.RefreshEdit()
    End Sub

    Private Sub 失败的项目ToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles 失败的项目ToolStripMenuItem.Click
        For i As Integer = 1 To refsView.Count
            If DataGridView1.Rows(i - 1).Cells(6).Value = "success" Then
                DataGridView1.Rows(i - 1).Cells(0).Value = False
            Else
                DataGridView1.Rows(i - 1).Cells(0).Value = True

            End If
        Next
        DataGridView1.RefreshEdit()
    End Sub



    Private Sub TabPage2_VisibleChanged(sender As Object, e As EventArgs) Handles TabPage2.VisibleChanged
        If File.Exists(TextBox1.Text + "\log.txt") And TabPage2.Visible Then
            Using sr As New StreamReader(TextBox1.Text + "\log.txt")
                RichTextBox1.Text = sr.ReadToEnd
            End Using
        End If
    End Sub

    Private Sub 过深的ToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles 过深的ToolStripMenuItem.Click
        Dim max_depth As Integer = InputBox("Max Depth", "", 256)
        For i As Integer = 1 To refsView.Count
            If DataGridView1.Rows(i - 1).Cells(5).FormattedValue <> "" Then
                If CInt(DataGridView1.Rows(i - 1).Cells(5).FormattedValue) > max_depth Then
                    DataGridView1.Rows(i - 1).Cells(0).Value = True
                Else
                    DataGridView1.Rows(i - 1).Cells(0).Value = False

                End If
            End If
        Next
        DataGridView1.RefreshEdit()
    End Sub

    Private Sub 刷新数据ToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles 刷新数据ToolStripMenuItem.Click
        out_dir = TextBox1.Text
        Dim filePath As String = TextBox1.Text + "\ref_reads_count_dict.txt"
        If File.Exists(filePath) Then
            ref_filter_result(filePath)
        End If
        filePath = TextBox1.Text + "\result_dict.txt"
        If File.Exists(filePath) Then
            ref_assemble_result(filePath)
        End If


    End Sub

    Private Sub 过浅的项ToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles 过浅的项ToolStripMenuItem.Click
        Dim min_depth As Integer = InputBox("Min Depth", "", 64)
        For i As Integer = 1 To refsView.Count
            If DataGridView1.Rows(i - 1).Cells(5).FormattedValue <> "" Then
                If CInt(DataGridView1.Rows(i - 1).Cells(5).FormattedValue) > min_depth Then
                    DataGridView1.Rows(i - 1).Cells(0).Value = False
                Else
                    DataGridView1.Rows(i - 1).Cells(0).Value = True

                End If
            Else
                DataGridView1.Rows(i - 1).Cells(0).Value = True
            End If
        Next
        DataGridView1.RefreshEdit()
    End Sub

    Private Sub 打开输出目录ToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles 打开输出目录ToolStripMenuItem.Click
        If TextBox1.Text <> "" Then
            Process.Start("explorer.exe", TextBox1.Text)
        End If
    End Sub


    Private Sub 迭代ToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles 迭代ToolStripMenuItem.Click
        If TextBox1.Text <> "" Then
            DataGridView1.EndEdit()
            Dim refs_count As Integer = 0
            Dim seqs_count As Integer = 0
            reads_length = 0
            ref_dir = (currentDirectory + "temp\temp_refs\").Replace("\", "/")
            out_dir = (TextBox1.Text + "/iteration").Replace("\", "/")
            DeleteDir(out_dir)
            My.Computer.FileSystem.CreateDirectory(out_dir)
            q1 = ""
            q2 = ""
            k1 = NumericUpDown1.Value.ToString
            k2 = NumericUpDown5.Value.ToString
            DeleteDir(ref_dir)
            My.Computer.FileSystem.CreateDirectory(ref_dir)

            For i As Integer = 1 To refsView.Count
                If DataGridView1.Rows(i - 1).Cells(0).FormattedValue.ToString = "True" Then
                    If File.Exists(TextBox1.Text + "\contigs_all\" + DataGridView1.Rows(i - 1).Cells(2).Value.ToString + ".fasta") Then
                        refs_count += 1
                        File.Copy(TextBox1.Text + "\contigs_all\" + DataGridView1.Rows(i - 1).Cells(2).Value.ToString + ".fasta", ref_dir + DataGridView1.Rows(i - 1).Cells(2).Value.ToString + ".fasta")
                    End If
                End If
            Next

            For i As Integer = 1 To seqsView.Count
                If DataGridView2.Rows(i - 1).Cells(0).FormattedValue.ToString = "True" Then
                    seqs_count += 1
                    q1 += " " + """" + DataGridView2.Rows(i - 1).Cells(2).Value.ToString.Replace("\", "/") + """"
                    If DataGridView2.Rows(i - 1).Cells(3).FormattedValue.ToString = "" Then
                        q2 += " " + """" + DataGridView2.Rows(i - 1).Cells(2).Value.ToString.Replace("\", "/") + """"
                    Else
                        q2 += " " + """" + DataGridView2.Rows(i - 1).Cells(3).Value.ToString.Replace("\", "/") + """"
                    End If
                End If
            Next
            If seqs_count >= 1 And refs_count >= 1 Then
                Dim th1 As New Thread(AddressOf do_all)
                th1.Start()
            Else
                MsgBox("Please select at least one reference and one sequence data!")
            End If
        Else
            MsgBox("Please select an output folder!")
        End If
    End Sub
    Public Sub do_all()
        do_filter(False)
        do_filter(True)
        do_assemble()
    End Sub

    Private Sub 进一步过滤ToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles 进一步过滤ToolStripMenuItem.Click
        DataGridView1.EndEdit()
        Dim refs_count As Integer = 0
        Dim seqs_count As Integer = 0
        reads_length = 0
        ref_dir = (currentDirectory + "temp\temp_refs\").Replace("\", "/")
        out_dir = TextBox1.Text.Replace("\", "/")
        q1 = ""
        q2 = ""
        k1 = NumericUpDown1.Value.ToString
        DeleteDir(ref_dir)
        My.Computer.FileSystem.CreateDirectory(ref_dir)

        For i As Integer = 1 To refsView.Count
            If DataGridView1.Rows(i - 1).Cells(0).FormattedValue.ToString = "True" Then
                refs_count += 1
                File.Copy(currentDirectory + "temp\org_seq\" + DataGridView1.Rows(i - 1).Cells(2).Value.ToString + ".fasta", ref_dir + DataGridView1.Rows(i - 1).Cells(2).Value.ToString + ".fasta")
            End If
        Next

        For i As Integer = 1 To seqsView.Count
            If DataGridView2.Rows(i - 1).Cells(0).FormattedValue.ToString = "True" Then
                seqs_count += 1
                q1 += " " + """" + DataGridView2.Rows(i - 1).Cells(2).Value.ToString.Replace("\", "/") + """"
                If DataGridView2.Rows(i - 1).Cells(3).FormattedValue.ToString = "" Then
                    q2 += " " + """" + DataGridView2.Rows(i - 1).Cells(2).Value.ToString.Replace("\", "/") + """"
                Else
                    q2 += " " + """" + DataGridView2.Rows(i - 1).Cells(3).Value.ToString.Replace("\", "/") + """"
                End If
            End If
        Next

        If seqs_count >= 1 And refs_count >= 1 Then
            Dim th1 As New Thread(AddressOf do_filter)
            th1.Start(True)
        Else
            MsgBox("Please select at least one reference and one sequence data!")
        End If
    End Sub

    Private Sub 从头过滤ToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles 从头过滤ToolStripMenuItem.Click
        If TextBox1.Text <> "" Then
            DataGridView1.EndEdit()
            If Directory.GetFileSystemEntries(TextBox1.Text).Length > 0 Then
                Dim result As DialogResult = MessageBox.Show("Clear the output directory?", "Confirm Operation", MessageBoxButtons.YesNo, MessageBoxIcon.Question)
                If result = DialogResult.Yes Then
                    DeleteDir(TextBox1.Text)
                    My.Computer.FileSystem.CreateDirectory(TextBox1.Text)
                End If
            End If

            Dim refs_count As Integer = 0
            Dim seqs_count As Integer = 0
            reads_length = 0
            ref_dir = (currentDirectory + "temp\temp_refs\").Replace("\", "/")
            out_dir = TextBox1.Text.Replace("\", "/")
            q1 = ""
            q2 = ""
            k1 = NumericUpDown1.Value.ToString
            DeleteDir(ref_dir)
            My.Computer.FileSystem.CreateDirectory(ref_dir)

            For i As Integer = 1 To refsView.Count
                If DataGridView1.Rows(i - 1).Cells(0).FormattedValue.ToString = "True" Then
                    refs_count += 1
                    File.Copy(currentDirectory + "temp\org_seq\" + DataGridView1.Rows(i - 1).Cells(2).Value.ToString + ".fasta", ref_dir + DataGridView1.Rows(i - 1).Cells(2).Value.ToString + ".fasta")
                End If
            Next

            For i As Integer = 1 To seqsView.Count
                If DataGridView2.Rows(i - 1).Cells(0).FormattedValue.ToString = "True" Then
                    seqs_count += 1
                    q1 += " " + """" + DataGridView2.Rows(i - 1).Cells(2).Value.ToString.Replace("\", "/") + """"
                    If DataGridView2.Rows(i - 1).Cells(3).FormattedValue.ToString = "" Then
                        q2 += " " + """" + DataGridView2.Rows(i - 1).Cells(2).Value.ToString.Replace("\", "/") + """"
                    Else
                        q2 += " " + """" + DataGridView2.Rows(i - 1).Cells(3).Value.ToString.Replace("\", "/") + """"
                    End If
                End If
            Next

            If seqs_count >= 1 And refs_count >= 1 Then
                Dim th1 As New Thread(AddressOf do_filter)
                th1.Start(False)
            Else
                MsgBox("Please select at least one reference and one sequence data!")
            End If
        Else
            MsgBox("Please select an output folder!")
        End If

    End Sub



    Private Sub 载入参考序列ToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles 载入参考序列ToolStripMenuItem.Click
        Dim opendialog As New OpenFileDialog
        opendialog.Filter = "Fasta File(*.fasta)|*.fas;*.fasta;*.fa|GenBank File|*.gb"
        opendialog.FileName = ""
        opendialog.Multiselect = True
        opendialog.DefaultExt = ".fas"
        opendialog.CheckFileExists = True
        opendialog.CheckPathExists = True
        Dim resultdialog As DialogResult = opendialog.ShowDialog()
        If resultdialog = DialogResult.OK Then
            current_file = opendialog.FileName
            mydata_Dataset.Tables("Refs Table").Clear()
            DataGridView1.DataSource = Nothing
            data_loaded = False
            If UBound(opendialog.FileNames) >= 1 Then
                If opendialog.FileName.ToLower.EndsWith(".gb") Then
                    Dim sw As New StreamWriter(root_path + "temp\temp.gb")
                    For Each file_name As String In opendialog.FileNames
                        Dim sr As New StreamReader(file_name)
                        sw.Write(sr.ReadToEnd)
                        sr.Close()
                    Next
                    sw.Close()
                    current_file = root_path + "temp\temp.gb"
                    form_config_split.Show()
                    'data_type = "gb"
                Else
                    DeleteDir(root_path + "temp\org_seq")
                    My.Computer.FileSystem.CreateDirectory(root_path + "temp\org_seq")
                    For Each FileName As String In opendialog.FileNames
                        File.Copy(FileName, root_path + "temp\org_seq\" + System.IO.Path.GetFileNameWithoutExtension(FileName) + ".fasta", True)
                    Next
                    refresh_file()
                End If

            Else
                current_file = opendialog.FileName
                If opendialog.FileName.ToLower.EndsWith(".gb") Then
                    form_config_split.Show()
                    'data_type = "gb"
                Else
                    DeleteDir(root_path + "temp\org_seq")
                    My.Computer.FileSystem.CreateDirectory(root_path + "temp\org_seq")
                    File.Copy(current_file, root_path + "temp\org_seq\" + System.IO.Path.GetFileNameWithoutExtension(current_file) + ".fasta", True)
                    'data_type = "fas"
                    refresh_file()
                End If
            End If

        End If
    End Sub

    Private Sub 导出ToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles 导出ToolStripMenuItem.Click
        Dim opendialog As New SaveFileDialog
        opendialog.Filter = "CSV File (*.csv)|*.csv;*.CSV"
        opendialog.FileName = ""
        opendialog.DefaultExt = ".csv"
        opendialog.CheckFileExists = False
        opendialog.CheckPathExists = True
        Dim resultdialog As DialogResult = opendialog.ShowDialog()
        If resultdialog = DialogResult.OK Then
            If opendialog.FileName.ToLower.EndsWith(".csv") Then
                Dim dw As New StreamWriter(opendialog.FileName, False)
                Dim state_line As String = "ID,Name"
                For j As Integer = 3 To DataGridView1.ColumnCount - 1
                    state_line += "," + DataGridView1.Columns(j).HeaderText
                Next
                dw.WriteLine(state_line)
                For i As Integer = 1 To refsView.Count
                    state_line = i.ToString
                    For j As Integer = 2 To DataGridView1.ColumnCount - 1
                        If j = 3 Then
                            state_line += "," + fasta_seq(i)
                        Else
                            state_line += "," + refsView.Item(i - 1).Item(j - 1)
                        End If
                    Next
                    dw.WriteLine(state_line)
                Next
                dw.Close()
            End If
        End If
    End Sub

    Private Sub CheckBox3_CheckedChanged(sender As Object, e As EventArgs) Handles CheckBox3.CheckedChanged
        NumericUpDown3.Enabled = CheckBox3.Checked Xor False
    End Sub

    Private Sub GroupBox3_Enter(sender As Object, e As EventArgs) Handles GroupBox3.Enter

    End Sub

    Private Sub 全自动ToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles 全自动ToolStripMenuItem.Click
        If TextBox1.Text <> "" Then
            DataGridView1.EndEdit()
            If Directory.GetFileSystemEntries(TextBox1.Text).Length > 0 Then
                Dim result As DialogResult = MessageBox.Show("Clear the output directory?", "Confirm Operation", MessageBoxButtons.YesNo, MessageBoxIcon.Question)
                If result = DialogResult.Yes Then
                    DeleteDir(TextBox1.Text)
                    My.Computer.FileSystem.CreateDirectory(TextBox1.Text)
                End If
            End If
            Dim refs_count As Integer = 0
            Dim seqs_count As Integer = 0
            reads_length = 0
            ref_dir = (currentDirectory + "temp\temp_refs\").Replace("\", "/")
            out_dir = TextBox1.Text.Replace("\", "/")
            q1 = ""
            q2 = ""
            k1 = NumericUpDown1.Value.ToString
            k2 = NumericUpDown5.Value.ToString
            DeleteDir(ref_dir)
            My.Computer.FileSystem.CreateDirectory(ref_dir)


            For i As Integer = 1 To refsView.Count
                If DataGridView1.Rows(i - 1).Cells(0).FormattedValue.ToString = "True" Then
                    refs_count += 1
                    File.Copy(currentDirectory + "temp\org_seq\" + DataGridView1.Rows(i - 1).Cells(2).Value.ToString + ".fasta", ref_dir + DataGridView1.Rows(i - 1).Cells(2).Value.ToString + ".fasta")
                End If
            Next
            For i As Integer = 1 To refsView.Count
                If DataGridView1.Rows(i - 1).Cells(0).FormattedValue.ToString = "True" Then
                    If File.Exists(out_dir + "\contigs_best\" + DataGridView1.Rows(i - 1).Cells(2).Value.ToString + ".fasta") Then
                        File.Delete(out_dir + "\contigs_best\" + DataGridView1.Rows(i - 1).Cells(2).Value.ToString + ".fasta")
                    End If
                End If
            Next

            For i As Integer = 1 To seqsView.Count
                If DataGridView2.Rows(i - 1).Cells(0).FormattedValue.ToString = "True" Then
                    seqs_count += 1
                    q1 += " " + """" + DataGridView2.Rows(i - 1).Cells(2).Value.ToString.Replace("\", "/") + """"
                    If DataGridView2.Rows(i - 1).Cells(3).FormattedValue.ToString = "" Then
                        q2 += " " + """" + DataGridView2.Rows(i - 1).Cells(2).Value.ToString.Replace("\", "/") + """"
                    Else
                        q2 += " " + """" + DataGridView2.Rows(i - 1).Cells(3).Value.ToString.Replace("\", "/") + """"
                    End If
                End If
            Next

            If seqs_count >= 1 And refs_count >= 1 Then
                Dim th1 As New Thread(AddressOf do_all)
                th1.Start()
            Else
                MsgBox("Please select at least one reference and one sequence data!")
            End If
        Else
            MsgBox("Please select an output folder!")
        End If

    End Sub

    Private Sub Label6_Click(sender As Object, e As EventArgs) Handles Label6.Click

    End Sub
End Class