Imports System.Web
Imports System.Collections.Generic
Imports System.Globalization
Imports System.Net

''' <summary>
''' Redirects the Request to HTTPS if it comes in on an insecure channel.
''' </summary>
Public Class HttpsOnlyModule

    Implements IHttpModule

    Private mConfig As HttpsOnlyModule.Configuration

    Public Sub Init(ByVal context As HttpApplication) Implements IHttpModule.Init

        'Check Context
        If context Is Nothing Then Exit Sub

        'Get Config
        mConfig = ConfigurationManager.GetSection("httpsOnly")
        If mConfig Is Nothing Then mConfig = New HttpsOnlyModule.Configuration

        'Check if mode is off
        If mConfig.Mode = Configuration.ConfigMode.Off Then Exit Sub

        'Add a handler to page request
        AddHandler context.BeginRequest, AddressOf Me.Application_OnBeginRequestHandler

    End Sub

    Public Sub Application_OnBeginRequestHandler(ByVal sender As Object, ByVal e As EventArgs)

        'Get request
        Dim request As HttpRequest = HttpContext.Current.Request
        If request Is Nothing Then Exit Sub

        ' Note we cannot trust IsSecureConnection when 
        ' in a webfarm, because usually only the load balancer 
        ' will come in on a secure port the request will be then 
        ' internally redirected to local machine on a specified port.

        'Only if http or not port 443
        If Not request.IsSecureConnection OrElse request.Url.Port <> mConfig.Port Then

            'Check mode, do not process if request if local and remoteonly mode is set
            If Not (request.IsLocal And mConfig.Mode = Configuration.ConfigMode.RemoteOnly) Then

                'Check ignored paths
                If Not IsIgnoredPath(request.RawUrl) Then

                    'Uri we are dealing with
                    Dim newURI As New UriBuilder(request.Url)

                    'Set scheme & port
                    newURI.Scheme = Uri.UriSchemeHttps
                    newURI.Port = -1 'removes port, the scheme (https) will determine port.  If you need an alternate port then a new config element will be needed

                    'If host domain starts with www prefix then strip it off if configured
                    If mConfig.RemoveWWWPrefix AndAlso newURI.Host.StartsWith("www.") Then newURI.Host = newURI.Host.Substring(4, newURI.Host.Length - 4)

                    'Do tld translation
                    For Each mItem As TLDTranslationElement In mConfig.TldTranslation

                        If newURI.Host.EndsWith(mItem.From) Then newURI.Host = newURI.Host.Substring(0, newURI.Host.Length - mItem.From.Length) & mItem.To

                    Next

                    'Perform permanent redirect
                    HttpContext.Current.Response.Status = "301 Moved Permanently"
                    HttpContext.Current.Response.AddHeader("Location", newURI.ToString)
                    HttpContext.Current.Response.End()

                End If

            End If

        Else

            'If https and HSTS enabled then add the appropriate header
            If mConfig.HSTSEnabled Then HttpContext.Current.Response.AddHeader("Strict-Transport-Security", "max-age=" & mConfig.HSTSMaxAge)

        End If

    End Sub

    Private Function IsIgnoredPath(ByVal Url As String) As Boolean

        For Each mItem As PathElement In mConfig.IgnoredPaths

            If Url.StartsWith(VirtualPathUtility.ToAbsolute(mItem.Path), True, CultureInfo.InvariantCulture) Then

                Return True

            End If

        Next

        Return False

    End Function

    Public Sub Dispose() Implements IHttpModule.Dispose

        ' Needed for IHttpModule

    End Sub

    Public Class Configuration

        Inherits ConfigurationSection

        Public Enum ConfigMode As Integer
            [On] = 0
            RemoteOnly = 1
            Off = 2
        End Enum

        <ConfigurationProperty("ignoredPaths", IsDefaultCollection:=False)> _
        Public ReadOnly Property IgnoredPaths() As PathCollection
            Get
                Return TryCast(Me("ignoredPaths"), PathCollection)
            End Get
        End Property

        <ConfigurationProperty("tldTranslation", IsDefaultCollection:=False)> _
        Public ReadOnly Property TldTranslation() As TLDTranslationCollection
            Get
                Return TryCast(Me("tldTranslation"), TLDTranslationCollection)
            End Get
        End Property

        <ConfigurationProperty("mode", DefaultValue:=ConfigMode.RemoteOnly, IsRequired:=True)> _
        Public ReadOnly Property Mode() As ConfigMode
            Get
                Return System.Enum.Parse(GetType(ConfigMode), Me("mode"), False)
            End Get
        End Property

        <ConfigurationProperty("port", DefaultValue:=443, IsRequired:=False)> _
        Public ReadOnly Property Port() As Integer
            Get
                Return Integer.Parse(Me("port"))
            End Get
        End Property

        <ConfigurationProperty("hstsEnabled", DefaultValue:=False, IsRequired:=False)> _
        Public ReadOnly Property HSTSEnabled() As Boolean
            Get
                Return Boolean.Parse(Me("hstsEnabled"))
            End Get
        End Property

        <ConfigurationProperty("hstsMaxAge", DefaultValue:=31536000, IsRequired:=False)> _
        Public ReadOnly Property HSTSMaxAge() As Integer
            Get
                Return Integer.Parse(Me("hstsMaxAge"))
            End Get
        End Property

        <ConfigurationProperty("removeWWWPrefix", DefaultValue:=False, IsRequired:=False)> _
        Public ReadOnly Property RemoveWWWPrefix() As Boolean
            Get
                Return Boolean.Parse(Me("removeWWWPrefix"))
            End Get
        End Property

        Protected Overrides Function OnDeserializeUnrecognizedAttribute(ByVal name As String, ByVal value As String) As Boolean

            If name = "xmlns" Then Return True
            Return MyBase.OnDeserializeUnrecognizedAttribute(name, value)

        End Function

    End Class

    Public Class PathElement
        Inherits ConfigurationElement

        <ConfigurationProperty("path", IsRequired:=True)> _
         Public ReadOnly Property Path() As String
            Get
                Return TryCast(Me("path"), String)
            End Get
        End Property

    End Class

    <ConfigurationCollection(GetType(PathElement), AddItemName:="add", CollectionType:=ConfigurationElementCollectionType.AddRemoveClearMap)> _
    Public Class PathCollection
        Inherits ConfigurationElementCollection

        Default Public Overloads Property Item(ByVal index As Integer) As PathElement
            Get
                Return TryCast(MyBase.BaseGet(index), PathElement)
            End Get
            Set(ByVal value As PathElement)
                If MyBase.BaseGet(index) IsNot Nothing Then
                    MyBase.BaseRemoveAt(index)
                End If
                Me.BaseAdd(index, value)
            End Set
        End Property

        Protected Overrides Function CreateNewElement() As ConfigurationElement
            Return New PathElement()
        End Function

        Protected Overrides Function GetElementKey(ByVal element As ConfigurationElement) As Object
            Return DirectCast(element, PathElement).Path
        End Function

    End Class

    Public Class TLDTranslationElement
        Inherits ConfigurationElement

        <ConfigurationProperty("from", IsRequired:=True)> _
         Public ReadOnly Property From() As String
            Get
                Return TryCast(Me("from"), String)
            End Get
        End Property

        <ConfigurationProperty("to", IsRequired:=True)> _
        Public ReadOnly Property [To]() As String
            Get
                Return TryCast(Me("to"), String)
            End Get
        End Property

    End Class

    <ConfigurationCollection(GetType(TLDTranslationElement), AddItemName:="add", CollectionType:=ConfigurationElementCollectionType.AddRemoveClearMap)> _
    Public Class TLDTranslationCollection
        Inherits ConfigurationElementCollection

        Default Public Overloads Property Item(ByVal index As Integer) As TLDTranslationElement
            Get
                Return TryCast(MyBase.BaseGet(index), TLDTranslationElement)
            End Get
            Set(ByVal value As TLDTranslationElement)
                If MyBase.BaseGet(index) IsNot Nothing Then
                    MyBase.BaseRemoveAt(index)
                End If
                Me.BaseAdd(index, value)
            End Set
        End Property

        Protected Overrides Function CreateNewElement() As ConfigurationElement
            Return New TLDTranslationElement()
        End Function

        Protected Overrides Function GetElementKey(ByVal element As ConfigurationElement) As Object
            Return DirectCast(element, TLDTranslationElement).From
        End Function

    End Class

End Class