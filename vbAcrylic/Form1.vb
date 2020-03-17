Imports System.Runtime.InteropServices
Imports System.Windows.Interop

Public Class Form1
    Private Declare Sub keybd_event Lib "user32" (ByVal bVk As Byte, ByVal bScan As Byte, ByVal dwFlags As Long, ByVal dwExtraInfo As Long)
    Private Const KEYEVENTF_KEYUP = &H2
    Private Const VK_LWIN = &H5B
    Private Sub Form1_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        HideAllOtherWindows()
        EnableBlur()
        Refresh()
    End Sub
    Public Sub HideAllOtherWindows()
        keybd_event(VK_LWIN, 0, 0, 0)
        keybd_event(Keys.Home, 0, 0, 0)
        keybd_event(Keys.Home, 0, KEYEVENTF_KEYUP, 0)
        keybd_event(VK_LWIN, 0, KEYEVENTF_KEYUP, 0)
    End Sub
    Protected Overrides ReadOnly Property CreateParams As CreateParams
        Get
            Const CS_DROPSHADOW As Integer = &H20000
            Dim cp As CreateParams = MyBase.CreateParams
            cp.ClassStyle = cp.ClassStyle Or CS_DROPSHADOW
            Return cp
        End Get
    End Property

    Friend Enum AccentState
        ACCENT_DISABLED = 0
        ACCENT_ENABLE_GRADIENT = 1
        ACCENT_ENABLE_TRANSPARENTGRADIENT = 2
        ACCENT_ENABLE_BLURBEHIND = 3
        ACCENT_ENABLE_ACRYLICBLURBEHIND = 4
        ACCENT_INVALID_STATE = 5
    End Enum

    <StructLayout(LayoutKind.Sequential)>
    Friend Structure AccentPolicy
        Public AccentState As AccentState
        Public AccentFlags As Integer
        Public GradientColor As Integer
        Public AnimationId As Integer
    End Structure

    <StructLayout(LayoutKind.Sequential)>
    Friend Structure WindowCompositionAttributeData
        Public Attribute As WindowCompositionAttribute
        Public Data As IntPtr
        Public SizeOfData As Integer
    End Structure

    Friend Enum WindowCompositionAttribute
        ' ...
        WCA_ACCENT_POLICY = 19
        ' ...
    End Enum

    <DllImport("user32.dll")>
    Friend Shared Function SetWindowCompositionAttribute(ByVal hwnd As IntPtr, ByRef data As WindowCompositionAttributeData) As Integer
    End Function

    Private _blurOpacity As UInteger

    Public Property BlurOpacity As Double
        Get
            Return _blurOpacity
        End Get
        Set(ByVal value As Double)
            _blurOpacity = CUInt(value)
            EnableBlur()
        End Set
    End Property
    Private _blurBackgroundColor As UInteger = &HFFFFFF

    Friend Sub EnableBlur()
        Dim windowHelper = Me
        Dim accent = New AccentPolicy()
        accent.AccentState = AccentState.ACCENT_ENABLE_ACRYLICBLURBEHIND
        accent.GradientColor = _blurBackgroundColor
        accent.AccentFlags = &H20 Or &H40 Or &H80 Or &H100
        Dim accentStructSize = Marshal.SizeOf(accent)
        Dim accentPtr = Marshal.AllocHGlobal(accentStructSize)
        Marshal.StructureToPtr(accent, accentPtr, False)
        Dim data = New WindowCompositionAttributeData()
        data.Attribute = WindowCompositionAttribute.WCA_ACCENT_POLICY
        data.SizeOfData = accentStructSize
        data.Data = accentPtr
        SetWindowCompositionAttribute(windowHelper.Handle, data)
        Marshal.FreeHGlobal(accentPtr)
    End Sub

    Private Sub TrackBar2_Scroll(sender As Object, e As EventArgs) Handles TrackBar2.Scroll
        Panel1.BackColor = Color.FromArgb(TrackBar2.Value, 0, 0, 0)
    End Sub
    Dim IsMouseDown As Boolean
    Dim startPoint

    Private Sub Panel4_MouseUp(ByVal sender As Object, ByVal e As System.Windows.Forms.MouseEventArgs) Handles Panel4.MouseUp
        IsMouseDown = False
    End Sub

    Private Sub Panel4_MouseMove(ByVal sender As Object, ByVal e As System.Windows.Forms.MouseEventArgs) Handles Panel4.MouseMove
        If IsMouseDown Then
            Dim p1 = New Point(e.X, e.Y)
            Dim p2 = PointToScreen(p1)
            Dim p3 = New Point(p2.X - startPoint.X, p2.Y - startPoint.Y)
            Location = p3
            Opacity = 0.95
        End If
    End Sub

    Private Sub Panel4_MouseDown(ByVal sender As Object, ByVal e As MouseEventArgs) Handles Panel4.MouseDown
        If e.Button = MouseButtons.Left Then
            Me.Panel4.Capture = False
            Const WM_NCLBUTTONDOWN As Integer = &HA1S
            Const HTCAPTION As Integer = 2
            Dim msg As Message = Message.Create(Me.Handle, WM_NCLBUTTONDOWN, New IntPtr(HTCAPTION), IntPtr.Zero)
            Me.DefWndProc(msg)
        End If
    End Sub


End Class
