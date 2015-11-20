<p><strong>Project Description</strong><br />A simple HttpModule for ASP.NET that redirects all non-secure http requests to an https equivalent. Module is fully configurable from web.config.<br /><br /><strong>System Requirements</strong><br />ASP.NET 2.0 or higher<br /><br /><strong>Usage</strong><br />Place binary in /bin folder<br /><br />Add following to web.config <span class="codeInline">&lt;configSections&gt;</span>:</p>
<div style="color: black; background-color: white;">
<pre><span style="color: blue;">&lt;</span><span style="color: #a31515;">section</span> <span style="color: red;">name</span><span style="color: blue;">=</span><span style="color: black;">"</span><span style="color: blue;">httpsOnly</span><span style="color: black;">"</span> <span style="color: red;">type</span><span style="color: blue;">=</span><span style="color: black;">"</span><span style="color: blue;">HttpsOnly.HttpsOnlyModule+Configuration</span><span style="color: black;">"</span> <span style="color: blue;">/&gt;</span>
</pre>
</div>
<p><br />Add following to <span class="codeInline">&lt;httpModules&gt;</span></p>
<div style="color: black; background-color: white;">
<pre><span style="color: blue;">&lt;</span><span style="color: #a31515;">add</span> <span style="color: red;">name</span><span style="color: blue;">=</span><span style="color: black;">"</span><span style="color: blue;">HttpsOnlyModule</span><span style="color: black;">"</span> <span style="color: red;">type</span><span style="color: blue;">=</span><span style="color: black;">"</span><span style="color: blue;">HttpsOnly.HttpsOnlyModule, HttpsOnly</span><span style="color: black;">"</span> <span style="color: blue;">/&gt;</span>
</pre>
</div>
<p><br />Add your configuration:</p>
<div style="color: black; background-color: white;">
<pre><span style="color: blue;">&lt;</span><span style="color: #a31515;">httpsOnly</span> <span style="color: red;">mode</span><span style="color: blue;">=</span><span style="color: black;">"</span><span style="color: blue;">RemoteOnly</span><span style="color: black;">"</span> <span style="color: red;">hstsEnabled</span><span style="color: blue;">=</span><span style="color: black;">"</span><span style="color: blue;">true</span><span style="color: black;">"</span> <span style="color: red;">hstsMaxAge</span><span style="color: blue;">=</span><span style="color: black;">"</span><span style="color: blue;">31536000</span><span style="color: black;">"</span> <span style="color: red;">removeWWWPrefix</span><span style="color: blue;">=</span><span style="color: black;">"</span><span style="color: blue;">true</span><span style="color: black;">"</span><span style="color: blue;">&gt;</span>
	<span style="color: blue;">&lt;</span><span style="color: #a31515;">ignoredPaths</span><span style="color: blue;">&gt;</span>
		<span style="color: blue;">&lt;</span><span style="color: #a31515;">add</span> <span style="color: red;">path</span><span style="color: blue;">=</span><span style="color: black;">"</span><span style="color: blue;">~/webservices</span><span style="color: black;">"</span> <span style="color: blue;">/&gt;</span>
	<span style="color: blue;">&lt;/</span><span style="color: #a31515;">ignoredPaths</span><span style="color: blue;">&gt;</span>
	<span style="color: blue;">&lt;</span><span style="color: #a31515;">tldTranslation</span><span style="color: blue;">&gt;</span>
		<span style="color: blue;">&lt;</span><span style="color: #a31515;">add</span> <span style="color: red;">from</span><span style="color: blue;">=</span><span style="color: black;">"</span><span style="color: blue;">net</span><span style="color: black;">"</span> <span style="color: red;">to</span><span style="color: blue;">=</span><span style="color: black;">"</span><span style="color: blue;">com</span><span style="color: black;">"</span> <span style="color: blue;">/&gt;</span>
	<span style="color: blue;">&lt;/</span><span style="color: #a31515;">tldTranslation</span><span style="color: blue;">&gt;</span>
<span style="color: blue;">&lt;/</span><span style="color: #a31515;">httpsOnly</span><span style="color: blue;">&gt;</span>
</pre>
</div>
<p><br /><strong>Configuration</strong></p>
<ul>
<li><strong>mode</strong>: RemoteOnly, On, Off (Only required configuration setting)</li>
<li><strong>port</strong>: TCP port to check if request is secure (Default: 443)</li>
<li><strong>hstsEnabled</strong>: Enable/Disable <a href="http://en.wikipedia.org/wiki/HTTP_Strict_Transport_Security">HTTP Strict Transport Security</a> header (Default: False)</li>
<li><strong>hstsMaxAge</strong>: The time in seconds for the HSTS header (Default: 31536000)</li>
<li><strong>removeWWWPrefix</strong>: removes www. prefix in domain name if present (Default: False)</li>
<li><strong>&lt;ignoredPaths&gt;</strong>: List of virtual paths to ignore requests on (optional element)</li>
<li><strong>&lt;tldTranslation&gt;</strong>: Changes top level domain (tld) from one to another (optional element)</li>
</ul>
