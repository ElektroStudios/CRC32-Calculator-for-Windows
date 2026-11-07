
#Region " Option Statements "

Option Strict On
Option Explicit On
Option Infer Off

#End Region

#Region " Imports "

Imports System.Drawing

#End Region

#Region " DarkThemeColors "

''' <summary>
''' Provides the color palette of the dark theme, inspired by the Windows 11 dark mode.
''' </summary>
Friend Module DarkThemeColors

    ''' <summary>
    ''' The background color of windows.
    ''' </summary>
    Friend ReadOnly WindowBackground As Color = Color.FromArgb(32, 32, 32)

    ''' <summary>
    ''' The background color of input and button controls.
    ''' </summary>
    Friend ReadOnly ControlBackground As Color = Color.FromArgb(45, 45, 45)

    ''' <summary>
    ''' The background color of controls when the mouse pointer is over them.
    ''' </summary>
    Friend ReadOnly ControlHover As Color = Color.FromArgb(58, 58, 58)

    ''' <summary>
    ''' The background color of controls while they are pressed.
    ''' </summary>
    Friend ReadOnly ControlPressed As Color = Color.FromArgb(70, 70, 70)

    ''' <summary>
    ''' The color of control borders and separators.
    ''' </summary>
    Friend ReadOnly Border As Color = Color.FromArgb(85, 85, 85)

    ''' <summary>
    ''' The background color of status bars and tooltips.
    ''' </summary>
    Friend ReadOnly StatusBackground As Color = Color.FromArgb(43, 43, 43)

    ''' <summary>
    ''' The primary text color.
    ''' </summary>
    Friend ReadOnly Text As Color = Color.FromArgb(242, 242, 242)

    ''' <summary>
    ''' The secondary (dimmed) text color.
    ''' </summary>
    Friend ReadOnly SecondaryText As Color = Color.FromArgb(166, 166, 166)

    ''' <summary>
    ''' The text color of disabled elements.
    ''' </summary>
    Friend ReadOnly DisabledText As Color = Color.FromArgb(110, 110, 110)

    ''' <summary>
    ''' The color of hyperlinks.
    ''' </summary>
    Friend ReadOnly Link As Color = Color.FromArgb(76, 194, 255)

    ''' <summary>
    ''' The color of hyperlinks while they are clicked.
    ''' </summary>
    Friend ReadOnly LinkActive As Color = Color.FromArgb(153, 235, 255)

    ''' <summary>
    ''' The fill color of progress bars.
    ''' </summary>
    Friend ReadOnly ProgressFill As Color = Color.FromArgb(16, 124, 16)

End Module

#End Region
