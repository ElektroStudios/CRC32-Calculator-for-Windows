
#Region " Option Statements "

Option Strict On
Option Explicit On
Option Infer Off

#End Region

#Region " Imports "

Imports System.Drawing

#End Region

#Region " DarkToolStripRenderer "

''' <summary>
''' A <see cref="ToolStripProfessionalRenderer"/> that renders tool strips, status strips and menus with the dark theme colors.
''' </summary>
Friend NotInheritable Class DarkToolStripRenderer : Inherits ToolStripProfessionalRenderer

#Region " Constructors "

    ''' <summary>
    ''' Initializes a new instance of the <see cref="DarkToolStripRenderer"/> class.
    ''' </summary>
    Friend Sub New()
        MyBase.New(New DarkColorTable())
        Me.RoundedEdges = False
    End Sub

#End Region

#Region " Overridden Methods "

    ''' <summary>
    ''' Raises the <see cref="ToolStripRenderer.RenderToolStripBorder"/> event.
    ''' <para></para>
    ''' The professional renderer draws a light (<see cref="SystemColors.ButtonHighlight"/>) line on top of status strips,
    ''' so it is replaced by a line with the dark theme border color.
    ''' </summary>
    '''
    ''' <param name="e">
    ''' A <see cref="ToolStripRenderEventArgs"/> that contains the event data.
    ''' </param>
    Protected Overrides Sub OnRenderToolStripBorder(e As ToolStripRenderEventArgs)

        If TypeOf e.ToolStrip Is StatusStrip Then
            Using borderPen As New Pen(DarkThemeColors.Border)
                e.Graphics.DrawLine(borderPen, 0, 0, e.ToolStrip.Width, 0)
            End Using
            Exit Sub
        End If

        MyBase.OnRenderToolStripBorder(e)
    End Sub

    ''' <summary>
    ''' Raises the <see cref="ToolStripRenderer.RenderItemText"/> event,
    ''' using the dark theme color for the text of disabled items.
    ''' </summary>
    '''
    ''' <param name="e">
    ''' A <see cref="ToolStripItemTextRenderEventArgs"/> that contains the event data.
    ''' </param>
    Protected Overrides Sub OnRenderItemText(e As ToolStripItemTextRenderEventArgs)

        If Not e.Item.Enabled Then
            e.TextColor = DarkThemeColors.DisabledText
        End If

        MyBase.OnRenderItemText(e)
    End Sub

    ''' <summary>
    ''' Raises the <see cref="ToolStripRenderer.RenderArrow"/> event, using the dark theme text color.
    ''' </summary>
    '''
    ''' <param name="e">
    ''' A <see cref="ToolStripArrowRenderEventArgs"/> that contains the event data.
    ''' </param>
    Protected Overrides Sub OnRenderArrow(e As ToolStripArrowRenderEventArgs)

        e.ArrowColor = If(e.Item IsNot Nothing AndAlso Not e.Item.Enabled, DarkThemeColors.DisabledText, DarkThemeColors.Text)
        MyBase.OnRenderArrow(e)
    End Sub

#End Region

#Region " DarkColorTable "

    ''' <summary>
    ''' A <see cref="ProfessionalColorTable"/> with the dark theme colors.
    ''' </summary>
    Private NotInheritable Class DarkColorTable : Inherits ProfessionalColorTable

        ''' <summary>
        ''' Initializes a new instance of the <see cref="DarkColorTable"/> class.
        ''' </summary>
        Friend Sub New()
            Me.UseSystemColors = False
        End Sub

#Region " Status Strip / Tool Strip "

        ''' <inheritdoc/>
        Public Overrides ReadOnly Property StatusStripGradientBegin As Color
            Get
                Return DarkThemeColors.StatusBackground
            End Get
        End Property

        ''' <inheritdoc/>
        Public Overrides ReadOnly Property StatusStripGradientEnd As Color
            Get
                Return DarkThemeColors.StatusBackground
            End Get
        End Property

        ''' <inheritdoc/>
        Public Overrides ReadOnly Property ToolStripGradientBegin As Color
            Get
                Return DarkThemeColors.StatusBackground
            End Get
        End Property

        ''' <inheritdoc/>
        Public Overrides ReadOnly Property ToolStripGradientMiddle As Color
            Get
                Return DarkThemeColors.StatusBackground
            End Get
        End Property

        ''' <inheritdoc/>
        Public Overrides ReadOnly Property ToolStripGradientEnd As Color
            Get
                Return DarkThemeColors.StatusBackground
            End Get
        End Property

        ''' <inheritdoc/>
        Public Overrides ReadOnly Property ToolStripBorder As Color
            Get
                Return DarkThemeColors.Border
            End Get
        End Property

        ''' <inheritdoc/>
        Public Overrides ReadOnly Property ToolStripDropDownBackground As Color
            Get
                Return DarkThemeColors.StatusBackground
            End Get
        End Property

        ''' <inheritdoc/>
        Public Overrides ReadOnly Property ToolStripContentPanelGradientBegin As Color
            Get
                Return DarkThemeColors.WindowBackground
            End Get
        End Property

        ''' <inheritdoc/>
        Public Overrides ReadOnly Property ToolStripContentPanelGradientEnd As Color
            Get
                Return DarkThemeColors.WindowBackground
            End Get
        End Property

        ''' <inheritdoc/>
        Public Overrides ReadOnly Property GripDark As Color
            Get
                Return DarkThemeColors.Border
            End Get
        End Property

        ''' <inheritdoc/>
        Public Overrides ReadOnly Property GripLight As Color
            Get
                Return DarkThemeColors.StatusBackground
            End Get
        End Property

        ''' <inheritdoc/>
        Public Overrides ReadOnly Property SeparatorDark As Color
            Get
                Return DarkThemeColors.Border
            End Get
        End Property

        ''' <inheritdoc/>
        Public Overrides ReadOnly Property SeparatorLight As Color
            Get
                Return DarkThemeColors.StatusBackground
            End Get
        End Property

