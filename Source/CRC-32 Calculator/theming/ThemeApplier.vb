
#Region " Option Statements "

Option Strict On
Option Explicit On
Option Infer Off

#End Region

#Region " Imports "

Imports System.Collections.Generic
Imports System.ComponentModel
Imports System.Diagnostics
Imports System.Drawing
Imports System.Runtime.CompilerServices

#End Region

#Region " ThemeApplier "

''' <summary>
''' Applies the dark theme to a control hierarchy (and its tool strip items and tooltips),
''' and restores the original (designer) appearance when switching back to the light theme.
''' </summary>
'''
''' <remarks>
''' Before modifying any object for the first time, the state of every modified property is captured,
''' including whether the property was explicitly set or inherits its value (ambient/default value).
''' Switching back to the light theme restores exactly that state, so the original design is preserved
''' (e.g. a read-only <see cref="TextBox"/> keeps its system gray background,
''' because its <see cref="Control.BackColor"/> is reset instead of being assigned).
''' <para></para>
''' Applying the light theme to objects that were never modified does nothing.
''' </remarks>
Friend Module ThemeApplier

#Region " Fields "

    ''' <summary>
    ''' The captured original property states of every object modified by the dark theme.
    ''' </summary>
    Private ReadOnly Snapshots As New ConditionalWeakTable(Of Object, PropertySnapshot)()

    ''' <summary>
    ''' The tooltips whose <see cref="ToolTip.Draw"/> event is already handled.
    ''' </summary>
    Private ReadOnly HookedToolTips As New ConditionalWeakTable(Of ToolTip, Object)()

#End Region

#Region " Public Methods "

    ''' <summary>
    ''' Applies the dark or light theme to the specified control, all its child controls and their tool strip items.
    ''' </summary>
    '''
    ''' <param name="rootControl">
    ''' The root control (usually a <see cref="Form"/>).
    ''' </param>
    '''
    ''' <param name="useDarkTheme">
    ''' <see langword="True"/> to apply the dark theme;
    ''' <see langword="False"/> to restore the original (light) appearance.
    ''' </param>
    '''
    ''' <exception cref="ArgumentNullException">
    ''' <paramref name="rootControl"/> is <see langword="Nothing"/>.
    ''' </exception>
    <DebuggerStepThrough>
    Friend Sub ApplyTheme(rootControl As Control, useDarkTheme As Boolean)

        If rootControl Is Nothing Then
            Throw New ArgumentNullException(NameOf(rootControl))
        End If

        rootControl.SuspendLayout()
        Try
            Dim pendingControls As New Stack(Of Control)()
            pendingControls.Push(rootControl)

            Do While pendingControls.Count > 0
                Dim ctrl As Control = pendingControls.Pop()

                ThemeApplier.ApplyControlTheme(ctrl, useDarkTheme)

                Dim strip As ToolStrip = TryCast(ctrl, ToolStrip)
                If strip IsNot Nothing Then
                    ThemeApplier.ApplyToolStripItemsTheme(strip, useDarkTheme)
                    ' The child controls of a tool strip are the controls hosted by its items, already handled as items.
                    Continue Do
                End If

                For Each child As Control In ctrl.Controls
                    pendingControls.Push(child)
                Next
            Loop

        Finally
            rootControl.ResumeLayout(performLayout:=True)

        End Try

        rootControl.Invalidate(invalidateChildren:=True)
    End Sub

    ''' <summary>
    ''' Applies the dark or light theme to the specified tooltip.
    ''' </summary>
    '''
    ''' <param name="toolTip">
    ''' The tooltip.
    ''' </param>
    '''
    ''' <param name="useDarkTheme">
    ''' <see langword="True"/> to apply the dark theme (owner-drawn tooltip);
    ''' <see langword="False"/> to restore the original (system-drawn) appearance.
    ''' </param>
    '''
    ''' <exception cref="ArgumentNullException">
    ''' <paramref name="toolTip"/> is <see langword="Nothing"/>.
    ''' </exception>
    <DebuggerStepThrough>
    Friend Sub ApplyTheme(toolTip As ToolTip, useDarkTheme As Boolean)

        If toolTip Is Nothing Then
            Throw New ArgumentNullException(NameOf(toolTip))
        End If

        If Not useDarkTheme Then
            ThemeApplier.RestoreSnapshot(toolTip)
            Exit Sub
        End If

        ThemeApplier.CaptureSnapshot(toolTip, NameOf(ToolTip.OwnerDraw), NameOf(ToolTip.BackColor), NameOf(ToolTip.ForeColor))

        Dim hookMarker As Object = Nothing
        If Not ThemeApplier.HookedToolTips.TryGetValue(toolTip, hookMarker) Then
            AddHandler toolTip.Draw, AddressOf ThemeApplier.OnDarkToolTipDraw
            ThemeApplier.HookedToolTips.Add(toolTip, New Object())
        End If

        toolTip.BackColor = DarkThemeColors.StatusBackground
        toolTip.ForeColor = DarkThemeColors.Text
        toolTip.OwnerDraw = True
    End Sub

#End Region

#Region " Private Methods "

    ''' <summary>
    ''' Applies the dark or light theme to a single control (not to its children).
    ''' </summary>
    '''
    ''' <param name="ctrl">
    ''' The control.
    ''' </param>
    '''
    ''' <param name="useDarkTheme">
    ''' <see langword="True"/> to apply the dark theme; <see langword="False"/> to restore the original appearance.
    ''' </param>
    <DebuggerStepThrough>
    Private Sub ApplyControlTheme(ctrl As Control, useDarkTheme As Boolean)

        Dim button As ButtonBase = TryCast(ctrl, ButtonBase)

        If Not useDarkTheme Then
            ThemeApplier.RestoreSnapshot(ctrl)
            If button IsNot Nothing Then
                ThemeApplier.RestoreSnapshot(button.FlatAppearance)
            End If
            Exit Sub
        End If

        If TypeOf ctrl Is TextBoxBase Then
            Dim textBox As TextBoxBase = DirectCast(ctrl, TextBoxBase)
            ThemeApplier.CaptureSnapshot(textBox, NameOf(Control.BackColor), NameOf(Control.ForeColor), NameOf(TextBoxBase.BorderStyle))
            textBox.BackColor = DarkThemeColors.ControlBackground
            textBox.ForeColor = DarkThemeColors.Text
            textBox.BorderStyle = BorderStyle.FixedSingle

        ElseIf button IsNot Nothing Then
            ' Note: UseVisualStyleBackColor is captured (and therefore restored) after BackColor, on purpose.
            ThemeApplier.CaptureSnapshot(button, NameOf(Control.BackColor), NameOf(Control.ForeColor),
                                         NameOf(ButtonBase.FlatStyle), NameOf(ButtonBase.UseVisualStyleBackColor))
            ThemeApplier.CaptureSnapshot(button.FlatAppearance, NameOf(FlatButtonAppearance.BorderColor),
                                         NameOf(FlatButtonAppearance.MouseOverBackColor), NameOf(FlatButtonAppearance.MouseDownBackColor))
            button.UseVisualStyleBackColor = False
            button.FlatStyle = FlatStyle.Flat
            button.BackColor = DarkThemeColors.ControlBackground
            button.ForeColor = DarkThemeColors.Text
            button.FlatAppearance.BorderColor = DarkThemeColors.Border
            button.FlatAppearance.MouseOverBackColor = DarkThemeColors.ControlHover
            button.FlatAppearance.MouseDownBackColor = DarkThemeColors.ControlPressed

        ElseIf TypeOf ctrl Is LinkLabel Then
            Dim link As LinkLabel = DirectCast(ctrl, LinkLabel)
            ThemeApplier.CaptureSnapshot(link, NameOf(Control.BackColor), NameOf(Control.ForeColor),
                                         NameOf(LinkLabel.LinkColor), NameOf(LinkLabel.ActiveLinkColor),
                                         NameOf(LinkLabel.VisitedLinkColor), NameOf(LinkLabel.DisabledLinkColor))
            link.BackColor = DarkThemeColors.WindowBackground
            link.ForeColor = DarkThemeColors.Text
            link.LinkColor = DarkThemeColors.Link
            link.ActiveLinkColor = DarkThemeColors.LinkActive
            link.VisitedLinkColor = DarkThemeColors.Link
            link.DisabledLinkColor = DarkThemeColors.DisabledText

        ElseIf TypeOf ctrl Is ToolStrip Then
            Dim strip As ToolStrip = DirectCast(ctrl, ToolStrip)
            ' Note: RenderMode is captured before replacing the renderer, so restoring it recreates the original renderer.
            ThemeApplier.CaptureSnapshot(strip, NameOf(ToolStrip.RenderMode), NameOf(Control.BackColor), NameOf(Control.ForeColor))
            If Not TypeOf strip.Renderer Is DarkToolStripRenderer Then
                strip.Renderer = New DarkToolStripRenderer()
            End If
            strip.BackColor = DarkThemeColors.StatusBackground
            strip.ForeColor = DarkThemeColors.Text

        Else
            ' Forms, containers, labels and any other control.
            ThemeApplier.CaptureSnapshot(ctrl, NameOf(Control.BackColor), NameOf(Control.ForeColor))
            ctrl.BackColor = DarkThemeColors.WindowBackground
            ctrl.ForeColor = DarkThemeColors.Text

        End If
    End Sub

    ''' <summary>
    ''' Applies the dark or light theme to all the items (including drop-down items) of the specified tool strip.
    ''' </summary>
    '''
    ''' <param name="strip">
    ''' The tool strip.
    ''' </param>
    '''
    ''' <param name="useDarkTheme">
    ''' <see langword="True"/> to apply the dark theme; <see langword="False"/> to restore the original appearance.
    ''' </param>
    <DebuggerStepThrough>
    Private Sub ApplyToolStripItemsTheme(strip As ToolStrip, useDarkTheme As Boolean)

        Dim pendingItems As New Stack(Of ToolStripItem)()
        For Each item As ToolStripItem In strip.Items
            pendingItems.Push(item)
        Next

        Do While pendingItems.Count > 0
            Dim item As ToolStripItem = pendingItems.Pop()

            Dim progressBar As TextToolStripProgressBar = TryCast(item, TextToolStripProgressBar)
            If progressBar IsNot Nothing Then
                progressBar.UseDarkTheme = useDarkTheme

            ElseIf useDarkTheme Then
                ' Items without an explicit text color inherit it from the tool strip.
                ' Items with an explicit text color (e.g. a dimmed status label) get the secondary text color.
                Dim foreColorProperty As PropertyDescriptor = ThemeApplier.GetPropertyDescriptor(item, NameOf(ToolStripItem.ForeColor))
                Dim hasSnapshot As Boolean = ThemeApplier.HasSnapshot(item)
                If hasSnapshot OrElse foreColorProperty.ShouldSerializeValue(item) Then
                    ThemeApplier.CaptureSnapshot(item, NameOf(ToolStripItem.ForeColor))
                    item.ForeColor = DarkThemeColors.SecondaryText
                End If

            Else
                ThemeApplier.RestoreSnapshot(item)

            End If

            Dim dropDownItem As ToolStripDropDownItem = TryCast(item, ToolStripDropDownItem)
            If dropDownItem IsNot Nothing Then
                For Each subItem As ToolStripItem In dropDownItem.DropDownItems
                    pendingItems.Push(subItem)
                Next
            End If
        Loop
    End Sub

    ''' <summary>
    ''' Captures the current state of the specified properties of an object,
    ''' only if its state was not already captured (the first captured state is the original one).
    ''' </summary>
    '''
    ''' <param name="target">
    ''' The target object.
    ''' </param>
    '''
    ''' <param name="propertyNames">
    ''' The names of the properties to capture, in the order they must be restored.
    ''' </param>
    <DebuggerStepThrough>
    Private Sub CaptureSnapshot(target As Object, ParamArray propertyNames As String())

        If ThemeApplier.HasSnapshot(target) Then
            Exit Sub
        End If

        ThemeApplier.Snapshots.Add(target, New PropertySnapshot(target, propertyNames))
    End Sub

    ''' <summary>
    ''' Restores the original state of the properties of an object, if it was captured, and discards the captured state.
    ''' </summary>
    '''
    ''' <param name="target">
    ''' The target object.
    ''' </param>
    <DebuggerStepThrough>
    Private Sub RestoreSnapshot(target As Object)

        Dim snapshot As PropertySnapshot = Nothing
        If ThemeApplier.Snapshots.TryGetValue(target, snapshot) Then
            snapshot.Restore()
            ThemeApplier.Snapshots.Remove(target)
        End If
    End Sub

    ''' <summary>
    ''' Determines whether the original state of an object was captured.
    ''' </summary>
    '''
    ''' <param name="target">
    ''' The target object.
    ''' </param>
    '''
    ''' <returns>
    ''' <see langword="True"/> if the original state was captured; otherwise, <see langword="False"/>.
    ''' </returns>
    <DebuggerStepThrough>
    Private Function HasSnapshot(target As Object) As Boolean

        Dim snapshot As PropertySnapshot = Nothing
        Return ThemeApplier.Snapshots.TryGetValue(target, snapshot)
    End Function

    ''' <summary>
    ''' Gets the <see cref="PropertyDescriptor"/> of the specified property of an object.
    ''' </summary>
    '''
    ''' <param name="target">
    ''' The target object.
    ''' </param>
    '''
    ''' <param name="propertyName">
    ''' The property name.
    ''' </param>
    '''
    ''' <returns>
    ''' The resulting <see cref="PropertyDescriptor"/>.
    ''' </returns>
    '''
    ''' <exception cref="InvalidOperationException">
    ''' The property does not exist.
    ''' </exception>
    <DebuggerStepThrough>
    Private Function GetPropertyDescriptor(target As Object, propertyName As String) As PropertyDescriptor

        Dim descriptor As PropertyDescriptor = TypeDescriptor.GetProperties(target)(propertyName)
        If descriptor Is Nothing Then
            Throw New InvalidOperationException($"The type '{target.GetType().FullName}' has no property named '{propertyName}'.")
        End If

        Return descriptor
    End Function

    ''' <summary>
    ''' Handles the <see cref="ToolTip.Draw"/> event of owner-drawn (dark themed) tooltips.
    ''' </summary>
    '''
    ''' <param name="sender">
    ''' The source of the event.
    ''' </param>
    '''
    ''' <param name="e">
    ''' A <see cref="DrawToolTipEventArgs"/> that contains the event data.
    ''' </param>
    Private Sub OnDarkToolTipDraw(sender As Object, e As DrawToolTipEventArgs)

        Dim bounds As Rectangle = e.Bounds

        Using backgroundBrush As New SolidBrush(DarkThemeColors.StatusBackground),
              borderPen As New Pen(DarkThemeColors.Border)

            e.Graphics.FillRectangle(backgroundBrush, bounds)
            e.Graphics.DrawRectangle(borderPen, bounds.X, bounds.Y, bounds.Width - 1, bounds.Height - 1)
        End Using

        Const flags As TextFormatFlags = TextFormatFlags.HorizontalCenter Or
                                         TextFormatFlags.VerticalCenter Or
                                         TextFormatFlags.NoPrefix
        TextRenderer.DrawText(e.Graphics, e.ToolTipText, e.Font, bounds, DarkThemeColors.Text, flags)
    End Sub

#End Region

#Region " PropertySnapshot "

    ''' <summary>
    ''' Stores the state of a set of properties of an object, to restore it later.
    ''' </summary>
    Private NotInheritable Class PropertySnapshot

        ''' <summary>
        ''' The object whose properties were captured.
        ''' </summary>
        Private ReadOnly target As Object

        ''' <summary>
        ''' The captured property states, in restoration order.
        ''' </summary>
        Private ReadOnly states As List(Of PropertyState)

        ''' <summary>
        ''' Initializes a new instance of the <see cref="PropertySnapshot"/> class, capturing the current property states.
        ''' </summary>
        '''
        ''' <param name="target">
        ''' The target object.
        ''' </param>
        '''
        ''' <param name="propertyNames">
        ''' The names of the properties to capture, in restoration order.
        ''' </param>
        Friend Sub New(target As Object, propertyNames As String())

            If target Is Nothing Then
                Throw New ArgumentNullException(NameOf(target))
            End If
            If propertyNames Is Nothing OrElse propertyNames.Length = 0 Then
                Throw New ArgumentException("At least one property name must be specified.", NameOf(propertyNames))
            End If

            Me.target = target
            Me.states = New List(Of PropertyState)(propertyNames.Length)

            For Each propertyName As String In propertyNames
                Dim descriptor As PropertyDescriptor = ThemeApplier.GetPropertyDescriptor(target, propertyName)
                Me.states.Add(New PropertyState(descriptor,
                                                isExplicitlySet:=descriptor.ShouldSerializeValue(target),
                                                value:=descriptor.GetValue(target)))
            Next
        End Sub

        ''' <summary>
        ''' Restores the captured property states: explicitly set properties get their original value back,
        ''' and properties that were not explicitly set are reset, so they inherit their ambient or default value again.
        ''' </summary>
        Friend Sub Restore()

            For Each state As PropertyState In Me.states
                If Not state.IsExplicitlySet AndAlso state.Descriptor.CanResetValue(Me.target) Then
                    state.Descriptor.ResetValue(Me.target)
                Else
                    state.Descriptor.SetValue(Me.target, state.Value)
                End If
            Next
        End Sub

    End Class

#End Region

#Region " PropertyState "

    ''' <summary>
    ''' Stores the captured state of a single property.
    ''' </summary>
    Private NotInheritable Class PropertyState

        ''' <summary>
        ''' Gets the descriptor of the property.
        ''' </summary>
        Friend ReadOnly Property Descriptor As PropertyDescriptor

        ''' <summary>
        ''' Gets a value indicating whether the property was explicitly set (not ambient nor default).
        ''' </summary>
        Friend ReadOnly Property IsExplicitlySet As Boolean

        ''' <summary>
        ''' Gets the captured property value.
        ''' </summary>
        Friend ReadOnly Property Value As Object

        ''' <summary>
        ''' Initializes a new instance of the <see cref="PropertyState"/> class.
        ''' </summary>
        '''
        ''' <param name="descriptor">
        ''' The descriptor of the property.
        ''' </param>
        '''
        ''' <param name="isExplicitlySet">
        ''' A value indicating whether the property was explicitly set.
        ''' </param>
        '''
        ''' <param name="value">
        ''' The captured property value.
        ''' </param>
        Friend Sub New(descriptor As PropertyDescriptor, isExplicitlySet As Boolean, value As Object)
            Me.Descriptor = descriptor
            Me.IsExplicitlySet = isExplicitlySet
            Me.Value = value
        End Sub

    End Class

#End Region

End Module

#End Region
