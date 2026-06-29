<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
Partial Class Form1 : Inherits System.Windows.Forms.Form

    'Form overrides dispose to clean up the component list.
    <System.Diagnostics.DebuggerNonUserCode()>
    Protected Overrides Sub Dispose(ByVal disposing As Boolean)
        Try
            If disposing AndAlso components IsNot Nothing Then
                components.Dispose()
            End If
        Finally
            MyBase.Dispose(disposing)
        End Try
    End Sub

    'Required by the Windows Form Designer
    Private components As System.ComponentModel.IContainer

    'NOTE: The following procedure is required by the Windows Form Designer
    'It can be modified using the Windows Form Designer.  
    'Do not modify it using the code editor.
    <System.Diagnostics.DebuggerStepThrough()>
    Private Sub InitializeComponent()
        Me.components = New System.ComponentModel.Container()
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(Form1))
        Me.LabelFile = New System.Windows.Forms.Label()
        Me.TextBoxCrc = New System.Windows.Forms.TextBox()
        Me.TextBoxFile = New System.Windows.Forms.TextBox()
        Me.LabelCrc = New System.Windows.Forms.Label()
        Me.ButtonBrowse = New System.Windows.Forms.Button()
        Me.OpenFileDialog1 = New System.Windows.Forms.OpenFileDialog()
        Me.LinkLabelGithub = New System.Windows.Forms.LinkLabel()
        Me.ToolTip1 = New System.Windows.Forms.ToolTip(Me.components)
        Me.ButtonClear = New System.Windows.Forms.Button()
        Me.ButtonCopy = New System.Windows.Forms.Button()
        Me.StatusStrip1 = New System.Windows.Forms.StatusStrip()
        Me.ToolStripStatusLabel1 = New System.Windows.Forms.ToolStripStatusLabel()
        Me.StatusStrip1.SuspendLayout()
        Me.SuspendLayout()
        '
        'LabelFile
        '
        Me.LabelFile.Location = New System.Drawing.Point(12, 10)
        Me.LabelFile.Name = "LabelFile"
        Me.LabelFile.Size = New System.Drawing.Size(38, 27)
        Me.LabelFile.TabIndex = 0
        Me.LabelFile.Text = "File"
        Me.LabelFile.TextAlign = System.Drawing.ContentAlignment.MiddleLeft
        '
        'TextBoxCrc
        '
        Me.TextBoxCrc.Enabled = False
        Me.TextBoxCrc.Location = New System.Drawing.Point(56, 45)
        Me.TextBoxCrc.Margin = New System.Windows.Forms.Padding(3, 4, 3, 4)
        Me.TextBoxCrc.MaxLength = 8
        Me.TextBoxCrc.Name = "TextBoxCrc"
        Me.TextBoxCrc.ReadOnly = True
        Me.TextBoxCrc.Size = New System.Drawing.Size(106, 27)
        Me.TextBoxCrc.TabIndex = 4
        Me.TextBoxCrc.TextAlign = System.Windows.Forms.HorizontalAlignment.Center
        Me.TextBoxCrc.WordWrap = False
        '
        'TextBoxFile
        '
        Me.TextBoxFile.Enabled = False
        Me.TextBoxFile.Location = New System.Drawing.Point(56, 10)
        Me.TextBoxFile.Margin = New System.Windows.Forms.Padding(3, 4, 3, 4)
        Me.TextBoxFile.MaxLength = 999999999
        Me.TextBoxFile.Name = "TextBoxFile"
        Me.TextBoxFile.ReadOnly = True
        Me.TextBoxFile.Size = New System.Drawing.Size(258, 27)
        Me.TextBoxFile.TabIndex = 2
        Me.TextBoxFile.WordWrap = False
        '
        'LabelCrc
        '
        Me.LabelCrc.Location = New System.Drawing.Point(12, 45)
        Me.LabelCrc.Name = "LabelCrc"
        Me.LabelCrc.Size = New System.Drawing.Size(38, 27)
        Me.LabelCrc.TabIndex = 3
        Me.LabelCrc.Text = "CRC"
        Me.LabelCrc.TextAlign = System.Drawing.ContentAlignment.MiddleLeft
        '
        'ButtonBrowse
        '
        Me.ButtonBrowse.Image = Global.My.Resources.Resource1.browse
        Me.ButtonBrowse.Location = New System.Drawing.Point(320, 10)
        Me.ButtonBrowse.Margin = New System.Windows.Forms.Padding(3, 4, 3, 4)
        Me.ButtonBrowse.Name = "ButtonBrowse"
        Me.ButtonBrowse.Size = New System.Drawing.Size(33, 27)
        Me.ButtonBrowse.TabIndex = 1
        Me.ButtonBrowse.Text = "📂"
        Me.ButtonBrowse.UseVisualStyleBackColor = True
        '
        'OpenFileDialog1
        '
        Me.OpenFileDialog1.FileName = "OpenFileDialog1"
        Me.OpenFileDialog1.Filter = "All files (*.*)|*.*"
        Me.OpenFileDialog1.Title = "Select a file"
        '
        'LinkLabelGithub
        '
        Me.LinkLabelGithub.LinkBehavior = System.Windows.Forms.LinkBehavior.NeverUnderline
        Me.LinkLabelGithub.Location = New System.Drawing.Point(320, 45)
        Me.LinkLabelGithub.Name = "LinkLabelGithub"
        Me.LinkLabelGithub.Size = New System.Drawing.Size(24, 27)
        Me.LinkLabelGithub.TabIndex = 7
        Me.LinkLabelGithub.TabStop = True
        Me.LinkLabelGithub.Text = "🌐"
        Me.LinkLabelGithub.TextAlign = System.Drawing.ContentAlignment.MiddleCenter
        '
        'ButtonClear
        '
        Me.ButtonClear.Enabled = False
        Me.ButtonClear.Image = Global.My.Resources.Resource1.clear
        Me.ButtonClear.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft
        Me.ButtonClear.Location = New System.Drawing.Point(244, 45)
        Me.ButtonClear.Name = "ButtonClear"
        Me.ButtonClear.Size = New System.Drawing.Size(70, 27)
        Me.ButtonClear.TabIndex = 6
        Me.ButtonClear.Text = "Clear"
        Me.ButtonClear.TextAlign = System.Drawing.ContentAlignment.TopRight
        Me.ButtonClear.UseVisualStyleBackColor = True
        '
        'ButtonCopy
        '
        Me.ButtonCopy.Enabled = False
        Me.ButtonCopy.Image = Global.My.Resources.Resource1.copy
        Me.ButtonCopy.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft
        Me.ButtonCopy.Location = New System.Drawing.Point(168, 45)
        Me.ButtonCopy.Name = "ButtonCopy"
        Me.ButtonCopy.Size = New System.Drawing.Size(70, 27)
        Me.ButtonCopy.TabIndex = 5
        Me.ButtonCopy.Text = "Copy"
        Me.ButtonCopy.TextAlign = System.Drawing.ContentAlignment.TopRight
        Me.ButtonCopy.UseVisualStyleBackColor = True
        '
        'StatusStrip1
        '
        Me.StatusStrip1.Items.AddRange(New System.Windows.Forms.ToolStripItem() {Me.ToolStripStatusLabel1})
        Me.StatusStrip1.Location = New System.Drawing.Point(0, 79)
        Me.StatusStrip1.Name = "StatusStrip1"
        Me.StatusStrip1.Size = New System.Drawing.Size(364, 22)
        Me.StatusStrip1.SizingGrip = False
        Me.StatusStrip1.TabIndex = 8
        Me.StatusStrip1.Text = "StatusStrip1"
        '
        'ToolStripStatusLabel1
        '
        Me.ToolStripStatusLabel1.Name = "ToolStripStatusLabel1"
        Me.ToolStripStatusLabel1.Size = New System.Drawing.Size(57, 17)
        Me.ToolStripStatusLabel1.Text = "Waiting..."
        '
        'Form1
        '
        Me.AllowDrop = True
        Me.AutoScaleDimensions = New System.Drawing.SizeF(8.0!, 20.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.BackColor = System.Drawing.SystemColors.Control
        Me.ClientSize = New System.Drawing.Size(364, 101)
        Me.Controls.Add(Me.StatusStrip1)
        Me.Controls.Add(Me.ButtonClear)
        Me.Controls.Add(Me.ButtonCopy)
        Me.Controls.Add(Me.LinkLabelGithub)
        Me.Controls.Add(Me.ButtonBrowse)
        Me.Controls.Add(Me.LabelCrc)
        Me.Controls.Add(Me.TextBoxFile)
        Me.Controls.Add(Me.TextBoxCrc)
        Me.Controls.Add(Me.LabelFile)
        Me.DoubleBuffered = True
        Me.Font = New System.Drawing.Font("Segoe UI", 11.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle
        Me.Icon = CType(resources.GetObject("$this.Icon"), System.Drawing.Icon)
        Me.Margin = New System.Windows.Forms.Padding(5)
        Me.MaximizeBox = False
        Me.Name = "Form1"
        Me.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen
        Me.Text = "CRC-32 Calculator"
        Me.StatusStrip1.ResumeLayout(False)
        Me.StatusStrip1.PerformLayout()
        Me.ResumeLayout(False)
        Me.PerformLayout()

    End Sub

    Friend WithEvents LabelFile As Label
    Friend WithEvents TextBoxCrc As TextBox
    Friend WithEvents TextBoxFile As TextBox
    Friend WithEvents LabelCrc As Label
    Friend WithEvents ButtonBrowse As Button
    Friend WithEvents OpenFileDialog1 As OpenFileDialog
    Friend WithEvents LinkLabelGithub As LinkLabel
    Friend WithEvents ButtonCopy As Button
    Friend WithEvents ButtonClear As Button
    Friend WithEvents ToolTip1 As ToolTip
    Friend WithEvents StatusStrip1 As StatusStrip
    Friend WithEvents ToolStripStatusLabel1 As ToolStripStatusLabel
End Class