#End Region

#Region " Buttons "

        ''' <inheritdoc/>
        Public Overrides ReadOnly Property ButtonSelectedHighlight As Color
            Get
                Return DarkThemeColors.ControlHover
            End Get
        End Property

        ''' <inheritdoc/>
        Public Overrides ReadOnly Property ButtonSelectedGradientBegin As Color
            Get
                Return DarkThemeColors.ControlHover
            End Get
        End Property

        ''' <inheritdoc/>
        Public Overrides ReadOnly Property ButtonSelectedGradientMiddle As Color
            Get
                Return DarkThemeColors.ControlHover
            End Get
        End Property

        ''' <inheritdoc/>
        Public Overrides ReadOnly Property ButtonSelectedGradientEnd As Color
            Get
                Return DarkThemeColors.ControlHover
            End Get
        End Property

        ''' <inheritdoc/>
        Public Overrides ReadOnly Property ButtonSelectedBorder As Color
            Get
                Return DarkThemeColors.Border
            End Get
        End Property

        ''' <inheritdoc/>
        Public Overrides ReadOnly Property ButtonPressedHighlight As Color
            Get
                Return DarkThemeColors.ControlPressed
            End Get
        End Property

        ''' <inheritdoc/>
        Public Overrides ReadOnly Property ButtonPressedGradientBegin As Color
            Get
                Return DarkThemeColors.ControlPressed
            End Get
        End Property

        ''' <inheritdoc/>
        Public Overrides ReadOnly Property ButtonPressedGradientMiddle As Color
            Get
                Return DarkThemeColors.ControlPressed
            End Get
        End Property

        ''' <inheritdoc/>
        Public Overrides ReadOnly Property ButtonPressedGradientEnd As Color
            Get
                Return DarkThemeColors.ControlPressed
            End Get
        End Property

        ''' <inheritdoc/>
        Public Overrides ReadOnly Property ButtonPressedBorder As Color
            Get
                Return DarkThemeColors.Border
            End Get
        End Property

        ''' <inheritdoc/>
        Public Overrides ReadOnly Property ButtonCheckedHighlight As Color
            Get
                Return DarkThemeColors.ControlPressed
            End Get
        End Property

        ''' <inheritdoc/>
        Public Overrides ReadOnly Property ButtonCheckedGradientBegin As Color
            Get
                Return DarkThemeColors.ControlPressed
            End Get
        End Property

        ''' <inheritdoc/>
        Public Overrides ReadOnly Property ButtonCheckedGradientMiddle As Color
            Get
                Return DarkThemeColors.ControlPressed
            End Get
        End Property

        ''' <inheritdoc/>
        Public Overrides ReadOnly Property ButtonCheckedGradientEnd As Color
            Get
                Return DarkThemeColors.ControlPressed
            End Get
        End Property

#End Region

#Region " Menus "

        ''' <inheritdoc/>
        Public Overrides ReadOnly Property MenuBorder As Color
            Get
                Return DarkThemeColors.Border
            End Get
        End Property

        ''' <inheritdoc/>
        Public Overrides ReadOnly Property MenuItemBorder As Color
            Get
                Return DarkThemeColors.Border
            End Get
        End Property

        ''' <inheritdoc/>
        Public Overrides ReadOnly Property MenuItemSelected As Color
            Get
                Return DarkThemeColors.ControlHover
            End Get
        End Property

        ''' <inheritdoc/>
        Public Overrides ReadOnly Property MenuItemSelectedGradientBegin As Color
            Get
                Return DarkThemeColors.ControlHover
            End Get
        End Property

        ''' <inheritdoc/>
        Public Overrides ReadOnly Property MenuItemSelectedGradientEnd As Color
            Get
                Return DarkThemeColors.ControlHover
            End Get
        End Property

        ''' <inheritdoc/>
        Public Overrides ReadOnly Property MenuItemPressedGradientBegin As Color
            Get
                Return DarkThemeColors.ControlPressed
            End Get
        End Property

        ''' <inheritdoc/>
        Public Overrides ReadOnly Property MenuItemPressedGradientEnd As Color
            Get
                Return DarkThemeColors.ControlPressed
            End Get
        End Property

        ''' <inheritdoc/>
        Public Overrides ReadOnly Property ImageMarginGradientBegin As Color
            Get
                Return DarkThemeColors.StatusBackground
            End Get
        End Property

        ''' <inheritdoc/>
        Public Overrides ReadOnly Property ImageMarginGradientMiddle As Color
            Get
                Return DarkThemeColors.StatusBackground
            End Get
        End Property

        ''' <inheritdoc/>
        Public Overrides ReadOnly Property ImageMarginGradientEnd As Color
            Get
                Return DarkThemeColors.StatusBackground
            End Get
        End Property

#End Region

    End Class

#End Region

End Class

#End Region
